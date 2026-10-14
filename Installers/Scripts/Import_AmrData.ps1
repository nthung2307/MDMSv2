<#
.SYNOPSIS
    Kich ban tu dong Import du lieu van hanh thuc te amr_data.sql (1.54 GB) vao MDMSv2.
.DESCRIPTION
    1. Kiem tra va tim kiem cong cu mysql.exe.
    2. Tu dong nhan dien thong tin ket noi MySQL tu Backend/appsettings.Production.json.
    3. Toi uu hoa tham so MySQL (max_allowed_packet, net_buffer_length, innodb_flush_log_at_trx_commit).
    4. Tai khoi tao cau truc co so du lieu tu amr_full.sql (tranh xung dot Duplicate entry).
    5. Nạp toan bo du lieu thuc te 1.54 GB tu amr_data.sql.
    6. Dong bo va cap nhat mat khau tai khoan 'admin' ve 'Admin@123' (ma hoa PBKDF2).
    7. Thong ke va kiem tra tinh toan ven du lieu (Khach hang, Diem do, So GCS, Du lieu do dem).
#>

[CmdletBinding()]
param (
    [string]$MySqlHost = "",
    [int]$MySqlPort = 0,
    [string]$MySqlUser = "",
    [string]$MySqlPassword = "",
    [string]$AdminPassword = "Admin@123"
)

$ErrorActionPreference = "Stop"

Clear-Host
Write-Host "================================================================================" -ForegroundColor Cyan
Write-Host "         MDMSv2 - CONG CU IMPORT DU LIEU VAN HANH THUC TE (amr_data.sql)        " -ForegroundColor Cyan
Write-Host "================================================================================" -ForegroundColor Cyan
Write-Host "Thoi gian thuc hien: $(Get-Date -Format 'dd/MM/yyyy HH:mm:ss')" -ForegroundColor Gray

# 1. Xac dinh duong dan thu muc & tap tin SQL (Ho tro che do Portable doc lap tren May Ao)
$scriptDir = $PSScriptRoot

# Tim tap tin amr_full.sql hoac amr-full.sql
$candidateFull = @(
    (Join-Path $scriptDir "amr_full.sql"),
    (Join-Path $scriptDir "amr-full.sql"),
    (Join-Path $scriptDir "..\Database\amr_full.sql"),
    (Join-Path $scriptDir "..\..\Installers\Database\amr_full.sql"),
    "C:\MDMSv2\Database\amr_full.sql",
    "C:\MDMSv2\Installers\Database\amr_full.sql"
)
$fileFull = $null
foreach ($f in $candidateFull) {
    if (Test-Path $f) { $fileFull = (Resolve-Path $f).Path; break }
}

# Tim tap tin amr_data.sql hoac amr-data.sql
$candidateData = @(
    (Join-Path $scriptDir "amr_data.sql"),
    (Join-Path $scriptDir "amr-data.sql"),
    (Join-Path $scriptDir "..\Database\amr_data.sql"),
    (Join-Path $scriptDir "..\Database\amr-data.sql"),
    (Join-Path $scriptDir "..\..\Installers\Database\amr_data.sql"),
    (Join-Path $scriptDir "..\..\Installers\Database\amr-data.sql"),
    "C:\MDMSv2\Database\amr_data.sql",
    "C:\MDMSv2\Database\amr-data.sql"
)
$fileData = $null
foreach ($f in $candidateData) {
    if (Test-Path $f) { $fileData = (Resolve-Path $f).Path; break }
}

if (-not $fileFull) {
    Write-Host "`n[CANH BAO] Khong tim thay tap tin khung 'amr_full.sql' trong cung thu muc hoac C:\MDMSv2!" -ForegroundColor Red
    $manualFull = Read-Host "Vui long nhap duong dan day du toi amr_full.sql"
    if (Test-Path $manualFull) { $fileFull = $manualFull } else { Write-Error "Khong tim thay amr_full.sql. Dung tien trinh." }
}

if (-not $fileData) {
    Write-Host "`n[CANH BAO] Khong tim thay tap tin du lieu 'amr_data.sql' (hoac amr-data.sql)!" -ForegroundColor Red
    $manualData = Read-Host "Vui long nhap duong dan day du toi amr_data.sql"
    if (Test-Path $manualData) { $fileData = $manualData } else { Write-Error "Khong tim thay amr_data.sql. Dung tien trinh." }
}

$dataSizeMB = [math]::Round(((Get-Item $fileData).Length / 1MB), 2)
Write-Host "Phat hien tap tin DDL  : $fileFull" -ForegroundColor Green
Write-Host "Phat hien tap tin Data : $fileData ($dataSizeMB MB)" -ForegroundColor Green

# Tim thu muc Backend chua appsettings.Production.json tren may ao
$backendCandidates = @(
    (Join-Path $scriptDir "..\..\Backend\Presentation\WebAPI"),
    (Join-Path $scriptDir "..\Backend"),
    "C:\MDMSv2\Backend"
)
$backendDir = $null
foreach ($b in $backendCandidates) {
    if (Test-Path $b) { $backendDir = $b; break }
}

# 2. Tim kiem mysql.exe
function Find-MySqlExe {
    $mysqlCmd = Get-Command "mysql.exe" -ErrorAction SilentlyContinue
    if ($mysqlCmd) { return $mysqlCmd.Source }

    $searchPaths = @(
        "C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe",
        "C:\Program Files\MySQL\MySQL Server 8.4\bin\mysql.exe",
        "C:\Program Files (x86)\MySQL\MySQL Server 8.0\bin\mysql.exe"
    )
    foreach ($p in $searchPaths) {
        if (Test-Path $p) { return $p }
    }
    
    $found = Get-ChildItem -Path "C:\Program Files\MySQL" -Filter "mysql.exe" -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($found) { return $found.FullName }

    return $null
}

$mysqlExe = Find-MySqlExe
if (-not $mysqlExe) {
    Write-Host "`n[CANH BAO] Khong tim thay mysql.exe trong PATH hoac thu muc mac dinh cua MySQL Server!" -ForegroundColor Red
    $manualExe = Read-Host "Vui long nhap duong dan day du toi file mysql.exe"
    if (Test-Path $manualExe) {
        $mysqlExe = $manualExe
    } else {
        Write-Error "Duong dan mysql.exe khong hop le. Dung tien trinh."
    }
}
Write-Host "Cong cu MySQL Client   : $mysqlExe" -ForegroundColor Gray

# 3. Doc thong tin cau hinh MySQL tu appsettings.Production.json neu co
$appsettingsPath = $null
if ($backendDir -and (Test-Path $backendDir)) {
    $cand1 = Join-Path $backendDir "appsettings.Production.json"
    $cand2 = Join-Path $backendDir "appsettings.json"
    if (Test-Path $cand1) { $appsettingsPath = $cand1 }
    elseif (Test-Path $cand2) { $appsettingsPath = $cand2 }
}

if ($appsettingsPath -and (Test-Path $appsettingsPath)) {
    try {
        $config = Get-Content -Path $appsettingsPath -Raw -Encoding UTF8 | ConvertFrom-Json
        if ($config.AppDb) {
            if ([string]::IsNullOrWhiteSpace($MySqlHost)) { $MySqlHost = $config.AppDb.Server }
            if ($MySqlPort -eq 0 -and $config.AppDb.Port) { $MySqlPort = [int]$config.AppDb.Port }
            if ([string]::IsNullOrWhiteSpace($MySqlUser)) { $MySqlUser = $config.AppDb.UserName }
            if ([string]::IsNullOrWhiteSpace($MySqlPassword)) { $MySqlPassword = $config.AppDb.Password }
            Write-Host "Da tu dong nhan dien cau hinh DB tu: $(Split-Path $appsettingsPath -Leaf)" -ForegroundColor Gray
        }
    } catch {}
}

if ([string]::IsNullOrWhiteSpace($MySqlHost)) { $MySqlHost = "localhost" }
if ($MySqlPort -eq 0) { $MySqlPort = 3306 }
if ([string]::IsNullOrWhiteSpace($MySqlUser)) { $MySqlUser = "root" }

Write-Host "MySQL Host: $MySqlHost | Port: $MySqlPort | User: $MySqlUser" -ForegroundColor Cyan

while ([string]::IsNullOrWhiteSpace($MySqlPassword)) {
    $secPass = Read-Host "Vui long nhap mat khau MySQL root cho tai khoan '$MySqlUser'" -AsSecureString
    $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($secPass)
    $MySqlPassword = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
    [System.Runtime.InteropServices.Marshal]::ZeroFreeBSTR($BSTR)
}

# Ham ma hoa mat khau PBKDF2 RFC2898 tuong thich MDMSv2
function Hash-MdmsPassword {
    param([string]$password)
    $salt = New-Object byte[] 16
    $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    $rng.GetBytes($salt)
    $derive = New-Object System.Security.Cryptography.Rfc2898DeriveBytes($password, $salt, 1000)
    $subKey = $derive.GetBytes(32)
    $dst = New-Object byte[] 49
    $dst[0] = 0
    [Array]::Copy($salt, 0, $dst, 1, 16)
    [Array]::Copy($subKey, 0, $dst, 17, 32)
    return [Convert]::ToBase64String($dst)
}

# Ham thuc thi script SQL bang mysql.exe thong qua file tam bat
function Invoke-MySqlCommand {
    param(
        [string]$SqlContent,
        [string]$Description
    )
    Write-Host "   -> $Description..." -ForegroundColor Gray
    $tempSql = Join-Path $env:TEMP "mdms_exec_$([guid]::NewGuid().ToString('N')).sql"
    $tempBat = Join-Path $env:TEMP "mdms_exec_$([guid]::NewGuid().ToString('N')).bat"
    try {
        Set-Content -Path $tempSql -Value $SqlContent -Encoding UTF8
        $batCmd = "@echo off`r`n`"$mysqlExe`" -h $MySqlHost -P $MySqlPort -u $MySqlUser `"-p$MySqlPassword`" --default-character-set=utf8mb4 < `"$tempSql`""
        Set-Content -Path $tempBat -Value $batCmd -Encoding ASCII
        $p = Start-Process -FilePath $tempBat -Wait -PassThru -NoNewWindow
        if ($p.ExitCode -ne 0) {
            throw "Lenh MySQL ket thuc voi ma loi: $($p.ExitCode)"
        }
    } finally {
        Remove-Item -Path $tempSql, $tempBat -Force -ErrorAction SilentlyContinue
    }
}

# 4. BUOC 1: Toi uu hoa tham so MySQL Server
Write-Host "`n[BUOC 1/5] Toi uu tham so Server cho qua trinh nạp du lieu lon..." -ForegroundColor Yellow
$optimizeSql = @"
SET GLOBAL max_allowed_packet = 1073741824;
SET GLOBAL net_buffer_length = 1048576;
SET GLOBAL innodb_flush_log_at_trx_commit = 2;
"@
try {
    Invoke-MySqlCommand -SqlContent $optimizeSql -Description "Thiet lap max_allowed_packet = 1GB, innodb_flush_log = 2"
    Write-Host "   [PASS] Da toi uu tham so MySQL thanh cong." -ForegroundColor Green
} catch {
    Write-Warning "   Khong the SET GLOBAL (co the user khong co quyen SUPER): $($_.Exception.Message)"
}

# 5. BUOC 2: Khoi tao lai Schema sach tu amr_full.sql
Write-Host "`n[BUOC 2/5] Khoi tao cau truc Database 'amr' sach tu amr_full.sql..." -ForegroundColor Yellow
$recreateDbSql = @"
DROP DATABASE IF EXISTS amr;
CREATE DATABASE amr DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci;
"@
Invoke-MySqlCommand -SqlContent $recreateDbSql -Description "Drop va Re-create database amr"

$importFullBat = Join-Path $env:TEMP "import_full.bat"
try {
    Write-Host "   -> Dang nạp DDL & Stored Procedures tu amr_full.sql..." -ForegroundColor Gray
    $batCmd = "@echo off`r`n`"$mysqlExe`" -h $MySqlHost -P $MySqlPort -u $MySqlUser `"-p$MySqlPassword`" --default-character-set=utf8mb4 amr < `"$fileFull`""
    Set-Content -Path $importFullBat -Value $batCmd -Encoding ASCII
    $p = Start-Process -FilePath $importFullBat -Wait -PassThru -NoNewWindow
    if ($p.ExitCode -ne 0) {
        throw "Import amr_full.sql that bai voi ma loi $($p.ExitCode)"
    }
    Write-Host "   [PASS] Da khoi tao thanh cong toan bo bang, views va stored procedures." -ForegroundColor Green
} finally {
    Remove-Item -Path $importFullBat -Force -ErrorAction SilentlyContinue
}

# 6. BUOC 3: Nạp du lieu thuc te 1.54 GB tu amr_data.sql
Write-Host "`n[BUOC 3/5] Dang nạp du lieu thuc te tu amr_data.sql ($dataSizeMB MB)..." -ForegroundColor Yellow
Write-Host "   (Luu y: Qua trinh nay co the mat 3 - 8 phut tuy toc do o dia. Vui long doi...)" -ForegroundColor Gray

$stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
$importDataBat = Join-Path $env:TEMP "import_data.bat"
try {
    $batCmd = "@echo off`r`n`"$mysqlExe`" -h $MySqlHost -P $MySqlPort -u $MySqlUser `"-p$MySqlPassword`" --max-allowed-packet=1073741824 --net-buffer-length=1048576 --default-character-set=utf8mb4 amr < `"$fileData`""
    Set-Content -Path $importDataBat -Value $batCmd -Encoding ASCII
    $p = Start-Process -FilePath $importDataBat -Wait -PassThru -NoNewWindow
    $stopwatch.Stop()
    
    if ($p.ExitCode -ne 0) {
        throw "Import amr_data.sql that bai voi ma loi $($p.ExitCode)"
    }
    $elapsedMinutes = [math]::Round($stopwatch.Elapsed.TotalMinutes, 2)
    Write-Host "   [PASS] Nạp xong toan bo du lieu thuc te trong $elapsedMinutes phut!" -ForegroundColor Green
} finally {
    Remove-Item -Path $importDataBat -Force -ErrorAction SilentlyContinue
}

# 7. BUOC 4: Dong bo mat khau tai khoan Admin MDMS ve AdminPassword (Admin@123)
Write-Host "`n[BUOC 4/5] Dong bo mat khau tai khoan Admin Web Portal..." -ForegroundColor Yellow
$adminHash = Hash-MdmsPassword -password $AdminPassword
$updateAdminSql = @"
USE amr;
SET SQL_SAFE_UPDATES = 0;
UPDATE users 
SET Password = '$adminHash',
    Status = 'ACTIVE',
    IsLocked = 0,
    RoleName = 'SUPERADMIN',
    ModifiedTime = NOW()
WHERE UserName = 'admin' OR Id = 921946681335811;
SET SQL_SAFE_UPDATES = 1;
"@
Invoke-MySqlCommand -SqlContent $updateAdminSql -Description "Cap nhat mat khau cho admin ve '$AdminPassword'"
Write-Host "   [PASS] Tai khoan 'admin' da duoc thiet lap mat khau: $AdminPassword" -ForegroundColor Green

# 8. BUOC 5: Khoi phuc tham so innodb_flush_log_at_trx_commit ve 1 va kiem tra du lieu
Write-Host "`n[BUOC 5/5] Khoi phuc tham so he thong va xac minh tinh toan ven du lieu..." -ForegroundColor Yellow
$restoreParamSql = @"
SET GLOBAL innodb_flush_log_at_trx_commit = 1;
"@
try {
    Invoke-MySqlCommand -SqlContent $restoreParamSql -Description "Khoi phuc innodb_flush_log_at_trx_commit = 1"
} catch {}

# Query dem so luong ban ghi de xac minh
$verifyQueryFile = Join-Path $env:TEMP "verify_query.sql"
$verifyResultFile = Join-Path $env:TEMP "verify_result.txt"
$verifySql = @"
USE amr;
SELECT 'CUSTOMERS' AS Metric, COUNT(*) AS TotalRecords FROM customers
UNION ALL
SELECT 'MEASUREMENT_POINTS', COUNT(*) FROM measurementpoints
UNION ALL
SELECT 'GCS_CYCLES', COUNT(*) FROM gcscutoffcycle
UNION ALL
SELECT 'ENERGY_REPORTS_DAILY', COUNT(*) FROM measurementpointdailyenergyreports
UNION ALL
SELECT 'ENERGY_REPORTS_HALFHOUR', COUNT(*) FROM measurementpointhalfhourenergyreports
UNION ALL
SELECT 'TELEMETRY_MONITORING', COUNT(*) FROM measurementpointmonitoring
UNION ALL
SELECT 'USERS', COUNT(*) FROM users;
"@
try {
    Set-Content -Path $verifyQueryFile -Value $verifySql -Encoding UTF8
    $batVerify = "@echo off`r`n`"$mysqlExe`" -h $MySqlHost -P $MySqlPort -u $MySqlUser `"-p$MySqlPassword`" -t < `"$verifyQueryFile`" > `"$verifyResultFile`""
    $tempBatVerify = Join-Path $env:TEMP "run_verify.bat"
    Set-Content -Path $tempBatVerify -Value $batVerify -Encoding ASCII
    $p = Start-Process -FilePath $tempBatVerify -Wait -PassThru -NoNewWindow
    Remove-Item -Path $tempBatVerify -Force -ErrorAction SilentlyContinue

    if (Test-Path $verifyResultFile) {
        Write-Host "`n=== KET QUA XAC MINH DU LIEU TRONG CO SO DU LIEU 'amr' ===" -ForegroundColor Cyan
        Get-Content $verifyResultFile | Write-Host -ForegroundColor White
    }
} finally {
    Remove-Item -Path $verifyQueryFile, $verifyResultFile -Force -ErrorAction SilentlyContinue
}

Write-Host "`n================================================================================" -ForegroundColor Green
Write-Host "                HOAN TAT IMPORT DU LIEU VAN HANH THANH CONG!                    " -ForegroundColor Green
Write-Host "================================================================================" -ForegroundColor Green
Write-Host "Ban co the dang nhap ngay vao Web Portal MDMS theo thong tin:" -ForegroundColor White
Write-Host "   - URL       : http://$MySqlHost (hoac IP may ao)" -ForegroundColor Cyan
Write-Host "   - Tai khoan : admin" -ForegroundColor Cyan
Write-Host "   - Mat khau  : $AdminPassword" -ForegroundColor Cyan
Write-Host "================================================================================" -ForegroundColor Green
