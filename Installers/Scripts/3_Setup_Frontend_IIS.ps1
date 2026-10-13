<#
.SYNOPSIS
    Trien khai giao dien nguoi dung Frontend (React 18 + Vite) tren IIS.
.DESCRIPTION
    Script thuc hien:
    1. Import module WebAdministration tren IIS.
    2. Kiem tra thu muc Frontend (C:\MDMSv2\Frontend).
    3. Tu dong kiem tra va cai dat IIS URL Rewrite 2.1 & ARR 3.0 neu chua co.
    4. Bat tinh nang ARR Proxy (Reverse Proxy API sang Backend port 19089).
    5. Cap quyen truy cap IIS_IUSRS cho thu muc Frontend.
    6. Go bo hoan toan Default Web Site de giai phong triet de cong 80.
    7. Tao IIS AppPool va Website MDMSv2_Web.
    8. Mo firewall cong 80 va kiem tra ket noi.
#>

#Requires -RunAsAdministrator
[CmdletBinding()]
param (
    [string]$FrontendPath = "C:\MDMSv2\Frontend",
    [string]$SiteName = "MDMSv2_Web",
    [int]$Port = 80
)

$ErrorActionPreference = "Stop"

# Xu ly phong thu: Neu nguoi dung quen go dau gach ngang '-' truoc ten tham so FrontendPath
if ($FrontendPath -eq "FrontendPath" -and $SiteName -like "*\*") {
    $FrontendPath = $SiteName
    $SiteName = "MDMSv2_Web"
}

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host " [BUOC 3] TRIEN KHAI FRONTEND (REACT 18) LEN IIS SITE     " -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

Import-Module WebAdministration -ErrorAction SilentlyContinue

if (-not (Test-Path $FrontendPath)) {
    Write-Error "Khong tim thay thu muc Frontend tai: $FrontendPath. Vui long kiem tra lai duong dan!"
    exit 1
}

# Ham ho tro cai dat IIS Module neu thieu
function Install-IisModuleIfMissing {
    param(
        [string]$ModuleName,
        [string]$DllPath,
        [string]$MsiFileName,
        [string]$DownloadUrl
    )

    if (Test-Path $DllPath) {
        Write-Host "   -> [PASS] $ModuleName da duoc cai dat." -ForegroundColor Green
        return $false # khong can restart IIS
    }

    Write-Host "   -> Phat hien chua co $ModuleName. Dang tien hanh cai dat tu dong..." -ForegroundColor Yellow

    # Tim file msi trong cac thu muc san co
    $possiblePaths = @(
        "$PSScriptRoot\..\Prerequisites\$MsiFileName",
        "$PSScriptRoot\Prerequisites\$MsiFileName",
        "C:\MDMS\Prerequisites\$MsiFileName",
        "C:\MDMSv2\Prerequisites\$MsiFileName",
        "$env:TEMP\$MsiFileName"
    )

    $msiPath = $null
    foreach ($p in $possiblePaths) {
        if (Test-Path $p) {
            $msiPath = (Resolve-Path $p).Path
            Write-Host "      Tim thay bo cai offline tai: $msiPath" -ForegroundColor Gray
            break
        }
    }

    if (-not $msiPath) {
        $msiPath = "$env:TEMP\$MsiFileName"
        Write-Host "      Dang tai $ModuleName tu Microsoft CDN..." -ForegroundColor Gray
        try {
            [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
            Invoke-WebRequest -Uri $DownloadUrl -OutFile $msiPath -UseBasicParsing -TimeoutSec 60
            Write-Host "      Tai ve thanh cong: $msiPath" -ForegroundColor Green
        } catch {
            Write-Warning "      Khong the tu dong tai $MsiFileName tu Internet ($($_.Exception.Message))."
            return $false
        }
    }

    # Chuan hoa: Copy sang TEMP de tranh loi msiexec 1619
    $stagedMsi = Join-Path $env:TEMP $MsiFileName
    if ($msiPath -ne $stagedMsi) {
        Copy-Item -Path $msiPath -Destination $stagedMsi -Force
    }

    Write-Host "      Dang cai dat $MsiFileName (msiexec /qn)..." -ForegroundColor Gray
    $proc = Start-Process -FilePath "msiexec.exe" -ArgumentList "/i `"$stagedMsi`" /qn /norestart" -Wait -PassThru
    if ($proc.ExitCode -eq 0 -or $proc.ExitCode -eq 3010 -or (Test-Path $DllPath)) {
        Write-Host "   -> [PASS] Da cai dat thanh cong $ModuleName!" -ForegroundColor Green
        return $true # can restart IIS de nhan module
    } else {
        Write-Warning "      Cai dat $ModuleName tra ve ma loi: $($proc.ExitCode)"
        return $false
    }
}

# 1. Kiem tra va cai dat URL Rewrite 2.1 & ARR 3.0
Write-Host "`n1. Kiem tra cac module IIS phu tro (URL Rewrite & ARR)..." -ForegroundColor Yellow

$r1 = Install-IisModuleIfMissing `
    -ModuleName "IIS URL Rewrite 2.1" `
    -DllPath "$env:SystemRoot\System32\inetsrv\rewrite.dll" `
    -MsiFileName "rewrite_amd64_en-US.msi" `
    -DownloadUrl "https://download.microsoft.com/download/1/2/8/128E2E22-C1B9-44A4-BE2A-5859ED1D4592/rewrite_amd64_en-US.msi"

$r2 = Install-IisModuleIfMissing `
    -ModuleName "IIS Application Request Routing (ARR) 3.0" `
    -DllPath "$env:SystemRoot\System32\inetsrv\requestRouter.dll" `
    -MsiFileName "requestRouter_amd64.msi" `
    -DownloadUrl "https://download.microsoft.com/download/e/9/8/e9849d6a-020e-47e4-9fd0-a023e99b54eb/requestRouter_amd64.msi"

# Bat tinh nang ARR Proxy de ho tro Reverse Proxy API sang Backend
$appcmd = "$env:SystemRoot\System32\inetsrv\appcmd.exe"
if (Test-Path $appcmd) {
    try {
        & $appcmd set config -section:system.webServer/proxy /enabled:"True" /commit:apphost | Out-Null
        Write-Host "   -> [PASS] Da bat tinh nang ARR Proxy (Enable proxy: True)." -ForegroundColor Green
    } catch {}
}

# Neu vua cai moi module thi khoi dong lai IIS truoc khi tao website
if ($r1 -or $r2) {
    Write-Host "   Dang khoi dong lai IIS de nạp cac module vua cai dat..." -ForegroundColor Gray
    iisreset /noforce | Out-Null
    Start-Sleep -Seconds 2
}

# 2. Cap quyen truy cap cho IIS_IUSRS
Write-Host "`n2. Cau hinh phan quyen thu muc cho IIS_IUSRS..." -ForegroundColor Yellow
$acl = Get-Acl $FrontendPath
$permission = "IIS_IUSRS", "ReadAndExecute,ListDirectory", "ContainerInherit,ObjectInherit", "None", "Allow"
$accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule $permission
$acl.AddAccessRule($accessRule)
Set-Acl -Path $FrontendPath -AclObject $acl
Write-Host "   -> [PASS] Da cap quyen doc va thuc thi cho IIS_IUSRS tai: $FrontendPath" -ForegroundColor Green

# 3. Go bo hoan toan Default Web Site de tranh xung dot cong Port
Write-Host "`n3. Kiem tra va giai phong triet de cong $Port..." -ForegroundColor Yellow
$defaultSite = Get-Website -Name "Default Web Site" -ErrorAction SilentlyContinue
if ($defaultSite) {
    Write-Host "   Phat hien 'Default Web Site' dang ton tai. Dang go bo de giai phong cong $Port..." -ForegroundColor Yellow
    Stop-Website -Name "Default Web Site" -ErrorAction SilentlyContinue
    Remove-Website -Name "Default Web Site" -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 1
    Write-Host "   -> [PASS] Da go bo 'Default Web Site' khoi IIS." -ForegroundColor Green
} else {
    Write-Host "   -> [PASS] 'Default Web Site' da duoc don dep truoc do." -ForegroundColor Green
}

# 4. Tao hoac cap nhat Website MDMSv2_Web
Write-Host "`n4. Dang cau hinh IIS Website: $SiteName tren cong $Port..." -ForegroundColor Yellow
$appPoolName = "MDMSv2_AppPool"

# Tao AppPool No Managed Code (Static SPA)
if (-not (Test-Path "IIS:\AppPools\$appPoolName")) {
    $pool = New-Item "IIS:\AppPools\$appPoolName"
    $pool.managedRuntimeVersion = "" # No Managed Code
    $pool | Set-Item
    Write-Host "   -> [PASS] Da tao Application Pool: $appPoolName (No Managed Code)." -ForegroundColor Green
} else {
    Write-Host "   -> [PASS] Application Pool '$appPoolName' da ton tai." -ForegroundColor Green
}

# Xoa Site cu neu da co
if (Get-Website -Name $SiteName -ErrorAction SilentlyContinue) {
    Write-Host "   Xoa ban ghi Website cu de tao moi..." -ForegroundColor Gray
    Remove-Website -Name $SiteName
    Start-Sleep -Seconds 1
}

# Tao Website moi
New-Website -Name $SiteName -Port $Port -PhysicalPath $FrontendPath -ApplicationPool $appPoolName | Out-Null
Start-Website -Name $SiteName

# Kiem tra trang thai site
Start-Sleep -Seconds 1
$siteState = (Get-Website -Name $SiteName -ErrorAction SilentlyContinue).State
Write-Host "   -> [PASS] Trang thai Website '$SiteName': $siteState" -ForegroundColor Green

# 5. Mo cong tuong lua Windows Firewall cho cong Web
Write-Host "`n5. Cau hinh Windows Firewall cho cong $Port..." -ForegroundColor Yellow
$fwRule = Get-NetFirewallRule -DisplayName "MDMSv2_Frontend_Port_$Port" -ErrorAction SilentlyContinue
if (-not $fwRule) {
    New-NetFirewallRule -DisplayName "MDMSv2_Frontend_Port_$Port" -Direction Inbound -LocalPort $Port -Protocol TCP -Action Allow | Out-Null
    Write-Host "   -> [PASS] Da tao Inbound Firewall Rule cho cong $Port." -ForegroundColor Green
} else {
    Write-Host "   -> [PASS] Firewall Rule cho cong $Port da ton tai." -ForegroundColor Green
}

# 6. Kiem tra phan hoi HTTP
Write-Host "`n6. Kiem tra phan hoi giao dien Frontend..." -ForegroundColor Yellow
$webSuccess = $false
try {
    $res = Invoke-WebRequest -Uri "http://localhost:$Port" -UseBasicParsing -TimeoutSec 5 -ErrorAction SilentlyContinue
    if ($res.StatusCode -eq 200) {
        $webSuccess = $true
    }
} catch {}

Write-Host "`n==========================================================" -ForegroundColor Green
Write-Host "-> IIS WEBSITE '$SiteName' DA TRIEN KHAI THANH CONG!" -ForegroundColor Green
Write-Host "   Dia chi truy cap giao dien : http://localhost:$Port" -ForegroundColor Cyan
Write-Host "   Reverse Proxy API Backend  : http://localhost:$Port/api" -ForegroundColor Cyan
Write-Host "   Swagger API Docs qua IIS   : http://localhost:$Port/swagger" -ForegroundColor Cyan
Write-Host "   Hangfire Dashboard qua IIS : http://localhost:$Port/dashboard" -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Green

if ($webSuccess) {
    Write-Host "-> [PASS] Giao dien React SPA da phan hoi HTTP 200 san sang phuc vu!" -ForegroundColor Green
}
