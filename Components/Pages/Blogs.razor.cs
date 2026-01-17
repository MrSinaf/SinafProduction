using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using SinafProduction.Data;
using SinafProduction.Data.Entities;

namespace SinafProduction.Components.Pages;

public partial class Blogs : ComponentBase, IDisposable
{
	private readonly CancellationTokenSource cts = new ();
	
	private List<Blog>? blogs;
	private Blog[]? blogsCached;
	
	private string Search
	{
		get;
		set
		{
			field = value;
			blogs = value == string.Empty
				? blogsCached?.ToList()
				: blogsCached?
					.Where(x => (x.Title + x.Description).Contains(value, StringComparison.OrdinalIgnoreCase)).ToList();
			
			StateHasChanged();
		}
	} = string.Empty;
	
	protected override async Task OnInitializedAsync()
	{
		try
		{
			Loading.Show(this);
			await using var context = new AppDbContext();
			blogsCached = await context.Blogs.Include(x => x.Author)
									   .Where(x => x.IsPublished)
									   .OrderByDescending(x => x.PublishAt)
									   .Select(x => new Blog
									   {
										   Id = x.Id,
										   Title = x.Title,
										   Description = x.Description,
										   PublishAt = x.PublishAt,
										   Author = x.Author,
									   }).ToArrayAsync();
			Search = string.Empty;
		}
		finally
		{
			Loading.Hide(this);
		}
	}
	
	void IDisposable.Dispose()
	{
		cts.Cancel();
		cts.Dispose();
		GC.SuppressFinalize(this);
	}
}