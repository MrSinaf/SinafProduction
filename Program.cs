using System.Globalization;
using System.Text;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.IdentityModel.Tokens;
using MySqlConnector;
using SinafProduction;
using SinafProduction.Services;

var cultureInfo = new CultureInfo("fr-FR");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

Env.Load();
var env = Environment.GetEnvironmentVariables();
var key = (env["JWT_KEY"] as string)!;
var dbWeb = new MySqlDataSource($"Server=localhost;Port=3306;Database=web;User={env["DB_WEB_USER"]};Password={env["DB_WEB_PASS"]};");

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRouting(options => { options.LowercaseUrls = true; });
builder.Services.AddControllersWithViews();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	   .AddJwtBearer(options =>
		{
			options.Events = new JwtBearerEvents
			{
				OnMessageReceived = context =>
				{
					if (context.Request.Cookies.ContainsKey("jwt"))
						context.Token = context.Request.Cookies["jwt"];
					
					return Task.CompletedTask;
				},
				OnChallenge = context =>
				{
					context.Response.Redirect("/login?returnUrl=" + context.Request.Path);
					context.HandleResponse();
					return Task.CompletedTask;
				},
				OnTokenValidated = async context =>
				{
					if (context.Principal == null)
						context.Fail("Le compte n'a pas pu être récupéré !");
					
					var user = context.Principal!;
					
					await using var connexion = await dbWeb.OpenConnectionAsync();
					await using var commands = new MySqlCommand("SELECT version FROM users WHERE id = @id;", connexion);
					commands.Parameters.AddWithValue("id", user.GetId());
					
					if (await commands.ExecuteScalarAsync() is int version && version == user.GetUserVersion())
						return;
					
					context.Fail("Votre compte a été modifié récemment. Pour des raisons de sécurité, vous devez vous reconnecter.");
				}
			};
			
			options.TokenValidationParameters = new TokenValidationParameters
			{
				ValidateIssuer = true,
				ValidateAudience = true,
				ValidateLifetime = true,
				ValidateIssuerSigningKey = true,
				ValidIssuer = "https://sinafproduction.xyz/",
				ValidAudience = "https://sinafproduction.xyz/",
				IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
			};
		});

builder.Services.Configure<FormOptions>(form => form.MultipartBodyLengthLimit = 5242880);
builder.Services.AddSingleton(_ => new JwtTokenService(key, "https://sinafproduction.xyz/", "https://sinafproduction.xyz/"));
builder.Services.AddSingleton(_ => new DataBase(dbWeb, new MySqlDataSource($"Server=localhost;Port=3306;Database=puss_in_bot;User={env["DB_PUSSINBOT_USER"]};" +
																		   $"Password={env["DB_PUSSINBOT_PASS"]};")));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseStatusCodePagesWithReExecute("/Home/Error", "?statusCode={0}");
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.Run();