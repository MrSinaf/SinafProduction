using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using SinafProduction.Models;
using SinafProduction.Services.PussInBot;
using System.Text.Json;

namespace SinafProduction.Controllers;

[Route("projects/pussinbot")]
public class PussInBotController(DataBase db) : Controller
{
    private const string REDIRECT_URI = "https://sinafproduction.xyz/pussinbot/role-access";
    
    public async Task<IActionResult> Index()
    {
        await using var connection = await db.pussInBot.OpenConnectionAsync();
        await using var command = new MySqlCommand("SELECT COUNT(*) FROM guilds;", connection);

        return View(new PussInBotModel { nServeur = Convert.ToInt32(await command.ExecuteScalarAsync()) });
    }

    [HttpGet]
    [Route("role-access")]
    public async Task<IActionResult> LinkedRole(string code)
    {
        if (string.IsNullOrEmpty(code))
            return BadRequest("Code de Discord manquant.");

        var clientId = Environment.GetEnvironmentVariable("DISCORD_CLIENT_ID");
        var clientSecret = Environment.GetEnvironmentVariable("DISCORD_CLIENT_SECRET");
        
        using var httpClient = new HttpClient();
        // Récupère le token :
        var responseToken = await httpClient.PostAsync("https://discord.com/api/v10/oauth2/token", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "client_id", clientId },
            { "client_secret", clientSecret },
            { "grant_type", "authorization_code" },
            { "code", code },
            { "redirect_uri", REDIRECT_URI }
        }));
        var tokenString = await responseToken.Content.ReadAsStringAsync();

        if (!responseToken.IsSuccessStatusCode)
            return BadRequest("Échec de l'obtention du jeton d'accès.");

        var dToken = JsonSerializer.Deserialize<DiscordToken>(tokenString);

        // Utilise le token pour obtenir les informations de User :
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(dToken.type, dToken.token);
        responseToken = await httpClient.GetAsync("https://discord.com/api/v10/users/@me");
        var userString = await responseToken.Content.ReadAsStringAsync();

        if (!responseToken.IsSuccessStatusCode)
            return BadRequest("Échec de la vérification des informations utilisateur.");


        var elements = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(userString);
        var userId = elements["id"].GetUInt64();

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", dToken.token);

        await using var connection = await db.pussInBot.OpenConnectionAsync();
        await using var cmd = new MySqlCommand("UPDATE users SET token = @token WHERE id = @id;" +
                                               "SELECT xp FROM users WHERE id = @id;", connection);
        cmd.Parameters.AddWithValue("id", userId);
        cmd.Parameters.AddWithValue("token", tokenString);
        var result = (uint)(await cmd.ExecuteScalarAsync() ?? 0);

        var metadata = new Dictionary<string, object>
        {
            { "levels", result / 6000 }
        };

        var metadataRequest = new
        {
            platform_name = "Puss In Bot",
            metadata
        };

        var resMeta = await httpClient.PutAsync($"https://discord.com/api/v10/users/@me/applications/{clientId}/role-connection",
                                                new StringContent(JsonSerializer.Serialize(metadataRequest), Encoding.UTF8, "application/json"));

        if (!resMeta.IsSuccessStatusCode)
            return BadRequest($"Échec de la mise à jour des métadonnées. Réponse : {tokenString}");

        return View("Success");
    }

    [HttpGet]
    [Route("authorize")]
    public IActionResult Authorize()
    {
        var clientId = Environment.GetEnvironmentVariable("DISCORD_CLIENT_ID");
        const string scope = "identify role_connections.write";

        return Redirect($"https://discord.com/api/oauth2/authorize?client_id={clientId}&redirect_uri={REDIRECT_URI}&response_type=code&scope={scope}");
    }
}