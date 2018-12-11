using System;
using System.Collections.Generic;
using VkApi.Logic.UserAction;

namespace VkApi.Logic
{
    public class WorkingPeriod
    {
        public short PeriodNumber { get; set; }
        public TimeSpan StartPoint { get; set; }
        public List<BaseUserAction> UserActions { get; set; }

        public WorkingPeriod(short periodNumber, TimeSpan startPoint, List<BaseUserAction> userActions)
        {
            PeriodNumber = periodNumber;
            StartPoint = startPoint;
            UserActions = userActions;
        }
    }
}
