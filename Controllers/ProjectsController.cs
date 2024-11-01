using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using SinafProduction.Models;

namespace SinafProduction.Controllers;

[Route("projects")]
public class ProjectsController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    [Route("snake-js")]
    public IActionResult SnakeJS()
    {
        return View();
    }
}