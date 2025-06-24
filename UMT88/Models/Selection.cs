using System;
using System.Collections.Generic;

namespace UMT88.Models;

public partial class Selection
{
    public long selection_id { get; set; }

    public long market_id { get; set; }

    public string name { get; set; } = null!;

    public string status { get; set; } = null!;

    public DateTime created_at { get; set; }

    public virtual ICollection<Bet_Item> Bet_Items { get; set; } = new List<Bet_Item>();

    public virtual ICollection<Odd> Odds { get; set; } = new List<Odd>();

    public virtual Market market { get; set; } = null!;
}
