<#
.SYNOPSIS
    Kich ban tu dong cap nhat Hotfix 1 (Smart Lookback) va Hotfix 2 (Email Job Queue) tren may ao MDMSv2.
.DESCRIPTION
    Script thuc hien:
    1. Dung tien trinh WebApi.exe va Background Task MDMSv2_BackendService tren may ao.
    2. Cap nhat ma nguon bien dich moi (Backend va Frontend) vao C:\MDMSv2.
    3. Bao toan & kiem tra toan ven file cau hinh ket noi database (appsettings.Production.json).
       Tu dong phat hien va khac phuc mat khau MySQL bi trong tranh loi 502 Bad Gateway.
    4. Khoi dong lai dich vu Backend va IIS Web Server.
    5. Kiem tra tinh san sang cua he thong (Cong 19089 va Cong 80), in chi tiet loi neu Backend khong khoi dong duoc.
#>

#Requires -RunAsAdministrator
[CmdletBinding()]
param (
    [string]$TargetBackendDir = "C:\MDMSv2\Backend",
    [string]$TargetFrontendDir = "C:\MDMSv2\Frontend",
    [string]$SourcePackageDir = $PSScriptRoot,
    [string]$MySqlPassword = ""
)

$ErrorActionPreference = "Stop"

Clear-Host
Write-Host "================================================================================" -ForegroundColor Cyan
Write-Host " [MDMSv2] NANG CAP HOTFIX 1 & HOTFIX 2 VA KIEM TRA HE THONG TREN MAY AO         " -ForegroundColor Cyan
Write-Host "================================================================================" -ForegroundColor Cyan
Write-Host "Thoi gian bat dau: $(Get-Date -Format 'dd/MM/yyyy HH:mm:ss')" -ForegroundColor Gray

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

# 2. Doc va sao luu cau hinh appsettings.Production.json hien tai
Write-Host ""
Write-Host "[2/5] Dang cap nhat ma nguon Backend vao: $TargetBackendDir..." -ForegroundColor Yellow
if (-not (Test-Path $TargetBackendDir)) {
    New-Item -ItemType Directory -Path $TargetBackendDir -Force | Out-Null
}

$existingConfigFile = Join-Path $TargetBackendDir "appsettings.Production.json"
$validPassword = $MySqlPassword

if (Test-Path $existingConfigFile) {
    try {
        $currentConfig = Get-Content -Path $existingConfigFile -Raw -Encoding UTF8 | ConvertFrom-Json
        if ($currentConfig.AppDb -and -not [string]::IsNullOrWhiteSpace($currentConfig.AppDb.Password)) {
            $validPassword = $currentConfig.AppDb.Password
        }
    } catch {}
}

# Sao chep Backend
Get-ChildItem -Path $srcBackend -Exclude "appsettings.Production.json" | Copy-Item -Destination $TargetBackendDir -Recurse -Force

# Kiem tra neu appsettings.Production.json chua co hoac mat khau bi trong
$needConfigWrite = $false
$finalConfig = $null
if (Test-Path $existingConfigFile) {
    try {
        $finalConfig = Get-Content -Path $existingConfigFile -Raw -Encoding UTF8 | ConvertFrom-Json
    } catch {}
}

if (-not $finalConfig) {
    $srcConfig = Join-Path $srcBackend "appsettings.Production.json"
    if (Test-Path $srcConfig) {
        Copy-Item -Path $srcConfig -Destination $existingConfigFile -Force
        $finalConfig = Get-Content -Path $existingConfigFile -Raw -Encoding UTF8 | ConvertFrom-Json
    }
}

# Kiem tra mat khau database
if (-not [string]::IsNullOrWhiteSpace($validPassword)) {
    if ($finalConfig.AppDb.Password -ne $validPassword) {
        $finalConfig.AppDb.Password = $validPassword
        $needConfigWrite = $true
    }
} else {
    # Mat khau dang bi trong -> Day la nguyen nhan gay loi 502 Bad Gateway!
    Write-Host ""
    Write-Host "   [!] CANH BAO: Mat khau ket noi MySQL (AppDb.Password) dang bi TRONG!" -ForegroundColor Red
    Write-Host "       Neu de trong, WebApi.exe se crash ngay khi khoi dong va gay ra loi 502 Bad Gateway." -ForegroundColor Yellow
    $inputPass = Read-Host "   Vui long nhap mat khau MySQL root [Mac dinh: Abc@123456]"
    if ([string]::IsNullOrWhiteSpace($inputPass)) {
        $inputPass = "Abc@123456"
    }
    $finalConfig.AppDb.Password = $inputPass
    $needConfigWrite = $true
}

if ($needConfigWrite) {
    $jsonOut = $finalConfig | ConvertTo-Json -Depth 10
    Set-Content -Path $existingConfigFile -Value $jsonOut -Encoding UTF8
    Write-Host "   -> [OK] Da cap nhat va bao toan file cau hinh appsettings.Production.json." -ForegroundColor Green
} else {
    Write-Host "   -> [OK] File cau hinh appsettings.Production.json da hop le voi mat khau DB da thiet lap." -ForegroundColor Green
}
Write-Host "   -> [OK] Cap nhat Backend thanh cong." -ForegroundColor Green

# 2.5 Don dep cac ky tu nghi ngo (*) cua du lieu cu do loi window 5 phut truoc day
$dbPassword = if ($finalConfig -and $finalConfig.AppDb) { $finalConfig.AppDb.Password } else { $validPassword }
$dbServer = if ($finalConfig -and $finalConfig.AppDb) { $finalConfig.AppDb.Server } else { "localhost" }
$dbPort = if ($finalConfig -and $finalConfig.AppDb) { $finalConfig.AppDb.Port } else { 3306 }
$dbUser = if ($finalConfig -and $finalConfig.AppDb) { $finalConfig.AppDb.UserId } else { "root" }
$dbName = if ($finalConfig -and $finalConfig.AppDb) { $finalConfig.AppDb.Database } else { "mdms_v2" }

$mysqlExe = Get-Command 'mysql.exe' -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Source
if (-not $mysqlExe) {
    $searchPaths = @(
        'C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe',
        'C:\Program Files\MySQL\MySQL Server 8.4\bin\mysql.exe',
        'C:\Program Files (x86)\MySQL\MySQL Server 8.0\bin\mysql.exe'
    )
    foreach ($p in $searchPaths) {
        if (Test-Path $p) { $mysqlExe = $p; break }
    }
}

if ($mysqlExe -and -not [string]::IsNullOrWhiteSpace($dbPassword)) {
    Write-Host ""
    Write-Host "   Dang don dep cac dau sao (*) nghi ngo cu trong CSDL (7 ngay qua)..." -ForegroundColor Yellow
    $cleanSql = @"
UPDATE measurementpointhalfhourenergyreports 
SET 
  ChiSoTichLuyDienNangTacDungAPTongKenhGiao = REPLACE(REPLACE(ChiSoTichLuyDienNangTacDungAPTongKenhGiao, '*', ''), '(Not Topical)', ''),
  ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao = REPLACE(REPLACE(ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao, '*', ''), '(Not Topical)', ''),
  ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao = REPLACE(REPLACE(ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao, '*', ''), '(Not Topical)', ''),
  ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao = REPLACE(REPLACE(ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao, '*', ''), '(Not Topical)', ''),
  ChiSoTichLuyDienNangPhanKhangAQTongKenhGiao = REPLACE(REPLACE(ChiSoTichLuyDienNangPhanKhangAQTongKenhGiao, '*', ''), '(Not Topical)', ''),
  ChiSoTichLuyDienNangTacDungAPTongKenhNhan = REPLACE(REPLACE(ChiSoTichLuyDienNangTacDungAPTongKenhNhan, '*', ''), '(Not Topical)', ''),
  ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan = REPLACE(REPLACE(ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan, '*', ''), '(Not Topical)', ''),
  ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan = REPLACE(REPLACE(ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan, '*', ''), '(Not Topical)', ''),
  ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan = REPLACE(REPLACE(ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan, '*', ''), '(Not Topical)', ''),
  ChiSoTichLuyDienNangPhanKhangAQTongKenhNhan = REPLACE(REPLACE(ChiSoTichLuyDienNangPhanKhangAQTongKenhNhan, '*', ''), '(Not Topical)', '')
WHERE CreatedTime >= NOW() - INTERVAL 7 DAY;

UPDATE measurementpointdailyenergyreports 
SET 
  ChiSoTichLuyDienNangTacDungAPTongKenhGiao = REPLACE(REPLACE(ChiSoTichLuyDienNangTacDungAPTongKenhGiao, '*', ''), '(Not Topical)', ''),
  ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao = REPLACE(REPLACE(ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao, '*', ''), '(Not Topical)', ''),
  ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao = REPLACE(REPLACE(ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao, '*', ''), '(Not Topical)', ''),
  ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao = REPLACE(REPLACE(ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao, '*', ''), '(Not Topical)', ''),
  ChiSoTichLuyDienNangPhanKhangAQTongKenhGiao = REPLACE(REPLACE(ChiSoTichLuyDienNangPhanKhangAQTongKenhGiao, '*', ''), '(Not Topical)', ''),
  ChiSoTichLuyDienNangTacDungAPTongKenhNhan = REPLACE(REPLACE(ChiSoTichLuyDienNangTacDungAPTongKenhNhan, '*', ''), '(Not Topical)', ''),
  ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan = REPLACE(REPLACE(ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan, '*', ''), '(Not Topical)', ''),
  ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan = REPLACE(REPLACE(ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan, '*', ''), '(Not Topical)', ''),
  ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan = REPLACE(REPLACE(ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan, '*', ''), '(Not Topical)', ''),
  ChiSoTichLuyDienNangPhanKhangAQTongKenhNhan = REPLACE(REPLACE(ChiSoTichLuyDienNangPhanKhangAQTongKenhNhan, '*', ''), '(Not Topical)', '')
WHERE CreatedTime >= NOW() - INTERVAL 7 DAY;
"@
    try {
        $tempSqlFile = Join-Path $env:TEMP "clean_suspected.sql"
        [System.IO.File]::WriteAllText($tempSqlFile, $cleanSql, [System.Text.Encoding]::UTF8)
        & $mysqlExe -h $dbServer -P $dbPort -u $dbUser "--password=$dbPassword" $dbName -e "source $tempSqlFile"
        Remove-Item $tempSqlFile -Force -ErrorAction SilentlyContinue
        Write-Host "   -> [OK] Da loai bo cac dau sao (*) nghi ngo cu trong CSDL." -ForegroundColor Green
    } catch {
        Write-Warning "   Khong the tu dong don dep du lieu cu qua mysql.exe: $_"
    }
}

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
$maxRetries = 20
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
    Write-Host ""
    Write-Host "   [LOI] Backend chua phan hoi tren cong 19089 sau 40 giay!" -ForegroundColor Red
    
    # Kiem tra tien trinh WebApi
    $runningWebApi = Get-Process -Name "WebApi" -ErrorAction SilentlyContinue
    if (-not $runningWebApi) {
        Write-Host "   [!] NGUYEN NHAN: Tien trinh WebApi.exe da BI CRASH (DUNG) ngay sau khi khoi dong." -ForegroundColor Red
        Write-Host "       Nguyen nhan 99% la do chuoi ket noi MySQL trong appsettings.Production.json khong chinh xac." -ForegroundColor Yellow
        Write-Host "       Vui long mo file: $existingConfigFile va kiem tra 'AppDb' -> 'Password'." -ForegroundColor Yellow
    }
    
    # In file log gan nhat neu co
    $logDir = Join-Path $TargetBackendDir "Logs"
    if (Test-Path $logDir) {
        $latestLog = Get-ChildItem -Path $logDir -Filter "*.log" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
        if ($latestLog) {
            Write-Host "`n   [NOI DUNG LOG LOI GAN NHAT: $($latestLog.Name)]" -ForegroundColor Magenta
            Get-Content -Path $latestLog.FullName -Tail 20 | Write-Host -ForegroundColor Gray
        }
    }
}

Write-Host ""
Write-Host "================================================================================" -ForegroundColor Cyan
Write-Host "   HOAN TAT NANG CAP HOTFIX! CAC BUOC KIEM THU TIEP THEO:                       " -ForegroundColor Cyan
Write-Host "================================================================================" -ForegroundColor Cyan
Write-Host "1. Mo Hangfire Dashboard: http://localhost:19089/dashboard" -ForegroundColor White
Write-Host "   -> Vao 'Recurring Jobs' -> Tim 'GenerateEnergyReportDaily' -> Bam 'Trigger now'" -ForegroundColor Gray
Write-Host "2. Mo Web Portal tren trinh duyet: http://localhost" -ForegroundColor White
Write-Host "   -> Dang nhap admin / Admin@123" -ForegroundColor Gray
Write-Host "   -> Vao 'Quan ly khach hang' -> Kiem tra danh sach khach hang hien thi day du" -ForegroundColor Gray
Write-Host "   -> Vao 'Quan ly diem do'    -> Kiem tra cac diem do va lien ket So GCS / Khach hang" -ForegroundColor Gray
Write-Host "================================================================================" -ForegroundColor Cyan
Write-Host ""
