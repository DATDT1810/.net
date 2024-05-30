using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyProjectClient.Filters;
using MyProjectClient.Models;

namespace MyProjectClient.Controllers
{
    [StaffAuthenticationRedirect]
    public class UserManagementController : Controller
    {
        private readonly HttpClient client = null;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private string userApi;
        public UserManagementController(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
            client = new HttpClient();
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            client.DefaultRequestHeaders.Accept.Add(contentType);
            this.userApi = "https://localhost:5001/api/UserManagement";
        }

        public async Task<ActionResult> Index(int id)
        {
            switch (id)
            {
                case -1:
                    ViewBag.nameTable = "Delete account";
                    ViewBag.userType = -1;
                    break;
                case 0:
                    ViewBag.nameTable = "Ban account";
                    ViewBag.userType = 0;
                    break;
                case 1:
                    ViewBag.nameTable = "Admin account";
                    ViewBag.userType = 1;
                    break;
                case 2:
                    ViewBag.nameTable = "Staff account";
                    ViewBag.userType = 2;
                    break;
                case 3:
                    ViewBag.nameTable = "Customer account";
                    ViewBag.userType = 3;
                    break;
                default:
                    ViewBag.nameTable = "Customer account";
                    ViewBag.userType = 3;
                    break;
            }
            var UserList = await GetUserListAsync();
            List<string> UserListName = new List<string>();
            foreach (var item in UserList)
            {
                UserListName.Add(item.Username);
            }
            ViewBag.UserListName = UserListName;
            return View(UserList);
        }

        public async Task<IActionResult> UserDetails(string id)
        {
            Users user = await GetUserDetailsAsync(id);
            if (user != null)
            {
                return View(user);
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> UserDetails(string id, Users user, IFormFile userPicture)
        {
            if (userPicture != null && userPicture.Length > 0)
            {
                var fileName = Path.GetFileName(userPicture.FileName);
                var filePath = Path.Combine(_hostingEnvironment.WebRootPath, "assets/user_pic", fileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    userPicture.CopyTo(fileStream);
                }
                user.Picture = fileName;
            }
            user.Username = id;
            user.updateAt = DateTime.Now;
            string data = JsonSerializer.Serialize(user);
            var content = new StringContent(data, System.Text.Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PutAsync(userApi + "/" + id, content);
            if (response.IsSuccessStatusCode)
            {
                TempData["SystemNotification"] = "Edited the object successfully";
                return RedirectToAction("Index", new { id = 3 });
            }
            return Redirect("Home/Error");
        }

        [HttpGet]
        public async Task<IActionResult> BanAccount(string id, int uStyle)
        {
            Users user = await GetUserDetailsAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.UserType = uStyle;

            string data = JsonSerializer.Serialize(user);
            var content = new StringContent(data, System.Text.Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PutAsync(userApi + "/" + id, content);

            if (response.IsSuccessStatusCode)
            {
                string notificationMessage = uStyle switch
                {
                    0 => "User has been banned.",
                    3 => "User has been unbanned.",
                    -1 => "User has been deleted.",
                    _ => "Operation successful."
                };
                TempData["SystemNotification"] = notificationMessage;
                return Json(notificationMessage);
            }
            else
            {
                TempData["SystemNotificationError"] = "Failed to ban user.";
                return Json("Failed to ban user.");
            }
        }


        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Users user)
        {
            user.Picture = "l60Hf-150x150.png";
            user.createdAt = DateTime.Now;
            user.updateAt = DateTime.Now;
            user.UserType = 2;
            if (ModelState.IsValid)
            {
                string data = JsonSerializer.Serialize(user);
                var content = new StringContent(data, System.Text.Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(userApi, content);
                if (response.IsSuccessStatusCode)
                {
                    TempData["SystemNotification"] = "Add staff successfully";
                    return RedirectToAction("Index", new { id = 3 });
                }
                TempData["SystemNotificationError"] = "Account is already exit!";
                return RedirectToAction("Index", new { id = 3 });
            }
            TempData["SystemNotificationError"] = "Invalid input";
            return View(user);
        }

        private async Task<List<Users>> GetUserListAsync()
        {
            HttpResponseMessage response = await client.GetAsync(userApi);
            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                var option = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<List<Users>>(data, option);
            }
            else
            {
                // Handle error if needed
                return new List<Users>();
            }
        }

        private async Task<Users> GetUserDetailsAsync(string id)
        {
            HttpResponseMessage response = await client.GetAsync(userApi + "/" + id);
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    string data = await response.Content.ReadAsStringAsync();
                    var option = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    return JsonSerializer.Deserialize<Users>(data, option);
                }
                catch (JsonException ex)
                {
                    // Log the exception or handle it gracefully
                    Console.WriteLine($"Error deserializing JSON: {ex.Message}");
                    return null;
                }
            }
            else
            {
                // Handle error if needed
                return null;
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetOrderFiltered(string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                return NotFound("Invalid customer name");
            }

            // Call the API with the constructed query string
            HttpResponseMessage response = await client.GetAsync($"https://localhost:5001/api/UserManagement/search/{userName}");
            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();

                // Deserialize the response data to a list of orders
                var option = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var orderList = JsonSerializer.Deserialize<List<Users>>(data, option);
                return View("GetOrderFiltered", orderList);
            }
            return NotFound();
        }
    }
}