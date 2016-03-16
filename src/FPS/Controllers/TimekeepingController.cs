using System;
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
        public async Task<IActionResult> Download(DateTime from, DateTime to)
        {
            var model = await TimekeepingService.GetAttendanceAsync(from, to);
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
        public IActionResult Search(ICollection<TimeAttendance> list, string keyword)
        {
            var model = string.IsNullOrEmpty(keyword) ? list : list.Where(q => q.EmployeeName.ToUpper().Contains(keyword.ToUpper()));
            return PartialView("_SummaryRows", AttendanceSummary.Create(model.ToList()));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DataEntry(ICollection<TimeAttendance> timeAttendance, TimeAttendance data, bool overtime, string keyword)
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
            var model = string.IsNullOrEmpty(keyword) ? timeAttendance : timeAttendance.Where(q => q.EmployeeName.ToUpper().Contains(keyword.ToUpper()));
            return PartialView("_SummaryRows", AttendanceSummary.Create(model.ToList()));
        }
    }
}
