using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SinafProduction.Models;

namespace SinafProduction.Services;

public class JwtTokenService(string secretKey, string issuer, string audience)
{
    public string GenerateToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(secretKey);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([
                new Claim("id", user.id.ToString()),
                new Claim("username", user.name),
                new Claim("admin", user.isAdmin.ToString()),
                new Claim("version", user.version.ToString())
            ]),
            Expires = DateTime.UtcNow.AddDays(2),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}