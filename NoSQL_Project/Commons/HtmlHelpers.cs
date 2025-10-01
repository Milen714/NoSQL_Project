using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace NoSQL_Project.Commons
{
    public static class HtmlHelpers
    {
        public static string IsActive(this HtmlHelper html, string controllers = "", string actions = "", string cssClass = "active")
        {
            var viewContext = html.ViewContext;
            var currentController = viewContext.RouteData.Values["controller"] as string;
            var currentAction = viewContext.RouteData.Values["action"] as string;

            var acceptedControllers = (controllers ?? currentController).Split(',');
            var acceptedActions = (actions ?? currentAction).Split(',');

            return acceptedControllers.Contains(currentController) && acceptedActions.Contains(currentAction)
                ? cssClass
                : string.Empty;
        }
    }
}
