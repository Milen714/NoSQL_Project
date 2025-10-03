using ChapeauPOS.Commons;
using Microsoft.AspNetCore.Mvc;
using NoSQL_Project.Commons;
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
        public async Task<IActionResult> Index(string searchString, int pageNumber, string currentFilter)
        {
            User loggedInUser = HttpContext.Session.GetObject<User>("LoggedInUser");
            try
            {
                if (searchString != null)
                {
                    pageNumber = 1;
                }
                else
                {
                    searchString = currentFilter;
                }

                var users = _userService.GetAll().Where(u => u.UserType == UserType.Service_employee);

                if (pageNumber < 1)
                {
                    pageNumber = 1;
                }

                int pageSize = 10;
                return View(PaginatedList<User>.CreateAsync(users, pageNumber, pageSize));

            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Could not retrieve users: {ex.Message}";
                return View(new PaginatedList<User>(new List<User>(), 0, 1, 1));
            }
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
                TempData["Error"] = $"Failed to fullfill your submision: {ex.Message}";
                return View(user);
            }
        }
    }
}

