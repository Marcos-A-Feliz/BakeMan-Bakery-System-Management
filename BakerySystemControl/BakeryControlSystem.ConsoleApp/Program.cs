using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using BakeryControlSystem.Data.DB;
using BakeryControlSystem.Data.RepositoriesInterfaces;
using BakeryControlSystem.Data.Repositories;
using BakeryControlSystem.Helpers;

namespace BakeryControlSystem.ConsoleApp
{
    internal class Program
    {
        private static IServiceProvider _serviceProvider = null!;

        static void Main(string[] args)
        {
            Console.Title = "BakeMan ~~(Bakery Production Control System)~~";
            Console.WriteLine("Initializing...");

            try
            {
                ConfigureServices();

                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<BakeryDbContext>();

                    Console.WriteLine("Checking database connection...");
                    if (dbContext.Database.CanConnect())
                    {
                        Console.WriteLine("Database connection successful!");
                    }
                    else
                    {
                        Console.WriteLine("Warning: Cannot connect to database.");
                        Console.WriteLine("The application will attempt to create the database if needed.");
                    }
                }

                RunMainMenu();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fatal error during startup: {ex.Message}");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }

        private static void ConfigureServices()
        {
            var services = new ServiceCollection();

            var connectionString = "Server=(localdb)\\mssqllocaldb;Database=BakeryControlDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

            services.AddDbContext<BakeryDbContext>(options =>
                options.UseSqlServer(connectionString,
                    sqlOptions => sqlOptions.EnableRetryOnFailure()));

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IIngredientRepository, IngredientRepository>();
            services.AddScoped<IRecipeRepository, RecipeRepository>();
            services.AddScoped<ISaleRepository, SaleRepository>();
            services.AddScoped<IDailyProductionRepository, DailyProductionRepository>();

            services.AddScoped<BakeryDbContext>();

            _serviceProvider = services.BuildServiceProvider();
        }

        private static void RunMainMenu()
        {
            bool exit = false;

            while (!exit)
            {
                try
                {
                    Console.Clear();
                    Console.WriteLine("/// BAKERY PRODUCTION CONTROL SYSTEM ///\n");
                    Console.WriteLine("1- Product Management");
                    Console.WriteLine("2- Ingredient Management");
                    Console.WriteLine("3- Recipe Management");
                    Console.WriteLine("4- Daily Production");
                    Console.WriteLine("5- Sales Management");
                    Console.WriteLine("6- System Information");
                    Console.WriteLine("0- Exit Application");
                    Console.Write("\nSelect option: ");

                    var option = Console.ReadLine();

                    using var scope = _serviceProvider.CreateScope();
                    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    switch (option)
                    {
                        case "1":
                            ProductHelper.Products(unitOfWork);
                            break;
                        case "2":
                            IngredientHelper.Ingredients(unitOfWork);
                            break;
                        case "3":
                            RecipeHelper.Recipes(unitOfWork);
                            break;
                        case "4":
                            ProductionHelper.Production(unitOfWork);
                            break;
                        case "5":
                            SalesHelper.Sales(unitOfWork);
                            break;
                        case "6":
                            ShowSystemInfo(scope);
                            break;
                        case "0":
                            exit = true;
                            Console.WriteLine("\nExiting application...");
                            break;
                        default:
                            Console.WriteLine("\nInvalid option. Please try again.");
                            Console.WriteLine("Press Enter to continue...");
                            Console.ReadLine();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\nError in main menu: {ex.Message}");
                    Console.WriteLine("Press Enter to return to main menu...");
                    Console.ReadLine();
                }
            }
        }

        private static void ShowSystemInfo(IServiceScope scope)
        {
            try
            {
                Console.Clear();
                Console.WriteLine("=== SYSTEM INFORMATION ===\n");

                using var dbContext = scope.ServiceProvider.GetRequiredService<BakeryDbContext>();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                Console.WriteLine("Database Status:");
                Console.WriteLine($"- Can connect: {dbContext.Database.CanConnect()}");

                try
                {
                    var productCount = unitOfWork.Products.GetAll().Count();
                    var ingredientCount = unitOfWork.Ingredients.GetAll().Count();
                    var recipeCount = unitOfWork.Recipes.GetAll().Count();
                    var saleCount = unitOfWork.Sales.GetAll().Count();
                    var productionCount = unitOfWork.DailyProductions.GetAll().Count();

                    Console.WriteLine("\nRecord Counts:");
                    Console.WriteLine($"- Products: {productCount}");
                    Console.WriteLine($"- Ingredients: {ingredientCount}");
                    Console.WriteLine($"- Recipes: {recipeCount}");
                    Console.WriteLine($"- Sales: {saleCount}");
                    Console.WriteLine($"- Productions: {productionCount}");

                    var inventoryValue = unitOfWork.Ingredients.GetTotalInventoryValue();
                    Console.WriteLine($"\nTotal Inventory Value: ${inventoryValue:F2}");


                    var todaysSales = unitOfWork.Sales.GetTotalSalesByDate(DateTime.Today);
                    Console.WriteLine($"Today's Sales: ${todaysSales:F2}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error retrieving counts: {ex.Message}");
                }

                Console.WriteLine($"\nCurrent Date: {DateTime.Now:yyyy-MM-dd HH:mm}");
                Console.WriteLine($"Application: Bakery Control System v1.0");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error showing system info: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("\nPress Enter to continue...");
                Console.ReadLine();
            }
        }
    }
}