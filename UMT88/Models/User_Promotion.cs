using System;
using System.Collections.Generic;

namespace UMT88.Models;

public partial class User_Promotion
{
    public long user_promotion_id { get; set; }

    public long user_id { get; set; }

    public long promotion_id { get; set; }

    public string status { get; set; } = null!;

    public DateTime assigned_at { get; set; }

    public DateTime? used_at { get; set; }

    public virtual Promotion promotion { get; set; } = null!;

    public virtual User user { get; set; } = null!;
}
