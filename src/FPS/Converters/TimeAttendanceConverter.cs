using System;
using System.Data.Common;
using FPS.Data;
using FPS.ViewModels.Timekeeping;

namespace FPS.Converters
{
    public class TimeAttendanceConverter : IEntityConverter<TimeAttendance>
    {
        public TimeAttendance ConvertToEntity(DbDataRecord record)
        {
            return new TimeAttendance
            {
                EmployeeName = record["NAME"].ToString(),
                EmployeeTitle = record["TITLE"].ToString(),
                Date = DateTime.Parse(record["DATE"].ToString()),
                TimeIn = ConvertToDate(record["TIMEIN"]),
                TimeOut = ConvertToDate(record["TIMEOUT"])
            }.Compute();
        }

        public static DateTime? ConvertToDate(object value)
        {
            DateTime result;
            if (DateTime.TryParse(value?.ToString(), out result))
                return result;
            return null;
        }
    }
}
