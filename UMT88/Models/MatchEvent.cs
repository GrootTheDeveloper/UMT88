using System;
using System.Collections.Generic;

namespace UMT88.Models;

public partial class MatchEvent
{
    public long event_id { get; set; }

    public long match_id { get; set; }

    public long team_id { get; set; }

    public DateTime event_time { get; set; }

    public string event_type { get; set; } = null!;

    public string? description { get; set; }

    public DateTime created_at { get; set; }

    public virtual Match match { get; set; } = null!;

    public virtual Team team { get; set; } = null!;
}
