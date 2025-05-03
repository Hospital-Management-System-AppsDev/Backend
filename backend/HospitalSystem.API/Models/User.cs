using System;
using System.ComponentModel.DataAnnotations;

namespace HospitalApp.Models
{
    public class User
    {
        public int Id { get; set; }

        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [RegularExpression(@"^[^\s]+$", ErrorMessage = "Email should not contain spaces")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Username can only contain letters and numbers")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }

        public string Sex { get; set; }

        [Required(ErrorMessage = "Contact number is required")]
        public string ContactNumber { get; set; }

        public DateTime Birthday { get; set; }

        // Age is calculated from Birthday
        public int Age
        {
            get
            {
                var today = DateTime.Today;
                int age = today.Year - Birthday.Year;
                if (Birthday.Date > today.AddYears(-age)) age--;
                return age;
            }
        }

        public virtual string Role { get; set; }
    }
}
