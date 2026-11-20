using System;
using System.Collections.Generic;
using System.Text;

namespace Vylersant_Facturacion.Application.Invoices
{
    public class InvoiceNotFoundException : Exception
    {
        public InvoiceNotFoundException(Guid invoiceId)
            : base($"la factura con el  ID '{invoiceId}' no fue encontrada")
        {
        }

    }
}

