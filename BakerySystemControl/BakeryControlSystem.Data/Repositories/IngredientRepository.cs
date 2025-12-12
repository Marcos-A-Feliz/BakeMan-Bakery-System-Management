using BakeryControlSystem.Data.RepositoriesInterfaces;
using BakeryControlSystem.Domain;
using BakeryControlSystem.Data.DB;
using Microsoft.EntityFrameworkCore;

namespace BakeryControlSystem.Data.Repositories
{
    public class IngredientRepository : IIngredientRepository
    {
        private readonly BakeryDbContext _context;

        public IngredientRepository(BakeryDbContext context)
        {
            _context = context;
        }

        public Ingredient GetById(int id)
        {
            try
            {
                return _context.Ingredients.Find(id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving ingredient with ID {id}", ex);
            }
        }

        public IEnumerable<Ingredient> GetAll()
        {
            try
            {
                return _context.Ingredients.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving all ingredients", ex);
            }
        }

        public void Add(Ingredient ingredient)
        {
            if (ingredient == null)
                throw new ArgumentNullException(nameof(ingredient));

            try
            {
                if (ingredient.LastRestockDate == default)
                    ingredient.LastRestockDate = DateTime.Now;

                if (ingredient.ExpirationDate == default)
                    ingredient.ExpirationDate = DateTime.Now.AddDays(30);

                _context.Ingredients.Add(ingredient);
                _context.SaveChanges();
            }
            catch (DbUpdateException dbEx)
            {
                throw new Exception("Error saving ingredient to database. Possible duplicate name.", dbEx);
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding ingredient", ex);
            }
        }

        public void Update(Ingredient ingredient)
        {
            if (ingredient == null)
                throw new ArgumentNullException(nameof(ingredient));

            try
            {
                var existing = _context.Ingredients.Find(ingredient.Id);
                if (existing == null)
                    throw new KeyNotFoundException($"Ingredient with ID {ingredient.Id} not found");

                _context.Entry(existing).CurrentValues.SetValues(ingredient);
                _context.SaveChanges();
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (DbUpdateException dbEx)
            {
                throw new Exception("Error updating ingredient in database", dbEx);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating ingredient with ID {ingredient.Id}", ex);
            }
        }

        public void Delete(int id)
        {
            try
            {
                var ingredient = _context.Ingredients.Find(id);
                if (ingredient == null)
                    throw new KeyNotFoundException($"Ingredient with ID {id} not found");

                _context.Ingredients.Remove(ingredient);
                _context.SaveChanges();
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (DbUpdateException dbEx)
            {
                throw new Exception($"Cannot delete ingredient with ID {id}. It may be referenced by recipes or production records.", dbEx);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting ingredient with ID {id}", ex);
            }
        }

        public void UpdateStock(int ingredientId, decimal quantityChange)
        {
            if (quantityChange == 0)
                return;

            try
            {
                var ingredient = _context.Ingredients.Find(ingredientId);
                if (ingredient == null)
                    throw new KeyNotFoundException($"Ingredient with ID {ingredientId} not found");

                var newStock = ingredient.CurrentStock + quantityChange;

                if (newStock < 0)
                    throw new InvalidOperationException(
                        $"Cannot reduce stock of {ingredient.Name} below 0. " +
                        $"Current: {ingredient.CurrentStock}, Change: {quantityChange}");

                ingredient.CurrentStock = newStock;
                ingredient.LastRestockDate = DateTime.Now;

                _context.SaveChanges();
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating stock for ingredient ID {ingredientId}", ex);
            }
        }

        public IEnumerable<Ingredient> GetLowStock()
        {
            try
            {
                return _context.Ingredients
                    .Where(i => i.CurrentStock <= i.MinimumStock)
                    .OrderBy(i => i.CurrentStock / i.MinimumStock)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving low stock ingredients", ex);
            }
        }

        public IEnumerable<Ingredient> GetExpiringSoon(int daysThreshold)
        {
            if (daysThreshold < 0)
                throw new ArgumentException("Days threshold cannot be negative", nameof(daysThreshold));

            try
            {
                var thresholdDate = DateTime.Now.AddDays(daysThreshold);
                return _context.Ingredients
                    .Where(i => i.ExpirationDate <= thresholdDate && i.ExpirationDate >= DateTime.Now)
                    .OrderBy(i => i.ExpirationDate)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving ingredients expiring within {daysThreshold} days", ex);
            }
        }

        public decimal GetTotalInventoryValue()
        {
            try
            {
                return _context.Ingredients
                    .Sum(i => i.CurrentStock * i.UnitPrice);
            }
            catch (Exception ex)
            {
                throw new Exception("Error calculating total inventory value", ex);
            }
        }

        public Dictionary<string, decimal> GetStockAlertReport()
        {
            try
            {
                var report = new Dictionary<string, decimal>();
                var lowStockItems = GetLowStock();

                foreach (var item in lowStockItems)
                {
                    var percentage = item.MinimumStock > 0
                        ? (item.CurrentStock / item.MinimumStock) * 100
                        : 0;
                    report.Add(item.Name, percentage);
                }

                return report;
            }
            catch (Exception ex)
            {
                throw new Exception("message", ex);
            }
        }
    }
}