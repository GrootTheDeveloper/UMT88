using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UMT88.Data;
using UMT88.Models;
using UMT88.ViewModels;

namespace UMT88.Controllers;

public class DepositController : Controller
{
    private readonly AppDbContext _db;
    private const decimal Rate = 10000m;      // 10 000 VNĐ = 1 xu

    public DepositController(AppDbContext db) => _db = db;

    /* ------------ GET ------------- */
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        long uid = HttpContext.Session.GetInt32("UserId") ?? 0;
        if (uid == 0) return RedirectToAction("Login", "Auth");

        var user = await _db.Users.FindAsync(uid);
        var banks = new List<string> { "MB Bank", "Vietcombank", "Techcombank" };

        // lấy 20 giao dịch gần nhất từ 2 bảng
        var dep = await _db.Deposit_Requests.Where(r => r.user_id == uid && r.status == "approved")
                    .Select(r => new DepositHistoryRowVm(r.requested_at, r.amount * Rate, true))
                    .ToListAsync();
        var wit = await _db.Withdraw_Requests.Where(r => r.user_id == uid && r.status == "approved")
                    .Select(r => new DepositHistoryRowVm(r.requested_at, r.amount * Rate, false))
                    .ToListAsync();
        var history = dep.Concat(wit)
                         .OrderByDescending(x => x.Date)
                         .Take(20)
                         .ToList();

        // Lấy pendingDepositId từ TempData["PayId"]
        long? pendingDepositId = null;
        if (TempData.ContainsKey("PayId"))
        {
            if (long.TryParse(TempData["PayId"] as string, out var pid))
                pendingDepositId = pid;
        }

        var vm = new DepositFormVm(
            user.balance,
            banks,
            history,
            pendingDepositId   // null nếu không có giao dịch chờ
        );
        return View(vm);
    }

    /* ------------ POST (gửi form) ------------- */
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(decimal amountVnd, string bank)
    {
        long uid = HttpContext.Session.GetInt32("UserId") ?? 0;
        if (uid == 0) return Unauthorized();

        if (amountVnd < 10000)          // tối thiểu 10k
        {
            TempData["Toast"] = "Số tiền tối thiểu là 10 000 VNĐ";
            return RedirectToAction(nameof(Index));
        }

        var xu = Math.Round(amountVnd / Rate, 2);

        var req = new Deposit_Request
        {
            user_id = uid,
            amount = xu,
            payment_method = bank,
            status = "pending",
            requested_at = DateTime.UtcNow
        };
        _db.Deposit_Requests.Add(req);
        await _db.SaveChangesAsync();

        // Kích hoạt modal QR
        TempData["ShowQr"] = true;
        TempData["PayId"] = req.deposit_id.ToString();   // Lưu dưới dạng string!
        TempData["Toast"] = "Quét QR để thanh toán";

        return RedirectToAction(nameof(Index));
    }

    /* ------------ AJAX CONFIRM (mock sau 10s) ------------- */
    [HttpPost]
    public async Task<IActionResult> Confirm([FromBody] ConfirmDepositRequest dto)
    {
        var dr = await _db.Deposit_Requests.FindAsync(dto.DepositId);
        if (dr == null || dr.status != "pending")
            return BadRequest();

        dr.status = "approved";
        dr.processed_at = DateTime.UtcNow;

        var user = await _db.Users.FindAsync(dr.user_id);
        user.balance += dr.amount;

        _db.Transactions.Add(new Transaction
        {
            user_id = user.user_id,
            transaction_type = "deposit",
            amount = dr.amount,
            balance_after = user.balance,
            created_at = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        return Ok(new { newBalance = user.balance });
    }
}
