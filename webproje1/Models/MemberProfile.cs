using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace webproje1.Models
{
    public enum BodyType
    {
        Ectomorph,  // Zayıf yapılı
        Mesomorph,  // Atletik yapılı
        Endomorph   // Kalın yapılı
    }

    public class MemberProfile
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        // ===== AccountController İÇİN EKSİK PROPERTY'LER =====
        [MaxLength(100)]
        public string? FirstName { get; set; }  // ← YENİ EKLENDI!

        [MaxLength(100)]
        public string? LastName { get; set; }  // ← YENİ EKLENDI!

        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }  // ← YENİ EKLENDI!

        public DateTime JoinDate { get; set; } = DateTime.Now;  // ← YENİ EKLENDI!

        // ===== MEVCUT FITNESS PROPERTY'LERİN =====
        [Range(100, 250)]
        public int? Height { get; set; } // cm

        [Column(TypeName = "decimal(5,2)")]
        [Range(30, 300)]
        public decimal? Weight { get; set; } // kg

        public BodyType? BodyType { get; set; }

        [MaxLength(200)]
        public string? FitnessGoal { get; set; }

        [MaxLength(500)]
        public string? HealthConditions { get; set; }

        [MaxLength(500)]
        public string? ProfilePhotoPath { get; set; }

        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        public virtual ICollection<Appointment> Appointments { get; set; }  // ← İlişki eklendi
    }
}