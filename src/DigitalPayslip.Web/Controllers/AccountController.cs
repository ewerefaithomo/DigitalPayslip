using DigitalPayslip.Web.Data;
using DigitalPayslip.Web.Models;
using DigitalPayslip.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace DigitalPayslip.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IEmailService _email;

        public AccountController(AppDbContext db, IEmailService email)
        {
            _db = db;
            _email = email;
        }

        public IActionResult Signup() => View();

        [HttpPost]
        public async Task<IActionResult> Signup(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Email and Password are required.");
                return View();
            }

            // Check if user already exists
            if (await _db.Users.AnyAsync(u => u.Email == email))
            {
                ModelState.AddModelError("", "Email already exists.");
                return View();
            }

            var user = new UserAccount
            {
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                IsVerified = true   // If no email verification is used
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Signup successful!";
            return RedirectToAction("Signup");
        }

        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _db.Users.FirstOrDefaultAsync(x => x.Email == email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Invalid login attempt.");
                return View();
            }

            HttpContext.Session.SetString("user", email);
            return RedirectToAction("Index", "Dashboard");
        }
    }
}
