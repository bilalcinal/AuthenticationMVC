using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MyProject.Core;

namespace MyProject.Data
{
    public class City 
    {
        [Key]
        public int CityId { get; set; }
        public string CityName{ get; set; }
        public int NumberPlate { get; set; }
        public int PhoneCode { get; set; }
        public int RowNumber { get; set; }
    }
}