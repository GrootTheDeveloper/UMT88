namespace UMT88.ViewModels.AdminMatches;

public class MatchEditVm
{
    /* Thông tin chung */
    public long MatchId { get; init; }
    public string Competition { get; init; } = "";
    public string HomeTeam { get; init; } = "";
    public string AwayTeam { get; init; } = "";
    public string StartTime { get; init; } = "";
    public string Status { get; init; } = "";

    /* Danh sách Market */
    public List<MarketVm> Markets { get; init; } = [];
}
