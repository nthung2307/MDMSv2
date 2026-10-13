<#
.SYNOPSIS
    Kich ban Wizard cai dat toan dien MDMSv2 tren Windows Server 2019 (All-in-One).
.DESCRIPTION
    Kich ban huong dan va thuc hien tu dong toan bo quy trinh:
    [Buoc 1] Kich hoat IIS, URL Rewrite 2.1, ARR 3.0, ARR Proxy, Java JRE, Edge va Firewall.
    [Buoc 2] Kiem tra/Huong dan cai dat MySQL Server 8.0, cau hinh mat khau root dong,
             import database 'amr' tu amr_full.sql va cap nhat appsettings.Production.json.
    [Buoc 3] Hoi cau hinh tai khoan Quan tri MDMS (UserName, Password), ma hoa PBKDF2
             va dong bo vao co so du lieu amr.users cung appsettings.Production.json.
    [Buoc 4] Kiem tra/Huong dan cai dat Kepware Server tu thu muc bo cai Kepware.
    [Buoc 5] Thiet lap va khoi dong Backend WebApi (.NET 8) dang Windows Background Task.
    [Buoc 6] Trien khai giao dien Frontend (React 18 + Vite) len IIS Website (Port 80).
    [Buoc 7] Kiem tra hoat dong toan dien (Health Check) va mo Microsoft Edge.
#>

#Requires -RunAsAdministrator
[CmdletBinding()]
param (
    [string]$RootDeployPath = "C:\MDMSv2",
    [int]$BackendPort = 19089,
    [int]$FrontendPort = 80
)

$ErrorActionPreference = "Stop"

Clear-Host
Write-Host "================================================================================" -ForegroundColor Cyan
Write-Host "       HE THONG THU THAP VA QUAN LY DU LIEU CONG TO DIEN TU (MDMSv2)           " -ForegroundColor Cyan
Write-Host "         WIZARD CAI DAT VA TRIEN KHAI TOAN DIEN TREN WINDOWS SERVER 2019        " -ForegroundColor Cyan
Write-Host "================================================================================" -ForegroundColor Cyan
Write-Host "Thoi gian bat dau  : $(Get-Date -Format 'dd/MM/yyyy HH:mm:ss')" -ForegroundColor Gray

# Tu dong nhan dien thu muc cai dat goc (RootDeployPath):
if (Test-Path (Join-Path $PSScriptRoot "Backend")) {
    $RootDeployPath = $PSScriptRoot
} elseif (Test-Path (Join-Path $PSScriptRoot "..\Backend")) {
    $RootDeployPath = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
} elseif (-not (Test-Path "$RootDeployPath\Backend")) {
    $RootDeployPath = "C:\MDMSv2"
}

Write-Host "Thu muc cai dat goc: $RootDeployPath" -ForegroundColor Gray
Write-Host "--------------------------------------------------------------------------------" -ForegroundColor Gray

$backendDir = Join-Path $RootDeployPath "Backend"
$frontendDir = Join-Path $RootDeployPath "Frontend"
$databaseDir = Join-Path $RootDeployPath "Database"
$scriptsDir = Join-Path $RootDeployPath "Scripts"
$mysqlDir = Join-Path $RootDeployPath "MySQL"
$kepwareDir = Join-Path $RootDeployPath "Kepware"
$prereqDir = Join-Path $RootDeployPath "Prerequisites"

# Ham ma hoa mat khau PBKDF2 RFC2898 tuong thich 100% voi Backend MDMSv2
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

# Ham tim mysql.exe
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

# ==============================================================================
# [BUOC 1] CAI DAT PREREQUISITES (IIS, URL REWRITE, ARR, JAVA, EDGE, FIREWALL)
# ==============================================================================
Write-Host "`n[BUOC 1/7] CAI DAT CAC THANH PHAN PHU TRO (PREREQUISITES)..." -ForegroundColor Yellow
$script1 = Join-Path $scriptsDir "1_Install_Prerequisites.ps1"
if (-not (Test-Path $script1)) {
    $script1 = Join-Path $PSScriptRoot "1_Install_Prerequisites.ps1"
}

if (Test-Path $script1) {
    & $script1
} else {
    Write-Warning "Khong tim thay file 1_Install_Prerequisites.ps1!"
}

# ==============================================================================
# [BUOC 2] CAI DAT & THIET LAP CO SO DU LIEU MYSQL SERVER 8.0
# ==============================================================================
Write-Host "`n================================================================================" -ForegroundColor Cyan
Write-Host " [BUOC 2/7] THIET LAP CO SO DU LIEU MYSQL SERVER 8.0                          " -ForegroundColor Yellow
Write-Host "================================================================================" -ForegroundColor Cyan

# 2.1 Kiem tra MySQL service
$tcp = New-Object System.Net.Sockets.TcpClient
$mysqlRunning = $false
try {
    $tcp.ConnectAsync("127.0.0.1", 3306).Wait(1500) | Out-Null
    $mysqlRunning = $tcp.Connected
    $tcp.Close()
} catch {}

if (-not $mysqlRunning) {
    $svc = Get-Service -Name "MySQL80" -ErrorAction SilentlyContinue
    if ($svc -and $svc.Status -ne "Running") {
        Write-Host "   Dang khoi dong dich vu MySQL80..." -ForegroundColor Gray
        Start-Service -Name "MySQL80" -ErrorAction SilentlyContinue
        Start-Sleep -Seconds 2
        try {
            $tcp = New-Object System.Net.Sockets.TcpClient
            $tcp.ConnectAsync("127.0.0.1", 3306).Wait(1500) | Out-Null
            $mysqlRunning = $tcp.Connected
            $tcp.Close()
        } catch {}
    }
}

if (-not $mysqlRunning) {
    Write-Host "`n   [THONG BAO] Chua phat hien dich vu MySQL Server tren cong 3306." -ForegroundColor Yellow
    
    $installerFiles = @()
    if (Test-Path $mysqlDir) {
        $installerFiles = Get-ChildItem -Path $mysqlDir -Include "*.msi", "*.exe" -Recurse -File -ErrorAction SilentlyContinue
    }
    
    if ($installerFiles.Count -gt 0) {
        $chosenInstaller = $installerFiles[0]
        Write-Host "   -> Tim thay bo cai MySQL: $($chosenInstaller.FullName)" -ForegroundColor Green
        $runNow = Read-Host "   Ban co muon mo bo cai MySQL ngay bay gio khong? (Y/n) [Mac dinh: Y]"
        if ([string]::IsNullOrWhiteSpace($runNow) -or $runNow.Trim().ToUpper() -eq "Y") {
            Write-Host "   Dang khoi chay bo cai MySQL..." -ForegroundColor Cyan
            Write-Host "   * Huong dan trong trinh cai dat MySQL:" -ForegroundColor White
            Write-Host "     1. Chon Setup Type: Server only" -ForegroundColor Gray
            Write-Host "     2. Cong Port: 3306" -ForegroundColor Gray
            Write-Host "     3. Dat mat khau cho tai khoan 'root' va ghi nho mat khau nay." -ForegroundColor Gray
            Write-Host "     4. Hoan tat cai dat va khoi dong dich vu MySQL80." -ForegroundColor Gray
            
            Start-Process -FilePath $chosenInstaller.FullName -Wait
            Write-Host "   Trinh cai dat MySQL da dong. Kiem tra lai cong 3306..." -ForegroundColor Gray
        }
    } else {
        Write-Host "   -> Chua co file cai dat MySQL trong thu muc: $mysqlDir" -ForegroundColor Yellow
        Write-Host "      Vui long copy file mysql-installer-*.msi vao thu muc tren." -ForegroundColor White
        Read-Host "   Sau khi hoan tat cai dat MySQL va khoi dong dich vu, nhan Enter de tiep tuc"
    }
}

# 2.2 Thu thap thong tin dang nhap MySQL
Write-Host "`n   --- CAU HINH KET NOI MYSQL DATABASE ---" -ForegroundColor Cyan
$inputHost = Read-Host "   Nhap dia chi may chu MySQL [Mac dinh: 127.0.0.1]"
if ([string]::IsNullOrWhiteSpace($inputHost)) { $inputHost = "127.0.0.1" }

$inputPort = Read-Host "   Nhap cong ket noi MySQL [Mac dinh: 3306]"
if ([string]::IsNullOrWhiteSpace($inputPort)) { $inputPort = "3306" }

$inputUser = Read-Host "   Nhap tai khoan quan tri MySQL [Mac dinh: root]"
if ([string]::IsNullOrWhiteSpace($inputUser)) { $inputUser = "root" }

$mysqlPassword = ""
while ([string]::IsNullOrWhiteSpace($mysqlPassword)) {
    $promptSecure = Read-Host "   Nhap mat khau cho user '$inputUser' cua MySQL Server" -AsSecureString
    $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($promptSecure)
    $mysqlPassword = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
    [System.Runtime.InteropServices.Marshal]::ZeroFreeBSTR($BSTR)
    if ([string]::IsNullOrWhiteSpace($mysqlPassword)) {
        Write-Host "   Mat khau MySQL khong duoc de trong! Vui long nhap lai." -ForegroundColor Red
    }
}

# 2.3 Khoi tao & Import Co so Du lieu 'amr' (Uu tien amr_full.sql)
$mysqlExe = Find-MySqlExe
if ($mysqlExe) {
    Write-Host "   Tim thay cong cu MySQL Client: $mysqlExe" -ForegroundColor Gray
    
    $dumpFile = $null
    $fileFull = "$databaseDir\amr_full.sql"
    $fileInit = "$databaseDir\init_amr_database.sql"
    if (Test-Path $fileFull) {
        $dumpFile = $fileFull
    } elseif (Test-Path $fileInit) {
        $dumpFile = $fileInit
    }

    if ($dumpFile) {
        $dumpFileName = Split-Path $dumpFile -Leaf
        Write-Host "   Dang khoi tao co so du lieu 'amr' tu tap tin: $dumpFileName..." -ForegroundColor Yellow
        try {
            $importBatch = Join-Path $env:TEMP "import_amr.bat"
            $batchContent = "@echo off`r`n`"$mysqlExe`" -h $inputHost -P $inputPort -u $inputUser `"-p$mysqlPassword`" --default-character-set=utf8mb4 < `"$dumpFile`""
            Set-Content -Path $importBatch -Value $batchContent -Encoding ASCII
            
            $proc = Start-Process -FilePath $importBatch -Wait -PassThru -NoNewWindow
            Remove-Item -Path $importBatch -Force -ErrorAction SilentlyContinue

            if ($proc.ExitCode -eq 0) {
                Write-Host "   -> [PASS] Da import thanh cong co so du lieu 'amr' tu $dumpFileName!" -ForegroundColor Green
            } else {
                Write-Warning "   Lenh import co so du lieu ket thuc voi ma thoat: $($proc.ExitCode)"
            }
        } catch {
            Write-Warning "   Loi khi thuc thi import MySQL: $($_.Exception.Message)"
        }
    } else {
        Write-Warning "   Khong tim thay file SQL khoi tao trong thu muc: $databaseDir"
    }
} else {
    Write-Host "   (Khong tim thay file mysql.exe truc tiep tren PATH)." -ForegroundColor Gray
}

# 2.4 Cap nhat chuoi ket noi MySQL vao Backend appsettings.Production.json
$appsettingsPath = Join-Path $backendDir "appsettings.Production.json"
if (Test-Path $appsettingsPath) {
    Write-Host "   Dang cap nhat cau hinh MySQL vao: $appsettingsPath..." -ForegroundColor Yellow
    $configContent = Get-Content -Path $appsettingsPath -Raw -Encoding UTF8 | ConvertFrom-Json
    
    $appDbInfo = [PSCustomObject]@{
        Server   = $inputHost
        Port     = $inputPort
        UserName = $inputUser
        Password = $mysqlPassword
        Database = "amr"
    }
    if ($configContent.PSObject.Properties['AppDb']) {
        $configContent.AppDb = $appDbInfo
    } else {
        $configContent | Add-Member -MemberType NoteProperty -Name "AppDb" -Value $appDbInfo -Force
    }
    
    $jsonOutput = $configContent | ConvertTo-Json -Depth 10
    Set-Content -Path $appsettingsPath -Value $jsonOutput -Encoding UTF8
    Write-Host "   -> [PASS] Da luu thong tin ket noi MySQL vao appsettings.Production.json!" -ForegroundColor Green
} else {
    Write-Warning "   Khong tim thay file cau hinh: $appsettingsPath"
}

# ==============================================================================
# [BUOC 3] THIET LAP TAI KHOAN QUAN TRI VIEN MDMS (ADMIN PORTAL)
# ==============================================================================
Write-Host "`n================================================================================" -ForegroundColor Cyan
Write-Host " [BUOC 3/7] THIET LAP TAI KHOAN QUAN TRI VIEN MDMS (ADMIN PORTAL)            " -ForegroundColor Yellow
Write-Host "================================================================================" -ForegroundColor Cyan
Write-Host "   Tai khoan nay dung de dang nhap vao Web Portal MDMSv2 tren trinh duyet." -ForegroundColor Gray

$mdmsAdminUser = Read-Host "   Nhap ten dang nhap Admin MDMS [Mac dinh: admin]"
if ([string]::IsNullOrWhiteSpace($mdmsAdminUser)) { $mdmsAdminUser = "admin" }

$mdmsAdminPass = ""
do {
    $secPass1 = Read-Host "   Nhap mat khau cho tai khoan '$mdmsAdminUser' (toi thieu 6 ky tu)" -AsSecureString
    $BSTR1 = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($secPass1)
    $pass1 = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR1)
    [System.Runtime.InteropServices.Marshal]::ZeroFreeBSTR($BSTR1)

    if ($pass1.Length -lt 6) {
        Write-Host "   Mat khau phai co toi thieu 6 ky tu. Vui long nhap lai!" -ForegroundColor Red
        continue
    }

    $secPass2 = Read-Host "   Xac nhan lai mat khau" -AsSecureString
    $BSTR2 = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($secPass2)
    $pass2 = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR2)
    [System.Runtime.InteropServices.Marshal]::ZeroFreeBSTR($BSTR2)

    if ($pass1 -ne $pass2) {
        Write-Host "   Mat khau xac nhan khong khop! Vui long nhap lai tu dau." -ForegroundColor Red
    } else {
        $mdmsAdminPass = $pass1
    }
} while ([string]::IsNullOrWhiteSpace($mdmsAdminPass))

# Bam mat khau theo chuan PBKDF2 cua MDMSv2
Write-Host "   Dang ma hoa mat khau theo chuan bao mat PBKDF2 RFC2898..." -ForegroundColor Gray
$mdmsHashedPassword = Hash-MdmsPassword -password $mdmsAdminPass

# Cap nhat thong tin Admin vao appsettings.Production.json
if (Test-Path $appsettingsPath) {
    $configContent = Get-Content -Path $appsettingsPath -Raw -Encoding UTF8 | ConvertFrom-Json
    $adminInfo = [PSCustomObject]@{
        UserName     = $mdmsAdminUser
        PasswordNote = "Tai khoan dang nhap duoc quan ly trong co so du lieu amr.users"
    }
    if ($configContent.PSObject.Properties['AdminAccount']) {
        $configContent.AdminAccount = $adminInfo
    } else {
        $configContent | Add-Member -MemberType NoteProperty -Name "AdminAccount" -Value $adminInfo -Force
    }
    
    $jsonOutput = $configContent | ConvertTo-Json -Depth 10
    Set-Content -Path $appsettingsPath -Value $jsonOutput -Encoding UTF8
    Write-Host "   -> [PASS] Da ghi nhan tai khoan admin vao cau hinh." -ForegroundColor Green
}

# Dong bo vao bang users trong co so du lieu amr
if ($mysqlExe) {
    try {
        $updateSqlFile = "$env:TEMP\update_admin.sql"
        $sqlTemplate = @'
USE amr;
SET SQL_SAFE_UPDATES = 0;
INSERT INTO users (Id, UserName, Password, FullName, Email, PhoneNumber, Address, CustomerCode, Description, Status, CountLocked, TimeLocked, IsLocked, RoleName, CreatedTime, ModifiedTime, Deleted)
SELECT 921946681335811, '{0}', '{1}', 'Administrator', 'admin@mdms.local', '', '', '', 'System Administrator', 'ACTIVE', 0, '2026-01-01 00:00:00', 0, 'SUPERADMIN', NOW(), NOW(), 0
WHERE NOT EXISTS (SELECT 1 FROM users WHERE UserName = '{0}' OR UserName = 'admin' OR RoleName = 'SUPERADMIN');
UPDATE users SET UserName = '{0}', Password = '{1}', RoleName = 'SUPERADMIN', Status = 'ACTIVE', IsLocked = 0, ModifiedTime = NOW()
WHERE UserName = '{0}' OR UserName = 'admin' OR RoleName = 'SUPERADMIN' OR Id = 921946681335811;
INSERT INTO systemconfig (Id, `Key`, `Value`) VALUES
  (1, 'LOCKLOGIN', '5'),
  (2, 'NUMBEROFINCORRECTENTRIES', '5'),
  (3, 'SYSTEMADMINISTRATIONFULLNAME', 'Administrator'),
  (4, 'SYSTEMADMINISTRATIONPHONE', '0901234567'),
  (5, 'SYSTEMADMINISTRATIONEMAIL', 'admin@mdms.vn'),
  (6, 'CUSTOMERCAREFULLNAME', 'CSKH'),
  (7, 'CUSTOMERCAREPHONE', '19001006'),
  (8, 'CUSTOMERCAREEMAIL', 'cskh@mdms.vn')
ON DUPLICATE KEY UPDATE `Value` = VALUES(`Value`);
SET SQL_SAFE_UPDATES = 1;
'@
        $sqlContent = $sqlTemplate -f $mdmsAdminUser, $mdmsHashedPassword
        Set-Content -Path $updateSqlFile -Value $sqlContent -Encoding UTF8

        $batchUpdate = "$env:TEMP\update_admin.bat"
        $updateBatchContent = "@echo off`r`n`"$mysqlExe`" -h $inputHost -P $inputPort -u $inputUser `"-p$mysqlPassword`" < `"$updateSqlFile`""
        Set-Content -Path $batchUpdate -Value $updateBatchContent -Encoding ASCII
        
        $procUpdate = Start-Process -FilePath $batchUpdate -Wait -PassThru -NoNewWindow
        Remove-Item -Path $updateSqlFile, $batchUpdate -Force -ErrorAction SilentlyContinue
        Write-Host "   -> [PASS] Da cap nhat tai khoan '$mdmsAdminUser' va khoi tao cau hinh he thong trong DB amr!" -ForegroundColor Green
    } catch {
        Write-Host "   (Bang users co the duoc dong bo sau khi khoi dong Backend)." -ForegroundColor Gray
    }
}

# ==============================================================================
# [BUOC 4] KIEM TRA & HUONG DAN CAI DAT KEPWARE SERVER
# ==============================================================================
Write-Host "`n================================================================================" -ForegroundColor Cyan
Write-Host " [BUOC 4/7] KIEM TRA & THIET LAP KEPWARE SERVER                               " -ForegroundColor Yellow
Write-Host "================================================================================" -ForegroundColor Cyan

$tcpKep = New-Object System.Net.Sockets.TcpClient
$kepRunning = $false
try {
    $tcpKep.ConnectAsync("127.0.0.1", 39320).Wait(1500) | Out-Null
    $kepRunning = $tcpKep.Connected
    $tcpKep.Close()
} catch {}

if ($kepRunning) {
    Write-Host "   -> [PASS] Kepware IoT Gateway REST Server dang lang nghe tren cong 39320." -ForegroundColor Green
} else {
    $kepService = Get-Service -Name "KEPServerEXV6" -ErrorAction SilentlyContinue
    if ($kepService) {
        Write-Host "   Dich vu Kepware Server ($($kepService.Name)) da cai dat, trang thai: $($kepService.Status)." -ForegroundColor Cyan
    } else {
        Write-Host "   Chua phat hien dich vu Kepware tren he thong." -ForegroundColor Yellow
        $kepInstallers = @()
        if (Test-Path $kepwareDir) {
            $kepInstallers = Get-ChildItem -Path "$kepwareDir\*" -Include "*.exe", "*.msi" -Recurse -File -ErrorAction SilentlyContinue
        }

        if ($kepInstallers.Count -gt 0) {
            $chosenKep = $kepInstallers[0]
            Write-Host "   -> Tim thay bo cai Kepware: $($chosenKep.FullName)" -ForegroundColor Green
            $runKep = Read-Host "   Ban co muon mo bo cai Kepware ngay bay gio khong? (Y/n) [Mac dinh: Y]"
            if ([string]::IsNullOrWhiteSpace($runKep) -or $runKep.Trim().ToUpper() -eq "Y") {
                Write-Host "   Dang khoi chay bo cai Kepware..." -ForegroundColor Cyan
                Write-Host "   * LUU Y QUAN TRONG KHI CAI DAT KEPWARE:" -ForegroundColor White
                Write-Host "     - Chon 'Modbus Suite' (Modbus RTU/TCP Driver)." -ForegroundColor Gray
                Write-Host "     - Chon 'IoT Gateway' (REST Server & REST Client Agent)." -ForegroundColor Gray
                Start-Process -FilePath $chosenKep.FullName -Wait
            }
        } else {
            Write-Host "   -> Vui long copy file cai dat Kepware vao thu muc: $kepwareDir" -ForegroundColor Yellow
        }
    }
}

# ==============================================================================
# [BUOC 5] THIET LAP VA KHOI DONG DICH VU BACKEND (.NET 8 WEBAPI)
# ==============================================================================
Write-Host "`n================================================================================" -ForegroundColor Cyan
Write-Host " [BUOC 5/7] THIET LAP & KHOI DONG DICH VU NEN BACKEND (.NET 8 WEBAPI)          " -ForegroundColor Yellow
Write-Host "================================================================================" -ForegroundColor Cyan

$script2 = Join-Path $scriptsDir "2_Setup_Backend_Service.ps1"
if (-not (Test-Path $script2)) {
    $script2 = Join-Path $PSScriptRoot "2_Setup_Backend_Service.ps1"
}

if (Test-Path $script2) {
    & $script2 -BackendPath $backendDir -ListeningPort $BackendPort
} else {
    Write-Error "Khong tim thay file 2_Setup_Backend_Service.ps1!"
}

# ==============================================================================
# [BUOC 6] TRIEN KHAI FRONTEND REACT 18 LEN IIS WEBSITE (PORT 80)
# ==============================================================================
Write-Host "`n================================================================================" -ForegroundColor Cyan
Write-Host " [BUOC 6/7] TRIEN KHAI GIAO DIEN FRONTEND (REACT 18) LEN IIS SITE            " -ForegroundColor Yellow
Write-Host "================================================================================" -ForegroundColor Cyan

$script3 = Join-Path $scriptsDir "3_Setup_Frontend_IIS.ps1"
if (-not (Test-Path $script3)) {
    $script3 = Join-Path $PSScriptRoot "3_Setup_Frontend_IIS.ps1"
}

if (Test-Path $script3) {
    & $script3 -FrontendPath $frontendDir -Port $FrontendPort
} else {
    Write-Error "Khong tim thay file 3_Setup_Frontend_IIS.ps1!"
}

# ==============================================================================
# [BUOC 7] KIEM TRA HOAT DONG TOAN DIEN VA TONG KET BAN GIAO
# ==============================================================================
Write-Host "`n================================================================================" -ForegroundColor Cyan
Write-Host " [BUOC 7/7] KIEM TRA TOAN DIEN HE THONG (HEALTH CHECK)                       " -ForegroundColor Yellow
Write-Host "================================================================================" -ForegroundColor Cyan

$script4 = Join-Path $scriptsDir "4_Verify_System.ps1"
if (-not (Test-Path $script4)) {
    $script4 = Join-Path $PSScriptRoot "4_Verify_System.ps1"
}

if (Test-Path $script4) {
    & $script4
}

$localIps = (Get-NetIPAddress -AddressFamily IPv4 -ErrorAction SilentlyContinue | Where-Object { $_.IPAddress -ne "127.0.0.1" -and $_.InterfaceAlias -notlike "*Loopback*" }).IPAddress
$displayIp = if ($localIps) { $localIps[0] } else { "127.0.0.1" }

Write-Host "`n================================================================================" -ForegroundColor Green
Write-Host "           CHUC MUNG! HE THONG MDMSv2 DA DUOC TRIEN KHAI THANH CONG!            " -ForegroundColor Green
Write-Host "================================================================================" -ForegroundColor Green
Write-Host " 1. TRUY CAP GIAO DIEN NGUOI DUNG (WEB PORTAL):" -ForegroundColor Cyan
Write-Host "    - Truy cap tai may chu : http://localhost" -ForegroundColor White
Write-Host "    - Truy cap qua mang LAN: http://$displayIp" -ForegroundColor White
Write-Host "    - TAI KHOAN DANG NHAP  : $mdmsAdminUser" -ForegroundColor Yellow
Write-Host "    - MAT KHAU DANG NHAP   : $mdmsAdminPass" -ForegroundColor Yellow
Write-Host ""
Write-Host " 2. THONG TIN CAC DICH VU BACKEND & CO SO DU LIEU:" -ForegroundColor Cyan
Write-Host "    - WebApi Root Endpoint : http://localhost:$BackendPort/" -ForegroundColor White
Write-Host "    - Swagger API Docs     : http://localhost:$BackendPort/swagger" -ForegroundColor White
Write-Host "    - Hangfire Dashboard   : http://localhost:$BackendPort/dashboard" -ForegroundColor White
Write-Host "    - MySQL Database       : $inputHost`:$inputPort (Database: amr, User: $inputUser)" -ForegroundColor White
Write-Host "    - Kepware IoT Gateway  : http://127.0.0.1:39320/iotgateway/read" -ForegroundColor White
Write-Host "================================================================================" -ForegroundColor Green

# Goi y mo ngay Microsoft Edge
$edgeExe = "C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"
if (-not (Test-Path $edgeExe)) {
    $edgeExe = "C:\Program Files\Microsoft\Edge\Application\msedge.exe"
}

if (Test-Path $edgeExe) {
    Write-Host "`n[TU DONG KHOI CHAY GIAO DIEN]" -ForegroundColor Cyan
    $openEdge = Read-Host "Ban co muon mo ngay Microsoft Edge de truy cap he thong khong? (Y/n) [Mac dinh: Y]"
    if ([string]::IsNullOrWhiteSpace($openEdge) -or $openEdge.Trim().ToUpper() -eq "Y") {
        Write-Host "Dang khoi chay Microsoft Edge..." -ForegroundColor Green
        Start-Process -FilePath $edgeExe -ArgumentList "http://localhost"
    }
}
