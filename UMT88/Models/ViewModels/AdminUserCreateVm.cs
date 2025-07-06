namespace UMT88.ViewModels;

/// <summary>Form tạo user mới</summary>
public class AdminUserCreateVm
{
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public int RoleId { get; set; } = 2;          // 1 = admin, 2 = bettor
    public decimal InitBalance { get; set; } = 0;
}
