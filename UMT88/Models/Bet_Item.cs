using System;
using System.Collections.Generic;

namespace UMT88.Models;

public partial class Bet_Item
{
    public long bet_item_id { get; set; }

    public long bet_id { get; set; }

    public long selection_id { get; set; }

    public decimal odds_at_placement { get; set; }

    public string result { get; set; } = null!;

    public virtual Bet bet { get; set; } = null!;

    public virtual Selection selection { get; set; } = null!;
}
