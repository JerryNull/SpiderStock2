namespace SpiderStock.Domain
{
    // eSun API: /intraday/tickers 回傳
    public class TickerItem
    {
        public string Symbol { get; set; } = "";
        public string Name { get; set; } = "";
    }

    public class TickersResponse
    {
        public string Date { get; set; } = "";
        public string Type { get; set; } = "";
        public string Exchange { get; set; } = "";
        public string Market { get; set; } = "";
        public string Industry { get; set; } = "";
        public List<TickerItem> Data { get; set; } = new();
    }

    // eSun API: /snapshot/quotes/{market} 回傳
    public class SnapshotQuote
    {
        public string Date { get; set; } = "";
        public string Type { get; set; } = "";
        public string Symbol { get; set; } = "";
        public string Name { get; set; } = "";
        public string Market { get; set; } = "";
        public decimal? OpenPrice { get; set; }
        public decimal? HighPrice { get; set; }
        public decimal? LowPrice { get; set; }
        public decimal? ClosePrice { get; set; }
        public decimal? Change { get; set; }
        public decimal? ChangePercent { get; set; }
        public long? TradeVolume { get; set; }
        public decimal? TradeValue { get; set; }
    }

    public class SnapshotResponse
    {
        public string Date { get; set; } = "";
        public string Market { get; set; } = "";
        public List<SnapshotQuote> Data { get; set; } = new();
    }

    // 產業彙總 - 產業漲跌圖用
    public class SectorSummary
    {
        public string IndustryCode { get; set; } = "";
        public string IndustryName { get; set; } = "";
        public decimal AvgChangePercent { get; set; }
        public long TotalVolume { get; set; }
        public int StockCount { get; set; }
    }

    // 個股資料 - 類股交易量圖用
    public class StockInSector
    {
        public string Symbol { get; set; } = "";
        public string Name { get; set; } = "";
        public decimal? ChangePercent { get; set; }
        public long? Volume { get; set; }
    }

    // 產業代碼對照
    public static class IndustryCodeMap
    {
        public static readonly Dictionary<string, string> Map = new()
        {
            { "01", "水泥工業" },
            { "02", "食品工業" },
            { "03", "塑膠工業" },
            { "04", "紡織纖維" },
            { "05", "電機機械" },
            { "06", "電器電纜" },
            { "07", "化學生技醫療" },
            { "08", "玻璃陶瓷" },
            { "09", "造紙工業" },
            { "10", "鋼鐵工業" },
            { "11", "橡膠工業" },
            { "12", "汽車工業" },
            { "13", "電子工業" },
            { "14", "建材營造" },
            { "15", "航運業" },
            { "16", "觀光餐旅" },
            { "17", "金融保險" },
            { "18", "貿易百貨" },
            { "19", "綜合" },
            { "20", "其他" },
            { "21", "化學工業" },
            { "22", "生技醫療業" },
            { "23", "油電燃氣業" },
            { "24", "半導體業" },
            { "25", "電腦及週邊設備業" },
            { "26", "光電業" },
            { "27", "通信網路業" },
            { "28", "電子零組件業" },
            { "29", "電子通路業" },
            { "30", "資訊服務業" },
            { "31", "其他電子業" },
            { "32", "文化創意業" },
            { "33", "農業科技業" },
            { "34", "電子商務" },
            { "36", "存託憑證" },
            { "38", "運動休閒業" },
            { "80", "管理股票" },
        };

        public static string GetName(string code) =>
            Map.TryGetValue(code, out var name) ? name : $"產業{code}";
    }
}
