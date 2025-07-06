using System;
using System.Collections.Generic;

namespace UMT88.Models;

public partial class Match
{
    public long match_id { get; set; }

    public long season_id { get; set; }

    public long home_team_id { get; set; }

    public long away_team_id { get; set; }

    public DateTime start_time { get; set; }

    public string status { get; set; } = null!;

    public DateTime created_at { get; set; }

    public virtual ICollection<Market> Markets { get; set; } = new List<Market>();

    public virtual ICollection<MatchEvent> MatchEvents { get; set; } = new List<MatchEvent>();

    public virtual ICollection<Match_Result> Match_Results { get; set; } = new List<Match_Result>();

    public virtual Team away_team { get; set; } = null!;

    public virtual Team home_team { get; set; } = null!;

    public virtual Season season { get; set; } = null!;
}
