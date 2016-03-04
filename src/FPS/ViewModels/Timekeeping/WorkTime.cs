using System;
using System.Collections.Generic;
using System.Linq;

namespace FPS.ViewModels.Timekeeping
{
    public struct WorkTime
    {
        public int Days { get; set; }
        public TimeSpan TimeSpan { get; set; }

        public static WorkTime Parse(ICollection<TimeSpan> collection)
        {
            return new WorkTime
            {
                Days = collection.Count(t => t > new TimeSpan(0, 0, 0)),
                TimeSpan = collection.Aggregate((a, b) => a + b)
            };
        }

        public override string ToString()
        {
            return TimeSpan.ToString("g");
        }
    }
}
