namespace MultiWarehouse.Shared.Configurations
{
    public class CustomTokenOption
    {
        public string Audience { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string SecurityKey { get; set; } = string.Empty;

        public int AccessTokenExpiration { get; set; }
        public int RefreshTokenExpiration { get; set; }
    }
}
