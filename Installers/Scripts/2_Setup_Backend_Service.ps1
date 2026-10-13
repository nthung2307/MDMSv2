<#
.SYNOPSIS
    Thiet lap va dang ky Backend WebApi (.NET 8) chay nen tu dong tren Windows Server 2019.
.DESCRIPTION
    Script thuc hien:
    1. Kiem tra ket noi toi MySQL Server (Cong 3306).
    2. Dong tien trinh WebApi cu va don dep dich vu cu (sc.exe / Scheduled Task).
    3. Dang ky dich vu chay ngam tu dong cung Windows (Background Task duoi quyen SYSTEM)
       tranh loi handshake Win32 Service Control Manager (sc.exe 1053).
    4. Cau hinh Windows Firewall mo cong 19089.
    5. Khoi dong va kiem tra phan hoi cong 19089 (vong lap 12 lan).
#>

#Requires -RunAsAdministrator
[CmdletBinding()]
param (
    [string]$BackendPath = "C:\MDMSv2\Backend",
    [string]$ServiceName = "MDMSv2_BackendService",
    [int]$ListeningPort = 19089
)

$ErrorActionPreference = "Stop"

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host " [BUOC 2] THIET LAP DICH VU CHAY NEN BACKEND (.NET 8)     " -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

$exePath = Join-Path $BackendPath "WebApi.exe"

if (-not (Test-Path $exePath)) {
    Write-Error "Khong tim thay file WebApi.exe tai: $exePath. Vui long kiem tra lai duong dan Backend!"
    exit 1
}

# 1. Kiem tra ket noi co so du lieu MySQL (Doc dong tu appsettings.Production.json neu co)
$configPath = Join-Path $BackendPath "appsettings.Production.json"
$mysqlHost = "127.0.0.1"
$mysqlPort = 3306
if (Test-Path $configPath) {
    try {
        $cfg = Get-Content $configPath -Raw -Encoding UTF8 | ConvertFrom-Json
        if ($cfg.AppDb.Server) { $mysqlHost = $cfg.AppDb.Server }
        if ($cfg.AppDb.Port) { $mysqlPort = [int]$cfg.AppDb.Port }
    } catch {}
}

Write-Host "`n1. Dang kiem tra ket noi toi MySQL Database ($mysqlHost`:$mysqlPort)..." -ForegroundColor Yellow
$tcp = New-Object System.Net.Sockets.TcpClient
$mysqlRunning = $false
try {
    $tcp.ConnectAsync($mysqlHost, $mysqlPort).Wait(2000) | Out-Null
    $mysqlRunning = $tcp.Connected
    $tcp.Close()
} catch {}

if ($mysqlRunning) {
    Write-Host "   -> [PASS] MySQL Server dang lang nghe tren cong $mysqlPort." -ForegroundColor Green
} else {
    Write-Host "   -> [CANH BAO] Khong the ket noi toi MySQL Server tren cong $mysqlPort!" -ForegroundColor Red
    Write-Host "      Backend WebApi va Hangfire bat buoc phai ket noi toi MySQL de khoi dong." -ForegroundColor Yellow
    Write-Host "      Dang thu khoi dong dich vu MySQL80..." -ForegroundColor Gray
    Start-Service -Name MySQL80 -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 2
}

# 2. Don dep tien trinh cu va dich vu cu
Write-Host "`n2. Dang don dep cac tien trinh va dich vu cu..." -ForegroundColor Yellow

# Dung tien trinh WebApi cu neu dang chay
$oldProcesses = Get-Process -Name "WebApi" -ErrorAction SilentlyContinue
if ($oldProcesses) {
    Write-Host "   Dong tien trinh WebApi.exe dang chay..." -ForegroundColor Yellow
    $oldProcesses | Stop-Process -Force -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 1
}

# Don dep Win32 Service cu neu da tung dang ky qua sc.exe
$existingWinService = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
if ($existingWinService) {
    Write-Host "   Don dep dich vu sc.exe cu ($ServiceName)..." -ForegroundColor Yellow
    if ($existingWinService.Status -eq "Running") {
        Stop-Service -Name $ServiceName -Force -ErrorAction SilentlyContinue
    }
    & sc.exe delete $ServiceName | Out-Null
    Start-Sleep -Seconds 1
}

# Don dep Task Scheduler cu neu co
$existingTask = Get-ScheduledTask -TaskName $ServiceName -ErrorAction SilentlyContinue
if ($existingTask) {
    Write-Host "   Dung va cap nhat tac vu Scheduled Task cu: $ServiceName..." -ForegroundColor Yellow
    Stop-ScheduledTask -TaskName $ServiceName -ErrorAction SilentlyContinue
    Unregister-ScheduledTask -TaskName $ServiceName -Confirm:$false | Out-Null
    Start-Sleep -Seconds 1
}

# 3. Dang ky Dich vu Chay ngam tu dong (Windows Background Task)
Write-Host "`n3. Dang dang ky dich vu chay ngam tu dong cung Windows..." -ForegroundColor Yellow

$action = New-ScheduledTaskAction `
    -Execute "$exePath" `
    -Argument "--urls=http://0.0.0.0:$ListeningPort --environment=Production" `
    -WorkingDirectory "$BackendPath"

$trigger = New-ScheduledTaskTrigger -AtStartup

$principal = New-ScheduledTaskPrincipal `
    -UserId "NT AUTHORITY\SYSTEM" `
    -LogonType ServiceAccount `
    -RunLevel Highest

$settings = New-ScheduledTaskSettingsSet `
    -AllowStartIfOnBatteries `
    -DontStopIfGoingOnBatteries `
    -ExecutionTimeLimit ([TimeSpan]::Zero) `
    -RestartCount 3 `
    -RestartInterval (New-TimeSpan -Minutes 1)

Register-ScheduledTask `
    -TaskName $ServiceName `
    -Description "Dich vu xu ly nen MDMSv2 WebApi (.NET 8) va thu thap chi so cong to Hangfire" `
    -Action $action `
    -Trigger $trigger `
    -Principal $principal `
    -Settings $settings `
    -Force | Out-Null

Write-Host "   -> [PASS] Da dang ky Scheduled Task: $ServiceName (Quyen SYSTEM, tu chay khi khoi dong)." -ForegroundColor Green

# 4. Mo cong tuong lua Windows Firewall
Write-Host "`n4. Cau hinh Windows Firewall cho cong $ListeningPort..." -ForegroundColor Yellow
$fwRule = Get-NetFirewallRule -DisplayName "MDMSv2_Backend_Port_$ListeningPort" -ErrorAction SilentlyContinue
if (-not $fwRule) {
    New-NetFirewallRule -DisplayName "MDMSv2_Backend_Port_$ListeningPort" -Direction Inbound -LocalPort $ListeningPort -Protocol TCP -Action Allow | Out-Null
    Write-Host "   -> [PASS] Da tao Inbound Firewall Rule cho cong $ListeningPort." -ForegroundColor Green
} else {
    Write-Host "   -> [PASS] Firewall Rule cho cong $ListeningPort da ton tai." -ForegroundColor Green
}

# 5. Khoi dong dich vu
Write-Host "`n5. Dang khoi dong dich vu $ServiceName..." -ForegroundColor Green
Start-ScheduledTask -TaskName $ServiceName

# 6. Kiem tra trang thai cong (Thu lap lai toi da 12 lan x 2 giay)
Write-Host "`n6. Dang kiem tra cong lang nghe $ListeningPort..." -ForegroundColor Yellow
$backendRunning = $false
for ($i = 1; $i -le 12; $i++) {
    Write-Host "   Kiem tra lan $i/12..." -NoNewline
    $tcpBackend = New-Object System.Net.Sockets.TcpClient
    try {
        $tcpBackend.ConnectAsync("127.0.0.1", $ListeningPort).Wait(1500) | Out-Null
        if ($tcpBackend.Connected) {
            $backendRunning = $true
            $tcpBackend.Close()
            Write-Host " [DA KET NOI!]" -ForegroundColor Green
            break
        }
    } catch {}
    if ($tcpBackend) { $tcpBackend.Close() }
    Write-Host " [Cho dich vu nap...]" -ForegroundColor Gray
    Start-Sleep -Seconds 2
}

if ($backendRunning) {
    # Thu goi API Root
    try {
        $testHttp = Invoke-WebRequest -Uri "http://127.0.0.1:$ListeningPort" -UseBasicParsing -TimeoutSec 3 -ErrorAction SilentlyContinue
        if ($testHttp.StatusCode -eq 200) {
            Write-Host "   -> API Endpoint phan hoi: $($testHttp.Content.Trim())" -ForegroundColor Cyan
        }
    } catch {}

    Write-Host "`n==========================================================" -ForegroundColor Green
    Write-Host "-> DICH VU BACKEND DA KHOI DONG THANH CONG TREN CONG $ListeningPort!" -ForegroundColor Green
    Write-Host "   Trang chu API      : http://localhost:$ListeningPort/" -ForegroundColor Cyan
    Write-Host "   Swagger API Docs   : http://localhost:$ListeningPort/swagger" -ForegroundColor Cyan
    Write-Host "   Hangfire Dashboard : http://localhost:$ListeningPort/dashboard" -ForegroundColor Cyan
    Write-Host "==========================================================" -ForegroundColor Green
} else {
    $proc = Get-Process -Name "WebApi" -ErrorAction SilentlyContinue
    if ($proc) {
        Write-Warning "`n-> Tien trinh WebApi dang chay (PID: $($proc.Id)) nhung chua mo cong $ListeningPort."
        Write-Host "   Co the Hangfire dang khoi tao Database hoac nap schema. Vui long doi them 10-15 giay roi kiem tra lai." -ForegroundColor Yellow
    } else {
        Write-Warning "`n-> Dich vu chua phan hoi tren cong $ListeningPort."
        Write-Host "   De xem truc tiep log loi chi tiet:" -ForegroundColor Yellow
        Write-Host "   1. Mo PowerShell tai: $BackendPath" -ForegroundColor White
        Write-Host "   2. Chay: .\WebApi.exe --environment=Production" -ForegroundColor Cyan
    }
}

Write-Host "`n* Cac lenh quan ly dich vu nen sau nay:" -ForegroundColor Gray
Write-Host "  - Khoi dong lai : Restart-ScheduledTask -TaskName $ServiceName" -ForegroundColor Gray
Write-Host "  - Dung dich vu  : Stop-ScheduledTask -TaskName $ServiceName" -ForegroundColor Gray
Write-Host "  - Xem trang thai: (Get-ScheduledTask -TaskName $ServiceName).State" -ForegroundColor Gray
Write-Host "  - Dong tien trinh: Stop-Process -Name WebApi -Force" -ForegroundColor Gray
