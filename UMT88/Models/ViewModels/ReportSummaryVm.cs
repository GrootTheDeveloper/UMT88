// ViewModels/ReportSummaryVm.cs
namespace UMT88.ViewModels;

/// <summary>Dữ liệu hiển thị (và export) trên báo cáo</summary>
public class ReportSummaryVm
{
    public decimal TotalStake { get; init; }
    public decimal TotalPayout { get; init; }
    public double ProfitPercent => TotalStake == 0 ? 0
                                  : (double)((TotalStake - TotalPayout) / TotalStake * 100);

    /* dùng 2 View-Model mới tạo */
    public List<TopMatchVm> TopMatches { get; init; } = new();
    public List<TopUserVm> TopUsers { get; init; } = new();
}
