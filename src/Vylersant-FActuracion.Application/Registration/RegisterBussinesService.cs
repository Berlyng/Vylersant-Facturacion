using Vylersant_Facturacion.Application.Abstraccion;
using Vylersant_Facturacion.Application.Bussinesses;
using Vylersant_Facturacion.Application.Security;
using Vylersant_Facturacion.Application.Users;
using Vylersant_Facturacion.Domain.Entities.Businesses;
using Vylersant_Facturacion.Domain.Entities.Users;

namespace Vylersant_Facturacion.Application.Registration
{
    public sealed class RegisterBussinesService
    {
        private readonly IBussinesRepository _bussinesRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;

        public RegisterBussinesService(IBussinesRepository bussinesRepository, IUnitOfWork unitOfWork, IUserRepository userRepository, IPasswordHasher passwordHasher)
        {
            _bussinesRepository = bussinesRepository;
            _unitOfWork = unitOfWork;
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }


        public async Task<Guid> ExecuteAsync(RegisterBusinessRequest request, CancellationToken cancellationToken = default)
        {
            var existingUser = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (existingUser is not null)
            {
               throw new EmailAlreadyExistsException(request.Email);
            }

            var bussines = new Business(request.BussinesName);
            var passwordHash = _passwordHasher.Hash(request.Password);
            var owner = new User(bussines.Id, request.OwnerName, request.Email, passwordHash, UserRole.Owner);

            await _bussinesRepository.AddAsync(bussines, cancellationToken);
            await _userRepository.AddAsync(owner, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return bussines.Id;
        }
    }
}
