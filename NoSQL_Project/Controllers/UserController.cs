using Microsoft.AspNetCore.Mvc;
using NoSQL_Project.Models;
using NoSQL_Project.Repositories.Interfaces;

namespace NoSQL_Project.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserRepository _repo;

        public UserController(IUserRepository repo) => _repo = repo;

        public IActionResult Index()
        {
            var users = _repo.GetAll();
            return View(users);
        }

        [HttpPost]
        public IActionResult Create(string name, string email)
        {
            var user = new User { Name = name, Email = email };
            _repo.Add(user);
            return RedirectToAction("Index");
        }
    }
}

