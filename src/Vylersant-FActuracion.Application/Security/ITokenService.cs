using System;
using System.Collections.Generic;
using System.Text;
using Vylersant_Facturacion.Domain.Entities.Users;

namespace Vylersant_Facturacion.Application.Security
{
    public interface ITokenService
    {
        AccessToken Generate(User user);
    }
}
