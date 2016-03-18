using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace FPS.Models
{
    [Table("EmployeeDepartments")]
    public class EmployeeDepartment
    {
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }

        public string DepartmentId { get; set; }
        public Department Department { get; set; }
    }
}
