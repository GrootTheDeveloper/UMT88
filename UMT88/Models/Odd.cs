using System;
using System.Collections.Generic;

namespace UMT88.Models;

public partial class Odd
{
    public long odds_id { get; set; }

    public long selection_id { get; set; }

    public decimal odds_value { get; set; }

    public DateTime created_at { get; set; }

    public virtual Selection selection { get; set; } = null!;
}
