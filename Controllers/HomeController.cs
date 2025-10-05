using Microsoft.AspNetCore.Mvc;

namespace CISS411_GroupProject.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
