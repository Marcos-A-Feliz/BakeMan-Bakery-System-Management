namespace BakeryControlSystem.Domain
{
    public class Sale
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }  
        public decimal TotalPrice { get; set; }
        public DateTime SaleDate { get; set; }
        public string CustomerName { get; set; }
        public string InvoiceNumber { get; set; }  
        public PaymentMethod PaymentMethod { get; set; }
        public Sale()
        {
            SaleDate = DateTime.Now;
            CustomerName = string.Empty;
            InvoiceNumber = string.Empty;
            PaymentMethod = PaymentMethod.Cash;
        }
        public void CalculateTotal()
        {
            TotalPrice = Quantity * UnitPrice;
        }
    }
    public enum PaymentMethod
    {
        Cash,
        Card,
        Transfer,
        Credit
    }
}