using System;
using System.Collections.Generic;
using System.Text;

namespace Vylersant_Facturacion.Application.Registration
{
    public sealed class EmailAlreadyExistsException : Exception
    {
        public EmailAlreadyExistsException(string email) : base($"Ya existe un usuario con el email '{email}'")
        {
        }
    }
}
