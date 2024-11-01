using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using SinafProduction.Models;

namespace SinafProduction.Controllers;

public class HomeController(DataBase db) : Controller
{
    [Route("")]
    public async Task<IActionResult> Index()
    {
        await using var connection = await db.web.OpenConnectionAsync();
        await using var cmd = new MySqlCommand("SELECT b.id, u.username, b.title, b.description, b.created_at FROM blogs b INNER JOIN users u " +
                                               $"ON u.id = b.author_id WHERE b.public = true ORDER BY b.created_at DESC LIMIT 3;", connection);
        await using var reader = await cmd.ExecuteReaderAsync();
        var blogs = new List<BlogModel>();
        while (await reader.ReadAsync())
        {
            blogs.Add(new BlogModel
            {
                id = reader.GetUInt32(0), authorName = reader.GetString(1), title = reader.GetString(2), description = reader.GetString(3),
                createdAt = reader.GetDateTime(4)
            });
        }

        return View(blogs);
    }

    [Route("About")]
    public IActionResult About()
    {
        return View();
    }

    [Route("Ping")]
    [HttpGet]
    public IActionResult Ping()
    {
        return Ok("pong");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(int? statusCode = null)
    {
        ViewData["StatusCode"] = statusCode;

        return View(new ErrorModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}