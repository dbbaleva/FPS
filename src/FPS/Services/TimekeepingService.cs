using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FPS.Converters;
using FPS.Data;
using FPS.Reports;
using FPS.ViewModels.Timekeeping;
using Microsoft.Extensions.OptionsModel;
using Database = FPS.Data.Database;

namespace FPS.Services
{
    public class BiometricsConfig
    {
        public string ConnectionString { get; set; }
    }
    public class TimekeepingService
    {
        private readonly Database _database;
        public TimekeepingService(IOptions<BiometricsConfig> options)
        {
            _database = new Database(options.Value.ConnectionString, ConnectionType.OleDbConnection);
            _database.SetConverter(new TimeAttendanceConverter());
            _database.SetConverter(new StringConverter());
        }

        public IEnumerable<string> GetEmployeeNames()
        {
            const string commandText = "SELECT NAME FROM USERINFO";
            return _database.Query<string>(commandText).OrderBy(q => q);
        }

        public Task<IEnumerable<string>> GetEmployeeNamesAsync()
        {
            return Task.Run(() => GetEmployeeNames());
        }

        public IEnumerable<TimeAttendance> GetAttendance(DateTime? dateFrom = null, DateTime? dateTo = null, bool strict = false)
        {
            const string commandText = "SELECT u.USERID, u.NAME, u.TITLE, u.DATE, cin.TIMEIN, cout.TIMEOUT FROM ((SELECT u.USERID, u.NAME, u.TITLE, CDATE(FORMAT(c.CHECKTIME,'MM/DD/YYYY')) AS [DATE] FROM USERINFO AS u INNER JOIN CHECKINOUT AS c ON u.USERID = c.USERID GROUP BY u.USERID, u.NAME, u.TITLE, CDATE(FORMAT(c.CHECKTIME,'MM/DD/YYYY')) ) AS u LEFT JOIN (SELECT USERID, MIN(CHECKTIME) AS TIMEIN, CDATE(FORMAT(CHECKTIME,'MM/DD/YYYY')) AS [DATE] FROM CHECKINOUT WHERE CHECKTYPE='I' GROUP BY USERID, CDATE(FORMAT(CHECKTIME,'MM/DD/YYYY'))) AS cin ON (u.DATE = cin.DATE) AND (u.USERID = cin.USERID)) LEFT JOIN (SELECT USERID, MAX(CHECKTIME) AS TIMEOUT, CDATE(FORMAT(CHECKTIME,'MM/DD/YYYY')) AS [DATE] FROM CHECKINOUT WHERE CHECKTYPE='O' GROUP BY USERID, CDATE(FORMAT(CHECKTIME,'MM/DD/YYYY'))) AS cout ON (u.DATE = cout.DATE) AND (u.USERID = cout.USERID) WHERE ((u.DATE >= @0) OR (@0 IS NULL)) AND ((u.DATE <= @1) OR (@1 IS NULL))";
            return _database.Query<TimeAttendance>(commandText, dateFrom, dateTo).OrderBy(q => q.Date).ThenBy(q => q.EmployeeName);
        }

        public Task<IEnumerable<TimeAttendance>> GetAttendanceAsync(DateTime? dateFrom = null, DateTime? dateTo = null, bool strict = false)
        {
            return Task.Run(() => GetAttendance(dateFrom, dateTo, strict));
        }

        public Task<Stream> ExportAsync(IEnumerable<TimeAttendance> enumerable)
        {
            return Task.Run(() => new TimekeepingReport(enumerable).Generate());
        }
    }
}
