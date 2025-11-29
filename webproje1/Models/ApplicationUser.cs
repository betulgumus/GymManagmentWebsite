using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using webproje1.Models;

namespace webproje1.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [MaxLength(10)]
        public string? Gender { get; set; }

        [MaxLength(200)]
        public string? Address { get; set; }

        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public virtual MemberProfile? MemberProfile { get; set; }
        public virtual Trainer? Trainer { get; set; }
        public virtual ICollection<Appointment> Appointments { get; set; }
        public virtual ICollection<AIRecommendation> AIRecommendations { get; set; }
    }
}