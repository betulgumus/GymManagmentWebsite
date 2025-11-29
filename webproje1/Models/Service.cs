using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using webproje1.Models;

namespace webproje1.Models
{
    public enum ServiceType
    {
        Fitness,
        Yoga,
        Pilates,
        PersonalTraining,
        GroupTraining,
        Cardio,
        Zumba,
        Spinning,
        CrossFit,
        Boxing
    }

    public class Service
    {
        public int Id { get; set; }

        [Required]
        public int GymCenterId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        [Range(15, 240)]
        public int DurationMinutes { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 10000)]
        public decimal Price { get; set; }

        [Required]
        public ServiceType ServiceType { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation Properties
        [ForeignKey("GymCenterId")]
        public virtual GymCenter GymCenter { get; set; }
        public virtual ICollection<TrainerService> TrainerServices { get; set; }
        public virtual ICollection<Appointment> Appointments { get; set; }
    }
}