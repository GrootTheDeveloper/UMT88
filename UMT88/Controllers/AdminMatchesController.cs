using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UMT88.Data;
using UMT88.Models;
using UMT88.ViewModels.AdminMatches;

namespace UMT88.Controllers;

public class AdminMatchesController : Controller
{
    private readonly AppDbContext _db;
    public AdminMatchesController(AppDbContext db) => _db = db;

    /* ---------- helper ---------- */
    private bool NotAdmin => (HttpContext.Session.GetInt32("RoleId") ?? 0) != 1;
    private IActionResult ForbidIfNotAdmin() => NotAdmin ? Forbid() : null!;

    /* ---------- LIST ---------- */
    public async Task<IActionResult> Index()
    {
        if (NotAdmin) return Forbid();

        var rows = await _db.Matches
            .Include(m => m.season).ThenInclude(s => s.competition)
            .Include(m => m.home_team).Include(m => m.away_team)
            .OrderByDescending(m => m.start_time)
            .Take(100)
            .Select(m => new MatchRowVm(
                m.match_id,
                m.season.competition.name,
                m.start_time.ToString("dd/MM HH:mm"),
                m.home_team.short_name ?? m.home_team.name,
                m.away_team.short_name ?? m.away_team.name,
                m.status))
            .ToListAsync();

        return View(rows);
    }

    /* ---------- CREATE ---------- */
    [HttpGet]
    public IActionResult Create()
    {
        if (NotAdmin) return Forbid();
        // TODO: load dropdown Competition / Season / Team
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(long seasonId, long homeId, long awayId, DateTime startTime)
    {
        if (NotAdmin) return Forbid();
        if (homeId == awayId) return BadRequest("Hai đội trùng nhau");

        var m = new Match
        {
            season_id = seasonId,
            home_team_id = homeId,
            away_team_id = awayId,
            start_time = startTime,
            status = "scheduled",
            created_at = DateTime.UtcNow
        };
        _db.Matches.Add(m);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    /* ---------- DELETE ---------- */
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id)
    {
        if (NotAdmin) return Forbid();
        var m = await _db.Matches.FindAsync(id);
        if (m == null) return NotFound();
        _db.Matches.Remove(m);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    /* ---------- EDIT ---------- */
    [HttpGet]
    public async Task<IActionResult> Edit(long id)
    {
        if (NotAdmin) return Forbid();

        var match = await _db.Matches
            .Include(m => m.season).ThenInclude(s => s.competition)
            .Include(m => m.home_team).Include(m => m.away_team)
            .Include(m => m.Markets).ThenInclude(k => k.market_type)
            .Include(m => m.Markets).ThenInclude(k => k.Selections).ThenInclude(s => s.Odds)
            .FirstOrDefaultAsync(m => m.match_id == id);

        if (match == null) return NotFound();

        var vm = new MatchEditVm
        {
            MatchId = match.match_id,
            Competition = match.season.competition.name,
            HomeTeam = match.home_team.short_name ?? match.home_team.name,
            AwayTeam = match.away_team.short_name ?? match.away_team.name,
            StartTime = match.start_time.ToString("dd/MM HH:mm"),
            Status = match.status,
            Markets = match.Markets.Select(k =>
            {
                var s1 = k.Selections.ElementAt(0);
                var s2 = k.Selections.ElementAt(1);
                var o1 = s1.Odds.OrderByDescending(o => o.created_at).First();
                var o2 = s2.Odds.OrderByDescending(o => o.created_at).First();
                return new MarketVm
                {
                    MarketId = k.market_id,
                    Type = k.market_type.code,
                    Status = k.status,
                    Sel1Id = s1.selection_id,
                    Sel1 = s1.name,
                    Odds1 = o1.odds_value,
                    Sel2Id = s2.selection_id,
                    Sel2 = s2.name,
                    Odds2 = o2.odds_value
                };
            }).ToList()
        };
        return View(vm);
    }

    /* ---------- ADD MARKET (AH / OU) ---------- */
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddMarket(long matchId, string type) // type = "AH" | "OU"
    {
        if (NotAdmin) return Forbid();
        var match = await _db.Matches.Include(m => m.home_team).Include(m => m.away_team)
                                     .FirstOrDefaultAsync(m => m.match_id == matchId);
        if (match == null) return NotFound();

        var mType = await _db.Market_Types.FirstOrDefaultAsync(t => t.code == type);
        if (mType == null) return BadRequest("Loại kèo không hợp lệ");

        // mặc định handicap 0 / over-under 2.5
        string s1name, s2name;
        if (type == "AH")
        {
            s1name = $"{match.home_team.short_name ?? match.home_team.name} 0";
            s2name = $"{match.away_team.short_name ?? match.away_team.name} 0";
        }
        else // OU
        {
            s1name = "Over 2.5";
            s2name = "Under 2.5";
        }

        var market = new Market
        {
            match_id = matchId,
            market_type_id = mType.market_type_id,
            status = "open",
            created_at = DateTime.UtcNow
        };

        var sel1 = new Selection { name = s1name, status = "open", created_at = DateTime.UtcNow };
        var sel2 = new Selection { name = s2name, status = "open", created_at = DateTime.UtcNow };

        /* dùng List để có sẵn 2 selection */
        market.Selections = new List<Selection> { sel1, sel2 };

        _db.Markets.Add(market);
        await _db.SaveChangesAsync();

        // odds = 1.90
        _db.Odds.AddRange(
            new Odd { selection_id = sel1.selection_id, odds_value = 1.90m, created_at = DateTime.UtcNow },
            new Odd { selection_id = sel2.selection_id, odds_value = 1.90m, created_at = DateTime.UtcNow });
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Edit), new { id = matchId });
    }

    /* ---------- SAVE ODDS & SELECTION NAMES ---------- */
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveMarket(
        long sel1Id, string sel1Name, decimal odds1,
        long sel2Id, string sel2Name, decimal odds2)
    {
        var s1 = await _db.Selections
                          .Include(x => x.market)
                          .FirstOrDefaultAsync(x => x.selection_id == sel1Id);
        var s2 = await _db.Selections
                          .FirstOrDefaultAsync(x => x.selection_id == sel2Id);

        if (s1 == null || s2 == null || s1.market == null)
            return NotFound();

        /* cập nhật tên & odds */
        s1.name = sel1Name;
        s2.name = sel2Name;

        var o1 = await _db.Odds.OrderByDescending(o => o.created_at)
                              .FirstAsync(o => o.selection_id == sel1Id);
        var o2 = await _db.Odds.OrderByDescending(o => o.created_at)
                              .FirstAsync(o => o.selection_id == sel2Id);

        o1.odds_value = odds1;
        o2.odds_value = odds2;

        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Edit),
                new { id = s1.market.match_id });
    }

    /* ---------- TOGGLE MARKET ---------- */
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleMarket(long marketId)
    {
        if (NotAdmin) return Forbid();
        var m = await _db.Markets.FindAsync(marketId);
        if (m == null) return NotFound();

        m.status = m.status == "open" ? "closed" : "open";
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Edit), new { id = m.match_id });
    }
}
