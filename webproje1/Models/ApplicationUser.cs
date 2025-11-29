using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using webproje1.Models;

namespace webproje1.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "Ad alanı zorunludur")]
        [MaxLength(50, ErrorMessage = "Ad en fazla 50 karakter olabilir")]
        [Display(Name = "Adınız")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Soyad alanı zorunludur")]
        [MaxLength(50, ErrorMessage = "Soyad en fazla 50 karakter olabilir")]
        [Display(Name = "Soyadınız")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "E-posta adresi zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        [Display(Name = "E-posta")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Şifre zorunludur")]
        [StringLength(100, ErrorMessage = "{0} en az {2} ve en fazla {1} karakter olmalıdır.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Şifre Tekrar")]
        [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor")]
        public string ConfirmPassword { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Doğum Tarihi")]
        public DateTime? DateOfBirth { get; set; }

        [MaxLength(10)]
        [Display(Name = "Cinsiyet")]
        public string? Gender { get; set; }


        // Navigation Properties
        public virtual MemberProfile? MemberProfile { get; set; }
        public virtual Trainer? Trainer { get; set; }
        public virtual ICollection<Appointment> Appointments { get; set; }
        public virtual ICollection<AIRecommendation> AIRecommendations { get; set; }
    }
}