using System.ComponentModel.DataAnnotations;
using webproje1.Models;

namespace webproje1.ViewModels
{
    public class AIRecommendationViewModel
    {
        [Required(ErrorMessage = "Lütfen bir soru yazın.")]
        [StringLength(500, ErrorMessage = "Soru en fazla 500 karakter olmalıdır.")]
        [Display(Name = "Soru")]
        public string Question { get; set; } = string.Empty;

        [Display(Name = "İstek Türü")]
        [Required]
        public RequestType RequestType { get; set; } = RequestType.ExercisePlan;

        [Display(Name = "AI Yanıtı")]
        public string? Response { get; set; }

        public List<AIRecommendation> History { get; set; } = new();
    }
}

