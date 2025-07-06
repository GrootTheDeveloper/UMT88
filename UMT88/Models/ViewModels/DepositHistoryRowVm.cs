namespace UMT88.ViewModels;
public record DepositHistoryRowVm
(
    DateTime Date,
    decimal AmountVnd,
    bool IsDeposit               // true: nạp, false: rút
)
{
    public string Sign => IsDeposit ? "+" : "-";
    public string Color => IsDeposit ? "text-green-600" : "text-red-600";
    public string Label => IsDeposit ? "(Nạp)" : "(Rút)";
}