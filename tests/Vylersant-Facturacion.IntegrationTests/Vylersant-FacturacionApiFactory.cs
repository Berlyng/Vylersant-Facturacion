using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Vylersant_Facturacion.IntegrationTests
{
    public sealed class Vylersant_FacturacionApiFactory : WebApplicationFactory<Program>
    {
        public const string JwtIssuer= "Vylersant_Facturacion.Api.Tests";
        public const string JwtAudience = "Vylersant_Facturacion.Desktop.Tests";
        public const string Key = "Vylersant_Facturacion-IntegrationTests-Key-2026-123456789";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                var settings = new Dictionary<string, string?>
                {
                    ["Jwt:Issuer"] = JwtIssuer,
                    ["Jwt:Audience"] = JwtAudience,
                    ["Jwt:Key"] = Key,
                    ["Jwt:SecretKey"] = Key,
                    ["Jwt:ExpirationMinutes"] = "30",

                    ["ConnectionStrings:DefaultConnection"] = "Server=(localdb)\\MSSQLLocalDB;"
                    + "Database=VylersantFacturacionDbTests;" +
                    "User Id=sa; " +
                    "Password=VylersantFacturacion2026; " +
                    "TrustServerCertificate=True"
                };

                configuration.AddInMemoryCollection(settings);
            });
        }
    }
}
