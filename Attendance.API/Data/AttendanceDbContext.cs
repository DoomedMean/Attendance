using System;
using System.Collections.Generic;
using Attendance.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Attendance.API.Data;

public partial class AttendanceDbContext : DbContext
{
    public AttendanceDbContext()
    {
    }

    public AttendanceDbContext(DbContextOptions<AttendanceDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<EmployeeAttendance> EmployeeAttendances { get; set; }

    public virtual DbSet<OfficeConfiguration> OfficeConfigurations { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.Property(e => e.EmployeeId).ValueGeneratedNever();
        });

        modelBuilder.Entity<EmployeeAttendance>(entity =>
        {
            entity.HasKey(e => e.EmployeeId).HasName("EmployeeAttendances_pkey");

            entity.Property(e => e.EmployeeId).ValueGeneratedNever();

            entity.Property(e => e.isOTClaimed).HasDefaultValue(false);

            entity.HasOne(d => d.Employee).WithOne(p => p.EmployeeAttendance).HasForeignKey<EmployeeAttendance>(d => d.EmployeeId);
        });

        modelBuilder.Entity<OfficeConfiguration>(entity =>
        {
            entity.HasKey(e => e.OfficeId);

            entity.Property(e => e.OfficeId).ValueGeneratedNever();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
