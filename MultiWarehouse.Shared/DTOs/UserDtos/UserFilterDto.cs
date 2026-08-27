using MultiWarehouse.Entity.Enums.User;

namespace MultiWarehouse.Shared.DTOs.UserDtos
{

    public class UserFilterDto
    {
        public string? SearchText { get; set; }
        public Guid? WarehouseId { get; set; }
        public UserRole? Role { get; set; }
        public bool IsActive { get; set; } = true;
    }
}