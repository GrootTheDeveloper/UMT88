namespace UMT88.ViewModels.AdminMatches;

/// <summary>Dòng hiển thị ở bảng trận đấu</summary>
public record MatchRowVm
(
    long MatchId,
    string Competition,
    string StartTime,        // "dd/MM HH:mm"
    string Home,
    string Away,
    string Status            // scheduled / live / finished / cancelled
);
