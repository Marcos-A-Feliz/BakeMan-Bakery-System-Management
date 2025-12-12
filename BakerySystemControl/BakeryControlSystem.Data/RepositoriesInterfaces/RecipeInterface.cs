using BakeryControlSystem.Domain;

namespace BakeryControlSystem.Data.RepositoriesInterfaces
{
    public interface IRecipeRepository
    {
        Recipe GetById(int id);
        Recipe GetByProductId(int productId);
        IEnumerable<Recipe> GetAll();
        void Add(Recipe recipe);
        void Update(Recipe recipe);
        void AddDetail(int recipeId, RecipeDetail detail);
        void RemoveDetail(int recipeId, int ingredientId);
        void Delete(int id);
        decimal CalculateCost(int recipeId);
        bool CanProduce(int recipeId, int quantity);
        IEnumerable<RecipeDetail> GetRecipeDetails(int recipeId);

        void AddIngredientToRecipe(int recipeId, int ingredientId, decimal quantity);
        void UpdateRecipeDetail(int recipeDetailId, decimal newQuantity);
        void RemoveIngredientFromRecipe(int recipeDetailId);
        RecipeDetail GetRecipeDetailById(int recipeDetailId);
        IEnumerable<RecipeDetail> GetAllRecipeDetails();
    }
}
