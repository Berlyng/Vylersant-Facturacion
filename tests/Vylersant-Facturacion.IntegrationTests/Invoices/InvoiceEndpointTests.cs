using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Vylersant_Facturacion.Application.Customers;
using Vylersant_Facturacion.Application.Invoices;
using Vylersant_Facturacion.Application.Security;
using Vylersant_Facturacion.Contracts.Invoices;
using Vylersant_Facturacion.Domain.Entities.Customers;
using Vylersant_Facturacion.Domain.Entities.Invoices;
using Vylersant_Facturacion.Domain.Entities.Users;

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

    private WebApplicationFactory<Program> CreateFactory(Invoice[] invoices, Customer[] customers) =>
        _factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IInvoiceRepository>();
            services.RemoveAll<ICustomerRepository>();
            services.AddSingleton<IInvoiceRepository>(new FakeInvoiceRepository(invoices));
            services.AddSingleton<ICustomerRepository>(new FakeCustomerRepository(customers));
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
}

