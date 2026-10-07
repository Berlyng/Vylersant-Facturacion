using System;
using System.Collections.Generic;
using System.Text;

namespace Vylersant_Facturacion.Infrastructure.Security
{
    public sealed class JwtSettings
    {
        public const string SectionName = "Jwt";
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public int ExpirationMinutes
        {
            get; init;
        }
    }
}
