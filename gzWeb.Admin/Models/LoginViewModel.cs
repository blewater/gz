using System.ComponentModel.DataAnnotations;

namespace gzWeb.Admin.Models
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Username / Email")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}