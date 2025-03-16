using System.Security.Claims;

namespace SinafProduction.Services;

public static class UserExtends
{
	public static ulong GetId(this ClaimsPrincipal user)
		=> ulong.TryParse(user.Claims.FirstOrDefault(c => c.Type == "id")?.Value, out var userId) ? userId : 0;
	
	public static bool IsAdmin(this ClaimsPrincipal user)
		=> bool.TryParse(user.Claims.FirstOrDefault(c => c.Type == "admin")?.Value, out var isAdmin) && isAdmin;
	
	public static string GetName(this ClaimsPrincipal user)
		=> user.Claims.FirstOrDefault(c => c.Type == "username")?.Value ?? "";
	
	public static int GetUserVersion(this ClaimsPrincipal user)
		=> int.TryParse(user.Claims.FirstOrDefault(c => c.Type == "version")?.Value, out var version) ? version : 0;
}