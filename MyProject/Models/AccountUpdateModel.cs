using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MyProject.Models
{
    public class AccountUpdateModel
    {
        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        [RegularExpression(@"^[a-zA-ZğüşıöçĞÜŞİÖÇ]+$", ErrorMessage = "Sadece harf karakterleri kullanılabilir.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Soyad alanı zorunludur.")]
        [RegularExpression(@"^[a-zA-ZğüşıöçĞÜŞİÖÇ]+$", ErrorMessage = "Sadece harf karakterleri kullanılabilir.")]
        public string LastName { get; set; }

         [Required(ErrorMessage = "Telefon alanı zorunludur.")]
        [RegularExpression(@"^5[0-9]{9,15}$", ErrorMessage = "Geçerli bir telefon numarası giriniz. Telefon numaranız 5 ile başlamalıdır")]
        public string Phone { get; set; }
        public int SehirId { get; set; }
        public DateTime? ModifiedDate { get; set; }
        
    }
}