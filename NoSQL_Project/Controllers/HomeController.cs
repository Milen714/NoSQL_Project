using Microsoft.AspNetCore.Mvc;
using NoSQL_Project.Models;
using NoSQL_Project.Services.Interfaces;
using System.Diagnostics;
using NoSQL_Project.Commons;

namespace NoSQL_Project.Controllers
{
    public class HomeController : BaseController
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
        public async Task<IActionResult> Login(LoginModel loginModel)
        {
            User user = await _userService.AuthenticateUserAsync(loginModel);
            if (user == null)
            {
                // Failed login
                TempData["ErrorMessage"] = "Invalid Email or Password";
                return View();

            }
            // Successful login
            TempData["Success"] = "Login successful!";
            HttpContext.Session.SetObject("LoggedInUser", user);
            return RedirectToAction("Index", "User");
        }

        public IActionResult Logout()
        {
            try
            {
                HttpContext.Session.Remove("LoggedInUser");
                return RedirectToAction("Login", "Home");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Logout failed: {ex.Message}";
                return RedirectToAction("Login");
            }
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public IActionResult SetTheme(string? theme)
        {
            try
            {
                if (theme != null)
                {
                    CookieOptions options = new CookieOptions()
                    {
                        Expires = DateTime.Now.AddDays(5),
                        Path = "/",
                        Secure = false,
                        HttpOnly = true,
                        IsEssential = true
                    };
                    Response.Cookies.Append("PreferedTheme", theme, options);
                }
                TempData["Success"] = $"Theme set successfully!{theme}";
                var referer = Request.Headers["Referer"].ToString();
                if (!string.IsNullOrEmpty(referer))
                {
                    return Redirect(referer);
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to set theme: {ex.Message}";
                return RedirectToAction("Index");
            }
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
