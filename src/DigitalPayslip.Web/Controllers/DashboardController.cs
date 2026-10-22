using Microsoft.AspNetCore.Mvc;
using DigitalPayslip.Web.Data;
using DigitalPayslip.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace DigitalPayslip.Web.Controllers
{
    public class DashboardController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IPayslipProcessor _processor;

        public DashboardController(AppDbContext db, IPayslipProcessor processor)
        {
            _db = db;
            _processor = processor;
        }

        // ----------------------------
        // Dashboard Home
        // ----------------------------
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("user") == null)
                return RedirectToAction("Login", "Account");

            return View();
        }

        // ----------------------------
        // Upload Payslip PDF
        // ----------------------------
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile payslipFile)
        {
            if (payslipFile == null || payslipFile.Length == 0)
            {
                TempData["UploadMessage"] = "No file selected. Upload failed.";
                return RedirectToAction("Index");
            }

            try
            {
                // Ensure Uploaded folder exists
                var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploaded");
                if (!Directory.Exists(uploadFolder))
                    Directory.CreateDirectory(uploadFolder);

                var filePath = Path.Combine(uploadFolder, payslipFile.FileName);

                // Save file
                using (var fs = new FileStream(filePath, FileMode.Create))
                {
                    await payslipFile.CopyToAsync(fs);
                }

                // Verify file
                if (System.IO.File.Exists(filePath))
                {
                    TempData["UploadMessage"] = "Payslip successfully uploaded to the server!";
                    TempData["file"] = filePath; // store path for processing
                }
                else
                {
                    TempData["UploadMessage"] = "Upload failed: file not saved.";
                }
            }
            catch (Exception ex)
            {
                TempData["UploadMessage"] = $"Upload failed: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        // ----------------------------
        // Process Payslip PDF
        // ----------------------------
        [HttpPost]
        public async Task<IActionResult> Process()
        {
            var pdf = TempData["file"] as string;

            if (string.IsNullOrEmpty(pdf))
            {
                TempData["ProcessMessage"] = "No payslip uploaded to process.";
                return RedirectToAction("Index");
            }

            try
            {
                var rawMappings = await _db.EmployeeMappings
                    .Select(x => new { x.IPPISNO, x.Email })
                    .ToListAsync();

                var mappings = rawMappings
                    .Select(x => (ippis: x.IPPISNO, email: x.Email))
                    .ToList();

                var outputFolder = Path.Combine(Directory.GetCurrentDirectory(), "Processed");
                Directory.CreateDirectory(outputFolder);

                (int successCount, int failureCount) =
                    await _processor.ProcessAsync(pdf, outputFolder, mappings);

                TempData["ProcessMessage"] =
                    $"Processing done. Emails sent successfully: {successCount}, Failed: {failureCount}";
            }
            catch (Exception ex)
            {
                TempData["ProcessMessage"] = $"Error processing payslip: {ex.Message}";
            }

            return RedirectToAction("Index");
        }
    }
}
