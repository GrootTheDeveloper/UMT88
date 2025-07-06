using System;
using System.Collections.Generic;

namespace UMT88.Models;

public partial class League_Standing
{
    public long league_standing_id { get; set; }

    public long season_id { get; set; }

    public long team_id { get; set; }

    public int matches_played { get; set; }

    public int wins { get; set; }

    public int draws { get; set; }

    public int losses { get; set; }

    public int points { get; set; }

    public int goal_difference { get; set; }

    public DateTime last_updated { get; set; }

    public int goals_for { get; set; }

    public int goals_against { get; set; }

    public virtual Season season { get; set; } = null!;

    public virtual Team team { get; set; } = null!;
}
