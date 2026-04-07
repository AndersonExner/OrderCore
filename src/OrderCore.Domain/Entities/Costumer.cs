using OrderCore.Domain.Common;

namespace OrderCore.Domain.Entities
{
    public class Customer : BaseEntity
    {
        public string Name { get; private set; }
        public string Email { get; private set; }

        private Customer() 
        {
            Name = string.Empty;
            Email = string.Empty;
        }
        
        public Customer(string name, string email)
        {
            SetName(name);
            SetEmail(email);
        }

        public void Update(string name, string email)
        {
            SetName(name);
            SetEmail(email);
        }

        private void SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Customer name cannot be empty.");

            Name = name.Trim();
        }

        private void SetEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Customer email cannot be empty.");

            if (!email.Contains("@"))
                throw new ArgumentException("Invalid email.");

            Email = email.Trim().ToLowerInvariant();
        }


    }
}
