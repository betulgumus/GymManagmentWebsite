using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace webproje1.ViewModels
{
    public class AdminTrainerViewModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Lütfen bir kullanıcı seçin.")]
        [Display(Name = "Kullanıcı")]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Uzmanlık alanı zorunludur.")]
        [StringLength(200, ErrorMessage = "Uzmanlık en fazla 200 karakter olabilir.")]
        [Display(Name = "Uzmanlık")]
        public string Specialization { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Biyografi en fazla 1000 karakter olabilir.")]
        [Display(Name = "Biyografi")]
        public string? Bio { get; set; }

        [Range(0, 50, ErrorMessage = "Deneyim 0 ile 50 yıl arasında olmalıdır.")]
        [Display(Name = "Deneyim (Yıl)")]
        public int ExperienceYears { get; set; }

        [Url(ErrorMessage = "Geçerli bir URL giriniz.")]
        [StringLength(500)]
        [Display(Name = "Fotoğraf URL")]
        public string? PhotoUrl { get; set; }

        [Display(Name = "Aktif / Müsait")]
        public bool IsAvailable { get; set; } = true;

        [Display(Name = "Hizmetler")]
        [Required(ErrorMessage = "En az bir hizmet seçmelisiniz.")]
        public List<int> SelectedServiceIds { get; set; } = new();

        // Dropdown & Checkbox Listeleri
        public List<SelectListItem> AvailableUsers { get; set; } = new();
        public List<SelectListItem> AvailableServices { get; set; } = new();
    }
}

