using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MailKit;
using Microsoft.AspNetCore.Mvc;
using MyProjectApi.DB;
using MyProjectApi.Helpter;
using MyProjectApi.Service;

namespace MyProjectApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ForgotPasswordController : ControllerBase
    {
        private readonly ApplicationDBContext _db;
        private readonly IEmailService emailService;

        public ForgotPasswordController(IEmailService emailService, ApplicationDBContext db)
        {
            this.emailService = (IEmailService)emailService;
            this._db = db;
        }

        [HttpGet("SendMail/{toIEmail}")]
        public async Task<IActionResult> SendMail(string toIEmail)
        {
            string rdn = randomNumber();
            try
            {
                Mailrequest mailrequest = new Mailrequest();
                mailrequest.ToEmail = toIEmail;
                mailrequest.Subject = "User Management - Reset Password";
                mailrequest.Body = GetHtmlcontent(toIEmail, rdn);
                await emailService.SendEmailAsync(mailrequest);
                return Ok(rdn);
            }
            catch (Exception ex)
            {
                throw new Exception("Can't send mail");
            }
        }

        private string randomNumber()
        {
            string rs = "";
            Random random = new Random();
            for (int i = 0; i < 5; i++)
            {
                rs += random.Next(0, 10) + " ";
            }
            return rs.Trim();
        }
        private string GetHtmlcontent(string username, string id)
        {
            string Response = "<div style=\"width:100%;background-color:lightblue;text-align:center;margin:10px\">";
            Response += "<h1>Dear " + username + " </h1>";
            Response += "<h2>You have requested to reset the password for your User Management account. Here is your verification code:</h2>";
            Response += "<h1>" + id + "</h1>";
            Response += "<p>Please note that this verification code is only valid for a short period of time. Please do not share this code with anyone else.</p>";
            Response += "<p>If you did not request a password reset, please disregard this email.</p>";
            Response += "<div><h1> Contact us : datdtce171751@fpt.edu.vn</h1></div>";
            Response += "</div>";
            return Response;
        }

        [HttpGet("ResetPass/{Iusername}/{Ipass}")]
        public IActionResult ResetPass(string Iusername, string Ipass)
        {
            if (Iusername == null)
            {
                return BadRequest("Email can't be nul;");
            }
            if (Ipass == null)
            {
                return BadRequest("Pass can't be null");
            }
            var user = this._db.users.FirstOrDefault(u => u.Username.Equals(Iusername));

            user.Password = ComputeMD5Hash(Ipass);
            this._db.SaveChanges();
            return Ok(User);
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