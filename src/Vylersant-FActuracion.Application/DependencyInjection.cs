using Microsoft.Extensions.DependencyInjection;
using Vylersant_Facturacion.Application.Authentication;
using Vylersant_Facturacion.Application.Products;
using Vylersant_Facturacion.Application.Registration;

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
            return services;
        }
    }
}
