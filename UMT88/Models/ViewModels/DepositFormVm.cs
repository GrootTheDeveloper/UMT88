namespace UMT88.ViewModels;
public record DepositFormVm
(
    decimal Balance,
    List<string> Banks,
    List<DepositHistoryRowVm> History,
    long? PendingDepositId  = null   // id để JS confirm (null nếu chưa nạp)
);