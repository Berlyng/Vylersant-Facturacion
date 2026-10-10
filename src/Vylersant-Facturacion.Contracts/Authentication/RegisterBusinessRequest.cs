using System;
using System.Collections.Generic;
using System.Text;

namespace Vylersant_Facturacion.Contracts.Authentication
{
    public sealed record RegisterBusinessRequest(
        string BusinessName,
        string OwnerName,
        string Email,
        string Password);
  
}
