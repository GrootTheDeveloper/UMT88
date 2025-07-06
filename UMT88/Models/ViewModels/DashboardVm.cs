using System.Collections.Generic;

namespace UMT88.ViewModels;

public class DashboardVm
{
    public string user_name { get; set; } = null!;
    public decimal balance { get; set; }

    public List<LiveVm> featured { get; set; } = new();
    public List<MatchCardVm> others { get; set; } = new();
}
