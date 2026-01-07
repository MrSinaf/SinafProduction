using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SinafProduction.Data;
using SinafProduction.Models;

namespace SinafProduction.Controllers;

public class AuthController(AppDbContext context) : Controller
{
	[HttpGet("/login")]
	public IActionResult Login([FromQuery] string? returnUrl) => View(new AuthLoginModel { ReturnUrl = returnUrl });
	
	[HttpPost("/login"), ValidateAntiForgeryToken]
	public async Task<IActionResult> Login(AuthLoginModel model)
	{
		if (!ModelState.IsValid)
			return View(model);
		
		var user = await context.Users.FirstOrDefaultAsync(x => x.UniqueName == model.Username.ToLower());
		
		if (user != null && BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
		{
			var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY");
			if (jwtKey == null)
				throw new NullReferenceException("Impossible de trouver la variable d'environnement 'JWT_KEY' !");
			
			var jwtToken = new JwtSecurityToken(
				issuer: "sinafproduction", audience: "sinafproduction", claims:
				[
					new Claim(ClaimTypes.Name, user.UniqueName),
					new Claim(ClaimTypes.Role, user.Admin ? "Admin" : "User")
				],
				expires: DateTime.Now.AddDays(6),
				signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
														   SecurityAlgorithms.HmacSha256)
			);
			
			var token = new JwtSecurityTokenHandler().WriteToken(jwtToken);
			Response.Cookies.Append("jwt", token, new CookieOptions
			{
				HttpOnly = true,
				Secure = true,
				SameSite = SameSiteMode.Strict,
				Expires = DateTime.Now.AddDays(6)
			});
			
			return LocalRedirect(model.ReturnUrl ?? "/");
		}
		
		ModelState.AddModelError(string.Empty, "Le nom d'utilisateur ou le mot de passe est incorrect !");
		return View(model);
	}
	
	[HttpGet("/logout")]
	public Task<IActionResult> Logout()
	{
		Response.Cookies.Delete("jwt");
		return Task.FromResult<IActionResult>(LocalRedirect("/"));
	}
}