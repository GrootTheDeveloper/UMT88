// ViewModels/ReportFilterVm.cs
namespace UMT88.ViewModels;

/// <summary>Điều kiện lọc khi sinh báo cáo</summary>
public record ReportFilterVm
(
    DateTime? From = null,   // Ngày bắt đầu (UTC), null = không giới hạn
    DateTime? To = null    // Ngày kết thúc   (UTC), null = tới hiện tại
);
