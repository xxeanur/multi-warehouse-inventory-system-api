using MultiWarehouse.Entity.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiWarehouse.Shared.DTOs.UserDtos
{
    public class UserCreateDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        // Admin yeni kullanıcıyı eklerken ona geçici veya kalıcı bir şifre belirler.
        public string Password { get; set; } = string.Empty;

        // Bu kullanıcının yetkisi ne olacak? (Örn: Admin mi, Depo Sorumlusu mu?)
        public UserRole Role { get; set; }
    }
}