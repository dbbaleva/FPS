using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FPS.ViewModels.Timekeeping
{
    public class AttendanceSummary
    {
        public string EmployeeName { get; set; }
        public DateTime From { get; set; }
        public WorkTime Late { get; set; }
        public WorkTime Overtime { get; set; }
        public DateTime To { get; set; }
        public WorkTime Undertime { get; set; }
        public WorkTime Worktime { get; set; }
        public ICollection<TimeAttendance> Attendance { get; set; }

        public AttendanceSummary()
        {
            Attendance = new HashSet<TimeAttendance>();
        }

        public static ICollection<AttendanceSummary> Create(IEnumerable<TimeAttendance> collection)
        {
            return collection.GroupBy(q => q.EmployeeName)
                .Select(q => new AttendanceSummary
                {
                    EmployeeName = q.Key,
                    From = q.Min(t => t.Date),
                    To = q.Max(t => t.Date),
                    Worktime = WorkTime.Parse(q.Select(t => t.Worktime).ToList()),
                    Overtime = WorkTime.Parse(q.Select(t => t.Overtime).ToList()),
                    Late = WorkTime.Parse(q.Select(t => t.Late).ToList()),
                    Undertime = WorkTime.Parse(q.Select(t => t.Undertime).ToList()),
                    Attendance = q.OrderBy(t => t.Date).ToList()
                }).OrderBy(q => q.EmployeeName).ToList();
        }
    }
}
