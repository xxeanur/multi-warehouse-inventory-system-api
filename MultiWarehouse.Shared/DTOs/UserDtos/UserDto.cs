using MultiWarehouse.Entity.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiWarehouse.Shared.DTOs.UserDtos
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public UserRole Role { get; set; }

        // BaseEntity'den gelen, frontend'de "Kayıt Tarihi" ve "Durum" sütunlarında göstereceğimiz alanlar
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }
    }
}