using MultiWarehouse.Shared.DTOs;
using MultiWarehouse.Shared.DTOs.UserDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiWarehouse.Service.Services.Interfaces
{
    public interface IUserService
    {
            // Sisteme yeni bir personel kaydeder
            Task<UserDto> CreateUserAsync(UserCreateDto createDto);

            // ID parametresine göre spesifik bir personeli getirir
            Task<UserDto> GetUserByIdAsync(Guid id);

            //tüm kullanıcıları getirir.
            Task<IEnumerable<UserDto>> GetAllUsersAsync();

            // DİKKAT: Veri dönmeyeceği için T almayan, yalın versiyonu kullanıyoruz.
            Task RemoveUserAsync(Guid id);
        }
    }
