
using System.ComponentModel.DataAnnotations;

namespace DigitalPayslip.Web.Models
{
    public class UserAccount
    {
        [Key]
        public int Id { get; set; }
        [Required, EmailAddress]
        public string Email { get; set; }
        [Required]
        public string PasswordHash { get; set; }
        public bool IsVerified { get; set; }
        public string? VerificationToken { get; set; }
    }
}
