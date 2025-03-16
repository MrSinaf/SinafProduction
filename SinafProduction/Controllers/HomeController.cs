using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SinafProduction.Data;
using SinafProduction.Models;

namespace SinafProduction.Controllers;

public class HomeController : Controller
{
	[Route("")]
	public async Task<IActionResult> Index()
	{
		await using var context = new AppDbContext();
		var blogs = context.blogs.Include(b => b.author).Where(b => b.isPublic).OrderByDescending(b => b.createdAt).Take(3);
		return View(blogs.ToList());
	}
	
	[Route("About")]
	public IActionResult About() => View();
	
	[Route("Ping")]
	[HttpGet]
	public IActionResult Ping() => Ok("pong");
	
	[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
	public IActionResult Error(int? statusCode = null)
	{
		ViewData["StatusCode"] = statusCode;
		
		return View(new ErrorModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
	}
}