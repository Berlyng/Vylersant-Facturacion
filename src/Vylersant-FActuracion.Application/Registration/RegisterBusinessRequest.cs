using System;
using System.Collections.Generic;
using System.Text;

namespace Vylersant_Facturacion.Application.Registration
{
    public sealed record RegisterBusinessRequest(string BussinesName,
        string OwnerName, 
        string Email, 
        string Password);

}
