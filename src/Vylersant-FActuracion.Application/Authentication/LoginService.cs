using System;
using System.Collections.Generic;
using System.Text;
using Vylersant_Facturacion.Application.Security;
using Vylersant_Facturacion.Application.Users;

namespace Vylersant_Facturacion.Application.Authentication
{
    public sealed class LoginService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;

        public LoginService(IUserRepository userRepository, IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<LoginResult> ExecuteAsync(LoginRequest request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (user is null)
            {
                throw new InvalidCredentialsException();
            }
            if (!user.IsActive)
            {
                throw new InactiveUserException();
            }
            if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            {
                throw new InvalidCredentialsException();
            }
            return new LoginResult(
                user.Id,
                user.BusinessId,
                user.Name,
                user.Email,
                user.Role);
        }
    }
}
