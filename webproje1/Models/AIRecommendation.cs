using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using webproje1.Models;

namespace webproje1.Models
{
    public enum RequestType
    {
        ExercisePlan,
        DietPlan,
        ProgressVisualization,
        BodyAnalysis
    }

    public class AIRecommendation
    {
        public int Id { get; set; }

        [Required]
        public string MemberId { get; set; }

        public DateTime RequestDate { get; set; } = DateTime.Now;

        [Required]
        public RequestType RequestType { get; set; }

        [MaxLength(2000)]
        public string? InputData { get; set; } // JSON formatında

        [MaxLength(5000)]
        public string? AIResponse { get; set; } // JSON formatında

        [MaxLength(500)]
        public string? ImagePath { get; set; }

        // Navigation Properties
        [ForeignKey("MemberId")]
        public virtual ApplicationUser Member { get; set; }
    }
}