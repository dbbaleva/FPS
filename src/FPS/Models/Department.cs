using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace FPS.Models
{
    [Table("Departments")]
    public class Department
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public List<EmployeeDepartment> EmployeesDepartments { get; set; }
    }
}
