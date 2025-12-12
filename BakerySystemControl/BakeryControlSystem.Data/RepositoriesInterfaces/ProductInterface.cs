using BakeryControlSystem.Domain;

namespace BakeryControlSystem.Data.RepositoriesInterfaces
{
    public interface IProductRepository
    {
        Product GetById(int id);
        IEnumerable<Product> GetAll();
        IEnumerable<Product> GetActiveProducts();
        IEnumerable<Product> GetByCategory(string category);
        void Add(Product product);
        void Update(Product product);
        void Delete(int id);
        decimal CalculateProductionCost(int productId);
        int GetTotalSold(int productId, DateTime startDate, DateTime endDate);
        IEnumerable<Recipe> GetProductRecipes(int productId);
        Recipe GetPrimaryRecipe(int productId);
        void AddRecipeToProduct(int productId, Recipe recipe);
        void UpdateWithRecipes(Product product);
    }
}