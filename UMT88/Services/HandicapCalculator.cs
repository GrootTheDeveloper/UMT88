namespace UMT88.Services;

public static class HandicapCalculator
{
    /* ----- Asian Handicap ----- */
    public static decimal CalcAH(int rankDiff)
    {
        int d = Math.Abs(rankDiff);
        if (d <= 1) return 0m;
        if (d == 2) return 0.25m;
        if (d <= 4) return 0.5m;
        if (d <= 6) return 0.75m;
        if (d <= 8) return 1.0m;
        return 1.25m;
    }

    /* ----- Over-Under line: làm tròn 0.25 ----- */
    public static decimal RoundOU(decimal x)
        => Math.Round(x * 4m, MidpointRounding.AwayFromZero) / 4m;
}
