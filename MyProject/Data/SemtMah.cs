using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MyProject.Core;

namespace MyProject.Data
{
    public class SemtMah 
    {
        [Key]
        public int SemtMahId { get; set; }
        public string SemtAdi { get; set; }
        public string MahalleAdi { get; set; }
        public string PostaKodu { get; set; }
        public int ilceId { get; set; }
    }
}