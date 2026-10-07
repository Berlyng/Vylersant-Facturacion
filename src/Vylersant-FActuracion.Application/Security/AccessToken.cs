using System;
using System.Collections.Generic;
using System.Text;

namespace Vylersant_Facturacion.Application.Security
{
    public sealed record AccessToken(string Token, DateTime ExpiresAtUtc);
    
}
