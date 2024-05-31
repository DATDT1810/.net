using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
            var user1 = JsonSerializer.Deserialize<Users>(id);
            // Lấy thông tin của người dùng dựa vào Username
            Users user = await GetUserDetailsAsync(user1.Username);
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
            existingUser.UserType = 3;
            existingUser.isDeleted = false;
            existingUser.updateAt = DateTime.Now;
            // Chuyển đổi user thành chuỗi Json
            string data = JsonSerializer.Serialize(user);
            // Gửi yêu cầu PUT đến API để cập nhật thông tin
            var content = new StringContent(data, System.Text.Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PutAsync(userApi + "/" + existingUser.Username, content);

            if (response.IsSuccessStatusCode)
            {
                TempData["SystemNotification"] = "Your changes have been saved successfully!";
                return RedirectToAction("CustomerProfile");
            }

            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError(string.Empty, errorMessage);
                return View(user); // Ensure the view returns the model with the error messages
            }
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
                    if (string.IsNullOrEmpty(data))
                    {
                        Console.WriteLine("Response content is empty.");
                        return null;
                    }

                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    return JsonSerializer.Deserialize<Users>(data, options);
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
