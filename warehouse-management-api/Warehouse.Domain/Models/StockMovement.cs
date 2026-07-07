namespace Warehouse.DomainWarehouse.Domain.Models
{
    public enum MovementType
    {
        Inbound,
        Outbound,
        Adjustment
    }

    public class StockMovement
    {
        public string? Id { get; private set; }
        public string? ProductId { get; private set; }
        public MovementType Type { get; private set; }
        public int Quantity { get; private set; }
        public string? Reason { get; private set; }
        public DateTime MovementDate { get; private set; }

        private StockMovement() { }

        public static StockMovement Create(string productId, MovementType type, int quantity, string reason)
        {
            if (string.IsNullOrWhiteSpace(productId))
                throw new ArgumentException("Product ID is required.");
                
            if (quantity <= 0)
                throw new ArgumentException("Movement quantity must be greater than zero.");
                
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("A reason for the stock movement is required.");

            return new StockMovement
            {
                Id = Guid.NewGuid().ToString(),
                ProductId = productId,
                Type = type,
                Quantity = quantity, // Absolute value of the movement
                Reason = reason,
                MovementDate = DateTime.UtcNow
            };
        }
    }
}