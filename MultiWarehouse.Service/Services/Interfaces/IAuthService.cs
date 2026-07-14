using MultiWarehouse.Shared.DTOs.AuthDtos;
using System.Threading.Tasks;

namespace MultiWarehouse.Service.Services.Interfaces
{
    public interface IAuthService
    {
        // Artık sadece saf TokenDto dönüyor.
        Task<TokenDto> LoginAsync(LoginDto loginDto);
    }
}