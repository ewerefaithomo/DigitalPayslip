using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DigitalPayslip.Web.Data;
using UglyToad.PdfPig;
//using PdfPig.Content;
using UglyToad.PdfPig.Content;

namespace DigitalPayslip.Web.Services
{
    public class PayslipProcessor : IPayslipProcessor
    {
        private readonly IEmailService _email;

        public PayslipProcessor(IEmailService email)
        {
            _email = email;
        }

        public async Task<(int success, int failed)> ProcessAsync(
            string pdfPath,
            string outputFolder,
            IEnumerable<(string ippis, string email)> mapping)
        {
            if (!File.Exists(pdfPath))
                throw new FileNotFoundException("Uploaded PDF not found.", pdfPath);

            Directory.CreateDirectory(outputFolder);
            var map = mapping.ToDictionary(m => m.ippis, m => m.email);

            int success = 0, failed = 0;

            // Open PDF using PdfPig
            using var doc = PdfDocument.Open(pdfPath);

            // Collect page groups for each employee
            var groups = new List<(int startPage, int endPage, string headerText)>();
            string? currentHeader = null;
            int startPage = 1;

            for (int i = 1; i <= doc.NumberOfPages; i++)
            {
                var page = doc.GetPage(i);
                var text = page.Text;

                // Detect new payslip start
                if (text.Contains("Employee Name:") || text.Contains("IPPIS"))
                {
                    if (currentHeader != null)
                    {
                        groups.Add((startPage, i - 1, currentHeader));
                    }
                    startPage = i;
                    currentHeader = text;
                }
            }

            // Add final group
            if (currentHeader != null)
            {
                groups.Add((startPage, doc.NumberOfPages, currentHeader));
            }

            // Process each payslip
            foreach (var g in groups)
            {
                var ippisMatch = Regex.Match(g.headerText, @"\d{5,}");
                if (!ippisMatch.Success)
                {
                    failed++;
                    continue;
                }

                var ippis = ippisMatch.Value;
                var filename = $"Payslip_{ippis}.pdf";
                var outPath = Path.Combine(outputFolder, filename);

                // Extract employee pages
                using (var inputDoc = PdfSharpCore.Pdf.IO.PdfReader.Open(
                    pdfPath, PdfSharpCore.Pdf.IO.PdfDocumentOpenMode.Import))
                {
                    using var outDoc = new PdfSharpCore.Pdf.PdfDocument();
                    for (int p = g.startPage - 1; p <= g.endPage - 1; p++)
                    {
                        outDoc.AddPage(inputDoc.Pages[p]);
                    }
                    outDoc.Save(outPath);
                }

                // Email payslip
                if (map.TryGetValue(ippis, out var email))
                {
                    try
                    {
                        await _email.SendEmailAsync(
                            email,
                            "Your Payslip",
                            "Attached is your payslip.",
                            outPath);

                        success++;
                    }
                    catch
                    {
                        failed++;
                    }
                }
                else
                {
                    failed++;
                }
            }

            return (success, failed);
        }
    }
}
