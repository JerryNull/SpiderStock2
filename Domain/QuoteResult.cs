using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace SpiderStock.Domain
{
    public class Tick
    {
        public string Code { get; set; }
        public string Seq { get; set; }
        [JsonProperty(PropertyName = "tick_time")]
        public long TickTime { get; set; }
        public decimal Price { get; set; }
        public int Volume { get; set; }
        public decimal Turnover { get; set; }

        [JsonProperty(PropertyName = "trade_direction")]
        public string TradeDirection { get; set; }
    }

    public class QuoteResult
    {
        public int Ret { get; set; }
        public string Msg { get; set; }
        public string Trace { get; set; }
        public Data? Data { get; set; }

    }
    public class Data
    {
        [JsonProperty(PropertyName = "tick_list")]
        public List<Tick> TickList { get; set; }
    }
}
