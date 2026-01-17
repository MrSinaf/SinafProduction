using System;
using System.Collections.Generic;

namespace SinafProduction.Data.Entities;

public partial class Blog
{
    public ulong Id { get; set; }

    public ulong AuthorId { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string Content { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public bool IsPublished { get; set; }

    public DateOnly PublishAt { get; set; }

    public virtual User Author { get; set; } = null!;
}
