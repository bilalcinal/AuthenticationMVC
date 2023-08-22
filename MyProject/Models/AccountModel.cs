using System.ComponentModel.DataAnnotations;

namespace MyProject.Models
{
    public class AccountModel 
    {
        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        [RegularExpression(@"^[a-zA-ZğüşıöçĞÜŞİÖÇ]+$", ErrorMessage = "Sadece harf karakterleri kullanılabilir.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Soyad alanı zorunludur.")]
        [RegularExpression(@"^[a-zA-ZğüşıöçĞÜŞİÖÇ]+$", ErrorMessage = "Sadece harf karakterleri kullanılabilir.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Telefon alanı zorunludur.")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Sadece sayılar kullanılabilir.")]
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}