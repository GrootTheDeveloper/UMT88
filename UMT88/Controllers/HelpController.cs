using Microsoft.AspNetCore.Mvc;

namespace UMT88.Controllers
{
    public class HelpController : Controller
    {
        // GET: /Help
        [HttpGet]
        public IActionResult Index() => View();
    }
}
