
namespace DigitalPayslip.Web.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body, string? attachmentPath = null);
    }
}
