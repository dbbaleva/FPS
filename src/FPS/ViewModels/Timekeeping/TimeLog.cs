using System;
using System.Collections.Generic;
using System.Linq;

namespace FPS.ViewModels.Timekeeping
{
    public class TimeLog
    {
        public int EnrollNumber { get; set; }
        public int TimeCode { get; set; }
        public DateTime TimeStamp { get; set; }
        public int Verification { get; set; }

        public static IEnumerable<TimeAttendance> CreateList(IEnumerable<TimeLog> logs, ICollection<Employee> employees, DateTime from, DateTime to)
        {
            var timeLogs = logs.OrderBy(q => q.TimeStamp).ToList();
            for (var date = from; date <= to; date = date.AddDays(1))
            {
                if (date.DayOfWeek == DayOfWeek.Sunday)
                    continue;

                foreach (var ee in employees)
                {
                    var attendance = new TimeAttendance
                    {
                        EmployeeName = ee.EmployeeName,
                        EmployeeTitle = ee.EmployeeTitle,
                        Date = date
                    };

                    var attLog = timeLogs.Where(l => l.EnrollNumber == ee.BadgeNumber &&
                                                 l.TimeStamp.Date == attendance.Date).ToList();

                    if (attLog.Any())
                    {
                        var cin = attLog.FirstOrDefault(l => l.TimeCode == 0);
                        var cout = attLog.LastOrDefault(l => l.TimeCode == 1);

                        var cin2 = cin;
                        var cout2 = cout;

                        // no timein but with timeout
                        if (cin == null)
                        {
                            if (cout != null)
                            {
                                cin2 =
                                    attLog.FirstOrDefault(l => l.TimeStamp < cout.TimeStamp);
                            }
                        }

                        // no timeout but with timein
                        if (cout == null)
                        {
                            if (cin != null && cin.TimeStamp.Date != DateTime.Today)
                            {
                                cout2 =
                                    attLog.LastOrDefault(l => l.TimeStamp > cin.TimeStamp);
                            }
                        }


                        // get timeout
                        if (cin != null || cin2 != null)
                        {
                            var timeIn = (cin ?? cin2).TimeStamp;
                            attendance.TimeIn = timeIn.Date + new TimeSpan(timeIn.Hour, timeIn.Minute, 0); // discard seconds
                        }

                        // get timein
                        if (cout != null || cout2 != null)
                        {
                            var timeOut = (cout ?? cout2).TimeStamp;
                            attendance.TimeOut = timeOut.Date + new TimeSpan(timeOut.Hour, timeOut.Minute, 0); // discard seconds
                        }

                        attendance.Compute();
                    }
                    else
                    {
                        // absent
                        attendance.TimeIn = date;
                        attendance.TimeOut = date;
                        attendance.Remarks = $"ABSENT ({date.ToShortDateString()})";
                    }

                    yield return attendance;
                }
            }
        }
    }
}

