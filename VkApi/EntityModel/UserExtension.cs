
namespace VkApi
{
    public partial class User 
    {
        public bool Equals(User user)
        {
            if (this.FullName != user.FullName) return false;
            if (this.Status != user.Status) return false;
            if (this.Followers != user.Followers) return false;
            if (this.Common != user.Common) return false;

            return true;
        }
    }
}
