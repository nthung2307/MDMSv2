<#
.SYNOPSIS
    Kich ban tu dong cap nhat Hotfix 1 (Smart Lookback) va Hotfix 2 (Email Job Queue) tren may ao MDMSv2.
.DESCRIPTION
    Script thuc hien:
    1. Dung tien trinh WebApi.exe va Background Task MDMSv2_BackendService tren may ao.
    2. Cap nhat ma nguon bien dich moi (Backend va Frontend) vao C:\MDMSv2.
    3. Bao toan nguyen ven file cau hinh ket noi database (appsettings.Production.json).
    4. Khoi dong lai dich vu Backend va IIS Web Server.
    5. Kiem tra tinh san sang cua he thong (Cong 19089 va Cong 80).
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
Write-Host " [MDMSv2] NANG CAP HOTFIX 1 VA HOTFIX 2 TREN MAY AO       " -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

# Xac dinh duong dan nguon
$srcBackend = Join-Path $SourcePackageDir "Backend"
if (-not (Test-Path $srcBackend)) {
    $parentDir = Split-Path $SourcePackageDir -Parent
    if ($parentDir) {
        $srcBackend = Join-Path $parentDir "Backend"
    }
}

$srcFrontend = Join-Path $SourcePackageDir "Frontend"
if (-not (Test-Path $srcFrontend)) {
    $parentDir = Split-Path $SourcePackageDir -Parent
    if ($parentDir) {
        $srcFrontend = Join-Path $parentDir "Frontend"
    }
}

if (-not (Test-Path $srcBackend)) {
    Write-Error "Khong tim thay thu muc nguon Backend tai: $srcBackend"
    exit 1
}

# 1. Dung dich vu Backend hien tai
Write-Host ""
Write-Host "[1/5] Dang dung tien trinh WebApi cu..." -ForegroundColor Yellow
try {
    Stop-ScheduledTask -TaskName "MDMSv2_BackendService" -ErrorAction SilentlyContinue | Out-Null
} catch {}

$procs = Get-Process -Name "WebApi" -ErrorAction SilentlyContinue
if ($procs) {
    $procs | Stop-Process -Force -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 2
}
Write-Host "   -> [OK] Da giai phong tien trinh WebApi." -ForegroundColor Green

# 2. Cap nhat Backend (Bao toan appsettings.Production.json)
Write-Host ""
Write-Host "[2/5] Dang cap nhat ma nguon Backend vao: $TargetBackendDir..." -ForegroundColor Yellow
if (-not (Test-Path $TargetBackendDir)) {
    New-Item -ItemType Directory -Path $TargetBackendDir -Force | Out-Null
}

$backupConfig = $null
$existingConfig = Join-Path $TargetBackendDir "appsettings.Production.json"
if (Test-Path $existingConfig) {
    $backupConfig = Get-Content $existingConfig -Raw -Encoding UTF8
}

Copy-Item -Path "$srcBackend\*" -Destination $TargetBackendDir -Recurse -Force

if ($backupConfig) {
    Set-Content -Path $existingConfig -Value $backupConfig -Encoding UTF8
    Write-Host "   -> [OK] Da bao toan file cau hinh appsettings.Production.json." -ForegroundColor Green
}
Write-Host "   -> [OK] Cap nhat Backend thanh cong." -ForegroundColor Green

# 3. Cap nhat Frontend
Write-Host ""
Write-Host "[3/5] Dang cap nhat Frontend vao: $TargetFrontendDir..." -ForegroundColor Yellow
if (Test-Path $srcFrontend) {
    if (-not (Test-Path $TargetFrontendDir)) {
        New-Item -ItemType Directory -Path $TargetFrontendDir -Force | Out-Null
    }
    Copy-Item -Path "$srcFrontend\*" -Destination $TargetFrontendDir -Recurse -Force
    Write-Host "   -> [OK] Cap nhat Frontend thanh cong." -ForegroundColor Green
} else {
    Write-Warning "Khong tim thay thu muc Frontend nguon, bo qua cap nhat Frontend."
}

# 4. Khoi dong lai dich vu
Write-Host ""
Write-Host "[4/5] Dang khoi dong lai Backend va IIS..." -ForegroundColor Yellow

try {
    iisreset /restart | Out-Null
    Write-Host "   -> [OK] Da khoi dong lai IIS Web Server." -ForegroundColor Green
} catch {
    Write-Warning "Khong the khoi dong lai IIS (co the IIS chua duoc bat)."
}

$task = Get-ScheduledTask -TaskName "MDMSv2_BackendService" -ErrorAction SilentlyContinue
if ($task) {
    Start-ScheduledTask -TaskName "MDMSv2_BackendService"
    Write-Host "   -> [OK] Da kich hoat Scheduled Task MDMSv2_BackendService." -ForegroundColor Green
} else {
    Write-Host "   -> Khoi dong WebApi.exe truc tiep trong tien trinh rieng..." -ForegroundColor Yellow
    Start-Process -FilePath (Join-Path $TargetBackendDir "WebApi.exe") -WorkingDirectory $TargetBackendDir
}

# 5. Kiem tra lang nghe cong 19089
Write-Host ""
Write-Host "[5/5] Dang kiem tra ket noi toi Backend (Cong 19089)..." -ForegroundColor Yellow
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

Write-Host ""
Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "   HOAN TAT NANG CAP HOTFIX! CAC BUOC KIEM THU TIEP THEO: " -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "1. Mo Hangfire Dashboard: http://localhost:19089/hangfire" -ForegroundColor White
Write-Host "   -> Vao 'Recurring Jobs' -> Tim 'GenerateEnergyReportDaily' -> Bam 'Trigger now'" -ForegroundColor Gray
Write-Host "2. Kiem tra MySQL Database (0 NULL):" -ForegroundColor White
Write-Host "   SELECT COUNT(*) FROM amr.measurementpointdailyenergyreports WHERE ChiSoTichLuyDienNangTacDungAPTongKenhGiao IS NULL;" -ForegroundColor Gray
Write-Host "3. Mo Frontend: http://localhost" -ForegroundColor White
Write-Host "   -> Dang nhap admin / Admin@123 -> Vao 'Quan ly phieu bao' -> Bam 'Gui tat ca phieu bao'" -ForegroundColor Gray
Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host ""
