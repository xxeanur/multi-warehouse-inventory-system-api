using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MultiWarehouse.Entity.Entities; // Kendi User namespace'in
using MultiWarehouse.Service.Services.Interfaces;
using MultiWarehouse.Shared.Configurations;
using MultiWarehouse.Shared.DTOs.AuthDtos;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MultiWarehouse.Service.Services.Implementations
{
    public class TokenService : ITokenService
    {
        private readonly CustomTokenOption _tokenOption;

        public TokenService(IOptions<CustomTokenOption> options)
        {
            _tokenOption = options.Value;
        }

        public TokenDto CreateToken(User user)
        {
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), 
        
        // Yorum satırlarını kaldırdık ve projendeki Entity alanlarına göre aktif ettik.
        new Claim(ClaimTypes.Name, user.FirstName),
        new Claim(ClaimTypes.Surname, user.LastName),
        
        // EN KRİTİK NOKTA: Role enum olduğu için .ToString() ile metne çevirmek zorundayız.
        new Claim(ClaimTypes.Role, user.Role.ToString())
    };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenOption.SecurityKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);
            var expiration = DateTime.UtcNow.AddMinutes(_tokenOption.AccessTokenExpiration);

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _tokenOption.Issuer,
                audience: _tokenOption.Audience,
                expires: expiration,
                notBefore: DateTime.UtcNow,
                claims: claims,
                signingCredentials: credentials
            );

            var handler = new JwtSecurityTokenHandler();

            return new TokenDto
            {
                AccessToken = handler.WriteToken(jwtSecurityToken),
                RefreshToken = GenerateRefreshToken(),
                AccessTokenExpiration = expiration,
                RefreshTokenExpiration = DateTime.UtcNow.AddDays(_tokenOption.RefreshTokenExpiration)
            };
        }


        private string GenerateRefreshToken()
        {
            var number = new byte[32];
            using var random = RandomNumberGenerator.Create();
            random.GetBytes(number);
            return Convert.ToBase64String(number);
        }
    }
}