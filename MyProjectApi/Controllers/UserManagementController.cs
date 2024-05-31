using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyProjectApi.DB;
using MyProjectApi.Helpter;
using MyProjectApi.Models;
using MyProjectApi.Service;

namespace MyProjectApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserManagementController : ControllerBase
    {
        private readonly ApplicationDBContext _db;
        private readonly IEmailService _emailService;
        public UserManagementController(ApplicationDBContext db, IEmailService emailService)
        {
            this._db = db;
            this._emailService = emailService;

        }

        [HttpGet]
        public IActionResult GetAllOfUser()
        {
            var listOfUser = this._db.users.ToList();
            return Ok(listOfUser);
        }

        [HttpGet("{id}", Name = "GetUserByUsername")]
        public IActionResult GetUserByUsername(string id)
        {
            var user = this._db.users.FirstOrDefault(u => u.Username == id);
            return Ok(user);
        }

        [HttpPost]
        public IActionResult AddNewUser(Users user)
        {
            if (user == null)
            {
                return BadRequest("User can't be null");
            }
            var hassPass = ComputeMD5Hash(user.Password);
            this._db.users.Add(new Users(user.FirstName, user.LastName, user.DateOfBirth, user.Gender, user.Address, user.Picture, user.ZipCode, user.PhoneNumber, user.Email, user.Username, user.IDCard, hassPass, user.UserType, user.isDeleted, user.createdAt, user.updateAt));
            this._db.SaveChanges();
            return Ok(user);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateUsername(string id, Users user)
        {
            if (user == null)
            {
                return BadRequest("User can't be null");
            }

            // Tìm kiếm user cũ trong cơ sở dữ liệu
            Users existingUser = this._db.users.FirstOrDefault(u => u.Username.Equals(id));
            if (existingUser == null)
            {
                return NotFound("User not found");
            }
            var existingUsers = this._db.users.FirstOrDefault(u => u.Username == user.Username || u.Email == user.Email);
            if (existingUser != null)
            {
                return BadRequest("Username or email already exists. Please choose a different username or email.");
            }

            // Lưu thông tin người dùng trước khi cập nhật
            string originalData = GetUserDataAsString(existingUser);

            // Cập nhật thông tin từ user mới vào user cũ
            UpdateUserFields(existingUser, user);

            // Lưu thay đổi vào cơ sở dữ liệu
            this._db.SaveChanges();

            // Tạo nội dung email thông báo về các trường đã thay đổi
            string changedFields = GetChangedFields(originalData, existingUser);
            string subject = "Thông báo: Hồ sơ của bạn đã được chỉnh sửa";
            string body = $"Chào {existingUser.Username},\n\n" +
                          $"Hồ sơ của bạn đã được chỉnh sửa bởi quản trị viên. Các trường đã thay đổi là: {changedFields.ToString()}\n\n" +
                          "\nVui lòng kiểm tra lại thông tin của bạn.\n\n" +
                          "\n\n\nTrân trọng,\nBan quản trị";

            // Gửi email thông báo cho người dùng
            SendEmail(existingUser.Email, subject, body);
            return Ok(existingUser);
        }


        // Hàm gửi email
        private void SendEmail(string toEmail, string subject, string body)
        {
            try
            {
                Mailrequest mailrequest = new Mailrequest();
                mailrequest.ToEmail = toEmail;
                mailrequest.Subject = subject;
                mailrequest.Body = body;
                _emailService.SendEmailAsync(mailrequest); // Gọi dịch vụ gửi email
            }
            catch (Exception ex)
            {
                throw new Exception("Can not sent email");
            }
        }


        // Hàm để lấy thông tin người dùng dưới dạng chuỗi
        private string GetUserDataAsString(Users user)
        {
            return $"{user.FirstName},{user.LastName},{user.DateOfBirth},{user.Gender},{user.Address},{user.Picture},{user.ZipCode},{user.PhoneNumber},{user.Email},{user.IDCard},{user.Password},{user.UserType},{user.isDeleted}";
        }

        // Hàm cập nhật thông tin người dùng từ user mới vào user cũ
        private void UpdateUserFields(Users existingUser, Users newUser)
        {
            existingUser.FirstName = newUser.FirstName;
            existingUser.LastName = newUser.LastName;
            existingUser.DateOfBirth = newUser.DateOfBirth;
            existingUser.Gender = newUser.Gender;
            existingUser.Address = newUser.Address;
            existingUser.Picture = newUser.Picture;
            existingUser.ZipCode = newUser.ZipCode;
            existingUser.PhoneNumber = newUser.PhoneNumber;
            existingUser.Email = newUser.Email;
            existingUser.IDCard = newUser.IDCard;
            existingUser.Password = newUser.Password;
            existingUser.UserType = newUser.UserType;
            existingUser.isDeleted = newUser.isDeleted;
        }

        // Hàm để lấy danh sách trường đã thay đổi và giá trị cũ và mới
        private string GetChangedFields(string originalData, Users updatedUser)
        {
            string[] originalFields = originalData.Split(',');
            string[] updatedFields = GetUserDataAsString(updatedUser).Split(',');

            StringBuilder changedFields = new StringBuilder();

            for (int i = 0; i < originalFields.Length; i++)
            {
                if (originalFields[i] != updatedFields[i])
                {
                    changedFields.Append($"{originalFields[i]} đã được thay đổi từ {originalFields[i]} sang {updatedFields[i]}, ");
                }
            }

            return changedFields.ToString();
        }

        [HttpGet("search/{userName}")]
        public IActionResult GetOrdersFiltered(string userName)
        {
            IQueryable<Users> query = this._db.users;

            if (!string.IsNullOrEmpty(userName))
            {
                query = query.Where(o => o.Username.Contains(userName.Trim().ToLower()));
            }
            var orderList = query.ToList();
            return Ok(orderList);
        }


        public static string ComputeMD5Hash(string password)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(password);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }
    }
}