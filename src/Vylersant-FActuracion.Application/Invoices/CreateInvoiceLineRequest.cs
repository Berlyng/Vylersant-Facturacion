using System;
using System.Collections.Generic;
using System.Text;

namespace Vylersant_Facturacion.Application.Invoices
{
    public sealed record CreateInvoiceLineRequest(Guid ProductId, decimal Quantity);
  
}

