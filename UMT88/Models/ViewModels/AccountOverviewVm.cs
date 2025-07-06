namespace UMT88.ViewModels;

public class BetBriefVm
{
    public long BetId { get; set; }
    public string MarketType { get; set; } = "";
    public string Selection { get; set; } = "";
    public decimal Stake { get; set; }
    public decimal Payout { get; set; }
    public string Status { get; set; } = "";
    public DateTime PlacedAt { get; set; }
}

public class AccountOverviewVm
{
    /* Thông tin cơ bản */
    public string UserName { get; set; } = "";
    public string Email { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public decimal Balance { get; set; }

    /* Thống kê tháng hiện tại */
    public decimal MonthDeposit { get; set; }
    public decimal MonthSpend { get; set; }

    /* Lịch sử cược (giới hạn n dòng) */
    public List<BetBriefVm> RecentBets { get; set; } = new();
}
