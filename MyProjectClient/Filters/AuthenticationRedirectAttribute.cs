using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using MyProjectClient.Models;

namespace MyProjectClient.Filters
{
    public class AuthenticationRedirectAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userJson = context.HttpContext.Session.GetString("_user");
            // var user = JsonSerializer.Deserialize<User>(userJson);
            if (string.IsNullOrEmpty(userJson))
            {
                context.Result = new RedirectToRouteResult(
                new RouteValueDictionary(new { controller = "Auth", action = "Login" }));
            }
            else
            {
                base.OnActionExecuting(context);
            }
        }
    }

    public class StaffAuthenticationRedirectAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userJson = context.HttpContext.Session.GetString("_user");
            if (string.IsNullOrEmpty(userJson))
            {
                context.Result = new RedirectToRouteResult(
                new RouteValueDictionary(new { controller = "Auth", action = "Login" }));
                return;
            }
            var user = JsonSerializer.Deserialize<Users>(userJson);
            if (user.UserType != 1 && user.UserType != 2)
            {
                context.Result = new RedirectToRouteResult(
                new RouteValueDictionary(new { controller = "Error", action = "ErrorPage" }));
            }
            else
            {
                base.OnActionExecuting(context);
            }
        }
    }

    public class AdminAuthenticationRedirectAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userJson = context.HttpContext.Session.GetString("_user");
            if (string.IsNullOrEmpty(userJson))
            {
                context.Result = new RedirectToRouteResult(
                new RouteValueDictionary(new { controller = "Auth", action = "Login" }));
                return;
            }
            var user = JsonSerializer.Deserialize<Users>(userJson);
            if (user.UserType != 1)
            {
                context.Result = new RedirectToRouteResult(
                new RouteValueDictionary(new { controller = "Error", action = "ErrorPage" }));
            }
            else
            {
                base.OnActionExecuting(context);
            }
        }
    }
}