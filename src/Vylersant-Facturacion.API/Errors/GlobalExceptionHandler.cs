using Microsoft.AspNetCore.Diagnostics;
using Vylersant_Facturacion.Application.Authentication;
using Vylersant_Facturacion.Application.Registration;

namespace Vylersant_Facturacion.API.Errors
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var (statusCode, title, detail) = exception switch
            {
                EmailAlreadyExistsException => (StatusCodes.Status409Conflict, "Correo ya existe", exception.Message),
                InvalidCredentialsException => (StatusCodes.Status401Unauthorized, "Credenciales inválidas", "el correo electrónico o la contraseña son incorrectos"),
                InactiveUserException => (StatusCodes.Status403Forbidden, "Usuario inactivo", "No fue posible iniciar sesión con las credenciales proporcionadas."),
                ArgumentException => (StatusCodes.Status400BadRequest, "Solicitud inválida", exception.Message),
                _ => (StatusCodes.Status500InternalServerError, "Error interno del servidor", "Ocurrió un error inesperado. Por favor, inténtelo de nuevo más tarde.")
            };
            if (statusCode == StatusCodes.Status500InternalServerError)
            {
                _logger.LogError(
                    exception,
                    "Ocurrió una excepción no controlada.");
            }

            await Results.Problem(
                    statusCode: statusCode,
                    title: title,
                    detail: detail,
                    instance: httpContext.Request.Path)
                .ExecuteAsync(httpContext);

            return true;
        }


    }
}
