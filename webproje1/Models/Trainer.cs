using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using webproje1.Models;

namespace webproje1.Models
{
    public class Trainer
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public int GymCenterId { get; set; }

        [MaxLength(1000)]
        public string? Biography { get; set; }

        [MaxLength(500)]
        public string? Specializations { get; set; } // Virgülle ayrılmış: "Kilo Verme, Kas Geliştirme, Yoga"

        [Range(0, 50)]
        public int ExperienceYears { get; set; }

        [MaxLength(500)]
        public string? CertificationInfo { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 5000)]
        public decimal? HourlyRate { get; set; }

        public bool IsAvailable { get; set; } = true;

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        [ForeignKey("GymCenterId")]
        public virtual GymCenter GymCenter { get; set; }

        public virtual ICollection<TrainerService> TrainerServices { get; set; }
        public virtual ICollection<TrainerAvailability> Availabilities { get; set; }
        public virtual ICollection<Appointment> Appointments { get; set; }
    }
}