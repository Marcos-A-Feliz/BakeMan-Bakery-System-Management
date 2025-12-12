using BakeryControlSystem.Domain;
using Microsoft.EntityFrameworkCore;

namespace BakeryControlSystem.Data.DB
{
    public class BakeryDbContext : DbContext
    {
        public BakeryDbContext(DbContextOptions<BakeryDbContext> options)
            : base(options)
        {
        }

        public DbSet<RecipeDetail> RecipeDetails { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<DailyProduction> DailyProductions { get; set; }
        public DbSet<ProductionDetail> ProductionDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===== RECIPE DETAIL =====
            modelBuilder.Entity<RecipeDetail>(entity =>
            {
                entity.ToTable("RecipeDetails");
                entity.HasKey(rd => rd.Id);

                entity.HasOne(rd => rd.Recipe)
                    .WithMany(r => r.Details)
                    .HasForeignKey(rd => rd.RecipeId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(rd => rd.Ingredient)
                    .WithMany()
                    .HasForeignKey(rd => rd.IngredientId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== RECIPE =====
            modelBuilder.Entity<Recipe>(entity =>
            {
                entity.HasOne(r => r.Product)
                    .WithMany(p => p.Recipes)
                    .HasForeignKey(r => r.ProductId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // ===== SALE ===== (CON navegación inversa)
            modelBuilder.Entity<Sale>()
                .HasOne(s => s.Product)
                .WithMany(p => p.Sales)  // ← NUEVO: Con colección
                .HasForeignKey(s => s.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // ===== DAILY PRODUCTION ===== (CON navegación inversa)
            modelBuilder.Entity<DailyProduction>()
                .HasOne(dp => dp.Product)
                .WithMany(p => p.DailyProductions)  // ← NUEVO: Con colección
                .HasForeignKey(dp => dp.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // ===== PRODUCTION DETAIL =====
            modelBuilder.Entity<ProductionDetail>(entity =>
            {
                entity.ToTable("ProductionDetails");
                entity.HasKey(pd => pd.Id);

                entity.HasOne(pd => pd.DailyProduction)
                    .WithMany(dp => dp.UsedIngredients)
                    .HasForeignKey(pd => pd.DailyProductionId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(pd => pd.Ingredient)
                    .WithMany()
                    .HasForeignKey(pd => pd.IngredientId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(pd => pd.PlannedQuantity)
                    .HasPrecision(18, 3);

                entity.Property(pd => pd.ActualQuantity)
                    .HasPrecision(18, 3);

                entity.Property(pd => pd.Variance)
                    .HasPrecision(18, 3);
            });

            // ===== INGREDIENT =====
            modelBuilder.Entity<Ingredient>()
                .Property(i => i.UnitPrice)
                .HasPrecision(18, 2);

            // ===== CONFIGURACIONES ADICIONALES =====
            ConfigureDecimalPrecision(modelBuilder);
            ConfigureIndexes(modelBuilder);
            ConfigureDefaultValues(modelBuilder);
        }

        private void ConfigureDecimalPrecision(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>()
                .Property(p => p.SalePrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Sale>()
                .Property(s => s.TotalPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Ingredient>()
                .Property(i => i.UnitPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Recipe>()
                .Property(r => r.PreparationCost)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Recipe>()
                .Property(r => r.TotalCost)
                .HasPrecision(18, 2);
        }

        private void ConfigureIndexes(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Sale>()
                .HasIndex(s => s.SaleDate);

            modelBuilder.Entity<DailyProduction>()
                .HasIndex(dp => dp.ProductionDate);

            modelBuilder.Entity<Ingredient>()
                .HasIndex(i => i.Name)
                .IsUnique();

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Name)
                .IsUnique();
        }

        private void ConfigureDefaultValues(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>()
                .Property(p => p.IsActive)
                .HasDefaultValue(true);

            modelBuilder.Entity<Product>()
                .Property(p => p.CreationDate)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<Sale>()
                .Property(s => s.SaleDate)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<DailyProduction>()
                .Property(dp => dp.ProductionDate)
                .HasDefaultValueSql("GETDATE()");
        }
    }
}