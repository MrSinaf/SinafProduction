using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using SinafProduction.Models;
using SinafProduction.Services;

namespace SinafProduction.Controllers;

[Route("user")]
public class UserController(DataBase db) : Controller
{
	[AdminAuthorize]
	public IActionResult Index() => View();
	
	[Route("{username}")]
	public async Task<IActionResult> Profil(string username)
	{
		await using var connection = await db.web.OpenConnectionAsync();
		await using var cmd = new MySqlCommand("SELECT id_discord, admin FROM users WHERE username = @username;", connection);
		cmd.Parameters.AddWithValue("username", username);
		
		await using var reader = await cmd.ExecuteReaderAsync();
		if (await reader.ReadAsync())
		{
			using var client = new HttpClient();
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bot", Environment.GetEnvironmentVariable("DISCORD_TOKEN"));
			
			var idDiscord = reader.GetUInt64(0);
			var response = await client.GetAsync("https://discord.com/api/v10/users/" + idDiscord);
			if (response.IsSuccessStatusCode)
			{
				var result = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
				var model = new ProfilModel
				{
					username = username,
					avatar = $"https://cdn.discordapp.com/avatars/{idDiscord}/{result.GetProperty("avatar").GetString()}",
					banner = $"https://cdn.discordapp.com/banners/{idDiscord}/{result.GetProperty("banner").GetString()}",
					isAdmin = reader.GetBoolean(1)
				};
				
				return View(model);
			}
			
			throw new Exception($"Erreur API Discord : {response.StatusCode}");
		}
		
		return NotFound();
	}
}