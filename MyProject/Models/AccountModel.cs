using System.ComponentModel.DataAnnotations;

namespace MyProject.Models
{
    public class AccountModel 
    { 
        [Required(ErrorMessage = "Name field is required.")]
        [RegularExpression(@"^[a-zA-ZğüşıöçĞÜŞİÖÇ]{1,20}$", ErrorMessage = "Only alphabet characters can be used.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "The Surname field is required.")]
        [RegularExpression(@"^[a-zA-ZğüşıöçĞÜŞİÖÇ]{1,20}$", ErrorMessage = "Only alphabet characters can be used.")]
        public string LastName { get; set; }
        
        [Required(ErrorMessage = "Phone field is required.")]
        [RegularExpression(@"^5[0-9]{9,13}$", ErrorMessage = "Please enter a valid phone number. Your phone number must start with 5")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Email field is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; }
        public int CityId { get; set; }
        [Required(ErrorMessage = "Password field is required.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{1,20}$", ErrorMessage = "The password must contain at least one uppercase letter, one lowercase letter, one number and one special character. The password must not be longer than 20 characters.")]
        public string Password { get; set; }
        public string PasswordAgain { get; set; }
        public bool IsActive { get; set; }
    }
}