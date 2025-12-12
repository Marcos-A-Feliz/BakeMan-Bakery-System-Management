namespace BakeryControlSystem.Domain
{
    public class Ingredient
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public MeasurementUnit Unit { get; set; } 
        public decimal CurrentStock { get; set; }
        public decimal MinimumStock { get; set; }
        public decimal MaximumStock { get; set; }
        public decimal UnitPrice { get; set; }  
        public string Supplier { get; set; }
        public DateTime LastRestockDate { get; set; }
        public DateTime ExpirationDate { get; set; }  

        public Ingredient()
        {
            Name = string.Empty;
            Description = string.Empty;
            Supplier = string.Empty;
            LastRestockDate = DateTime.Now;
            ExpirationDate = DateTime.Now.AddDays(30);
        }

        public bool NeedsRestock()
        {
            return CurrentStock <= MinimumStock;
        }

        public decimal InventoryValue()
        {
            return CurrentStock * UnitPrice;
        }
    }

    public enum MeasurementUnit
    {
        Kilogram,
        Gram,
        Liter,
        Milliliter,
        Unit,
        Package,
        Dozen
    }
}