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

        // [HttpPost]
        // public async Task<IActionResult> Login(string username, string password)
        // {
        //     if (username == null || password == null)
        //     {
        //         ViewBag.errorMessage = ("Please enter your username and password!");
        //         return View("Login");
        //     }
        //     HttpResponseMessage response = await client.GetAsync(api + "/getUser/" + username + "/" + password);
        //     if (!response.IsSuccessStatusCode)
        //     {
        //         ViewBag.errorMessage = ("User doesn't exist");
        //         return View("Login");
        //     }

        //     string data = await response.Content.ReadAsStringAsync();
        //     var result = JsonObject.Parse(data); // threw the exception mentioned in the question

        //     var option = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        //     var user = JsonSerializer.Deserialize<Users>(data, option);

        //     if (user == null)
        //     {
        //         ViewBag.errorMessage = ("User doesn't exist");
        //         return View("Login");
        //     }

        //     const string _user = "_user";
        //     if (user.UserType == 1 || user.UserType == 2)
        //     {
        //         HttpContext.Session.SetString(_user, data);
        //         return RedirectToAction("Index", "UserManagement");
        //     }
        //     else if (user.UserType == 3)
        //     {
        //         HttpContext.Session.SetString(_user, data);
        //         return RedirectToAction("CustomerProfile", "CustomerProfile");
        //     }
        //     else if (user.UserType == 0)
        //     {
        //         ViewBag.errorMessage = ("Your account is ban");
        //         return View("Login");
        //     }
        //     else
        //     {
        //         ViewBag.errorMessage = ("Your account does not exit");
        //         return View("Login");
        //     }
        // }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (username == null || password == null)
            {
                ViewBag.errorMessage = "Please enter your username and password!";
                return View("Login");
            }

            try
            {
                HttpResponseMessage response = await client.GetAsync($"{api}/getUser/{username}/{password}");
                response.EnsureSuccessStatusCode(); // Ensure HTTP success status code

                string data = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(data))
                {
                    ViewBag.errorMessage = "Empty response from the API.";
                    return View("Login");
                }

                var option = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var user = JsonSerializer.Deserialize<Users>(data, option);

                if (user == null)
                {
                    ViewBag.errorMessage = "User doesn't exist";
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
                    ViewBag.errorMessage = "Your account is banned.";
                    return View("Login");
                }
                else
                {
                    ViewBag.errorMessage = "Your account does not exist.";
                    return View("Login");
                }
            }
            catch (HttpRequestException ex)
            {
                ViewBag.errorMessage = "Error communicating with the API: " + ex.Message;
                return View("Login");
            }
            catch (JsonException ex)
            {
                ViewBag.errorMessage = "Error deserializing JSON response: " + ex.Message;
                return View("Login");
            }
            catch (Exception ex)
            {
                ViewBag.errorMessage = "An error occurred: " + ex.Message;
                return View("Login");
            }
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
            if (email == null)
            {
                TempData["SystemNotificationError"] = "Your email is incorrect, Please try again!";
                return View("ForgotPassword");
            }
            HttpResponseMessage response = await client.GetAsync(api);
            string data = await response.Content.ReadAsStringAsync();

            var option = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var Listuser = JsonSerializer.Deserialize<List<Users>>(data, option);
            Users user = null;
            foreach (var item in Listuser)
            {
                if (item.Email.Equals(email))
                {
                    user = item;
                }
            }

            if (user == null)
            {
                TempData["SystemNotificationError"] = "Your email does not exist";
                return View("ForgotPassword");
            }
            System.Console.WriteLine(user.Email);
            response = await client.GetAsync(resetPassApi + "/SendMail/" + user.Email);
            if (response.IsSuccessStatusCode)
            {
                const string _user = "username";
                // string userDataJson = JsonConvert.SerializeObject(user);
                HttpContext.Session.SetString(_user, user.Username);

                var dataCode = response.Content.ReadAsStringAsync().Result;
                var requestCode = JsonSerializer.Deserialize<string>(dataCode);
                TempData["RequestCode"] = requestCode;
            }
            return View("ConfirmEmail");
        }

        private IActionResult ConfirmEmail()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmEmail(string code)
        {
            string requestCode = TempData["RequestCode"] as string;
            if (requestCode.Equals(code))
            {
                return View("ResetPassword");
            }
            return RedirectToAction("Login");
        }

        private IActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string pass)
        {
            string email = HttpContext.Session.GetString("username");

            HttpResponseMessage response = await client.GetAsync(resetPassApi + "/ResetPass/" + email + "/" + pass);
            if (response.IsSuccessStatusCode)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login");
            }
            HttpContext.Session.Clear();
            return View();
        }
    }
}