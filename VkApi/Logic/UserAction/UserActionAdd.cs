namespace VkApi.Logic.UserAction
{
    public class UserActionAdd : BaseUserAction
    {
        public UserActionAdd()
        {
            ActionTypeEnum = UserActionTypeEnum.Add;
        }

        protected override void Act(VkNet.VkApi vk, ref User user, long sid = 0, string captchaInText = null)
        {
            vk.Friends.Add(user.Url, captchaSid: sid, captchaKey: captchaInText);
        }

        public override BaseUserAction Clone()
        {
            return new UserActionAdd();
        }
    }
}
