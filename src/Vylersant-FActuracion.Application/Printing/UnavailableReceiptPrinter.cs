namespace Vylersant_Facturacion.Application.Printing;

/// <summary>Safe default for hosts that have not configured a printer implementation.</summary>
public sealed class UnavailableReceiptPrinter(TimeProvider timeProvider) : IReceiptPrinter
{
    public Task<ReceiptPrintResult> PrintAsync(ReceiptPrintDocument document, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(ReceiptPrintResult.Failure(
            timeProvider.GetUtcNow().UtcDateTime,
            "No hay una impresora configurada para este equipo."));
    }
}

