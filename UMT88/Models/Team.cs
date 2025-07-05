using System;
using System.Collections.Generic;

namespace UMT88.Models;

public partial class Team
{
    public long team_id { get; set; }

    public string name { get; set; } = null!;

    public string? short_name { get; set; }

    public string country { get; set; } = null!;

    public DateTime created_at { get; set; }

    public string? image_url { get; set; }

    public virtual ICollection<League_Standing> League_Standings { get; set; } = new List<League_Standing>();

    public virtual ICollection<Match> Matchaway_teams { get; set; } = new List<Match>();

    public virtual ICollection<Match> Matchhome_teams { get; set; } = new List<Match>();

    public virtual ICollection<Team_Season> Team_Seasons { get; set; } = new List<Team_Season>();
}
