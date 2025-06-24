using System;
using System.Collections.Generic;

namespace UMT88.Models;

public partial class Promotion
{
    public long promotion_id { get; set; }

    public string name { get; set; } = null!;

    public string? description { get; set; }

    public string type { get; set; } = null!;

    public decimal value { get; set; }

    public DateOnly start_date { get; set; }

    public DateOnly end_date { get; set; }

    public virtual ICollection<User_Promotion> User_Promotions { get; set; } = new List<User_Promotion>();
}
