using BakeryControlSystem.Data.RepositoriesInterfaces;
using BakeryControlSystem.Domain;
using BakeryControlSystem.Data.DB;
using Microsoft.EntityFrameworkCore;

namespace BakeryControlSystem.Data.Repositories
{
    public class RecipeRepository : IRecipeRepository
    {
        private readonly BakeryDbContext _context;

        public RecipeRepository(BakeryDbContext context)
        {
            _context = context;
        }

        public Recipe GetById(int id)
        {
            try
            {
                return _context.Recipes
                    .Include(r => r.Details)
                        .ThenInclude(d => d.Ingredient)
                    .FirstOrDefault(r => r.Id == id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving recipe with ID {id}", ex);
            }
        }

        public Recipe GetByProductId(int productId)
        {
            try
            {
                return _context.Recipes
                    .Include(r => r.Details)
                        .ThenInclude(d => d.Ingredient)
                    .FirstOrDefault(r => r.ProductId == productId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving recipe for product ID {productId}", ex);
            }
        }

        public IEnumerable<Recipe> GetAll()
        {
            try
            {
                return _context.Recipes
                    .Include(r => r.Details)
                        .ThenInclude(d => d.Ingredient)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving all recipes", ex);
            }
        }

        public void Add(Recipe recipe)
        {
            if (recipe == null)
                throw new ArgumentNullException(nameof(recipe));

            try
            {
                if (string.IsNullOrWhiteSpace(recipe.Name))
                    throw new ArgumentException("Recipe name cannot be empty", nameof(recipe.Name));

                if (recipe.LastUpdated == default)
                    recipe.LastUpdated = DateTime.Now;

                if (recipe.Yield <= 0)
                    recipe.Yield = 1;

                _context.Recipes.Add(recipe);
                _context.SaveChanges();
            }
            catch (DbUpdateException dbEx)
            {
                throw new Exception("Error saving recipe to database", dbEx);
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding recipe", ex);
            }
        }

        public void Update(Recipe recipe)
        {
            if (recipe == null)
                throw new ArgumentNullException(nameof(recipe));

            try
            {
                var existing = _context.Recipes
                    .Include(r => r.Details)
                    .FirstOrDefault(r => r.Id == recipe.Id);

                if (existing == null)
                    throw new KeyNotFoundException($"Recipe with ID {recipe.Id} not found");

                recipe.LastUpdated = DateTime.Now;
                _context.Entry(existing).CurrentValues.SetValues(recipe);

                UpdateRecipeDetails(existing, recipe.Details);
                _context.SaveChanges();
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating recipe with ID {recipe.Id}", ex);
            }
        }

        public void AddDetail(int recipeId, RecipeDetail detail)
        {
            if (detail == null)
                throw new ArgumentNullException(nameof(detail));

            try
            {
                var recipe = _context.Recipes.Find(recipeId);
                if (recipe == null)
                    throw new KeyNotFoundException($"Recipe with ID {recipeId} not found");

                var ingredient = _context.Ingredients.Find(detail.IngredientId);
                if (ingredient == null)
                    throw new KeyNotFoundException($"Ingredient with ID {detail.IngredientId} not found");

                if (detail.Quantity <= 0)
                    throw new ArgumentException("Quantity must be greater than 0", nameof(detail.Quantity));

                detail.RecipeId = recipeId;
                _context.RecipeDetails.Add(detail);
                _context.SaveChanges();
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error adding detail to recipe ID {recipeId}", ex);
            }
        }

        public void RemoveDetail(int recipeId, int ingredientId)
        {
            try
            {
                var detail = _context.RecipeDetails
                    .FirstOrDefault(d => d.RecipeId == recipeId && d.IngredientId == ingredientId);

                if (detail == null)
                    throw new KeyNotFoundException($"Recipe detail not found for recipe ID {recipeId} and ingredient ID {ingredientId}");

                _context.RecipeDetails.Remove(detail);
                _context.SaveChanges();
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error removing detail from recipe ID {recipeId}", ex);
            }
        }

        public void Delete(int id)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                var recipe = _context.Recipes
                    .Include(r => r.Details)
                    .FirstOrDefault(r => r.Id == id);

                if (recipe == null)
                    throw new KeyNotFoundException($"Recipe with ID {id} not found");

                if (recipe.Details.Any())
                {
                    _context.RecipeDetails.RemoveRange(recipe.Details);
                }

                _context.Recipes.Remove(recipe);
                _context.SaveChanges();
                transaction.Commit();
            }
            catch (KeyNotFoundException)
            {
                transaction.Rollback();
                throw;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new Exception($"Error deleting recipe with ID {id}", ex);
            }
        }

        public decimal CalculateCost(int recipeId)
        {
            try
            {
                var recipe = GetById(recipeId);
                if (recipe == null || recipe.Details == null)
                    return 0;

                decimal totalCost = 0;
                foreach (var detail in recipe.Details)
                {
                    if (detail.Ingredient != null)
                    {
                        totalCost += detail.Quantity * detail.Ingredient.UnitPrice;
                    }
                }
                return totalCost;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error calculating cost for recipe ID {recipeId}", ex);
            }
        }

        public bool CanProduce(int recipeId, int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be greater than 0", nameof(quantity));

            try
            {
                var recipe = GetById(recipeId);
                if (recipe == null) return false;

                foreach (var detail in recipe.Details)
                {
                    if (detail.Ingredient == null ||
                        detail.Ingredient.CurrentStock < (detail.Quantity * quantity))
                    {
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error checking production feasibility for recipe ID {recipeId}", ex);
            }
        }

        public IEnumerable<RecipeDetail> GetRecipeDetails(int recipeId)
        {
            try
            {
                return _context.RecipeDetails
                    .Include(rd => rd.Ingredient)
                    .Where(rd => rd.RecipeId == recipeId)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving details for recipe ID {recipeId}", ex);
            }
        }

        public void AddIngredientToRecipe(int recipeId, int ingredientId, decimal quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be greater than 0", nameof(quantity));

            try
            {
                var recipe = _context.Recipes.Find(recipeId);
                if (recipe == null)
                    throw new KeyNotFoundException($"Recipe with ID {recipeId} not found");

                var ingredient = _context.Ingredients.Find(ingredientId);
                if (ingredient == null)
                    throw new KeyNotFoundException($"Ingredient with ID {ingredientId} not found");

                var existingDetail = _context.RecipeDetails
                    .FirstOrDefault(rd => rd.RecipeId == recipeId && rd.IngredientId == ingredientId);

                if (existingDetail != null)
                {
                    existingDetail.Quantity += quantity;
                    _context.Entry(existingDetail).State = EntityState.Modified;
                }
                else
                {
                    var recipeDetail = new RecipeDetail
                    {
                        RecipeId = recipeId,
                        IngredientId = ingredientId,
                        Quantity = quantity
                    };
                    _context.RecipeDetails.Add(recipeDetail);
                }

                _context.SaveChanges();
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error adding ingredient {ingredientId} to recipe {recipeId}", ex);
            }
        }

        public void UpdateRecipeDetail(int recipeDetailId, decimal newQuantity)
        {
            if (newQuantity <= 0)
                throw new ArgumentException("Quantity must be greater than 0", nameof(newQuantity));

            try
            {
                var recipeDetail = _context.RecipeDetails.Find(recipeDetailId);
                if (recipeDetail == null)
                    throw new KeyNotFoundException($"Recipe detail with ID {recipeDetailId} not found");

                recipeDetail.Quantity = newQuantity;
                _context.SaveChanges();
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating recipe detail with ID {recipeDetailId}", ex);
            }
        }

        public void RemoveIngredientFromRecipe(int recipeDetailId)
        {
            try
            {
                var recipeDetail = _context.RecipeDetails.Find(recipeDetailId);
                if (recipeDetail == null)
                    throw new KeyNotFoundException($"Recipe detail with ID {recipeDetailId} not found");

                _context.RecipeDetails.Remove(recipeDetail);
                _context.SaveChanges();
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error removing recipe detail with ID {recipeDetailId}", ex);
            }
        }

        public RecipeDetail GetRecipeDetailById(int recipeDetailId)
        {
            try
            {
                return _context.RecipeDetails
                    .Include(rd => rd.Ingredient)
                    .Include(rd => rd.Recipe)
                    .FirstOrDefault(rd => rd.Id == recipeDetailId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving recipe detail with ID {recipeDetailId}", ex);
            }
        }

        public IEnumerable<RecipeDetail> GetAllRecipeDetails()
        {
            try
            {
                return _context.RecipeDetails
                    .Include(rd => rd.Ingredient)
                    .Include(rd => rd.Recipe)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving all recipe details", ex);
            }
        }

        private void UpdateRecipeDetails(Recipe existingRecipe, List<RecipeDetail> newDetails)
        {
            try
            {
                var detailsToRemove = existingRecipe.Details
                    .Where(ed => !newDetails.Any(nd => nd.Id == ed.Id && nd.Id != 0))
                    .ToList();

                foreach (var detail in detailsToRemove)
                {
                    _context.RecipeDetails.Remove(detail);
                }

                foreach (var newDetail in newDetails)
                {
                    var existingDetail = existingRecipe.Details
                        .FirstOrDefault(ed => ed.Id == newDetail.Id && ed.Id != 0);

                    if (existingDetail != null)
                    {
                        _context.Entry(existingDetail).CurrentValues.SetValues(newDetail);
                    }
                    else
                    {
                        newDetail.RecipeId = existingRecipe.Id;
                        existingRecipe.Details.Add(newDetail);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating recipe details for recipe {existingRecipe.Id}", ex);
            }
        }
    }
}