using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UMT88.Data;
using UMT88.ViewModels;

namespace UMT88.Controllers
{
    public class UserDashboardController : Controller
    {
        private readonly AppDbContext _db;
        public UserDashboardController(AppDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            /* -------- 1. Lấy user hiện tại -------- */
            var uid = HttpContext.Session.GetInt32("UserId");
            if (uid == null) return RedirectToAction("Login", "Auth");

            var user = await _db.Users.FirstAsync(u => u.user_id == uid.Value);

            /* -------- 2. Lấy các trận (live hoặc scheduled) -------- */
            var baseQuery = _db.Matches
                .Include(m => m.season).ThenInclude(s => s.competition)
                .Include(m => m.home_team)
                .Include(m => m.away_team)
                .Include(m => m.Match_Results)
                .Where(m => m.status == "live" || m.status == "scheduled");

            /* 3 trận nổi bật ngẫu nhiên */
            var featured = await baseQuery
                .OrderBy(r => Guid.NewGuid())
                .Take(3)
                .Select(m => new MatchVm
                {
                    match_id = m.match_id,
                    competition = m.season.competition.name,
                    home_name = m.home_team.name,
                    away_name = m.away_team.name,
                    home_logo = m.home_team.image_url,
                    away_logo = m.away_team.image_url,
                    home_score = m.Match_Results
                                   .OrderByDescending(r => r.created_at)
                                   .Select(r => (int?)r.home_score)
                                   .FirstOrDefault(),
                    away_score = m.Match_Results
                                   .OrderByDescending(r => r.created_at)
                                   .Select(r => (int?)r.away_score)
                                   .FirstOrDefault(),
                    status = m.status
                })
                .ToListAsync();

            /* Phần còn lại */
            var others = await baseQuery
                .OrderBy(m => m.match_id)
                .Select(m => new MatchVm
                {
                    match_id = m.match_id,
                    competition = m.season.competition.name,
                    home_name = m.home_team.name,
                    away_name = m.away_team.name,
                    home_logo = m.home_team.image_url,
                    away_logo = m.away_team.image_url,
                    home_score = m.Match_Results
                                   .OrderByDescending(r => r.created_at)
                                   .Select(r => (int?)r.home_score)
                                   .FirstOrDefault(),
                    away_score = m.Match_Results
                                   .OrderByDescending(r => r.created_at)
                                   .Select(r => (int?)r.away_score)
                                   .FirstOrDefault(),
                    status = m.status
                })
                .ToListAsync();

            /* -------- 3. Build ViewModel -------- */
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
