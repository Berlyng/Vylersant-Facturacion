using System;
using System.Collections.Generic;
using System.Text;

namespace Vylersant_Facturacion.Application.Authentication
{
    public sealed class InactiveUserException : Exception
    {
        public InactiveUserException() : base("El usuario está inactivo.")
        {
        }
    }
}
