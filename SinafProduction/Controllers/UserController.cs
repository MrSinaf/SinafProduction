using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SinafProduction.Data;
using SinafProduction.Models;
using SinafProduction.Services;

namespace SinafProduction.Controllers;

[Route("user")]
public class UserController : Controller
{
	[AdminAuthorize]
	public IActionResult Index() => View();
	
	[Route("{username}")]
	public async Task<IActionResult> Profil(string username)
	{
		await using var context = new AppDbContext();
		var user = await context.users.Where(u => u.username == username).FirstOrDefaultAsync();
		
		if (user == null)
			return NotFound();
		
		using var client = new HttpClient();
		client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bot", Environment.GetEnvironmentVariable("DISCORD_TOKEN"));
		
		var response = await client.GetAsync("https://discord.com/api/v10/users/" + user.discordId);
		if (response.IsSuccessStatusCode)
		{
			var result = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
			var model = new ProfilModel
			{
				username = username,
				avatar = $"https://cdn.discordapp.com/avatars/{user.discordId}/{result.GetProperty("avatar").GetString()}",
				banner = $"https://cdn.discordapp.com/banners/{user.discordId}/{result.GetProperty("banner").GetString()}",
				isAdmin = user.admin
			};
			
			return View(model);
		}
		
		throw new Exception($"Erreur API Discord : {response.StatusCode}");
	}
}