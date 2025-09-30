using ChapeauPOS.Commons;
using Microsoft.AspNetCore.Mvc;
using NoSQL_Project.Models;
using NoSQL_Project.Models.Enums;
using NoSQL_Project.Repositories.Interfaces;
using NoSQL_Project.Services.Interfaces;

namespace NoSQL_Project.Controllers
{
    public class UserController : BaseController
    {
        private readonly IUserService _userService;
        private readonly ILocationService _locationService;

        public UserController(IUserService userService, ILocationService locationSrvice)
        {
            _locationService = locationSrvice;
            _userService = userService;
        }

        [SessionAuthorize(UserType.Service_employee)]
        public IActionResult Index()
        {
            var users = _userService.GetAll();
            return View(users);
        }

        public IActionResult AddNewUser()
        {
            ViewBag.Locations = _locationService.GetAllLocations().Result;
            return View(new User());
        }
        //[SessionAuthorize(UserRoles.Admin)]
        [HttpPost]
        public IActionResult AddNewUser(User user, string locationId)
        {
            try
            {
                Location userLocation = _locationService.GetLocationById(locationId).Result;
                UserLocationRef userLocationRef = new UserLocationRef();
                userLocationRef.MapLocation(userLocation);
                user.Location = userLocationRef;
                _userService.Add(user);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                string errorMessage = ex.Message;
                return View(user);
            }
        }
    }
}

