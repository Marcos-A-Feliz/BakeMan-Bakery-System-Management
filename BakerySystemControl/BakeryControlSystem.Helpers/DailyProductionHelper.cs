using BakeryControlSystem.Data.RepositoriesInterfaces;
using BakeryControlSystem.Domain;

namespace BakeryControlSystem.Helpers
{
    public static class ProductionHelper
    {
        public static void Production(IUnitOfWork unitOfWork)
        {
            bool back = false;
            while (!back)
            {
                try
                {
                    Console.Clear();
                    Console.WriteLine("/// PRODUCTION MANAGEMENT ///\n");
                    Console.WriteLine("1- Production Calculator");
                    Console.WriteLine("2- View Production History");
                    Console.WriteLine("3- Check Ingredient Stock");
                    Console.WriteLine("0- Back");
                    Console.Write("\nOption: ");

                    var opt = Console.ReadLine();

                    switch (opt)
                    {
                        case "1":
                            ProductionCalculator(unitOfWork);
                            break;
                        case "2":
                            ViewProductionHistory(unitOfWork);
                            break;
                        case "3":
                            CheckIngredientStock(unitOfWork);
                            break;
                        case "0":
                            back = true;
                            break;
                        default:
                            Console.WriteLine("Invalid option. Press Enter...");
                            Console.ReadLine();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\nError in Production menu: {ex.Message}");
                    Console.WriteLine("Press Enter to continue...");
                    Console.ReadLine();
                }
            }
        }

        private static void ProductionCalculator(IUnitOfWork unitOfWork)
        {
            try
            {
                Console.Clear();
                Console.WriteLine("/// PRODUCTION CALCULATOR ///\n");

                Console.Write("Recipe ID: ");
                if (!int.TryParse(Console.ReadLine(), out int id) || id <= 0)
                {
                    Console.WriteLine("Invalid Recipe ID. Must be a positive number.");
                    Console.ReadLine();
                    return;
                }

                Console.Write("Quantity: ");
                if (!int.TryParse(Console.ReadLine(), out int qty) || qty <= 0)
                {
                    Console.WriteLine("Invalid quantity. Must be a positive number.");
                    Console.ReadLine();
                    return;
                }

                var recipe = unitOfWork.Recipes.GetById(id);
                if (recipe == null)
                {
                    Console.WriteLine("Recipe not found.");
                    Console.ReadLine();
                    return;
                }

                var costPerUnit = unitOfWork.Recipes.CalculateCost(id);
                var totalCost = costPerUnit * qty;

                bool canProduce = unitOfWork.Recipes.CanProduce(id, qty);

                Console.WriteLine($"\nCan produce: {(canProduce ? "YES" : "NO")}");
                Console.WriteLine($"Total Cost: ${totalCost:F2}");
                Console.WriteLine($"Cost per unit: ${costPerUnit:F2}");

                if (recipe.Product != null)
                {
                    var revenue = recipe.Product.SalePrice * qty;
                    var profit = revenue - totalCost;
                    var margin = revenue > 0 ? (profit / revenue) * 100 : 0;

                    Console.WriteLine($"\nRevenue: ${revenue:F2}");
                    Console.WriteLine($"Profit: ${profit:F2}");
                    Console.WriteLine($"Profit Margin: {margin:F1}%");

                    if (profit < 0)
                        Console.WriteLine("Warning: Production at a loss!");
                }

                if (!canProduce)
                {
                    Console.WriteLine("\nMissing ingredients:");
                    foreach (var detail in recipe.Details)
                    {
                        if (detail.Ingredient != null)
                        {
                            var required = detail.Quantity * qty;
                            var available = detail.Ingredient.CurrentStock;
                            if (available < required)
                            {
                                var missing = required - available;
                                Console.WriteLine($"• {detail.Ingredient.Name}: Missing {missing:F2} {detail.Ingredient.Unit}");
                            }
                        }
                    }
                }

                Console.WriteLine("\nCalculation completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError in production calculator: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("\nPress Enter to continue...");
                Console.ReadLine();
            }
        }


        private static void ViewProductionHistory(IUnitOfWork unitOfWork)
        {
            try
            {
                Console.Clear();
                Console.WriteLine("/// PRODUCTION HISTORY ///\n");

                Console.WriteLine("1- View Today's Production");
                Console.WriteLine("2- View by Date");
                Console.WriteLine("3- View by Date Range");
                Console.WriteLine("4- View All");
                Console.Write("\nOption: ");

                var option = Console.ReadLine();
                IEnumerable<DailyProduction> productions = null;

                try
                {
                    switch (option)
                    {
                        case "1":
                            productions = unitOfWork.DailyProductions.GetByDate(DateTime.Today);
                            break;
                        case "2":
                            Console.Write("Enter date (yyyy-MM-dd): ");
                            if (DateTime.TryParse(Console.ReadLine(), out DateTime specificDate))
                            {
                                productions = unitOfWork.DailyProductions.GetByDate(specificDate);
                            }
                            else
                            {
                                Console.WriteLine("Invalid date format.");
                                Console.ReadLine();
                                return;
                            }
                            break;
                        case "3":
                            Console.Write("Start date (yyyy-MM-dd): ");
                            if (!DateTime.TryParse(Console.ReadLine(), out DateTime startDate))
                            {
                                Console.WriteLine("Invalid start date format.");
                                Console.ReadLine();
                                return;
                            }

                            Console.Write("End date (yyyy-MM-dd): ");
                            if (!DateTime.TryParse(Console.ReadLine(), out DateTime endDate))
                            {
                                Console.WriteLine("Invalid end date format.");
                                Console.ReadLine();
                                return;
                            }

                            if (startDate > endDate)
                            {
                                Console.WriteLine("Warning: Start date is after end date.");
                                var temp = startDate;
                                startDate = endDate;
                                endDate = temp;
                            }

                            productions = unitOfWork.DailyProductions.GetByDateRange(startDate, endDate);
                            break;
                        case "4":
                            productions = unitOfWork.DailyProductions.GetAll();
                            break;
                        default:
                            Console.WriteLine("Invalid option.");
                            Console.ReadLine();
                            return;
                    }

                    if (productions == null || !productions.Any())
                    {
                        Console.WriteLine("\nNo production records found.");
                    }
                    else
                    {
                        Console.WriteLine($"\nFound {productions.Count()} production records:");
                        Console.WriteLine("=============================================\n");

                        foreach (var prod in productions)
                        {
                            try
                            {
                                var productName = "Unknown";
                                var product = unitOfWork.Products.GetById(prod.ProductId);
                                if (product != null)
                                {
                                    productName = product.Name;
                                }

                                Console.WriteLine($"[{prod.ProductionDate:yyyy-MM-dd}] Product: {productName}");
                                Console.WriteLine($"  Planned: {prod.PlannedQuantity} | Actual: {prod.ActualQuantity} | Waste: {prod.WasteQuantity}");
                                Console.WriteLine($"  Status: {prod.ProductionStatus} | Efficiency: {prod.EfficiencyPercentage():F1}%");
                                if (!string.IsNullOrEmpty(prod.Notes))
                                    Console.WriteLine($"  Notes: {prod.Notes}");
                                Console.WriteLine();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error displaying production record: {ex.Message}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\nError loading production history: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nUnexpected error: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("\nPress Enter to continue...");
                Console.ReadLine();
            }
        }

        private static void CheckIngredientStock(IUnitOfWork unitOfWork)
        {
            try
            {
                Console.Clear();
                Console.WriteLine("/// INGREDIENT STOCK STATUS ///\n");

                var ingredients = unitOfWork.Ingredients.GetAll();

                if (!ingredients.Any())
                {
                    Console.WriteLine("No ingredients found.");
                }
                else
                {
                    Console.WriteLine("ID | Name | Current Stock | Unit | Min Stock | Status");
                    Console.WriteLine(new string('-', 70));

                    int lowStockCount = 0;
                    int criticalStockCount = 0;

                    foreach (var ingredient in ingredients)
                    {
                        try
                        {
                            string status = "OK";
                            if (ingredient.CurrentStock <= 0)
                            {
                                status = "OUT OF STOCK";
                                criticalStockCount++;
                            }
                            else if (ingredient.CurrentStock <= ingredient.MinimumStock)
                            {
                                status = "LOW STOCK";
                                lowStockCount++;
                            }
                            else if (ingredient.CurrentStock <= ingredient.MinimumStock * 1.5m)
                            {
                                status = "WARNING";
                            }

                            Console.WriteLine($"{ingredient.Id,3} | {ingredient.Name,-20} | " +
                                            $"{ingredient.CurrentStock,12:F2} | " +
                                            $"{ingredient.Unit,-6} | " +
                                            $"{ingredient.MinimumStock,9:F2} | " +
                                            $"{status}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error displaying ingredient {ingredient.Id}: {ex.Message}");
                        }
                    }

                    Console.WriteLine($"\nStock Summary:");
                    Console.WriteLine($" - Total ingredients: {ingredients.Count()}");
                    Console.WriteLine($" - In stock: {ingredients.Count() - lowStockCount - criticalStockCount}");
                    Console.WriteLine($" - Low stock: {lowStockCount}");
                    Console.WriteLine($" - Out of stock: {criticalStockCount}");

                    if (criticalStockCount > 0)
                    {
                        Console.WriteLine($"\nURGENT: {criticalStockCount} ingredients are OUT OF STOCK!");
                    }
                    else if (lowStockCount > 0)
                    {
                        Console.WriteLine($"\nWarning: {lowStockCount} ingredients are below minimum stock!");
                    }

                    var lowStockItems = unitOfWork.Ingredients.GetLowStock();
                    if (lowStockItems.Any())
                    {
                        Console.WriteLine("\nLow Stock Items:");
                        foreach (var item in lowStockItems)
                        {
                            var needed = item.MinimumStock - item.CurrentStock;
                            if (needed > 0)
                            {
                                Console.WriteLine($"• {item.Name}: Need {needed:F2} {item.Unit} (Current: {item.CurrentStock:F2})");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError checking ingredient stock: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("\nPress Enter to continue...");
                Console.ReadLine();
            }
        }
    }
}