
using Microsoft.AspNetCore.Mvc;

namespace DigitalPayslip.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => View();
    }
}
