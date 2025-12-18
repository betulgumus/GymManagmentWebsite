using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace webproje1.Models
{
    public class ApplicationUser : IdentityUser
    {
        // SADECE EKSTRA ALANLAR!
        // Email, Password, ConfirmPassword IdentityUser'da zaten var!

        [MaxLength(50)]
        public string? FirstName { get; set; }

        [MaxLength(50)]
        public string? LastName { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [MaxLength(10)]
        public string? Gender { get; set; }

        // Navigation Properties
        public virtual MemberProfile? MemberProfile { get; set; }
        public virtual Trainer? Trainer { get; set; }
        public virtual ICollection<Appointment> Appointments { get; set; }
        public virtual ICollection<AIRecommendation> AIRecommendations { get; set; }
    }
}