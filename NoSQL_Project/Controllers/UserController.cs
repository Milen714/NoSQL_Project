using ChapeauPOS.Commons;
using Microsoft.AspNetCore.Mvc;
using NoSQL_Project.Models;
using NoSQL_Project.Repositories.Interfaces;
using NoSQL_Project.Services.Interfaces;

namespace NoSQL_Project.Controllers
{
    public class UserController : BaseController
    {
        private readonly IUserService _UserService;

        public UserController(IUserService userService) => _UserService = userService;

        //[SessionAuthorize(UserRoles.Admin)]
        public IActionResult Index()
        {
            var users = _UserService.GetAll();
            return View(users);
        }

        public IActionResult AddNewUser()
        {
            return View(new User());
        }
        //[SessionAuthorize(UserRoles.Admin)]
        [HttpPost]
        public IActionResult AddNewUser(User user)
        {
            User newUser = user;
            _UserService.Add(user);
            return RedirectToAction("Index");
        }
    }
}

