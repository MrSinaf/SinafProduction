using Microsoft.EntityFrameworkCore;
using SinafProduction.Data;
using SinafProduction.Data.Entities;

namespace SinafProduction.Components.Pages;

public partial class Home
{
	private ProjectRepository? lastRepository;
	private ProjectStar[]? projectStars;
	private Blog[]? lastBlogs;
	
	private bool isLoaded;
	
	protected override async Task OnInitializedAsync()
	{
		await using var context = new AppDbContext();
		lastRepository = await context.ProjectRepositories.Include(x => x.Project).OrderByDescending(x => x.Update)
									  .FirstOrDefaultAsync();
		projectStars = await context.ProjectStars.Include(x => x.Project).ToArrayAsync();
		StateHasChanged();
		
		lastBlogs = await context.Blogs.Where(x => x.IsPublished)
								 .OrderByDescending(x => x.PublishAt)
								 .Include(x => x.Author)
								 .Select(x => new Blog
								 {
									 Id = x.Id,
									 Title = x.Title,
									 Description = x.Description,
								 }).Take(3).ToArrayAsync();
		isLoaded = true;
		StateHasChanged();
	}
}