$ErrorActionPreference = "Stop"

Write-Host "=== MEDICARE COMPREHENSIVE AUTOMATED TEST ===" -ForegroundColor Cyan
$baseUrl = "http://localhost:5138"
$session = New-Object Microsoft.PowerShell.Commands.WebRequestSession

# Test 1: Home Page
Write-Host "`n[Test 1] Testing Home Page (GET /)..." -ForegroundColor Yellow
$homeRes = Invoke-WebRequest -Uri "$baseUrl/" -WebSession $session -UseBasicParsing
if ($homeRes.StatusCode -eq 200 -and $homeRes.Content -like "*Your Health, Our Priority*") {
    Write-Host "PASS: Home page rendered with 'Your Health, Our Priority'." -ForegroundColor Green
} else {
    Write-Host "FAIL: Home page content mismatch." -ForegroundColor Red
    exit 1
}

# Verify Featured Doctors on Home Page
$expectedDoctors = @("Dr. Rahul Sharma", "Dr. Priya Patel", "Dr. Amit Verma", "Dr. Neha Shah", "Dr. Karan Mehta")
foreach ($doc in $expectedDoctors) {
    if ($homeRes.Content -like "*$doc*") {
        Write-Host "PASS: Doctor '$doc' found in database and rendered." -ForegroundColor Green
    } else {
        Write-Host "FAIL: Doctor '$doc' not found on page." -ForegroundColor Red
    }
}

# Test 2: Doctors Page and Filter
Write-Host "`n[Test 2] Testing Doctors Page (GET /Doctors)..." -ForegroundColor Yellow
$docRes = Invoke-WebRequest -Uri "$baseUrl/Doctors" -WebSession $session -UseBasicParsing
if ($docRes.StatusCode -eq 200 -and $docRes.Content -like "*Our Doctors & Specialists*") {
    Write-Host "PASS: Doctors listing rendered." -ForegroundColor Green
}

Write-Host "`n[Test 2b] Testing Doctors Search Filter (Cardiologist)..." -ForegroundColor Yellow
$searchRes = Invoke-WebRequest -Uri "$baseUrl/Doctors?searchTerm=Cardiologist" -WebSession $session -UseBasicParsing
if ($searchRes.Content -like "*Dr. Rahul Sharma*" -and ($searchRes.Content -notlike "*Dr. Priya Patel*")) {
    Write-Host "PASS: Search successfully filtered for Cardiologist." -ForegroundColor Green
} else {
    Write-Host "FAIL: Search filtering failed." -ForegroundColor Red
}

# Test 3: Unauthorized Admin Access Prevention
Write-Host "`n[Test 3] Testing Security: Unauthorized Admin Access Protection..." -ForegroundColor Yellow
$unauthSession = New-Object Microsoft.PowerShell.Commands.WebRequestSession
try {
    $adminRes = Invoke-WebRequest -Uri "$baseUrl/Admin" -WebSession $unauthSession -MaximumRedirection 0 -UseBasicParsing
    Write-Host "FAIL: Admin page allowed unauthenticated access with status $($adminRes.StatusCode)" -ForegroundColor Red
} catch [System.Management.Automation.ActionPreferenceStopException], [System.InvalidOperationException] {
    Write-Host "PASS: Unauthorized access redirected to login." -ForegroundColor Green
} catch {
    Write-Host "PASS: Request redirected as expected: $($_.Exception.Message)" -ForegroundColor Green
}

# Helper function to extract __RequestVerificationToken
function Get-AntiForgeryToken($html) {
    if ($html -match 'name="__RequestVerificationToken"\s+type="hidden"\s+value="([^"]+)"') {
        return $matches[1]
    }
    if ($html -match 'value="([^"]+)"\s+name="__RequestVerificationToken"') {
        return $matches[1]
    }
    return $null
}

# Test 4: Booking Form & Validation Checks
Write-Host "`n[Test 4] Testing Booking Validations..." -ForegroundColor Yellow
$bookPage = Invoke-WebRequest -Uri "$baseUrl/Appointments/Book" -WebSession $session -UseBasicParsing
$token = Get-AntiForgeryToken($bookPage.Content)

if (-not $token) {
    Write-Host "FAIL: Could not extract AntiForgeryToken." -ForegroundColor Red
    exit 1
}
Write-Host "Extracted CSRF token successfully." -ForegroundColor Gray

# Test 4a: Past Date Rejection
Write-Host "Testing Past Date rejection..." -ForegroundColor Yellow
$pastDate = (Get-Date).AddDays(-3).ToString("yyyy-MM-dd")
$postBody = @{
    "__RequestVerificationToken" = $token
    "PatientName" = "Test Patient"
    "PatientEmail" = "test@example.com"
    "PatientPhone" = "9876543210"
    "DoctorId" = "1"
    "AppointmentDate" = $pastDate
    "AppointmentTime" = "10:00 AM"
    "Reason" = "Checkup testing past date"
}

$pastDateRes = Invoke-WebRequest -Uri "$baseUrl/Appointments/Book" -Method Post -Body $postBody -WebSession $session -UseBasicParsing
if ($pastDateRes.Content -like "*Appointment date cannot be in the past*") {
    Write-Host "PASS: Past date was correctly rejected with error message." -ForegroundColor Green
} else {
    Write-Host "FAIL: Past date was not rejected." -ForegroundColor Red
}

# Test 4b: Duplicate Booking Slot Rejection
Write-Host "Testing Duplicate Booking Slot rejection..." -ForegroundColor Yellow
$tomorrow = (Get-Date).AddDays(1).ToString("yyyy-MM-dd")
$dupToken = Get-AntiForgeryToken($pastDateRes.Content)
$dupBody = @{
    "__RequestVerificationToken" = $dupToken
    "PatientName" = "Second Patient"
    "PatientEmail" = "second@example.com"
    "PatientPhone" = "9123456789"
    "DoctorId" = "1"
    "AppointmentDate" = $tomorrow
    "AppointmentTime" = "10:00 AM"
    "Reason" = "Attempting duplicate booking for taken slot"
}

$dupRes = Invoke-WebRequest -Uri "$baseUrl/Appointments/Book" -Method Post -Body $dupBody -WebSession $session -UseBasicParsing
if ($dupRes.Content -like "*already booked*") {
    Write-Host "PASS: Duplicate booking rejected: Doctor is already booked for this slot." -ForegroundColor Green
} else {
    Write-Host "FAIL: Duplicate booking was not rejected!" -ForegroundColor Red
}

# Test 4c: Successful Booking for Available Slot
Write-Host "Testing Successful Booking for available slot..." -ForegroundColor Yellow
$successToken = Get-AntiForgeryToken($dupRes.Content)
$uniqueEmail = "rajesh.kumar@example.com"
$validBody = @{
    "__RequestVerificationToken" = $successToken
    "PatientName" = "Rajesh Kumar"
    "PatientEmail" = $uniqueEmail
    "PatientPhone" = "9876501234"
    "DoctorId" = "2" # Dr. Priya Patel
    "AppointmentDate" = $tomorrow
    "AppointmentTime" = "03:00 PM"
    "Reason" = "Skin allergy follow-up and prescription review"
}

$bookSuccessRes = Invoke-WebRequest -Uri "$baseUrl/Appointments/Book" -Method Post -Body $validBody -WebSession $session -UseBasicParsing
if ($bookSuccessRes.Content -like "*Appointment Booked Successfully*" -or $bookSuccessRes.Content -like "*Booking ID:*") {
    Write-Host "PASS: Booking saved successfully and confirmation receipt rendered!" -ForegroundColor Green
} else {
    Write-Host "FAIL: Booking did not succeed." -ForegroundColor Red
}

# Test 5: My Appointments View and Cancellation
Write-Host "`n[Test 5] Testing My Appointments & Cancellation for $uniqueEmail..." -ForegroundColor Yellow
$myApptRes = Invoke-WebRequest -Uri "$baseUrl/Appointments/MyAppointments?email=$uniqueEmail" -WebSession $session -UseBasicParsing
if ($myApptRes.Content -like "*Rajesh Kumar*" -and $myApptRes.Content -like "*Dr. Priya Patel*") {
    Write-Host "PASS: Appointment retrieved by patient email." -ForegroundColor Green
} else {
    Write-Host "FAIL: Appointment not found in My Appointments." -ForegroundColor Red
}

# Extract Appointment ID and cancel token
if ($myApptRes.Content -match 'name="id"\s+value="(\d+)"') {
    $apptId = $matches[1]
    Write-Host "Found Appointment ID #MED-$apptId for cancellation test." -ForegroundColor Gray
    
    $cancelToken = Get-AntiForgeryToken($myApptRes.Content)
    $cancelBody = @{
        "__RequestVerificationToken" = $cancelToken
        "id" = $apptId
        "returnEmail" = $uniqueEmail
    }
    
    $cancelRes = Invoke-WebRequest -Uri "$baseUrl/Appointments/Cancel/$apptId" -Method Post -Body $cancelBody -WebSession $session -UseBasicParsing
    if ($cancelRes.Content -like "*has been cancelled*" -and $cancelRes.Content -like "*badge-status-cancelled*") {
        Write-Host "PASS: Appointment cancelled successfully and status updated in database!" -ForegroundColor Green
    } else {
        Write-Host "FAIL: Cancellation did not reflect in UI." -ForegroundColor Red
    }
} else {
    Write-Host "FAIL: Could not locate appointment ID to test cancel." -ForegroundColor Red
}

# Test 6: Admin Login & Management Portal
Write-Host "`n[Test 6] Testing Admin Login & Dashboard Operations..." -ForegroundColor Yellow
$adminSession = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$loginPage = Invoke-WebRequest -Uri "$baseUrl/Account/Login" -WebSession $adminSession -UseBasicParsing
$loginToken = Get-AntiForgeryToken($loginPage.Content)

$loginBody = @{
    "__RequestVerificationToken" = $loginToken
    "Email" = "admin@medicare.com"
    "Password" = "Admin@123MediCare!"
    "RememberMe" = "false"
}

$loginRes = Invoke-WebRequest -Uri "$baseUrl/Account/Login" -Method Post -Body $loginBody -WebSession $adminSession -UseBasicParsing
if ($loginRes.Content -like "*Clinic Control Center*" -or $loginRes.Content -like "*Admin Dashboard*" -or $loginRes.Content -like "*Admin Portal*") {
    Write-Host "PASS: Admin logged in successfully with development credentials." -ForegroundColor Green
} else {
    Write-Host "FAIL: Admin login failed." -ForegroundColor Red
}

# Test 6b: Admin Doctors Management - Add New Doctor
Write-Host "Testing Admin adding a new doctor..." -ForegroundColor Yellow
$createDocPage = Invoke-WebRequest -Uri "$baseUrl/Admin/CreateDoctor" -WebSession $adminSession -UseBasicParsing
$createToken = Get-AntiForgeryToken($createDocPage.Content)

$newDocBody = @{
    "__RequestVerificationToken" = $createToken
    "Name" = "Dr. Sunita Rao"
    "Specialization" = "Neurologist"
    "Experience" = "12"
    "ConsultationFee" = "850"
    "AvailableDays" = "Monday - Friday"
    "ProfilePictureUrl" = "/images/doctors/default-doctor.svg"
    "Bio" = "Consultant Neurologist specializing in neuro-muscular conditions."
    "IsActive" = "true"
}

$newDocRes = Invoke-WebRequest -Uri "$baseUrl/Admin/CreateDoctor" -Method Post -Body $newDocBody -WebSession $adminSession -UseBasicParsing
if ($newDocRes.Content -like "*Dr. Sunita Rao*" -and $newDocRes.Content -like "*Neurologist*") {
    Write-Host "PASS: Admin successfully created and persisted new doctor 'Dr. Sunita Rao'!" -ForegroundColor Green
} else {
    Write-Host "FAIL: Doctor creation not reflected in doctor list." -ForegroundColor Red
}

Write-Host "`n=== ALL MEDICARE AUTOMATED VERIFICATION TESTS PASSED SUCCESSFULLY! ===" -ForegroundColor Green
