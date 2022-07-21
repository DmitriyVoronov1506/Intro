using Microsoft.AspNetCore.Http;

namespace WebApplicationClassWork.Models
{
    public class RegUserModel
    {
        public string RealName { get; set; }
        public string Login { get; set; }
        public string Password1 { get; set; }
        public string Password2 { get; set; }
        public string Email { get; set; }
        public IFormFile Avatar { get; set; }
    }
}
