using FileTypeChecker.Web.Attributes;
using Microsoft.AspNetCore.Mvc;
using SinafProduction.Services;

namespace SinafProduction.Controllers.API;

[AdminAuthorize]
[ApiController]
[Route("api/blogs/{id:int}")]
public class BlogsAPI(IWebHostEnvironment environment) : ControllerBase
{
	[HttpDelete("icon")]
	public IActionResult DeleteIcon(int id)
	{
		var dir = new DirectoryInfo(environment.WebRootPath + $"/blogs/{id}");
		if (!dir.Exists)
			return NotFound();
		
		foreach (var file in dir.EnumerateFiles())
			if (file.Name == "icon.png")
			{
				file.Delete();
				return Ok();
			}
		
		return NoContent();
	}
	
	[HttpPost("icon")]
	public async Task<IActionResult> SendIcon(int id, [AllowImages] IFormFile icon)
	{
		var dir = new DirectoryInfo(environment.WebRootPath + $"/blogs/{id}");
		if (!dir.Exists)
			dir.Create();
		
		await using var stream = new FileStream(Path.Combine(dir.FullName, "icon.png"), FileMode.Create);
		await icon.CopyToAsync(stream);
		
		return Ok();
	}
	
	[HttpGet("images")]
	public IActionResult GetImages(int id)
	{
		var dir = new DirectoryInfo(environment.WebRootPath + $"/blogs/{id}/images");
		if (!dir.Exists)
			return NotFound();
		
		var images = new List<object>();
		foreach (var file in dir.EnumerateFiles())
		{
			var name = file.Name;
			var path = $"/blogs/{id}/images/{name.Replace(" ", "%20")}";
			images.Add(new { name, path });
		}
		
		return Ok(new { images });
	}
	
	[HttpPost("images")]
	[RequestSizeLimit(5242880)]
	public async Task<IActionResult> SendImage(int id, [AllowImages] IFormFile image)
	{
		var dir = new DirectoryInfo(environment.WebRootPath + $"/blogs/{id}/images");
		if (!dir.Exists)
			dir.Create();
		
		// Veille à lui donner un nom unique :
		var imageExtension = Path.GetExtension(image.FileName);
		var imageName = Path.GetFileNameWithoutExtension(image.FileName);
		var imagePath = Path.Combine(dir.FullName, $"{imageName}{imageExtension}");
		var n = 0;
		while (System.IO.File.Exists(imagePath))
			imagePath = Path.Combine(dir.FullName, $"{imageName}_{++n}{imageExtension}");
		
		await using var stream = new FileStream(imagePath, FileMode.Create);
		await image.CopyToAsync(stream);
		
		var name = Path.GetFileName(imagePath);
		var path = $"/blogs/{id}/images/{name.Replace(" ", "%20")}";
		return Ok(new { name, path });
	}
	
	[HttpPut("images/{name}")]
	public async Task<IActionResult> ReplaceImage(int id, string name, [AllowImages] IFormFile image)
	{
		var dir = new DirectoryInfo(environment.WebRootPath + $"/blogs/{id}/images");
		if (!dir.Exists)
			return NotFound();
		
		var path = Path.Combine(dir.FullName, name);
		if (!System.IO.File.Exists(path))
			return NoContent();
		
		await using var stream = new FileStream(path, FileMode.Open);
		await image.CopyToAsync(stream);
		return Ok();
	}
	
	[HttpDelete("images/{name}")]
	public IActionResult DeleteImage(int id, string name)
	{
		var dir = new DirectoryInfo(environment.WebRootPath + $"/blogs/{id}/images");
		if (!dir.Exists)
			return NotFound();
		
		foreach (var file in dir.EnumerateFiles())
			if (file.Name == name)
			{
				file.Delete();
				return Ok();
			}
		
		return NoContent();
	}
}