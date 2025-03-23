using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SinafProduction.Data;
using SinafProduction.Models;
using SinafProduction.Services;

namespace SinafProduction.Controllers;

public class LoginController(JwtTokenService jwtService) : Controller
{
	private const int N_ATTEMPTS_AUTHORIZED = 3;
	
	[HttpGet]
	[Route("Login")]
	public IActionResult Index(string? returnUrl) => View(new LoginModel { returnUrl = returnUrl });
	
	[HttpPost]
	[ValidateAntiForgeryToken]
	[Route("Login")]
	public async Task<IActionResult> Index(LoginModel model)
	{
		if (!ModelState.IsValid)
			return View(model);
		
		string? ip = Request.Headers["X-Forwarded-For"];
		if (string.IsNullOrEmpty(ip))
			ip = Request.HttpContext.Connection.RemoteIpAddress?.ToString();
		
		var success = false;
		
		await using var context = new AppDbContext();
		var isBlacklist = await context.blacklists.Where(b => b.ip == ip).AnyAsync();
		var nAttempts = await context.loginAttempts.Where(a => (a.ip == ip || a.username == model.username) && !a.success && a.date >= DateTime.Now.AddMinutes(-15)).CountAsync();
		var authorised = !isBlacklist && nAttempts < N_ATTEMPTS_AUTHORIZED;
		if (authorised)
		{
			var user = await context.users.Where(u => u.username == model.username).FirstOrDefaultAsync();
			if (user != null)
			{
				if (BCrypt.Net.BCrypt.Verify(model.password, user.password))
				{
					var token = jwtService.GenerateToken(user);
					HttpContext.Response.Cookies.Append("jwt", token, new CookieOptions
					{
						Domain = ".sinafproduction.xyz",
						HttpOnly = true,
						Secure = true,
						SameSite = SameSiteMode.Lax,
						Expires = DateTimeOffset.Now.AddDays(2)
					});
					
					success = true;
				}
			}
		}
		
		await context.Database.ExecuteSqlRawAsync("INSERT INTO login_attempts (ip, username, success) VALUES ({0}, {1}, {2});", ip ?? "0.0.0.404", model.username, success);
		
		if (success)
			return Redirect("https://panel.sinafproduction.xyz");
			// TODO > Remettre la redirection vers le returnURL
			// return model.returnUrl == null ? Redirect("https://panel.sinafproduction.xyz") : Redirect(model.returnUrl);
		
		ModelState.AddModelError(string.Empty, authorised ? "Nom d'utilisateur ou mot de passe incorrect." : "Vous avez effectué trop de tentatives, veuillez réessayer plus tard !");
		
		return View(model);
	}
	
	public IActionResult Logout()
	{
		HttpContext.Response.Cookies.Delete("jwt");
		return RedirectToAction("Index");
	}
}