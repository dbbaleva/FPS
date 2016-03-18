using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FPS.Models
{
    [Table("Employees")]
    public class Employee
    {
        public int Id { get; set; }

        [MaxLength(30)]
        public string FirstName { get; set; }

        [MaxLength(30)]
        public string MiddleName { get; set; }

        [MaxLength(30)]
        public string LastName { get; set; }

        [MaxLength(10)]
        public string Suffix { get; set; }

        [MaxLength(10)]
        public string Gender { get; set; }

        public string BirthDate { get; set; }

        [MaxLength(10)]
        public string CivilStatus { get; set; }

        [MaxLength(500)]
        public string Address { get; set; }

        [MaxLength(50)]
        public string Position { get; set; }

        [MaxLength(15)]
        public string Status { get; set; }

        public UserAccount UserAccount { get; set; }

        public List<EmployeeDepartment> EmployeeDepartments { get; set; }
    }
}