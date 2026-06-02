using Microsoft.Extensions.Caching.Memory;
using SpiderStock.Domain;
using SpiderStock.Infrastructure;

namespace SpiderStock.Application
{
    public interface ISectorService
    {
        /// <summary>取得所有產業漲跌彙總（產業漲跌圖用）</summary>
        Task<List<SectorSummary>> GetSectorSummariesAsync();

        /// <summary>取得特定產業下各股票的交易量與漲跌（類股交易量圖用）</summary>
        Task<List<StockInSector>> GetStocksInSectorAsync(string industryCode);
    }

    public class SectorService : ISectorService
    {
        private readonly IEsunApiClient _esun;
        private readonly IMemoryCache _cache;

        private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

        public SectorService(IEsunApiClient esun, IMemoryCache cache)
        {
            _esun = esun;
            _cache = cache;
        }

        public async Task<List<SectorSummary>> GetSectorSummariesAsync()
        {
            return await _cache.GetOrCreateAsync("sector_summaries", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheTtl;

                // 同時取 TSE + OTC 快照
                var (tsQuotes, otcQuotes, industryMap) = await (
                    _esun.GetSnapshotQuotesAsync("TSE"),
                    _esun.GetSnapshotQuotesAsync("OTC"),
                    _esun.GetSymbolIndustryMapAsync()
                );

                var allQuotes = tsQuotes.Concat(otcQuotes).ToList();

                // 依產業彙總
                var grouped = allQuotes
                    .Where(q => industryMap.ContainsKey(q.Symbol) && q.ChangePercent.HasValue)
                    .GroupBy(q => industryMap[q.Symbol])
                    .Select(g => new SectorSummary
                    {
                        IndustryCode = g.Key,
                        IndustryName = IndustryCodeMap.GetName(g.Key),
                        AvgChangePercent = Math.Round(g.Average(q => q.ChangePercent!.Value), 2),
                        TotalVolume = g.Sum(q => q.TradeVolume ?? 0),
                        StockCount = g.Count(),
                    })
                    .OrderByDescending(s => s.AvgChangePercent)
                    .ToList();

                return grouped;
            }) ?? new();
        }

        public async Task<List<StockInSector>> GetStocksInSectorAsync(string industryCode)
        {
            var cacheKey = $"sector_stocks_{industryCode}";
            return await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheTtl;

                // 取該產業的股票清單（TWSE + TPEx）
                var (twseTickers, tpexTickers) = await (
                    _esun.GetTickersByIndustryAsync(industryCode, "TWSE"),
                    _esun.GetTickersByIndustryAsync(industryCode, "TPEx")
                );
                var symbols = twseTickers.Concat(tpexTickers)
                    .DistinctBy(t => t.Symbol)
                    .Select(t => t.Symbol)
                    .ToHashSet();

                // 取全市場快照後過濾
                var (tseQuotes, otcQuotes) = await (
                    _esun.GetSnapshotQuotesAsync("TSE"),
                    _esun.GetSnapshotQuotesAsync("OTC")
                );

                return tseQuotes.Concat(otcQuotes)
                    .Where(q => symbols.Contains(q.Symbol))
                    .Select(q => new StockInSector
                    {
                        Symbol = q.Symbol,
                        Name = q.Name,
                        ChangePercent = q.ChangePercent,
                        Volume = q.TradeVolume,
                    })
                    .OrderByDescending(s => s.Volume)
                    .ToList();
            }) ?? new();
        }
    }
}
