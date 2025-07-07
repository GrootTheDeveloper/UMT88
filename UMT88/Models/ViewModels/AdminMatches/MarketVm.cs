namespace UMT88.ViewModels.AdminMatches;

/// <summary>Market với 2 selection cố định (AH hoặc OU)</summary>
public class MarketVm
{
    public long MarketId { get; init; }
    public string Type { get; init; } = "";      // AH / OU
    public string Status { get; init; } = "open";  // open / closed

    public long Sel1Id { get; init; }
    public string Sel1 { get; set; } = "";
    public decimal Odds1 { get; set; }

    public long Sel2Id { get; init; }
    public string Sel2 { get; set; } = "";
    public decimal Odds2 { get; set; }
}
