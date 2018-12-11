using System;

namespace VkApi.Utility
{
    public class WrongAuthException : Exception
    {
        public WrongAuthException(string message)
          : base(message)
        { }
    }

    public class WrongDataException : Exception
    {
        public WrongDataException(string message)
          : base(message)
        { }
    }

    public class LimitAchievedException : Exception
    {
        public LimitAchievedException(string message) : base(message) { }
    }

    public class WrongActionTypeException : Exception
    {
        public WrongActionTypeException(string message) : base(message) { }
    }
}
