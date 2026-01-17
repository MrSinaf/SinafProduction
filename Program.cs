using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SinafProduction.Components;
using SinafProduction.Data;
using SinafProduction.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddRazorComponents().AddInteractiveServerComponents(options => { options.DetailedErrors = true; });
builder.Services.AddCascadingAuthenticationState();

// Scope service //
builder.Services.AddScoped<LoadingService>();
builder.Services.AddScoped<AppDbContext>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
	var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY");
	if (jwtKey == null)
		throw new NullReferenceException("Impossible de trouver la variable d'environnement 'JWT_KEY' !");
	
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = true,
		ValidateAudience = true,
		ValidateLifetime = true,
		ValidateIssuerSigningKey = true,
		ValidIssuer = "sinafproduction",
		IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
	};
});
builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/error", true);
	app.UseStatusCodePagesWithReExecute("/error");  // Ajoutez cette ligne
	app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.UseMiddleware<JwtCookieAuthenticationMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.MapControllers();
app.MapRazorPages();

app.Run();