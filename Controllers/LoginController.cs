using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using SinafProduction.Models;
using SinafProduction.Services;

namespace SinafProduction.Controllers;

public class LoginController(JwtTokenService jwtService, DataBase db) : Controller
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
		await using var connection = await db.web.OpenConnectionAsync();
		
		await using var cmd = new MySqlCommand("SELECT TRUE FROM blacklist WHERE ip = @ip;", connection);
		cmd.Parameters.AddWithValue("ip", ip);
		
		bool isBlackList;
		await using (var rBlackList = await cmd.ExecuteReaderAsync())
		{
			isBlackList = await rBlackList.ReadAsync() && rBlackList.GetBoolean(0);
		}
		
		cmd.CommandText = "SELECT COUNT(*) FROM login_attempts WHERE (username = @username OR ip = @ip) AND success = false AND date >= NOW() - INTERVAL 15 MINUTE;";
		cmd.Parameters.AddWithValue("username", model.username);
		var authorised = !isBlackList && (long)(await cmd.ExecuteScalarAsync() ?? 0) < N_ATTEMPTS_AUTHORIZED;
		if (authorised)
		{
			cmd.CommandText = "SELECT id, username, password, admin, version FROM users WHERE username = @username;";
			await using var reader = await cmd.ExecuteReaderAsync();
			if (await reader.ReadAsync())
			{
				var user = new User(reader.GetUInt64(0), reader.GetString(1), reader.GetString(2), reader.GetBoolean(3), reader.GetInt32(4));
				if (BCrypt.Net.BCrypt.Verify(model.password, user.password))
				{
					var token = jwtService.GenerateToken(user);
					HttpContext.Response.Cookies.Append("jwt", token, new CookieOptions
					{
						HttpOnly = true,
						Secure = true,
						SameSite = SameSiteMode.Strict,
						Expires = DateTimeOffset.Now.AddDays(2)
					});
					
					success = true;
				}
			}
		}
		
		cmd.CommandText = "INSERT INTO login_attempts (ip, username, success) VALUES (@ip, @username, @success);";
		cmd.Parameters.AddWithValue("success", success);
		await cmd.ExecuteNonQueryAsync();
		
		if (success)
			return model.returnUrl == null ? RedirectToAction("Index", "User") : Redirect(model.returnUrl);
		
		ModelState.AddModelError(string.Empty,
								 authorised ? "Nom d'utilisateur ou mot de passe incorrect." : "Vous avez effectué trop de tentatives, veuillez réessayer plus tard !");
		
		return View(model);
	}
	
	public IActionResult Logout()
	{
		HttpContext.Response.Cookies.Delete("jwt");
		return RedirectToAction("Index");
	}
}