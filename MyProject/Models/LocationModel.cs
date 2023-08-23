using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyProject.Models
{
    public class LocationModel
    {
        public string SelectedCity { get; set; }
        public string SelectedDistrict { get; set; }
        public List<string> Cities { get; set; }
        public Dictionary<string, List<string>> DistrictsByCity { get; set; }
    }
}