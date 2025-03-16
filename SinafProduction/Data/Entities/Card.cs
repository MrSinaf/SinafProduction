using System;
using System.Collections.Generic;

namespace SinafProduction.Data.Entities;

public partial class Card
{
    public ulong id { get; set; }

    public ulong boardId { get; set; }

    public string title { get; set; } = null!;

    public string content { get; set; } = null!;

    public sbyte priority { get; set; }

    public sbyte state { get; set; }

    public DateTime date { get; set; }

    public virtual Board board { get; set; } = null!;

    public virtual ICollection<CardTask> cardTasks { get; set; } = new List<CardTask>();
}
