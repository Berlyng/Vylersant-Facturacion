using System.Net.Http;
using System.Windows;
using Vylersant_Facturacion.Desktop.Authentication;

namespace Vylersant_Facturacion.Desktop;

public partial class MainWindow : Window
{
    private readonly SessionService _sessionService;

    public MainWindow(SessionService sessionService)
    {
        _sessionService = sessionService;
        InitializeComponent();
        LoginPanel.Visibility = Visibility.Visible;
    }

    public async Task RestoreSessionAsync()
    {
        try
        {
            var session = await _sessionService.RestoreAsync();
            if (session is not null)
                ShowAuthenticatedShell(session);
        }
        catch (HttpRequestException)
        {
            LoginErrorText.Text = "No se pudo conectar con el servidor. Verifica la conexión e inténtalo de nuevo.";
        }
        catch (TaskCanceledException)
        {
            LoginErrorText.Text = "El servidor tardó demasiado en responder. Inténtalo de nuevo.";
        }
    }

    private async void Login_Click(object sender, RoutedEventArgs e)
    {
        LoginErrorText.Text = string.Empty;
        LoginButton.IsEnabled = false;
        try
        {
            var session = await _sessionService.LoginAsync(EmailTextBox.Text, PasswordBox.Password);
            PasswordBox.Clear();
            ShowAuthenticatedShell(session);
        }
        catch (AuthenticationApiException exception)
        {
            LoginErrorText.Text = exception.Message;
        }
        catch (HttpRequestException)
        {
            LoginErrorText.Text = "No se pudo conectar con el servidor. Verifica la conexión e inténtalo de nuevo.";
        }
        catch (TaskCanceledException)
        {
            LoginErrorText.Text = "El servidor tardó demasiado en responder. Inténtalo de nuevo.";
        }
        finally
        {
            LoginButton.IsEnabled = true;
        }
    }

    private async void Logout_Click(object sender, RoutedEventArgs e)
    {
        await _sessionService.LogoutAsync();
        ShellPanel.Visibility = Visibility.Collapsed;
        LoginPanel.Visibility = Visibility.Visible;
        EmailTextBox.Clear();
        PasswordBox.Clear();
        LoginErrorText.Text = "La sesión se cerró en este dispositivo.";
    }

    private void Navigation_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: string section })
            return;

        PageTitleText.Text = section;
        PageDescriptionText.Text = section == "Inicio"
            ? "Tu espacio de trabajo está listo."
            : $"La sección de {section.ToLowerInvariant()} estará disponible en un siguiente issue.";
    }

    private void ShowAuthenticatedShell(AuthenticatedSession session)
    {
        UserNameText.Text = session.User.Name;
        UserEmailText.Text = session.User.Email;
        LoginPanel.Visibility = Visibility.Collapsed;
        ShellPanel.Visibility = Visibility.Visible;
    }
}

