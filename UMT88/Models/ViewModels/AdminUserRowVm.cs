namespace UMT88.ViewModels;

public record AdminUserRowVm(
    long Id,
    string Name,
    string Email,
    decimal Balance,
    string Status,          // active | suspended | banned
    string RoleName         // admin | bettor
);
