using Microsoft.EntityFrameworkCore;

namespace SinafProduction.Data;

public partial class AppDbContext
{
	public AppDbContext() { }
	
	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		=> optionsBuilder.UseMySql(
			$"server=sinafproduction.xyz;port=3306;database=web;" +
			$"user={Environment.GetEnvironmentVariable("DB_WEB_USER")};" +
			$"password={Environment.GetEnvironmentVariable("DB_WEB_PASS")};", 
			ServerVersion.Parse("10.11.11-mariadb"),
			option => option.EnableRetryOnFailure());
}