namespace UMT88.ViewModels
{
    public class BetHistoryRowVm
    {
        public string Match { get; set; } = "";
        public string MarketType { get; set; } = "";
        public string ResultText { get; set; } = "";     // Thắng / Thua / Pending
        public string StatusText { get; set; } = "";     // Đã lên kèo / Chưa lên kèo
        public DateTime PlacedAt { get; set; }
        public decimal Profit { get; set; }           // âm => lỗ
        public bool IsWin => Profit > 0;
    }
}
