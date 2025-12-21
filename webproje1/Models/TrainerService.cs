using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using webproje1.Models;

namespace webproje1.Models
{
    public class TrainerService
    {
        public int Id { get; set; }

        [Required]
        public int TrainerId { get; set; }

        [Required]
        public int ServiceId { get; set; }

        // Navigation Properties
        [ForeignKey("TrainerId")]
        public virtual Trainer Trainer { get; set; }

        [ForeignKey("ServiceId")]
        public virtual Service Service { get; set; }
    }
}