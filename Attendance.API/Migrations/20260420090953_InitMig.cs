using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Attendance.API.Migrations
{
    /// <inheritdoc />
    public partial class InitMig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Password = table.Column<string>(type: "text", nullable: false),
                    PlaceOfBirth = table.Column<string>(type: "text", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Gender = table.Column<bool>(type: "boolean", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Position = table.Column<string>(type: "text", nullable: false),
                    Department = table.Column<string>(type: "text", nullable: false),
                    IdentityNo = table.Column<string>(type: "text", nullable: false),
                    IdentityType = table.Column<string>(type: "text", nullable: false),
                    IdentityExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EmergencyContact = table.Column<string>(type: "text", nullable: false),
                    EmergencyContactPhoneNumber = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.EmployeeId);
                });

            migrationBuilder.CreateTable(
                name: "OfficeConfigurations",
                columns: table => new
                {
                    OfficeId = table.Column<Guid>(type: "uuid", nullable: false),
                    OfficeName = table.Column<string>(type: "text", nullable: false),
                    OfficeLicence = table.Column<string>(type: "text", nullable: true),
                    OfficeStartTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    OfficeEndTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    OfficeGracePeriod = table.Column<TimeSpan>(type: "interval", nullable: false),
                    OfficeBreakDuration = table.Column<TimeSpan>(type: "interval", nullable: false),
                    OfficeDescription = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfficeConfigurations", x => x.OfficeId);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeAttendances",
                columns: table => new
                {
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    ClockInTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClockOutTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    BreakStartTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    BreakEndTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    isOTClaimed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("EmployeeAttendances_pkey", x => x.EmployeeId);
                    table.ForeignKey(
                        name: "FK_EmployeeAttendances_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeAttendances");

            migrationBuilder.DropTable(
                name: "OfficeConfigurations");

            migrationBuilder.DropTable(
                name: "Employees");
        }
    }
}
