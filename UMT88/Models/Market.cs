using System;
using System.Collections.Generic;

namespace UMT88.Models;

public partial class Market
{
    public long market_id { get; set; }

    public long match_id { get; set; }

    public long market_type_id { get; set; }

    public string status { get; set; } = null!;

    public DateTime created_at { get; set; }

    public virtual ICollection<Selection> Selections { get; set; } = new List<Selection>();

    public virtual Market_Type market_type { get; set; } = null!;

    public virtual Match match { get; set; } = null!;
}
