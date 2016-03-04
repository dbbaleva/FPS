using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FPS.ViewModels.Timekeeping
{
    public class TimeAttendance
    {
        public string EmployeeTitle { get; set; }

        public string EmployeeName { get; set; }

        public DateTime Date { get; set; }

        public DateTime? TimeIn { get; set; }

        public DateTime? TimeOut { get; set; }

        public TimeSpan Worktime { get; set; }

        public TimeSpan Late { get; set; }

        public TimeSpan Overtime { get; set; }

        public TimeSpan Undertime { get; set; }

        public string Remarks { get; set; }

        public TimeAttendance ComputeOT()
        {
            // compute worktime
            Worktime = (TimeSpan)(TimeOut - TimeIn);
            Overtime = (TimeSpan)(TimeOut - TimeIn);
            Remarks = "Overtime";

            return this;
        }

        private static bool IsWeekDay(DateTime date)
        {
            return date.DayOfWeek >= DayOfWeek.Monday && date.DayOfWeek <= DayOfWeek.Friday;
        }

        public TimeAttendance Compute()
        {
            var date = Date;

            var timein = string.IsNullOrEmpty(EmployeeTitle) && IsWeekDay(date)
                ? new DateTime(date.Year, date.Month, date.Day, 8, 30, 0)
                : new DateTime(date.Year, date.Month, date.Day, 9, 00, 0);

            var timeout = date.DayOfWeek == DayOfWeek.Saturday
                ? new DateTime(date.Year, date.Month, date.Day, 12, 00, 0)
                : new DateTime(date.Year, date.Month, date.Day, 17, 30, 0);

            var gracePeriod = string.IsNullOrEmpty(EmployeeTitle) && IsWeekDay(date)
                ? new TimeSpan(0, 10, 0)
                : new TimeSpan();

            var overtime = IsWeekDay(date)
                ? new DateTime(date.Year, date.Month, date.Day, 19, 00, 0)
                : new DateTime(date.Year, date.Month, date.Day, 14, 00, 0);

            var lunch = new DateTime(date.Year, date.Month, date.Day, 12, 00, 0);
            var afterLunch = new DateTime(date.Year, date.Month, date.Day, 13, 00, 0);

            var remarks = new List<string>();

            // offset 60 minutes when employee did not time in/out
            if (TimeIn == null)
            {
                TimeIn = timein.AddMinutes(60);
                remarks.Add("NO TIMEIN");
            }

            if (TimeOut == null)
            {
                TimeOut = timeout.AddMinutes(-60);
                remarks.Add("NO TIMEOUT");
            }

            // compute worktime
            Worktime = (TimeSpan)(TimeOut - TimeIn);

            // if weekdays then deduct one hour from worktime as lunch break
            if (IsWeekDay(date))
            {
                if (TimeOut > lunch && TimeOut < afterLunch)
                {
                    Worktime = Worktime.Add((TimeSpan)(TimeOut - lunch));
                }
                if (TimeOut >= afterLunch && Worktime >= new TimeSpan(1, 0, 0))
                {
                    Worktime -= new TimeSpan(1, 0, 0);
                }
            }

            // late if beyond grace period
            if (TimeIn > timein.Add(gracePeriod))
            {
                Late = (TimeSpan)(TimeIn - timein);
                remarks.Add("LATE");
            }
            // overtime
            if (TimeOut > overtime)
            {
                Overtime = (TimeSpan)(TimeOut - overtime);
                // deduct 5 minutes from overtime
                if (Overtime >= new TimeSpan(0, 5, 0))
                    Overtime = Overtime.Add(new TimeSpan(0, -5, 0));
                remarks.Add("OVERTIME");
            }
            // undertime
            if (TimeOut < timeout)
            {
                Undertime = (TimeSpan)(timeout - TimeOut);
                remarks.Add("UNDERTIME");
            }

            if (remarks.Any())
                Remarks = string.Join(" | ", remarks);

            return this;
        }
    }
}
