using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;
using FPS.Services;
using FPS.ViewModels.Timekeeping;
using Microsoft.AspNet.Mvc;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace FPS.Api
{
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class TimekeepingController : Controller
    {
        private readonly TimekeepingService _service;
        public TimekeepingController(TimekeepingService service)
        {
            _service = service;
        }

        public async Task<IEnumerable<TimeAttendance>> Get(DateTime? from, DateTime? to)
        {
            return await _service.GetAttendanceAsync(from ?? DateTime.Today, to ?? DateTime.Today);
        }

        [Route("Employees")]
        public async Task<IEnumerable<object>> GetEmployees(string q, int? s)
        {
            var model = await _service.GetEmployeesAsync();
            model = model.OrderBy(ee => ee.EmployeeName);
            if (!string.IsNullOrEmpty(q))
                model = model.Where(ee => ee.EmployeeName.ToUpper().StartsWith(q.ToUpper()));
            if (s != null)
                model = model.Take((int)s);
            return model.Select(ee => new 
            {
                id = ee.UserId,
                text = ee.EmployeeName
            }).ToList();
        }
    }
}
