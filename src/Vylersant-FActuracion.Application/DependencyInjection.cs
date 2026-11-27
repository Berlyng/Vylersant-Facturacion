using Microsoft.Extensions.DependencyInjection;
using Vylersant_Facturacion.Application.Authentication;
using Vylersant_Facturacion.Application.Customers;
using Vylersant_Facturacion.Application.Invoices;
using Vylersant_Facturacion.Application.Products;
using Vylersant_Facturacion.Application.Payments;
using Vylersant_Facturacion.Application.Registration;
using Vylersant_Facturacion.Application.Printing;

namespace Vylersant_Facturacion.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<RegisterBussinesService>();
            services.AddScoped<LoginService>();
            services.AddScoped<RefreshSessionService>();
            services.AddScoped<CreateProductService>();
            services.AddScoped<GetProductService>();
            services.AddScoped<GetProductByIdService>();
            services.AddScoped<UpdateProductService>();
            services.AddScoped<ChangeProductStatusService>();
            services.AddScoped<CreateCustomerService>();
            services.AddScoped<GetCustomersService>();
            services.AddScoped<GetCustomerByIdService>();
            services.AddScoped<UpdateCustomerService>();
            services.AddScoped<ChangeCustomerStatusService>();
            services.AddScoped<CreateInvoiceService>();
            services.AddScoped<GetInvoicesService>();
            services.AddScoped<GetInvoiceByIdService>();
            services.AddScoped<CancelInvoiceService>();
            services.AddScoped<RecordPaymentService>();
            services.AddScoped<VoidPaymentService>();
            services.AddScoped<GetInvoicePaymentsService>();
            services.AddScoped<PrintInvoiceReceiptService>();
            services.AddScoped<IReceiptPrinter, UnavailableReceiptPrinter>();
            services.AddSingleton(TimeProvider.System);
            return services;
        }
    }
}

