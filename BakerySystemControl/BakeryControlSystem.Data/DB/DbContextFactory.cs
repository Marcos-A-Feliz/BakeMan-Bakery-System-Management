using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BakeryControlSystem.Data.DB
{
    public class BakeryDbContextFactory : IDesignTimeDbContextFactory<BakeryDbContext>
    {
        public BakeryDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<BakeryDbContext>();

            // Cadena de conexión DIRECTA (sin appsettings.json para diseño)
            var connectionString = "Server=(localdb)\\mssqllocaldb;Database=BakeryControlDB;Trusted_Connection=True;MultipleActiveResultSets=true";

            optionsBuilder.UseSqlServer(connectionString);

            return new BakeryDbContext(optionsBuilder.Options);
        }
    }
}