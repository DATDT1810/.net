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
using MyProjectClient.Models;

namespace MyProjectClient.Controllers
{
    public class CustomerProfileController : Controller
    {
        private readonly HttpClient client = null;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private string userApi;
        public CustomerProfileController(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
            client = new HttpClient();
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            client.DefaultRequestHeaders.Accept.Add(contentType);
            this.userApi = "https://localhost:5001/api/CustomerProfile";
        }

        public async Task<IActionResult> CustomerProfile()
        {
            // Lấy id của người dùng từ Session
            string id = HttpContext.Session.GetString("_user");
            // Chuyển chuổi id lấy được thành đối tượng User
            // var user1 = JsonSerializer.Deserialize<Users>(id);
            // Lấy thông tin của người dùng dựa vào Username
            Users user = await GetUserDetailsAsync(id);
            if (user != null)
            {
                return View(user);
            }
            return NotFound();
        }


        [HttpPost]
        public async Task<IActionResult> CustomerProfile(Users user, IFormFile UserPicture)
        {
            // Lấy id của người dùng từ Session
            string id = HttpContext.Session.GetString("_user");
            // Chuyển chuổi id lấy được thành đối tượng User
            Users user1 = await GetUserDetailsAsync(id);
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
                user.Picture = user.Picture;
            }
            // Cập nhật userName từ session vào object user
            user.Username = user1.Username;
            user.updateAt = DateTime.Now;
            // Chuyển đổi user thành chuỗi Json
            string data = JsonSerializer.Serialize(user);
            // Tạo http để gửi đi
            var content = new StringContent(data, System.Text.Encoding.UTF8, "application/json");
            // Gửi yêu cầu PUT đến API để cập nhật thông tin
            HttpResponseMessage response = await client.PutAsync(userApi + "/" + user1.Username, content);
            if (response.IsSuccessStatusCode)
            {
                TempData["SystemNotification"] = "Your changes have been saved successfully!";
                return RedirectToAction("CustomerProfile");
            }
            return RedirectToAction("Index", "Home");
        }

        private async Task<Users> GetUserDetailsAsync(string id)
        {
            // Gửi yêu cầu GET đến API để lấy thông tin user
            HttpResponseMessage response = await client.GetAsync(userApi + "/" + id);
            // Nếu yêu cầu thành công, đọc và chuyển đổi dữ liệu JSON thành đối tượng User
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
                    Console.WriteLine($"Error deserializing JSON: {ex.Message}");
                    return null;
                }
            }
            else
            {
                Console.WriteLine($"Failed to retrieve user. Status code: {response.StatusCode}");
                return null;
            }
        }
    }
}


// using System;
// using System.Diagnostics;
// using System.IO;
// using System.Net.Http;
// using System.Net.Http.Headers;
// using System.Text.Json;
// using System.Threading.Tasks;
// using Microsoft.AspNetCore.Hosting;
// using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.Mvc;
// using MyProjectClient.Models;

// namespace MyProjectClient.Controllers
// {
//     public class CustomerProfileController : Controller
//     {
//         private readonly HttpClient client = null;
//         private readonly IWebHostEnvironment _hostingEnvironment;
//         private readonly string userApi;

//         public CustomerProfileController(IWebHostEnvironment hostingEnvironment)
//         {
//             _hostingEnvironment = hostingEnvironment;
//             client = new HttpClient();
//             var contentType = new MediaTypeWithQualityHeaderValue("application/json");
//             client.DefaultRequestHeaders.Accept.Add(contentType);
//             userApi = "https://localhost:5001/api/CustomerProfile";
//         }

//         public async Task<IActionResult> CustomerProfile()
//         {
//             // Lấy id của người dùng từ Session
//             string id = HttpContext.Session.GetString("_user");
//             if (string.IsNullOrEmpty(id))
//             {
//                 return RedirectToAction("Index", "Home"); // Or any other appropriate action
//             }

//             // Chuyển chuỗi id lấy được thành đối tượng User
//             var user1 = JsonSerializer.Deserialize<Users>(id);
//             // Lấy thông tin của người dùng dựa vào Username
//             Users user = await GetUserDetailsAsync(user1.Username);
//             if (user != null)
//             {
//                 return View(user);
//             }
//             return NotFound();
//         }

//         [HttpPost]
//         public async Task<IActionResult> CustomerProfile(Users user, IFormFile UserPicture)
//         {
//             // Lấy id của người dùng từ Session
//             string id = HttpContext.Session.GetString("_user");
//             if (string.IsNullOrEmpty(id))
//             {
//                 return RedirectToAction("Index", "Home"); // Or any other appropriate action
//             }

//             // Chuyển chuỗi id lấy được thành đối tượng User
//             var user1 = JsonSerializer.Deserialize<Users>(id);
//             if (user1 == null)
//             {
//                 return RedirectToAction("Index", "Home"); // Or any other appropriate action
//             }

//             if (UserPicture != null && UserPicture.Length > 0)
//             {
//                 // Lấy tên file ảnh
//                 var fileName = Path.GetFileName(UserPicture.FileName);
//                 // Xác định đường dẫn lưu file ảnh
//                 var filePath = Path.Combine(_hostingEnvironment.WebRootPath, "assets/user_pic", fileName);
//                 // Lưu file ảnh vào đường dẫn đã xác định
//                 using (var fileStream = new FileStream(filePath, FileMode.Create))
//                 {
//                     await UserPicture.CopyToAsync(fileStream);
//                 }
//                 // Cập nhật đường dẫn vào user
//                 user.Picture = fileName;
//             }
//             user.Username = user1.Username;
//             user.updateAt = DateTime.Now;

//             // Chuyển đổi user thành chuỗi Json
//             string data = JsonSerializer.Serialize(user);
//             // Tạo http để gửi đi
//             var content = new StringContent(data, System.Text.Encoding.UTF8, "application/json");
//             // Gửi yêu cầu PUT đến API để cập nhật thông tin
//             HttpResponseMessage response = await client.PutAsync($"{userApi}/{user1.Username}", content);
//             if (response.IsSuccessStatusCode)
//             {
//                 TempData["SystemNotification"] = "Your changes have been saved successfully!";
//                 return RedirectToAction("CustomerProfile");
//             }
//             return RedirectToAction("Index", "Home");
//         }

//         private async Task<Users> GetUserDetailsAsync(string id)
//         {
//             // Gửi yêu cầu GET đến API để lấy thông tin user
//             HttpResponseMessage response = await client.GetAsync($"{userApi}/{id}");
//             // Nếu yêu cầu thành công, đọc và chuyển đổi dữ liệu JSON thành đối tượng User
//             if (response.IsSuccessStatusCode)
//             {
//                 try
//                 {
//                     string data = await response.Content.ReadAsStringAsync();
//                     var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
//                     return JsonSerializer.Deserialize<Users>(data, options);
//                 }
//                 catch (JsonException ex)
//                 {
//                     Console.WriteLine($"Error deserializing JSON: {ex.Message}");
//                     return null;
//                 }
//             }
//             else
//             {
//                 Console.WriteLine($"Failed to retrieve user. Status code: {response.StatusCode}");
//                 return null;
//             }
//         }
//     }
// }
