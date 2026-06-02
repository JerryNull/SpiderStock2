using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using SpiderStock.Domain;

namespace SpiderStock.Infrastructure
{
    public interface IEsunApiClient
    {
        /// <summary>取得市場所有股票即時行情快照（含漲跌幅、交易量）</summary>
        Task<List<SnapshotQuote>> GetSnapshotQuotesAsync(string market = "TSE");

        /// <summary>取得特定產業的股票清單</summary>
        Task<List<TickerItem>> GetTickersByIndustryAsync(string industryCode, string exchange = "TWSE");

        /// <summary>取得所有股票的產業代碼對照表（symbol → industryCode）</summary>
        Task<Dictionary<string, string>> GetSymbolIndustryMapAsync();
    }

    public class EsunApiClient : IEsunApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string _token;

        private static readonly string[] Exchanges = { "TWSE", "TPEx" };
        private static readonly string[] IndustryCodes = {
            "01","02","03","04","05","06","07","08","09","10",
            "11","12","13","14","15","16","17","18","19","20",
            "21","22","23","24","25","26","27","28","29","30",
            "31","32","33","34","36","38","80"
        };

        public EsunApiClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _baseUrl = configuration["EsunApi:BaseUrl"] ?? "https://esunmarket.esunsec.com.tw/v1";
            _token = configuration["EsunApi:Token"] ?? "";
            if (!string.IsNullOrEmpty(_token))
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _token);
        }

        public async Task<List<SnapshotQuote>> GetSnapshotQuotesAsync(string market = "TSE")
        {
            var url = $"{_baseUrl}/snapshot/quotes/{market}?type=COMMONSTOCK";
            var resp = await _httpClient.GetAsync(url);
            if (!resp.IsSuccessStatusCode) return new();
            var json = await resp.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<SnapshotResponse>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return result?.Data ?? new();
        }

        public async Task<List<TickerItem>> GetTickersByIndustryAsync(string industryCode, string exchange = "TWSE")
        {
            var url = $"{_baseUrl}/intraday/tickers?type=EQUITY&exchange={exchange}&industry={industryCode}";
            var resp = await _httpClient.GetAsync(url);
            if (!resp.IsSuccessStatusCode) return new();
            var json = await resp.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<TickersResponse>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return result?.Data ?? new();
        }

        public async Task<Dictionary<string, string>> GetSymbolIndustryMapAsync()
        {
            var map = new Dictionary<string, string>();
            var tasks = new List<Task>();

            foreach (var exchange in Exchanges)
            {
                foreach (var code in IndustryCodes)
                {
                    var c = code;
                    var ex = exchange;
                    tasks.Add(Task.Run(async () =>
                    {
                        var tickers = await GetTickersByIndustryAsync(c, ex);
                        lock (map)
                        {
                            foreach (var t in tickers)
                                map[t.Symbol] = c;
                        }
                    }));
                }
            }

            await Task.WhenAll(tasks);
            return map;
        }
    }
}
