using Microsoft.EntityFrameworkCore;
using MinimalApi.Domain.DTO;
using MinimalApi.Domain.Entity;
using MinimalApi.Domain.Interfaces;
using MinimalApi.Infrastructure.DB;

namespace MinimalApi.Domain.Services
{
    public class AdminService : IAdminService
    {
        private readonly VehiclesContext _context;

        public AdminService(VehiclesContext context)
        {
            _context = context;
        }

        public Admin? Login(LoginDTO loginDTO)
        {
            var adm = (_context.Admin.Where(a => a.Email == loginDTO.Email && a.Password == loginDTO.Password).FirstOrDefault());

            return adm;
        }
           
    }
}
