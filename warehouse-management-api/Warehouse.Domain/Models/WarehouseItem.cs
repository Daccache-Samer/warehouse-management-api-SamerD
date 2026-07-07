namespace Warehouse.DomainWarehouse.Domain.Models
{
    public class WarehouseItem
    {
        public string Id { get; private set; }
        public string ProductId { get; private set; }
        public string LocationCode { get; private set; }
        public int Quantity { get; private set; }
        public DateTime LastUpdatedAt { get; private set; }

        // Private constructor forces the use of the factory method
        private WarehouseItem() { }

        public static WarehouseItem Create(string productId, string locationCode, int initialQuantity)
        {
            if (string.IsNullOrWhiteSpace(productId))
                throw new ArgumentException("Product ID is required.");
            
            if (string.IsNullOrWhiteSpace(locationCode))
                throw new ArgumentException("Location code is required.");
                
            if (initialQuantity < 0)
                throw new ArgumentException("Initial quantity cannot be negative."); // Enforcing standard inventory rules

            return new WarehouseItem
            {
                Id = Guid.NewGuid().ToString(),
                ProductId = productId,
                LocationCode = locationCode,
                Quantity = initialQuantity,
                LastUpdatedAt = DateTime.UtcNow
            };
        }

        public void AdjustQuantity(int amount)
        {
            // amount can be positive (adding stock) or negative (removing stock)
            if (Quantity + amount < 0)
                throw new InvalidOperationException("Insufficient stock in this location. Quantity cannot drop below zero.");

            Quantity += amount;
            LastUpdatedAt = DateTime.UtcNow;
        }
    }
}