using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MyProject.Core;

namespace MyProject.Data
{
    public class Ulkeler 
    {
        [Key]
        public int UlkeId { get; set; }
        public string IkiliKod { get; set; }
        public string UcluKod { get; set; }
        public string UlkeAdi { get; set; }
        public string TelKodu { get; set; }
    }
}