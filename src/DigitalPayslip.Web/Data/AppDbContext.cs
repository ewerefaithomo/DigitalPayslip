
using Microsoft.EntityFrameworkCore;
using DigitalPayslip.Web.Models;

namespace DigitalPayslip.Web.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<UserAccount> Users { get; set; }
        public DbSet<EmployeeMapping> EmployeeMappings { get; set; }
    }
}
