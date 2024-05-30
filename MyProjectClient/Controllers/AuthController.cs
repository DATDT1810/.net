using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;
using MyProjectClient.Models;

namespace MyProjectClient.Controllers
{
    public class AuthController : Controller
    {
        private readonly HttpClient client = null;
        private string api;

        private string resetPassApi;


        public AuthController()
        {
            client = new HttpClient();
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            client.DefaultRequestHeaders.Accept.Add(contentType);
            this.api = "https://localhost:5001/api/Auth";
            this.resetPassApi = "https://localhost:5001/api/ForgotPassword";
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (username == null || password == null)
            {
                ViewBag.errorMessage = ("Please enter your username and password!");
                return View("Login");
            }
            HttpResponseMessage response = await client.GetAsync(api + "/getUser/" + username + "/" + password);
            if (!response.IsSuccessStatusCode)
            {
                ViewBag.errorMessage = ("User doesn't exist");
                return View("Login");
            }

            string data = await response.Content.ReadAsStringAsync();
            var result = JsonObject.Parse(data); // threw the exception mentioned in the question

            var option = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var user = JsonSerializer.Deserialize<Users>(data, option);

            if (user == null)
            {
                ViewBag.errorMessage = ("User doesn't exist");
                return View("Login");
            }

            const string _user = "_user";
            if (user.UserType == 1 || user.UserType == 2)
            {
                HttpContext.Session.SetString(_user, JsonSerializer.Serialize(user));
                return RedirectToAction("Index", "UserManagement");
            }
            else if (user.UserType == 3)
            {
                HttpContext.Session.SetString(_user, JsonSerializer.Serialize(user));
                return RedirectToAction("CustomerProfile", "CustomerProfile");
            }
            else if (user.UserType == 0)
            {
                ViewBag.errorMessage = ("Your account is ban");
                return View("Login");
            }
            else
            {
                ViewBag.errorMessage = ("Your account does not exit");
                return View("Login");
            }
        }

        public async Task LoginWithGoogle()
        {
            await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme,
            new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleResponse")
            });
        }

        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var claims = result.Principal.Identities.FirstOrDefault().Claims.Select(claim => new
            {
                claim.Issuer,
                claim.OriginalIssuer,
                claim.Type,
                claim.Value

            });

            HttpContext.Session.SetString("_user", claims.FirstOrDefault(c => c.Type == "sub")?.Value);
            return RedirectToAction("CustomerProfile", "CustomerProfile");
        }



        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Auth");
        }

        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(Users user)
        {
            user.FirstName = "";
            user.LastName = "";
            user.DateOfBirth = "";
            user.Gender = "";
            user.Address = "";
            user.Picture = "l60Hf-150x150.png";
            user.ZipCode = "";
            user.IDCard = "";
            user.createdAt = DateTime.Now;
            user.updateAt = DateTime.Now;
            // user.UserType = 3;
            if (ModelState.IsValid)
            {
                string data = JsonSerializer.Serialize(user);
                var content = new StringContent(data, System.Text.Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(api, content);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Login");
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError(string.Empty, errorMessage);
                }
            }
            return View(user);
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                TempData["SystemNotificationError"] = "Your email is incorrect, Please try again!";
                return View("ForgotPassword");
            }

            HttpResponseMessage response = await client.GetAsync(api);
            string data = await response.Content.ReadAsStringAsync();

            var option = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var Listuser = JsonSerializer.Deserialize<List<Users>>(data, option);
            Users user = Listuser?.FirstOrDefault(item => item.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

            if (user == null)
            {
                TempData["SystemNotificationError"] = "Your email does not exist";
                return View("ForgotPassword");
            }

            response = await client.GetAsync(resetPassApi + "/SendMail/" + user.Email);
            if (response.IsSuccessStatusCode)
            {
                HttpContext.Session.SetString("username", user.Username);
                string dataCode = await response.Content.ReadAsStringAsync();
                string requestCode = JsonSerializer.Deserialize<string>(dataCode);
                TempData["RequestCode"] = requestCode;
                TempData.Keep("RequestCode"); // Ensure TempData is kept for the next request
            }
            return View("ConfirmEmail");
        }

        public IActionResult ConfirmEmail()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmEmail(string code)
        {
            string requestCode = TempData["RequestCode"] as string;

            if (requestCode == null)
            {
                TempData["SystemNotificationError"] = "Request code is missing. Please try the password reset process again.";
                return RedirectToAction("ForgotPassword");
            }

            if (requestCode.Equals(code, StringComparison.OrdinalIgnoreCase))
            {
                return View("ResetPassword");
            }

            TempData["SystemNotificationError"] = "The code you entered is incorrect.";
            return View("ConfirmEmail");
        }

        public IActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string pass)
        {
            string username = HttpContext.Session.GetString("username");

            if (string.IsNullOrWhiteSpace(username))
            {
                TempData["SystemNotificationError"] = "Session has expired or invalid. Please try again.";
                return RedirectToAction("Login");
            }

            HttpResponseMessage response = await client.GetAsync(resetPassApi + "/ResetPass/" + username + "/" + pass);
            if (response.IsSuccessStatusCode)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login");
            }

            TempData["SystemNotificationError"] = "Failed to reset password. Please try again.";
            HttpContext.Session.Clear();
            return View();
        }
    }
}