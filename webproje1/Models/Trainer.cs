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