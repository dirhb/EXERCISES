using Microsoft.AspNetCore.Mvc;

namespace JobWebApp.Controllers
{
    [Route("[action]")]
    public class VisitorController : Controller
    {
        [Route("~/")]
        [Route("index")]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
