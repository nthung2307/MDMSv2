<#
.SYNOPSIS
    Kịch bản tự động cập nhật Hotfix 1 (Smart Lookback) & Hotfix 2 (Email Job Queue) trên máy ảo MDMSv2.
.DESCRIPTION
    Script thực hiện:
    1. Dừng tiến trình WebApi.exe và Background Task MDMSv2_BackendService trên máy ảo.
    2. Cập nhật mã nguồn biên dịch mới (Backend & Frontend) vào C:\MDMSv2.
    3. Bảo toàn nguyên vẹn file cấu hình kết nối database (appsettings.Production.json).
    4. Khởi động lại dịch vụ Backend và IIS Web Server.
    5. Kiểm tra tính sẵn sàng của hệ thống (Cổng 19089 và Cổng 80).
#>

#Requires -RunAsAdministrator
[CmdletBinding()]
param (
    [string]$TargetBackendDir = "C:\MDMSv2\Backend",
    [string]$TargetFrontendDir = "C:\MDMSv2\Frontend",
    [string]$SourcePackageDir = $PSScriptRoot
)

$ErrorActionPreference = "Stop"

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host " [MDMSv2] NANG CAP HOTFIX 1 & HOTFIX 2 TREN MAY AO       " -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

# Xác định đường dẫn nguồn
$srcBackend = Join-Path $SourcePackageDir "Backend"
if (-not (Test-Path $srcBackend)) {
    # Nếu script nằm trong thư mục Scripts, lùi ra 1 cấp
    $srcBackend = Join-Path (Split-Path $SourcePackageDir -Parent) "Backend"
}
$srcFrontend = Join-Path $SourcePackageDir "Frontend"
if (-not (Test-Path $srcFrontend)) {
    $srcFrontend = Join-Path (Split-Path $SourcePackageDir -Parent) "Frontend"
}

if (-not (Test-Path $srcBackend)) {
    Write-Error "Không tìm thấy thư mục nguồn Backend tại: $srcBackend"
    exit 1
}

# 1. Dừng dịch vụ Backend hiện tại
Write-Host "`n[1/5] Dang dung tien trinh WebApi cu..." -ForegroundColor Yellow
try {
    Stop-ScheduledTask -TaskName "MDMSv2_BackendService" -ErrorAction SilentlyContinue | Out-Null
} catch {}

$procs = Get-Process -Name "WebApi" -ErrorAction SilentlyContinue
if ($procs) {
    $procs | Stop-Process -Force -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 2
}
Write-Host "   -> [OK] Da giai phong tien trinh WebApi." -ForegroundColor Green

# 2. Cập nhật Backend (Bảo toàn appsettings.Production.json)
Write-Host "`n[2/5] Dang cap nhat ma nguon Backend vao: $TargetBackendDir..." -ForegroundColor Yellow
if (-not (Test-Path $TargetBackendDir)) {
    New-Item -ItemType Directory -Path $TargetBackendDir -Force | Out-Null
}

# Sao lưu cấu hình hiện tại nếu có
$backupConfig = $null
$existingConfig = Join-Path $TargetBackendDir "appsettings.Production.json"
if (Test-Path $existingConfig) {
    $backupConfig = Get-Content $existingConfig -Raw -Encoding UTF8
}

# Sao chép toàn bộ file Backend mới
Copy-Item -Path "$srcBackend\*" -Destination $TargetBackendDir -Recurse -Force

# Khôi phục cấu hình database đã thiết lập
if ($backupConfig) {
    Set-Content -Path $existingConfig -Value $backupConfig -Encoding UTF8
    Write-Host "   -> [OK] Da bao toan file cau hinh appsettings.Production.json." -ForegroundColor Green
}
Write-Host "   -> [OK] Cap nhat Backend thanh cong." -ForegroundColor Green

# 3. Cập nhật Frontend
Write-Host "`n[3/5] Dang cap nhat Frontend vao: $TargetFrontendDir..." -ForegroundColor Yellow
if (Test-Path $srcFrontend) {
    if (-not (Test-Path $TargetFrontendDir)) {
        New-Item -ItemType Directory -Path $TargetFrontendDir -Force | Out-Null
    }
    Copy-Item -Path "$srcFrontend\*" -Destination $TargetFrontendDir -Recurse -Force
    Write-Host "   -> [OK] Cap nhat Frontend thanh cong." -ForegroundColor Green
} else {
    Write-Warning "Khong tim thay thu muc Frontend nguon, bo qua cap nhat Frontend."
}

# 4. Khởi động lại dịch vụ
Write-Host "`n[4/5] Dang khoi dong lai Backend va IIS..." -ForegroundColor Yellow

# Khởi động lại IIS
try {
    iisreset /restart | Out-Null
    Write-Host "   -> [OK] Da khoi dong lai IIS Web Server." -ForegroundColor Green
} catch {
    Write-Warning "Khong the khoi dong lai IIS (co the IIS chua duoc bat)."
}

# Khởi động lại Backend Service
$task = Get-ScheduledTask -TaskName "MDMSv2_BackendService" -ErrorAction SilentlyContinue
if ($task) {
    Start-ScheduledTask -TaskName "MDMSv2_BackendService"
    Write-Host "   -> [OK] Da kich hoat Scheduled Task MDMSv2_BackendService." -ForegroundColor Green
} else {
    Write-Host "   -> Khoi dong WebApi.exe truc tiep trong tien trinh rieng..." -ForegroundColor Yellow
    Start-Process -FilePath (Join-Path $TargetBackendDir "WebApi.exe") -WorkingDirectory $TargetBackendDir
}

# 5. Kiểm tra lắng nghe cổng 19089
Write-Host "`n[5/5] Dang kiem tra ket noi toi Backend (Cong 19089)..." -ForegroundColor Yellow
$maxRetries = 15
$connected = $false
for ($i = 1; $i -le $maxRetries; $i++) {
    Start-Sleep -Seconds 2
    $tcp = New-Object System.Net.Sockets.TcpClient
    try {
        $tcp.ConnectAsync("127.0.0.1", 19089).Wait(1000) | Out-Null
        if ($tcp.Connected) {
            $connected = $true
            $tcp.Close()
            break
        }
    } catch {}
    Write-Host "   Dang cho Backend san sang... ($i/$maxRetries)" -ForegroundColor Gray
}

if ($connected) {
    Write-Host "   -> [THANH CONG] Backend WebApi da san sang tai http://127.0.0.1:19089" -ForegroundColor Green
} else {
    Write-Warning "Backend chua phan hoi tren cong 19089 sau 30 giay. Vui long kiem tra file log tai $TargetBackendDir\Logs"
}

Write-Host "`n==========================================================" -ForegroundColor Cyan
Write-Host "   HOAN TAT NANG CAP HOTFIX! CAC BUOC KIEM THU TIEP THEO: " -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "1. Mo Hangfire Dashboard: http://localhost:19089/hangfire" -ForegroundColor White
Write-Host "   -> Vao 'Recurring Jobs' -> Tim 'GenerateEnergyReportDaily' -> Bam 'Trigger now'" -ForegroundColor Gray
Write-Host "2. Kiem tra MySQL Database (0 NULL):" -ForegroundColor White
Write-Host "   SELECT COUNT(*) FROM amr.measurementpointdailyenergyreports WHERE ChiSoTichLuyDienNangTacDungAPTongKenhGiao IS NULL;" -ForegroundColor Gray
Write-Host "3. Mo Frontend: http://localhost" -ForegroundColor White
Write-Host "   -> Dang nhap admin / Admin@123 -> Vao 'Quan ly phieu bao' -> Bam 'Gui tat ca phieu bao'" -ForegroundColor Gray
Write-Host "==========================================================`n" -ForegroundColor Cyan
