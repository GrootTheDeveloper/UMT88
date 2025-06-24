using System;
using System.Collections.Generic;

namespace UMT88.Models;

public partial class Competition
{
    public long competition_id { get; set; }

    public string name { get; set; } = null!;

    public string country { get; set; } = null!;

    public int? level { get; set; }

    public DateTime created_at { get; set; }

    public virtual ICollection<Season> Seasons { get; set; } = new List<Season>();
}
