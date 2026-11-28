using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace Vylersant_Facturacion.Desktop.Printing;

public sealed class WindowsRawPrinterTransport : IRawPrinterTransport
{
    public Task<string> SendAsync(string printerName, byte[] data, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(printerName);
        ArgumentNullException.ThrowIfNull(data);
        cancellationToken.ThrowIfCancellationRequested();

        if (!OpenPrinter(printerName, out var printer, IntPtr.Zero))
            throw new Win32Exception(Marshal.GetLastWin32Error(), $"No se pudo abrir la impresora '{printerName}'.");
        try
        {
            EnsurePrinterReady(printer);
            var info = new DocInfo1 { DocumentName = "Vylersant - Recibo", DataType = "RAW" };
            var jobId = StartDocPrinter(printer, 1, ref info);
            if (jobId == 0)
                throw new Win32Exception(Marshal.GetLastWin32Error(), "La cola de impresión no aceptó el recibo.");
            try
            {
                if (!StartPagePrinter(printer))
                    throw new Win32Exception(Marshal.GetLastWin32Error(), "No se pudo iniciar la página de impresión.");
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (!WritePrinter(printer, data, data.Length, out var written) || written != data.Length)
                        throw new Win32Exception(Marshal.GetLastWin32Error(), "La impresora no recibió todos los datos del recibo.");
                }
                finally
                {
                    EndPagePrinter(printer);
                }
            }
            finally
            {
                EndDocPrinter(printer);
            }
            return Task.FromResult(jobId.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }
        finally
        {
            ClosePrinter(printer);
        }
    }

    private static void EnsurePrinterReady(IntPtr printer)
    {
        GetPrinter(printer, 2, IntPtr.Zero, 0, out var requiredBytes);
        if (requiredBytes == 0)
            throw new Win32Exception(Marshal.GetLastWin32Error(), "No se pudo consultar el estado de la impresora.");

        var buffer = Marshal.AllocHGlobal((int)requiredBytes);
        try
        {
            if (!GetPrinter(printer, 2, buffer, requiredBytes, out _))
                throw new Win32Exception(Marshal.GetLastWin32Error(), "No se pudo consultar el estado de la impresora.");
            var info = Marshal.PtrToStructure<PrinterInfo2>(buffer);
            if ((info.Status & 0x00000020) != 0)
                throw new IOException("La impresora indica que no tiene papel.");
            if ((info.Status & 0x00000080) != 0)
                throw new IOException("La impresora está desconectada.");
            if ((info.Status & 0x00000008) != 0)
                throw new IOException("La impresora indica un atasco de papel.");
            if ((info.Status & 0x00000002) != 0)
                throw new IOException("La impresora reporta un error. Revisa el dispositivo y vuelve a intentar.");
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct DocInfo1
    {
        [MarshalAs(UnmanagedType.LPWStr)] public string? DocumentName;
        [MarshalAs(UnmanagedType.LPWStr)] public string? OutputFile;
        [MarshalAs(UnmanagedType.LPWStr)] public string? DataType;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct PrinterInfo2
    {
        public IntPtr ServerName;
        public IntPtr PrinterName;
        public IntPtr ShareName;
        public IntPtr PortName;
        public IntPtr DriverName;
        public IntPtr Comment;
        public IntPtr Location;
        public IntPtr DevMode;
        public IntPtr SepFile;
        public IntPtr PrintProcessor;
        public IntPtr DataType;
        public IntPtr Parameters;
        public IntPtr SecurityDescriptor;
        public uint Attributes;
        public uint Priority;
        public uint DefaultPriority;
        public uint StartTime;
        public uint UntilTime;
        public uint Status;
        public uint JobCount;
        public uint AveragePagesPerMinute;
    }

    [DllImport("winspool.drv", EntryPoint = "OpenPrinterW", SetLastError = true, CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool OpenPrinter(string printerName, out IntPtr printer, IntPtr defaults);

    [DllImport("winspool.drv", EntryPoint = "ClosePrinter", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ClosePrinter(IntPtr printer);

    [DllImport("winspool.drv", EntryPoint = "GetPrinterW", SetLastError = true, CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetPrinter(IntPtr printer, int level, IntPtr printerInfo, uint bufferSize, out uint needed);

    [DllImport("winspool.drv", EntryPoint = "StartDocPrinterW", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern uint StartDocPrinter(IntPtr printer, int level, ref DocInfo1 documentInfo);

    [DllImport("winspool.drv", EntryPoint = "EndDocPrinter", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EndDocPrinter(IntPtr printer);

    [DllImport("winspool.drv", EntryPoint = "StartPagePrinter", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool StartPagePrinter(IntPtr printer);

    [DllImport("winspool.drv", EntryPoint = "EndPagePrinter", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EndPagePrinter(IntPtr printer);

    [DllImport("winspool.drv", EntryPoint = "WritePrinter", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool WritePrinter(IntPtr printer, byte[] data, int count, out int written);
}

