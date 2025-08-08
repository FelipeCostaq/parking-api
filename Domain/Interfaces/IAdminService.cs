using MinimalApi.Domain.DTO;
using MinimalApi.Domain.Entity;

namespace MinimalApi.Domain.Interfaces
{
    public interface IAdminService
    {
        Admin? Login(LoginDTO loginDTO);
    }
}
