using System.ComponentModel.DataAnnotations;

namespace MyProject.Models
{
    public class AccountModel 
    {
        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        [RegularExpression(@"^[a-zA-ZğüşıöçĞÜŞİÖÇ]{1,20}$", ErrorMessage = "Sadece harf karakterleri kullanılabilir.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Soyad alanı zorunludur.")]
        [RegularExpression(@"^[a-zA-ZğüşıöçĞÜŞİÖÇ]{1,20}$", ErrorMessage = "Sadece harf karakterleri kullanılabilir.")]
        public string LastName { get; set; }
        
        [Required(ErrorMessage = "Telefon alanı zorunludur.")]
        [RegularExpression(@"^5[0-9]{9,15}$", ErrorMessage = "Geçerli bir telefon numarası giriniz. Telefon numaranız 5 ile başlamalıdır")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Email alanı zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz.")]
        public string Email { get; set; }
        public int SehirId { get; set; }
        public int IlceId { get; set; }
        public int UlkeId { get; set; }

        [Required(ErrorMessage = "Şifre alanı zorunludur.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{1,20}$", ErrorMessage = "Şifre en az bir büyük harf, bir küçük harf, bir rakam ve bir özel karakter içermelidir.Şifre 20 karakterden uzun olmamalıdır.")]
        public string Password { get; set; }
        public string PasswordAgain { get; set; }
    }
}