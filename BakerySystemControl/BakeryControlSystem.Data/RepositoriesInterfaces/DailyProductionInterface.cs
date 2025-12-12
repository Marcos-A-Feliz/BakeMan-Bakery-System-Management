using BakeryControlSystem.Domain;

namespace BakeryControlSystem.Data.RepositoriesInterfaces
{
    public interface IDailyProductionRepository
    {
        DailyProduction GetById(int id);
        DailyProduction GetByDateAndProduct(DateTime date, int productId);
        IEnumerable<DailyProduction> GetAll();
        IEnumerable<DailyProduction> GetByDate(DateTime date);
        IEnumerable<DailyProduction> GetByDateRange(DateTime startDate, DateTime endDate);
        void Add(DailyProduction production);
        void Update(DailyProduction production);
        void Delete(int id);
        decimal GetProductionEfficiency(DateTime date);
        Dictionary<int, int> GetWasteReport(DateTime startDate, DateTime endDate);
        bool RegisterProduction(DailyProduction production, bool updateInventory);
        IEnumerable<DailyProduction> GetByProduct(int productId);
        IEnumerable<DailyProduction> GetByDateRangeAndProduct(DateTime startDate, DateTime endDate, int productId);
        decimal GetTotalProductionByDate(DateTime date);
        int GetTotalUnitsProducedByProduct(int productId, DateTime startDate, DateTime endDate);
        Dictionary<int, decimal> GetProductionCostReport(DateTime startDate, DateTime endDate);
        bool CheckIngredientsAvailability(int productId, int quantity);
    }
}