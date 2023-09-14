using System.ComponentModel.DataAnnotations;

namespace MyProject.Core
{
    public class BaseEntity
    {
        [Key]
        public int Id { get; set; }
    }
}