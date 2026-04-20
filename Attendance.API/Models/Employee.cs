using System;
using System.Collections.Generic;

namespace Attendance.API.Models;

public partial class Employee
{
    public Guid EmployeeId { get; set; }

    public string Name { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string PlaceOfBirth { get; set; } = null!;

    public DateTime DateOfBirth { get; set; }

    public bool Gender { get; set; }

    public string Address { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Position { get; set; } = null!;

    public string Department { get; set; } = null!;

    public string IdentityNo { get; set; } = null!;

    public string IdentityType { get; set; } = null!;

    public DateTime IdentityExpiryDate { get; set; }

    public string EmergencyContact { get; set; } = null!;

    public string EmergencyContactPhoneNumber { get; set; } = null!;

    public virtual EmployeeAttendance? EmployeeAttendance { get; set; }
}
