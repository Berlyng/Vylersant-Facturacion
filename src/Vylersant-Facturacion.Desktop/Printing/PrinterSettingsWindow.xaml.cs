using System.Printing;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Vylersant_Facturacion.Desktop.Printing;

public partial class PrinterSettingsWindow : Window
{
    private readonly Guid _businessId;
    private readonly ReceiptPrinterSettingsStore _store;

    public PrinterSettingsWindow(Guid businessId, ReceiptPrinterSettingsStore store)
    {
        _businessId = businessId;
        _store = store;
        InitializeComponent();
        PaperWidthComboBox.SelectedIndex = 1;
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            var printers = new LocalPrintServer().GetPrintQueues().Select(queue => queue.Name).OrderBy(name => name).ToList();
            PrinterComboBox.ItemsSource = printers;
            var existing = await _store.LoadAsync(_businessId);
            if (existing is not null)
            {
                BusinessNameTextBox.Text = existing.BusinessName;
                if (!printers.Contains(existing.PrinterName, StringComparer.OrdinalIgnoreCase))
                    printers.Add(existing.PrinterName);
                PrinterComboBox.ItemsSource = printers;
                PrinterComboBox.SelectedItem = printers.FirstOrDefault(name => string.Equals(name, existing.PrinterName, StringComparison.OrdinalIgnoreCase));
                PaperWidthComboBox.SelectedIndex = existing.CharacterWidth switch { 32 => 0, 48 => 2, _ => 1 };
            }
            if (PrinterComboBox.SelectedIndex < 0 && printers.Count > 0)
                PrinterComboBox.SelectedIndex = 0;
            if (printers.Count == 0)
                PrinterErrorText.Text = "Windows no muestra impresoras instaladas. Instala o conecta la impresora y vuelve a abrir esta configuración.";
        }
        catch (PrintQueueException exception)
        {
            PrinterErrorText.Text = $"No se pudo consultar la lista de impresoras: {exception.Message}";
        }
        catch (System.Printing.PrintSystemException exception)
        {
            PrinterErrorText.Text = $"No se pudo consultar la lista de impresoras: {exception.Message}";
        }
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var selectedWidth = PaperWidthComboBox.SelectedItem as ComboBoxItem;
            var settings = new ReceiptPrinterSettings(
                BusinessNameTextBox.Text,
                PrinterComboBox.SelectedItem as string ?? string.Empty,
                int.TryParse(selectedWidth?.Tag?.ToString(), out var width) ? width : 42).Validate();
            await _store.SaveAsync(_businessId, settings);
            DialogResult = true;
        }
        catch (ArgumentException exception)
        {
            PrinterErrorText.Text = exception.Message;
        }
        catch (IOException exception)
        {
            PrinterErrorText.Text = $"No se pudo guardar la configuración: {exception.Message}";
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}

