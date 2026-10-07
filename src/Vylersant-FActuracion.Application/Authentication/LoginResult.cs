using System;
using System.Collections.Generic;
using System.Text;
using Vylersant_Facturacion.Domain.Entities.Users;

namespace Vylersant_Facturacion.Application.Authentication
{
    public sealed record LoginResult(
        Guid UserId,
        Guid BusinessId,
        string Name,
        string Email,
        UserRole Role,
        string AccessToken,
        DateTime ExpiresAtUtc);
  
}
