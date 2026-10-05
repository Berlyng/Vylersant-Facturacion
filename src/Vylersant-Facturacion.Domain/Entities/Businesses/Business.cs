namespace Vylersant_Facturacion.Domain.Entities.Businesses
{
    public sealed class Business
    {

        public Business( string name, string? address = null, string? phoneNumber = null, string? email = null)
        { 
            if(string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("El nombre del negocio no puede ser nulo o vacío.", nameof(name));
            }

            Id = Guid.NewGuid();
            Name = name.Trim();
            Address = address?.Trim();
            PhoneNumber = phoneNumber?.Trim();
            Email = email?.Trim();
            IsActive = true;
        }

        public Guid Id { get; private set; }
        public string Name { get; private set; }
        
        public bool IsActive { get; private set; }
        public string? Address { get; private set; }
        public string? PhoneNumber { get; private set; }
        public string? Email { get; private set; }

        public void Activate()
        {
            IsActive = true;
        }

        public void Deactivate()
        {
            IsActive = false;
        }

    }
}
