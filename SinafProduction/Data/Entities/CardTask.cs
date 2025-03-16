using System;
using System.Collections.Generic;

namespace SinafProduction.Data.Entities;

public partial class CardTask
{
    public ulong id { get; set; }

    public ulong cardId { get; set; }

    public string content { get; set; } = null!;

    public bool completed { get; set; }

    public virtual Card card { get; set; } = null!;
}
