using System;
using System.Collections.Generic;
using System.Text;

namespace Vylersant_Facturacion.Application.Security
{
    public interface IPasswordHasher
    {
        string Hash(string password);
        bool Verify(string password, string hashedPassword);
    }
}
