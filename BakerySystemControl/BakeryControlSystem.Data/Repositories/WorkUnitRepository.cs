using BakeryControlSystem.Data.RepositoriesInterfaces;
using BakeryControlSystem.Data.DB;
using Microsoft.EntityFrameworkCore.Storage;

namespace BakeryControlSystem.Data.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly BakeryDbContext _context;
        private bool _disposed = false;
        private IDbContextTransaction _transaction;

        private ProductRepository _productRepository;
        private IngredientRepository _ingredientRepository;
        private RecipeRepository _recipeRepository;
        private SaleRepository _saleRepository;
        private DailyProductionRepository _dailyProductionRepository;

        public UnitOfWork(BakeryDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IProductRepository Products =>
            _productRepository ??= new ProductRepository(_context);

        public IIngredientRepository Ingredients =>
            _ingredientRepository ??= new IngredientRepository(_context);

        public IRecipeRepository Recipes =>
            _recipeRepository ??= new RecipeRepository(_context);

        public ISaleRepository Sales =>
            _saleRepository ??= new SaleRepository(_context);

        public IDailyProductionRepository DailyProductions =>
            _dailyProductionRepository ??= new DailyProductionRepository(_context);

        public int Complete()
        {
            try
            {
                return _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception("Error saving changes to database", ex);
            }
        }

        public void BeginTransaction()
        {
            if (_transaction != null)
            {
                throw new InvalidOperationException("Transaction already started");
            }

            try
            {
                _transaction = _context.Database.BeginTransaction();
            }
            catch (Exception ex)
            {
                throw new Exception("Error starting transaction", ex);
            }
        }

        public void CommitTransaction()
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("No transaction to commit");
            }

            try
            {
                _transaction.Commit();
                _transaction.Dispose();
                _transaction = null;
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                throw new Exception("Error committing transaction", ex);
            }
        }

        public void RollbackTransaction()
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("No transaction to rollback");
            }

            try
            {
                _transaction.Rollback();
                _transaction.Dispose();
                _transaction = null;
            }
            catch (Exception ex)
            {
                throw new Exception("Error rolling back transaction", ex);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _transaction?.Dispose();
                    _context.Dispose();
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}