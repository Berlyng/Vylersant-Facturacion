namespace Vylersant_Facturacion.Application.Printing;

/// <summary>Port for sending a prepared invoice receipt to a printer implementation.</summary>
public interface IReceiptPrinter
{
    Task<ReceiptPrintResult> PrintAsync(ReceiptPrintDocument document, CancellationToken cancellationToken = default);
}

