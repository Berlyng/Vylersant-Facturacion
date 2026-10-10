using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Vylersant_Facturacion.Application.Authentication;
using Vylersant_Facturacion.Application.Registration;
using Vylersant_Facturacion.Application.Security;
using Vylersant_Facturacion.Contracts.Authentication;
using LoginApplication =
    Vylersant_Facturacion.Application.Authentication.LoginRequest;
using LoginContract =
    Vylersant_Facturacion.Contracts.Authentication.LoginRequest;
using RegisterBusinessApplication =
    Vylersant_Facturacion.Application.Registration.RegisterBusinessRequest;
using RegisterBusinessContract =
    Vylersant_Facturacion.Contracts.Authentication.RegisterBusinessRequest;
using RefreshSessionContract =
    Vylersant_Facturacion.Contracts.Authentication.RefreshSessionRequest;

using RefreshSessionApplication =
    Vylersant_Facturacion.Application.Authentication.RefreshSessionRequest;

namespace Vylersant_Facturacion.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly RegisterBussinesService _registerBusinessService;
        private readonly LoginService _loginService;
        private readonly RefreshSessionService _refreshSessionService;

        public AuthController(RegisterBussinesService registerBusinessService, LoginService loginService, RefreshSessionService refreshSessionService)
        {
            _registerBusinessService = registerBusinessService;
            _loginService = loginService;
            _refreshSessionService = refreshSessionService;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<RegisterBusinessResponse>> Register(
     RegisterBusinessContract request,
     CancellationToken cancellationToken)
        {
            var applicationRequest =
                new RegisterBusinessApplication(
                    request.BusinessName,
                    request.OwnerName,
                    request.Email,
                    request.Password);

            var businessId =
                await _registerBusinessService.ExecuteAsync(
                    applicationRequest,
                    cancellationToken);

            return Ok(
                new RegisterBusinessResponse(businessId));
        }


        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login(
    LoginContract request,
    CancellationToken cancellationToken)
        {
            var applicationRequest =
                new LoginApplication(
                    request.Email,
                    request.Password);

            var result =
                await _loginService.ExecuteAsync(
                    applicationRequest,
                    cancellationToken);

            return Ok(
                    new LoginResponse(
                        result.UserId,
                        result.BusinessId,
                        result.Name,
                        result.Email,
                        result.Role.ToString(),
                        result.AccessToken,
                        result.AccessTokenExpiresAtUtc,
                        result.RefreshToken,
                        result.RefreshTokenExpiresAtUtc));
        }


        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<ActionResult<RefreshSessionResponse>> Refresh(RefreshSessionContract request,CancellationToken cancellationToken)
        {
            var applicationRequest =
                new RefreshSessionApplication(
                    request.RefreshToken);

            var result =
                await _refreshSessionService.ExecuteAsync(
                    applicationRequest,
                    cancellationToken);

            return Ok(
                new RefreshSessionResponse(
                    result.AccessToken,
                    result.AccessTokenExpiresAtUtc,
                    result.RefreshToken,
                    result.RefreshTokenExpiresAtUtc));
        }


        [Authorize]
        [HttpGet("me")]
        public ActionResult<CurrentUserResponse> Me()
        {
            var userIdValue =
                User.FindFirstValue(TokenClaimNames.UserId);

            var businessIdValue =
                User.FindFirstValue(TokenClaimNames.BusinessId);

            var email =
                User.FindFirstValue(TokenClaimNames.Email);

            var name =
                User.FindFirstValue(TokenClaimNames.Name);

            var role =
                User.FindFirstValue(TokenClaimNames.Role);
            User.FindFirstValue(ClaimTypes.Role);

            if (!Guid.TryParse(userIdValue, out var userId) ||
                !Guid.TryParse(businessIdValue, out var businessId))
            {
                return Unauthorized();
            }

            return Ok(
                new CurrentUserResponse(
                    userId,
                    businessId,
                    name ?? string.Empty,
                    email ?? string.Empty,
                    role ?? string.Empty));
        }
    }
}
