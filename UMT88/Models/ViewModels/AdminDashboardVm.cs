namespace UMT88.ViewModels;

/// <summary>Dữ liệu hiển thị trên Admin Dashboard</summary>
public class AdminDashboardVm
{
    /* ô thống kê */
    public int OnlineUsers { get; set; }      // active status + login < 30′ (simple)
    public int NewUsersToday { get; set; }
    public decimal RevenueVnd { get; set; }      // doanh thu = stake thua – payout thắng (VNĐ)
    public int BetsToday { get; set; }

    /* bảng cược gần đây */
    public List<RecentBetRow> RecentBets { get; set; } = new();
    public record RecentBetRow(
        string UserName,
        string MatchName,
        decimal Stake,          // coin
        DateTime Time,
        string Status           // pending / won / lost
    );
}
