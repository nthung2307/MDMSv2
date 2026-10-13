<#
.SYNOPSIS
    Cai dat cac thanh phan phu tro (Prerequisites) tren Windows Server 2019.
.DESCRIPTION
    Script thuc hien:
    1. Kich hoat tinh nang Web Server (IIS) va cac module can thiet (WebSocket, Static Content).
    2. Cai dat tu dong module IIS URL Rewrite 2.1 va Application Request Routing (ARR) 3.0.
    3. Bat tinh nang ARR Proxy de ho tro Reverse Proxy API sang Backend.
    4. Tao quy tac Windows Defender Firewall cho cac cong dich vu (80, 443, 19089, 39320, 3306).
#>

#Requires -RunAsAdministrator
$ErrorActionPreference = "Stop"

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host " [BUOC 1] CAI DAT PREREQUISITES TREN WINDOWS SERVER 2019  " -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

# 1. Kich hoat IIS va cac tinh nang phu tro
Write-Host "`n1. Dang kiem tra va kich hoat Web-Server (IIS)..." -ForegroundColor Yellow
$features = @(
    "Web-Server",
    "Web-WebServer",
    "Web-Common-Http",
    "Web-Default-Doc",
    "Web-Dir-Browsing",
    "Web-Http-Errors",
    "Web-Static-Content",
    "Web-Http-Redirect",
    "Web-Health",
    "Web-Http-Logging",
    "Web-Performance",
    "Web-Stat-Compression",
    "Web-Security",
    "Web-Filtering",
    "Web-App-Dev",
    "Web-Net-Ext45",
    "Web-Asp-Net45",
    "Web-ISAPI-Ext",
    "Web-ISAPI-Filter",
    "Web-WebSockets",
    "Web-Mgmt-Tools",
    "Web-Mgmt-Console"
)

foreach ($feature in $features) {
    if ((Get-WindowsFeature -Name $feature).Installed -ne $true) {
        Write-Host "   -> Dang cai dat feature: $feature..." -ForegroundColor Gray
        Install-WindowsFeature -Name $feature | Out-Null
    }
}
Write-Host "   -> [PASS] IIS va WebSockets da duoc kich hoat thanh cong!" -ForegroundColor Green

# Ham ho tro cai dat IIS Module
function Install-IisModuleIfMissing {
    param(
        [string]$ModuleName,
        [string]$DllPath,
        [string]$MsiFileName,
        [string]$DownloadUrl
    )

    if (Test-Path $DllPath) {
        Write-Host "   -> [PASS] $ModuleName da duoc cai dat." -ForegroundColor Green
        return $true
    }

    Write-Host "   -> Phat hien chua co $ModuleName. Dang tien hanh cai dat tu dong..." -ForegroundColor Yellow

    # Tim file msi trong thu muc Prerequisites cuc bo truoc
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
            Write-Host "      Tim thay file bo cai offline tai: $msiPath" -ForegroundColor Gray
            break
        }
    }

    # Neu khong co offline, thu tai tu Microsoft CDN
    if (-not $msiPath) {
        $msiPath = "$env:TEMP\$MsiFileName"
        Write-Host "      Dang tai $ModuleName tu Microsoft CDN..." -ForegroundColor Gray
        try {
            [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
            Invoke-WebRequest -Uri $DownloadUrl -OutFile $msiPath -UseBasicParsing -TimeoutSec 60
            Write-Host "      Tai xuong thanh cong: $msiPath" -ForegroundColor Green
        } catch {
            Write-Warning "      Khong the tu dong tai $MsiFileName tu Internet ($($_.Exception.Message))."
            return $false
        }
    }

    # Chuan hoa tuyet doi: Copy file MSI sang TEMP de tranh loi msiexec 1619 (do relative path hoac permission)
    $stagedMsi = Join-Path $env:TEMP $MsiFileName
    if ($msiPath -ne $stagedMsi) {
        Copy-Item -Path $msiPath -Destination $stagedMsi -Force
    }

    Write-Host "      Dang cai dat $MsiFileName (msiexec /qn)..." -ForegroundColor Gray
    $proc = Start-Process -FilePath "msiexec.exe" -ArgumentList "/i `"$stagedMsi`" /qn /norestart" -Wait -PassThru
    
    # Kiem tra ket qua: hoac exit code thanh cong (0 / 3010) hoac DLL da ton tai
    if ($proc.ExitCode -eq 0 -or $proc.ExitCode -eq 3010 -or (Test-Path $DllPath)) {
        Write-Host "   -> [PASS] Da cai dat thanh cong $ModuleName!" -ForegroundColor Green
        return $true
    } else {
        Write-Warning "      Cai dat $ModuleName tra ve ma loi: $($proc.ExitCode)"
        return $false
    }
}

# 2. Cai dat URL Rewrite 2.1 va ARR 3.0
Write-Host "`n2. Dang kiem tra va cai dat IIS URL Rewrite & Application Request Routing (ARR)..." -ForegroundColor Yellow

# URL Rewrite 2.1
Install-IisModuleIfMissing `
    -ModuleName "IIS URL Rewrite 2.1" `
    -DllPath "$env:SystemRoot\System32\inetsrv\rewrite.dll" `
    -MsiFileName "rewrite_amd64_en-US.msi" `
    -DownloadUrl "https://download.microsoft.com/download/1/2/8/128E2E22-C1B9-44A4-BE2A-5859ED1D4592/rewrite_amd64_en-US.msi" | Out-Null

# ARR 3.0
Install-IisModuleIfMissing `
    -ModuleName "IIS Application Request Routing (ARR) 3.0" `
    -DllPath "$env:SystemRoot\System32\inetsrv\requestRouter.dll" `
    -MsiFileName "requestRouter_amd64.msi" `
    -DownloadUrl "https://download.microsoft.com/download/e/9/8/e9849d6a-020e-47e4-9fd0-a023e99b54eb/requestRouter_amd64.msi" | Out-Null

# Bat tinh nang ARR Proxy de ho tro Reverse Proxy API sang Backend
$appcmd = "$env:SystemRoot\System32\inetsrv\appcmd.exe"
if (Test-Path $appcmd) {
    try {
        & $appcmd set config -section:system.webServer/proxy /enabled:"True" /commit:apphost | Out-Null
        Write-Host "   -> [PASS] Da bat tinh nang ARR Proxy (Enable proxy: True)." -ForegroundColor Green
    } catch {
        Write-Warning "   Khong the tu dong bat ARR Proxy qua appcmd.exe."
    }
}

# 3. Kiem tra va cai dat Java Runtime Environment (JRE) cho Kepware IoT Gateway
Write-Host "`n3. Dang kiem tra va cai dat Java Runtime Environment (cho Kepware IoT Gateway)..." -ForegroundColor Yellow

function Install-JavaIfMissing {
    $javaCmd = Get-Command "java.exe" -ErrorAction SilentlyContinue
    $javaRegistry = "HKLM:\SOFTWARE\JavaSoft\Java Runtime Environment"
    $javaRegistry64 = "HKLM:\SOFTWARE\WOW6432Node\JavaSoft\Java Runtime Environment"
    $hasJava = ($javaCmd -ne $null) -or (Test-Path $javaRegistry) -or (Test-Path $javaRegistry64) -or (Test-Path "C:\Program Files\Java") -or (Test-Path "C:\Program Files (x86)\Java")

    if ($hasJava) {
        Write-Host "   -> [PASS] Java Runtime Environment (JRE) da ton tai tren he thong." -ForegroundColor Green
        return $true
    }

    Write-Host "   -> Phat hien chua co Java. Dang tim bo cai offline de cai dat tu dong..." -ForegroundColor Yellow
    $possibleDirs = @(
        "$PSScriptRoot\..\Prerequisites",
        "C:\MDMSv2\Prerequisites",
        "$PSScriptRoot"
    )

    $javaPkg = $null
    foreach ($d in $possibleDirs) {
        if (Test-Path $d) {
            $found = Get-ChildItem -Path $d -Include "jre*.exe", "*java*.exe", "jdk*.exe", "jre*.msi" -Recurse -File -ErrorAction SilentlyContinue | Select-Object -First 1
            if ($found) {
                $javaPkg = $found.FullName
                break
            }
        }
    }

    if ($javaPkg) {
        Write-Host "      Tim thay bo cai Java: $javaPkg" -ForegroundColor Gray
        Write-Host "      Dang cai dat Java silent (/s)..." -ForegroundColor Gray
        if ($javaPkg -like "*.msi") {
            $proc = Start-Process -FilePath "msiexec.exe" -ArgumentList "/i `"$javaPkg`" /qn /norestart" -Wait -PassThru
        } else {
            $proc = Start-Process -FilePath $javaPkg -ArgumentList "/s" -Wait -PassThru
        }
        if ($proc.ExitCode -eq 0 -or $proc.ExitCode -eq 3010) {
            Write-Host "   -> [PASS] Da cai dat thanh cong Java Runtime Environment!" -ForegroundColor Green
            return $true
        } else {
            Write-Warning "      Cai dat Java tra ve ma thoat: $($proc.ExitCode)"
            return $false
        }
    } else {
        Write-Warning "      Khong tim thay file cai dat Java trong thu muc Prerequisites!"
        return $false
    }
}
Install-JavaIfMissing | Out-Null

# 4. Kiem tra va cai dat Microsoft Edge (thay the Internet Explorer tren Windows Server 2019)
Write-Host "`n4. Dang kiem tra va cai dat Microsoft Edge..." -ForegroundColor Yellow

function Install-MicrosoftEdgeIfMissing {
    $edgePaths = @(
        "C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe",
        "C:\Program Files\Microsoft\Edge\Application\msedge.exe"
    )
    $edgeInstalled = $false
    foreach ($ep in $edgePaths) {
        if (Test-Path $ep) {
            $edgeInstalled = $true
            break
        }
    }

    if ($edgeInstalled) {
        Write-Host "   -> [PASS] Microsoft Edge da duoc cai dat tren he thong." -ForegroundColor Green
        return $true
    }

    Write-Host "   -> Phat hien chua co Microsoft Edge. Dang tim bo cai de cai dat tu dong..." -ForegroundColor Yellow
    $possibleDirs = @(
        "$PSScriptRoot\..\Prerequisites",
        "C:\MDMSv2\Prerequisites",
        "$PSScriptRoot"
    )

    $edgePkg = $null
    foreach ($d in $possibleDirs) {
        if (Test-Path $d) {
            $found = Get-ChildItem -Path $d -Include "*MicrosoftEdge*.exe", "*Edge*.exe", "*Edge*.msi" -Recurse -File -ErrorAction SilentlyContinue | Select-Object -First 1
            if ($found) {
                $edgePkg = $found.FullName
                break
            }
        }
    }

    if ($edgePkg) {
        Write-Host "      Tim thay bo cai Microsoft Edge: $edgePkg" -ForegroundColor Gray
        Write-Host "      Dang cai dat Microsoft Edge (/silent /install)..." -ForegroundColor Gray
        if ($edgePkg -like "*.msi") {
            $proc = Start-Process -FilePath "msiexec.exe" -ArgumentList "/i `"$edgePkg`" /qn /norestart" -Wait -PassThru
        } else {
            $proc = Start-Process -FilePath $edgePkg -ArgumentList "/silent /install" -Wait -PassThru
        }
        if ($proc.ExitCode -eq 0 -or $proc.ExitCode -eq 3010) {
            Write-Host "   -> [PASS] Da cai dat thanh cong Microsoft Edge!" -ForegroundColor Green
            return $true
        } else {
            Write-Warning "      Cai dat Microsoft Edge tra ve ma thoat: $($proc.ExitCode)"
            return $false
        }
    } else {
        Write-Warning "      Khong tim thay file cai dat Microsoft Edge trong thu muc Prerequisites!"
        return $false
    }
}
Install-MicrosoftEdgeIfMissing | Out-Null

# 5. Cau hinh Windows Firewall
Write-Host "`n5. Dang cau hinh Windows Firewall cho he thong MDMSv2..." -ForegroundColor Yellow

function Add-FirewallPortRule {
    param([string]$RuleName, [int]$Port, [string]$Protocol = "TCP")
    if (-not (Get-NetFirewallRule -DisplayName $RuleName -ErrorAction SilentlyContinue)) {
        New-NetFirewallRule -DisplayName $RuleName -Direction Inbound -LocalPort $Port -Protocol $Protocol -Action Allow | Out-Null
        Write-Host "   -> [PASS] Da mo cong $Port ($Protocol): $RuleName" -ForegroundColor Green
    } else {
        Write-Host "   -> [PASS] Quy tac $RuleName da ton tai (Cong $Port)." -ForegroundColor Gray
    }
}

Add-FirewallPortRule -RuleName "MDMSv2 - Web Frontend (HTTP)" -Port 80
Add-FirewallPortRule -RuleName "MDMSv2 - Web Frontend (HTTPS)" -Port 443
Add-FirewallPortRule -RuleName "MDMSv2 - WebApi Backend" -Port 19089
Add-FirewallPortRule -RuleName "MDMSv2 - Kepware IoT Gateway REST Server" -Port 39320
Add-FirewallPortRule -RuleName "MDMSv2 - MySQL Server Database" -Port 3306

Write-Host "`n==========================================================" -ForegroundColor Green
Write-Host " HOAN TAT BUOC 1. TIEP TUC BUOC 2 DE CAI DAT BACKEND SERVICE." -ForegroundColor Green
Write-Host "==========================================================" -ForegroundColor Green
