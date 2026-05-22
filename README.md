# 📋 Attendance System

A full-stack **Employee Attendance Management System** built with a modern tech stack — featuring a RESTful API backend and a reactive single-page application frontend. Employees can clock in/out, manage breaks, track work hours, and claim overtime — all from a clean, minimal web interface.

---

## 🖼️ Screenshots

| Login | Calendar | Attendance Detail |
|---|---|---|
| ![Login](img/Screenshot%202026-04-20%20165016.png) | ![Calendar](img/Screenshot%202026-04-20%20165235.png) | ![Detail](img/Screenshot%202026-04-20%20165307.png) |

---

## ⚙️ Tech Stack

### Backend — `Attendance.API`

| Layer | Technology |
|---|---|
| Runtime | .NET 10 / ASP.NET Core 10 |
| Language | C# 13 |
| ORM | Entity Framework Core 10 |
| Database | PostgreSQL (via Npgsql EF Provider) |
| Password Hashing | Argon2id (`Konscious.Security.Cryptography`) |
| API Docs | ASP.NET Core OpenAPI (built-in) |
| Architecture | RESTful API, Controller-based MVC |

### Frontend — `Attendance.UI`

| Layer | Technology |
|---|---|
| Framework | Vue 3 (Composition API + `<script setup>`) |
| Language | TypeScript 6 |
| Build Tool | Vite 8 |
| State Management | Pinia 3 |
| Routing | Vue Router 5 |
| Linting | ESLint 10 + OXLint |
| Formatting | Prettier |

---

## 🗃️ Database Schema

```
┌─────────────────────────┐         ┌───────────────────────────────┐
│       Employee           │         │      EmployeeAttendance        │
├─────────────────────────┤         ├───────────────────────────────┤
│ EmployeeId       UUID PK │────1:1──│ EmployeeId       UUID FK/PK   │
│ Name             STRING  │         │ Date             DATE PK       │
│ Password         STRING  │         │ ClockInTime      TIMESTAMPTZ   │
│ PlaceOfBirth     STRING  │         │ ClockOutTime     TIMESTAMPTZ   │
│ DateOfBirth      DATE    │         │ BreakStartTime   TIMESTAMPTZ   │
│ Gender           BOOL    │         │ BreakEndTime     TIMESTAMPTZ   │
│ Address          STRING  │         │ Notes            TEXT          │
│ PhoneNumber      STRING  │         │ isOTClaimed      BOOL          │
│ Email            STRING  │         └───────────────────────────────┘
│ Position         STRING  │
│ Department       STRING  │         ┌───────────────────────────────┐
│ IdentityNo       STRING  │         │     OfficeConfiguration        │
│ IdentityType     ENUM    │         ├───────────────────────────────┤
│ IdentityExpiryDate DATE  │         │ OfficeId         UUID PK       │
│ EmergencyContact STRING  │         │ OfficeName       STRING        │
│ EmergencyContactPhone    │         │ OfficeLicence    STRING        │
└─────────────────────────┘         │ OfficeStartTime  TIMESTAMPTZ   │
                                     │ OfficeEndTime    TIMESTAMPTZ   │
                                     │ OfficeGracePeriod INTERVAL     │
                                     │ OfficeBreakDuration INTERVAL   │
                                     │ OfficeDescription TEXT         │
                                     └───────────────────────────────┘
```

---

## 🔐 Security

- Passwords are hashed using **Argon2id** with per-user random salt (128-bit), 4 iterations, and 64 MB memory cost — resistant to GPU and side-channel attacks.
- Hash verification uses `CryptographicOperations.FixedTimeEquals` to prevent **timing attacks**.
- Hash format stores all parameters inline: `iterations.memoryKB.degree.salt.key`, enabling future parameter upgrades without invalidating existing hashes.
- CORS is configured via a named policy — only the designated frontend origin is allowed in production.

---

## 🌐 REST API Endpoints

### `EmployeeController` — `/api/Employee`

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/Employee/Login` | Authenticate employee, returns `employeeId` + `employeeName` |
| `POST` | `/api/Employee/new-employee` | Register a new employee with full profile |

### `AttendController` — `/api/Attend`

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/Attend/Tap` | Clock In, Clock Out, Break Start, or Break End |
| `GET` | `/api/Attend/Detail` | Get attendance record with computed work hours & overtime |
| `POST` | `/api/Attend/ClaimOvertime` | Claim overtime for a given date |

### Tap Actions (via `/api/Attend/Tap`)

```json
{ "employeeId": "...", "action": "clockin" }
{ "employeeId": "...", "action": "breakstart" }
{ "employeeId": "...", "action": "breakend" }
{ "employeeId": "...", "action": "clockout" }
```

### Attendance Detail Response

```json
{
  "employeeId": "uuid",
  "date": "2026-04-20",
  "clockInTime": "08:00:00",
  "clockOutTime": "17:15:00",
  "breakStartTime": "12:00:00",
  "breakEndTime": "13:00:00",
  "workHours": 8.25,
  "overTime": 0.25,
  "notes": null,
  "isOTClimed": false
}
```

---

## 🧠 Business Logic

- **Net Work Hours** = (Clock Out − Clock In) − Break Duration
- **Overtime** = Net Work Hours − 8 hours (if positive)
- All timestamps stored as `TIMESTAMPTZ` (UTC), converted to **WIB (UTC+7)** for display
- Overtime can only be claimed after clock-out if overtime ≥ 1 hour and not yet claimed
- All tap actions are date-scoped — one record per employee per day

---

## 🗂️ Project Structure

```
Attendance/
├── Attendance.API/               # ASP.NET Core Web API
│   ├── Controllers/
│   │   ├── AttendController.cs   # Tap in/out, attendance detail, overtime
│   │   └── EmployeeController.cs # Login, employee registration
│   ├── Data/
│   │   └── AttendanceDbContext.cs
│   ├── Models/
│   │   ├── Employee.cs
│   │   ├── EmployeeAttendance.cs
│   │   └── OfficeConfiguration.cs
│   ├── Migrations/               # EF Core migrations
│   ├── Shared/
│   │   └── AppEnum.cs            # IdentityType enum
│   └── Program.cs
│
└── Attendance.UI/                # Vue 3 SPA
    └── src/
        ├── views/
        │   ├── LoginView.vue
        │   ├── CalendarView.vue
        │   └── AttendanceView.vue
        ├── stores/
        │   └── auth.ts           # Pinia auth store
        ├── services/
        │   └── api.ts            # Typed fetch wrappers
        └── router/
            └── index.ts
```

---

## 🚀 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js ≥ 20.19](https://nodejs.org/)
- [PostgreSQL](https://www.postgresql.org/)

### Backend Setup

```bash
# Navigate to API project
cd Attendance.API

# Configure connection string in appsettings.Development.json
# "ConnectionStrings": { "PostgreAttendance": "Host=...;Database=...;Username=...;Password=..." }

# Apply migrations
dotnet ef database update

# Run the API
dotnet run
# API available at https://localhost:7059
```

### Frontend Setup

```bash
cd Attendance.UI

# Install dependencies
npm install

# Start dev server
npm run dev
# UI available at http://localhost:5173
```

---

## 📦 Dependencies

### Backend NuGet Packages

| Package | Version | Purpose |
|---|---|---|
| `Npgsql.EntityFrameworkCore.PostgreSQL` | 10.0.1 | PostgreSQL EF Core provider |
| `Microsoft.EntityFrameworkCore` | 10.0.6 | ORM |
| `Microsoft.EntityFrameworkCore.Design` | 10.0.6 | Migrations tooling |
| `Microsoft.AspNetCore.OpenApi` | 10.0.6 | OpenAPI/Swagger |
| `Konscious.Security.Cryptography.Argon2` | 1.3.1 | Argon2id password hashing |

### Frontend npm Packages

| Package | Version | Purpose |
|---|---|---|
| `vue` | ^3.5.32 | UI framework |
| `vue-router` | ^5.0.4 | Client-side routing |
| `pinia` | ^3.0.4 | State management |
| `vite` | ^8.0.8 | Build tool & dev server |
| `typescript` | ~6.0.0 | Type safety |
| `oxlint` | ~1.60.0 | Ultra-fast Rust-based linter |

---

## 👤 Author

Project  full-stack development with modern .NET and Vue 3 ecosystems.

---

<p align="center">
  <img src="https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white"/>
  <img src="https://img.shields.io/badge/C%23-13-239120?style=for-the-badge&logo=csharp&logoColor=white"/>
  <img src="https://img.shields.io/badge/Vue.js-3-4FC08D?style=for-the-badge&logo=vuedotjs&logoColor=white"/>
  <img src="https://img.shields.io/badge/TypeScript-6-3178C6?style=for-the-badge&logo=typescript&logoColor=white"/>
  <img src="https://img.shields.io/badge/PostgreSQL-17-4169E1?style=for-the-badge&logo=postgresql&logoColor=white"/>
  <img src="https://img.shields.io/badge/Vite-8-646CFF?style=for-the-badge&logo=vite&logoColor=white"/>
  <img src="https://img.shields.io/badge/Argon2id-secured-red?style=for-the-badge&logo=springsecurity&logoColor=white"/>
</p>
