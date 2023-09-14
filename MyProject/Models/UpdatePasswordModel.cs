using System.ComponentModel.DataAnnotations;

namespace MyProject.Models
{
    public class UpdatePasswordModel
    {
        public string OldPassword { get; set; }
        
        [Required]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{1,20}$", ErrorMessage = "The password must contain at least one uppercase letter, one lowercase letter, one number and one special character. The password must not be longer than 20 characters.")]
        public string NewPassword { get; set; }
        
        [Required]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmNewPassword { get; set; }
    }
}