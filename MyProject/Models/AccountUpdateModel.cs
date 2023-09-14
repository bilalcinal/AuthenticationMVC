using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MyProject.Models
{
    public class AccountUpdateModel
    {
        [Required(ErrorMessage = "Name field is required.")]
        [RegularExpression(@"^[a-zA-ZğüşıöçĞÜŞİÖÇ]{1,20}$", ErrorMessage = "Only alphabet characters can be used.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "The Surname field is required.")]
        [RegularExpression(@"^[a-zA-ZğüşıöçĞÜŞİÖÇ]{1,20}$", ErrorMessage = "Only alphabet characters can be used.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Phone field is required.")]
        [RegularExpression(@"^5[0-9]{9,13}$", ErrorMessage = "Geçerli bir telefon numarası giriniz. Telefon numaranız 5 ile başlamalıdır")]
        public string Phone { get; set; }
        public int CityId { get; set; }
        public DateTime? ModifiedDate { get; set; }
        
    }
}