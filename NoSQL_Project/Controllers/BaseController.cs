using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NoSQL_Project.Commons;
using NoSQL_Project.Models;

namespace NoSQL_Project.Controllers
{
    public class BaseController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            // Retrieve the logged-in employee from session
            var loggedInEmployee = HttpContext.Session.GetObject<User>("LoggedInUser");

            // Pass it to the ViewBag
            ViewBag.LoggedInEmployee = loggedInEmployee;
        }
    }
}
