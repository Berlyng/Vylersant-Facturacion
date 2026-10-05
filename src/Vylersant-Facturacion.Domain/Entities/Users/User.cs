using System;
using System.Collections.Generic;
using System.Text;

namespace Vylersant_Facturacion.Domain.Entities.Users
{
    public sealed class User
    {
        public User(Guid businessId, string name, string email, string passwordHash, UserRole role)
        {
            if (businessId == Guid.Empty)
                throw new ArgumentException("BusinessId no puede estar vacío.", nameof(businessId));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name no puede estar vacío.", nameof(name));
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email no puede estar vacío.", nameof(email));
            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new ArgumentException("PasswordHash no puede estar vacío.", nameof(passwordHash));
            if (!Enum.IsDefined(role))
            {
                throw new ArgumentException(
                    "El rol especificado no es válido.",
                    nameof(role));
            }

            Id = Guid.NewGuid();
            BusinessId = businessId;
            Name = name.Trim();
            Email = email.Trim().ToLowerInvariant();
            PasswordHash = passwordHash.Trim();
            IsActive = true;
            Role = role;
        }

        public Guid Id { get; private set; }
        public Guid BusinessId { get; private set; }
        public string Name { get; private set; }
        public string Email { get; private set; }
        public string PasswordHash { get; private set; }
        public bool IsActive { get; private set; }
        public UserRole Role { get; private set; }

        public void Activate()
        {
            IsActive = true;
        }

        public void Deactivate()
        {
            IsActive = false;
        }

        public void ChangeRole(UserRole role)
        {
            if (!Enum.IsDefined(role))
            {
                throw new ArgumentException(
                    "El rol especificado no es válido.",
                    nameof(role));
            }

            Role = role;
        }

    }
}
