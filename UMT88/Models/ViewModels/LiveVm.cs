namespace UMT88.ViewModels;

public class LiveVm
{
    public long match_id { get; set; }
    public string home_name { get; set; } = null!;
    public string away_name { get; set; } = null!;
    public int? home_score { get; set; }
    public int? away_score { get; set; }
    public string status { get; set; } = null!;
    public string? home_logo { get; set; }
    public string? away_logo { get; set; }
}
