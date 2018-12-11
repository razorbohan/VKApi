using System;
using VkApi.Utility;
using VkNet.Exception;

namespace VkApi.Logic.UserAction
{
    public enum UserActionTypeEnum
    {
        Add = 1,
        Like,
        Write
    }

    public abstract class BaseUserAction : BaseAction
    {
        public UserActionTypeEnum ActionTypeEnum { get; set; }

        /// <summary>
        /// Произведение конкретного действия.
        /// </summary>
        /// <param name="vk">Экземпляр ВК-сущности</param>
        /// <param name="user">Пользователь, над которым производится действие.</param>
        /// <param name="sid">ID каптчи.</param>
        /// <param name="captchaInText">Текст разгаданной каптчи.</param>
        /// <returns>Возвращает пользователя типа UserData.</returns>
        public bool MakeAction(VkNet.VkApi vk, ref User user, long sid = 0, string captchaInText = null)
        {
            try
            {
                Logger.Log($"{ActionTypeEnum} {user.FullName}({user.Url})");

                Act(vk, ref user, sid, captchaInText);

                ActionCommittedCount++;
                return true;
            }
            catch (CaptchaNeededException ex)
            {
                Logger.Log(ex.Message);

                var solvedCaptcha = SolveCaptcha(ex.Img);
                return !string.IsNullOrEmpty(solvedCaptcha) && MakeAction(vk, ref user, sid: ex.Sid, captchaInText: solvedCaptcha);
            }
            catch (Exception ex)
            {
                Logger.ErrorLog(ex);

                //TODO to special typed Exceprion?
                if (ex.Message.Contains("Unknown error occurred") ||
                    ex.Message.Contains("Flood control") ||
                    ex.Message.Contains("Permission to perform this action is denied"))
                {
                    IsActionLimitAchieved = true;
                }

                if (ex.Message.Contains("privacy settings") ||
                    ex.Message.Contains("user not found") ||
                    ex.Message.Contains("put you on their blacklist") ||
                    ex.Message.Contains("user deactivated") ||
                    ex.Message.Contains("Access denied"))

                    user.UserActions.Add(new VkApi.UserAction
                    {
                        ProfileId = Profile.Id,
                        ActionTypeId = 0,
                        UpdateDateTime = DateTime.Now
                    });
            }

            return false;
        }

        protected abstract void Act(VkNet.VkApi vk, ref User user, long sid = 0, string captchaInText = null);

        public abstract BaseUserAction Clone();
    }
}
