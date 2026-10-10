using System;
using System.Collections.Generic;
using System.Text;

namespace Vylersant_Facturacion.Contracts.Authentication
{
    public sealed record CurrentUserResponse(
        Guid UserId,
        Guid BusinessId,
        string Name,
        string Email,
        string Role
    );
}
