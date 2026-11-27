using System.Net;
using Vylersant_Facturacion.Contracts.Invoices;
using Vylersant_Facturacion.Desktop.PointOfSale;

namespace Vylersant_Facturacion.Desktop.Tests.PointOfSale;

public sealed class InvoiceHistoryViewModelTests
{
    [Fact]
    public async Task LoadAsync_ShouldShowInvoiceHistoryAndFetchSelectedDetails()
    {
        var item = new InvoiceListItemResponse(Guid.NewGuid(), 22, DateTime.UtcNow, "Issued", null, 120m);
        var details = new InvoiceDetailsResponse(item.Id, item.Number, item.IssuedAtUtc, item.Status, null, null, item.Total, null, []);
        var api = new FakeInvoiceApi(new InvoicePageResponse([item], 1, 50, 1), details);
        var viewModel = new InvoiceHistoryViewModel(api);

        await viewModel.LoadAsync("access-token");
        await viewModel.LoadDetailsAsync(item, "access-token");

        Assert.Single(viewModel.Invoices);
        Assert.Equal(1, viewModel.TotalCount);
        Assert.Equal(item.Id, api.LastRequestedInvoiceId);
        Assert.Equal(22, viewModel.Details?.Number);
        Assert.True(viewModel.HasDetails);
    }

    [Fact]
    public async Task LoadAsync_ShouldExposeApiErrorsAndEmptyState()
    {
        var api = new FakeInvoiceApi(new InvoicePageResponse([], 1, 50, 0), null)
        {
            Failure = new InvoiceApiException(HttpStatusCode.ServiceUnavailable, "Facturas no disponibles.")
        };
        var viewModel = new InvoiceHistoryViewModel(api);

        await viewModel.LoadAsync("access-token");

        Assert.Equal("Facturas no disponibles.", viewModel.ErrorMessage);
        Assert.Empty(viewModel.Invoices);
        Assert.False(viewModel.IsLoading);
        Assert.False(viewModel.HasNoInvoices);
    }

    private sealed class FakeInvoiceApi(InvoicePageResponse page, InvoiceDetailsResponse? details) : IInvoiceApiClient
    {
        public Exception? Failure { get; init; }
        public Guid? LastRequestedInvoiceId { get; private set; }

        public Task<CreateInvoiceResponse> CreateAsync(CreateInvoiceRequest request, string accessToken, CancellationToken cancellationToken = default) =>
            Task.FromResult(new CreateInvoiceResponse(Guid.NewGuid(), 1, 0m));

        public Task<InvoicePageResponse> GetPageAsync(int pageNumber, int pageSize, string accessToken, CancellationToken cancellationToken = default) =>
            Failure is null ? Task.FromResult(page) : Task.FromException<InvoicePageResponse>(Failure);

        public Task<InvoiceDetailsResponse> GetDetailsAsync(Guid invoiceId, string accessToken, CancellationToken cancellationToken = default)
        {
            LastRequestedInvoiceId = invoiceId;
            return details is null
                ? Task.FromException<InvoiceDetailsResponse>(new InvalidOperationException("No se encontró detalle."))
                : Task.FromResult(details);
        }
    }
}

