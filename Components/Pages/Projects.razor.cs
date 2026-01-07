using Microsoft.EntityFrameworkCore;
using SinafProduction.Data;
using SinafProduction.Data.Entities;

namespace SinafProduction.Components.Pages;

public partial class Projects : IDisposable
{
	private readonly CancellationTokenSource cts = new ();
	private Project[]? projects;
	
	protected override async Task OnInitializedAsync()
	{
		try
		{
			Loading.Show(this);
			await using var context = new AppDbContext();
			projects = await context.Projects.Include(x => x.Tag).ToArrayAsync();
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