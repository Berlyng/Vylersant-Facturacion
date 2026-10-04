using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vylersant_Facturacion.Infrastructure.Persistence;

namespace Vylersant_Facturacion.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Aquí puedes agregar la configuración de tus servicios de infraestructura
            // Por ejemplo, si estás usando Entity Framework Core:
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'DefaultConnection'.");
            services.AddDbContext<VylersantFacturacionDbContext>(options =>
                options.UseSqlServer(connectionString));
            // Agrega otros servicios de infraestructura según sea necesario
            return services;
        }
    }
}
