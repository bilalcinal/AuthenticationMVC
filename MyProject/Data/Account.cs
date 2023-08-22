using System.ComponentModel.DataAnnotations;
using MyProject.Core;

namespace MyProject.Data
{
    public class Account : BaseEntity
    {
    
        [RegularExpression(@"^[a-zA-Z]+$")]
        public string FirstName { get; set; }

        [RegularExpression(@"^[a-zA-Z]+$")]
        public string LastName { get; set; }
        
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$", ErrorMessage = "Geçersiz e-posta adresi.")]
        public string Email { get; set; }

        [RegularExpression(@"^\d{10}$", ErrorMessage = "Geçersiz telefon numarası.")]
        public string Phone { get; set; }
        public string Password { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}