using BakeryControlSystem.Domain;

namespace BakeryControlSystem.Data.RepositoriesInterfaces
{
    public interface IIngredientRepository
    {
        Ingredient GetById(int id);
        IEnumerable<Ingredient> GetAll();
        IEnumerable<Ingredient> GetLowStock();  
        IEnumerable<Ingredient> GetExpiringSoon(int daysThreshold);
        void Add(Ingredient ingredient);
        void Update(Ingredient ingredient);
        void UpdateStock(int ingredientId, decimal quantityChange);
        void Delete(int id);
        decimal GetTotalInventoryValue();
        Dictionary<string, decimal> GetStockAlertReport();
    }
}
