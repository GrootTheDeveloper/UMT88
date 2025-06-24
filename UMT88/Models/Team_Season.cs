using System;
using System.Collections.Generic;

namespace UMT88.Models;

public partial class Team_Season
{
    public long team_season_id { get; set; }

    public long team_id { get; set; }

    public long season_id { get; set; }

    public virtual Season season { get; set; } = null!;

    public virtual Team team { get; set; } = null!;
}
