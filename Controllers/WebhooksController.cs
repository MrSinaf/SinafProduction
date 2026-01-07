using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SinafProduction.Data;

namespace SinafProduction.Controllers;

[ApiController]
[Route("/webhooks")]
public class WebhooksController : Controller
{
	[HttpPost("github/push")]
	public async Task<IActionResult> NewGithubPush()
	{
		// TODO : Ajouter la vérification de la signature GitHub.
		
		using var reader = new StreamReader(Request.Body);
		var root = JsonDocument.Parse(await reader.ReadToEndAsync()).RootElement;
		
		if (root.GetProperty("sender").GetProperty("login").GetString() != "MrSinaf")
			return Ok("Not MrSinaf");
		
		await using var context = new AppDbContext();
		var repository =
			await context.ProjectRepositories
						 .FirstOrDefaultAsync(p => p.Repository == root.GetProperty("repository")
																	   .GetProperty("name").GetString());
		if (repository == null)
			return Ok("Repository not found in SinafProduction");
		
		var headerCommit = root.GetProperty("head_commit");
		var commit = headerCommit.GetProperty("message").GetString()!;
		
		repository.Update = DateTime.Now;
		repository.Branch = root.GetProperty("ref").GetString()!.Replace("refs/heads/", "");
		repository.Commit = (commit.Length > 64 ? commit[..64] : commit).Split('\n')[0];
		repository.Added = (uint)headerCommit.GetProperty("added").GetArrayLength();
		repository.Removed = (uint)headerCommit.GetProperty("removed").GetArrayLength();
		repository.Modified = (uint)headerCommit.GetProperty("modified").GetArrayLength();
		await context.SaveChangesAsync();
		
		return Ok();
	}
}