# 📋 Smart Meeting Room Booking System — Complete Documentation

> **Purpose:** This document explains the entire project end-to-end.
> A beginner C# developer should be able to read this and fully understand the system.

---

## 📌 Table of Contents

1. [What is this Project?](#1-what-is-this-project)
2. [Tech Stack](#2-tech-stack)
3. [High Level Design (HLD)](#3-high-level-design-hld)
4. [Low Level Design (LLD)](#4-low-level-design-lld)
5. [Folder Structure](#5-folder-structure)
6. [Database Design](#6-database-design)
7. [SOLID Principles Applied](#7-solid-principles-applied)
8. [Design Patterns Used](#8-design-patterns-used)
9. [API Endpoints](#9-api-endpoints)
10. [End-to-End Flow Walkthroughs](#10-end-to-end-flow-walkthroughs)
11. [JWT Authentication Explained](#11-jwt-authentication-explained)
12. [Business Rules](#12-business-rules)
13. [Error Handling](#13-error-handling)
14. [How to Run the Project](#14-how-to-run-the-project)
15. [Testing Guide](#15-testing-guide)

---

## 1. What is this Project?

A **Meeting Room Booking REST API** built with ASP.NET Core (.NET 8).

### Problem it solves:
In an office, multiple teams need to book meeting rooms. Without a system:
- Two teams might book the same room at the same time (double booking)
- There is no way to know which rooms are available
- No access control (anyone can do anything)

### What this system provides:
- Users can **register and login** securely
- **Admins** can create meeting rooms
- **Users** can view available rooms and book them
- The system **prevents double bookings** automatically
- Users can only **cancel their own bookings**

---

## 2. Tech Stack

| Technology | Version | Purpose |
|------------|---------|---------|
| ASP.NET Core Web API | .NET 8 | Web framework to build REST APIs |
| Entity Framework Core | 8.0.4 | ORM — talks to the database using C# instead of SQL |
| SQLite | via EF Core | Lightweight file-based database (smartbooking.db) |
| JWT (JSON Web Tokens) | Microsoft.AspNetCore.Authentication.JwtBearer 8.x | Authentication — proving who you are |
| BCrypt.Net-Next | 4.0.3 | Password hashing — never store plain text passwords |
| Swashbuckle (Swagger) | 6.5.0 | Auto-generates API documentation and test UI |

---

## 3. High Level Design (HLD)

### What is HLD?
HLD describes the system at a bird's eye view — the major components and how they talk to each other. No code details here.

```
┌─────────────────────────────────────────────────────────┐
│                    CLIENT (Browser/App)                 │
│              Sends HTTP Requests with JSON              │
└────────────────────────┬────────────────────────────────┘
                         │ HTTP Request
                         ▼
┌─────────────────────────────────────────────────────────┐
│               ASP.NET Core Web API                      │
│                                                         │
│  ┌──────────────┐   ┌──────────────┐  ┌─────────────┐   │
│  │ Middleware   │   │  Controllers  │  │   Swagger  │   |
│  │ (JWT Auth +  │──▶│  (AuthCtrl   │  │     UI      │    |
│  │  Exception   │   │   RoomsCtrl  │  └─────────────┘    │
│  │  Handling)   │   │  BookingCtrl)│                     │
│  └──────────────┘   └──────┬───────┘                     │
│                             │                            │
│                    ┌────────▼────────┐                   │
│                    │    Services     │                   │
│                    │  (AuthService   │                   │
│                    │   RoomService   │                   │
│                    │ BookingService) │                   │
│                    └────────┬────────┘                   │
│                             │                            │
│                    ┌────────▼────────┐                   │
│                    │  Unit of Work   │                   │
│                    │  + Repositories │                   │
│                    └────────┬────────┘                   │
│                             │                            │
│                    ┌────────▼────────┐                   │
│                    │  AppDbContext   │                   │
│                    │  (EF Core)      │                   │
│                    └────────┬────────┘                   │
└─────────────────────────────┼─────────────────────────── ┘
                              │ SQL Queries
                              ▼
                   ┌──────────────────────┐
                   │   smartbooking.db    │
                   │   (SQLite Database)  │
                   │  ┌────────────────┐  │
                   │  │  Users Table   │  │
                   │  │  Rooms Table   │  │
                   │  │ Bookings Table │  │
                   │  └────────────────┘  │
                   └──────────────────────┘
```

### How a request flows (simplified):
```
Client → Middleware (check JWT) → Controller → Service → Repository → Database
                                                                           │
Client ← Middleware (catch errors) ← Controller ← Service ← Repository ←─┘
```

---

## 4. Low Level Design (LLD)

### What is LLD?
LLD goes deeper — it describes each class, its properties, methods, and how classes connect to each other.

### Class Diagram

```
┌─────────────────────────────────────────────────────────────────────┐
│                        CORE LAYER                                    │
│                                                                      │
│  ┌──────────────────┐  ┌──────────────────┐  ┌──────────────────┐  │
│  │      User        │  │      Room        │  │     Booking      │  │
│  │──────────────────│  │──────────────────│  │──────────────────│  │
│  │ + Id: int        │  │ + Id: int        │  │ + Id: int        │  │
│  │ + Name: string   │  │ + Name: string   │  │ + RoomId: int    │  │
│  │ + Email: string  │  │ + Capacity: int  │  │ + Room: Room     │  │
│  │ + PasswordHash   │  └──────────────────┘  │ + UserId: int    │  │
│  │ + Role: string   │                         │ + User: User     │  │
│  └──────────────────┘                         │ + StartTime      │  │
│                                               │ + EndTime        │  │
│                                               └──────────────────┘  │
│                                                                      │
│  ┌───────────────────────────┐  ┌──────────────────────────────┐   │
│  │  IGenericRepository<T>    │  │        IUnitOfWork           │  │
│  │───────────────────────────│  │──────────────────────────────│   │
│  │ + GetByIdAsync(id)        │  │ + Users: IGenericRepo<User>  │  │
│  │ + GetAllAsync()           │  │ + Rooms: IGenericRepo<Room>  │  │
│  │ + FindAsync(predicate)    │  │ + Bookings: IGenericRepo<..> │  │
│  │ + AddAsync(entity)        │  │ + SaveChangesAsync()         │  │
│  │ + Update(entity)          │  └──────────────────────────────┘   │
│  │ + Remove(entity)          │                                      │
│  └───────────────────────────┘                                     │
│                                                                 │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────────────┐  │
│  │ IAuthService │  │ IRoomService │  │     IBookingService      │  │
│  │──────────────│  │──────────────│  │──────────────────────────│  │
│  │ + Register() │  │ + GetAll()   │  │ + CreateBookingAsync()   │  │
│  │ + Login()    │  │ + Create()   │  │ + CancelBookingAsync()   │  │
│  └──────────────┘  └──────────────┘  │ + GetUserBookingsAsync() │  │
│                                       └──────────────────────────┘  │
└──────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────┐
│                     INFRASTRUCTURE LAYER                             │
│                                                                      │
│  ┌────────────────────────┐      ┌───────────────────────────────┐  │
│  │  GenericRepository<T>  │      │          UnitOfWork            │  │
│  │  implements            │      │  implements IUnitOfWork        │  │
│  │  IGenericRepository<T> │      │───────────────────────────────│  │
│  │────────────────────────│      │ - _context: AppDbContext       │  │
│  │ - _context: AppDbContext│      │ + Users: GenericRepo<User>    │  │
│  │ - _dbSet: DbSet<T>     │      │ + Rooms: GenericRepo<Room>    │  │
│  │ + All methods via EF   │      │ + Bookings: GenericRepo<..>   │  │
│  └────────────────────────┘      │ + SaveChangesAsync()          │  │
│                                  │ + Dispose()                   │  │
│  ┌────────────────────────┐      └───────────────────────────────┘  │
│  │     AppDbContext       │                                          │
│  │────────────────────────│                                          │
│  │ + Users: DbSet<User>   │                                          │
│  │ + Rooms: DbSet<Room>   │                                          │
│  │ + Bookings: DbSet<...> │                                          │
│  │ + OnModelCreating()    │                                          │
│  └────────────────────────┘                                          │
└──────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────┐
│                      APPLICATION LAYER                               │
│                                                                      │
│  ┌────────────────┐  ┌────────────────┐  ┌───────────────────┐     │
│  │  AuthService   │  │  RoomService   │  │  BookingService   │     │
│  │────────────────│  │────────────────│  │───────────────────│     │
│  │uses IUnitOfWork│  |uses IUnitOfWork│  │ uses IUnitOfWork  │     │
│  │ uses IConfig   │  │                │  │                   │     │
│  │ uses BCrypt    │  │                │  │ Contains:         │     │
│  │ generates JWT  │  │                │  │ Overlap check     │     │
│  └────────────────┘  └────────────────┘  └───────────────────┘     │
└──────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────┐
│                      PRESENTATION LAYER                              │
│                                                                      │
│  ┌────────────────┐  ┌────────────────┐  ┌───────────────────┐     │
│  │ AuthController │  │ RoomsController│  │BookingsController │     │
│  │────────────────│  │────────────────│  │───────────────────│     │
│  │ POST /register │  │ GET /rooms     │  │ GET /bookings/my  │     │
│  │ POST /login    │  │ POST /rooms    │  │ POST /bookings    │     │
│  │                │  │ (Admin only)   │  │ DELETE /bookings  │     │
│  └────────────────┘  └────────────────┘  └───────────────────┘     │
└──────────────────────────────────────────────────────────────────────┘
```

---

## 5. Folder Structure

```
SmartBookingApi/
│
├── 📁 Core/                          ← Domain Layer (no EF, no HTTP, pure business)
│   ├── 📁 Entities/
│   │   ├── User.cs                   ← Represents a system user (Id, Name, Email, PasswordHash, Role)
│   │   ├── Room.cs                   ← Represents a meeting room (Id, Name, Capacity)
│   │   └── Booking.cs                ← Represents a booking (links User + Room + Time)
│   └── 📁 Interfaces/
│       ├── IGenericRepository.cs     ← Contract for DB operations (generic, works for any entity)
│       ├── IUnitOfWork.cs            ← Contract for transaction management
│       ├── IAuthService.cs           ← Contract for Register/Login
│       ├── IRoomService.cs           ← Contract for room operations
│       └── IBookingService.cs        ← Contract for booking operations
│
├── 📁 Infrastructure/                ← Data Access Layer (EF Core lives here)
│   ├── 📁 Data/
│   │   └── AppDbContext.cs           ← EF Core DbContext — maps entities to DB tables
│   └── 📁 Repositories/
│       ├── GenericRepository.cs      ← Implements IGenericRepository using EF Core
│       └── UnitOfWork.cs             ← Implements IUnitOfWork, holds all repositories
│
├── 📁 Application/                   ← Business Logic Layer
│   ├── 📁 DTOs/
│   │   ├── AuthDtos.cs               ← RegisterDto, LoginDto
│   │   ├── RoomDto.cs                ← RoomDto (input & output shape)
│   │   └── BookingDtos.cs            ← CreateBookingDto (input), BookingDto (output)
│   └── 📁 Services/
│       ├── AuthService.cs            ← Handles register (BCrypt hash) + login (JWT generation)
│       ├── RoomService.cs            ← Handles get all rooms + create room
│       └── BookingService.cs         ← Handles create booking (with overlap check) + cancel + get
│
├── 📁 Controllers/                   ← Presentation Layer (thin HTTP layer)
│   ├── AuthController.cs             ← POST /api/auth/register, POST /api/auth/login
│   ├── RoomsController.cs            ← GET /api/rooms, POST /api/rooms
│   └── BookingsController.cs         ← GET /api/bookings/my, POST /api/bookings, DELETE /api/bookings/{id}
│
├── 📁 Middleware/
│   └── ExceptionHandlingMiddleware.cs ← Catches ALL exceptions, returns clean JSON errors
│
├── 📁 Migrations/                    ← Auto-generated by EF Core (don't edit manually)
│   ├── 20260410_InitialCreate.cs     ← SQL to create all tables
│   └── AppDbContextModelSnapshot.cs  ← EF Core's snapshot of current DB state
│
├── Program.cs                        ← App startup: registers services, configures middleware
├── appsettings.json                  ← Configuration: DB connection string, JWT settings
└── smartbooking.db                   ← The actual SQLite database file (auto-created)
```

---

## 6. Database Design

### Tables and Columns

```
┌─────────────────────────────────────────┐
│              Users Table                │
├──────────────┬────────────┬─────────────┤
│ Column       │ Type       │ Notes       │
├──────────────┼────────────┼─────────────┤
│ Id           │ INTEGER    │ Primary Key, Auto-increment │
│ Name         │ TEXT(100)  │ Required    │
│ Email        │ TEXT(150)  │ Required, UNIQUE INDEX │
│ PasswordHash │ TEXT       │ BCrypt hash of password │
│ Role         │ TEXT       │ "User" or "Admin" │
└──────────────┴────────────┴─────────────┘

┌─────────────────────────────────────────┐
│              Rooms Table                │
├──────────────┬────────────┬─────────────┤
│ Column       │ Type       │ Notes       │
├──────────────┼────────────┼─────────────┤
│ Id           │ INTEGER    │ Primary Key, Auto-increment │
│ Name         │ TEXT(50)   │ Required, UNIQUE INDEX |
│ Capacity     │ INTEGER    │ Required    │
└──────────────┴────────────┴─────────────┘

┌──────────────────────────────────────────────────────────────────────┐
│                          Bookings Table                               │
├───────────────┬────────────┬──────────────────────────────────────── ┤
│ Column        │ Type       │ Notes                                    │
├───────────────┼────────────┼──────────────────────────────────────── ┤
│ Id            │ INTEGER    │ Primary Key, Auto-increment              │
│ RoomId        │ INTEGER    │ Foreign Key → Rooms.Id (CASCADE DELETE)  │
│ UserId        │ INTEGER    │ Foreign Key → Users.Id (CASCADE DELETE)  │
│ StartTime     │ TEXT       │ Required (stored as ISO 8601 string)     │
│ EndTime       │ TEXT       │ Required                                 │
└───────────────┴────────────┴──────────────────────────────────────── ┘
```

### Entity Relationships

```
Users ──┐
        │ 1 User can have MANY Bookings
        └──────────────────────────────▶ Bookings
                                              ▲
        ┌─────────────────────────────────────┘
        │ 1 Room can have MANY Bookings
Rooms ──┘
```

**Cascade Delete:** If a Room is deleted → all its Bookings are automatically deleted.
If a User is deleted → all their Bookings are automatically deleted.

---

## 7. SOLID Principles Applied

### S — Single Responsibility Principle
> *"A class should have only ONE reason to change."*

| Class | Its ONE responsibility |
|---|---|
| `AuthController` | Handle HTTP request/response for auth routes only |
| `AuthService` | Business logic for register and login only |
| `GenericRepository<T>` | Database CRUD operations only |
| `UnitOfWork` | Transaction management only (SaveChanges) |
| `ExceptionHandlingMiddleware` | Error catching and formatting only |

### O — Open/Closed Principle
> *"Open for extension, closed for modification."*

`GenericRepository<T>` works for `User`, `Room`, AND `Booking` without any changes to it.
To add a new entity (e.g. `Floor`), just add `IGenericRepository<Floor>` to `IUnitOfWork` — the base `GenericRepository` doesn't change.

### L — Liskov Substitution Principle
> *"You should be able to replace a class with its subclass/implementation without breaking the program."*

Anywhere `IUnitOfWork` is used, you can swap `UnitOfWork` with a mock implementation (e.g. for unit testing) and the code will still work correctly.

### I — Interface Segregation Principle
> *"Don't force classes to implement methods they don't need."*

Instead of ONE giant `IApplicationService` with all methods:
```
✅ IAuthService    → only Register, Login
✅ IRoomService    → only GetAll, Create
✅ IBookingService → only Create, Cancel, GetUserBookings
```
Each controller only depends on the interface it needs.

### D — Dependency Inversion Principle
> *"Depend on abstractions, not concretions."*

```csharp
// ✅ CORRECT — depends on interface (abstraction)
public class BookingService : IBookingService
{
    private readonly IUnitOfWork _unitOfWork; // ← interface, not UnitOfWork class
}

// ❌ WRONG — depends on concrete class
public class BookingService
{
    private readonly UnitOfWork _unitOfWork; // ← concrete class, hard to test/swap
}
```
All services receive `IUnitOfWork`, not `UnitOfWork`. All controllers receive `IXxxService`, not the concrete service.

---

## 8. Design Patterns Used

### Pattern 1: Repository Pattern
**Problem:** Business logic (services) shouldn't know how data is stored (SQL, files, etc.)  
**Solution:** Put all DB access behind an interface (`IGenericRepository<T>`).

```
Service → IGenericRepository<T>  ← interface (contract)
                ↑
          GenericRepository<T>    ← implementation (uses EF Core)
```

**Benefit:** If you switch from SQLite to SQL Server tomorrow, only `GenericRepository` changes. Services don't change at all.

### Pattern 2: Unit of Work Pattern
**Problem:** If a booking fails halfway through (room added, but confirmation failed), you get dirty/partial data.  
**Solution:** All operations share ONE `DbContext`. Nothing saves until you call `SaveChangesAsync()`.

```
_unitOfWork.Bookings.AddAsync(booking);  // Stage in memory
// ... more operations ...
_unitOfWork.SaveChangesAsync();          // ONE commit - all or nothing
```

### Pattern 3: Dependency Injection (DI)
**Problem:** If classes create their own dependencies (`new UnitOfWork()`), they're tightly coupled and untestable.  
**Solution:** Register dependencies in `Program.cs`, ASP.NET Core injects them automatically.

```csharp
// In Program.cs — we register:
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IBookingService, BookingService>();

// In BookingService — ASP.NET Core automatically provides IUnitOfWork:
public BookingService(IUnitOfWork unitOfWork)  // ← injected automatically
```

`AddScoped` means: create ONE instance per HTTP request, dispose when request ends.

### Pattern 4: DTO Pattern (Data Transfer Object)
**Problem:** Returning raw Entity objects exposes sensitive fields (PasswordHash) and DB internals.  
**Solution:** Map entities to DTOs that contain only what the caller should see.

```
User entity:    Id, Name, Email, PasswordHash, Role
UserDto (none): We never return a User DTO — we just return the JWT token
RoomDto:        Id, Name, Capacity (no DB metadata)
BookingDto:     Id, RoomId, RoomName, UserId, StartTime, EndTime (RoomName is extra — from join)
```

---

## 9. API Endpoints

| Method | URL | Auth Required | Role | Purpose |
|---|---|---|---|---|
| POST | `/api/auth/register` | ❌ No | Any | Register new user |
| POST | `/api/auth/login` | ❌ No | Any | Login, get JWT token |
| GET | `/api/rooms` | ✅ Yes | Any user | Get all rooms |
| POST | `/api/rooms` | ✅ Yes | Admin only | Create a room |
| GET | `/api/bookings/my` | ✅ Yes | Any user | Get my bookings |
| POST | `/api/bookings` | ✅ Yes | Any user | Create a booking |
| DELETE | `/api/bookings/{id}` | ✅ Yes | Owner only | Cancel own booking |

### Request/Response Shapes

#### POST /api/auth/register
```json
// Request body:
{ "name": "John Doe", "email": "john@test.com", "password": "Pass@123" }

// Response (200 OK):
{ "message": "Registration successful." }
```

#### POST /api/auth/login
```json
// Request body:
{ "email": "john@test.com", "password": "Pass@123" }

// Response (200 OK):
{ "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." }
```

#### GET /api/rooms
```json
// Response (200 OK):
[
  { "id": 1, "name": "Conference Room A", "capacity": 10 },
  { "id": 2, "name": "Board Room", "capacity": 20 }
]
```

#### POST /api/rooms
```json
// Request body:
{ "id": 0, "name": "Conference Room B", "capacity": 8 }

// Response (201 Created):
{ "id": 3, "name": "Conference Room B", "capacity": 8 }
```

#### POST /api/bookings
```json
// Request body:
{ "roomId": 1, "startTime": "2026-04-11T09:00:00", "endTime": "2026-04-11T10:00:00" }

// Response (201 Created):
{
  "id": 1,
  "roomId": 1,
  "roomName": "Conference Room A",
  "userId": 2,
  "startTime": "2026-04-11T09:00:00",
  "endTime": "2026-04-11T10:00:00"
}
```

---

## 10. End-to-End Flow Walkthroughs

### Flow 1: User Registration

```
Step 1: Client sends POST /api/auth/register with { name, email, password }

Step 2: ExceptionHandlingMiddleware wraps the request in try/catch

Step 3: JWT Middleware checks — no [Authorize] on this endpoint, so it passes through

Step 4: AuthController.Register() receives the RegisterDto

Step 5: Controller calls _authService.RegisterAsync(dto)

Step 6: AuthService checks if email already exists:
        _unitOfWork.Users.FindAsync(u => u.Email == dto.Email)
        → If found: throw InvalidOperationException("Email already exists")

Step 7: AuthService creates User entity:
        PasswordHash = BCrypt.HashPassword(dto.Password)
        Role = "User"

Step 8: _unitOfWork.Users.AddAsync(user) → stages in memory
        _unitOfWork.SaveChangesAsync()   → writes to database

Step 9: Returns "Registration successful."

Step 10: Controller wraps in Ok({ message }) → HTTP 200
```

### Flow 2: User Login

```
Step 1: Client sends POST /api/auth/login with { email, password }

Step 2: AuthController.Login() calls _authService.LoginAsync(dto)

Step 3: AuthService finds user by email:
        _unitOfWork.Users.FindAsync(u => u.Email == dto.Email)

Step 4: Verifies password:
        BCrypt.Verify(dto.Password, user.PasswordHash)
        → If wrong: throw UnauthorizedAccessException("Invalid email or password")

Step 5: Generates JWT token with claims:
        - sub: userId
        - email: user.Email
        - role: user.Role
        - jti: unique token ID
        Signs with SecretKey from appsettings.json
        Sets expiry to 60 minutes

Step 6: Returns JWT token string

Step 7: Client stores token and sends it with every future request as:
        Authorization: Bearer eyJhbGci...
```

### Flow 3: Create a Booking (The Core Feature)

```
Step 1: Client sends POST /api/bookings with { roomId, startTime, endTime }
        + Header: Authorization: Bearer {token}

Step 2: ExceptionHandlingMiddleware wraps in try/catch

Step 3: JWT Middleware validates the token:
        - Is signature valid? (uses SecretKey to verify)
        - Is token expired?
        - Is issuer and audience correct?
        → If invalid: return 401 Unauthorized automatically

Step 4: JWT Middleware extracts claims from token and puts into HttpContext.User

Step 5: [Authorize] attribute on BookingsController checks — user is authenticated ✅

Step 6: BookingsController.CreateBooking() runs:
        int userId = GetCurrentUserId();  // reads "sub" claim from token
        calls _bookingService.CreateBookingAsync(dto, userId)

Step 7: BookingService checks for overlapping bookings:
        FindAsync(b =>
            b.RoomId == dto.RoomId &&
            b.StartTime < dto.EndTime &&   // existing starts before new ends
            b.EndTime > dto.StartTime)     // existing ends after new starts

        → If overlap found: throw InvalidOperationException("Room already booked")
        → ExceptionHandlingMiddleware catches it → returns 400 Bad Request

Step 8: No overlap — create booking:
        new Booking { RoomId, UserId (from JWT!), StartTime, EndTime }
        _unitOfWork.Bookings.AddAsync(booking)
        _unitOfWork.SaveChangesAsync()  ← actual DB write happens here

Step 9: Fetch room name for response:
        _unitOfWork.Rooms.GetByIdAsync(booking.RoomId)

Step 10: Map to BookingDto and return

Step 11: Controller returns 201 Created with booking details
```

### Flow 4: Error Flow (Double Booking)

```
Step 1: Client tries to book Room 1 from 9:30 to 11:00
        (Room 1 already booked from 9:00 to 10:00)

Step 2: BookingService.FindAsync() finds the existing booking
        Overlap condition: 9:00 < 11:00 (✅) AND 10:00 > 9:30 (✅) = OVERLAP!

Step 3: throw new InvalidOperationException("Room is already booked for this time slot.")

Step 4: Exception travels up the call stack to ExceptionHandlingMiddleware

Step 5: Middleware catches InvalidOperationException:
        → Sets HTTP status to 400 Bad Request
        → Writes JSON: { "error": "Room is already booked for this time slot." }

Step 6: Client receives clean error response — no stack trace exposed
```

---

## 11. JWT Authentication Explained

### What is JWT?
JWT = JSON Web Token. It's a compact, self-contained way to securely transmit information.

A JWT looks like this (3 parts separated by dots):
```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9    ← Header (algorithm info)
.eyJzdWIiOiIxIiwiZW1haWwiOiJqb2hu...     ← Payload (claims/data)
.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV...   ← Signature (verification)
```

### What's inside our JWT (Claims):
```json
{
  "sub": "2",                  // UserId — who this token belongs to
  "email": "john@test.com",   // User's email
  "role": "Admin",            // User's role — used for [Authorize(Roles="Admin")]
  "jti": "abc-123-...",       // Unique token ID (prevents reuse)
  "exp": 1712345678           // Expiry timestamp (60 minutes from login)
}
```

### How it works in our system:

```
1. LOGIN → Server creates JWT signed with SecretKey → sends to client
2. CLIENT stores the token (in memory/localStorage)
3. EVERY REQUEST → client sends token in header: Authorization: Bearer {token}
4. SERVER validates: Is signature valid? Is it expired? Is issuer correct?
5. If valid → extract userId from "sub" claim → proceed
6. If invalid → return 401 Unauthorized
```

### Why is it secure?
- The token is **signed** with a secret key only the server knows
- If someone changes the payload (e.g. changes userId from 2 to 1), the signature won't match → rejected
- Tokens **expire** after 60 minutes → stolen tokens become useless

---

## 12. Business Rules

### Rule 1: No Double Booking
```csharp
// A new booking overlaps with an existing one if:
existingBooking.StartTime < newBooking.EndTime    // existing starts before new ends
AND
existingBooking.EndTime > newBooking.StartTime    // existing ends after new starts

// Visual example:
// Existing:  [========]           (9:00 - 10:00)
// New 1:           [========]     (9:30 - 11:00) ← OVERLAP ❌
// New 2:                [====]    (10:00 - 11:00) ← OK ✅ (starts exactly when old ends)
// New 3: [====]                   (8:00 - 9:00)  ← OK ✅ (ends exactly when old starts)
```

### Rule 2: Only Owner Can Cancel Booking
```csharp
if (booking.UserId != userId)  // userId comes from JWT token
    throw new UnauthorizedAccessException("You can only cancel your own bookings.");
```

### Rule 3: Only Admin Can Create Rooms
```csharp
[Authorize(Roles = "Admin")]  // ASP.NET Core checks the "role" claim in JWT
public async Task<IActionResult> CreateRoom([FromBody] RoomDto dto)
```

### Rule 4: Passwords Are Never Stored in Plain Text
```csharp
// Registration:
PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
// Produces: "$2a$11$Kh8e5v3Xm..." (60-char hash)

// Login verification:
BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
// Returns true or false — original password is never seen again
```

### Rule 5: Unique Email per User
Enforced at the DB level: `HasIndex(e => e.Email).IsUnique()`  
AND at the service level: check before inserting.

---

## 13. Error Handling

All errors are handled centrally by `ExceptionHandlingMiddleware`. No try/catch needed in controllers.

| Exception Type | HTTP Status | When it happens |
|---|---|---|
| `InvalidOperationException` | 400 Bad Request | Double booking, duplicate email |
| `KeyNotFoundException` | 404 Not Found | Booking ID doesn't exist |
| `UnauthorizedAccessException` | 403 Forbidden | Cancelling someone else's booking |
| `Exception` (any other) | 500 Internal Server Error | Unexpected bugs |
| JWT invalid/missing | 401 Unauthorized | Handled by ASP.NET Core JWT middleware automatically |
| Wrong role | 403 Forbidden | Handled by ASP.NET Core `[Authorize(Roles)]` automatically |

### Error Response Format
All errors return the same clean JSON shape:
```json
{ "error": "Descriptive error message here." }
```

---

## 14. How to Run the Project

### Prerequisites
- Visual Studio 2022 (or later)
- .NET 8 SDK

### Steps
1. Open `SmartBookingApi.sln` in Visual Studio
2. Press **F5** (Debug) or **Ctrl+F5** (Run without debug)
3. App starts on `http://localhost:5121`
4. Open browser: `http://localhost:5121/swagger`

### First Time Setup
The database (`smartbooking.db`) is created automatically when you first run `Update-Database` in Package Manager Console.  
The file appears in: `SmartBookingApi\SmartBookingApi\smartbooking.db`

### Configuration (appsettings.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=smartbooking.db"
  },
  "JwtSettings": {
    "SecretKey": "ThisIsAVeryStrongSecretKeyForSmartBooking2024!",
    "Issuer": "SmartBookingApi",
    "Audience": "SmartBookingClient",
    "ExpiryInMinutes": 60
  }
}
```

---

## 15. Testing Guide

### Step-by-Step Testing via Swagger

**Step 1: Register a User**
- `POST /api/auth/register`
- Body: `{ "name": "Admin", "email": "admin@test.com", "password": "Admin@123" }`
- Expected: `200 OK` → `{ "message": "Registration successful." }`

**Step 2: Login**
- `POST /api/auth/login`
- Body: `{ "email": "admin@test.com", "password": "Admin@123" }`
- Expected: `200 OK` → `{ "token": "eyJ..." }`
- **Copy the token!**

**Step 3: Authorize in Swagger**
- Click the **🔓 Authorize** button (top right)
- Enter: `Bearer {paste your token here}`
- Click Authorize → Close

**Step 4: Create a Room** (requires Admin role)
- `POST /api/rooms`
- Body: `{ "id": 0, "name": "Conference Room A", "capacity": 10 }`
- Expected: `201 Created` → `{ "id": 1, "name": "Conference Room A", "capacity": 10 }`

**Step 5: Get All Rooms**
- `GET /api/rooms`
- Expected: `200 OK` → array of rooms

**Step 6: Create a Booking**
- `POST /api/bookings`
- Body: `{ "roomId": 1, "startTime": "2026-04-11T09:00:00", "endTime": "2026-04-11T10:00:00" }`
- Expected: `201 Created` with booking details including `roomName`

**Step 7: Test Double Booking Prevention** ⭐
- `POST /api/bookings` again with overlapping time:
- Body: `{ "roomId": 1, "startTime": "2026-04-11T09:30:00", "endTime": "2026-04-11T11:00:00" }`
- Expected: `400 Bad Request` → `{ "error": "Room is already booked for this time slot." }`

**Step 8: Get My Bookings**
- `GET /api/bookings/my`
- Expected: `200 OK` → list of your bookings

**Step 9: Cancel a Booking**
- `DELETE /api/bookings/1`
- Expected: `204 No Content`

---

## 🎓 Key Concepts Summary for Interviews

| Question | Answer |
|---|---|
| What is EF Core? | An ORM (Object Relational Mapper) that lets you write C# instead of SQL |
| What is Code-First? | You write C# classes first, EF Core generates the DB tables |
| What is a Migration? | A C# file describing how to update the DB schema. `Up()` applies it, `Down()` rolls it back |
| What is Repository Pattern? | Abstracting DB access behind an interface so business logic doesn't know the DB details |
| What is Unit of Work? | A pattern that groups operations — all saved at once or not at all (atomic transactions) |
| What is JWT? | A signed token containing user claims, used for stateless authentication |
| What is BCrypt? | A password hashing algorithm that is intentionally slow to prevent brute-force attacks |
| What is AddScoped? | Creates one instance per HTTP request — shared within the request, disposed after |
| Why ControllerBase not Controller? | Controller adds View/Razor support. ControllerBase is pure API — lighter and appropriate |
| What is middleware? | Code that runs before/after every request. Ordered pipeline — sequence matters |

---

*Documentation generated for: Smart Meeting Room Booking System v1.0*
*Built with ASP.NET Core 8, Entity Framework Core, SQLite, JWT Authentication*
