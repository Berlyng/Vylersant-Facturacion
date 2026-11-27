namespace Vylersant_Facturacion.Application.Printing;

public sealed record ReceiptPrintResult(
    bool Succeeded,
    DateTime CompletedAtUtc,
    string? JobId = null,
    string? ErrorMessage = null)
{
    public static ReceiptPrintResult Success(DateTime completedAtUtc, string? jobId = null) =>
        new(true, completedAtUtc, jobId);

    public static ReceiptPrintResult Failure(DateTime completedAtUtc, string errorMessage, string? jobId = null) =>
        new(false, completedAtUtc, jobId, errorMessage);
}

