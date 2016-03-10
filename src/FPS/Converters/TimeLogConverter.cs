using System;
using System.Data.Common;
using FPS.Data;
using FPS.ViewModels.Timekeeping;

namespace FPS.Converters
{
    public class TimeLogConverter : IEntityConverter<TimeLog>
    {
        public TimeLog ConvertToEntity(DbDataRecord record)
        {
            return new TimeLog
            {
                EnrollNumber = int.Parse(record["BADGENUMBER"].ToString()),
                Verification = int.Parse(record["VERIFYCODE"].ToString()),
                TimeStamp = DateTime.Parse(record["CHECKTIME"].ToString()),
                TimeCode = record["CHECKTYPE"]?.ToString() == "I" ? 0 : 1
            };
        }
    }
}