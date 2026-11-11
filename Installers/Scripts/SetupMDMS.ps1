<#
.SYNOPSIS
    Kich ban Wizard cai dat toan dien MDMSv2 tren Windows Server 2019 (All-in-One).
.DESCRIPTION
    Kich ban huong dan va thuc hien tu dong toan bo quy trinh cai dat moi:
    [Buoc 1] Kich hoat IIS, URL Rewrite 2.1, ARR 3.0, ARR Proxy, Java JRE, Edge, LibreOffice va Firewall.
    [Buoc 2] Kiem tra/Huong dan cai dat MySQL Server 8.0/8.4, xac thuc ket noi MySQL,
             khoi tao CSDL 'amr' tu amr_full.sql, chay day du cac script migration Phase 1
             (toi uu Wide-Table Snapshot, TimeSeries Partition 12 thang, bo 9 cot *Unit)
             va tuy chon nap Master Data, cap nhat appsettings.Production.json.
    [Buoc 3] Hoi cau hinh tai khoan Quan tri MDMS (UserName, Password), ma hoa PBKDF2
             va dong bo an toan vao CSDL amr.users & systemconfig (khong trung key).
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

$backendDir  = Join-Path $RootDeployPath "Backend"
$frontendDir = Join-Path $RootDeployPath "Frontend"
$databaseDir = Join-Path $RootDeployPath "Database"
$scriptsDir  = Join-Path $RootDeployPath "Scripts"
$mysqlDir    = Join-Path $RootDeployPath "MySQL"
$kepwareDir  = Join-Path $RootDeployPath "Kepware"
$prereqDir   = Join-Path $RootDeployPath "Prerequisites"

# Ham ghi file JSON UTF-8 khong BOM
function Write-JsonUtf8NoBom {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)]$Object
    )
    $json = $Object | ConvertTo-Json -Depth 10
    $utf8NoBom = New-Object System.Text.UTF8Encoding $false
    [System.IO.File]::WriteAllText($Path, $json, $utf8NoBom)
}

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

# Ham khoi dong MySQL Service neu da cai dat
function Start-MySqlServiceIfStopped {
    foreach ($svcName in @("MySQL80", "MySQL84", "MySQL")) {
        $svc = Get-Service -Name $svcName -ErrorAction SilentlyContinue
        if ($svc) {
            if ($svc.Status -ne "Running") {
                Write-Host "   Dang khoi dong dich vu $svcName..." -ForegroundColor Gray
                Start-Service -Name $svcName -ErrorAction SilentlyContinue
                Start-Sleep -Seconds 3
            }
            return $true
        }
    }
    return $false
}

# Ham thuc thi file SQL an toan bang MYSQL_PWD va RedirectStandardInput
# Tranh loi escape mat khau qua file .bat va ho tro day du DELIMITER $$ / multi-line
function Invoke-MySqlFile {
    param(
        [Parameter(Mandatory = $true)][string]$MySqlExe,
        [Parameter(Mandatory = $true)][string]$HostName,
        [Parameter(Mandatory = $true)][string]$Port,
        [Parameter(Mandatory = $true)][string]$UserName,
        [Parameter(Mandatory = $true)][string]$Password,
        [Parameter(Mandatory = $true)][string]$SqlFilePath,
        [string]$Database = ""
    )

    if (-not (Test-Path $SqlFilePath)) {
        throw "Khong tim thay file SQL: $SqlFilePath"
    }

    $mysqlArgs = @(
        "`"--host=$HostName`"",
        "`"--port=$Port`"",
        "`"--user=$UserName`"",
        "`"--default-character-set=utf8mb4`""
    )
    if (-not [string]::IsNullOrWhiteSpace($Database)) {
        $mysqlArgs += "`"$Database`""
    }

    $prevPwd = $env:MYSQL_PWD
    $env:MYSQL_PWD = $Password
    try {
        $proc = Start-Process -FilePath $MySqlExe `
            -ArgumentList $mysqlArgs `
            -RedirectStandardInput $SqlFilePath `
            -NoNewWindow -Wait -PassThru
        if (-not $proc -or $proc.ExitCode -ne 0) {
            $exitCode = if ($proc) { $proc.ExitCode } else { -1 }
            throw "Thuc thi script SQL '$(Split-Path $SqlFilePath -Leaf)' that bai (ExitCode: $exitCode)."
        }
    } finally {
        if ($null -ne $prevPwd) {
            $env:MYSQL_PWD = $prevPwd
        } else {
            Remove-Item Env:MYSQL_PWD -ErrorAction SilentlyContinue
        }
    }
}

# Ham thuc thi cau truy van SQL tra ve gia tri don (dung de test ket noi & verify schema)
function Invoke-MySqlScalar {
    param(
        [Parameter(Mandatory = $true)][string]$MySqlExe,
        [Parameter(Mandatory = $true)][string]$HostName,
        [Parameter(Mandatory = $true)][string]$Port,
        [Parameter(Mandatory = $true)][string]$UserName,
        [Parameter(Mandatory = $true)][string]$Password,
        [Parameter(Mandatory = $true)][string]$Query,
        [string]$Database = ""
    )

    $tempSql = Join-Path $env:TEMP ("mdms_q_" + [guid]::NewGuid().ToString("N") + ".sql")
    $tempOut = Join-Path $env:TEMP ("mdms_o_" + [guid]::NewGuid().ToString("N") + ".txt")
    $tempErr = Join-Path $env:TEMP ("mdms_e_" + [guid]::NewGuid().ToString("N") + ".txt")
    $prevPwd = $env:MYSQL_PWD
    try {
        $utf8NoBom = New-Object System.Text.UTF8Encoding $false
        [System.IO.File]::WriteAllText($tempSql, $Query, $utf8NoBom)

        $mysqlArgs = @(
            "`"--host=$HostName`"",
            "`"--port=$Port`"",
            "`"--user=$UserName`"",
            "`"--default-character-set=utf8mb4`"",
            "-N", "-s"
        )
        if (-not [string]::IsNullOrWhiteSpace($Database)) {
            $mysqlArgs += "`"$Database`""
        }

        $env:MYSQL_PWD = $Password
        $proc = Start-Process -FilePath $MySqlExe `
            -ArgumentList $mysqlArgs `
            -RedirectStandardInput $tempSql `
            -RedirectStandardOutput $tempOut `
            -RedirectStandardError $tempErr `
            -NoNewWindow -Wait -PassThru

        if (-not $proc -or $proc.ExitCode -ne 0) {
            $errText = if (Test-Path $tempErr) { (Get-Content -Path $tempErr -Raw -ErrorAction SilentlyContinue).Trim() } else { "" }
            throw "Truy van MySQL that bai (ExitCode: $($proc.ExitCode)): $errText"
        }

        if (Test-Path $tempOut) {
            return ((Get-Content -Path $tempOut -Raw -ErrorAction SilentlyContinue) -replace "`r", "").Trim()
        }
        return ""
    } finally {
        if ($null -ne $prevPwd) {
            $env:MYSQL_PWD = $prevPwd
        } else {
            Remove-Item Env:MYSQL_PWD -ErrorAction SilentlyContinue
        }
        Remove-Item -Path $tempSql, $tempOut, $tempErr -Force -ErrorAction SilentlyContinue
    }
}

# ==============================================================================
# [BUOC 1] CAI DAT PREREQUISITES (IIS, URL REWRITE, ARR, JAVA, EDGE, LIBREOFFICE, FIREWALL)
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
    if (Start-MySqlServiceIfStopped) {
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
            Start-MySqlServiceIfStopped | Out-Null
        }
    } else {
        Write-Host "   -> Chua co file cai dat MySQL trong thu muc: $mysqlDir" -ForegroundColor Yellow
        Write-Host "      Vui long cai dat MySQL Server 8.0+ truoc khi tiep tuc." -ForegroundColor White
        Read-Host "   Sau khi hoan tat cai dat MySQL va khoi dong dich vu, nhan Enter de tiep tuc"
        Start-MySqlServiceIfStopped | Out-Null
    }
}

# 2.2 Tim mysql.exe & thu thap thong tin dang nhap MySQL co xac thuc
$mysqlExe = Find-MySqlExe
while (-not $mysqlExe) {
    Write-Host "`n   [CANH BAO] Khong tim thay mysql.exe trong PATH hoac thu muc mac dinh!" -ForegroundColor Red
    $manualMysql = Read-Host "   Vui long nhap duong dan day du toi mysql.exe (vi du: C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe)"
    if (-not [string]::IsNullOrWhiteSpace($manualMysql) -and (Test-Path $manualMysql)) {
        $mysqlExe = (Resolve-Path $manualMysql).Path
    } else {
        Write-Host "   Duong dan '$manualMysql' khong ton tai. Vui long kiem tra lai!" -ForegroundColor Red
    }
}
Write-Host "   Tim thay cong cu MySQL Client: $mysqlExe" -ForegroundColor Gray

Write-Host "`n   --- CAU HINH KET NOI MYSQL DATABASE ---" -ForegroundColor Cyan
$inputHost = ""
$inputPort = ""
$inputUser = ""
$mysqlPassword = ""
$dbConnected = $false

while (-not $dbConnected) {
    $inputHost = Read-Host "   Nhap dia chi may chu MySQL [Mac dinh: 127.0.0.1]"
    if ([string]::IsNullOrWhiteSpace($inputHost)) { $inputHost = "127.0.0.1" } else { $inputHost = $inputHost.Trim() }

    $inputPort = Read-Host "   Nhap cong ket noi MySQL [Mac dinh: 3306]"
    if ([string]::IsNullOrWhiteSpace($inputPort)) { $inputPort = "3306" } else { $inputPort = $inputPort.Trim() }

    $inputUser = Read-Host "   Nhap tai khoan quan tri MySQL [Mac dinh: root]"
    if ([string]::IsNullOrWhiteSpace($inputUser)) { $inputUser = "root" } else { $inputUser = $inputUser.Trim() }

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

    Write-Host "   Dang kiem tra xac thuc ket noi toi MySQL ($inputHost`:$inputPort)..." -ForegroundColor Gray
    try {
        $ping = Invoke-MySqlScalar -MySqlExe $mysqlExe -HostName $inputHost -Port $inputPort -UserName $inputUser -Password $mysqlPassword -Query "SELECT 1;"
        if ($ping -eq "1") {
            $dbConnected = $true
            Write-Host "   -> [PASS] Ket noi & xac thuc MySQL thanh cong!" -ForegroundColor Green
        }
    } catch {
        Write-Host "   -> [LOI] Khong the dang nhap MySQL: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "      Vui long kiem tra lai Host/Port/User/Mat khau va nhap lai." -ForegroundColor Yellow
    }
}

# 2.3 Khoi tao Co so Du lieu 'amr' + Chay day du Migration Phase 1
$fileFull = Join-Path $databaseDir "amr_full.sql"
$fileInit = Join-Path $databaseDir "init_amr_database.sql"
$dumpFile = if (Test-Path $fileFull) { $fileFull } elseif (Test-Path $fileInit) { $fileInit } else { $null }

if (-not $dumpFile) {
    throw "Khong tim thay file SQL khoi tao (amr_full.sql) trong thu muc: $databaseDir"
}

$existingTablesStr = Invoke-MySqlScalar `
    -MySqlExe $mysqlExe -HostName $inputHost -Port $inputPort -UserName $inputUser -Password $mysqlPassword `
    -Query "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'amr';"
$existingTableCount = 0
[int]::TryParse($existingTablesStr, [ref]$existingTableCount) | Out-Null

$shouldRunBaseDump = $true
if ($existingTableCount -gt 0) {
    Write-Host "`n   [CANH BAO] Phat hien CSDL 'amr' da ton tai voi $existingTableCount bang!" -ForegroundColor Yellow
    $confirmReset = Read-Host "   Chay lai amr_full.sql se XOA SACH du lieu cu trong 'amr'. Ban co chac chan muon reset trang DB? (y/N) [Mac dinh: N]"
    if ($confirmReset.Trim().ToUpper() -ne "Y") {
        $shouldRunBaseDump = $false
        Write-Host "   -> Giu nguyen CSDL 'amr' hien co va chi chay cac migration dong bo schema." -ForegroundColor Cyan
    }
}

if ($shouldRunBaseDump) {
    $dumpFileName = Split-Path $dumpFile -Leaf
    Write-Host "   Dang khoi tao co so du lieu 'amr' tu tap tin: $dumpFileName..." -ForegroundColor Yellow
    Invoke-MySqlFile `
        -MySqlExe $mysqlExe -HostName $inputHost -Port $inputPort -UserName $inputUser -Password $mysqlPassword `
        -SqlFilePath $dumpFile
    Write-Host "   -> [PASS] Da import thanh cong co so du lieu goc 'amr' tu $dumpFileName!" -ForegroundColor Green
}

# Thuc thi tuan tu cac migration theo ngay (yyyyMMdd_*.sql), bo qua font/encoding scripts
$migrationFiles = Get-ChildItem -Path $databaseDir -Filter "*.sql" -File -ErrorAction SilentlyContinue | Where-Object {
    ($_.Name -match "^\d{8}_") -and
    ($_.Name -notlike "*font*") -and
    ($_.Name -notlike "*encoding*")
} | Sort-Object Name

foreach ($mFile in $migrationFiles) {
    Write-Host "   -> Dang thuc thi migration schema: $($mFile.Name)..." -ForegroundColor Yellow
    Invoke-MySqlFile `
        -MySqlExe $mysqlExe -HostName $inputHost -Port $inputPort -UserName $inputUser -Password $mysqlPassword `
        -SqlFilePath $mFile.FullName -Database "amr"
    Write-Host "      [OK] Hoan tat $($mFile.Name)" -ForegroundColor Green
}

# Kiem tra toan ven schema Phase 1 sau khi migrate
$verifyPhase1Sql = @"
SELECT CONCAT(
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'amr' AND LOWER(TABLE_NAME) IN (
    'measurementpointcurrentsnapshots',
    'measurementpointtimeseries',
    'measurementpointhalfhouroperatingreports',
    'measurementpointhalfhourenergyreports',
    'measurementpointdailyenergyreports',
    'measurementpointevents',
    'measurementpointalerts',
    'systememailconfigs',
    'notificationtemplates',
    'statementpdftemplates'
  )), '|',
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'amr' AND LOWER(TABLE_NAME) = 'measurementpointhalfhouroperatingreports' AND COLUMN_NAME LIKE '%Unit'), '|',
  (SELECT COUNT(*) FROM INFORMATION_SCHEMA.PARTITIONS WHERE TABLE_SCHEMA = 'amr' AND LOWER(TABLE_NAME) = 'measurementpointtimeseries' AND PARTITION_NAME IS NOT NULL)
);
"@
$schemaVerify = Invoke-MySqlScalar `
    -MySqlExe $mysqlExe -HostName $inputHost -Port $inputPort -UserName $inputUser -Password $mysqlPassword `
    -Query $verifyPhase1Sql -Database "amr"
$parts = $schemaVerify -split "\|"
if ($parts.Count -eq 3) {
    $coreTblCnt = [int]$parts[0]
    $opUnitCnt  = [int]$parts[1]
    $tsPartCnt  = [int]$parts[2]
    if ($coreTblCnt -lt 10 -or $opUnitCnt -gt 0 -or $tsPartCnt -lt 2) {
        throw "Kiem tra schema Phase 1 that bai (CoreTables=$coreTblCnt/10, OpUnitCols=$opUnitCnt, TimeSeriesPartitions=$tsPartCnt)."
    }
    Write-Host "   -> [PASS] Xac minh schema Phase 1: $coreTblCnt/10 bang chuan, $opUnitCnt cot Unit du thua, $tsPartCnt partition TimeSeries san sang!" -ForegroundColor Green
}

# Tu dong kiem tra va nap Master Data (Khach hang, So GCS, Diem do, Tags)
$candidateMaster = @(
    (Join-Path $databaseDir "amr_master_data.sql"),
    (Join-Path $scriptsDir "amr_master_data.sql"),
    (Join-Path $RootDeployPath "amr_master_data.sql"),
    (Join-Path $databaseDir "amr_data.sql")
)
$chosenMasterFile = $null
foreach ($cm in $candidateMaster) {
    if (Test-Path $cm) { $chosenMasterFile = (Resolve-Path $cm).Path; break }
}

if ($chosenMasterFile) {
    Write-Host "`n   --- NAP DU LIEU CO SO (KHACH HANG, SO GCS, DIEM DO, LIEN KET DU LIEU) ---" -ForegroundColor Cyan
    Write-Host "   Tim thay tap tin Master Data: $(Split-Path $chosenMasterFile -Leaf)" -ForegroundColor Green
    $importMaster = Read-Host "   Ban co muon Import danh muc Khach hang & Diem do thuc te vao moi truong trang? (Y/n) [Mac dinh: Y]"
    if ([string]::IsNullOrWhiteSpace($importMaster) -or $importMaster.Trim().ToUpper() -eq "Y") {
        $importScript = Join-Path $scriptsDir "Import_MasterData.ps1"
        if (-not (Test-Path $importScript)) {
            $importScript = Join-Path $PSScriptRoot "Import_MasterData.ps1"
        }
        if (Test-Path $importScript) {
            Write-Host "   Dang thuc thi Import_MasterData.ps1..." -ForegroundColor Cyan
            & $importScript -MySqlHost $inputHost -MySqlPort ([int]$inputPort) -MySqlUser $inputUser -MySqlPassword $mysqlPassword -DataFile $chosenMasterFile
        } else {
            Write-Warning "   Khong tim thay script Import_MasterData.ps1 tai: $importScript"
        }
    }
} else {
    Write-Host "`n   [LUU Y] Chua tim thay tap tin 'amr_master_data.sql' trong thu muc: $databaseDir" -ForegroundColor Yellow
    Write-Host "   CSDL 'amr' vua duoc tao la KHUNG BANG TRANG (chua co Khach hang va Diem do)." -ForegroundColor Gray
    Write-Host "   Sau khi copy amr_master_data.sql vao $databaseDir, ban co the chay:" -ForegroundColor White
    Write-Host "   cd $scriptsDir; .\Import_MasterData.ps1 de nap du lieu bat cu luc nao!" -ForegroundColor Cyan
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

    Write-JsonUtf8NoBom -Path $appsettingsPath -Object $configContent
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
if ([string]::IsNullOrWhiteSpace($mdmsAdminUser)) { $mdmsAdminUser = "admin" } else { $mdmsAdminUser = $mdmsAdminUser.Trim() }

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

    Write-JsonUtf8NoBom -Path $appsettingsPath -Object $configContent
    Write-Host "   -> [PASS] Da ghi nhan tai khoan admin vao cau hinh." -ForegroundColor Green
}

# Dong bo vao bang users & bo sung systemconfig con thieu trong DB amr (khong trung key)
$safeAdminUser = $mdmsAdminUser -replace "'", "''"
$safeAdminHash = $mdmsHashedPassword -replace "'", "''"
$updateSqlFile = Join-Path $env:TEMP ("update_admin_" + [guid]::NewGuid().ToString("N") + ".sql")
$sqlTemplate = @'
USE `amr`;
SET SQL_SAFE_UPDATES = 0;

UPDATE `users`
SET `UserName` = '{0}',
    `Password` = '{1}',
    `RoleName` = 'SUPERADMIN',
    `Status` = 'ACTIVE',
    `IsLocked` = 0,
    `CountLocked` = 0,
    `ModifiedTime` = NOW()
WHERE (`Id` = 921946681335811 OR `UserName` = 'admin' OR `UserName` = '{0}')
  AND `Deleted` = 0;

INSERT INTO `users` (`Id`, `UserName`, `Password`, `FullName`, `Email`, `PhoneNumber`, `Address`, `CustomerCode`, `Description`, `Status`, `CountLocked`, `TimeLocked`, `IsLocked`, `RoleName`, `CreatedTime`, `ModifiedTime`, `Deleted`)
SELECT 921946681335811, '{0}', '{1}', 'Administrator', 'admin@mdms.local', '', '', '', 'System Administrator', 'ACTIVE', 0, '2026-01-01 00:00:00', 0, 'SUPERADMIN', NOW(), NOW(), 0
FROM DUAL
WHERE NOT EXISTS (
    SELECT 1 FROM (SELECT `Id`, `UserName`, `Deleted` FROM `users`) u
    WHERE u.`Deleted` = 0 AND (u.`Id` = 921946681335811 OR u.`UserName` = '{0}')
);

INSERT INTO `systemconfig` (`Id`, `Key`, `Value`)
SELECT d.`Id`, d.`Key`, d.`Value`
FROM (
    SELECT 1425540916019200 AS `Id`, 'LOCKLOGIN' AS `Key`, '5' AS `Value`
    UNION ALL SELECT 1425540916174848, 'NUMBEROFINCORRECTENTRIES', '5'
    UNION ALL SELECT 1431228722962432, 'SYSTEMADMINISTRATIONFULLNAME', 'Administrator'
    UNION ALL SELECT 1431228722987008, 'SYSTEMADMINISTRATIONPHONE', '0901234567'
    UNION ALL SELECT 1431228722995200, 'SYSTEMADMINISTRATIONEMAIL', 'admin@mdms.vn'
    UNION ALL SELECT 1431228722995201, 'CUSTOMERCAREFULLNAME', 'CSKH'
    UNION ALL SELECT 1431228722995202, 'CUSTOMERCAREPHONE', '19001006'
    UNION ALL SELECT 1431228722995203, 'CUSTOMERCAREEMAIL', 'cskh@mdms.vn'
) d
WHERE NOT EXISTS (
    SELECT 1 FROM (SELECT `Key` FROM `systemconfig`) sc WHERE sc.`Key` = d.`Key`
);

SET SQL_SAFE_UPDATES = 1;
'@
$sqlContent = $sqlTemplate -f $safeAdminUser, $safeAdminHash
$utf8NoBom = New-Object System.Text.UTF8Encoding $false
[System.IO.File]::WriteAllText($updateSqlFile, $sqlContent, $utf8NoBom)
try {
    Invoke-MySqlFile `
        -MySqlExe $mysqlExe -HostName $inputHost -Port $inputPort -UserName $inputUser -Password $mysqlPassword `
        -SqlFilePath $updateSqlFile -Database "amr"
    Write-Host "   -> [PASS] Da cap nhat tai khoan '$mdmsAdminUser' va xac minh systemconfig trong DB amr!" -ForegroundColor Green
} finally {
    Remove-Item -Path $updateSqlFile -Force -ErrorAction SilentlyContinue
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
    $kepService = $null
    foreach ($ksName in @("KEPServerEXV6", "KEPServerEX")) {
        $kepService = Get-Service -Name $ksName -ErrorAction SilentlyContinue
        if ($kepService) { break }
    }

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

$sampleOpf = Join-Path $kepwareDir "Meter Simulation.opf"
if (Test-Path $sampleOpf) {
    Write-Host "   * File du an Kepware mau co san tai: $sampleOpf" -ForegroundColor Gray
    Write-Host "     (Luu y: Trong IoT Gateway > Status, khuyen nghi dung Wide Format hoac Narrow Format voi Publish Rate 5000-10000ms va Send Every Scan = False)." -ForegroundColor Gray
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
    throw "Khong tim thay file 2_Setup_Backend_Service.ps1!"
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
    throw "Khong tim thay file 3_Setup_Frontend_IIS.ps1!"
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
    & $script4 -BackendPort $BackendPort -FrontendPort $FrontendPort
}

$localIps = (Get-NetIPAddress -AddressFamily IPv4 -ErrorAction SilentlyContinue | Where-Object { $_.IPAddress -ne "127.0.0.1" -and $_.InterfaceAlias -notlike "*Loopback*" }).IPAddress
$displayIp = if ($localIps) { $localIps[0] } else { "127.0.0.1" }

Write-Host "`n================================================================================" -ForegroundColor Green
Write-Host "           CHUC MUNG! HE THONG MDMSv2 DA DUOC TRIEN KHAI THANH CONG!            " -ForegroundColor Green
Write-Host "================================================================================" -ForegroundColor Green
Write-Host " 1. TRUY CAP GIAO DIEN NGUOI DUNG (WEB PORTAL):" -ForegroundColor Cyan
Write-Host "    - Truy cap tai may chu : http://localhost:$FrontendPort" -ForegroundColor White
Write-Host "    - Truy cap qua mang LAN: http://$displayIp`:$FrontendPort" -ForegroundColor White
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
        Start-Process -FilePath $edgeExe -ArgumentList "http://localhost:$FrontendPort"
    }
}
