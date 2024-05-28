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
            // Chuyển chuổi id lấy được thành đối tượng User
            var user1 = JsonSerializer.Deserialize<Users>(id);
            if (UserPicture != null && UserPicture.Length > 0)
            {
                // Lấy tên file ảnh
                var fileName = Path.GetFileName(UserPicture.FileName);
                // Xác định đường dẫn lưu file ảnh
                var filePath = Path.Combine(_hostingEnvironment.WebRootPath, "assets/user_pic", fileName);
                // Lưu file ảnh vào đường dẫn đã xác định
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    UserPicture.CopyTo(fileStream);
                }
                // Cập nhật đường dẫn vào user
                user.Picture = fileName;
            }
            else
            {
                user.Picture = user1.Picture;
            }
            // Cập nhật userName từ session vào object user
            user.Picture = user1.Picture;
            user.updateAt = DateTime.Now;
            // Chuyển đổi user thành chuỗi Json
            string data = JsonSerializer.Serialize(user);
            // Tạo http để gửi đi
            var content = new StringContent(data, System.Text.Encoding.UTF8, "application/json");
            // Gửi yêu cầu PUT đến API để cập nhật thông tin
            HttpResponseMessage response = await client.PutAsync(api + "/" + user1.Username, content);
            if (response.IsSuccessStatusCode)
            {
                TempData["SystemNotificationAdmin"] = "Your changes have been saved successfully!";
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index", "Home");
        }
    }
}