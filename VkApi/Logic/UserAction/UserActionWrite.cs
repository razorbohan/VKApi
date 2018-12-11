using System;
using VkNet.Model.RequestParams;

namespace VkApi.Logic.UserAction
{
    public class UserActionWrite : BaseUserAction
    {
        public UserActionWrite()
        {
            ActionTypeEnum = UserActionTypeEnum.Write;
        }

        protected override void Act(VkNet.VkApi vk, ref User user, long sid = 0, string captchaInText = null)
        {
            vk.Messages.Send(new MessagesSendParams
            {
                UserId = Convert.ToInt64(user.Url),
                Message = ":)",
                CaptchaSid = sid,
                CaptchaKey = captchaInText
            });
        }

        public override BaseUserAction Clone()
        {
            return new UserActionWrite();
        }
    }
}
