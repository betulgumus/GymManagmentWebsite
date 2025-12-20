using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webproje1.Data;
using webproje1.Models;

namespace webproje1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AppointmentsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AppointmentsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/AppointmentsApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAppointments()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Member)
                .Include(a => a.Trainer)
                    .ThenInclude(t => t.User)
                .Include(a => a.Service)
                .Include(a => a.GymCenter)
                .OrderByDescending(a => a.AppointmentDate)
                .Select(a => new
                {
                    a.Id,
                    AppointmentDate = a.AppointmentDate.ToString("yyyy-MM-dd"),
                    StartTime = a.StartTime.ToString(@"hh\:mm"),
                    EndTime = a.EndTime.ToString(@"hh\:mm"),
                    MemberName = a.Member.FirstName + " " + a.Member.LastName,
                    MemberEmail = a.Member.Email,
                    TrainerName = a.Trainer.User.FirstName + " " + a.Trainer.User.LastName,
                    ServiceName = a.Service.Name,
                    GymCenterName = a.GymCenter.Name,
                    a.TotalPrice,
                    Status = a.Status.ToString(),
                    a.Notes,
                    CreatedDate = a.CreatedDate.ToString("yyyy-MM-dd HH:mm")
                })
                .ToListAsync();

            return Ok(appointments);
        }

        // GET: api/AppointmentsApi/ByDate?date=2025-12-20
        [HttpGet("ByDate")]
        public async Task<ActionResult<IEnumerable<object>>> GetAppointmentsByDate([FromQuery] DateTime date)
        {
            var appointments = await _context.Appointments
                .Include(a => a.Member)
                .Include(a => a.Trainer)
                    .ThenInclude(t => t.User)
                .Include(a => a.Service)
                .Include(a => a.GymCenter)
                .Where(a => a.AppointmentDate.Date == date.Date)  // LINQ Filtreleme
                .OrderBy(a => a.StartTime)
                .Select(a => new
                {
                    a.Id,
                    AppointmentDate = a.AppointmentDate.ToString("yyyy-MM-dd"),
                    StartTime = a.StartTime.ToString(@"hh\:mm"),
                    EndTime = a.EndTime.ToString(@"hh\:mm"),
                    MemberName = a.Member.FirstName + " " + a.Member.LastName,
                    MemberEmail = a.Member.Email,
                    TrainerName = a.Trainer.User.FirstName + " " + a.Trainer.User.LastName,
                    ServiceName = a.Service.Name,
                    GymCenterName = a.GymCenter.Name,
                    a.TotalPrice,
                    Status = a.Status.ToString(),
                    a.Notes
                })
                .ToListAsync();

            return Ok(appointments);
        }

        // GET: api/AppointmentsApi/ByDateRange?startDate=2025-12-01&endDate=2025-12-31
        [HttpGet("ByDateRange")]
        public async Task<ActionResult<IEnumerable<object>>> GetAppointmentsByDateRange(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            var appointments = await _context.Appointments
                .Include(a => a.Member)
                .Include(a => a.Trainer)
                    .ThenInclude(t => t.User)
                .Include(a => a.Service)
                .Where(a => a.AppointmentDate.Date >= startDate.Date &&
                           a.AppointmentDate.Date <= endDate.Date)  // LINQ Filtreleme
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.StartTime)
                .Select(a => new
                {
                    a.Id,
                    AppointmentDate = a.AppointmentDate.ToString("yyyy-MM-dd"),
                    StartTime = a.StartTime.ToString(@"hh\:mm"),
                    EndTime = a.EndTime.ToString(@"hh\:mm"),
                    MemberName = a.Member.FirstName + " " + a.Member.LastName,
                    TrainerName = a.Trainer.User.FirstName + " " + a.Trainer.User.LastName,
                    ServiceName = a.Service.Name,
                    a.TotalPrice,
                    Status = a.Status.ToString()
                })
                .ToListAsync();

            return Ok(appointments);
        }

        // GET: api/AppointmentsApi/Statistics
        [HttpGet("Statistics")]
        public async Task<ActionResult<object>> GetStatistics()
        {
            var total = await _context.Appointments.CountAsync();
            var pending = await _context.Appointments
                .Where(a => a.Status == AppointmentStatus.Pending)
                .CountAsync();
            var confirmed = await _context.Appointments
                .Where(a => a.Status == AppointmentStatus.Confirmed)
                .CountAsync();
            var completed = await _context.Appointments
                .Where(a => a.Status == AppointmentStatus.Completed)
                .CountAsync();
            var cancelled = await _context.Appointments
                .Where(a => a.Status == AppointmentStatus.Cancelled)
                .CountAsync();

            var totalRevenue = await _context.Appointments
                .Where(a => a.Status == AppointmentStatus.Completed)
                .SumAsync(a => a.TotalPrice);

            return Ok(new
            {
                TotalAppointments = total,
                PendingAppointments = pending,
                ConfirmedAppointments = confirmed,
                CompletedAppointments = completed,
                CancelledAppointments = cancelled,
                TotalRevenue = totalRevenue
            });
        }
    }
}
