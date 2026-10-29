# MediCare – Doctor Appointment Booking System

MediCare is a modern, responsive, and robust **Doctor Appointment Booking System** built with **ASP.NET Core MVC (C#)**, **Entity Framework Core**, **Bootstrap 5**, and **ASP.NET Core Identity**.

---

## 🌟 Key Features

1. **Patient Portal**:
   - **Hero Section**: Modern medical welcome banner with trust metrics and quick booking CTA.
   - **Featured Specialists**: Interactive doctor cards highlighting experience, fee, available days, and departmental badges.
   - **Search & Filtering**: Real-time filtering by doctor name and medical specialization.
   - **Appointment Booking**:
     - Pre-selects doctor when navigating from a doctor card.
     - Validates patient name, email, and 10-digit phone number.
     - Enforces that appointment dates cannot be in the past (`[FutureOrTodayDate]`).
     - **Duplicate Booking Prevention**: Restricts two patients from booking the same doctor for the same date and time slot.
     - **Digital Booking Confirmation**: Displays immediate printable booking receipt with unique booking ID (`#MED-XXXX`).
   - **My Appointments Management**:
     - Lookup appointments by patient email.
     - View appointment history and live status (*Pending*, *Confirmed*, *Cancelled*).
     - Cancel upcoming appointments with interactive confirmation dialog.
     - Prevents cancellation of past consultations.

2. **Admin / Clinic Management**:
   - Protected by `[Authorize(Roles = "Admin")]`.
   - **Clinic Dashboard**: Live summary statistics (Total Doctors, Total Appointments, Pending, Confirmed, Cancelled).
   - **Appointment Controls**: Filter appointments by status (*All*, *Pending*, *Confirmed*, *Cancelled*), confirm or cancel appointments.
   - **Doctor Management**:
     - Add new doctors with validation.
     - Edit doctor profiles, fees, and available days.
     - Toggle doctor availability (*Active* / *Inactive*).
     - **Safe Integrity Deletion**: Deletion is blocked if active upcoming appointments exist. If past appointment history exists, the doctor is safely deactivated instead of deleting history, maintaining foreign key integrity.

3. **Database & Security**:
   - Multi-provider EF Core support (SQL Server LocalDB & SQLite).
   - ASP.NET Core Identity with password hashing (PBKDF2) and role-based authorization.
   - Anti-forgery CSRF tokens (`@Html.AntiForgeryToken()`) on all state-altering requests.
   - Server-side and client-side model validation.

---

## 🚀 Quick Start Instructions (VS Code Terminal)

### 1. Run the Project
Open a terminal in the project directory (`d:\doctor_appointment`) and execute:

```powershell
dotnet run --launch-profile http
```

Open your browser and navigate to:
👉 **`http://localhost:5138`**

### 2. Run Automated Verification Test
You can run the full automated integration test suite anytime:

```powershell
powershell -ExecutionPolicy Bypass -File .\test_medicare.ps1
```

---

## 🔐 Demo Credentials

| Role | Email | Password |
|---|---|---|
| **Administrator** | `admin@medicare.com` | `Admin@123MediCare!` |
| **Demo Patient 1** | `ananya.iyer@example.com` | *(Look up in My Appointments)* |
| **Demo Patient 2** | `rohan.das@example.com` | *(Look up in My Appointments)* |

> 💡 *Tip: The Login page includes a one-click button labeled **"Fill Admin Credentials"** for fast demonstration.*

---

## 🗄️ Database Configuration & Provider Switching

MediCare supports **SQL Server LocalDB** out of the box as requested, and also includes portable **SQLite** support.

In `appsettings.json`:

```json
{
  "DatabaseProvider": "Sqlite", // Switch to "SqlServer" when running on a machine with LocalDB installed
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MediCareDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True",
    "SqliteConnection": "Data Source=medicare.db"
  }
}
```

### To apply migrations in SQL Server LocalDB:
```powershell
dotnet dotnet-ef database update
```

---

## 🎬 College Viva & Teacher Demonstration Script

### Step 1: Homepage & UI (1 min)
1. Open `http://localhost:5138`.
2. Point out the medical color theme (white background with hospital blue `#0d6efd` and teal `#0d9488` accents).
3. Show the hero heading **"Your Health, Our Priority"**, quick statistics, and the 5 seeded specialists (Dr. Rahul Sharma, Dr. Priya Patel, Dr. Amit Verma, Dr. Neha Shah, Dr. Karan Mehta).
4. Explain the **"How It Works"** 3-step section and the professional healthcare footer.

### Step 2: Doctor Search & Filtering (1 min)
1. Click **"Doctors"** in the top navigation.
2. Type **"Cardio"** in the search bar and press **Filter** &rarr; Shows only *Dr. Rahul Sharma*.
3. Select **"Dermatologist"** from the dropdown &rarr; Shows *Dr. Priya Patel*.
4. Click **"Book Appointment"** on any doctor card &rarr; Automatically navigates to the booking form with that doctor pre-selected!

### Step 3: Appointment Booking & Validations (2 mins)
1. **Past Date Test**: Select a date in the past (e.g. yesterday) and attempt booking &rarr; Demonstrates server error: *"Appointment date cannot be in the past."*
2. **Duplicate Slot Collision Test**: Select *Dr. Rahul Sharma*, date = tomorrow, slot = `10:00 AM` &rarr; Demonstrates error: *"Dr. Rahul Sharma is already booked for this slot. Please choose another time slot."*
3. **Successful Booking**: Fill with your name, phone `9876543210`, email `myname@example.com`, select tomorrow at `04:00 PM` &rarr; Submits and presents the **Printable Digital Confirmation Receipt** with booking ID `#MED-XXXX`.

### Step 4: My Appointments & Cancellation (1.5 mins)
1. Click **"My Appointments"** in the navbar.
2. Enter `ananya.iyer@example.com` or `myname@example.com`.
3. Click **"View Appointments"** &rarr; Table displays appointment history with status badges (*Pending*, *Confirmed*).
4. Click **"Cancel"** &rarr; Confirmation modal appears. Click **"Yes, Cancel Appointment"** &rarr; Status updates to **Cancelled** in the database.

### Step 5: Admin Control Center (2 mins)
1. Click **"Admin Portal"** or navigate to `/Admin` without logging in &rarr; System enforces security and redirects to Login.
2. Click **"Fill Admin Credentials"** and click **Sign In**.
3. View the **Admin Dashboard**: Live counter cards (Total Doctors, Pending, Confirmed).
4. Demonstrate updating an appointment status from *Pending* to *Confirmed*.
5. Go to **"Manage Doctors"** &rarr; Click **"Add New Doctor"**, add *Dr. Sunita Rao (Neurologist)* and save.
6. Demonstrate **Safe Deletion Policy**: Attempting to delete a doctor with active appointments is blocked to protect patient records.

---

## 📚 Viva Voce Technical Explanation Guide

### 1. Model-View-Controller (MVC) Pattern
- **Models** (`Doctor.cs`, `Appointment.cs`, `ApplicationUser.cs`): Define business data entities, validation attributes (`[Required]`, `[EmailAddress]`, `[RegularExpression]`, `[FutureOrTodayDate]`), and database constraints.
- **Views** (`Razor .cshtml`): Render user interfaces using Bootstrap 5, tag helpers (`asp-controller`, `asp-action`, `asp-for`), and responsive styling.
- **Controllers** (`HomeController`, `DoctorsController`, `AppointmentsController`, `AdminController`, `AccountController`): Handle HTTP requests, execute business logic, interact with the database via EF Core, and return views or redirects.

### 2. Entity Framework Core & Relational Design
- `ApplicationDbContext` inherits from `IdentityDbContext<ApplicationUser>`, managing both application entities and Identity security tables.
- **Relationships**:
  - `Doctor` (1) &mdash;&gt; `Appointment` (Many).
  - Configured with `OnDelete(DeleteBehavior.Restrict)` to prevent accidental cascade deletions breaking historical medical records.
- **Indexes**:
  - Composite index on `(DoctorId, AppointmentDate, AppointmentTime)` for rapid slot availability queries.
  - Index on `PatientEmail` for instant appointment lookup.

### 3. Duplicate Booking Protection
Booking collision prevention is enforced at the controller level:
```csharp
var isSlotTaken = await _context.Appointments
    .AnyAsync(a => a.DoctorId == model.DoctorId 
                && a.AppointmentDate.Date == model.AppointmentDate.Date 
                && a.AppointmentTime == model.AppointmentTime 
                && a.Status != "Cancelled");
```
If `isSlotTaken` is true, the server rejects the submission and returns a friendly error message prompting the patient to choose another slot.
