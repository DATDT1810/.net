using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyProjectApi.DB;
using MyProjectApi.Models;

namespace MyProjectApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminProfileController : ControllerBase
    {
        private readonly ApplicationDBContext _db;
        public AdminProfileController(ApplicationDBContext db)
        {
            this._db = db;
        }
        [HttpGet]
        public IActionResult GetAllOfUser()
        {
            var ListOfUser = this._db.users
                                .Where(u => u.UserType == 1 || u.UserType == 2);
            return Ok(ListOfUser);
        }

        [HttpGet("{id}")]
        public IActionResult GetUserByUsername(string id)
        {
            var user = this._db.users.FirstOrDefault(u => u.Username == id);
            return Ok(user);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateUserProfile(string id, Users user)
        {
            if (user == null)
            {
                return BadRequest("User can't be null");
            }

            // Retrieve the existing user from the database
            var existingUser = this._db.users.FirstOrDefault(u => u.Username == id);
            if (existingUser == null)
            {
                return NotFound("User could not be found");
            }

            // Update the existing user's properties
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
            existingUser.createdAt = user.createdAt;
            existingUser.updateAt = user.updateAt;
            if (existingUser.Password == user.Password)
            {
                existingUser.Password = user.Password;
            }
            else
            {
                existingUser.Password = ComputeMD5Hash(user.Password);
            }
            // Save the changes to the database
            this._db.SaveChanges();

            return Ok(existingUser);
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