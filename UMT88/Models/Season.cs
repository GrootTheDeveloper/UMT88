using System;
using System.Collections.Generic;

namespace UMT88.Models;

public partial class Season
{
    public long season_id { get; set; }

    public long competition_id { get; set; }

    public string name { get; set; } = null!;

    public DateOnly start_date { get; set; }

    public DateOnly end_date { get; set; }

    public DateTime created_at { get; set; }

    public virtual ICollection<League_Standing> League_Standings { get; set; } = new List<League_Standing>();

    public virtual ICollection<Match> Matches { get; set; } = new List<Match>();

    public virtual ICollection<Team_Season> Team_Seasons { get; set; } = new List<Team_Season>();

    public virtual Competition competition { get; set; } = null!;
}
