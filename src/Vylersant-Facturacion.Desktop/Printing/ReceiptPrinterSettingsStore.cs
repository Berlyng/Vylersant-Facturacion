using System.Text.Json;
using System.IO;

namespace Vylersant_Facturacion.Desktop.Printing;

public sealed class ReceiptPrinterSettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };
    private readonly string _directory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Vylersant", "Facturacion", "Impresoras");

    public async Task<ReceiptPrinterSettings?> LoadAsync(Guid businessId, CancellationToken cancellationToken = default)
    {
        var path = GetPath(businessId);
        if (!File.Exists(path))
            return null;
        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<ReceiptPrinterSettings>(stream, JsonOptions, cancellationToken);
    }

    public async Task SaveAsync(Guid businessId, ReceiptPrinterSettings settings, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);
        settings.Validate();
        Directory.CreateDirectory(_directory);
        var path = GetPath(businessId);
        var temporaryPath = path + ".tmp";
        await using (var stream = File.Create(temporaryPath))
            await JsonSerializer.SerializeAsync(stream, settings, JsonOptions, cancellationToken);
        File.Move(temporaryPath, path, true);
    }

    private string GetPath(Guid businessId)
    {
        if (businessId == Guid.Empty)
            throw new ArgumentException("El negocio es obligatorio.", nameof(businessId));
        return Path.Combine(_directory, $"{businessId:N}.json");
    }
}

