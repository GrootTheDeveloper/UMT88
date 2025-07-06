using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using UMT88.Data;
using UMT88.Models;
using UMT88.ViewModels;

namespace UMT88.Controllers
{
    public class BetController : Controller
    {
        private readonly AppDbContext _db;
        public BetController(AppDbContext db) => _db = db;

        /* ---------------- XEM KÈO ---------------- */
        [HttpGet]
        public async Task<IActionResult> Index(long matchId)
        {
            var match = await _db.Matches
               .Include(m => m.home_team)
               .Include(m => m.away_team)
               .Include(m => m.Markets).ThenInclude(mk => mk.market_type)
               .Include(m => m.Markets).ThenInclude(mk => mk.Selections!)
                                        .ThenInclude(sel => sel.Odds)
               .FirstOrDefaultAsync(m => m.match_id == matchId);

            if (match == null) return NotFound();

            var vm = new MatchDetailVm
            {
                match_id = match.match_id,
                home_name = match.home_team.name,
                away_name = match.away_team.name,
                start_time = match.start_time.ToString("yyyy-MM-dd HH:mm"),
                markets = match.Markets.Select(mk => new MarketVm
                {
                    market_type = mk.market_type.code,
                    items = mk.Selections
                              .Select(sel => (sel,
                                  sel.Odds.OrderByDescending(o => o.created_at).First()))
                              .ToList()
                }).ToList()
            };

            /* truyền balance để JS hiển thị */
            long? uid = HttpContext.Session.GetInt32("UserId");
            ViewBag.Balance = uid == null ? 0
                              : (await _db.Users.FindAsync(uid))?.balance ?? 0;

            return View(vm);
        }

        /* ---------------- ĐẶT CƯỢC ---------------- */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceBet(long selectionId, decimal stake)
        {
            long uid = HttpContext.Session.GetInt32("UserId") ?? 0;
            if (uid == 0) return Unauthorized();
            if (stake <= 0) return BadRequest("Số tiền cược không hợp lệ.");

            using var trx = await _db.Database.BeginTransactionAsync();

            var user = await _db.Users.FirstAsync(u => u.user_id == uid);
            if (stake > user.balance)
                return BadRequest("Không đủ số dư.");

            var sel = await _db.Selections
                       .Include(s => s.market).ThenInclude(mk => mk.match)
                       .Include(s => s.Odds.OrderByDescending(o => o.created_at))
                       .FirstOrDefaultAsync(s => s.selection_id == selectionId);
            if (sel == null || sel.status != "open")
                return BadRequest("Cửa cược đã khóa.");

            var oddsNow = sel.Odds.First().odds_value;

            var bet = new Bet
            {
                user_id = uid,
                stake_amount = stake,
                potential_payout = stake * oddsNow,
                status = "pending",
                placed_at = DateTime.UtcNow
            };
            _db.Bets.Add(bet);

            _db.Bet_Items.Add(new Bet_Item
            {
                bet = bet,
                selection_id = selectionId,
                odds_at_placement = oddsNow,
                result = "pending"
            });

            user.balance -= stake;
            _db.Transactions.Add(new Transaction
            {
                user_id = uid,
                transaction_type = "bet",
                amount = -stake,
                balance_after = user.balance,
                created_at = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
            await trx.CommitAsync();

            // Lưu string thay vì decimal để TempData serialize được
            TempData["Balance"] = user.balance.ToString("0.##");
            TempData["Toast"] = "Đặt cược thành công!"; 
            return RedirectToAction("Index", new { matchId = sel.market.match_id });
        }
    }
}
