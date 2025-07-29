using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GPK.Lib.Utility.Helpers;
using Microsoft.Extensions.Hosting;
using SpiderStock.Infrastructure;
using System.Text.Json;
using Newtonsoft.Json;
using System.Runtime.InteropServices;

namespace SpiderStock.Application
{
    public interface IQuoteService
    {
        Task StartAsync(CancellationToken cancellationToken);
    }

    public class QuoteService : IQuoteService, IHostedService
    {
        private readonly IStockApiClient _stockApiClient;
        private readonly List<string> _symbols = new() { "TSLA.US", "AAPL.US", "GOOGL.US", "NVDA.US" }; // Example symbols, can be modified as needed
        private Timer _timer;

        public QuoteService(IStockApiClient stockApiClient)
        {
            _stockApiClient = stockApiClient;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(async _ => await FetchQuotesAsync(), null, TimeSpan.Zero, TimeSpan.FromSeconds(60));
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Dispose();
            return Task.CompletedTask;
        }

        private void SendToTelegram(string message)
        {
            TelegramBotHelper.SendTelegramMessage(new[] { TelegramBotHelper.TgBot.AutoUPayChatId }, message);
        }

        private async Task FetchQuotesAsync()
        {
            try
            {
                var quote = await _stockApiClient.GetQuoteAsync(_symbols);
                //var tick = quote.Data.TickList.FirstOrDefault();
                //var message = $"[{tick?.Code}] {tick?.Price} @ {DateTimeOffset.FromUnixTimeMilliseconds(tick?.TickTime ?? 0):yyyy-MM-dd HH:mm:ss}";
                var message = JsonConvert.SerializeObject(quote);
                Console.WriteLine(message);
                if (quote.Data != null)
                {
                    if (quote.Data.TickList.Any())
                    {
                        quote.Data.TickList.ForEach(tick =>
                        {
                            var tradeDirection = "";
                            if (tick.TradeDirection == "0")
                            {
                                tradeDirection = "中性盘";
                            }
                            if (tick.TradeDirection == "1")
                            {
                                tradeDirection = "主动买入";
                            }
                            if (tick.TradeDirection == "2")
                            {
                                tradeDirection = "主动卖出";
                            }


                            var loclalTime = ConvertFromUnixMillisecondsToTaipei(tick.TickTime);
                            // 格式化成字串
                            string tickTime = loclalTime.ToString("yyyy-MM-dd HH:mm:ss");
                            SendToTelegram(
                                $"美股通知 {Environment.NewLine}" +
                                $"代號:【{tick.Code}】{Environment.NewLine}" +
                                $"成交价:【{tick.Price.ToString()}】{Environment.NewLine}" +
                                $"交易方向:【{tradeDirection}】{Environment.NewLine}" +
                                $"時間:【{tickTime}】{Environment.NewLine}" 
                                );
                        });
                    }
                }
                
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error fetching quote for {_symbols}: {ex.Message}";
                Console.WriteLine(errorMessage);
                SendToTelegram(errorMessage);
            }

        }

        public static DateTime ConvertFromUnixMillisecondsToTaipei(long timestampMs)
        {
            long seconds = timestampMs / 1000;
            var utcTime = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime;

            var tz = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? TimeZoneInfo.FindSystemTimeZoneById("Taipei Standard Time")
                : TimeZoneInfo.FindSystemTimeZoneById("Asia/Taipei");

            return TimeZoneInfo.ConvertTimeFromUtc(utcTime, tz);
        }
    }
}
