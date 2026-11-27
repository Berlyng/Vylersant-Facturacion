using System.Net;
using Vylersant_Facturacion.Contracts.Customers;
using Vylersant_Facturacion.Contracts.Products;
using Vylersant_Facturacion.Contracts.Invoices;
using Vylersant_Facturacion.Desktop.PointOfSale;

namespace Vylersant_Facturacion.Desktop.Tests.PointOfSale;

public sealed class PosViewModelTests
{
    [Fact]
    public async Task LoadAsync_ShouldFilterInactiveProductsAndSearchByNameOrSku()
    {
        var coffee = Product("Café", "CAF-1", 85m);
        var bread = Product("Pan", "PAN-1", 45m);
        var inactive = Product("Oculto", "X-1", 3m, isActive: false);
        var viewModel = CreateViewModel(new FakeCatalog([coffee, bread, inactive], []));

        await viewModel.LoadAsync("access-token");
        viewModel.SearchText = "pan-1";

        Assert.Single(viewModel.Products);
        Assert.Equal(bread.Id, viewModel.Products[0].Id);
        Assert.DoesNotContain(viewModel.Products, product => product.Id == inactive.Id);
    }

    [Fact]
    public async Task Cart_ShouldUpdateQuantityRemoveLineAndCalculateSubtotalLocally()
    {
        var coffee = Product("Café", "CAF-1", 85.25m);
        var viewModel = CreateViewModel(new FakeCatalog([coffee], []));
        await viewModel.LoadAsync("access-token");

        viewModel.AddProduct(coffee);
        viewModel.AddProduct(coffee);
        var line = Assert.Single(viewModel.Cart);
        Assert.Equal(2m, line.Quantity);
        Assert.Equal(170.50m, viewModel.Subtotal);

        viewModel.ChangeQuantity(line, -1m);
        Assert.Equal(85.25m, viewModel.Subtotal);
        viewModel.ChangeQuantity(line, -1m);
        Assert.Empty(viewModel.Cart);
        Assert.Equal(0m, viewModel.Subtotal);
    }

    [Fact]
    public async Task LoadAsync_ShouldKeepOptionalCustomerAndExposeApiErrors()
    {
        var customer = new CustomerResponse(Guid.NewGuid(), "Cliente", null, null, null, true);
        var viewModel = CreateViewModel(new FakeCatalog([], [customer]) { Failure = new CatalogApiException(HttpStatusCode.ServiceUnavailable, "Catálogo no disponible.") });

        await viewModel.LoadAsync("access-token");

        Assert.Null(viewModel.SelectedCustomer);
        Assert.Equal("Catálogo no disponible.", viewModel.ErrorMessage);
        Assert.Empty(viewModel.Products);
        Assert.False(viewModel.IsLoading);
    }

    [Fact]
    public async Task ConfirmSale_ShouldSendOnlyProductIdsAndQuantitiesAndUseServerTotals()
    {
        var coffee = Product("Café", "CAF-1", 85m);
        var invoiceApi = new FakeInvoiceApi();
        var viewModel = new PosViewModel(new FakeCatalog([coffee], []), invoiceApi);
        await viewModel.LoadAsync("access-token");
        viewModel.AddProduct(coffee);
        viewModel.AddProduct(coffee);

        await viewModel.ConfirmSaleAsync("access-token");

        Assert.NotNull(invoiceApi.LastRequest);
        Assert.Null(invoiceApi.LastRequest.CustomerId);
        Assert.Equal(new CreateInvoiceLineRequest(coffee.Id, 2m), Assert.Single(invoiceApi.LastRequest.Lines));
        Assert.Empty(viewModel.Cart);
        Assert.Equal(4381, viewModel.LastCreatedInvoice?.Number);
        Assert.Equal(172m, viewModel.LastCreatedInvoice?.Total);
    }

    [Fact]
    public async Task ConfirmSale_ShouldPreventDuplicateSubmissionAndKeepCartWhilePending()
    {
        var coffee = Product("Café", "CAF-1", 85m);
        var invoiceApi = new FakeInvoiceApi { PendingResponse = new TaskCompletionSource<CreateInvoiceResponse>(TaskCreationOptions.RunContinuationsAsynchronously) };
        var viewModel = new PosViewModel(new FakeCatalog([coffee], []), invoiceApi);
        await viewModel.LoadAsync("access-token");
        viewModel.AddProduct(coffee);

        var first = viewModel.ConfirmSaleAsync("access-token");
        await viewModel.ConfirmSaleAsync("access-token");

        Assert.True(viewModel.IsSubmitting);
        Assert.Single(viewModel.Cart);
        Assert.Equal(1, invoiceApi.CreateCalls);
        invoiceApi.PendingResponse.SetResult(new CreateInvoiceResponse(Guid.NewGuid(), 11, 85m));
        await first;

        Assert.Empty(viewModel.Cart);
        Assert.False(viewModel.IsSubmitting);
    }

    [Fact]
    public async Task ConfirmSale_ShouldKeepCartAndShowApiErrorWhenServerRejectsRequest()
    {
        var coffee = Product("Café", "CAF-1", 85m);
        var invoiceApi = new FakeInvoiceApi { CreateFailure = new InvoiceApiException(HttpStatusCode.BadRequest, "El producto ya no está activo.") };
        var viewModel = new PosViewModel(new FakeCatalog([coffee], []), invoiceApi);
        await viewModel.LoadAsync("access-token");
        viewModel.AddProduct(coffee);

        await viewModel.ConfirmSaleAsync("access-token");

        Assert.Single(viewModel.Cart);
        Assert.Equal("El producto ya no está activo.", viewModel.SaleErrorMessage);
    }

    private static PosViewModel CreateViewModel(ICatalogApiClient catalog) => new(catalog, new FakeInvoiceApi());

    private static ProductResponse Product(string name, string sku, decimal price, bool isActive = true) =>
        new(Guid.NewGuid(), name, price, sku, null, isActive);

    private sealed class FakeCatalog(ProductResponse[] products, CustomerResponse[] customers) : ICatalogApiClient
    {
        public Exception? Failure { get; init; }
        public Task<IReadOnlyList<ProductResponse>> GetProductsAsync(string accessToken, CancellationToken cancellationToken = default) =>
            Failure is null ? Task.FromResult<IReadOnlyList<ProductResponse>>(products) : Task.FromException<IReadOnlyList<ProductResponse>>(Failure);
        public Task<IReadOnlyList<CustomerResponse>> GetCustomersAsync(string accessToken, CancellationToken cancellationToken = default) =>
            Failure is null ? Task.FromResult<IReadOnlyList<CustomerResponse>>(customers) : Task.FromException<IReadOnlyList<CustomerResponse>>(Failure);
    }

    private sealed class FakeInvoiceApi : IInvoiceApiClient
    {
        public CreateInvoiceRequest? LastRequest { get; private set; }
        public int CreateCalls { get; private set; }
        public Exception? CreateFailure { get; init; }
        public TaskCompletionSource<CreateInvoiceResponse>? PendingResponse { get; init; }

        public Task<CreateInvoiceResponse> CreateAsync(CreateInvoiceRequest request, string accessToken, CancellationToken cancellationToken = default)
        {
            LastRequest = request;
            CreateCalls++;
            if (CreateFailure is not null)
                return Task.FromException<CreateInvoiceResponse>(CreateFailure);
            if (PendingResponse is not null)
                return PendingResponse.Task;
            return Task.FromResult(new CreateInvoiceResponse(Guid.NewGuid(), 4381, 172m));
        }

        public Task<InvoicePageResponse> GetPageAsync(int page, int pageSize, string accessToken, CancellationToken cancellationToken = default) =>
            Task.FromResult(new InvoicePageResponse([], page, pageSize, 0));

        public Task<InvoiceDetailsResponse> GetDetailsAsync(Guid invoiceId, string accessToken, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }
}

