using Attendance.API.Data;
using Attendance.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Attendance.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttendController : ControllerBase
    {
        private readonly AttendanceDbContext _context;
        public AttendController(AttendanceDbContext context)
        {
            _context = context;
        }
        [HttpPost("Tap")]
        public async Task<IActionResult> TapInOut([FromBody] TapInOutRequest dto)
        {
            Guid.TryParse(dto.EmployeeId, out Guid employeeId);
            var Employee = await _context.Employees.FirstOrDefaultAsync(a => a.EmployeeId == employeeId);
            if (Employee == null)
            {
                return NotFound(new { message = "Employee not found" });
            }

            var employeeAttendance = await _context.EmployeeAttendances
                .FirstOrDefaultAsync(a => a.EmployeeId == employeeId &&
                                            a.Date == DateOnly.FromDateTime(DateTime.UtcNow)
                                            );
            if (employeeAttendance == null)
            {
                employeeAttendance = new EmployeeAttendance
                {
                    EmployeeId = employeeId,
                    Date = DateOnly.FromDateTime(DateTime.UtcNow)
                };
                _context.EmployeeAttendances.Add(employeeAttendance);
            }

            switch (dto.Action?.ToLower())
            {
                case "clockin":
                    employeeAttendance.ClockInTime = DateTimeOffset.UtcNow;
                    break;
                case "clockout":
                    employeeAttendance.ClockOutTime = DateTimeOffset.UtcNow;
                    break;
                case "breakstart":
                    employeeAttendance.BreakStartTime = DateTimeOffset.UtcNow;
                    break;
                case "breakend":
                    employeeAttendance.BreakEndTime = DateTimeOffset.UtcNow;
                    break;
                default:
                    return BadRequest(new { message = "Invalid action. Use 'clockin', 'clockout', 'breakstart', or 'breakend'." });
            }

            await _context.SaveChangesAsync();
            return Ok();
        }
        [HttpGet("Detail")]
        public async Task<IActionResult> GetAttendanceDetail(string employeeId, DateOnly date)
        {
            Guid.TryParse(employeeId, out Guid empId);
            var Employee = await _context.Employees.FirstOrDefaultAsync(a => a.EmployeeId == empId);
            if (Employee == null)
            {
                return NotFound(new { message = "Employee not found" });
            }

            var attendance = await _context.EmployeeAttendances
                .FirstOrDefaultAsync(a => a.EmployeeId == empId && a.Date == date);

            if (attendance == null)
            {
                return Ok(null);
            }

            TimeSpan grossWorkDuration = TimeSpan.Zero;
            if (attendance.ClockInTime.HasValue && attendance.ClockOutTime.HasValue)
            {
                grossWorkDuration = attendance.ClockOutTime.Value - attendance.ClockInTime.Value;
            }

            TimeSpan breakDuration = TimeSpan.Zero;
            if (attendance.BreakStartTime.HasValue && attendance.BreakEndTime.HasValue)
            {
                breakDuration = attendance.BreakEndTime.Value - attendance.BreakStartTime.Value;
            }

            var netWorkDuration = grossWorkDuration > breakDuration
                ? grossWorkDuration - breakDuration
                : TimeSpan.Zero;

            var standardWorkTime = TimeSpan.FromHours(8);
            var overTime = netWorkDuration > standardWorkTime
                ? netWorkDuration - standardWorkTime
                : TimeSpan.Zero;

            return Ok(new AttendanceDetailResponse
            {
                employeeId = attendance.EmployeeId.ToString(),
                date = attendance.Date,
                clockInTime = attendance.ClockInTime.HasValue ? TimeOnly.FromDateTime(attendance.ClockInTime.Value.ToOffset(TimeSpan.FromHours(7)).DateTime) : null,
                clockOutTime = attendance.ClockOutTime.HasValue ? TimeOnly.FromDateTime(attendance.ClockOutTime.Value.ToOffset(TimeSpan.FromHours(7)).DateTime) : null,
                breakStartTime = attendance.BreakStartTime.HasValue ? TimeOnly.FromDateTime(attendance.BreakStartTime.Value.ToOffset(TimeSpan.FromHours(7)).DateTime) : null,
                breakEndTime = attendance.BreakEndTime.HasValue ? TimeOnly.FromDateTime(attendance.BreakEndTime.Value.ToOffset(TimeSpan.FromHours(7)).DateTime) : null,
                workHours = netWorkDuration.TotalHours,
                overTime = overTime.TotalHours,
                notes = attendance.Notes,
                isOTClimed = attendance.isOTClaimed
            });
        }
    }
    #region DTOs
    public record TapInOutRequest(string EmployeeId, string? Action);
    public class AttendanceDetailResponse
    {
        public string employeeId { get; set; } = null!;
        public DateOnly? date { get; set; }
        public TimeOnly? clockInTime { get; set; }
        public TimeOnly? clockOutTime { get; set; }
        public TimeOnly? breakStartTime { get; set; }
        public TimeOnly? breakEndTime { get; set; }
        public double? workHours { get; set; }
        public double? overTime { get; set; }
        public string? notes { get; set; }
        public bool isOTClimed { get; set; }
    }
    #endregion
}
