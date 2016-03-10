using System.Data.Common;
using FPS.Data;
using FPS.ViewModels.Timekeeping;

namespace FPS.Converters
{
    public class EmployeeConverter : IEntityConverter<Employee>
    {
        public Employee ConvertToEntity(DbDataRecord record)
        {
            return new Employee
            {
                BadgeNumber = int.Parse(record["BADGENUMBER"].ToString()),
                EmployeeTitle = record["TITLE"]?.ToString(),
                EmployeeName = record["NAME"]?.ToString()
            };
        }
    }
}
