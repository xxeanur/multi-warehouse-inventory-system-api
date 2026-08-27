namespace MultiWarehouse.Shared.DTOs.AuthDtos
{
    public class UserContextDto
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? WarehouseId { get; set; }
    }

    public class TokenDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime AccessTokenExpiration { get; set; }
        public DateTime RefreshTokenExpiration { get; set; }

        public UserContextDto User { get; set; } = new UserContextDto();
    }
}
