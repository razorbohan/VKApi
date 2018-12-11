using System;
using VkApi.Utility;
using VkNet.Exception;
using VkNet.Model.RequestParams;

namespace VkApi.Logic
{
    public enum GroupActionType
    {
        Post = 1,
    }

    public class GroupActionLogic : BaseAction
    {
        public GroupActionType ActionType { get; set; }

        public bool MakeAction(VkNet.VkApi vk, long groupId, string post, long sid = 0, string captchaInText = null)
        {
            try
            {
                Logger.Log($"{ActionType} {groupId}");

                //var definedActionType = (GroupActionType) Enum.Parse(typeof (GroupActionType), actionType.ToString());
                switch (ActionType)
                {
                    case GroupActionType.Post:
                        vk.Wall.Post(new WallPostParams
                        {
                            OwnerId = -groupId,
                            Message = post,
                            CaptchaSid = sid,
                            CaptchaKey = captchaInText
                        }); return true;
                    default: throw new WrongActionTypeException(ActionType.ToString());
                }
            }
            catch (CaptchaNeededException ex)
            {
                Logger.Log(ex.Message);

                var solvedCaptcha = SolveCaptcha(ex.Img);

                if (!string.IsNullOrEmpty(solvedCaptcha))
                    MakeAction(vk, groupId, post, sid: ex.Sid, captchaInText: solvedCaptcha);
                else return false;
            }
            catch (Exception ex)
            {
                Logger.ErrorLog(ex);
            }

            return false;
        }
    }
}