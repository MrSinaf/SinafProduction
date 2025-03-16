using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace SinafProduction.Controllers;

[Route("projects")]
public class ProjectsController : Controller
{
	public IActionResult Index() => View();
	
	[Route("XYEngine")]
	public IActionResult XYEngine() => View();
	
	[Route("PussInBot")]
	public IActionResult PussInBot() => View();
	
	[Route("CDF")]
	public IActionResult CDF() => View();
	
	[Route("MinesweeperBasic")]
	public IActionResult MinesweeperBasic() => View();
	
	[Route("snake-js")]
	public IActionResult SnakeJS() => View();
}