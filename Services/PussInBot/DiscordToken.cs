using System.Text.Json.Serialization;

namespace SinafProduction.Services.PussInBot;

public class DiscordToken
{
    [JsonPropertyName("token_type")]
    public string type { get; set; }

    [JsonPropertyName("access_token")]
    public string token { get; set; }

    [JsonPropertyName("expires_in")]
    public int expiresIn { get; set; }

    [JsonPropertyName("refresh_token")]
    public string refreshToken { get; set; }

    [JsonPropertyName("scope")]
    public string scope { get; set; }
}