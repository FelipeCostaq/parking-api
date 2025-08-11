using MinimalApi.Domain.DTO;
using MinimalApi.Domain.Entity;

namespace MinimalApi.Domain.Interfaces
{
    public interface IAdminService
    {
        Admin? Login(LoginDTO loginDTO);

        Admin? AddAdmin(Admin admin);
        List<Admin> GetAllAdmins(int? page);
        Admin? GetAdminById(int id);
    }
}
