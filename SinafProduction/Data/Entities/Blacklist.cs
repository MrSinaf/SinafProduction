using System;
using System.Collections.Generic;

namespace SinafProduction.Data.Entities;

public partial class Blacklist
{
    public string ip { get; set; } = null!;

    public string reason { get; set; } = null!;
}
