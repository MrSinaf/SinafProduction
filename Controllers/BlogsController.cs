using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SinafProduction.Models;
using SinafProduction.Services;
using MySqlConnector;

namespace SinafProduction.Controllers;

[Route("blogs")]
public class BlogsController(DataBase db) : Controller
{
    public async Task<IActionResult> Index(string search = "", int page = 1)
    {
        page--;
        const int blogsPerPage = 5;
        await using var connection = await db.web.OpenConnectionAsync();
        await using var cmd = new MySqlCommand("SELECT COUNT(*) FROM blogs b INNER JOIN users u ON u.id = b.author_id " +
                                               "WHERE (title LIKE @search OR description LIKE @search) AND (@isAdmin OR b.author_id = @id OR  b.public = true);",
                                               connection);
        cmd.Parameters.AddWithValue("id", User.GetId());
        cmd.Parameters.AddWithValue("search", $"%{search}%");
        cmd.Parameters.AddWithValue("isAdmin", User.IsAdmin());
        var nPages = (int)MathF.Ceiling(1f * (long)(await cmd.ExecuteScalarAsync() ?? 0) / blogsPerPage);
        page = page < 0 ? 0 : page > nPages ? nPages : page;

        cmd.CommandText = "SELECT b.id, u.username, b.author_id, b.title, b.created_at, b.public, b.description FROM blogs b INNER JOIN users u ON u.id = b.author_id " +
                          "WHERE (title LIKE @search OR description LIKE @search) AND (@isAdmin OR b.author_id = @id OR b.public = true) " +
                          $"ORDER BY b.created_at DESC LIMIT {blogsPerPage} OFFSET {page * blogsPerPage};";
        await using var reader = await cmd.ExecuteReaderAsync();
        var blogs = new List<BlogModel>();
        while (await reader.ReadAsync())
        {
            blogs.Add(new BlogModel
            {
                id = reader.GetUInt32(0),
                authorName = reader.GetString(1),
                authorId = reader.GetUInt64(2),
                title = reader.GetString(3),
                createdAt = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                isPublic = reader.GetBoolean(5),
                description = reader.GetString(6)
            });
        }

        ViewBag.search = search;
        ViewBag.page = page + 1;
        ViewBag.nPages = nPages;
        return View(blogs);
    }

    [Route("{id:int}")]
    public async Task<IActionResult> Blog(uint id)
    {
        await using var connection = await db.web.OpenConnectionAsync();
        await using var cmd = new MySqlCommand("SELECT b.id, u.username, b.author_id, b.title, b.content, b.created_at FROM blogs b INNER JOIN users u " +
                                               "ON u.id = b.author_id WHERE b.id = @id AND (@isAdmin OR b.author_id = @userId OR b.public = true);", connection);
        cmd.Parameters.AddWithValue("id", id);
        cmd.Parameters.AddWithValue("userId", User.GetId());
        cmd.Parameters.AddWithValue("isAdmin", User.IsAdmin());
        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
            return View(new BlogModel
            {
                id = reader.GetUInt32(0),
                authorName = reader.GetString(1),
                authorId = reader.GetUInt64(2),
                title = reader.GetString(3),
                content = reader.GetString(4),
                createdAt = reader.IsDBNull(5) ? null : reader.GetDateTime(5)
            });

        return NotFound();
    }

    [Authorize, HttpGet, Route("edit/{id:int?}")]
    public async Task<IActionResult> Edit(uint? id)
    {
        await using var connection = await db.web.OpenConnectionAsync();

        if (id.HasValue)
        {
            await using var cmd = new MySqlCommand("SELECT Count(*) FROM blogs WHERE id = @id", connection);
            cmd.CommandText = "SELECT id, author_id, title, description, content, public FROM blogs WHERE id = @id;";
            cmd.Parameters.AddWithValue("id", id);
            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
                return View(new BlogModel
                {
                    id = reader.GetUInt32(0),
                    authorId = reader.GetUInt64(1),
                    title = reader.GetString(2),
                    description = reader.GetString(3),
                    content = reader.GetString(4),
                    isPublic = reader.GetBoolean(5)
                });

            return NotFound();
        }
        else
        {
            await using var cmd = new MySqlCommand("SELECT Count(*) FROM blogs WHERE id = @id", connection);
            var param = new MySqlParameter("id", MySqlDbType.UInt32);
            cmd.Parameters.Add(param);
            do
            {
                id = (uint)Random.Shared.Next(100000000, 999999999);
                param.Value = id;
            } while ((long)(await cmd.ExecuteScalarAsync())! != 0);

            cmd.CommandText = "INSERT INTO blogs (id, author_id) VALUES (@id, @authorId)";
            cmd.Parameters.AddWithValue("authorId", User.GetId());
            await cmd.ExecuteNonQueryAsync();
                
            return View(new BlogModel {id = id.Value});
        }
    }
    
    [Authorize, HttpPost, Route("edit/{id?}")]
    public async Task<IActionResult> Edit(BlogModel model, IFormFile icon)
    {
        if (model.title == null || model.title.Length is < 5 or > 60)
        {
            ViewBag.error = "Il faut que le titre soit entre 5 et 60 characters !";
            return View(model: model);
        }

        if (model.description == null || model.description.Length is < 5 or > 180)
        {
            ViewBag.error = "Il faut que la description soit entre 5 et 180 characters !";
            return View(model: model);
        }

        await using var connection = await db.web.OpenConnectionAsync();
        await using var cmd = new MySqlCommand(commandText: "SELECT created_at FROM blogs WHERE id = @id;", connection: connection);
        cmd.Parameters.AddWithValue(parameterName: "id", value: model.id);
        var cmdResult = await cmd.ExecuteScalarAsync();
        var blogExist = cmdResult != null;

        cmd.CommandText = blogExist
                              ? "UPDATE blogs SET author_id = @authorId, title = @title, description = @description, public = @public, content = @content WHERE id = @id;"
                              : "INSERT INTO blogs (id, author_id, title, description, content, public) " +
                                "VALUES (@id, @authorId, @title, @description, @content, @public);";
        cmd.Parameters.AddWithValue(parameterName: "authorId", value: User.GetId());
        cmd.Parameters.AddWithValue(parameterName: "title", value: model.title);
        cmd.Parameters.AddWithValue(parameterName: "description", value: model.description);
        cmd.Parameters.AddWithValue(parameterName: "content", value: model.content);
        cmd.Parameters.AddWithValue(parameterName: "public", value: model.isPublic);
        await cmd.ExecuteNonQueryAsync();
        
        // Envoi une notification, si le blog est pour la première fois publique :
        if (model.isPublic && cmdResult is DBNull)
        {
            await NotifyDiscord(title: model.title, description: model.description, id: model.id);
            cmd.CommandText = $"UPDATE blogs SET created_at = current_timestamp WHERE id = {model.id};";
            cmd.ExecuteNonQuery();
        }

        return RedirectToAction(actionName: "Blog", routeValues: new { model.id });
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