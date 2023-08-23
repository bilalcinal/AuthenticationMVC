using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MyProject.Core;

namespace MyProject.Data
{
    public class Sehirler 
    {
        [Key]
        public int SehirId { get; set; }
        public string SehirAdi { get; set; }
        public int PlakaNo { get; set; }
        public int TelefonKodu { get; set; }
        public int RowNumber { get; set; }
    }
}