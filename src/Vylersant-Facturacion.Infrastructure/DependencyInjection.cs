using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vylersant_Facturacion.Application.Abstraccion;
using Vylersant_Facturacion.Application.Authentication;
using Vylersant_Facturacion.Application.Bussinesses;
using Vylersant_Facturacion.Application.Products;
using Vylersant_Facturacion.Application.Security;
using Vylersant_Facturacion.Application.Users;
using Vylersant_Facturacion.Infrastructure.Persistence;
using Vylersant_Facturacion.Infrastructure.Repositories;
using Vylersant_Facturacion.Infrastructure.Repository;
using Vylersant_Facturacion.Infrastructure.Security;

namespace Vylersant_Facturacion.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
     this IServiceCollection services,
     IConfiguration configuration)
        {
            var connectionString =
                configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "No se encontró la cadena de conexión 'DefaultConnection'.");

            services.AddDbContext<VylersantFacturacionDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddScoped<IUserRepository, UserRepository>();

            services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();

            services.AddScoped<IBussinesRepository, BussinesRepository>();

            services.AddScoped<IUnitOfWork>(
                serviceProvider =>
                    serviceProvider.GetRequiredService<VylersantFacturacionDbContext>());

            services.Configure<JwtSettings>(
                configuration.GetSection(JwtSettings.SectionName));

            services.AddSingleton<ITokenService, JwtTokenService>();
            services.AddSingleton<IRefreshTokenService, RefreshTokenService>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

            services.Configure<RefreshTokenOptions>(
                configuration.GetSection("RefreshToken"));

            services.AddScoped<IProductRepository, ProductRepository>();

            return services;
        }
    }
}
