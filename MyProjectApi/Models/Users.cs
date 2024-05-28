using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MyProjectApi.Models
{
    public class Users
    {
        [Key]
        public int ID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
        public string Picture { get; set; }
        // [Required]
        // [RegularExpression(@"^[1-9]\d*$", ErrorMessage = "ZipCode must be a positive number, and must be equal 6 characters.")]
        public string ZipCode { get; set; }
        [Required]
        // [RegularExpression(@"^0[1-9]{2}[-.\s]?[0-9]{3}[-.\s]?[0-9]{4}$", ErrorMessage = "Please enter a 10-digit phone number beginning with the number '0'!")]
        public string PhoneNumber { get; set; }
        [Required]
        // [RegularExpression(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$", ErrorMessage = "Please enter a valid email address")]
        public string Email { get; set; }
        [Required]
        public string Username { get; set; }
        // [Required]
        // [RegularExpression(@"^[1-9]\d*$", ErrorMessage = "ZipCode must be a positive number.")]
        public string IDCard { get; set; }
        [Required]
        // [RegularExpression(@"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[a-zA-Z]).{8,}$", ErrorMessage = "Please enter a password with at least 8 characters including uppercase, lowercase letters and special characters!")]
        public string Password { get; set; }
        [Required]
        // [Range(2, 3, ErrorMessage = "UserType must be either 2 (staff) or 3 (customer).")]
        public int UserType { get; set; } // admin = 1, staff = 2, customer = 3
        public bool isDeleted { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updateAt { get; set; }

        public Users()
        {

        }
        public Users(string ZipCode, string PhoneNumber, string Email, string Username, string IDCard, string Password, DateTime updateAt)
        {
            this.ZipCode = ZipCode;
            this.PhoneNumber = PhoneNumber;
            this.Email = Email;
            this.Username = Username;
            this.IDCard = IDCard;
            this.Password = Password;
            this.Picture = "l60Hf-150x150.png";
            this.updateAt = DateTime.Now;
        }

        public Users(string FirstName, string LastName, string DateOfBirth, string Gender, string Address, string Picture, string ZipCode, string PhoneNumber, string Email, string Username, string IDCard, string Password, int UserType, bool isDeleted, DateTime createdAt, DateTime updateAt)
        {
            this.FirstName = FirstName;
            this.LastName = LastName;
            this.DateOfBirth = DateOfBirth;
            this.Gender = Gender;
            this.Address = Address;
            this.Picture = Picture;
            this.ZipCode = ZipCode;
            this.PhoneNumber = PhoneNumber;
            this.Email = Email;
            this.Username = Username;
            this.IDCard = IDCard;
            this.Password = Password;
            this.UserType = UserType;
            this.isDeleted = isDeleted;
            this.createdAt = createdAt;
            this.updateAt = updateAt;
        }
    }
}