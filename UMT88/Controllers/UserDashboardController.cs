using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using UMT88.Data;
using UMT88.ViewModels;

namespace UMT88.Controllers
{
    public class UserDashboardController : Controller
    {
        private readonly AppDbContext _db;
        public UserDashboardController(AppDbContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            /* -------- User session -------- */
            int? uid = HttpContext.Session.GetInt32("UserId");

            if (uid == null)
            {
                return RedirectToAction("Login", "Auth"); // hoặc xử lý khác khi chưa đăng nhập
            }

            long userId = Convert.ToInt64(uid.Value);

            var user = await _db.Users.FindAsync(userId);

            /* -------- Featured (3 trận) -------- */
            var featured = await _db.Matches
                .Include(m => m.home_team)
                .Include(m => m.away_team)
                .Include(m => m.Match_Results)
                .Where(m => m.status == "live" || m.status == "scheduled")
                .OrderBy(m => m.start_time)
                .Take(3)
                .Select(m => new LiveVm
                {
                    match_id = m.match_id,
                    home_name = m.home_team.name,
                    away_name = m.away_team.name,
                    home_score = m.Match_Results
                                   .OrderByDescending(r => r.created_at)
                                   .Select(r => (int?)r.home_score).FirstOrDefault(),
                    away_score = m.Match_Results
                                   .OrderByDescending(r => r.created_at)
                                   .Select(r => (int?)r.away_score).FirstOrDefault(),
                    status = m.status,
                    home_logo = m.home_team.image_url,
                    away_logo = m.away_team.image_url
                })
                .ToListAsync();

            /* -------- Others = Market AH -------- */
            /* trận scheduled bất kỳ trong 24h */
            var matchesScheduled = await _db.Matches
                .Include(m => m.home_team)
                .Include(m => m.away_team)
                .Include(m => m.season).ThenInclude(s => s.competition)
                .Where(m => m.status == "scheduled" &&
                            m.start_time <= DateTime.UtcNow.AddHours(24))
                .ToListAsync();

            /* map thành MatchCardVm – cố gắng tìm Market AH (nếu có) */
            var others = matchesScheduled.Select(m =>
            {
                var ahMarket = _db.Markets
                    .Include(mk => mk.Selections)
                    .Include(mk => mk.market_type)
                    .FirstOrDefault(mk => mk.match_id == m.match_id &&
                                          mk.market_type.code == "AH");

                decimal h = 0m;
                if (ahMarket != null && ahMarket.Selections.Any())
                {
                    var raw = ahMarket.Selections.First().name.Split(' ').Last()
                                                              .Replace("+", "")
                                                              .Replace(",", ".");
                    decimal.TryParse(raw, System.Globalization.NumberStyles.Any,
                                     System.Globalization.CultureInfo.InvariantCulture, out h);
                }

                return new MatchCardVm
                {
                    match_id = m.match_id,
                    competition = m.season.competition.name,
                    home_name = m.home_team.name,
                    away_name = m.away_team.name,
                    handicap_h = h         // sẽ là 0 nếu chưa sinh kèo
                };
            })
            .OrderBy(m => m.match_id)
            .ToList();


            /* -------- ViewModel -------- */
            var vm = new DashboardVm
            {
                user_name = user.name,
                balance = user.balance,
                featured = featured,
                others = others
            };
            return View(vm);
        }
    }
}
