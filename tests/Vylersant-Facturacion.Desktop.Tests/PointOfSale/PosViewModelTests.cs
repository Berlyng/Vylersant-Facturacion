using System.Net;
using Vylersant_Facturacion.Contracts.Customers;
using Vylersant_Facturacion.Contracts.Products;
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
        var viewModel = new PosViewModel(new FakeCatalog([coffee, bread, inactive], []));

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
        var viewModel = new PosViewModel(new FakeCatalog([coffee], []));
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
        var viewModel = new PosViewModel(new FakeCatalog([], [customer]) { Failure = new CatalogApiException(HttpStatusCode.ServiceUnavailable, "Catálogo no disponible.") });

        await viewModel.LoadAsync("access-token");

        Assert.Null(viewModel.SelectedCustomer);
        Assert.Equal("Catálogo no disponible.", viewModel.ErrorMessage);
        Assert.Empty(viewModel.Products);
        Assert.False(viewModel.IsLoading);
    }

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
}

