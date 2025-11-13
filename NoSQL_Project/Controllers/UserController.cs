using System.Runtime.ConstrainedExecution;
using System.Threading.Tasks;
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
        public async Task<IActionResult> Index(string searchString, int pageNumber, string userTypeFilter, bool hasType)
        {
            try
            {
                if (searchString != null)
                {
                    pageNumber = 1;
                }

                UserType parsedType = UserType.All_employee;

                if (hasType && Enum.TryParse<UserType>(userTypeFilter, true, out var tempStatus))
                {
                    parsedType = tempStatus;
                }
                var users = _userService.GetActiveUsers(searchString, parsedType, hasType).Result;

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
        [SessionAuthorize(UserType.Service_employee)]
        public IActionResult AddNewUser()
        {
            try
            {
                ViewBag.Locations = _locationService.GetAllLocations().Result;
                return View(new User());
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index");
            }
        }
        [SessionAuthorize(UserType.Service_employee)]
        [HttpPost]
        public async Task<IActionResult> AddNewUser(User user, string locationId)
        {
            try
            {
                Location userLocation = _locationService.GetLocationById(locationId).Result;
                UserLocationRef userLocationRef = new UserLocationRef();
                userLocationRef.MapLocation(userLocation);
                user.Location = userLocationRef;
                await _userService.Add(user);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to fullfill your submision: {ex.Message}";
                return View(user);
            }
        }
        [SessionAuthorize(UserType.Service_employee, UserType.Reg_employee)]
        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            User user = await _userService.FindByIdAsync(id);
            User loggedInUser = HttpContext.Session.GetObject<User>("LoggedInUser");
            ViewBag.LoggedInUser = loggedInUser;
            try
            {

                ViewBag.Locations = _locationService.GetAllLocations().Result;

                return View(user);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to save your changes: {ex.Message}";
                return View(user);
            }
        }
        [SessionAuthorize(UserType.Service_employee, UserType.Reg_employee)]
        [HttpPost]
        public async Task<IActionResult> EditUser(User user, string locationId, string newPassword)
        {
            try
            {
                if (!string.IsNullOrEmpty(newPassword))
                {
                    user.PasswordHash = newPassword;
                    user = _userService.HashUserPassword(user);
                }
                Location userLocation = _locationService.GetLocationById(locationId).Result;
                UserLocationRef userLocationRef = new UserLocationRef();
                userLocationRef.MapLocation(userLocation);
                user.Location = userLocationRef;
                user.Active = true;
                await _userService.UpdateUserAsync(user);

                User loggedInUser = HttpContext.Session.GetObject<User>("LoggedInUser");
                if(loggedInUser.UserType == UserType.Reg_employee)
                {
                    return RedirectToAction("MyTickets", "Incident");
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to save your changes: {ex.Message}";
                return View(user);
            }
        }
    }
}

