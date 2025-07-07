// Controllers/ReportController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using UMT88.Data;
using UMT88.ViewModels;


public class ReportController : Controller
{
    private readonly AppDbContext _db;
    private readonly ILogger<ReportController> _log;
    private const decimal VndPerXu = 10_000m;

    public ReportController(AppDbContext db, ILogger<ReportController> log)
        => (_db, _log) = (db, log);

    /* ---- form filter ---- */
    [HttpGet]
    public IActionResult Index()
        => View(new ReportFilterVm(null, null));

    /* ---- xem trước (HTML) ---- */
    [HttpPost]
    public async Task<IActionResult> Preview(ReportFilterVm f)
        => View("Preview", await BuildSummaryAsync(f));

    /* ---- tải PDF ---- */
    [HttpPost]
    public async Task<IActionResult> ExportPdf(ReportFilterVm f)
    {
        var summary = await BuildSummaryAsync(f);

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(20);
                page.Content()
                    .Column(col =>
                    {
                        col.Item().Text("Báo cáo kết quả cược").FontSize(18).Bold().AlignCenter();

                        col.Item().Text($"Khoảng thời gian: {f.From:dd/MM/yyyy} - {f.To:dd/MM/yyyy}")
                                  .FontSize(10).AlignCenter();

                        col.Item().PaddingVertical(10);

                        /* Tổng quan */
                        col.Item().Table(t =>
                        {
                            t.ColumnsDefinition(c => { c.RelativeColumn(); c.ConstantColumn(150); });
                            t.Cell().Text("Tổng stake").Bold();
                            t.Cell().Text($"{summary.TotalStake:N0} xu");

                            t.Cell().Text("Tổng payout").Bold();
                            t.Cell().Text($"{summary.TotalPayout:N0} xu");

                            t.Cell().Text("% lợi nhuận").Bold();
                            t.Cell().Text($"{summary.ProfitPercent:F2} %");
                        });

                        col.Item().PaddingVertical(10);

                        /* Top 10 trận */
                        col.Item().Text("Top 10 trận theo volume").Bold();
                        col.Item().Table(t =>
                        {
                            t.ColumnsDefinition(c => { c.RelativeColumn(); c.ConstantColumn(120); });
                            t.Header(h =>
                            {
                                h.Cell().Text("Trận").Bold();
                                h.Cell().Text("Volume (xu)").Bold();
                            });
                            foreach (var m in summary.TopMatches)
                            {
                                t.Cell().Text(m.Match);
                                t.Cell().Text($"{m.Volume:N0}");
                            }
                        });

                        col.Item().PaddingVertical(10);

                        /* Top 10 user */
                        col.Item().Text("Top 10 user thắng lớn").Bold();
                        col.Item().Table(t =>
                        {
                            t.ColumnsDefinition(c => { c.RelativeColumn(); c.ConstantColumn(120); });
                            t.Header(h =>
                            {
                                h.Cell().Text("Người chơi").Bold();
                                h.Cell().Text("Lãi (xu)").Bold();
                            });
                            foreach (var u in summary.TopUsers)
                            {
                                t.Cell().Text(u.User);
                                t.Cell().Text($"{u.Won:N0}");
                            }
                        });
                    });
            });
        });

        var bytes = doc.GeneratePdf();
        var file = $"BaoCao_{DateTime.Now:yyyyMMdd_HHmm}.pdf";
        return File(bytes, "application/pdf", file);
    }

    /* ---- hàm tính toán chung ---- */
    private async Task<ReportSummaryVm> BuildSummaryAsync(ReportFilterVm f)
    {
        var from = (f.From ?? DateTime.Today).Date;
        var to = (f.To ?? DateTime.Today).Date.AddDays(1).AddSeconds(-1);

        /* EF query */
        var bets = await _db.Bets
            .Include(b => b.Bet_Items)
                .ThenInclude(i => i.selection)
                    .ThenInclude(s => s.market)
                        .ThenInclude(mk => mk.match)
                            .ThenInclude(ma => ma.home_team)   // ⬅ phải có
            .Include(b => b.Bet_Items)
                .ThenInclude(i => i.selection)
                    .ThenInclude(s => s.market)
                        .ThenInclude(mk => mk.match)
                            .ThenInclude(ma => ma.away_team)   // ⬅ phải có
            .Where(b => b.placed_at >= from && b.placed_at <= to)
            .Include(b => b.Bet_Items).ThenInclude(i => i.selection)
            .ThenInclude(s => s.market).ThenInclude(m => m.match)
            .Include(b => b.user)
            .ToListAsync();

        var totalStake = bets.Sum(b => b.stake_amount);
        var totalPay = bets.Where(b => b.status == "won").Sum(b => b.potential_payout);

        /* Top 10 trận theo volume */
        var topMatches = bets
            .Where(b => b.Bet_Items.Any())                    // bỏ bet không có item
            .GroupBy(b => b.Bet_Items.First().selection.market.match_id)
            .Select(g => new
            {
                Match = g.First().Bet_Items.First().selection.market.match,
                Volume = g.Sum(x => x.stake_amount)
            })
            .Where(x => x.Match != null)                      // bỏ group không có match
            .OrderByDescending(x => x.Volume)
            .Take(10)
            .Select(x =>
            {
                var m = x.Match;
                var h = m.home_team?.short_name ?? m.home_team?.name ?? "N/A";
                var a = m.away_team?.short_name ?? m.away_team?.name ?? "N/A";
                return new TopMatchVm($"{h} vs {a}", x.Volume);
            })
            .ToList();


        /* Top 10 user thắng lớn */
        var topUsers = bets
            .Where(b => b.status == "won")
            .GroupBy(b => b.user_id)
            .Select(g => new
            {
                User = g.First().user.name,
                Won = g.Sum(x => x.potential_payout - x.stake_amount)
            })
            .OrderByDescending(x => x.Won).Take(10)
            .Select(x => new TopUserVm(x.User, x.Won)).ToList();

        return new ReportSummaryVm
        {
            TotalStake = totalStake,
            TotalPayout = totalPay,
            TopMatches = topMatches,
            TopUsers = topUsers
        };
    }
}
