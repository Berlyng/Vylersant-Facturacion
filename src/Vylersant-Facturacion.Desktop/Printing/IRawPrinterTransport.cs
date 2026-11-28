namespace Vylersant_Facturacion.Desktop.Printing;

public interface IRawPrinterTransport
{
    Task<string> SendAsync(string printerName, byte[] data, CancellationToken cancellationToken = default);
}

