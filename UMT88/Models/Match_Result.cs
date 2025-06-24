using System;
using System.Collections.Generic;

namespace UMT88.Models;

public partial class Match_Result
{
    public long match_result_id { get; set; }

    public long match_id { get; set; }

    public int home_score { get; set; }

    public int away_score { get; set; }

    public string full_time_result { get; set; } = null!;

    public DateTime created_at { get; set; }

    public virtual Match match { get; set; } = null!;
}
