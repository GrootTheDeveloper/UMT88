using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UMT88.Data;
using UMT88.Models;
using UMT88.ViewModels;

namespace UMT88.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext _db;
    private readonly ILogger<AccountController> _log;
    public AccountController(AppDbContext db, ILogger<AccountController> log)
    {
        _db = db;
        _log = log;
    }

    /* ---------------- Dashboard ---------------- */
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        long uid = HttpContext.Session.GetInt32("UserId") ?? 0;
        if (uid == 0) return RedirectToAction("Login", "Auth");

        var user = await _db.Users.FindAsync(uid);
        if (user == null) return Unauthorized();

        /* Thống kê tháng */
        var now = DateTime.UtcNow;
        var month0 = new DateTime(now.Year, now.Month, 1);

        var monthDeposit = await _db.Transactions
            .Where(t => t.user_id == uid && t.transaction_type == "deposit"
                     && t.created_at >= month0)
            .SumAsync(t => (decimal?)t.amount) ?? 0;

        var monthSpend = await _db.Transactions
            .Where(t => t.user_id == uid && t.transaction_type == "bet"
                     && t.created_at >= month0)
            .SumAsync(t => (decimal?)-t.amount) ?? 0;   // bet là âm

        /* 5 cược gần nhất */
        var recent = await _db.Bets
            .Where(b => b.user_id == uid)
            .OrderByDescending(b => b.placed_at)
            .Take(5)
            .Select(b => new BetBriefVm
            {
                BetId = b.bet_id,
                Stake = b.stake_amount,
                Payout = b.potential_payout,
                Status = b.status,
                PlacedAt = b.placed_at,
                Selection = b.Bet_Items.First().selection.name,
                MarketType = b.Bet_Items.First().selection.market.market_type.code
            })
            .ToListAsync();

        var vm = new AccountOverviewVm
        {
            UserName = user.name,
            Email = user.email,
            CreatedAt = user.created_at,
            Balance = user.balance,
            MonthDeposit = monthDeposit,
            MonthSpend = monthSpend,
            RecentBets = recent
        };

        return View(vm);
    }
    /* ---------- AJAX lấy lịch sử ---------- */
    [HttpGet]
    public async Task<IActionResult> BetHistory(string filter = "all")
    {
        long uid = HttpContext.Session.GetInt32("UserId") ?? 0;
        if (uid == 0) return Unauthorized();

        var bets = await _db.Bets
            .Where(b => b.user_id == uid)
            .Include(b => b.Bet_Items)
                .ThenInclude(i => i.selection)
                    .ThenInclude(s => s.market)
                        .ThenInclude(m => m.market_type)
            .Include(b => b.Bet_Items)
                .ThenInclude(i => i.selection)
                    .ThenInclude(s => s.market)
                        .ThenInclude(m => m.match)
                            .ThenInclude(ma => ma.home_team)
            .Include(b => b.Bet_Items)
                .ThenInclude(i => i.selection)
                    .ThenInclude(s => s.market)
                        .ThenInclude(m => m.match)
                            .ThenInclude(ma => ma.away_team)
            .OrderByDescending(b => b.placed_at)
            .ToListAsync();

        var rows = new List<BetHistoryRowVm>();

        foreach (var b in bets)
        {
            var item = b.Bet_Items.FirstOrDefault(i =>
                i.selection != null &&
                i.selection.market != null &&
                i.selection.market.match != null &&
                i.selection.market.match.home_team != null &&
                i.selection.market.match.away_team != null &&
                i.selection.market.market_type != null);

            if (item == null) continue;

            var match = item.selection.market.match;
            var profit = b.status switch
            {
                "won" => b.potential_payout - b.stake_amount,
                "lost" => -b.stake_amount,
                _ => 0
            };

            rows.Add(new BetHistoryRowVm
            {
                Match = $"{match.home_team.short_name ?? match.home_team.name} vs {match.away_team.short_name ?? match.away_team.name}",
                MarketType = item.selection.market.market_type.code,
                ResultText = b.status switch
                {
                    "won" => "Thắng",
                    "lost" => "Thua",
                    _ => "Chờ"
                },
                StatusText = b.status == "pending" ? "Chưa lên kèo" : "Đã lên kèo",
                PlacedAt = b.placed_at,
                Profit = profit
            });
        }

        rows = filter switch
        {
            "settled" => rows.Where(r => r.StatusText == "Đã lên kèo").ToList(),
            "pending" => rows.Where(r => r.StatusText == "Chưa lên kèo").ToList(),
            _ => rows
        };

        return PartialView("_BetHistoryTable", rows);
    }



    /* ---------------- Withdraw popup submit ---------------- */
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Withdraw(decimal amount, string bankName, string accountNumber)
    {
        long uid = HttpContext.Session.GetInt32("UserId") ?? 0;
        if (uid == 0) return Unauthorized();

        if (amount <= 0 ||
            string.IsNullOrWhiteSpace(bankName) ||
            string.IsNullOrWhiteSpace(accountNumber))
        {
            return BadRequest("Dữ liệu không hợp lệ.");
        }

        var user = await _db.Users.FindAsync(uid);
        if (user == null || user.balance < amount)
            return BadRequest("Số dư không đủ.");

        var info = $"{bankName.Trim()} – {accountNumber.Trim()}";

        _db.Withdraw_Requests.Add(new Withdraw_Request
        {
            user_id = uid,
            amount = amount,
            bank_account = info,
            status = "pending",
            requested_at = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        TempData["Toast"] = "Yêu cầu rút tiền đã gửi! Vui lòng chờ duyệt.";
        return RedirectToAction(nameof(Index));
    }


    /* ---------------- Change display name ---------------- */
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeName(string newName)
    {
        long uid = HttpContext.Session.GetInt32("UserId") ?? 0;
        if (uid == 0) return Unauthorized();

        if (string.IsNullOrWhiteSpace(newName) || newName.Length > 50)
            return BadRequest("Tên không hợp lệ.");

        var user = await _db.Users.FindAsync(uid);
        user!.name = newName.Trim();
        user.updated_at = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        TempData["Toast"] = "Đã cập nhật tên hiển thị.";
        return RedirectToAction(nameof(Index));
    }
}
