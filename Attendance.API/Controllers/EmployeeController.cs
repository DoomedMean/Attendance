using Attendance.API.Data;
using Attendance.API.Models;
using Konscious.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Attendance.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly AttendanceDbContext _context;
        public EmployeeController(AttendanceDbContext context)
        {
            _context = context;
        }
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest dto)
        {
            var employee = await _context.Employees.FirstOrDefaultAsync(a => a.Email == dto.Email);
            if (employee == null || !VerifyPassword(employee.Password, dto.Password))
            {
                return Unauthorized(new { message = "Email or password incorrect" });
            }
            return Ok(new { message = "Login successful", employeeId = employee.EmployeeId, employeeName = employee.Name });
        }
        [HttpPost("new-employee")]
        public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeRequest dto)
        {
            var employee = new Employee
            {
                EmployeeId = Guid.NewGuid(),
                Name = dto.Name,
                Password = HashPassword(dto.Password),
                PlaceOfBirth = dto.PlaceOfBirth,
                DateOfBirth = dto.DateOfBirth,
                Gender = dto.Gender,
                Address = dto.Address,
                PhoneNumber = dto.PhoneNumber,
                Email = dto.Email,
                Position = dto.Position,
                Department = dto.Department,
                IdentityNo = dto.IdentityNo,
                IdentityType = dto.IdentityType,
                IdentityExpiryDate = dto.IdentityExpiryDate,
                EmergencyContact = dto.EmergencyContact,
                EmergencyContactPhoneNumber = dto.EmergencyContactPhoneNumber,
            };
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Employee created successfully", employeeId = employee.EmployeeId });
        }

        #region Methods
        public string HashPassword(string password)
        {
            // Implement your password hashing logic here
            // For example, you can use a library like BCrypt or PBKDF2 to hash the password securely
            var salt = new byte[SaltSize];
            RandomNumberGenerator.Fill(salt);

            var degree = Math.Max(1, Environment.ProcessorCount);
            var argon = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = salt,
                DegreeOfParallelism = degree,
                Iterations = ArgonIterations,
                MemorySize = ArgonMemoryKB
            };

            var key = argon.GetBytes(KeySize);

            var saltB64 = Convert.ToBase64String(salt);
            var keyB64 = Convert.ToBase64String(key);

            // Store as: iterations.memoryKB.degree.salt.key
            return $"{ArgonIterations}.{ArgonMemoryKB}.{degree}.{saltB64}.{keyB64}";
        }

        // Verifies a stored hash in the format: iterations.memoryKB.degree.salt.key
        private static bool VerifyPassword(string hashedPassword, string password)
        {
            if (string.IsNullOrWhiteSpace(hashedPassword)) return false;

            var parts = hashedPassword.Split('.');
            if (parts.Length != 5) return false;

            if (!int.TryParse(parts[0], out var iterations)) return false;
            if (!int.TryParse(parts[1], out var memoryKB)) return false;
            if (!int.TryParse(parts[2], out var degree)) return false;

            var salt = Convert.FromBase64String(parts[3]);
            var key = Convert.FromBase64String(parts[4]);

            var argon = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = salt,
                DegreeOfParallelism = degree,
                Iterations = iterations,
                MemorySize = memoryKB
            };

            var attempted = argon.GetBytes(key.Length);

            return CryptographicOperations.FixedTimeEquals(attempted, key);
        }
        #endregion

        #region Constants
        private const int SaltSize = 16; // 128 bits
        private const int KeySize = 32; // 256 bits

        // Argon2 parameters (tunable)
        private const int ArgonIterations = 4; // number of passes
        private const int ArgonMemoryKB = 64 * 1024; // 64 MB in KB
        #endregion
    }

    #region DTOs
    public record LoginRequest(string Email, string Password);
    public class CreateEmployeeRequest
    {
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
    }
    #endregion
}
