using System;
using System.Collections.Generic;
using System.Text;

namespace Vylersant_Facturacion.Application.Invoices
{
    public sealed record CreateInvoiceResult(Guid InvoiceId, long Number, decimal Total);
    
}

