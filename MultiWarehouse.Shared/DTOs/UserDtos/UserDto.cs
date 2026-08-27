using MultiWarehouse.Entity.Enums.User;

namespace MultiWarehouse.Shared.DTOs.UserDtos
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public UserRole Role { get; set; }

        public string Phone { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }

        public Guid? WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public bool ReceiveEmailNotifications { get; set; }
        public bool ReceiveInAppNotifications { get; set; }
    }
}