using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Vylersant_Facturacion.Contracts.Invoices;
using Vylersant_Facturacion.Desktop.PointOfSale;

namespace Vylersant_Facturacion.Desktop;

public enum ToastKind { Success, Error, Warning, Info }

public partial class MainWindow
{
    private bool _sidebarCompact;
    private bool _sidebarUserSet;
    private TaskCompletionSource<bool>? _confirmTcs;
    private IInputElement? _focusBeforeConfirm;

    private static IEnumerable<T> LogicalDescendants<T>(DependencyObject root) where T : DependencyObject
    {
        foreach (var child in LogicalTreeHelper.GetChildren(root).OfType<DependencyObject>())
        {
            if (child is T match)
                yield return match;
            foreach (var descendant in LogicalDescendants<T>(child))
                yield return descendant;
        }
    }

    private int CartCount() => _posViewModel.Cart.Count;

    private void NavigateTo(string tag)
    {
        var item = LogicalDescendants<RadioButton>(Sidebar).FirstOrDefault(button => button.Tag as string == tag);
        if (item is null)
            return;
        item.IsChecked = true;
        Navigation_Click(item, new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent, item));
    }

    private void FocusAfterLayout(Control target, bool selectAll = false) =>
        Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() =>
        {
            target.Focus();
            Keyboard.Focus(target);
            if (selectAll && target is TextBox textBox)
                textBox.SelectAll();
        }));

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (ConfirmLayer.Visibility == Visibility.Visible)
        {
            if (e.Key == Key.Escape)
            {
                ResolveConfirm(false);
                e.Handled = true;
            }
            else if (e.Key == Key.F4 && Keyboard.Modifiers == ModifierKeys.Alt)
                e.Handled = true;
            return;
        }

        if (ShellPanel.Visibility != Visibility.Visible)
            return;

        if (e.Key == Key.F4 && Keyboard.Modifiers == ModifierKeys.Alt)
        {
            CloseWindow_Click(this, new RoutedEventArgs());
            e.Handled = true;
            return;
        }

        var onlyControl = Keyboard.Modifiers == ModifierKeys.Control;
        if (onlyControl)
        {
            switch (e.Key)
            {
                case Key.D1: case Key.NumPad1: NavigateTo("Inicio"); e.Handled = true; return;
                case Key.D2: case Key.NumPad2: NavigateTo("POS"); e.Handled = true; return;
                case Key.D3: case Key.NumPad3: NavigateTo("Facturas"); e.Handled = true; return;
                case Key.D4: case Key.NumPad4: NavigateTo("Productos"); e.Handled = true; return;
                case Key.D5: case Key.NumPad5: NavigateTo("Clientes"); e.Handled = true; return;
                case Key.N: NewSale_Click(this, new RoutedEventArgs()); e.Handled = true; return;
                case Key.K: FocusAfterLayout(GlobalSearchBox, true); e.Handled = true; return;
                case Key.B: _sidebarUserSet = true; SetSidebarCompact(!_sidebarCompact); e.Handled = true; return;
                case Key.P when InvoicesContent.Visibility == Visibility.Visible:
                    PrintInvoice_Click(this, new RoutedEventArgs()); e.Handled = true; return;
            }
        }
        else if (Keyboard.Modifiers == ModifierKeys.None)
        {
            switch (e.Key)
            {
                case Key.F2:
                    NewSale_Click(this, new RoutedEventArgs());
                    FocusAfterLayout(ProductSearchBox, true);
                    e.Handled = true;
                    return;
                case Key.F9 when PosContent.Visibility == Visibility.Visible && ConfirmSaleButton.IsEnabled:
                    ConfirmSale_Click(ConfirmSaleButton, new RoutedEventArgs());
                    e.Handled = true;
                    return;
                case Key.Escape when Keyboard.FocusedElement == ProductSearchBox && ProductSearchBox.Text.Length > 0:
                    ProductSearchBox.Clear(); e.Handled = true; return;
                case Key.Escape when Keyboard.FocusedElement == GlobalSearchBox && GlobalSearchBox.Text.Length > 0:
                    GlobalSearchBox.Clear(); e.Handled = true; return;
            }
        }
    }

    private void ProductSearch_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;
        if (ProductsItems.Items.Count == 1 && ProductsItems.Items[0] is Vylersant_Facturacion.Contracts.Products.ProductResponse product)
        {
            _posViewModel.AddProduct(product);
            ProductSearchBox.Clear();
            ShowToast(ToastKind.Success, "Producto agregado", product.Name, seconds: 2);
        }
        e.Handled = true;
    }

    private void ClearSearch_Click(object sender, RoutedEventArgs e)
    {
        ProductSearchBox.Clear();
        FocusAfterLayout(ProductSearchBox);
    }

    private void GlobalSearch_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter || string.IsNullOrWhiteSpace(GlobalSearchBox.Text))
            return;
        var query = GlobalSearchBox.Text.Trim();
        if (query.All(char.IsDigit))
        {
            _invoiceHistoryViewModel.Search(query);
            NavigateTo("Facturas");
        }
        else
        {
            NavigateTo("POS");
            ProductSearchBox.Text = query;
        }
        e.Handled = true;
    }

    private void GlobalSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (InvoicesContent.Visibility == Visibility.Visible)
            _invoiceHistoryViewModel.Search(GlobalSearchBox.Text.All(char.IsDigit) ? GlobalSearchBox.Text : string.Empty);
    }

    private void NewSale_Click(object sender, RoutedEventArgs e)
    {
        NavigateTo("POS");
        FocusAfterLayout(ProductSearchBox);
    }

    private void QuickAction_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: string section })
            NavigateTo(section);
    }

    private void GoToPos_Click(object sender, RoutedEventArgs e) => NewSale_Click(sender, e);

    private async void RetryCatalog_Click(object sender, RoutedEventArgs e)
    {
        if (_currentSession is not null)
            await _posViewModel.LoadAsync(_currentSession.Tokens.AccessToken);
    }

    private async void RetryInvoices_Click(object sender, RoutedEventArgs e)
    {
        if (_currentSession is not null)
        {
            _invoiceHistoryViewModel.Search(GlobalSearchBox.Text.All(char.IsDigit) ? GlobalSearchBox.Text : string.Empty);
            await _invoiceHistoryViewModel.LoadAsync(_currentSession.Tokens.AccessToken);
        }
    }

    private async void InvoiceList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_currentSession is not null && InvoicesList.SelectedItem is InvoiceListItemResponse invoice)
            await _invoiceHistoryViewModel.LoadDetailsAsync(invoice, _currentSession.Tokens.AccessToken);
    }

    private void ToggleSidebar_Click(object sender, RoutedEventArgs e)
    {
        _sidebarUserSet = true;
        SetSidebarCompact(!_sidebarCompact);
    }

    private void SetSidebarCompact(bool compact, bool animate = true)
    {
        _sidebarCompact = compact;
        var width = compact ? 64d : 216d;
        if (animate)
            Sidebar.BeginAnimation(FrameworkElement.WidthProperty,
                new DoubleAnimation(width, TimeSpan.FromMilliseconds(160)) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } });
        else
        {
            Sidebar.BeginAnimation(FrameworkElement.WidthProperty, null);
            Sidebar.Width = width;
        }

        foreach (var label in LogicalDescendants<FrameworkElement>(Sidebar).Where(element => element.Tag as string == "nav-label"))
            label.Visibility = compact ? Visibility.Collapsed : Visibility.Visible;
        SidebarBrand.Visibility = compact ? Visibility.Collapsed : Visibility.Visible;
        SidebarToggleButton.ToolTip = compact ? "Expandir menú (Ctrl+B)" : "Contraer menú (Ctrl+B)";
    }

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e) => ApplyResponsive();

    private void ApplyResponsive()
    {
        if (!IsInitialized || Sidebar is null)
            return;
        var width = ActualWidth;
        if (!_sidebarUserSet && (width < 1100) != _sidebarCompact)
            SetSidebarCompact(width < 1100, animate: false);
        PosCartColumn.Width = new GridLength(width < 1100 ? 320 : 372);
        GlobalSearchHost.Width = width < 1100 ? 190 : 260;
        ShiftChip.Visibility = width < 1100 || string.IsNullOrWhiteSpace(ShiftText.Text) ? Visibility.Collapsed : Visibility.Visible;
    }

    private void ProductsScroll_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        const double minCard = 172, gap = 10, padding = 12, scrollbar = 14;
        var usable = Math.Max(minCard, e.NewSize.Width - padding - scrollbar);
        var columns = Math.Max(1, (int)((usable + gap) / (minCard + gap)));
        ProductsItems.Tag = Math.Floor((usable + gap) / columns);
    }

    private void LoginField_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender == EmailTextBox)
            SetFieldError(EmailTextBox, EmailErrorText, null);
        else
        {
            SetFieldError(PasswordBox, PasswordErrorText, null);
            PasswordRevealBox.Tag = null;
        }
        LoginErrorText.Text = string.Empty;
    }

    private void LoginField_PasswordChanged(object sender, RoutedEventArgs e)
    {
        SetFieldError(PasswordBox, PasswordErrorText, null);
        LoginErrorText.Text = string.Empty;
    }

    private void LoginField_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && sender == EmailTextBox)
        {
            (RevealPasswordToggle.IsChecked == true ? (Control)PasswordRevealBox : PasswordBox).Focus();
            e.Handled = true;
        }
    }

    private void PasswordBox_CapsCheck(object sender, RoutedEventArgs e) =>
        CapsLockHint.Visibility = Keyboard.IsKeyToggled(Key.CapsLock) ? Visibility.Visible : Visibility.Collapsed;

    private void RevealPassword_Changed(object sender, RoutedEventArgs e)
    {
        if (!IsInitialized)
            return;
        var show = RevealPasswordToggle.IsChecked == true;
        if (show)
        {
            PasswordRevealBox.Text = PasswordBox.Password;
            PasswordBox.Visibility = Visibility.Collapsed;
            PasswordRevealBox.Visibility = Visibility.Visible;
            FocusAfterLayout(PasswordRevealBox);
            PasswordRevealBox.CaretIndex = PasswordRevealBox.Text.Length;
        }
        else
        {
            PasswordBox.Password = PasswordRevealBox.Text;
            PasswordRevealBox.Visibility = Visibility.Collapsed;
            PasswordBox.Visibility = Visibility.Visible;
            FocusAfterLayout(PasswordBox);
        }
        RevealPasswordToggle.Content = show ? "Ocultar" : "Mostrar";
    }

    private static void SetFieldError(Control field, TextBlock label, string? message)
    {
        var hasError = !string.IsNullOrWhiteSpace(message);
        field.Tag = hasError ? "Error" : null;
        label.Text = message ?? string.Empty;
        label.Visibility = hasError ? Visibility.Visible : Visibility.Collapsed;
    }

    private bool ValidateLoginFields()
    {
        var email = EmailTextBox.Text.Trim();
        var password = RevealPasswordToggle.IsChecked == true ? PasswordRevealBox.Text : PasswordBox.Password;
        Control? firstInvalid = null;
        var valid = true;

        if (email.Length == 0)
        {
            SetFieldError(EmailTextBox, EmailErrorText, "Escribe tu correo electrónico.");
            firstInvalid = EmailTextBox;
            valid = false;
        }
        else if (!System.Text.RegularExpressions.Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            SetFieldError(EmailTextBox, EmailErrorText, "Revisa el formato del correo electrónico.");
            firstInvalid = EmailTextBox;
            valid = false;
        }
        else
            SetFieldError(EmailTextBox, EmailErrorText, null);

        if (password.Length == 0)
        {
            SetFieldError(PasswordBox, PasswordErrorText, "Escribe tu contraseña.");
            PasswordRevealBox.Tag = "Error";
            firstInvalid ??= RevealPasswordToggle.IsChecked == true ? PasswordRevealBox : PasswordBox;
            valid = false;
        }
        else
        {
            SetFieldError(PasswordBox, PasswordErrorText, null);
            PasswordRevealBox.Tag = null;
        }

        LoginErrorText.Text = string.Empty;
        firstInvalid?.Focus();
        return valid;
    }

    private void SetLoginBusy(bool busy)
    {
        LoginButton.IsEnabled = !busy;
        EmailTextBox.IsEnabled = !busy;
        PasswordBox.IsEnabled = !busy;
        PasswordRevealBox.IsEnabled = !busy;
        RevealPasswordToggle.IsEnabled = !busy;
        LoginSpinner.Visibility = busy ? Visibility.Visible : Visibility.Collapsed;
        LoginButtonText.Text = busy ? "Entrando…" : "Entrar";
        if (!busy)
            FocusAfterLayout(string.IsNullOrWhiteSpace(EmailTextBox.Text) ? EmailTextBox : PasswordBox);
    }

    private void ResetLoginForm()
    {
        PasswordBox.Clear();
        PasswordRevealBox.Clear();
        RevealPasswordToggle.IsChecked = false;
        LoginErrorText.Text = string.Empty;
        SetFieldError(EmailTextBox, EmailErrorText, null);
        SetFieldError(PasswordBox, PasswordErrorText, null);
        PasswordRevealBox.Tag = null;
        CapsLockHint.Visibility = Visibility.Collapsed;
    }

    private async void ClearCart_Click(object sender, RoutedEventArgs e)
    {
        var count = CartCount();
        if (count == 0 || !await ConfirmAsync("¿Vaciar el carrito?", $"Se quitarán {count} {(count == 1 ? "producto" : "productos")} de esta venta.", "Vaciar carrito", destructive: true))
            return;
        _posViewModel.ClearCart();
        ShowToast(ToastKind.Info, "Carrito vaciado");
    }

    private void ConfirmCancel_Click(object sender, RoutedEventArgs e) => ResolveConfirm(false);
    private void ConfirmAccept_Click(object sender, RoutedEventArgs e) => ResolveConfirm(true);

    private Task<bool> ConfirmAsync(string title, string message, string acceptText, bool destructive = false, string cancelText = "Cancelar")
    {
        _confirmTcs?.TrySetResult(false);
        _confirmTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        _focusBeforeConfirm = Keyboard.FocusedElement;
        ConfirmTitle.Text = title;
        ConfirmMessage.Text = message;
        ConfirmAcceptButton.Content = acceptText;
        ConfirmCancelButton.Content = cancelText;
        ConfirmAcceptButton.Style = (Style)FindResource(destructive ? "DangerButton" : "PrimaryButton");
        ConfirmLayer.Visibility = Visibility.Visible;
        FocusAfterLayout(destructive ? ConfirmCancelButton : ConfirmAcceptButton);
        return _confirmTcs.Task;
    }

    private void ResolveConfirm(bool result)
    {
        ConfirmLayer.Visibility = Visibility.Collapsed;
        var pending = _confirmTcs;
        _confirmTcs = null;
        if (_focusBeforeConfirm is UIElement focusTarget && focusTarget.IsVisible)
            focusTarget.Focus();
        pending?.TrySetResult(result);
    }

    private void ShowToast(ToastKind kind, string title, string? message = null, string? actionText = null, Action? action = null, int seconds = 4)
    {
        while (ToastHost.Children.Count >= 4)
            ToastHost.Children.RemoveAt(0);

        var tone = kind switch
        {
            ToastKind.Success => (Accent: "SuccessText", Tint: "SuccessTint", Glyph: "\uE73E"),
            ToastKind.Error => (Accent: "Danger", Tint: "DangerTint", Glyph: "\uEA39"),
            ToastKind.Warning => (Accent: "Warning", Tint: "WarningTint", Glyph: "\uE7BA"),
            _ => (Accent: "TealDark", Tint: "TealTint", Glyph: "\uE946")
        };
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var icon = new Border
        {
            Width = 30, Height = 30, CornerRadius = new CornerRadius(15),
            Background = (Brush)FindResource(tone.Tint),
            Child = new TextBlock
            {
                Text = tone.Glyph, FontFamily = new FontFamily("Segoe MDL2 Assets"), FontSize = 14,
                Foreground = (Brush)FindResource(tone.Accent), HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            }
        };
        grid.Children.Add(icon);
        var textStack = new StackPanel { Margin = new Thickness(10, 0, 8, 0), VerticalAlignment = VerticalAlignment.Center };
        textStack.Children.Add(new TextBlock { Text = title, FontWeight = FontWeights.SemiBold, Foreground = (Brush)FindResource("Ink"), TextWrapping = TextWrapping.Wrap });
        if (!string.IsNullOrWhiteSpace(message))
            textStack.Children.Add(new TextBlock { Text = message, FontSize = 12, Foreground = (Brush)FindResource("Muted"), TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 2, 0, 0) });
        Grid.SetColumn(textStack, 1);
        grid.Children.Add(textStack);

        Border card = null!;
        DispatcherTimer timer = null!;
        var closing = false;
        var buttons = new StackPanel { Orientation = Orientation.Horizontal };
        if (!string.IsNullOrWhiteSpace(actionText) && action is not null)
        {
            var actionButton = new Button { Content = actionText, Style = (Style)FindResource("GhostButton"), MinHeight = 28, Padding = new Thickness(8, 0, 8, 0) };
            actionButton.Click += (_, _) => { Dismiss(); action(); };
            buttons.Children.Add(actionButton);
        }
        var closeButton = new Button
        {
            Style = (Style)FindResource("GhostButton"), Width = 28, MinHeight = 28, Padding = new Thickness(0),
            ToolTip = "Cerrar", Content = new TextBlock { Text = "\uE711", FontFamily = new FontFamily("Segoe MDL2 Assets"), FontSize = 11 }
        };
        AutomationProperties.SetName(closeButton, "Cerrar notificación");
        closeButton.Click += (_, _) => Dismiss();
        buttons.Children.Add(closeButton);
        Grid.SetColumn(buttons, 2);
        grid.Children.Add(buttons);

        card = new Border
        {
            Background = Brushes.White, BorderBrush = (Brush)FindResource("Line"), BorderThickness = new Thickness(1),
            CornerRadius = (CornerRadius)FindResource("RadiusLg"), Padding = new Thickness(12), Margin = new Thickness(0, 8, 0, 0),
            Effect = (System.Windows.Media.Effects.Effect)FindResource("ShadowPopover"), Opacity = 0, Child = grid
        };
        AutomationProperties.SetName(card, string.IsNullOrWhiteSpace(message) ? title : $"{title}. {message}");
        timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(seconds) };
        void Dismiss()
        {
            if (closing) return;
            closing = true;
            timer.Stop();
            var fade = new DoubleAnimation(0, TimeSpan.FromMilliseconds(140));
            fade.Completed += (_, _) => ToastHost.Children.Remove(card);
            card.BeginAnimation(OpacityProperty, fade);
        }
        timer.Tick += (_, _) => Dismiss();
        card.MouseEnter += (_, _) => timer.Stop();
        card.MouseLeave += (_, _) => { if (!closing) timer.Start(); };
        ToastHost.Children.Add(card);
        card.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(160)));
        timer.Start();
    }

    public void SetConnectionState(bool online, bool pending = false)
    {
        var colorKey = !online ? "Danger" : pending ? "WarningDot" : "Success";
        ConnectionDot.Fill = (Brush)FindResource(colorKey);
        ConnectionText.Foreground = (Brush)FindResource(!online ? "Danger" : pending ? "Warning" : "SuccessText");
        ConnectionText.Text = !online ? "Sin conexión" : pending ? "Sincronizando…" : "En línea";
        ConnectionChip.ToolTip = !online ? "No se puede contactar al servidor." : pending ? "Cambios pendientes de sincronizar." : "Conectado al servidor";
    }

    public void SetShiftState(string? text)
    {
        ShiftText.Text = text ?? string.Empty;
        ApplyResponsive();
    }
}

