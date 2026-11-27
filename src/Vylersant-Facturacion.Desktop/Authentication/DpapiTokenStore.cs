using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.IO;

namespace Vylersant_Facturacion.Desktop.Authentication;

public sealed class DpapiTokenStore : ITokenStore
{
    private const int CurrentUserProtection = 0x1;
    private readonly string _path = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "VylersantFacturacion",
        "session.bin");

    public async Task<SessionTokens?> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_path))
            return null;

        var protectedBytes = await File.ReadAllBytesAsync(_path, cancellationToken);
        var clearBytes = Unprotect(protectedBytes);
        try
        {
            return JsonSerializer.Deserialize<SessionTokens>(clearBytes);
        }
        catch (JsonException)
        {
            await ClearAsync(cancellationToken);
            return null;
        }
        finally
        {
            CryptographicOperations.ZeroMemory(clearBytes);
        }
    }

    public async Task SaveAsync(SessionTokens tokens, CancellationToken cancellationToken = default)
    {
        var clearBytes = JsonSerializer.SerializeToUtf8Bytes(tokens);
        byte[] protectedBytes;
        try
        {
            protectedBytes = Protect(clearBytes);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(clearBytes);
        }

        var directory = Path.GetDirectoryName(_path)!;
        Directory.CreateDirectory(directory);
        var temporaryPath = _path + ".tmp";
        await File.WriteAllBytesAsync(temporaryPath, protectedBytes, cancellationToken);
        File.Move(temporaryPath, _path, overwrite: true);
        CryptographicOperations.ZeroMemory(protectedBytes);
    }

    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (File.Exists(_path))
            File.Delete(_path);
        return Task.CompletedTask;
    }

    private static byte[] Protect(byte[] input) => Transform(input, protect: true);

    private static byte[] Unprotect(byte[] input) => Transform(input, protect: false);

    private static byte[] Transform(byte[] input, bool protect)
    {
        var inputBlob = CreateBlob(input);
        DataBlob outputBlob = default;
        try
        {
            var success = protect
                ? CryptProtectData(ref inputBlob, "Vylersant Facturación session", IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, CurrentUserProtection, out outputBlob)
                : CryptUnprotectData(ref inputBlob, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, CurrentUserProtection, out outputBlob);
            if (!success)
                throw new CryptographicException(Marshal.GetLastWin32Error());

            var output = new byte[outputBlob.Size];
            Marshal.Copy(outputBlob.Data, output, 0, output.Length);
            return output;
        }
        finally
        {
            FreeBlob(ref inputBlob, LocalFree);
            if (outputBlob.Data != IntPtr.Zero)
                LocalFree(outputBlob.Data);
        }
    }

    private static DataBlob CreateBlob(byte[] bytes)
    {
        var pointer = Marshal.AllocHGlobal(bytes.Length);
        Marshal.Copy(bytes, 0, pointer, bytes.Length);
        return new DataBlob { Size = bytes.Length, Data = pointer };
    }

    private static void FreeBlob(ref DataBlob blob, Func<IntPtr, IntPtr> free)
    {
        if (blob.Data != IntPtr.Zero)
        {
            Marshal.Copy(new byte[blob.Size], 0, blob.Data, blob.Size);
            free(blob.Data);
        }
        blob = default;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct DataBlob
    {
        public int Size;
        public IntPtr Data;
    }

    [DllImport("crypt32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CryptProtectData(ref DataBlob input, string description, IntPtr optionalEntropy, IntPtr reserved, IntPtr prompt, int flags, out DataBlob output);

    [DllImport("crypt32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CryptUnprotectData(ref DataBlob input, IntPtr description, IntPtr optionalEntropy, IntPtr reserved, IntPtr prompt, int flags, out DataBlob output);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr LocalFree(IntPtr memory);
}

