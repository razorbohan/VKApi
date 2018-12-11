using System;
using System.Linq;
using VkNet.Enums.SafetyEnums;
using VkNet.Model.RequestParams;

namespace VkApi.Logic.UserAction
{
    public class UserActionLike : BaseUserAction
    {
        public UserActionLike()
        {
            ActionTypeEnum = UserActionTypeEnum.Like;
        }

        protected override void Act(VkNet.VkApi vk, ref User user, long sid = 0, string captchaInText = null)
        {
            var profilePhotos = vk.Photo.Get(new PhotoGetParams
            {
                OwnerId = user.Url,
                AlbumId = PhotoAlbumType.Profile
            });

            if (profilePhotos.Any())
            {
                vk.Likes.Add(new LikesAddParams
                {
                    Type = LikeObjectType.Photo,
                    OwnerId = user.Url,
                    ItemId = Convert.ToInt64(profilePhotos[0].Id),
                    CaptchaSid = sid,
                    CaptchaKey = captchaInText
                });
            }
        }

        public override BaseUserAction Clone()
        {
            return new UserActionLike();
        }
    }
}
