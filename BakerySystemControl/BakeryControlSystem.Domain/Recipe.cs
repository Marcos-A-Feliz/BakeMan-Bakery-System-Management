namespace BakeryControlSystem.Domain
{
    public class Recipe
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? ProductId { get; set; }
        public virtual Product Product { get; set; }
        public List<RecipeDetail> Details { get; set; }
        public string Instructions { get; set; }
        public int PreparationTime { get; set; }
        public int BakingTime { get; set; }
        public int Yield { get; set; }
        public decimal PreparationCost { get; set; }
        public decimal TotalCost { get; set; }
        public DateTime LastUpdated { get; set; }

        public Recipe()
        {
            Details = new List<RecipeDetail>();
            Name = string.Empty;
            Instructions = string.Empty;
            LastUpdated = DateTime.Now;
            Yield = 1;
        }

        public void CalculateTotalCost()
        {
            TotalCost = 0;
            foreach (var detail in Details)
            {
                TotalCost += detail.IngredientCost();
            }

            if (Yield > 0)
            {
                TotalCost = TotalCost / Yield;
            }
        }
    }
}
namespace BakeryControlSystem.Domain
{
    public class RecipeDetail
    {
        public int Id { get; set; }
        public int RecipeId { get; set; }
        public int IngredientId { get; set; }
        public Ingredient Ingredient { get; set; }
        public Recipe Recipe { get; set; }
        public decimal Quantity { get; set; }

        public decimal IngredientCost()
        {
            return Quantity * (Ingredient?.UnitPrice ?? 0);
        }
    }
}