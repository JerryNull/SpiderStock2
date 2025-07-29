using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SpiderStock.Domain;
using Newtonsoft.Json;

namespace SpiderStock.Infrastructure
{
    public interface IStockApiClient
    {
        Task<QuoteResult> GetQuoteAsync(List<string> symbols);
    }

    public class AlltickApiClient : IStockApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _token;

        public AlltickApiClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _token = configuration["AlltickApiToken"];
        }

        public async Task<QuoteResult> GetQuoteAsync(List<string> symbols)
        {
            var symbolListJson = string.Join(",", symbols.Select(symbol => $"{{\"code\":\"{symbol}\"}}"));
            var query = Uri.EscapeDataString($"{{\"trace\":\"123456\",\"data\":{{\"symbol_list\":[{symbolListJson}]}}}}");
            var url = $"https://quote.alltick.io/quote-stock-b-api/trade-tick?token={_token}&query={query}";

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Failed to fetch quote for {symbolListJson}: {response.ReasonPhrase}");
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<QuoteResult>(json);
        }
    }
}
