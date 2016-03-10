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
using Libraries;
using Microsoft.Extensions.OptionsModel;
using Database = FPS.Data.Database;

namespace FPS.Services
{
    public class BiometricsConfig
    {
        public string ConnectionString { get; set; }
        public string DeviceIP { get; set; }
        public int DevicePort { get; set; }
    }

    public class TimekeepingService : Disposable
    {
        private readonly Database _database;
        private readonly Biometrics _biometrics;
        public TimekeepingService(IOptions<BiometricsConfig> options)
        {
            _database = new Database(options.Value.ConnectionString, ConnectionType.OleDbConnection);
            _database.SetConverter(new TimeAttendanceConverter());
            _database.SetConverter(new TimeLogConverter());
            _database.SetConverter(new EmployeeConverter());

            _biometrics = new Biometrics(options.Value.DeviceIP, options.Value.DevicePort);

        }

        public IEnumerable<Employee> GetEmployees()
        {
            const string commandText = "SELECT [BADGENUMBER], [TITLE], [NAME] FROM USERINFO";
            return _database.Query<Employee>(commandText).OrderBy(q => q).ToList();
        }

        public Task<IEnumerable<Employee>> GetEmployeesAsync()
        {
            return Task.Run(() => GetEmployees());
        }

        public IEnumerable<TimeLog> GetTimeLogs(DateTime from, DateTime to)
        {
            var commandText = "SELECT u.BADGENUMBER, c.VERIFYCODE, c.CHECKTIME, c.CHECKTYPE, c.DATE " +
                "FROM (SELECT USERID, VERIFYCODE, CHECKTIME, CHECKTYPE, CDATE(FORMAT(CHECKTIME,'MM/DD/YYYY')) AS [DATE] FROM CHECKINOUT) AS c INNER JOIN USERINFO AS u ON (c.USERID = u.USERID) " +
                $"WHERE ((c.DATE >= #{from}#) AND (c.DATE <= #{to}#))";

            return _database.Query<TimeLog>(commandText)
                .OrderBy(q => q.TimeStamp).ToList();
        }

        public Task<IEnumerable<TimeLog>> GetTimeLogsAsync(DateTime from, DateTime to)
        {
            return Task.Run(() => GetTimeLogs(from, to));
        }


        public IEnumerable<TimeLog> GetTimeLogs()
        {
            return _biometrics.ReadLogs().Select(q => new TimeLog
            {
                EnrollNumber = q.EnrollNumber,
                Verification = q.Verification,
                TimeStamp = q.TimeStamp,
                TimeCode = q.TimeCode
            }).ToList();
        }

        public Task<IEnumerable<TimeLog>> GetTimeLogsAsync()
        {
            return Task.Run(() => GetTimeLogs());
        }

        public IEnumerable<TimeAttendance> GetAttendance(DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            const string commandText = "SELECT u.USERID, u.NAME, u.TITLE, u.DATE, cin.TIMEIN, cout.TIMEOUT FROM ((SELECT u.USERID, u.NAME, u.TITLE, CDATE(FORMAT(c.CHECKTIME,'MM/DD/YYYY')) AS [DATE] FROM USERINFO AS u INNER JOIN CHECKINOUT AS c ON u.USERID = c.USERID GROUP BY u.USERID, u.NAME, u.TITLE, CDATE(FORMAT(c.CHECKTIME,'MM/DD/YYYY')) ) AS u LEFT JOIN (SELECT USERID, MIN(CHECKTIME) AS TIMEIN, CDATE(FORMAT(CHECKTIME,'MM/DD/YYYY')) AS [DATE] FROM CHECKINOUT WHERE CHECKTYPE='I' GROUP BY USERID, CDATE(FORMAT(CHECKTIME,'MM/DD/YYYY'))) AS cin ON (u.DATE = cin.DATE) AND (u.USERID = cin.USERID)) LEFT JOIN (SELECT USERID, MAX(CHECKTIME) AS TIMEOUT, CDATE(FORMAT(CHECKTIME,'MM/DD/YYYY')) AS [DATE] FROM CHECKINOUT WHERE CHECKTYPE='O' GROUP BY USERID, CDATE(FORMAT(CHECKTIME,'MM/DD/YYYY'))) AS cout ON (u.DATE = cout.DATE) AND (u.USERID = cout.USERID) WHERE ((u.DATE >= @0) OR (@0 IS NULL)) AND ((u.DATE <= @1) OR (@1 IS NULL))";
            return _database.Query<TimeAttendance>(commandText, dateFrom, dateTo).OrderBy(q => q.Date).ThenBy(q => q.EmployeeName);
        }

        public async Task<IEnumerable<TimeAttendance>> GetAttendanceAsync(DateTime from, DateTime to)
        {
            var getDeviceLogs = GetTimeLogsAsync();
            var getDatabaseLogs = GetTimeLogsAsync(from, to);
            var getEmployees = GetEmployeesAsync();

            await Task.WhenAll(getDeviceLogs, getDatabaseLogs, getEmployees);

            var logs = getDeviceLogs.Result.Union(
                getDatabaseLogs.Result).ToList();

            var employees = getEmployees.Result.ToList();

            return TimeLog.CreateList(logs, employees, from, to);
        }

        public Task<Stream> ExportAsync(IEnumerable<TimeAttendance> enumerable)
        {
            return Task.Run(() => new TimekeepingReport(enumerable).Generate());
        }

        protected override void Release()
        {
            _database.Dispose();
            _biometrics.Dispose();
        }
    }
}
