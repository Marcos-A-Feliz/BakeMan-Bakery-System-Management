using System;

namespace BakeryControlSystem.Data.RepositoriesInterfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IProductRepository Products { get; }
        IIngredientRepository Ingredients { get; }
        IRecipeRepository Recipes { get; }
        ISaleRepository Sales { get; }
        IDailyProductionRepository DailyProductions { get; }

        int Complete();
        void BeginTransaction();
        void CommitTransaction();
        void RollbackTransaction();
    }
}