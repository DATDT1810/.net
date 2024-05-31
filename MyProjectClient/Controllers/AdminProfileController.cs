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
    public class AdminProfileController : Controller
    {
        private readonly HttpClient client = null;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private string api;
        public AdminProfileController(IWebHostEnvironment hostEnvironment)
        {
            _hostingEnvironment = hostEnvironment;
            client = new HttpClient();
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            client.DefaultRequestHeaders.Accept.Add(contentType);
            this.api = "https://localhost:5001/api/AdminProfile";
        }

        private async Task<Users> GetUserDetailsAsync(string id)
        {
            HttpResponseMessage response = await client.GetAsync(api + "/" + id);
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

        public async Task<IActionResult> Index()
        {
            string id = HttpContext.Session.GetString("_user");
            var user1 = JsonSerializer.Deserialize<Users>(id);
            Users user = await GetUserDetailsAsync(user1.Username);
            if (user != null)
            {
                return View(user);
            }
            return NotFound();
        }


        [HttpPost]
        public async Task<IActionResult> AdminProfile(Users user, IFormFile UserPicture)
        {
            // Lấy id của người dùng từ Session
            string id = HttpContext.Session.GetString("_user");
            // var user1 = JsonSerializer.Deserialize<Users>(id);
            Users existingUser = JsonSerializer.Deserialize<Users>(id);

            if (UserPicture != null && UserPicture.Length > 0)
            {
                // Lấy tên file ảnh
                var fileName = Path.GetFileName(UserPicture.FileName);
                // Xác định đường dẫn lưu file ảnh
                var filePath = Path.Combine(_hostingEnvironment.WebRootPath, "assets/user_pic", fileName);
                // Lưu file ảnh vào đường dẫn đã xác định
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await UserPicture.CopyToAsync(fileStream);
                }
                // Cập nhật đường dẫn vào user
                user.Picture = fileName;
            }
            else
            {
                user.Picture = existingUser.Picture;
            }
             // Cập nhật thông tin của người dùng
            existingUser.FirstName = user.FirstName;
            existingUser.LastName = user.LastName;
            existingUser.DateOfBirth = user.DateOfBirth;
            existingUser.Gender = user.Gender;
            existingUser.Address = user.Address;
            existingUser.Picture = user.Picture;
            existingUser.ZipCode = user.ZipCode;
            existingUser.PhoneNumber = user.PhoneNumber;
            existingUser.Email = user.Email;
            existingUser.IDCard = user.IDCard;
            existingUser.UserType = 1;
            existingUser.isDeleted = false;
            existingUser.updateAt = DateTime.Now; 
            // Chuyển đổi user thành chuỗi Json
            string data = JsonSerializer.Serialize(user);
            // Gửi yêu cầu PUT đến API để cập nhật thông tin
            var content = new StringContent(data, System.Text.Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PutAsync(api + "/" + existingUser.Username, content);

            if (response.IsSuccessStatusCode)
            {
                TempData["SystemNotification"] = "Your changes have been saved successfully!";
                return RedirectToAction("Index", "AdminProfile");
            }

            return RedirectToAction("Index", "Home");
        }
    }
}