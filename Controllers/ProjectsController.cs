using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace SinafProduction.Controllers;

[Route("projects")]
public class ProjectsController(DataBase db) : Controller
{
	public IActionResult Index() => View();
	
	[Route("XYEngine")]
	public IActionResult XYEngine() => View();
	
	[Route("PussInBot")]
	public async Task<IActionResult> PussInBot()
	{
		await using var connection = await db.pussInBot.OpenConnectionAsync();
		await using var cmd = new MySqlCommand("SELECT COUNT(*) FROM guilds WHERE true;", connection);
		ViewBag.nServer = (long)(await cmd.ExecuteScalarAsync() ?? 0);
		
		return View();
	}
	
	[Route("CDF")]
	public IActionResult CDF() => View();
	
	[Route("MinesweeperBasic")]
	public IActionResult MinesweeperBasic() => View();
	
	[Route("snake-js")]
	public IActionResult SnakeJS() => View();
}