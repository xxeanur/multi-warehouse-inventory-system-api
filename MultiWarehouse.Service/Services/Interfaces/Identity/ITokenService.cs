using MultiWarehouse.Entity.Entities.Identity;
using MultiWarehouse.Shared.DTOs.AuthDtos;

namespace MultiWarehouse.Service.Services.Interfaces.Identity
{
    public interface ITokenService
    {
        /// <summary>
        /// Kullanıcı kimliğine ve rolüne (Claim) göre Access ve Refresh Token üretir.
        /// </summary>
        TokenDto CreateToken(User user);
    }
}
