using System;
using System.Collections.Generic;
using System.Text;

namespace Vylersant_Facturacion.Contracts.Authentication
{
    public sealed record LoginRequest(
        string Email,
        string Password
    );
}
