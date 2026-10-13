<#
.SYNOPSIS
    Kiem tra toan dien trang thai hoat dong (Health Check) cua MDMSv2 tren Windows Server 2019.
#>

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "    KIEM TRA HOAT DONG TOAN BO HE THONG (HEALTH CHECK)    " -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

function Test-Port {
    param([string]$TargetHost = "127.0.0.1", [int]$Port, [string]$ServiceName)
    try {
        $tcp = New-Object System.Net.Sockets.TcpClient
        $tcp.ConnectAsync($TargetHost, $Port).Wait(2000) | Out-Null
        if ($tcp.Connected) {
            $tcp.Close()
            Write-Host "[PASS] $ServiceName dang lang nghe tren cong $Port" -ForegroundColor Green
            return $true
        }
    } catch {}
    Write-Host "[FAIL] $ServiceName KHONG phan hoi tren cong $Port!" -ForegroundColor Red
    return $false
}

function Test-Http {
    param([string]$Url, [string]$Description)
    try {
        $response = Invoke-WebRequest -Uri $Url -Method Get -TimeoutSec 3 -UseBasicParsing
        if ($response.StatusCode -eq 200 -or $response.StatusCode -eq 302) {
            Write-Host "[PASS] ${Description} - $Url (HTTP $($response.StatusCode))" -ForegroundColor Green
            return $true
        }
    } catch {
        Write-Host "[WARN] ${Description} - Khong the ket noi $Url ($($_.Exception.Message))" -ForegroundColor Yellow
        return $false
    }
}

Write-Host "`n1. Kiem tra cac cong dich vu cuc bo (Local Ports):" -ForegroundColor Yellow
Test-Port -Port 3306 -ServiceName "MySQL Database"
Test-Port -Port 39320 -ServiceName "Kepware IoT Gateway REST Server"
Test-Port -Port 19089 -ServiceName "MDMSv2 Backend WebApi (.NET 8)"
Test-Port -Port 80 -ServiceName "MDMSv2 Frontend (IIS Web Site)"

Write-Host "`n2. Kiem tra cac tien trinh va dich vu Windows:" -ForegroundColor Yellow

# Kiem tra Scheduled Task MDMSv2_BackendService
$task = Get-ScheduledTask -TaskName "MDMSv2_BackendService" -ErrorAction SilentlyContinue
if ($task) {
    if ($task.State -eq "Running") {
        Write-Host "[PASS] Scheduled Task 'MDMSv2_BackendService' dang CHAY (Running)." -ForegroundColor Green
    } else {
        Write-Host "[INFO] Scheduled Task 'MDMSv2_BackendService' trang thai: $($task.State)." -ForegroundColor Cyan
    }
}

# Kiem tra tien trinh WebApi
$proc = Get-Process -Name "WebApi" -ErrorAction SilentlyContinue
if ($proc) {
    Write-Host "[PASS] Tien trinh 'WebApi.exe' dang chay (PID: $($proc.Id))." -ForegroundColor Green
}

# Kiem tra Windows Services
$services = @("MySQL80", "KEPServerEXV6")
foreach ($s in $services) {
    $svc = Get-Service -Name $s -ErrorAction SilentlyContinue
    if ($svc) {
        if ($svc.Status -eq "Running") {
            Write-Host "[PASS] Service '$s' dang CHAY (Running)." -ForegroundColor Green
        } else {
            Write-Host "[WARN] Service '$s' dang o trang thai: $($svc.Status)." -ForegroundColor Yellow
        }
    } else {
        Write-Host "[INFO] Khong tim thay ten service mac dinh '$s' (co the duoc dat ten khac)." -ForegroundColor Gray
    }
}

Write-Host "`n3. Kiem tra phan hoi HTTP API & Giao dien:" -ForegroundColor Yellow
Test-Http -Url "http://127.0.0.1:19089/" -Description "Backend Welcome Endpoint"
Test-Http -Url "http://127.0.0.1:19089/swagger/index.html" -Description "Swagger API Documentation"
Test-Http -Url "http://127.0.0.1:19089/dashboard" -Description "Hangfire Background Jobs Dashboard"
Test-Http -Url "http://127.0.0.1:80/" -Description "Frontend Web Portal"

Write-Host "`n==========================================================" -ForegroundColor Cyan
Write-Host "                KET THUC KIEM TRA HE THONG                " -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan
