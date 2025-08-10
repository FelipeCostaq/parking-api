using Microsoft.EntityFrameworkCore;
using MinimalApi.Domain.Entity;
using MinimalApi.Domain.Interfaces;
using MinimalApi.Infrastructure.DB;

namespace MinimalApi.Domain.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly VehiclesContext _context;

        public VehicleService(VehiclesContext context)
        {
            _context = context;
        }

        public void AddVehicle(Vehicle vehicle)
        {
            _context.Vehicles.Add(vehicle);
            _context.SaveChanges();
        }

        public void DeleteVehicle(Vehicle vehicle)
        {
            _context.Vehicles.Remove(vehicle);
            _context.SaveChanges();
        }

        public void EditVehicle(Vehicle vehicle)
        {
            _context.Vehicles.Update(vehicle);
            _context.SaveChanges();
        }

        public List<Vehicle> GetAllVehicles(int? page = 1, string? name = null, string? brand = null)
        {
            var query = _context.Vehicles.AsQueryable();

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(v => EF.Functions.Like(v.Name.ToLower(), $"%{name.ToLower()}%"));
            }

            int itemsPerPage = 10;

            if (page != null)
            {
                query = query.Skip(((int)page - 1) * itemsPerPage).Take(itemsPerPage);
            }

            return query.ToList();
        }

        public Vehicle? GetVehicleById(int id)
        {
            return _context.Vehicles.Where(v => v.Id == id).FirstOrDefault();
        }
    }
}
