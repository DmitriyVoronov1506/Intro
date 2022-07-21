using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WebApplicationClassWork.Models;
using WebApplicationClassWork.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApplicationClassWork.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly RandomService _randomService;
        private readonly IHasher _hasher;
        private readonly IAuthService _authService;

        private readonly ICurrentDate _currentDate;

        private readonly DAL.Context.IntroContext _introContext;

        public HomeController(ILogger<HomeController> logger, RandomService randomService, IHasher hasher, ICurrentDate currentDate,
            DAL.Context.IntroContext introContext, IAuthService authService)  
        {
            _logger = logger;
            _randomService = randomService;
            _hasher = hasher;
            _currentDate = currentDate;
            _introContext = introContext;
            _authService = authService;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            ViewData["AuthUser"] = _authService.User;  // Берём пользователя из нашего обьекта который получаем в сервис из middlware

            base.OnActionExecuting(context);
        }

        public IActionResult Index()
        {        
            ViewData["rnd"] = _randomService.Integer;
            ViewBag.hash = _hasher.Hash("123");

            ViewData["DateCurrent"] = _currentDate.GetCurrentDate();  // для удобства создадим два viewdata отдельно для даты и времени
            ViewData["DateTime"] = _currentDate.GetCurrentTime();

            ViewData["UsersCount"] = _introContext.Users.Count();

            ViewData["fromAuthMiddleware"] = HttpContext.Items["fromAuthMiddleware"];

            ViewData["UserNames"] = _introContext.Users.Select(u => u.RealName).ToList();

            // Используем линк для отбора Real Name у пользователей и запишем в массив ViewData   
            // После выполнения запроса - получаем коллекцию и приобразуем её в ToList() и получаем коллекцию типа List<string> для отображения в цикле в Index.cshtml

            ViewData["authUserName"] = _authService.User?.RealName;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult About()
        {
            var model = new AboutModel
            {
                Data = "About Data"
            };
       
            return View(model);
        }

        public IActionResult Contacts()
        {
            var model = new ContactsModel
            {
                Name = "Test Name",
                Address = "Test Address",
                Number = "+381234567890"
            };

            return View(model);
        }
       
        public async Task<IActionResult> Random()
        {
            return Content(_randomService.Integer.ToString());
        }

        public IActionResult Data()
        {
            return Json(new { field = "value" });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
