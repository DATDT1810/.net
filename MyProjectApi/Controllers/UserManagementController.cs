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
    public class UserManagementController : ControllerBase
    {
        private readonly ApplicationDBContext _db;
        public UserManagementController(ApplicationDBContext db)
        {
            this._db = db;
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
            var user = this._db.users .FirstOrDefault(u => u.Username == id);
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
            Users obj = this._db.users.AsNoTracking().FirstOrDefault(u => u.Username.Equals(id));
            if (obj == null)
            {
                return NotFound("Could not be found");
            }
            var hassPass = ComputeMD5Hash(user.Password);
            this._db.users.Update(user);
            this._db.SaveChanges();
            return Ok(user);
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