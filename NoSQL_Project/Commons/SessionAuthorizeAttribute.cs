//using ChapeauPOS.Models;
//using Microsoft.AspNetCore.Mvc.Filters;
//using Microsoft.AspNetCore.Mvc;

//namespace ChapeauPOS.Commons
//{
//    public class SessionAuthorizeAttribute : ActionFilterAttribute
//    {
//        private readonly Roles[] _allowedRoles;

//        public SessionAuthorizeAttribute(params Roles[] allowedRoles)
//        {
//            _allowedRoles = allowedRoles;
//        }

//        public override void OnActionExecuting(ActionExecutingContext context)
//        {
//            var httpContext = context.HttpContext;
//            var employee = httpContext.Session.GetObject<Employee>("LoggedInUser");

//            if (employee == null || !_allowedRoles.Contains(employee.Role))
//            {
//                var controller = (Controller)context.Controller;
//                controller.TempData["ErrorMessage"] = "You do not have permission to access this page.";
//                httpContext.Session.Remove("LoggedInUser");
//                context.Result = new RedirectToActionResult("Login", "Home", null);
//            }

//            base.OnActionExecuting(context);
//        }
//    }
//}
