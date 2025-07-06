using System.Collections.Generic;
using UMT88.Models;

namespace UMT88.ViewModels;

public class MarketVm
{
    public string market_type { get; set; } = null!;
    public List<(Selection sel, Odd odds)> items { get; set; } = new();
}

public class MatchDetailVm
{
    public long match_id { get; set; }
    public string home_name { get; set; } = null!;
    public string away_name { get; set; } = null!;
    public string start_time { get; set; } = null!;
    public List<MarketVm> markets { get; set; } = new();
}
