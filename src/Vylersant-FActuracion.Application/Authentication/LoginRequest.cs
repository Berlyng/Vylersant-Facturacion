using System;
using System.Collections.Generic;
using System.Text;

namespace Vylersant_Facturacion.Application.Authentication
{
    public sealed record LoginRequest(
        string Email,
        string Password
    );

}
