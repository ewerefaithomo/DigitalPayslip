using System.Collections.Generic;
using System.Threading.Tasks;

namespace DigitalPayslip.Web.Services
{
    public interface IPayslipProcessor
    {
        Task<(int success, int failed)> ProcessAsync(
            string pdfPath,
            string outputDirectory,
            IEnumerable<(string ippis, string email)> mappings);
    }
}
