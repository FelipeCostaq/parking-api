using Microsoft.EntityFrameworkCore;
using MinimalApi.Domain.Entity;

namespace MinimalApi.Infrastructure.DB
{
    public class VehiclesContext : DbContext
    {
        private readonly IConfiguration _contextAppSetting;

        public VehiclesContext(IConfiguration contextAppSetting)
        {
            _contextAppSetting = contextAppSetting;
        }

        public DbSet<Admin> Admin { get; set; } = default!;
        public DbSet<Vehicle> Vehicles { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Admin>().HasData(
                new Admin
                {
                    Id = 1,
                    Email = "administrador@admin.com",
                    Password = "admin123",
                    Profile = "Adm"
                }
            );
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = _contextAppSetting.GetConnectionString("mysql")?.ToString();

                if (!string.IsNullOrEmpty(connectionString))
                {
                    optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
                }
            }
        }
    }
}
