using System;
using System.Collections.Generic;
using System.Text;

namespace Vylersant_Facturacion.Application.Authentication
{
    public sealed class InvalidCredentialsException : Exception
    {
        public InvalidCredentialsException() : base("El correo electrónico o la contraseña son incorrectos.")
        {
        }
    }
}
