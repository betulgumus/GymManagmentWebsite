using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace webproje1.Models
{
    public class GymCenter
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [MaxLength(200)]
        public string Address { get; set; }

        [Required]
        [Phone]
        [MaxLength(20)]
        public string PhoneNumber { get; set; }

        // SeedData "Phone" kullanıyor, sen "PhoneNumber" kullanıyorsun
        // İkisi de çalışsın diye alias ekleyelim
        [NotMapped] // Veritabanına eklenmez
        public string Phone
        {
            get => PhoneNumber;
            set => PhoneNumber = value;
        }

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan OpeningTime { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan ClosingTime { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public virtual ICollection<Service> Services { get; set; }
        public virtual ICollection<Trainer> Trainers { get; set; }
        public virtual ICollection<Appointment> Appointments { get; set; }
    }
}