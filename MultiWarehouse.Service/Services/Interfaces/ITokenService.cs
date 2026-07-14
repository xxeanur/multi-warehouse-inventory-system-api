using MultiWarehouse.Entity.Entities;
using MultiWarehouse.Shared.DTOs.AuthDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiWarehouse.Service.Services.Interfaces
{
    public interface ITokenService
    {
        // Kullanıcı nesnesini alıp, karşılığında Access ve Refresh token paketini döner.
        TokenDto CreateToken(User user);
    }
}
