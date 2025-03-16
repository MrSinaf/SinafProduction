using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SinafProduction.Data;
using SinafProduction.Data.Entities;
using SinafProduction.Services;

namespace SinafProduction.Controllers;

[Route("blogs")]
public class BlogsController : Controller
{
	public async Task<IActionResult> Index(string search = "", int page = 1)
	{
		page--;
		const int blogsPerPage = 5;
		
		var isAdmin = User.IsAdmin();
		var userId = User.GetId();
		await using var context = new AppDbContext();
		var query = context.blogs
						   .Include(b => b.author)
						   .Where(b => EF.Functions.Like(b.title, $"%{search}%") || EF.Functions.Like(b.description, $"%{search}%") && (isAdmin || b.authorId == userId || b.isPublic))
						   .OrderByDescending(b => b.createdAt);
		
		var nPages = (int)MathF.Ceiling(1f * await query.CountAsync() / blogsPerPage);
		page = page < 0 ? 0 : page > nPages ? nPages : page;
		
		ViewBag.search = search;
		ViewBag.page = page + 1;
		ViewBag.nPages = nPages;
		
		return View(await query.Skip(page * blogsPerPage).Take(blogsPerPage).ToListAsync());
	}
	
	[Route("{id:int}")]
	public async Task<IActionResult> Blog(uint id)
	{
		await using var context = new AppDbContext();
		var blog = await context.blogs.Include(b => b.author).FirstOrDefaultAsync(b => b.id == id);
		
		if (blog == null && !blog.isPublic && blog.authorId != User.GetId() && !User.IsAdmin())
			return NotFound();
		
		return View(blog);
	}
	
	[Authorize]
	[HttpGet]
	[Route("edit/{id:int?}")]
	public async Task<IActionResult> Edit(uint? id)
	{
		await using var context = new AppDbContext();
		if (id.HasValue)
		{
			var blog = await context.blogs.FindAsync(id);
			if (blog == null)
				return NotFound();
			
			return View(blog);
		}
		
		// Si l'id n'est pas donné alors créer un nouveau Blog 👈(ﾟヮﾟ👈)
		do
		{
			id = (uint)Random.Shared.Next(100000000, 999999999);
		} while (await context.blogs.AnyAsync(b => b.id == id));
		
		var newBlog = new Blog { id = id.Value, authorId = User.GetId() };
		context.blogs.Add(newBlog);
		await context.SaveChangesAsync();
		
		return View(newBlog);
	}
	
	[Authorize]
	[HttpPost]
	[Route("edit/{id?}")]
	public async Task<IActionResult> Edit(Blog model, IFormFile icon)
	{
		if (model.title.Length is < 5 or > 60)
		{
			ViewBag.error = "Il faut que le titre soit entre 5 et 60 characters !";
			return View(model: model);
		}
		
		if (model.description.Length is < 5 or > 180)
		{
			ViewBag.error = "Il faut que la description soit entre 5 et 180 characters !";
			return View(model: model);
		}
		
		await using var context = new AppDbContext();
		var blog = await context.blogs.FindAsync(model.id);
		blog.title = model.title;
		blog.description = model.description;
		blog.isPublic = model.isPublic;
		
		if (blog.isPublic && blog.createdAt == null)
		{
			// Envoi une notification, si le blog est pour la première fois publique :
			await NotifyDiscord(blog.title, blog.description, blog.id);
			blog.createdAt = DateTime.Now;
		}
		
		await context.SaveChangesAsync();
		
		return RedirectToAction("Blog", new { model.id });
	}
	
	
	private static async Task NotifyDiscord(string title, string description, ulong id)
	{
		var payload = JsonSerializer.Serialize(new
			{
				embeds = new[]
				{
					new
					{
						title, description,
						url = $"https://sinafproduction.xyz/blogs/{id}",
						color = 0x00ff00,
						thumbnail = new { url = $"https://sinafproduction.xyz/blogs/{id}/icon.png" },
						footer = new { text = "Un blog vient tout juste d'être publié sur le site !" }
					}
				}
			}
		);
		
		var content = new StringContent(payload, Encoding.UTF8, "application/json");
		await new HttpClient().PostAsync(Environment.GetEnvironmentVariable("DISCORD_WEBHOOK"), content);
	}
}