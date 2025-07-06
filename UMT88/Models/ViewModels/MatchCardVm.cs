namespace UMT88.ViewModels;

public class MatchCardVm
{
    public long match_id { get; set; }
    public string competition { get; set; } = null!;
    public string home_name { get; set; } = null!;
    public string away_name { get; set; } = null!;
    public decimal handicap_h { get; set; }
}
