using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using SinafProduction.Data;
using SinafProduction.Data.Entities;

namespace SinafProduction.Components.Pages;

public partial class BlogView : IDisposable
{
	[Parameter] public string Id { get; set; } = string.Empty;
	private readonly CancellationTokenSource cts = new ();
	private Blog? blog;
	
	protected override async Task OnInitializedAsync()
	{
		try
		{
			Loading.Show(this);
			await using var context = new AppDbContext();
			if (ulong.TryParse(Id.Replace(" ", string.Empty), out var id))
				blog = await context.Blogs.Include(x => x.Author).FirstOrDefaultAsync(x => x.Id == id);
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