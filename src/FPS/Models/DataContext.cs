using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Data.Entity;
using FPS.Models;

namespace FPS.Models
{
    public class DataContext : IdentityDbContext<UserAccount>
    {
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            //builder.Entity<UserAccount>().ToTable("aspnet_Users");
            //builder.Entity<IdentityRole>().ToTable("aspnet_Roles");
            //builder.Entity<IdentityRoleClaim<string>>().ToTable("aspnet_RoleClaims");
            //builder.Entity<IdentityUserRole<string>>().ToTable("aspnet_UserRoles");
            //builder.Entity<IdentityUserClaim<string>>().ToTable("aspnet_UserClaims");
            //builder.Entity<IdentityUserLogin<string>>().ToTable("aspnet_UserLogins");


            // Many-to-many relationships without an entity class to represent the join table are not yet supported. 
            // https://ef.readthedocs.org/en/latest/modeling/relationships.html#many-to-many
            builder.Entity<EmployeeDepartment>()
                .HasKey(t => new { t.EmployeeId, t.DepartmentId });

            builder.Entity<EmployeeDepartment>()
               .HasOne(ed => ed.Employee)
               .WithMany(e => e.EmployeeDepartments)
               .HasForeignKey(ed => ed.EmployeeId);

            builder.Entity<EmployeeDepartment>()
                .HasOne(ed => ed.Department)
                .WithMany(d => d.EmployeesDepartments)
                .HasForeignKey(ed => ed.DepartmentId);
        }
    }
}
