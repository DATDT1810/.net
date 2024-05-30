using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyProjectApi.DB;
using MyProjectApi.Models;

namespace MyProjectApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDBContext _db;
        public AuthController(ApplicationDBContext db)
        {
            this._db = db;
        }
        [HttpGet("")]
        public IActionResult GetAllOfUser()
        {
            var listOfUser = this._db.users.ToList();
            return Ok(listOfUser);
        }

        [HttpGet("getUser/{username}/{pass}")]
        public IActionResult GetUserByUserName(string username, string pass)
        {
            var hashPass = ComputeMD5Hash(pass);
            var user = this._db.users.FirstOrDefault(u => u.Username.Equals(username) && u.Password.Equals(hashPass));
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        [HttpGet("getUser/{email:regex(^\\S+@\\S+\\.\\S+$)}/{pass}")]
        public IActionResult GetUserByEmail(string email, string pass)
        {
            var hashPass = ComputeMD5Hash(pass);
            var user = this._db.users.FirstOrDefault(u => u.Email.Equals(email) && u.Password.Equals(hashPass));
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        [HttpPost]
        public IActionResult SignUp(Users user)
        {
            if (user == null)
            {
                return BadRequest("User data is required");
            }

            var existingUser = this._db.users.FirstOrDefault(u => u.Username == user.Username || u.Email == user.Email);
            if (existingUser != null)
            {
                return BadRequest("Username or email already exists. Please choose a different username or email.");
            }

            user.Password = ComputeMD5Hash(user.Password);
            user.Picture = "l60Hf-150x150.png";
            user.createdAt = DateTime.Now;
            user.updateAt = DateTime.Now;

            this._db.users.Add(user);
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