using System;
using System.Collections.Generic;

namespace SinafProduction.Data.Entities;

public partial class LoginAttempt
{
    public string ip { get; set; } = null!;

    public string username { get; set; } = null!;

    public DateTime date { get; set; }

    public bool success { get; set; }
}
