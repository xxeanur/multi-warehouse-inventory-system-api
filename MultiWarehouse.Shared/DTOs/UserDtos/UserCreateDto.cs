using MultiWarehouse.Entity.Enums.User;

namespace MultiWarehouse.Shared.DTOs.UserDtos
{
    public class UserCreateDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public UserRole Role { get; set; }

        public Guid? WarehouseId { get; set; }
        public string Phone { get; set; } = string.Empty;
    }
}