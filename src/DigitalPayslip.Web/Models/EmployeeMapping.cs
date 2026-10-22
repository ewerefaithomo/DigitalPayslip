
using System.ComponentModel.DataAnnotations;

namespace DigitalPayslip.Web.Models
{
    public class EmployeeMapping
    {
        [Key]
        public int Id { get; set; }
        public string IPPISNO { get; set; }
        public string EmployeeName { get; set; }
        public string Email { get; set; }
    }
}
