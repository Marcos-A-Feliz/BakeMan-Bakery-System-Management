namespace BakeryControlSystem.Domain
{
    public class DailyProduction
    {
        public int Id { get; set; }
        public DateTime ProductionDate { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public int PlannedQuantity { get; set; }
        public int ActualQuantity { get; set; }
        public int WasteQuantity { get; set; }  
        public string ProductionStatus { get; set; }
        public string Notes { get; set; }
        public List<ProductionDetail> UsedIngredients { get; set; }

        public DailyProduction()
        {
            ProductionDate = DateTime.Today;
            ProductionStatus = "Planned";
            Notes = string.Empty;
            UsedIngredients = new List<ProductionDetail>();
        }


        public decimal EfficiencyPercentage()
        {
            if (PlannedQuantity == 0) return 0;
            return ((decimal)ActualQuantity / PlannedQuantity) * 100;
        }

        public decimal WastePercentage()
        {
            if (PlannedQuantity == 0) return 0;
            return (WasteQuantity / (decimal)PlannedQuantity) * 100;
        }
    }

    public class ProductionDetail
    {
        public int Id { get; set; }
        public int DailyProductionId { get; set; }
        public int IngredientId { get; set; }
        public DailyProduction DailyProduction { get; set; }
        public Ingredient Ingredient { get; set; }
        public decimal PlannedQuantity { get; set; }
        public decimal ActualQuantity { get; set; }
        public decimal Variance { get; set; }  
    }
}
