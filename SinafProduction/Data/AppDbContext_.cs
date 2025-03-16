using Microsoft.EntityFrameworkCore;

namespace SinafProduction.Data;

public partial class AppDbContext
{
	public AppDbContext() { }
	
	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		var user = Environment.GetEnvironmentVariable("DB_WEB_USER");
		var password = Environment.GetEnvironmentVariable("DB_WEB_PASS");
		optionsBuilder.UseMySql($"server=localhost;database=web;user={user};password={password}", ServerVersion.Parse("10.11.6-mariadb"));
	}
}