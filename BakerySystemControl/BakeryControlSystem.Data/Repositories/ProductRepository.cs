using BakeryControlSystem.Data.RepositoriesInterfaces;
using BakeryControlSystem.Domain;
using BakeryControlSystem.Data.DB;
using Microsoft.EntityFrameworkCore;

namespace BakeryControlSystem.Data.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly BakeryDbContext _context;

        public ProductRepository(BakeryDbContext context)
        {
            _context = context;
        }

        public Product GetById(int id)
        {
            try
            {
                return _context.Products
                    .Include(p => p.Recipes)
                        .ThenInclude(r => r.Details)
                            .ThenInclude(d => d.Ingredient)
                    .FirstOrDefault(p => p.Id == id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving product with ID {id}", ex);
            }
        }

        public IEnumerable<Product> GetAll()
        {
            try
            {
                return _context.Products
                    .Include(p => p.Recipes)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving all products", ex);
            }
        }

        public IEnumerable<Product> GetActiveProducts()
        {
            try
            {
                return _context.Products
                    .Where(p => p.IsActive)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving active products", ex);
            }
        }

        public IEnumerable<Product> GetByCategory(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
                throw new ArgumentException("Category cannot be empty", nameof(category));

            try
            {
                return _context.Products
                    .Where(p => p.Category == category)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving products by category '{category}'", ex);
            }
        }

        public void Add(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            try
            {
                if (string.IsNullOrWhiteSpace(product.Name))
                    throw new ArgumentException("Product name cannot be empty", nameof(product.Name));

                if (product.CreationDate == default)
                    product.CreationDate = DateTime.Now;

                _context.Products.Add(product);
                _context.SaveChanges();
            }
            catch (DbUpdateException dbEx)
            {
                throw new Exception($"Error saving product '{product.Name}' to database. Possible duplicate name.", dbEx);
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding product", ex);
            }
        }

        public void Update(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            try
            {
                var existing = _context.Products.Find(product.Id);
                if (existing == null)
                    throw new KeyNotFoundException($"Product with ID {product.Id} not found");

                _context.Entry(existing).CurrentValues.SetValues(product);
                _context.SaveChanges();
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (DbUpdateException dbEx)
            {
                throw new Exception($"Error updating product with ID {product.Id} in database", dbEx);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating product with ID {product.Id}", ex);
            }
        }

        public void Delete(int id)
        {
            try
            {
                var product = _context.Products.Find(id);
                if (product == null)
                    throw new KeyNotFoundException($"Product with ID {id} not found");

                product.IsActive = false;
                _context.SaveChanges();
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deactivating product with ID {id}", ex);
            }
        }

        public decimal CalculateProductionCost(int productId)
        {
            try
            {
                var product = GetById(productId);
                if (product?.Recipes == null || !product.Recipes.Any())
                    return 0;

                var recipe = product.Recipes.FirstOrDefault();
                if (recipe?.Details == null)
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
                throw new Exception($"Error calculating production cost for product ID {productId}", ex);
            }
        }

        public int GetTotalSold(int productId, DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
                throw new ArgumentException("Start date cannot be after end date");

            try
            {
                return _context.Sales
                    .Where(s => s.ProductId == productId &&
                               s.SaleDate >= startDate &&
                               s.SaleDate <= endDate)
                    .Sum(s => s.Quantity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error calculating total sold for product ID {productId}", ex);
            }
        }

        public IEnumerable<Recipe> GetProductRecipes(int productId)
        {
            try
            {
                return _context.Recipes
                    .Include(r => r.Details)
                        .ThenInclude(d => d.Ingredient)
                    .Where(r => r.ProductId == productId)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving recipes for product ID {productId}", ex);
            }
        }

        public Recipe GetPrimaryRecipe(int productId)
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
                throw new Exception($"Error retrieving primary recipe for product ID {productId}", ex);
            }
        }

        public void AddRecipeToProduct(int productId, Recipe recipe)
        {
            if (recipe == null)
                throw new ArgumentNullException(nameof(recipe));

            try
            {
                var product = _context.Products.Find(productId);
                if (product == null)
                    throw new KeyNotFoundException($"Product with ID {productId} not found");

                recipe.ProductId = productId;

                if (recipe.LastUpdated == default)
                    recipe.LastUpdated = DateTime.Now;

                _context.Recipes.Add(recipe);
                _context.SaveChanges();
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (DbUpdateException dbEx)
            {
                throw new Exception($"Error adding recipe to product ID {productId}", dbEx);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error adding recipe to product ID {productId}", ex);
            }
        }

        public void UpdateWithRecipes(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            using var transaction = _context.Database.BeginTransaction();

            try
            {
                var existing = _context.Products
                    .Include(p => p.Recipes)
                    .FirstOrDefault(p => p.Id == product.Id);

                if (existing == null)
                    throw new KeyNotFoundException($"Product with ID {product.Id} not found");

                _context.Entry(existing).CurrentValues.SetValues(product);

                foreach (var recipe in product.Recipes)
                {
                    var existingRecipe = existing.Recipes.FirstOrDefault(r => r.Id == recipe.Id);

                    if (existingRecipe != null)
                    {
                        _context.Entry(existingRecipe).CurrentValues.SetValues(recipe);
                    }
                    else
                    {
                        recipe.ProductId = product.Id;
                        existing.Recipes.Add(recipe);
                    }
                }

                var recipesToRemove = existing.Recipes
                    .Where(r => !product.Recipes.Any(pr => pr.Id == r.Id))
                    .ToList();

                foreach (var recipe in recipesToRemove)
                {
                    existing.Recipes.Remove(recipe);
                }

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
                throw new Exception($"Error updating product with recipes for ID {product.Id}", ex);
            }
        }
    }
}