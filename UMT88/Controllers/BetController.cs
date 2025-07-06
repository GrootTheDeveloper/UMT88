using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UMT88.Data;
using UMT88.ViewModels;

namespace UMT88.Controllers;

public class BetController : Controller
{
    private readonly AppDbContext _db;
    public BetController(AppDbContext db) => _db = db;

    /* -------- GET /Bet/Index/{matchId} -------- */
    [HttpGet]
    public async Task<IActionResult> Index(long matchId)
    {
        var match = await _db.Matches
            .Include(m => m.home_team)
            .Include(m => m.away_team)
            .Include(m => m.Markets!)
                .ThenInclude(mk => mk.market_type)
            .Include(m => m.Markets!)
                .ThenInclude(mk => mk.Selections!)
                    .ThenInclude(sel => sel.Odds)
            .FirstOrDefaultAsync(m => m.match_id == matchId);

        if (match == null) return NotFound();

        var vm = new MatchDetailVm
        {
            match_id = match.match_id,
            home_name = match.home_team.name,
            away_name = match.away_team.name,
            start_time = match.start_time.ToString("yyyy-MM-dd HH:mm"),
            markets = match.Markets
                          .OrderBy(mk => mk.market_type.code)
                          .Select(mk => new MarketVm
                          {
                              market_type = mk.market_type.code,      // AH / OU / 1X2 ...
                              items = mk.Selections.Select(sel => (
                                          sel,
                                          sel.Odds.OrderByDescending(o => o.created_at).First()
                                       )).ToList()
                          })
                          .ToList()
        };

        return View(vm);     // Views/Bet/Index.cshtml
    }
}
