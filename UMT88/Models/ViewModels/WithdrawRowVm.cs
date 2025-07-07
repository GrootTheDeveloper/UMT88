// ViewModels/WithdrawRowVm.cs
namespace UMT88.ViewModels;

public record WithdrawRowVm(
    long Id,
    string User,
    decimal Amount,
    string BankInfo,
    DateTime RequestedAt,
    string Status
);
