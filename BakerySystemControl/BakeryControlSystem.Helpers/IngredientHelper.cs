using BakeryControlSystem.Data.RepositoriesInterfaces;
using BakeryControlSystem.Domain;

namespace BakeryControlSystem.Helpers
{
    public static class IngredientHelper
    {
        public static void Ingredients(IUnitOfWork unitOfWork)
        {
            bool back = false;

            while (!back)
            {
                try
                {
                    Console.Clear();
                    Console.WriteLine("/// INGREDIENT MANAGEMENT ///\n");
                    Console.WriteLine("1- View All Ingredients");
                    Console.WriteLine("2- Create Ingredient");
                    Console.WriteLine("3- Delete Ingredient");
                    Console.WriteLine("4- Check Low Stock");
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
                            Delete(unitOfWork);
                            break;
                        case "4":
                            CheckLowStock(unitOfWork);
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
                Console.WriteLine("/// INGREDIENT LIST ///\n");

                var list = unitOfWork.Ingredients.GetAll();

                if (!list.Any())
                {
                    Console.WriteLine("No ingredients found.");
                }
                else
                {
                    foreach (var i in list)
                    {
                        Console.WriteLine($"{i.Id} | {i.Name} | Stock: {i.CurrentStock} {i.Unit} | Price: ${i.UnitPrice}");
                    }
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
                Console.WriteLine("/// CREATE INGREDIENT ///\n");

                Console.Write("Name: ");
                string name = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(name))
                {
                    Console.WriteLine("Name is required.");
                    Console.ReadLine();
                    return;
                }

                Console.Write("Description: ");
                string desc = Console.ReadLine();

                Console.WriteLine("\nMeasurement Units:");
                Console.WriteLine("0 = Kilogram");
                Console.WriteLine("1 = Gram");
                Console.WriteLine("2 = Liter");
                Console.WriteLine("3 = Milliliter");
                Console.WriteLine("4 = Unit");
                Console.WriteLine("5 = Package");
                Console.WriteLine("6 = Dozen");

                Console.Write("Unit: ");
                if (!int.TryParse(Console.ReadLine(), out int unit) || unit < 0 || unit > 6)
                {
                    Console.WriteLine("Invalid unit. Using Kilogram.");
                    unit = 0;
                }

                Console.Write("Stock: ");
                if (!decimal.TryParse(Console.ReadLine(), out decimal stock) || stock < 0)
                {
                    Console.WriteLine("Invalid stock. Using 0.");
                    stock = 0;
                }

                Console.Write("Price per unit: ");
                if (!decimal.TryParse(Console.ReadLine(), out decimal price) || price <= 0)
                {
                    Console.WriteLine("Invalid price. Using 1.00.");
                    price = 1.00m;
                }

                var ingredient = new Ingredient
                {
                    Name = name,
                    Description = desc ?? string.Empty,
                    Unit = (MeasurementUnit)unit,
                    CurrentStock = stock,
                    UnitPrice = price,
                    MinimumStock = 5,
                    MaximumStock = stock * 3,
                    LastRestockDate = DateTime.Now,
                    ExpirationDate = DateTime.Now.AddDays(30)
                };

                unitOfWork.Ingredients.Add(ingredient);
                unitOfWork.Complete();

                Console.WriteLine("\nIngredient created successfully!");
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

        private static void Delete(IUnitOfWork unitOfWork)
        {
            try
            {
                Console.Clear();
                Console.WriteLine("/// DELETE INGREDIENT ///\n");

                Console.Write("Ingredient ID: ");
                if (!int.TryParse(Console.ReadLine(), out int id) || id <= 0)
                {
                    Console.WriteLine("Invalid ID.");
                    Console.ReadLine();
                    return;
                }

                unitOfWork.Ingredients.Delete(id);
                unitOfWork.Complete();

                Console.WriteLine("Ingredient deleted.");
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

        private static void CheckLowStock(IUnitOfWork unitOfWork)
        {
            try
            {
                Console.Clear();
                Console.WriteLine("/// LOW STOCK CHECK ///\n");

                var lowStockItems = unitOfWork.Ingredients.GetLowStock();

                if (!lowStockItems.Any())
                {
                    Console.WriteLine("All ingredients have sufficient stock.");
                }
                else
                {
                    Console.WriteLine($"Found {lowStockItems.Count()} ingredients with low stock:\n");
                    
                    foreach (var item in lowStockItems)
                    {
                        decimal missing = item.MinimumStock - item.CurrentStock;
                        if (missing > 0)
                        {
                            Console.WriteLine($"• {item.Name}: Need {missing:F2} {item.Unit} (Current: {item.CurrentStock:F2})");
                        }
                    }
                    
                    Console.WriteLine("\nThese ingredients need to be restocked.");
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