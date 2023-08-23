using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyProject.Data;

namespace MyProject.Models
{
    public class AccountPlaceOfResidence
    {   // Diğer AccountModel özellikleri
        public List<Ulkeler> Countries { get; set; }
        public List<Sehirler> Cities { get; set; }
        public List<Ilceler> Districts { get; set; }
        public List<SemtMah> Neighborhoods { get; set; }

        // Seçilen ülke, il, ilçe ve semt/mahalle Id'leri
        public int SelectedCountryId { get; set; }
        public int SelectedCityId { get; set; }
        public int SelectedDistrictId { get; set; }
        public int SelectedNeighborhoodId { get; set; }
        

    }
}