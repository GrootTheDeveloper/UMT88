namespace UMT88.ViewModels
{
    public class MatchVm
    {
        public long match_id { get; set; }
        public string competition { get; set; } = null!;
        public string home_name { get; set; } = null!;
        public string away_name { get; set; } = null!;
        public string? home_logo { get; set; }
        public string? away_logo { get; set; }
        public int? home_score { get; set; }
        public int? away_score { get; set; }
        public string status { get; set; } = null!;
    }

    public class DashboardVm
    {
        public string user_name { get; set; } = null!;
        public decimal balance { get; set; }
        public List<MatchVm> featured { get; set; } = new();
        public List<MatchVm> others { get; set; } = new();
    }
}
