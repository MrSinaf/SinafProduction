namespace SinafProduction.Models;

public class BlogModel
{
	public uint id { get; init; }
	public string authorName { get; init; }
	public ulong authorId { get; init; }
	public string? title { get; set; }
	public string? description { get; set; }
	public string? content { get; set; }
	public bool isPublic { get; set; }
	public DateTime? createdAt { get; init; }
}