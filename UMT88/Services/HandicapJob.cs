using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UMT88.Data;
using UMT88.Models;
using static UMT88.Services.HandicapCalculator;

public class HandicapJob : BackgroundService
{
    private readonly IServiceProvider _sp;
    private readonly ILogger<HandicapJob> _log;
    public HandicapJob(IServiceProvider sp, ILogger<HandicapJob> log)
    { _sp = sp; _log = log; }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                using var scope = _sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                /* ===== 1. Rank + GPG ===== */
                var standings = await db.League_Standings
                    .Where(s => s.season_id == 1)                             // Premier League
                    .OrderByDescending(s => s.points)
                    .ThenByDescending(s => s.goal_difference)
                    .ToListAsync(ct);

                var ranks = standings.Select((s, idx) => new { s.team_id, Rank = idx + 1 })
                                     .ToDictionary(x => x.team_id, x => x.Rank);

                var gpg = standings.ToDictionary(s => s.team_id,
                                                 s => s.matches_played == 0
                                                        ? 0m
                                                        : (decimal)s.goals_for / s.matches_played);

                /* ===== 2. Trận scheduled 24h tới ===== */
                var matches = await db.Matches
                    .Include(m => m.home_team)
                    .Include(m => m.away_team)
                    .Where(m => m.status == "scheduled" &&
                                m.start_time <= DateTime.UtcNow.AddHours(24))
                    .ToListAsync(ct);

                var ahType = await db.Market_Types.FirstAsync(mt => mt.code == "AH", ct);
                var ouType = await db.Market_Types.FirstAsync(mt => mt.code == "OU", ct);

                foreach (var m in matches)
                {
                    if (!ranks.ContainsKey(m.home_team_id) || !ranks.ContainsKey(m.away_team_id))
                        continue;   // thiếu BXH

                    /* ---------- AH ---------- */
                    int diff = ranks[m.away_team_id] - ranks[m.home_team_id];
                    decimal h = CalcAH(diff);

                    await UpsertMarket(db, m, ahType,        // market_type AH
                        $"{m.home_team.short_name ?? m.home_team.name} {(-h):0.##}",
                        $"{m.away_team.short_name ?? m.away_team.name} {(+h):0.##}",
                        1.90m, 1.90m, ct);

                    /* ---------- OU ---------- */
                    decimal expected = (gpg[m.home_team_id] + gpg[m.away_team_id]) / 2m;
                    decimal line = RoundOU(expected);
                    if (line < 2m) line = 2m;

                    await UpsertMarket(db, m, ouType,        // market_type OU
                        $"Over {line:0.##}", $"Under {line:0.##}",
                        1.90m, 1.90m, ct);
                }

                await db.SaveChangesAsync(ct);
                _log.LogInformation("Handicap job ran OK at {time}", DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Handicap job error");
            }

            await Task.Delay(TimeSpan.FromMinutes(15), ct);
        }
    }

    /* helper tạo / làm mới Market */
    private static async Task UpsertMarket(
        AppDbContext db, Match m, Market_Type type,
        string sel1Name, string sel2Name,
        decimal odds1, decimal odds2,
        CancellationToken ct)
    {
        var mk = await db.Markets
                .Include(x => x.Selections).ThenInclude(s => s.Odds)
                .FirstOrDefaultAsync(x => x.match_id == m.match_id &&
                                          x.market_type_id == type.market_type_id, ct);

        if (mk == null)
        {
            mk = new Market
            {
                match_id = m.match_id,
                market_type_id = type.market_type_id,
                status = "open",
                created_at = DateTime.UtcNow
            };
            db.Markets.Add(mk);
        }

        db.Odds.RemoveRange(mk.Selections.SelectMany(s => s.Odds)); // xoá bản odds cũ
        db.Selections.RemoveRange(mk.Selections);

        var s1 = new Selection { name = sel1Name, status = "open", created_at = DateTime.UtcNow };
        var s2 = new Selection { name = sel2Name, status = "open", created_at = DateTime.UtcNow };
        mk.Selections = new List<Selection> { s1, s2 };

        db.Odds.AddRange(
            new Odd { selection = s1, odds_value = odds1, created_at = DateTime.UtcNow },
            new Odd { selection = s2, odds_value = odds2, created_at = DateTime.UtcNow });
    }
}
