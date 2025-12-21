using System.ComponentModel.DataAnnotations;

namespace webproje1.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email gereklidir")]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Şifre gereklidir")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Password { get; set; }

        [Display(Name = "Beni Hatırla")]
        public bool RememberMe { get; set; }
    }
}
