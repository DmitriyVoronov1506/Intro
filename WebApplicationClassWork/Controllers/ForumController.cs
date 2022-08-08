using Microsoft.AspNetCore.Mvc;

namespace WebApplicationClassWork.Controllers
{
    public class ForumController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
