using BakeryControlSystem.Domain;


namespace BakeryControlSystem.Data.RepositoriesInterfaces
{
    public interface ISaleRepository
    {
        IEnumerable<Sale> GetAll();
        Sale GetById(int id);
        void Add(Sale sale);
        void Update(Sale sale);
        void Delete(int id);

        IEnumerable<Sale> GetSalesByDate(DateTime date);
        IEnumerable<Sale> GetSalesByProduct(int productId);
        IEnumerable<Sale> GetSalesByDateRange(DateTime startDate, DateTime endDate);

        decimal GetTotalSalesByDate(DateTime date);
        decimal GetTotalSalesByDateRange(DateTime startDate, DateTime endDate);
        int GetTotalUnitsSoldByProduct(int productId);

        Dictionary<string, decimal> GetSalesByProductReport(DateTime startDate, DateTime endDate);
        Dictionary<string, decimal> GetSalesByPaymentMethodReport(DateTime startDate, DateTime endDate);
    }
}