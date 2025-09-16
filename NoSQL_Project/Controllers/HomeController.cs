using Microsoft.AspNetCore.Mvc;
using NoSQL_Project.Models;
using NoSQL_Project.Services.Interfaces;
using System.Diagnostics;
using NoSQL_Project.Commons;

namespace NoSQL_Project.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUserService _userService;

        public HomeController(ILogger<HomeController> logger, IUserService userService)
        {
            _logger = logger;
            _userService = userService;
        }
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Login(LoginModel loginModel)
        {
            User user = _userService.GetUserByEmail(loginModel);
            if (user == null)
            {
                // Failed login
                ViewBag.ErrorMessage = "Invalid email or password.";
                return View();

            }
            // Successful login
            TempData["Success"] = "Login successful!";
            HttpContext.Session.SetObject("LoggedInUser", user);
            return RedirectToAction("Index", "User");


        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
