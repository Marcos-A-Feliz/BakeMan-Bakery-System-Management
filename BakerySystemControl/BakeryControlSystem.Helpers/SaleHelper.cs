using BakeryControlSystem.Data.RepositoriesInterfaces;
using BakeryControlSystem.Domain;

namespace BakeryControlSystem.Helpers
{
    public static class SalesHelper
    {
        public static void Sales(IUnitOfWork unitOfWork)
        {
            bool back = false;

            while (!back)
            {
                try
                {
                    Console.Clear();
                    Console.WriteLine("/// SALES MANAGEMENT ///\n");
                    Console.WriteLine("1- View Sales");
                    Console.WriteLine("2- Create Sale");
                    Console.WriteLine("3- Sales Report");
                    Console.WriteLine("4- View Today's Sales");
                    Console.WriteLine("0- Back");
                    Console.Write("\nOption: ");

                    var opt = Console.ReadLine();

                    switch (opt)
                    {
                        case "1":
                            ViewAll(unitOfWork);
                            break;
                        case "2":
                            Create(unitOfWork);
                            break;
                        case "3":
                            SalesReport(unitOfWork);
                            break;
                        case "4":
                            ViewTodaysSales(unitOfWork);
                            break;
                        case "0":
                            back = true;
                            break;
                        default:
                            Console.WriteLine("Invalid option.");
                            Console.ReadLine();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\nError: {ex.Message}");
                    Console.ReadLine();
                }
            }
        }

        private static void ViewAll(IUnitOfWork unitOfWork)
        {
            try
            {
                Console.Clear();
                Console.WriteLine("/// ALL SALES ///\n");

                Console.WriteLine("View options:");
                Console.WriteLine("1- All sales");
                Console.WriteLine("2- By date range");
                Console.WriteLine("3- By product");
                Console.Write("\nOption: ");

                var viewOpt = Console.ReadLine();
                IEnumerable<Sale> sales = null;

                switch (viewOpt)
                {
                    case "1":
                        sales = unitOfWork.Sales.GetAll();
                        break;
                    case "2":
                        Console.Write("Start date (yyyy-MM-dd): ");
                        if (!DateTime.TryParse(Console.ReadLine(), out DateTime startDate))
                        {
                            Console.WriteLine("Invalid start date.");
                            Console.ReadLine();
                            return;
                        }

                        Console.Write("End date (yyyy-MM-dd): ");
                        if (!DateTime.TryParse(Console.ReadLine(), out DateTime endDate))
                        {
                            Console.WriteLine("Invalid end date.");
                            Console.ReadLine();
                            return;
                        }

                        if (startDate > endDate)
                        {
                            var temp = startDate;
                            startDate = endDate;
                            endDate = temp;
                        }

                        sales = unitOfWork.Sales.GetSalesByDateRange(startDate, endDate);
                        break;
                    case "3":
                        Console.Write("Product ID: ");
                        if (!int.TryParse(Console.ReadLine(), out int productId))
                        {
                            Console.WriteLine("Invalid product ID.");
                            Console.ReadLine();
                            return;
                        }

                        sales = unitOfWork.Sales.GetSalesByProduct(productId);
                        break;
                    default:
                        sales = unitOfWork.Sales.GetAll();
                        break;
                }

                Console.Clear();
                Console.WriteLine("/// SALES LIST ///\n");

                if (sales == null || !sales.Any())
                {
                    Console.WriteLine("No sales found.");
                }
                else
                {
                    decimal totalSales = 0;
                    int totalUnits = 0;

                    Console.WriteLine("Date       | Product              | Qty | Unit Price | Total     | Payment");
                    Console.WriteLine(new string('-', 75));

                    foreach (var s in sales.OrderByDescending(x => x.SaleDate))
                    {
                        var productName = s.Product?.Name ?? $"Product {s.ProductId}";
                        if (productName.Length > 20) productName = productName.Substring(0, 17) + "...";

                        Console.WriteLine($"{s.SaleDate:yyyy-MM-dd} | " +
                                        $"{productName,-20} | " +
                                        $"{s.Quantity,3} | " +
                                        $"${s.UnitPrice,9:F2} | " +
                                        $"${s.TotalPrice,9:F2} | " +
                                        $"{s.PaymentMethod}");

                        totalSales += s.TotalPrice;
                        totalUnits += s.Quantity;
                    }

                    Console.WriteLine(new string('-', 75));
                    Console.WriteLine($"Total: {sales.Count()} sales | Units: {totalUnits} | Amount: ${totalSales:F2}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("\nPress Enter...");
                Console.ReadLine();
            }
        }

        private static void Create(IUnitOfWork unitOfWork)
        {
            try
            {
                Console.Clear();
                Console.WriteLine("/// CREATE SALE ///\n");

                Console.WriteLine("Available Products (Active only):");
                var products = unitOfWork.Products.GetActiveProducts();

                if (!products.Any())
                {
                    Console.WriteLine("No active products available for sale.");
                    Console.ReadLine();
                    return;
                }

                foreach (var p in products)
                {
                    Console.WriteLine($"{p.Id}. {p.Name} - ${p.SalePrice:F2}");
                }

                Console.Write("\nProduct ID: ");
                if (!int.TryParse(Console.ReadLine(), out int productId) || productId <= 0)
                {
                    Console.WriteLine("Invalid product ID.");
                    Console.ReadLine();
                    return;
                }

                var product = unitOfWork.Products.GetById(productId);
                if (product == null)
                {
                    Console.WriteLine("Product not found.");
                    Console.ReadLine();
                    return;
                }

                if (!product.IsActive)
                {
                    Console.WriteLine("This product is not available for sale.");
                    Console.ReadLine();
                    return;
                }

                Console.Write("Quantity: ");
                if (!int.TryParse(Console.ReadLine(), out int quantity) || quantity <= 0)
                {
                    Console.WriteLine("Invalid quantity.");
                    Console.ReadLine();
                    return;
                }

                Console.Write("Customer Name (leave empty for 'Walk-in'): ");
                string customerName = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(customerName))
                    customerName = "Walk-in customer";

                Console.WriteLine("\nPayment Method:");
                Console.WriteLine("1- Cash");
                Console.WriteLine("2- Card");
                Console.WriteLine("3- Transfer");
                Console.WriteLine("4- Credit");
                Console.Write("Option (1-4): ");

                PaymentMethod paymentMethod = PaymentMethod.Cash;
                if (int.TryParse(Console.ReadLine(), out int paymentOption) && paymentOption >= 1 && paymentOption <= 4)
                {
                    paymentMethod = (PaymentMethod)(paymentOption - 1);
                }

                var sale = new Sale
                {
                    ProductId = productId,
                    Quantity = quantity,
                    UnitPrice = product.SalePrice,
                    SaleDate = DateTime.Now,
                    CustomerName = customerName,
                    PaymentMethod = paymentMethod
                };

                sale.CalculateTotal();

                Console.WriteLine($"\nSale Summary:");
                Console.WriteLine($"Product: {product.Name}");
                Console.WriteLine($"Quantity: {quantity}");
                Console.WriteLine($"Unit Price: ${product.SalePrice:F2}");
                Console.WriteLine($"Total: ${sale.TotalPrice:F2}");
                Console.WriteLine($"Customer: {customerName}");
                Console.WriteLine($"Payment: {paymentMethod}");

                Console.Write("\nConfirm sale? (y/n): ");
                if (Console.ReadLine()?.ToLower() != "y")
                {
                    Console.WriteLine("Sale cancelled.");
                    Console.ReadLine();
                    return;
                }

                unitOfWork.Sales.Add(sale);
                unitOfWork.Complete();

                Console.WriteLine($"\nSale recorded successfully! Invoice #{sale.Id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                Console.ReadLine();
            }
        }

        private static void SalesReport(IUnitOfWork unitOfWork)
        {
            try
            {
                Console.Clear();
                Console.WriteLine(" /// SALES REPORT ///\n");

                Console.Write("Start date (yyyy-MM-dd): ");
                if (!DateTime.TryParse(Console.ReadLine(), out DateTime startDate))
                {
                    Console.WriteLine("Invalid start date.");
                    Console.ReadLine();
                    return;
                }

                Console.Write("End date (yyyy-MM-dd): ");
                if (!DateTime.TryParse(Console.ReadLine(), out DateTime endDate))
                {
                    Console.WriteLine("Invalid end date.");
                    Console.ReadLine();
                    return;
                }

                if (startDate > endDate)
                {
                    var temp = startDate;
                    startDate = endDate;
                    endDate = temp;
                }

                var sales = unitOfWork.Sales.GetSalesByDateRange(startDate, endDate);
                var totalSales = unitOfWork.Sales.GetTotalSalesByDateRange(startDate, endDate);
                var byProduct = unitOfWork.Sales.GetSalesByProductReport(startDate, endDate);
                var byPayment = unitOfWork.Sales.GetSalesByPaymentMethodReport(startDate, endDate);

                Console.Clear();
                Console.WriteLine($" ///SALES REPORT: {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd} ====\n");

                Console.WriteLine($"Period: {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");
                Console.WriteLine($"Total Sales: ${totalSales:F2}");
                Console.WriteLine($"Number of Sales: {sales.Count()}");

                Console.WriteLine("\nSales by Product:");
                if (byProduct.Any())
                {
                    foreach (var item in byProduct.OrderByDescending(x => x.Value))
                    {
                        Console.WriteLine($"- {item.Key}: ${item.Value:F2}");
                    }
                }
                else
                {
                    Console.WriteLine("No sales data.");
                }

                Console.WriteLine("\nSales by Payment Method:");
                if (byPayment.Any())
                {
                    foreach (var item in byPayment.OrderByDescending(x => x.Value))
                    {
                        Console.WriteLine($"- {item.Key}: ${item.Value:F2}");
                    }
                }

                var topProduct = byProduct.OrderByDescending(x => x.Value).FirstOrDefault();
                if (!topProduct.Equals(default(KeyValuePair<string, decimal>)))
                {
                    Console.WriteLine($"\nBest Selling Product: {topProduct.Key} (${topProduct.Value:F2})");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("\nPress Enter...");
                Console.ReadLine();
            }
        }

        private static void ViewTodaysSales(IUnitOfWork unitOfWork)
        {
            try
            {
                Console.Clear();
                Console.WriteLine(" ///TODAY'S SALES/// \n");

                var today = DateTime.Today;
                var todaysSales = unitOfWork.Sales.GetSalesByDate(today);
                var todaysTotal = unitOfWork.Sales.GetTotalSalesByDate(today);

                Console.WriteLine($"Date: {today:yyyy-MM-dd}");
                Console.WriteLine($"Total Sales Today: ${todaysTotal:F2}");
                Console.WriteLine($"Number of Sales: {todaysSales.Count()}\n");

                if (todaysSales.Any())
                {
                    Console.WriteLine("Recent Sales:");
                    foreach (var s in todaysSales.OrderByDescending(x => x.SaleDate).Take(10))
                    {
                        var productName = s.Product?.Name ?? $"Product {s.ProductId}";
                        Console.WriteLine($"[{s.SaleDate:HH:mm}] {productName} - {s.Quantity} x ${s.UnitPrice:F2} = ${s.TotalPrice:F2}");
                    }

                    if (todaysSales.Count() > 10)
                    {
                        Console.WriteLine($"... and {todaysSales.Count() - 10} more sales.");
                    }
                }
                else
                {
                    Console.WriteLine("No sales today yet.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("\nPress Enter...");
                Console.ReadLine();
            }
        }
    }
}