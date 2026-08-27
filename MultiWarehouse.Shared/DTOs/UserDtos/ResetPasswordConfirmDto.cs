namespace MultiWarehouse.Shared.DTOs.UserDtos
{
    public class ResetPasswordConfirmDto
    {
        public string Token { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}