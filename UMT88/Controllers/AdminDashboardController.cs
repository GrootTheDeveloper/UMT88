using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UMT88.Data;
using UMT88.ViewModels;
namespace UMT88.Controllers;

public class AdminDashboardController : Controller
{
    private readonly AppDbContext _db;
    public AdminDashboardController(AppDbContext db) => _db = db;

    // GET /AdminDashboard
    public async Task<IActionResult> Index()
    {
        // -- kiểm quyền đơn giản
        if (HttpContext.Session.GetInt32("RoleId") != 1)
            return Unauthorized();

        var today = DateTime.UtcNow.Date;

        /* ==== Ô thống kê ==== */
        var onlineUsers = await _db.Users.CountAsync(u => u.status == "active");    // demo
        var newToday = await _db.Users.CountAsync(u => u.created_at.Date == today);

        var betsToday = await _db.Bets
                                   .Where(b => b.placed_at.Date == today)
                                   .ToListAsync();

        var lostStake = betsToday
                         .Where(b => b.status == "lost")
                         .Sum(b => b.stake_amount);

        var wonPayout = betsToday
                         .Where(b => b.status == "won")
                         .Sum(b => b.potential_payout - b.stake_amount);   // lãi trả cho user

        // coin → VNĐ (1 xu = 10 000)
        decimal revenue = (lostStake - wonPayout) * 10_000m;

        /* ==== bảng cược gần đây ==== */
        var recent = await _db.Bets
            .Include(b => b.Bet_Items).ThenInclude(i => i.selection).ThenInclude(s => s.market).ThenInclude(m => m.match)
            .Include(b => b.user)
            .OrderByDescending(b => b.placed_at)
            .Take(15)
            .Select(b => new AdminDashboardVm.RecentBetRow(
                b.user.name,
                $"{b.Bet_Items.First().selection.market.match.home_team.short_name ?? b.Bet_Items.First().selection.market.match.home_team.name} " +
                $"vs {b.Bet_Items.First().selection.market.match.away_team.short_name ?? b.Bet_Items.First().selection.market.match.away_team.name}",
                b.stake_amount,
                b.placed_at,
                b.status
            )).ToListAsync();

        var vm = new AdminDashboardVm
        {
            OnlineUsers = onlineUsers,
            NewUsersToday = newToday,
            BetsToday = betsToday.Count,
            RevenueVnd = revenue,
            RecentBets = recent
        };
        return View(vm);
    }
}
