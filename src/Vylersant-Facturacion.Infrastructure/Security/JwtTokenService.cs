using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Vylersant_Facturacion.Application.Security;
using Vylersant_Facturacion.Domain.Entities.Users;

namespace Vylersant_Facturacion.Infrastructure.Security
{
    public sealed class JwtTokenService : ITokenService
    {
        private readonly JwtSettings _jwtSettings;

        public JwtTokenService(IOptions<JwtSettings> options)
        {
            _jwtSettings = options.Value;
        }

        public AccessToken Generate(User user)
        {
            var expiresAtUtc =
                DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);

            var claims = new List<Claim>
            {
                new(TokenClaimNames.UserId, user.Id.ToString()),
                new(TokenClaimNames.BusinessId, user.BusinessId.ToString()),
                new(TokenClaimNames.Email, user.Email),
                new(TokenClaimNames.Name, user.Name),
                new(TokenClaimNames.Role, user.Role.ToString())
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtSettings.Key));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: expiresAtUtc,
                signingCredentials: credentials);

            var tokenValue =
                new JwtSecurityTokenHandler().WriteToken(token);

            return new AccessToken(
                tokenValue,
                expiresAtUtc);
        }
    }
}
