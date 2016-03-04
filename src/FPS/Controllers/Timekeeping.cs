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
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Download(DateTime? from, DateTime? to)
        {
            if (from == null || to == null)
                return new NoContentResult();

            var model = await TimekeepingService.GetAttendanceAsync(from, to);
            return PartialView("_Download", model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(IEnumerable<TimeAttendance> timeAttendance)
        {
            if (timeAttendance == null)
                return new NoContentResult();

            var stream = await TimekeepingService.ExportAsync(timeAttendance);
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Timekeeping-{DateTime.Today:MMddyyyy}.xlsx");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Search(IEnumerable<TimeAttendance> timeAttendance, string keyword)
        {
            var model = string.IsNullOrEmpty(keyword) ? timeAttendance : timeAttendance.Where(q => q.EmployeeName.ToUpper().Contains(keyword.ToUpper()));
            return PartialView("_Rows", AttendanceSummary.Create(model.ToList()));
        }
    }
}
