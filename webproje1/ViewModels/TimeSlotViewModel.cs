namespace webproje1.ViewModels
{
    public class TimeSlotViewModel
    {
        public int TrainerId { get; set; }
        public int ServiceId { get; set; }

        public DateTime Date { get; set; }

        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}
