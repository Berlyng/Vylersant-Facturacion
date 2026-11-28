using Vylersant_Facturacion.Application.Printing;
using System.IO;

namespace Vylersant_Facturacion.Desktop.Printing;

public sealed class EscPosReceiptPrinter(IRawPrinterTransport transport, TimeProvider timeProvider)
{
    public async Task<ReceiptPrintResult> PrintAsync(
        ReceiptPrintDocument document,
        ReceiptPrinterSettings settings,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        try
        {
            settings.Validate();
            var bytes = EscPosReceiptFormatter.Format(document, settings);
            var jobId = await Task.Run(() => transport.SendAsync(settings.PrinterName, bytes, cancellationToken), cancellationToken);
            return ReceiptPrintResult.Success(timeProvider.GetUtcNow().UtcDateTime, jobId);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException or System.ComponentModel.Win32Exception or IOException)
        {
            return ReceiptPrintResult.Failure(timeProvider.GetUtcNow().UtcDateTime, exception.Message);
        }
    }
}

