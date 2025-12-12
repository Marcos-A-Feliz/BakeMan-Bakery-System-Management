using BakeryControlSystem.Data.DB;
using BakeryControlSystem.Data.RepositoriesInterfaces;
using BakeryControlSystem.Domain;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BakeryControlSystem.Data.Repositories
{
    public class DailyProductionRepository : IDailyProductionRepository
    {
        private readonly BakeryDbContext _context;
        private readonly ILogger<DailyProductionRepository> _logger;

        public DailyProductionRepository(BakeryDbContext context, ILogger<DailyProductionRepository> logger = null)
        {
            _context = context;
            _logger = logger;
        }

        public DailyProduction GetById(int id)
        {
            try
            {
                return _context.DailyProductions
                    .Include(dp => dp.Product)
                    .Include(dp => dp.UsedIngredients)
                        .ThenInclude(pd => pd.Ingredient)
                    .FirstOrDefault(dp => dp.Id == id);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting DailyProduction by ID: {Id}", id);
                throw new Exception($"Error retrieving production with ID {id}", ex);
            }
        }

        public DailyProduction GetByDateAndProduct(DateTime date, int productId)
        {
            try
            {
                return _context.DailyProductions
                    .Include(dp => dp.Product)
                    .Include(dp => dp.UsedIngredients)
                        .ThenInclude(pd => pd.Ingredient)
                    .FirstOrDefault(dp => dp.ProductionDate.Date == date.Date && dp.ProductId == productId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting DailyProduction by date {Date} and product {ProductId}", date, productId);
                throw new Exception($"Error retrieving production for date {date:yyyy-MM-dd} and product {productId}", ex);
            }
        }

        public IEnumerable<DailyProduction> GetAll()
        {
            try
            {
                return _context.DailyProductions
                    .Include(dp => dp.Product)
                    .OrderByDescending(dp => dp.ProductionDate)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting all DailyProductions");
                throw new Exception("Error retrieving all productions", ex);
            }
        }

        public IEnumerable<DailyProduction> GetByDate(DateTime date)
        {
            try
            {
                return _context.DailyProductions
                    .Include(dp => dp.Product)
                    .Where(dp => dp.ProductionDate.Date == date.Date)
                    .OrderBy(dp => dp.Product.Name)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting DailyProductions by date {Date}", date);
                throw new Exception($"Error retrieving productions for date {date:yyyy-MM-dd}", ex);
            }
        }

        public IEnumerable<DailyProduction> GetByDateRange(DateTime startDate, DateTime endDate)
        {
            try
            {
                return _context.DailyProductions
                    .Include(dp => dp.Product)
                    .Where(dp => dp.ProductionDate.Date >= startDate.Date &&
                                dp.ProductionDate.Date <= endDate.Date)
                    .OrderByDescending(dp => dp.ProductionDate)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting DailyProductions by date range {StartDate} to {EndDate}", startDate, endDate);
                throw new Exception($"Error retrieving productions from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}", ex);
            }
        }

        public void Add(DailyProduction production)
        {
            if (production == null)
                throw new ArgumentNullException(nameof(production));

            try
            {
                if (production.ProductionDate == default)
                    production.ProductionDate = DateTime.Today;

                if (string.IsNullOrEmpty(production.ProductionStatus))
                    production.ProductionStatus = "Planned";

                _context.DailyProductions.Add(production);
                _context.SaveChanges();
            }
            catch (DbUpdateException dbEx)
            {
                _logger?.LogError(dbEx, "Database error adding DailyProduction");
                throw new Exception("Error saving production to database. Check data integrity.", dbEx);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error adding DailyProduction");
                throw new Exception("Error adding production", ex);
            }
        }

        public void Update(DailyProduction production)
        {
            if (production == null)
                throw new ArgumentNullException(nameof(production));

            using var transaction = _context.Database.BeginTransaction();

            try
            {
                var existing = _context.DailyProductions
                    .Include(dp => dp.UsedIngredients)
                    .FirstOrDefault(dp => dp.Id == production.Id);

                if (existing == null)
                    throw new KeyNotFoundException($"Production with ID {production.Id} not found");

                _context.Entry(existing).CurrentValues.SetValues(production);
                UpdateUsedIngredients(existing, production.UsedIngredients);

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
                _logger?.LogError(ex, "Error updating DailyProduction with ID {Id}", production.Id);
                throw new Exception($"Error updating production with ID {production.Id}", ex);
            }
        }

        public void Delete(int id)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                var production = _context.DailyProductions
                    .Include(dp => dp.UsedIngredients)
                    .FirstOrDefault(dp => dp.Id == id);

                if (production == null)
                    throw new KeyNotFoundException($"Production with ID {id} not found");

                _context.ProductionDetails.RemoveRange(production.UsedIngredients);
                _context.DailyProductions.Remove(production);

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
                _logger?.LogError(ex, "Error deleting DailyProduction with ID {Id}", id);
                throw new Exception($"Error deleting production with ID {id}", ex);
            }
        }

        public decimal GetProductionEfficiency(DateTime date)
        {
            try
            {
                var productions = GetByDate(date);

                if (!productions.Any())
                    return 0;

                decimal totalEfficiency = 0;
                int count = 0;

                foreach (var prod in productions)
                {
                    var efficiency = prod.EfficiencyPercentage();
                    if (efficiency > 0)
                    {
                        totalEfficiency += efficiency;
                        count++;
                    }
                }

                return count > 0 ? totalEfficiency / count : 0;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error calculating production efficiency for date {Date}", date);
                return 0;
            }
        }

        public Dictionary<int, int> GetWasteReport(DateTime startDate, DateTime endDate)
        {
            try
            {
                return _context.DailyProductions
                    .Include(dp => dp.Product)
                    .Where(dp => dp.ProductionDate.Date >= startDate.Date &&
                                dp.ProductionDate.Date <= endDate.Date)
                    .GroupBy(dp => dp.ProductId)
                    .Select(g => new
                    {
                        ProductId = g.Key,
                        TotalWaste = g.Sum(dp => dp.WasteQuantity)
                    })
                    .ToDictionary(x => x.ProductId, x => x.TotalWaste);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating waste report from {StartDate} to {EndDate}", startDate, endDate);
                return new Dictionary<int, int>();
            }
        }

        public bool RegisterProduction(DailyProduction production, bool updateInventory)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                production.ProductionStatus = "Completed";

                foreach (var detail in production.UsedIngredients)
                {
                    detail.Variance = detail.ActualQuantity - detail.PlannedQuantity;
                }

                if (production.Id == 0)
                    Add(production);
                else
                    Update(production);

                if (updateInventory)
                {
                    UpdateInventoryAfterProduction(production);
                }

                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _logger?.LogError(ex, "Error registering production");
                throw new Exception("Error registering production", ex);
            }
        }

        public IEnumerable<DailyProduction> GetByProduct(int productId)
        {
            try
            {
                return _context.DailyProductions
                    .Include(dp => dp.Product)
                    .Where(dp => dp.ProductId == productId)
                    .OrderByDescending(dp => dp.ProductionDate)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting DailyProductions for product {ProductId}", productId);
                throw new Exception($"Error retrieving productions for product {productId}", ex);
            }
        }

        public IEnumerable<DailyProduction> GetByDateRangeAndProduct(DateTime startDate, DateTime endDate, int productId)
        {
            try
            {
                return _context.DailyProductions
                    .Include(dp => dp.Product)
                    .Where(dp => dp.ProductionDate.Date >= startDate.Date &&
                                dp.ProductionDate.Date <= endDate.Date &&
                                dp.ProductId == productId)
                    .OrderByDescending(dp => dp.ProductionDate)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting DailyProductions for product {ProductId} in range {StartDate} to {EndDate}",
                    productId, startDate, endDate);
                throw new Exception($"Error retrieving productions for product {productId} in date range", ex);
            }
        }

        public decimal GetTotalProductionByDate(DateTime date)
        {
            try
            {
                return _context.DailyProductions
                    .Where(dp => dp.ProductionDate.Date == date.Date)
                    .Sum(dp => dp.ActualQuantity);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error calculating total production for date {Date}", date);
                return 0;
            }
        }

        public int GetTotalUnitsProducedByProduct(int productId, DateTime startDate, DateTime endDate)
        {
            try
            {
                return _context.DailyProductions
                    .Where(dp => dp.ProductId == productId &&
                                dp.ProductionDate.Date >= startDate.Date &&
                                dp.ProductionDate.Date <= endDate.Date)
                    .Sum(dp => dp.ActualQuantity);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error calculating total units for product {ProductId} in range {StartDate} to {EndDate}",
                    productId, startDate, endDate);
                return 0;
            }
        }

        public bool CheckIngredientsAvailability(int productId, int quantity)
        {
            try
            {
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error checking ingredients availability for product {ProductId}, quantity {Quantity}",
                    productId, quantity);
                return false;
            }
        }

        public Dictionary<int, decimal> GetProductionCostReport(DateTime startDate, DateTime endDate)
        {
            try
            {
                return new Dictionary<int, decimal>();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating production cost report from {StartDate} to {EndDate}",
                    startDate, endDate);
                return new Dictionary<int, decimal>();
            }
        }

        private void UpdateUsedIngredients(DailyProduction existing, List<ProductionDetail> newDetails)
        {
            try
            {
                var toRemove = existing.UsedIngredients
                    .Where(ed => !newDetails.Any(nd => nd.Id == ed.Id && nd.Id != 0))
                    .ToList();

                foreach (var detail in toRemove)
                {
                    _context.ProductionDetails.Remove(detail);
                }

                foreach (var newDetail in newDetails)
                {
                    var existingDetail = existing.UsedIngredients
                        .FirstOrDefault(ed => ed.Id == newDetail.Id && ed.Id != 0);

                    if (existingDetail != null)
                    {
                        _context.Entry(existingDetail).CurrentValues.SetValues(newDetail);
                    }
                    else
                    {
                        newDetail.DailyProductionId = existing.Id;
                        existing.UsedIngredients.Add(newDetail);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error updating used ingredients for production {ProductionId}", existing.Id);
                throw;
            }
        }

        private void UpdateInventoryAfterProduction(DailyProduction production)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                foreach (var detail in production.UsedIngredients)
                {
                    var ingredient = _context.Ingredients.Find(detail.IngredientId);
                    if (ingredient != null)
                    {
                        ingredient.CurrentStock -= detail.ActualQuantity;

                        if (ingredient.CurrentStock < 0)
                        {
                            throw new InvalidOperationException(
                                $"Insufficient stock for ingredient {ingredient.Name}. " +
                                $"Required: {detail.ActualQuantity}, Available: {ingredient.CurrentStock + detail.ActualQuantity}");
                        }

                        _context.Entry(ingredient).State = EntityState.Modified;
                    }
                }

                _context.SaveChanges();
                transaction.Commit();
            }
            catch (InvalidOperationException)
            {
                transaction.Rollback();
                throw;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _logger?.LogError(ex, "Error updating inventory after production {ProductionId}", production.Id);
                throw new Exception("Error updating inventory after production", ex);
            }
        }
    }

}