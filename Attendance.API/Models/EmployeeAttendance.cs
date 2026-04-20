using System;
using System.Collections.Generic;

namespace Attendance.API.Models;

public partial class EmployeeAttendance
{
    public Guid EmployeeId { get; set; }

    public DateOnly Date { get; set; }

    public DateTimeOffset? ClockInTime { get; set; }

    public DateTimeOffset? ClockOutTime { get; set; }

    public DateTimeOffset? BreakStartTime { get; set; }

    public DateTimeOffset? BreakEndTime { get; set; }

    public string? Notes { get; set; }

    public bool isOTClaimed { get; set; } = false;

    public virtual Employee Employee { get; set; } = null!;
}
