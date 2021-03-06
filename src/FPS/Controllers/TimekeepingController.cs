﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FPS.Services;
using FPS.ViewModels.Timekeeping;
using Microsoft.AspNet.Mvc;


namespace FPS.Controllers
{
    public class TimekeepingController : Controller
    {
        public TimekeepingService TimekeepingService { get; }
        public TimekeepingController(TimekeepingService timekeepingService)
        {
            TimekeepingService = timekeepingService;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            return View(new AttendanceSummary[] {});
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Download(TimeAttendance filter)
        {
            var result = await TimekeepingService.GetAttendanceAsync(filter.TimeIn ?? DateTime.Today, filter.TimeOut ?? DateTime.Today);
            var model = FilterRecords(result, employeeId:filter.EmployeeId, remarks: filter.Remarks);
            return PartialView("_Table", AttendanceSummary.Create(model));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(ICollection<TimeAttendance> timeAttendance)
        {
            if (timeAttendance == null)
                return new NoContentResult();

            var stream = await TimekeepingService.ExportAsync(timeAttendance);
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Timekeeping-{DateTime.Today:MMddyyyy}.xlsx");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Search(ICollection<TimeAttendance> list, TimeAttendance data)
        {
            var model = list.AsEnumerable();
            if (data != null)
            {
                model = FilterRecords(list.AsEnumerable(), data.EmployeeId, data.TimeIn, data.TimeOut, data.Remarks);
            }
            
            return PartialView("_SummaryRows", AttendanceSummary.Create(model.ToList()));
        }

        private IEnumerable<TimeAttendance> FilterRecords(IEnumerable<TimeAttendance> records, int? employeeId = null, DateTime? from =  null, DateTime? to = null, string remarks = null)
        {
            if (employeeId != null && employeeId != 0)
                records = records.Where(q => q.EmployeeId == employeeId);
            if (from != null)
                records = records.Where(q => q.Date >= from);
            if (to != null)
                records = records.Where(q => q.Date <= to);
            if (!string.IsNullOrEmpty(remarks))
                records = records.Where(q => (q.Remarks ?? "").ToUpper().Contains(remarks));
            
            return records;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DataEntry(ICollection<TimeAttendance> timeAttendance, TimeAttendance data, bool overtime)
        {
            if (data.TimeIn != null && data.TimeOut != null)
            {
                data.Date = data.TimeIn.Value.Date;
                if (overtime)
                    data.ComputeOT();
                else
                    data.Compute();
                timeAttendance.Add(data);
            }
            return PartialView("_SummaryRows", AttendanceSummary.Create(timeAttendance));
        }
    }
}
