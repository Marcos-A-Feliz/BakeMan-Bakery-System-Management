namespace BakeryControlSystem.Domain
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public decimal SalePrice { get; set; }
        public decimal ProductionCost { get; set; }
        public decimal ProfitMargin { get; set; }
        public DateTime CreationDate { get; set; }
        public bool IsActive { get; set; }
        public virtual ICollection<Recipe> Recipes { get; set; }
        public virtual ICollection<Sale> Sales { get; set; }
        public virtual ICollection<DailyProduction> DailyProductions { get; set; }

        public Product()
        {
            Name = string.Empty;
            Description = string.Empty;
            Category = string.Empty;
            CreationDate = DateTime.Now;
            IsActive = true;
            Recipes = new List<Recipe>(); 
            Sales = new List<Sale>();           
            DailyProductions = new List<DailyProduction>(); 
        }

        public void CalculateProductionCost()
        {
            
        }
    }
}