using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FPS.ViewModels.Timekeeping
{
    public class Employee : IComparable
    {
        public int Id { get; set; }
        public int BadgeNumber { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeTitle { get; set; }

        public int CompareTo(object obj)
        {
            var ee = (Employee)obj;
            if (BadgeNumber > ee.BadgeNumber)
                return 1;
            if (BadgeNumber < ee.BadgeNumber)
                return -1;
            return 0;
        }
    }
}
