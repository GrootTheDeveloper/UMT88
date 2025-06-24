using System;
using System.Collections.Generic;

namespace UMT88.Models;

public partial class Market_Type
{
    public long market_type_id { get; set; }

    public string code { get; set; } = null!;

    public string name { get; set; } = null!;

    public DateTime created_at { get; set; }

    public virtual ICollection<Market> Markets { get; set; } = new List<Market>();
}
