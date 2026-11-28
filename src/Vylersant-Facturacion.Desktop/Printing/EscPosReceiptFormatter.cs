using System.Globalization;
using System.IO;
using System.Text;
using Vylersant_Facturacion.Application.Printing;

namespace Vylersant_Facturacion.Desktop.Printing;

public static class EscPosReceiptFormatter
{
    private static readonly CultureInfo ReceiptCulture = CultureInfo.GetCultureInfo("es-DO");

    public static byte[] Format(ReceiptPrintDocument document, ReceiptPrinterSettings settings)
    {
        ArgumentNullException.ThrowIfNull(document);
        settings.Validate();
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var encoding = Encoding.GetEncoding(858, EncoderFallback.ReplacementFallback, DecoderFallback.ReplacementFallback);
        using var output = new MemoryStream();
        Write(output, [0x1B, 0x40]);
        Write(output, [0x1B, 0x61, 0x01]);
        Write(output, [0x1B, 0x45, 0x01]);
        WriteCentered(output, settings.BusinessName, settings.CharacterWidth, encoding);
        Write(output, [0x1B, 0x45, 0x00]);
        WriteCentered(output, "FACTURA", settings.CharacterWidth, encoding);
        WriteCentered(output, $"No. {document.InvoiceNumber}", settings.CharacterWidth, encoding);
        WriteCentered(output, document.IssuedAtUtc.ToLocalTime().ToString("dd/MM/yyyy HH:mm", ReceiptCulture), settings.CharacterWidth, encoding);
        if (!string.IsNullOrWhiteSpace(document.CustomerName))
            WriteCentered(output, $"Cliente: {document.CustomerName}", settings.CharacterWidth, encoding);

        Write(output, [0x1B, 0x61, 0x00]);
        WriteText(output, new string('-', settings.CharacterWidth), encoding);
        foreach (var line in document.Lines)
        {
            WriteWrapped(output, line.ProductName, settings.CharacterWidth, encoding);
            var quantityAndPrice = $"{line.Quantity:0.##} x {line.UnitPrice.ToString("N2", ReceiptCulture)}";
            var lineTotal = line.LineTotal.ToString("N2", ReceiptCulture);
            WriteText(output, AlignColumns(quantityAndPrice, lineTotal, settings.CharacterWidth), encoding);
        }

        WriteText(output, new string('-', settings.CharacterWidth), encoding);
        Write(output, [0x1B, 0x45, 0x01, 0x1D, 0x21, 0x10]);
        WriteText(output, AlignColumns("TOTAL", document.Total.ToString("C2", ReceiptCulture), settings.CharacterWidth), encoding);
        Write(output, [0x1D, 0x21, 0x00, 0x1B, 0x45, 0x00, 0x0A, 0x1B, 0x61, 0x01]);
        WriteCentered(output, "Gracias por su compra", settings.CharacterWidth, encoding);
        Write(output, [0x0A, 0x0A, 0x0A, 0x1D, 0x56, 0x00]);
        return output.ToArray();
    }

    private static string AlignColumns(string left, string right, int width)
    {
        if (right.Length >= width)
            right = right[^Math.Max(1, width - 1)..];
        var availableLeft = Math.Max(1, width - right.Length - 1);
        if (left.Length > availableLeft)
            left = left[..availableLeft];
        return left + new string(' ', Math.Max(1, width - left.Length - right.Length)) + right;
    }

    private static void WriteWrapped(Stream output, string value, int width, Encoding encoding)
    {
        var words = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var row = new StringBuilder();
        foreach (var word in words)
        {
            if (row.Length > 0 && row.Length + 1 + word.Length > width)
            {
                WriteText(output, row.ToString(), encoding);
                row.Clear();
            }
            if (row.Length > 0)
                row.Append(' ');
            if (word.Length > width)
            {
                for (var offset = 0; offset < word.Length; offset += width)
                {
                    var piece = word.Substring(offset, Math.Min(width, word.Length - offset));
                    if (offset + width < word.Length)
                        WriteText(output, piece, encoding);
                    else
                        row.Append(piece);
                }
            }
            else
                row.Append(word);
        }
        if (row.Length > 0)
            WriteText(output, row.ToString(), encoding);
    }

    private static void WriteCentered(Stream output, string value, int width, Encoding encoding)
    {
        foreach (var row in Wrap(value, width))
            WriteText(output, row.PadLeft((width + row.Length) / 2).PadRight(width), encoding);
    }

    private static IEnumerable<string> Wrap(string value, int width)
    {
        var row = new StringBuilder();
        foreach (var word in value.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            if (row.Length > 0 && row.Length + 1 + word.Length > width)
            {
                yield return row.ToString();
                row.Clear();
            }
            if (row.Length > 0)
                row.Append(' ');
            row.Append(word.Length > width ? word[..width] : word);
        }
        if (row.Length > 0)
            yield return row.ToString();
    }

    private static void WriteText(Stream output, string value, Encoding encoding)
    {
        Write(output, encoding.GetBytes(value));
        Write(output, [0x0A]);
    }

    private static void Write(Stream output, byte[] bytes) => output.Write(bytes);
}

