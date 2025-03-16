using System;
using System.Collections.Generic;

namespace SinafProduction.Data.Entities;

public partial class Board
{
    public ulong id { get; set; }

    public ulong projectId { get; set; }

    public string uniqueName { get; set; } = null!;

    public string name { get; set; } = null!;

    public string description { get; set; } = null!;

    public virtual ICollection<Card> cards { get; set; } = new List<Card>();

    public virtual Project project { get; set; } = null!;
}
