using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace webproje1.Models
{
    public class Trainer
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public int GymCenterId { get; set; }

        // ===== EKSİK PROPERTY'LER (SeedData bunları kullanıyor) =====
        [Required]
        [MaxLength(200)]
        public string Specialization { get; set; } = string.Empty;  // ← YENİ!

        [MaxLength(1000)]
        public string? Bio { get; set; }  // ← YENİ!

        public int ExperienceYears { get; set; } = 0;  // ← YENİ!

        [MaxLength(500)]
        public string? PhotoUrl { get; set; }  // ← YENİ!

        // ===== MEVCUT PROPERTY'LER =====
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