using System;
using System.Collections.Generic;

namespace UMT88.Models;

public partial class Bet
{
    public long bet_id { get; set; }

    public long user_id { get; set; }

    public decimal stake_amount { get; set; }

    public decimal potential_payout { get; set; }

    public string status { get; set; } = null!;

    public DateTime placed_at { get; set; }

    public DateTime? settled_at { get; set; }

    public virtual ICollection<Bet_Item> Bet_Items { get; set; } = new List<Bet_Item>();

    public virtual User user { get; set; } = null!;
}
