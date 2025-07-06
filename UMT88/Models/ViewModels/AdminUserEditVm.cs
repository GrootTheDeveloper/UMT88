namespace UMT88.ViewModels;

/// <summary>Dữ liệu form Edit User</summary>
public class AdminUserEditVm
{
    public long Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Status { get; set; } = "active";
    public int RoleId { get; set; }           // 1=admin, 2=bettor
}
