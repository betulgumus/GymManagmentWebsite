using System;
using System.ComponentModel.DataAnnotations;

namespace webproje1.Models
{
    public class TrainerAvailability
    {
        public int Id { get; set; }

        [Required]
        public int TrainerId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime AvailableDate { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan EndTime { get; set; }

        public bool IsActive { get; set; } = true;

        public Trainer Trainer { get; set; }
    }
}
