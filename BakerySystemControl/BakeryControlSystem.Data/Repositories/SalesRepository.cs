using BakeryControlSystem.Data.DB;
using BakeryControlSystem.Data.RepositoriesInterfaces;
using BakeryControlSystem.Domain;
using Microsoft.EntityFrameworkCore;

namespace BakeryControlSystem.Data.Repositories
{
    public class SaleRepository : ISaleRepository
    {
        private readonly BakeryDbContext _context;

        public SaleRepository(BakeryDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Sale> GetAll()
        {
            try
            {
                return _context.Sales
                    .Include(s => s.Product)
                    .OrderByDescending(s => s.SaleDate)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving all sales", ex);
            }
        }

        public Sale GetById(int id)
        {
            try
            {
                return _context.Sales
                    .Include(s => s.Product)
                    .FirstOrDefault(s => s.Id == id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving sale with ID {id}", ex);
            }
        }

        public void Add(Sale sale)
        {
            if (sale == null)
                throw new ArgumentNullException(nameof(sale));

            try
            {
                if (sale.Quantity <= 0)
                    throw new ArgumentException("Quantity must be greater than 0", nameof(sale.Quantity));

                if (sale.UnitPrice <= 0)
                    throw new ArgumentException("Unit price must be greater than 0", nameof(sale.UnitPrice));

                if (sale.SaleDate == default)
                    sale.SaleDate = DateTime.Now;

                if (string.IsNullOrWhiteSpace(sale.InvoiceNumber))
                {
                    sale.InvoiceNumber = GenerateInvoiceNumber();
                }

                if (sale.TotalPrice == 0)
                {
                    sale.CalculateTotal();
                }

                _context.Sales.Add(sale);
                _context.SaveChanges();
            }
            catch (DbUpdateException dbEx)
            {
                throw new Exception("Error saving sale to database", dbEx);
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding sale", ex);
            }
        }

        public void Update(Sale sale)
        {
            if (sale == null)
                throw new ArgumentNullException(nameof(sale));

            try
            {
                var existingSale = _context.Sales.Find(sale.Id);
                if (existingSale == null)
                    throw new KeyNotFoundException($"Sale with ID {sale.Id} not found");

                _context.Entry(existingSale).CurrentValues.SetValues(sale);

                if (sale.TotalPrice == 0 && sale.Quantity > 0 && sale.UnitPrice > 0)
                {
                    sale.CalculateTotal();
                }

                _context.SaveChanges();
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (DbUpdateException dbEx)
            {
                throw new Exception($"Error updating sale with ID {sale.Id} in database", dbEx);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating sale with ID {sale.Id}", ex);
            }
        }

        public void Delete(int id)
        {
            try
            {
                var sale = _context.Sales.Find(id);
                if (sale == null)
                    throw new KeyNotFoundException($"Sale with ID {id} not found");

                _context.Sales.Remove(sale);
                _context.SaveChanges();
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (DbUpdateException dbEx)
            {
                throw new Exception($"Cannot delete sale with ID {id}. It may be referenced elsewhere.", dbEx);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting sale with ID {id}", ex);
            }
        }

        public IEnumerable<Sale> GetSalesByDate(DateTime date)
        {
            try
            {
                return _context.Sales
                    .Include(s => s.Product)
                    .Where(s => s.SaleDate.Date == date.Date)
                    .OrderByDescending(s => s.SaleDate)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving sales for date {date:yyyy-MM-dd}", ex);
            }
        }

        public IEnumerable<Sale> GetSalesByProduct(int productId)
        {
            try
            {
                return _context.Sales
                    .Include(s => s.Product)
                    .Where(s => s.ProductId == productId)
                    .OrderByDescending(s => s.SaleDate)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving sales for product ID {productId}", ex);
            }
        }

        public IEnumerable<Sale> GetSalesByDateRange(DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
                throw new ArgumentException("Start date cannot be after end date");

            try
            {
                return _context.Sales
                    .Include(s => s.Product)
                    .Where(s => s.SaleDate.Date >= startDate.Date && s.SaleDate.Date <= endDate.Date)
                    .OrderByDescending(s => s.SaleDate)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving sales from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}", ex);
            }
        }

        public decimal GetTotalSalesByDate(DateTime date)
        {
            try
            {
                return _context.Sales
                    .Where(s => s.SaleDate.Date == date.Date)
                    .Sum(s => s.TotalPrice);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error calculating total sales for date {date:yyyy-MM-dd}", ex);
            }
        }

        public decimal GetTotalSalesByDateRange(DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
                throw new ArgumentException("Start date cannot be after end date");

            try
            {
                return _context.Sales
                    .Where(s => s.SaleDate.Date >= startDate.Date && s.SaleDate.Date <= endDate.Date)
                    .Sum(s => s.TotalPrice);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error calculating total sales from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}", ex);
            }
        }

        public int GetTotalUnitsSoldByProduct(int productId)
        {
            try
            {
                return _context.Sales
                    .Where(s => s.ProductId == productId)
                    .Sum(s => s.Quantity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error calculating total units sold for product ID {productId}", ex);
            }
        }

        public Dictionary<string, decimal> GetSalesByProductReport(DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
                throw new ArgumentException("Start date cannot be after end date");

            try
            {
                return _context.Sales
                    .Include(s => s.Product)
                    .Where(s => s.SaleDate.Date >= startDate.Date && s.SaleDate.Date <= endDate.Date)
                    .GroupBy(s => s.Product.Name)
                    .Select(g => new
                    {
                        ProductName = g.Key,
                        TotalSales = g.Sum(s => s.TotalPrice)
                    })
                    .ToDictionary(x => x.ProductName, x => x.TotalSales);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error generating sales by product report from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}", ex);
            }
        }

        public Dictionary<string, decimal> GetSalesByPaymentMethodReport(DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
                throw new ArgumentException("Start date cannot be after end date");

            try
            {
                return _context.Sales
                    .Where(s => s.SaleDate.Date >= startDate.Date && s.SaleDate.Date <= endDate.Date)
                    .GroupBy(s => s.PaymentMethod.ToString())
                    .Select(g => new
                    {
                        PaymentMethod = g.Key,
                        TotalSales = g.Sum(s => s.TotalPrice)
                    })
                    .ToDictionary(x => x.PaymentMethod, x => x.TotalSales);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error generating sales by payment method report from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}", ex);
            }
        }

        private string GenerateInvoiceNumber()
        {
            var datePrefix = DateTime.Now.ToString("yyyyMMdd");
            var count = _context.Sales
                .Count(s => s.SaleDate.Date == DateTime.Today) + 1;

            return $"{datePrefix}-{count:D4}";
        }
    }
}