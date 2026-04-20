using System;
using System.Collections.Generic;

namespace Attendance.API.Models;

public partial class OfficeConfiguration
{
    public Guid OfficeId { get; set; }

    public string OfficeName { get; set; } = null!;

    public string? OfficeLicence { get; set; }

    public DateTimeOffset OfficeStartTime { get; set; }

    public DateTimeOffset OfficeEndTime { get; set; }

    public TimeSpan OfficeGracePeriod { get; set; }

    public TimeSpan OfficeBreakDuration { get; set; }

    public string? OfficeDescription { get; set; }
}
