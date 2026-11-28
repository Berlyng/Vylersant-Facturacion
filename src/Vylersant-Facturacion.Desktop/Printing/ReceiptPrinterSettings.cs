namespace Vylersant_Facturacion.Desktop.Printing;

public sealed record ReceiptPrinterSettings(string BusinessName, string PrinterName, int CharacterWidth = 42)
{
    public ReceiptPrinterSettings Validate()
    {
        if (string.IsNullOrWhiteSpace(BusinessName))
            throw new ArgumentException("Escribe el nombre del negocio que aparecerá en el recibo.", nameof(BusinessName));
        if (string.IsNullOrWhiteSpace(PrinterName))
            throw new ArgumentException("Selecciona una impresora.", nameof(PrinterName));
        if (CharacterWidth is not (32 or 42 or 48))
            throw new ArgumentOutOfRangeException(nameof(CharacterWidth), "El ancho debe ser 32, 42 o 48 caracteres.");
        return this with { BusinessName = BusinessName.Trim(), PrinterName = PrinterName.Trim() };
    }
}

