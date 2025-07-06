using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UMT88.Data;
using UMT88.Models;
using UMT88.ViewModels;

public class AdminUsersController : Controller
{
    private readonly AppDbContext _db;
    public AdminUsersController(AppDbContext db) => _db = db;

    /* ══════════════ LIST ══════════════ */
    public async Task<IActionResult> Index()
    {
        if (HttpContext.Session.GetInt32("RoleId") != 1)
            return Unauthorized();

        var list = await _db.Users
            .Select(u => new AdminUserRowVm(
                u.user_id,
                u.name,
                u.email,
                u.balance,
                u.status,
                u.role_id == 1 ? "admin" : "bettor"))
            .ToListAsync();

        return View(list);
    }

    /* ══════════════ EDIT FORM ══════════════ */
    [HttpGet]
    public async Task<IActionResult> Edit(long id)
    {
        if (HttpContext.Session.GetInt32("RoleId") != 1) return Unauthorized();

        var u = await _db.Users.FindAsync(id);
        if (u == null) return NotFound();

        var vm = new AdminUserEditVm
        {
            Id = u.user_id,
            Name = u.name,
            Email = u.email,
            Status = u.status,
            RoleId = u.role_id
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AdminUserEditVm vm)
    {
        if (HttpContext.Session.GetInt32("RoleId") != 1) return Unauthorized();

        if (!ModelState.IsValid) return View(vm);

        var u = await _db.Users.FindAsync(vm.Id);
        if (u == null) return NotFound();

        u.name = vm.Name.Trim();
        u.email = vm.Email.Trim();
        u.status = vm.Status;
        u.role_id = vm.RoleId;
        u.updated_at = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        TempData["Toast"] = "Đã lưu thay đổi.";
        return RedirectToAction(nameof(Index));
    }

    /* ══════════════ KHÓA / MỞ ══════════════ */
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(long id)
    {
        if (HttpContext.Session.GetInt32("RoleId") != 1) return Unauthorized();

        var u = await _db.Users.FindAsync(id);
        if (u == null) return NotFound();

        u.status = u.status == "suspended" ? "active" : "suspended";
        u.updated_at = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    /* ══════════════ RESET PASSWORD (demo – đặt lại pass=123456) ═════════════ */
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPwd(long id)
    {
        if (HttpContext.Session.GetInt32("RoleId") != 1) return Unauthorized();

        var u = await _db.Users.FindAsync(id);
        if (u == null) return NotFound();

        u.password_hash = "123456";   // TODO: hash!
        u.updated_at = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        TempData["Toast"] = "Đã đặt lại mật khẩu mặc định (123456).";
        return RedirectToAction(nameof(Index));
    }

    /* ══════════════ CREATE – GET ══════════════ */
    [HttpGet]
    public IActionResult Create()
    {
        if (HttpContext.Session.GetInt32("RoleId") != 1) return Unauthorized();
        return View(new AdminUserCreateVm());
    }

    /* ══════════════ CREATE – POST ══════════════ */
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminUserCreateVm vm)
    {
        if (HttpContext.Session.GetInt32("RoleId") != 1) return Unauthorized();

        if (!ModelState.IsValid) return View(vm);

        // kiểm tra trùng e-mail
        if (await _db.Users.AnyAsync(u => u.email == vm.Email.Trim()))
        {
            ModelState.AddModelError("Email", "Email đã tồn tại.");
            return View(vm);
        }

        var user = new User
        {
            name = vm.Name.Trim(),
            email = vm.Email.Trim(),
            password_hash = vm.Password,      // TODO: hash!
            role_id = vm.RoleId,
            balance = vm.InitBalance,
            status = "active",
            created_at = DateTime.UtcNow,
            updated_at = DateTime.UtcNow
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        TempData["Toast"] = "Đã tạo tài khoản mới.";
        return RedirectToAction(nameof(Index));
    }

}
