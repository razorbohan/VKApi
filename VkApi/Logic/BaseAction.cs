using System;
using System.Net;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Args;
using VkApi.Utility;
using Timer = System.Timers.Timer;

namespace VkApi.Logic
{
    public abstract class BaseAction
    {
        private TelegramBotClient Telegram { get; }
        private string TelegramMessage { get; set; }

        public static Profile Profile { get; set; }

        public bool IsActionLimitAchieved = false;
        public int ActionCommittedCount = 0;
        public int ActionLimitCount;
        //private byte GroupViewName = 2;

        protected BaseAction()
        {
            if (Telegram != null) return;

            Telegram = new TelegramBotClient(Profile.TelegramToken);
            Telegram.OnMessage += BotOnMessageReceived;
        }

        //TODO: ловить сообщения отдельно от Fake'a и Real'a
        public void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            //if (e1.Message.Text.StartsWith(who.ToLower()))
            TelegramMessage = messageEventArgs.Message.Text /*.Replace(who + " ", "")*/;
        }

        public string SolveCaptcha(Uri captchaLink)
        {
            try
            {
                string captchaInText;

                if (!Profile.IsUseAntiCaptcha)
                {
                    var timerCaptcha = new Timer { Interval = 1000 };

                    var timerCaptchaCounter = 0;
                    timerCaptcha.Elapsed += (sender, e) =>
                        timerCaptchaCounter++;

                    TelegramMessage = "";
                    Telegram.SendTextMessageAsync(Profile.TelegramChatId, $"{Profile.ProfileName}: {captchaLink}");
                    timerCaptcha.Start();
                    Telegram.StartReceiving();

                    while (true/*!MainLogic.Stop*/)
                    {
                        Thread.Sleep(1000);

                        if (timerCaptchaCounter <= 600 && string.IsNullOrEmpty(TelegramMessage) && !Profile.IsUseAntiCaptcha)
                            continue;

                        captchaInText = TelegramMessage;

                        timerCaptcha.Stop();
                        break;
                    }

                    Telegram.StopReceiving();
                    //if (string.IsNullOrEmpty(captchaInText))
                    //    Telegram.SendTextMessageAsync(CurrentProfile.TelegramChatId,
                    //            $"{CurrentProfile.ProfileName}: Отослано на AntiCaptcha");
                    //else 
                    return captchaInText;
                }

                new WebClient().DownloadFile(captchaLink, $"captcha_{Profile.ProfileName}.jpg");
                captchaInText = ImageToText($"captcha_{Profile.ProfileName}.jpg");

                if (!captchaInText.Contains("ERROR")) return captchaInText;

                Logger.Log($"Error in anticaptcha: {captchaInText}");

                if (captchaInText.Contains("NO_SLOT_AVAILABLE") ||
                    captchaInText.Contains("ERROR_ZERO_BALANCE"))
                    Profile.IsUseAntiCaptcha = false;
            }
            catch (Exception ex)
            {
                Logger.ErrorLog(ex);
            }

            return null;
        }

        public string ImageToText(string imageName)
        {
            var antiCaptcha = new AntiCaptcha(Profile.AntiCaptchaApiKey, imageName);

            string result;
            var id = antiCaptcha.UploadFile();

            if (id.StartsWith("OK|"))
            {
                id = id.Replace("OK|", "");
                result = antiCaptcha.Recognize(id);

                if (result.StartsWith("OK|"))
                    result = result.Replace("OK|", "");
            }
            else result = id;

            return result;
        }
    }
}
