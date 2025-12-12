using BakeryControlSystem.Data.RepositoriesInterfaces;
using BakeryControlSystem.Domain;

namespace BakeryControlSystem.Helpers
{
    public static class ProductHelper
    {
        public static void Products(IUnitOfWork unitOfWork)
        {
            bool back = false;

            while (!back)
            {
                try
                {
                    Console.Clear();
                    Console.WriteLine("/// PRODUCT MANAGEMENT ///\n");
                    Console.WriteLine("1- View Products (with filter)");
                    Console.WriteLine("2- Create Product");
                    Console.WriteLine("3- Update Product Price");
                    Console.WriteLine("4- Toggle Product Availability");
                    Console.WriteLine("5- Delete Product");
                    Console.WriteLine("0- Back");
                    Console.Write("\nOption: ");

                    var opt = Console.ReadLine();

                    switch (opt)
                    {
                        case "1":
                            ViewWithFilter(unitOfWork);
                            break;
                        case "2":
                            Create(unitOfWork);
                            break;
                        case "3":
                            UpdatePrice(unitOfWork);
                            break;
                        case "4":
                            ToggleAvailability(unitOfWork);
                            break;
                        case "5":
                            Delete(unitOfWork);
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

        private static void ViewWithFilter(IUnitOfWork unitOfWork)
        {
            try
            {
                Console.Clear();
                Console.WriteLine("/// VIEW PRODUCTS ///\n");

                Console.WriteLine("Filter options:");
                Console.WriteLine("1- All products");
                Console.WriteLine("2- Available for sale (Active)");
                Console.WriteLine("3- Not available (Inactive)");
                Console.WriteLine("4- By category");
                Console.Write("\nOption: ");

                var filterOpt = Console.ReadLine();
                IEnumerable<Product> products = null;
                string filterDescription = "All products";

                switch (filterOpt)
                {
                    case "1":
                        products = unitOfWork.Products.GetAll();
                        filterDescription = "All products";
                        break;
                    case "2":
                        products = unitOfWork.Products.GetActiveProducts();
                        filterDescription = "Available for sale";
                        break;
                    case "3":
                        products = unitOfWork.Products.GetAll()
                            .Where(p => !p.IsActive);
                        filterDescription = "Not available";
                        break;
                    case "4":
                        Console.Write("\nEnter category name: ");
                        string category = Console.ReadLine();
                        if (!string.IsNullOrWhiteSpace(category))
                        {
                            products = unitOfWork.Products.GetByCategory(category);
                            filterDescription = $"Category: {category}";
                        }
                        else
                        {
                            products = unitOfWork.Products.GetAll();
                            filterDescription = "All products (no category specified)";
                        }
                        break;
                    default:
                        products = unitOfWork.Products.GetAll();
                        filterDescription = "All products";
                        break;
                }

                Console.Clear();
                Console.WriteLine($" --- PRODUCT LIST --- {filterDescription} ====\n");

                if (products == null || !products.Any())
                {
                    Console.WriteLine("No products found with the selected filter.");
                }
                else
                {
                    Console.WriteLine($"Found {products.Count()} products:\n");
                    Console.WriteLine("ID  | Name                     | Price    | Category      | Available | Recipes");
                    Console.WriteLine(new string('-', 80));

                    foreach (var p in products)
                    {
                        var recipeCount = p.Recipes?.Count ?? 0;
                        var available = p.IsActive ? "Yes" : "No";

                        Console.WriteLine($"{p.Id,3} | {p.Name,-24} | " +
                                        $"${p.SalePrice,7:F2} | " +
                                        $"{p.Category,-13} | " +
                                        $"{available,9} | " +
                                        $"{recipeCount,7}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("\nPress Enter to continue...");
                Console.ReadLine();
            }
        }

        private static void Create(IUnitOfWork unitOfWork)
        {
            try
            {
                Console.Clear();
                Console.WriteLine("/// CREATE PRODUCT ///\n");

                Console.Write("Product Name: ");
                string name = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(name))
                {
                    Console.WriteLine("Name is required.");
                    Console.ReadLine();
                    return;
                }

                Console.Write("Description: ");
                string desc = Console.ReadLine();

                Console.Write("Category: ");
                string category = Console.ReadLine();

                Console.Write("Sale Price: ");
                if (!decimal.TryParse(Console.ReadLine(), out decimal price) || price <= 0)
                {
                    Console.WriteLine("Invalid price. Using $1.00 as default.");
                    price = 1.00m;
                }

                Console.Write("Available for sale? (y/n): ");
                bool isAvailable = Console.ReadLine()?.ToLower() == "y";

                var product = new Product
                {
                    Name = name,
                    Description = desc ?? string.Empty,
                    Category = category ?? string.Empty,
                    SalePrice = price,
                    IsActive = isAvailable,
                    CreationDate = DateTime.Now
                };

                unitOfWork.Products.Add(product);
                unitOfWork.Complete();

                Console.WriteLine($"\nProduct '{name}' created successfully!");
                Console.WriteLine($"Price: ${price} | Available: {(isAvailable ? "Yes" : "No")}");
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

        private static void UpdatePrice(IUnitOfWork unitOfWork)
        {
            try
            {
                Console.Clear();
                Console.WriteLine("/// UPDATE PRODUCT PRICE ///\n");

                Console.Write("Product ID: ");
                if (!int.TryParse(Console.ReadLine(), out int id) || id <= 0)
                {
                    Console.WriteLine("Invalid ID.");
                    Console.ReadLine();
                    return;
                }

                var product = unitOfWork.Products.GetById(id);
                if (product == null)
                {
                    Console.WriteLine($"Product with ID {id} not found.");
                    Console.ReadLine();
                    return;
                }

                Console.WriteLine($"\nCurrent product: {product.Name}");
                Console.WriteLine($"Current price: ${product.SalePrice:F2}");
                Console.WriteLine($"Category: {product.Category}");
                Console.WriteLine($"Available: {(product.IsActive ? "Yes" : "No")}");

                Console.Write("\nNew price: $");
                if (!decimal.TryParse(Console.ReadLine(), out decimal newPrice) || newPrice <= 0)
                {
                    Console.WriteLine("Invalid price. Must be greater than 0.");
                    Console.ReadLine();
                    return;
                }

                var oldPrice = product.SalePrice;
                product.SalePrice = newPrice;

                unitOfWork.Products.Update(product);
                unitOfWork.Complete();

                Console.WriteLine($"\nPrice updated successfully!");
                Console.WriteLine($"Changed from ${oldPrice:F2} to ${newPrice:F2}");

                if (newPrice > oldPrice * 1.2m)
                {
                    Console.WriteLine("Note: Price increased by more than 20%.");
                }
                else if (newPrice < oldPrice * 0.8m)
                {
                    Console.WriteLine("Note: Price decreased by more than 20%.");
                }
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

        private static void ToggleAvailability(IUnitOfWork unitOfWork)
        {
            try
            {
                Console.Clear();
                Console.WriteLine("/// TOGGLE PRODUCT AVAILABILITY ///\n");

                Console.Write("Product ID: ");
                if (!int.TryParse(Console.ReadLine(), out int id) || id <= 0)
                {
                    Console.WriteLine("Invalid ID.");
                    Console.ReadLine();
                    return;
                }

                var product = unitOfWork.Products.GetById(id);
                if (product == null)
                {
                    Console.WriteLine($"Product with ID {id} not found.");
                    Console.ReadLine();
                    return;
                }

                Console.WriteLine($"\nProduct: {product.Name}");
                Console.WriteLine($"Current status: {(product.IsActive ? "AVAILABLE for sale" : "NOT AVAILABLE")}");
                Console.WriteLine($"Price: ${product.SalePrice:F2}");
                Console.WriteLine($"Category: {product.Category}");

                Console.Write($"\nMake product {(product.IsActive ? "UNAVAILABLE" : "AVAILABLE")}? (y/n): ");
                if (Console.ReadLine()?.ToLower() != "y")
                {
                    Console.WriteLine("No changes made.");
                    Console.ReadLine();
                    return;
                }

                product.IsActive = !product.IsActive;

                unitOfWork.Products.Update(product);
                unitOfWork.Complete();

                Console.WriteLine($"\nProduct availability updated!");
                Console.WriteLine($"Now: {(product.IsActive ? "AVAILABLE for sale" : "NOT AVAILABLE")}");

                if (!product.IsActive)
                {
                    Console.WriteLine("\nNote: This product will not appear in sales or production.");
                }
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
                Console.WriteLine("/// DELETE PRODUCT ///\n");

                Console.Write("Product ID: ");
                if (!int.TryParse(Console.ReadLine(), out int id) || id <= 0)
                {
                    Console.WriteLine("Invalid ID.");
                    Console.ReadLine();
                    return;
                }

                var product = unitOfWork.Products.GetById(id);
                if (product == null)
                {
                    Console.WriteLine($"Product with ID {id} not found.");
                    Console.ReadLine();
                    return;
                }

                Console.WriteLine($"\nProduct: {product.Name}");
                Console.WriteLine($"Price: ${product.SalePrice:F2}");
                Console.WriteLine($"Available: {(product.IsActive ? "Yes" : "No")}");
                Console.WriteLine($"Recipes: {product.Recipes?.Count ?? 0}");

                if (product.Recipes?.Count > 0)
                {
                    Console.WriteLine("\nWARNING: This product has recipes associated.");
                    Console.WriteLine("Deleting it will also remove the recipes.");
                }

                Console.Write("\nAre you sure? Type 'DELETE' to confirm: ");
                if (Console.ReadLine()?.ToUpper() != "DELETE")
                {
                    Console.WriteLine("Deletion cancelled.");
                    Console.ReadLine();
                    return;
                }

                unitOfWork.Products.Delete(id);
                unitOfWork.Complete();

                Console.WriteLine($"\nProduct '{product.Name}' deleted successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine("Note: Product cannot be deleted if it has sales records.");
            }
            finally
            {
                Console.ReadLine();
            }
        }
    }
}