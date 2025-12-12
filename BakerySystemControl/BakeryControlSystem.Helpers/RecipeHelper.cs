using BakeryControlSystem.Data.RepositoriesInterfaces;
using BakeryControlSystem.Domain;

namespace BakeryControlSystem.Helpers
{
    public static class RecipeHelper
    {
        public static void Recipes(IUnitOfWork unitOfWork)
        {
            bool back = false;

            while (!back)
            {
                try
                {
                    Console.Clear();
                    Console.WriteLine("/// RECIPE MANAGEMENT ///");
                    Console.WriteLine();
                    Console.WriteLine("[1] View All Recipes");
                    Console.WriteLine("[2] Create Recipe");
                    Console.WriteLine("[3] View Recipe Details");
                    Console.WriteLine("[4] Add Ingredient to Recipe");
                    Console.WriteLine("[5] Link Recipe to Product");
                    Console.WriteLine("[6] Unlink Recipe from Product");
                    Console.WriteLine("[7] Calculate Recipe Cost");
                    Console.WriteLine("[8] Check Production Feasibility");
                    Console.WriteLine("[0] Back");
                    Console.Write("Option: ");

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
                            ViewDetails(unitOfWork);
                            break;
                        case "4":
                            AddIngredient(unitOfWork);
                            break;
                        case "5":
                            LinkToProduct(unitOfWork);
                            break;
                        case "6":
                            UnlinkFromProduct(unitOfWork);
                            break;
                        case "7":
                            CalculateCost(unitOfWork);
                            break;
                        case "8":
                            CheckFeasibility(unitOfWork);
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
                    Console.WriteLine();
                    Console.WriteLine($"Error: {ex.Message}");
                    Console.ReadLine();
                }
            }
        }

        private static void ViewAll(IUnitOfWork unitOfWork)
        {
            try
            {
                Console.Clear();
                Console.WriteLine("/// RECIPE LIST ///");
                Console.WriteLine();

                var list = unitOfWork.Recipes.GetAll();

                if (!list.Any())
                {
                    Console.WriteLine("No recipes found.");
                }
                else
                {
                    foreach (var r in list)
                    {
                        var cost = unitOfWork.Recipes.CalculateCost(r.Id);
                        var productInfo = r.Product != null ? $"> {r.Product.Name}" : "(No product)";
                        var ingredientCount = r.Details?.Count ?? 0;

                        Console.WriteLine($"[{r.Id}] {r.Name} {productInfo}");
                        Console.WriteLine($"   Cost: ${cost:F2} | Ingredients: {ingredientCount} | Yield: {r.Yield}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                Console.WriteLine();
                Console.WriteLine("Press Enter...");
                Console.ReadLine();
            }
        }

        private static void Create(IUnitOfWork unitOfWork)
        {
            try
            {
                Console.Clear();
                Console.WriteLine("/// CREATE RECIPE ///");
                Console.WriteLine();

                Console.Write("Recipe Name: ");
                string name = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(name))
                {
                    Console.WriteLine("Name is required.");
                    Console.ReadLine();
                    return;
                }

                Console.Write("Instructions: ");
                string instructions = Console.ReadLine();

                Console.Write("Preparation Time (minutes): ");
                if (!int.TryParse(Console.ReadLine(), out int prep) || prep < 0)
                {
                    Console.WriteLine("Invalid time. Using 0.");
                    prep = 0;
                }

                Console.Write("Yield (units per recipe): ");
                if (!int.TryParse(Console.ReadLine(), out int yield) || yield <= 0)
                {
                    Console.WriteLine("Invalid yield. Using 1.");
                    yield = 1;
                }

                Console.WriteLine();
                Console.WriteLine("Available Products:");
                var products = unitOfWork.Products.GetAll();
                foreach (var p in products)
                    Console.WriteLine($"{p.Id}. {p.Name}");

                Console.Write("Link recipe to Product ID (0 for none): ");
                if (!int.TryParse(Console.ReadLine(), out int pid))
                    pid = 0;

                var recipe = new Recipe
                {
                    Name = name,
                    Instructions = instructions ?? string.Empty,
                    PreparationTime = prep,
                    Yield = yield,
                    ProductId = pid > 0 ? pid : null,
                    LastUpdated = DateTime.Now
                };

                unitOfWork.Recipes.Add(recipe);
                unitOfWork.Complete();

                Console.WriteLine();
                Console.WriteLine($"Recipe '{name}' created successfully! ID: {recipe.Id}");
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

        private static void ViewDetails(IUnitOfWork unitOfWork)
        {
            try
            {
                Console.Clear();
                Console.WriteLine("/// RECIPE DETAILS ///");
                Console.WriteLine();

                Console.Write("Recipe ID: ");
                if (!int.TryParse(Console.ReadLine(), out int id) || id <= 0)
                {
                    Console.WriteLine("Invalid ID.");
                    Console.ReadLine();
                    return;
                }

                var r = unitOfWork.Recipes.GetById(id);
                if (r == null)
                {
                    Console.WriteLine("Recipe not found.");
                    Console.ReadLine();
                    return;
                }

                Console.WriteLine();
                Console.WriteLine($"Name: {r.Name}");
                Console.WriteLine($"Instructions: {r.Instructions}");
                Console.WriteLine($"Preparation Time: {r.PreparationTime} minutes");
                Console.WriteLine($"Yield: {r.Yield} units");
                Console.WriteLine($"Last Updated: {r.LastUpdated:yyyy-MM-dd}");

                if (r.ProductId.HasValue && r.Product != null)
                {
                    Console.WriteLine();
                    Console.WriteLine($"Linked Product: {r.Product.Name} (ID: {r.Product.Id})");
                    Console.WriteLine($"Product Price: ${r.Product.SalePrice:F2}");
                    Console.WriteLine($"Available for Sale: {(r.Product.IsActive ? "Yes" : "No")}");

                    if (!r.Product.IsActive)
                    {
                        Console.WriteLine("WARNING: Linked product is not available for sale!");
                    }
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine("Linked Product: None (Recipe is not linked to any product)");
                    Console.WriteLine("Use 'Link Recipe to Product' option to link this recipe.");
                }

                Console.WriteLine();
                Console.WriteLine("Ingredients:");
                if (r.Details != null && r.Details.Any())
                {
                    decimal totalCost = 0;
                    foreach (var d in r.Details)
                    {
                        if (d.Ingredient != null)
                        {
                            var ingredientCost = d.Quantity * d.Ingredient.UnitPrice;
                            totalCost += ingredientCost;

                            Console.WriteLine($"- {d.Quantity:F2} {d.Ingredient.Unit} of {d.Ingredient.Name}");
                            Console.WriteLine($"  Unit Price: ${d.Ingredient.UnitPrice:F2} | Cost: ${ingredientCost:F2}");
                        }
                    }
                    Console.WriteLine();
                    Console.WriteLine($"Total Ingredient Cost: ${totalCost:F2}");
                    Console.WriteLine($"Cost per Unit: ${(r.Yield > 0 ? totalCost / r.Yield : 0):F2}");

                    if (r.ProductId.HasValue && r.Product != null && r.Product.SalePrice > 0 && r.Yield > 0)
                    {
                        var costPerUnit = totalCost / r.Yield;
                        var profit = r.Product.SalePrice - costPerUnit;
                        var margin = r.Product.SalePrice > 0 ? (profit / r.Product.SalePrice) * 100 : 0;

                        Console.WriteLine();
                        Console.WriteLine("Profitability Analysis:");
                        Console.WriteLine($"- Product Price: ${r.Product.SalePrice:F2}");
                        Console.WriteLine($"- Cost per Unit: ${costPerUnit:F2}");
                        Console.WriteLine($"- Profit per Unit: ${profit:F2}");
                        Console.WriteLine($"- Profit Margin: {margin:F1}%");

                        if (profit < 0)
                            Console.WriteLine("WARNING: Recipe costs more than product price!");
                        else if (margin < 20)
                            Console.WriteLine("Note: Profit margin is below 20%.");
                    }
                }
                else
                {
                    Console.WriteLine("No ingredients added.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                Console.WriteLine();
                Console.WriteLine("Press Enter...");
                Console.ReadLine();
            }
        }

        private static void AddIngredient(IUnitOfWork unitOfWork)
        {
            try
            {
                Console.Clear();
                Console.WriteLine("/// ADD INGREDIENT ///");
                Console.WriteLine();

                Console.Write("Recipe ID: ");
                if (!int.TryParse(Console.ReadLine(), out int recipeId) || recipeId <= 0)
                {
                    Console.WriteLine("Invalid Recipe ID.");
                    Console.ReadLine();
                    return;
                }

                var recipe = unitOfWork.Recipes.GetById(recipeId);
                if (recipe == null)
                {
                    Console.WriteLine("Recipe not found.");
                    Console.ReadLine();
                    return;
                }

                Console.WriteLine();
                Console.WriteLine($"Recipe: {recipe.Name}");
                Console.WriteLine($"Current ingredients: {recipe.Details?.Count ?? 0}");

                Console.WriteLine();
                Console.WriteLine("Available Ingredients:");
                var ingredients = unitOfWork.Ingredients.GetAll();
                foreach (var i in ingredients)
                    Console.WriteLine($"{i.Id}. {i.Name} - Stock: {i.CurrentStock} {i.Unit}");

                Console.Write("Ingredient ID: ");
                if (!int.TryParse(Console.ReadLine(), out int ingredientId) || ingredientId <= 0)
                {
                    Console.WriteLine("Invalid Ingredient ID.");
                    Console.ReadLine();
                    return;
                }

                Console.Write("Quantity needed: ");
                if (!decimal.TryParse(Console.ReadLine(), out decimal qty) || qty <= 0)
                {
                    Console.WriteLine("Invalid quantity.");
                    Console.ReadLine();
                    return;
                }

                var ingredient = unitOfWork.Ingredients.GetById(ingredientId);
                if (ingredient == null)
                {
                    Console.WriteLine("Ingredient not found.");
                    Console.ReadLine();
                    return;
                }

                Console.Write($"Unit: {ingredient.Unit} - Is this correct? (y/n): ");
                if (Console.ReadLine()?.ToLower() != "y")
                {
                    Console.WriteLine("Operation cancelled.");
                    Console.ReadLine();
                    return;
                }

                unitOfWork.Recipes.AddIngredientToRecipe(recipeId, ingredientId, qty);
                unitOfWork.Complete();

                Console.WriteLine();
                Console.WriteLine($"Added {qty} {ingredient.Unit} of '{ingredient.Name}' to recipe.");
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

        private static void LinkToProduct(IUnitOfWork unitOfWork)
        {
            try
            {
                Console.Clear();
                Console.WriteLine("/// LINK RECIPE TO PRODUCT ///");
                Console.WriteLine();

                Console.Write("Recipe ID to link: ");
                if (!int.TryParse(Console.ReadLine(), out int recipeId) || recipeId <= 0)
                {
                    Console.WriteLine("Invalid Recipe ID.");
                    Console.ReadLine();
                    return;
                }

                var recipe = unitOfWork.Recipes.GetById(recipeId);
                if (recipe == null)
                {
                    Console.WriteLine($"Recipe with ID {recipeId} not found.");
                    Console.ReadLine();
                    return;
                }

                if (recipe.ProductId.HasValue && recipe.Product != null)
                {
                    Console.WriteLine();
                    Console.WriteLine($"This recipe is currently linked to:");
                    Console.WriteLine($"Product: {recipe.Product.Name} (ID: {recipe.Product.Id})");
                    Console.Write("Do you want to change it? (y/n): ");
                    if (Console.ReadLine()?.ToLower() != "y")
                    {
                        Console.WriteLine("Operation cancelled.");
                        Console.ReadLine();
                        return;
                    }
                }

                Console.WriteLine();
                Console.WriteLine("Available Products:");
                var products = unitOfWork.Products.GetAll();

                if (!products.Any())
                {
                    Console.WriteLine("No products available.");
                    Console.ReadLine();
                    return;
                }

                Console.WriteLine();
                Console.WriteLine("ID  | Name                     | Price    | Available");
                Console.WriteLine("-------------------------------------------------------");

                foreach (var p in products)
                {
                    Console.WriteLine($"{p.Id,3} | {p.Name,-24} | ${p.SalePrice,7:F2} | {(p.IsActive ? "Yes" : "No")}");
                }

                Console.Write("Select Product ID (0 to unlink): ");
                if (!int.TryParse(Console.ReadLine(), out int productId))
                {
                    Console.WriteLine("Invalid Product ID.");
                    Console.ReadLine();
                    return;
                }

                if (productId == 0)
                {
                    recipe.ProductId = null;
                    unitOfWork.Recipes.Update(recipe);
                    unitOfWork.Complete();

                    Console.WriteLine();
                    Console.WriteLine($"Recipe '{recipe.Name}' unlinked from product.");
                }
                else
                {
                    var product = unitOfWork.Products.GetById(productId);
                    if (product == null)
                    {
                        Console.WriteLine($"Product with ID {productId} not found.");
                        Console.ReadLine();
                        return;
                    }

                    if (!product.IsActive)
                    {
                        Console.WriteLine();
                        Console.WriteLine($"Warning: Product '{product.Name}' is not available for sale.");
                        Console.Write("Link anyway? (y/n): ");
                        if (Console.ReadLine()?.ToLower() != "y")
                        {
                            Console.WriteLine("Operation cancelled.");
                            Console.ReadLine();
                            return;
                        }
                    }

                    var existingRecipe = unitOfWork.Recipes.GetByProductId(productId);
                    if (existingRecipe != null && existingRecipe.Id != recipeId)
                    {
                        Console.WriteLine();
                        Console.WriteLine($"Warning: Product '{product.Name}' already has a recipe:");
                        Console.WriteLine($"- Recipe: {existingRecipe.Name} (ID: {existingRecipe.Id})");
                        Console.Write("Replace it with this recipe? (y/n): ");
                        if (Console.ReadLine()?.ToLower() != "y")
                        {
                            Console.WriteLine("Operation cancelled.");
                            Console.ReadLine();
                            return;
                        }

                        existingRecipe.ProductId = null;
                        unitOfWork.Recipes.Update(existingRecipe);
                    }

                    recipe.ProductId = productId;
                    recipe.LastUpdated = DateTime.Now;

                    unitOfWork.Recipes.Update(recipe);
                    unitOfWork.Complete();

                    Console.WriteLine();
                    Console.WriteLine($"Recipe '{recipe.Name}' linked to product '{product.Name}' successfully!");

                    var recipeCost = unitOfWork.Recipes.CalculateCost(recipeId);
                    var costPerUnit = recipe.Yield > 0 ? recipeCost / recipe.Yield : 0;

                    Console.WriteLine();
                    Console.WriteLine("Cost Analysis:");
                    Console.WriteLine($"- Recipe Cost: ${recipeCost:F2}");
                    Console.WriteLine($"- Cost per Unit: ${costPerUnit:F2}");
                    Console.WriteLine($"- Product Price: ${product.SalePrice:F2}");

                    if (product.SalePrice > 0)
                    {
                        var profit = product.SalePrice - costPerUnit;
                        var margin = (profit / product.SalePrice) * 100;
                        Console.WriteLine($"- Profit per Unit: ${profit:F2}");
                        Console.WriteLine($"- Profit Margin: {margin:F1}%");

                        if (profit < 0)
                            Console.WriteLine("WARNING: Recipe costs more than product price!");
                    }
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

        private static void UnlinkFromProduct(IUnitOfWork unitOfWork)
        {
            try
            {
                Console.Clear();
                Console.WriteLine("/// UNLINK RECIPE FROM PRODUCT ///");
                Console.WriteLine();

                Console.Write("Recipe ID to unlink: ");
                if (!int.TryParse(Console.ReadLine(), out int recipeId) || recipeId <= 0)
                {
                    Console.WriteLine("Invalid Recipe ID.");
                    Console.ReadLine();
                    return;
                }

                var recipe = unitOfWork.Recipes.GetById(recipeId);
                if (recipe == null)
                {
                    Console.WriteLine($"Recipe with ID {recipeId} not found.");
                    Console.ReadLine();
                    return;
                }

                if (!recipe.ProductId.HasValue)
                {
                    Console.WriteLine();
                    Console.WriteLine($"Recipe '{recipe.Name}' is not linked to any product.");
                    Console.ReadLine();
                    return;
                }

                Console.WriteLine();
                Console.WriteLine($"Recipe: {recipe.Name}");
                Console.WriteLine($"Currently linked to: {recipe.Product?.Name} (ID: {recipe.ProductId})");

                Console.Write("Are you sure you want to unlink this recipe? (y/n): ");
                if (Console.ReadLine()?.ToLower() != "y")
                {
                    Console.WriteLine("Operation cancelled.");
                    Console.ReadLine();
                    return;
                }

                recipe.ProductId = null;
                recipe.LastUpdated = DateTime.Now;

                unitOfWork.Recipes.Update(recipe);
                unitOfWork.Complete();

                Console.WriteLine();
                Console.WriteLine($"Recipe '{recipe.Name}' unlinked from product successfully.");
                Console.WriteLine("Note: The product can now be linked to another recipe.");
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

        private static void CalculateCost(IUnitOfWork unitOfWork)
        {
            try
            {
                Console.Clear();
                Console.WriteLine("/// COST CALCULATOR ///");
                Console.WriteLine();

                Console.Write("Recipe ID: ");
                if (!int.TryParse(Console.ReadLine(), out int id) || id <= 0)
                {
                    Console.WriteLine("Invalid Recipe ID.");
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

                var total = unitOfWork.Recipes.CalculateCost(id);

                Console.WriteLine();
                Console.WriteLine($"Recipe: {recipe.Name}");
                Console.WriteLine($"Yield: {recipe.Yield} units");
                Console.WriteLine($"Total Cost: ${total:F2}");
                Console.WriteLine($"Cost per Unit: ${(recipe.Yield > 0 ? total / recipe.Yield : 0):F2}");

                if (recipe.Product != null && recipe.Product.SalePrice > 0)
                {
                    var costPerUnit = total / recipe.Yield;
                    var profit = recipe.Product.SalePrice - costPerUnit;
                    var margin = recipe.Product.SalePrice > 0 ? (profit / recipe.Product.SalePrice) * 100 : 0;

                    Console.WriteLine();
                    Console.WriteLine($"Product Sale Price: ${recipe.Product.SalePrice:F2}");
                    Console.WriteLine($"Profit per Unit: ${profit:F2}");
                    Console.WriteLine($"Profit Margin: {margin:F1}%");

                    if (profit < 0)
                        Console.WriteLine("WARNING: Recipe costs more than product sale price!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                Console.WriteLine();
                Console.WriteLine("Press Enter...");
                Console.ReadLine();
            }
        }

        private static void CheckFeasibility(IUnitOfWork unitOfWork)
        {
            try
            {
                Console.Clear();
                Console.WriteLine("/// PRODUCTION FEASIBILITY ///");
                Console.WriteLine();

                Console.Write("Recipe ID: ");
                if (!int.TryParse(Console.ReadLine(), out int id) || id <= 0)
                {
                    Console.WriteLine("Invalid Recipe ID.");
                    Console.ReadLine();
                    return;
                }

                Console.Write("Quantity to produce: ");
                if (!int.TryParse(Console.ReadLine(), out int qty) || qty <= 0)
                {
                    Console.WriteLine("Invalid quantity.");
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

                bool canProduce = unitOfWork.Recipes.CanProduce(id, qty);

                Console.WriteLine();
                Console.WriteLine($"Recipe: {recipe.Name}");
                Console.WriteLine($"Quantity: {qty} units");
                Console.WriteLine($"Can produce? {(canProduce ? "YES" : "NO")}");

                if (!canProduce && recipe.Details != null)
                {
                    Console.WriteLine();
                    Console.WriteLine("Missing ingredients:");
                    foreach (var d in recipe.Details)
                    {
                        if (d.Ingredient != null)
                        {
                            var required = d.Quantity * qty;
                            var available = d.Ingredient.CurrentStock;

                            if (available < required)
                            {
                                var missing = required - available;
                                Console.WriteLine($"- {d.Ingredient.Name}: Need {missing:F2} {d.Ingredient.Unit}");
                                Console.WriteLine($"  (Required: {required:F2}, Available: {available:F2})");
                            }
                        }
                    }
                }
                else if (canProduce)
                {
                    Console.WriteLine();
                    Console.WriteLine("All ingredients are available in sufficient quantity.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                Console.WriteLine();
                Console.WriteLine("Press Enter...");
                Console.ReadLine();
            }
        }
    }
}
    
