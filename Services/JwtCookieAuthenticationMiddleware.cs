using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace SinafProduction.Services;

public class JwtCookieAuthenticationMiddleware(RequestDelegate next)
{
	public async Task Invoke(HttpContext context)
	{
		var token = context.Request.Cookies["jwt"];
		if (!string.IsNullOrEmpty(token))
		{
			var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY");
			if (jwtKey == null)
				throw new NullReferenceException("Impossible de trouver la variable d'environnement 'JWT_KEY' !");
			
			var tokenHandler = new JwtSecurityTokenHandler();
			try
			{
				var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
				{
					ValidateIssuer = true,
					ValidateAudience = false,
					ValidIssuer = "sinafproduction",
					IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
					ValidateLifetime = true,
					ValidateIssuerSigningKey = true
				}, out _);
				
				context.User = principal;
			}
			catch
			{
				// Token invalide ou expiré : User reste anonyme
			}
		}
		
		await next(context);
	}
}