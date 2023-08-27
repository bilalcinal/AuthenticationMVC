using MyProject.Core;

namespace MyProject.Data
{
    public class RegisterToken : BaseEntity
    {
        public string Email { get; set; }
        public string Token { get; set; }
        public DateTime Expires { get; set; }
    }
}