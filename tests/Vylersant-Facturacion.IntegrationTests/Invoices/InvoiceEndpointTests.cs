using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Vylersant_Facturacion.Application.Abstraccion;
using Vylersant_Facturacion.Application.Customers;
using Vylersant_Facturacion.Application.Invoices;
using Vylersant_Facturacion.Application.Payments;
using Vylersant_Facturacion.Application.Security;
using Vylersant_Facturacion.Contracts.Invoices;
using Vylersant_Facturacion.Domain.Entities.Customers;
using Vylersant_Facturacion.Domain.Entities.Invoices;
using Vylersant_Facturacion.Domain.Entities.Users;
using Vylersant_Facturacion.Domain.Entities.Payments;
using PaymentRequest = Vylersant_Facturacion.Contracts.Payments.RecordPaymentRequest;
using VoidPaymentContractRequest = Vylersant_Facturacion.Contracts.Payments.VoidPaymentRequest;
using Vylersant_Facturacion.Contracts.Payments;

namespace Vylersant_Facturacion.IntegrationTests.Invoices;

public sealed class InvoiceEndpointTests : IClassFixture<Vylersant_FacturacionApiFactory>
{
    private readonly Vylersant_FacturacionApiFactory _factory;
    public InvoiceEndpointTests(Vylersant_FacturacionApiFactory factory) => _factory = factory;

    [Fact]
    public async Task GetAll_ShouldReturnOnlyInvoicesFromAuthenticatedBusiness()
    {
        var businessA = Guid.NewGuid();
        var invoiceA = CreateInvoice(businessA, 1);
        var invoiceB = CreateInvoice(Guid.NewGuid(), 2);
        using var factory = CreateFactory([invoiceA, invoiceB], []);
        using var client = CreateAuthenticatedClient(factory, CreateUser(businessA));

        var response = await client.GetAsync("/api/invoices?page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<InvoicePageResponse>();
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal(invoiceA.Id, result.Items[0].Id);
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenInvoiceBelongsToAnotherBusiness()
    {
        var invoice = CreateInvoice(Guid.NewGuid(), 1);
        using var factory = CreateFactory([invoice], []);
        using var client = CreateAuthenticatedClient(factory, CreateUser(Guid.NewGuid()));

        var response = await client.GetAsync($"/api/invoices/{invoice.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal("Factura no encontrada", problem!.Title);
    }

    [Fact]
    public async Task GetById_ShouldReturnHistoricalDetails_WhenInvoiceBelongsToBusiness()
    {
        var businessId = Guid.NewGuid();
        var customer = new Customer(businessId, "Cliente A");
        var invoice = new Invoice(businessId, Guid.NewGuid(), customer.Id, 1, DateTime.UtcNow,
            [new InvoiceLine(Guid.NewGuid(), "Producto histórico", 2m, 5.25m)]);
        using var factory = CreateFactory([invoice], [customer]);
        using var client = CreateAuthenticatedClient(factory, CreateUser(businessId));

        var response = await client.GetAsync($"/api/invoices/{invoice.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<InvoiceDetailsResponse>();
        Assert.NotNull(result);
        Assert.Equal("Cliente A", result.CustomerName);
        Assert.Equal("Producto histórico", Assert.Single(result.Lines).ProductName);
        Assert.Equal(10.50m, result.Total);
    }

    [Fact]
    public async Task Cancel_ShouldBeIdempotentAndPreserveInvoiceNumber()
    {
        var businessId = Guid.NewGuid();
        var invoice = CreateInvoice(businessId, 23);
        using var factory = CreateFactory([invoice], []);
        using var client = CreateAuthenticatedClient(factory, CreateUser(businessId));
        var route = $"/api/invoices/{invoice.Id}/cancel";

        var first = await client.PatchAsync(route, content: null);
        var cancelledAt = invoice.CancelledAtUtc;
        var second = await client.PatchAsync(route, content: null);

        Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, second.StatusCode);
        Assert.Equal(InvoiceStatus.Cancelled, invoice.Status);
        Assert.Equal(23, invoice.Number);
        Assert.NotNull(cancelledAt);
        Assert.Equal(cancelledAt, invoice.CancelledAtUtc);
    }

    [Fact]
    public async Task Cancel_ShouldReturnNotFound_WhenInvoiceBelongsToAnotherBusiness()
    {
        var invoice = CreateInvoice(Guid.NewGuid(), 24);
        using var factory = CreateFactory([invoice], []);
        using var client = CreateAuthenticatedClient(factory, CreateUser(Guid.NewGuid()));

        var response = await client.PatchAsync($"/api/invoices/{invoice.Id}/cancel", content: null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(InvoiceStatus.Issued, invoice.Status);
    }

    [Fact]
    public async Task RecordPayment_ShouldReturnDerivedBalanceAndPaidState()
    {
        var businessId = Guid.NewGuid();
        var invoice = CreateInvoice(businessId, 1);
        var payments = new FakePaymentRepository();
        using var factory = CreateFactory([invoice], [], payments);
        using var client = CreateAuthenticatedClient(factory, CreateUser(businessId));

        var response = await client.PostAsJsonAsync($"/api/invoices/{invoice.Id}/payments", new PaymentRequest(10m, "Cash"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PaymentResponse>();
        Assert.NotNull(result);
        Assert.Equal(10m, result.PaidTotal);
        Assert.Equal(0m, result.Balance);
        Assert.True(result.IsPaid);
    }

    [Fact]
    public async Task RecordPayment_ShouldNotRevealInvoiceFromAnotherBusiness()
    {
        var invoice = CreateInvoice(Guid.NewGuid(), 1);
        using var factory = CreateFactory([invoice], [], new FakePaymentRepository());
        using var client = CreateAuthenticatedClient(factory, CreateUser(Guid.NewGuid()));

        var response = await client.PostAsJsonAsync($"/api/invoices/{invoice.Id}/payments", new PaymentRequest(1m, "Cash"));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PaymentHistory_ShouldKeepVoidedPaymentAndRecalculateBalance()
    {
        var businessId = Guid.NewGuid();
        var invoice = CreateInvoice(businessId, 1);
        var payments = new FakePaymentRepository();
        using var factory = CreateFactory([invoice], [], payments);
        using var client = CreateAuthenticatedClient(factory, CreateUser(businessId));

        var created = await client.PostAsJsonAsync($"/api/invoices/{invoice.Id}/payments", new PaymentRequest(4m, "Cash"));
        var payment = await created.Content.ReadFromJsonAsync<PaymentResponse>();
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.NotNull(payment);

        var voidRoute = $"/api/invoices/{invoice.Id}/payments/{payment.Id}/void";
        var firstVoid = await client.PatchAsJsonAsync(voidRoute, new VoidPaymentContractRequest("Pago duplicado"));
        var repeatedVoid = await client.PatchAsJsonAsync(voidRoute, new VoidPaymentContractRequest("Motivo posterior"));
        var historyResponse = await client.GetAsync($"/api/invoices/{invoice.Id}/payments");
        var history = await historyResponse.Content.ReadFromJsonAsync<InvoicePaymentsResponse>();

        Assert.Equal(HttpStatusCode.NoContent, firstVoid.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, repeatedVoid.StatusCode);
        Assert.Equal(HttpStatusCode.OK, historyResponse.StatusCode);
        Assert.NotNull(history);
        Assert.Equal(0m, history.PaidTotal);
        Assert.Equal(invoice.Total, history.Balance);
        var item = Assert.Single(history.Payments);
        Assert.Equal("Voided", item.Status);
        Assert.Equal("Pago duplicado", item.VoidReason);
        Assert.NotNull(item.VoidedAtUtc);
    }

    private WebApplicationFactory<Program> CreateFactory(Invoice[] invoices, Customer[] customers, FakePaymentRepository? payments = null) =>
        _factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IInvoiceRepository>();
            services.RemoveAll<ICustomerRepository>();
            services.RemoveAll<IPaymentRepository>();
            services.RemoveAll<IUnitOfWork>();
            services.AddSingleton<IInvoiceRepository>(new FakeInvoiceRepository(invoices));
            services.AddSingleton<ICustomerRepository>(new FakeCustomerRepository(customers));
            services.AddSingleton<IPaymentRepository>(payments ?? new FakePaymentRepository());
            services.AddSingleton<IUnitOfWork>(new FakeUnitOfWork());
        }));

    private static User CreateUser(Guid businessId) =>
        new(businessId, "Integration User", $"{Guid.NewGuid()}@test.com", "hashed-password", UserRole.Owner);

    private static HttpClient CreateAuthenticatedClient(WebApplicationFactory<Program> factory, User user)
    {
        var client = factory.CreateClient();
        using var scope = factory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenService.Generate(user).Token);
        return client;
    }

    private static Invoice CreateInvoice(Guid businessId, long number) =>
        new(businessId, Guid.NewGuid(), null, number, DateTime.UtcNow,
            [new InvoiceLine(Guid.NewGuid(), $"Producto {number}", 1m, number * 10m)]);

    private sealed class FakeInvoiceRepository(Invoice[] invoices) : IInvoiceRepository
    {
        public Task<Invoice?> GetByIdAsync(Guid id, Guid businessId, CancellationToken token = default) =>
            Task.FromResult(invoices.FirstOrDefault(invoice => invoice.Id == id && invoice.BusinessId == businessId));
        public Task<IReadOnlyList<Invoice>> GetAllAsync(Guid businessId, CancellationToken token = default) =>
            Task.FromResult<IReadOnlyList<Invoice>>(invoices.Where(invoice => invoice.BusinessId == businessId).ToList());
        public Task<IReadOnlyList<Invoice>> GetPageAsync(Guid businessId, int skip, int take, CancellationToken token = default) =>
            Task.FromResult<IReadOnlyList<Invoice>>(invoices.Where(invoice => invoice.BusinessId == businessId)
                .OrderByDescending(invoice => invoice.IssuedAtUtc).ThenByDescending(invoice => invoice.Number).Skip(skip).Take(take).ToList());
        public Task<long> GetCountAsync(Guid businessId, CancellationToken token = default) =>
            Task.FromResult((long)invoices.Count(invoice => invoice.BusinessId == businessId));
        public Task<long> GetNextNumberAsync(Guid businessId, CancellationToken token = default) => Task.FromResult(1L);
        public Task AddAsync(Invoice invoice, CancellationToken token = default) => Task.CompletedTask;
    }

    private sealed class FakeCustomerRepository(Customer[] customers) : ICustomerRepository
    {
        public Task<Customer?> GetByIdAsync(Guid id, Guid businessId, CancellationToken token = default) =>
            Task.FromResult(customers.FirstOrDefault(customer => customer.Id == id && customer.BusinessId == businessId));
        public Task<IReadOnlyList<Customer>> GetAllAsync(Guid businessId, CancellationToken token = default) =>
            Task.FromResult<IReadOnlyList<Customer>>(customers.Where(customer => customer.BusinessId == businessId).ToList());
        public Task AddAsync(Customer customer, CancellationToken token = default) => Task.CompletedTask;
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);
    }

    private sealed class FakePaymentRepository : IPaymentRepository
    {
        private readonly List<Payment> _payments = [];
        public Task AddAsync(Payment payment, CancellationToken cancellationToken = default)
        {
            _payments.Add(payment);
            return Task.CompletedTask;
        }
        public Task<decimal> GetCompletedTotalAsync(Guid invoiceId, Guid businessId, CancellationToken cancellationToken = default) =>
            Task.FromResult(_payments.Where(payment => payment.InvoiceId == invoiceId && payment.BusinessId == businessId && payment.Status == PaymentStatus.Completed).Sum(payment => payment.Amount));
        public Task<Payment?> GetByIdAsync(Guid paymentId, Guid businessId, CancellationToken cancellationToken = default) =>
            Task.FromResult(_payments.FirstOrDefault(payment => payment.Id == paymentId && payment.BusinessId == businessId));
        public Task<IReadOnlyList<Payment>> GetForInvoiceAsync(Guid invoiceId, Guid businessId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Payment>>(_payments.Where(payment => payment.InvoiceId == invoiceId && payment.BusinessId == businessId).ToList());
    }
}

