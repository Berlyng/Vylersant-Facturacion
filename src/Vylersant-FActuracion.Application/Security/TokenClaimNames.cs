using System;
using System.Collections.Generic;
using System.Text;

namespace Vylersant_Facturacion.Application.Security
{
    public static class TokenClaimNames
    {
        public const string UserId = "sub";
        public const string BusinessId = "business_id";
        public const string Email = "email";
        public const string Name = "name";
        public const string Role = "role";
    }
}
