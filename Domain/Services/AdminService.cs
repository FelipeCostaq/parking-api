using System.Xml.Linq;
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

        public Admin? AddAdmin(Admin admin)
        {
            _context.Admin.Add(admin);
            _context.SaveChanges();

            return admin;
        }

        public Admin? GetAdminById(int id)
        {
            return _context.Admin.Where(a => a.Id == id).FirstOrDefault();
        }

        public List<Admin> GetAllAdmins(int? page)
        {
            var query = _context.Admin.AsQueryable();

            int itemsPerPage = 10;

            if (page != null)
            {
                query = query.Skip(((int)page - 1) * itemsPerPage).Take(itemsPerPage);
            }

            return query.ToList();
        }

        public Admin? Login(LoginDTO loginDTO)
        {
            var adm = (_context.Admin.Where(a => a.Email == loginDTO.Email && a.Password == loginDTO.Password).FirstOrDefault());

            return adm;
        }
           
    }
}
