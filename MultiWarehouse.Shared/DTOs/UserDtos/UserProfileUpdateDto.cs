namespace MultiWarehouse.Shared.DTOs.UserDtos
{
    public class UserProfileUpdateDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;

        public bool ReceiveEmailNotifications { get; set; }
        public bool ReceiveInAppNotifications { get; set; }
    }
}