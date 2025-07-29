using System;
using System.Net;

namespace GPK.Lib.Utility.Helpers
{
    public static class TelegramBotHelper
    {
        public enum TgBot
        {
            AutoUPayChatId
        }

        public static void SendTelegramMessage(TgBot[] tgBots, string message)
        {
            foreach (var bot in tgBots)
            {
                SendTelegramMessage(bot, message);
            }
        }

        public static bool SendTelegramMessage(TgBot tgBot, string message)
        {
            var token = string.Empty;
            var chatId = string.Empty;

            switch (tgBot)
            {
                case TgBot.AutoUPayChatId:
                    token = "8230600764:AAFxW8szIgwoFIVd9Zm_y067d5r0BrstGq8";
                    chatId = "-4883079448";
                    break;
            }

            if (token == string.Empty || chatId == string.Empty)
                return false;

            return SendMessage(token, chatId, message);
        }

        public static bool SendMessage(string apiToken, string chatId, string message)
        {
            try
            {
                using (var client = new WebClient())
                {
                    var fullRequest = $"https://api.telegram.org/bot{apiToken}/sendMessage?chat_id={chatId}&text={message}";
                    client.DownloadString(new Uri(fullRequest));
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
