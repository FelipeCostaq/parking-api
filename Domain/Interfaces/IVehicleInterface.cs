using Microsoft.AspNetCore.Mvc.RazorPages;
using MinimalApi.Domain.Entity;

namespace MinimalApi.Domain.Interfaces
{
    public interface IVehicleInterface
    {
        List<Vehicle> GetAllVehicles(int page = 1, string? name = null, string? brand = null);
        Vehicle? GetVehicleById(int id);
        void AddVehicle(Vehicle vehicle);
        void EditVehicle(Vehicle vehicle);
        void DeleteVehicle(Vehicle vehicle);
    }
}
