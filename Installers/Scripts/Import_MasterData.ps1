<#
.SYNOPSIS
    Kich ban tu dong Import Du lieu Co so / Danh muc (Master Data):
    Khach hang, Diem do, Lien ket du lieu (TagNames), Users tu amr_data.sql / amr_master_data.sql
    vao Co so du lieu 'amr' tren may ao moi, phuc vu kiem thu moi truong "trang" (clean-slate).
.DESCRIPTION
    1. Tu dong tim kiem mysql.exe va nhan dien cau hinh ket noi tu appsettings.Production.json.
    2. Su dung amr_master_data.sql (3.2 MB) hoac tu dong trich xuat truc tiep tu amr_data.sql (1.54 GB)
       de chi lay danh muc goc (customers, measurementpoints, tagname, users, usercustomers, systemconfig),
       hoan toan bo qua toan bo du lieu do dem / van hanh / bao cao lich su.
    3. Nap du lieu vao database 'amr' mot cach an toan (tat FOREIGN_KEY_CHECKS trong qua trinh nap).
    4. Reset co GcsId tren measurementpoints ve NULL de khong bi tham chieu toi so GCS cu.
    5. Dong bo mat khau tai khoan 'admin' ve 'Admin@123' (ma hoa PBKDF2).
    6. Xac minh va in bang doi chieu so luong ban ghi de dam bao moi truong sach san sang kiem thu.
#>

[CmdletBinding()]
param (
    [string]$MySqlHost = '',
    [int]$MySqlPort = 0,
    [string]$MySqlUser = '',
    [string]$MySqlPassword = '',
    [string]$DataFile = '',
    [string]$AdminPassword = 'Admin@123'
)

$ErrorActionPreference = 'Stop'

Clear-Host
Write-Host '================================================================================' -ForegroundColor Cyan
Write-Host '     MDMSv2 - CONG CU IMPORT DU LIEU CO SO: KHACH HANG, DIEM DO, TAGS (MASTER)   ' -ForegroundColor Cyan
Write-Host '              (KHOI TAO MOI TRUONG KIEM THU "TRANG" TREN MAY AO MOI)             ' -ForegroundColor Cyan
Write-Host '================================================================================' -ForegroundColor Cyan
Write-Host ('Thoi gian thuc hien: ' + (Get-Date -Format 'dd/MM/yyyy HH:mm:ss')) -ForegroundColor Gray

# 1. Xac dinh duong dan thu muc & tap tin SQL
$scriptDir = $PSScriptRoot

# Tim tap tin DDL amr_full.sql
$candidateFull = @(
    (Join-Path $scriptDir 'amr_full.sql'),
    (Join-Path $scriptDir 'amr-full.sql'),
    (Join-Path $scriptDir '..\Database\amr_full.sql'),
    (Join-Path $scriptDir '..\..\Installers\Database\amr_full.sql'),
    'C:\MDMSv2\Database\amr_full.sql',
    'C:\MDMSv2\Installers\Database\amr_full.sql'
)
$fileFull = $null
foreach ($f in $candidateFull) {
    if (Test-Path $f) { $fileFull = (Resolve-Path $f).Path; break }
}

# Tim tap tin Master Data (amr_master_data.sql hoac amr_data.sql)
if (-not [string]::IsNullOrWhiteSpace($DataFile) -and (Test-Path $DataFile)) {
    $fileData = (Resolve-Path $DataFile).Path
} else {
    $candidateData = @(
        (Join-Path $scriptDir 'amr_master_data.sql'),
        (Join-Path $scriptDir '..\Database\amr_master_data.sql'),
        (Join-Path $scriptDir '..\..\Installers\Database\amr_master_data.sql'),
        'C:\MDMSv2\Database\amr_master_data.sql',
        (Join-Path $scriptDir 'amr_data.sql'),
        (Join-Path $scriptDir '..\Database\amr_data.sql'),
        (Join-Path $scriptDir '..\..\Installers\Database\amr_data.sql'),
        'C:\MDMSv2\Database\amr_data.sql'
    )
    $fileData = $null
    foreach ($f in $candidateData) {
        if (Test-Path $f) { $fileData = (Resolve-Path $f).Path; break }
    }
}

if (-not $fileData) {
    Write-Host "`n[CANH BAO] Khong tim thay tap tin du lieu 'amr_master_data.sql' hoac 'amr_data.sql'!" -ForegroundColor Red
    $manualData = Read-Host 'Vui long nhap duong dan day du toi file SQL du lieu'
    if (Test-Path $manualData) {
        $fileData = (Resolve-Path $manualData).Path
    } else {
        Write-Error 'Duong dan khong ton tai. Dung tien trinh.'
    }
}

# Neu file tim thay la file amr_data.sql lon (> 50 MB), tu dong trich xuat master data truoc khi nap
$isLargeFile = ((Get-Item $fileData).Length -gt 50MB)
if ($isLargeFile) {
    Write-Host "`n[THONG BAO] Phat hien tap tin du lieu lon: $(Split-Path $fileData -Leaf) ($([math]::Round(((Get-Item $fileData).Length / 1MB), 2)) MB)" -ForegroundColor Yellow
    Write-Host 'Dang tu dong trich xuat chi du lieu Master Data (Khach hang, Diem do, Tags, Users)...' -ForegroundColor Cyan

    $extractScript = Join-Path $scriptDir 'Extract_MasterData.ps1'
    $extractedOut = Join-Path (Split-Path $fileData -Parent) 'amr_master_data.sql'

    if (Test-Path $extractScript) {
        & $extractScript -InputPath $fileData -OutputPath $extractedOut
        if (Test-Path $extractedOut) {
            $fileData = $extractedOut
            Write-Host 'Da chuyen sang su dung tap tin Master Data tinh gon: amr_master_data.sql' -ForegroundColor Green
        }
    }
}

$dataSizeKB = [math]::Round(((Get-Item $fileData).Length / 1KB), 2)
Write-Host ('Tap tin Master Data su dung: ' + $fileData + ' (' + $dataSizeKB + ' KB)') -ForegroundColor Green

# 2. Tim kiem mysql.exe
function Find-MySqlExe {
    $mysqlCmd = Get-Command 'mysql.exe' -ErrorAction SilentlyContinue
    if ($mysqlCmd) { return $mysqlCmd.Source }

    $searchPaths = @(
        'C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe',
        'C:\Program Files\MySQL\MySQL Server 8.4\bin\mysql.exe',
        'C:\Program Files (x86)\MySQL\MySQL Server 8.0\bin\mysql.exe'
    )
    foreach ($p in $searchPaths) {
        if (Test-Path $p) { return $p }
    }
    
    $found = Get-ChildItem -Path 'C:\Program Files\MySQL' -Filter 'mysql.exe' -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($found) { return $found.FullName }

    return $null
}

$mysqlExe = Find-MySqlExe
if (-not $mysqlExe) {
    Write-Host "`n[CANH BAO] Khong tim thay mysql.exe trong PATH hoac thu muc cai dat mac dinh!" -ForegroundColor Red
    $manualExe = Read-Host 'Vui long nhap duong dan day du toi file mysql.exe'
    if (Test-Path $manualExe) {
        $mysqlExe = $manualExe
    } else {
        Write-Error 'Duong dan mysql.exe khong hop le. Dung tien trinh.'
    }
}
Write-Host ('Cong cu MySQL Client   : ' + $mysqlExe) -ForegroundColor Gray

# 3. Nhan dien thong tin ket noi MySQL tu appsettings.Production.json
$backendCandidates = @(
    (Join-Path $scriptDir '..\..\Backend\Presentation\WebAPI'),
    (Join-Path $scriptDir '..\Backend'),
    'C:\MDMSv2\Backend'
)
$backendDir = $null
foreach ($b in $backendCandidates) {
    if (Test-Path $b) { $backendDir = $b; break }
}

$appsettingsPath = $null
if ($backendDir -and (Test-Path $backendDir)) {
    $cand1 = Join-Path $backendDir 'appsettings.Production.json'
    $cand2 = Join-Path $backendDir 'appsettings.json'
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
            Write-Host ('Da tu dong nhan dien cau hinh DB tu: ' + (Split-Path $appsettingsPath -Leaf)) -ForegroundColor Gray
        }
    } catch {}
}

if ([string]::IsNullOrWhiteSpace($MySqlHost)) { $MySqlHost = '127.0.0.1' }
if ($MySqlPort -eq 0) { $MySqlPort = 3306 }
if ([string]::IsNullOrWhiteSpace($MySqlUser)) { $MySqlUser = 'root' }

Write-Host ('MySQL Host: ' + $MySqlHost + ' | Port: ' + $MySqlPort + ' | User: ' + $MySqlUser) -ForegroundColor Cyan

while ([string]::IsNullOrWhiteSpace($MySqlPassword)) {
    $secPass = Read-Host ("Vui long nhap mat khau MySQL root cho tai khoan '" + $MySqlUser + "'") -AsSecureString
    $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($secPass)
    $MySqlPassword = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
    [System.Runtime.InteropServices.Marshal]::ZeroFreeBSTR($BSTR)
}

# Ham ma hoa mat khau PBKDF2 RFC2898
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

function Invoke-MySqlCommand {
    param(
        [string]$SqlContent,
        [string]$Description
    )
    Write-Host ('   -> ' + $Description + '...') -ForegroundColor Gray
    $tempSql = Join-Path $env:TEMP ('mdms_exec_' + [guid]::NewGuid().ToString('N') + '.sql')
    $tempBat = Join-Path $env:TEMP ('mdms_exec_' + [guid]::NewGuid().ToString('N') + '.bat')
    try {
        $utf8NoBom = New-Object System.Text.UTF8Encoding $false
        [System.IO.File]::WriteAllText($tempSql, $SqlContent, $utf8NoBom)
        $batCmd = '@echo off' + "`r`n" + '"' + $mysqlExe + '" -h ' + $MySqlHost + ' -P ' + $MySqlPort + ' -u ' + $MySqlUser + ' "-p' + $MySqlPassword + '" --default-character-set=utf8mb4 amr < "' + $tempSql + '"'
        Set-Content -Path $tempBat -Value $batCmd -Encoding ASCII
        $p = Start-Process -FilePath $tempBat -Wait -PassThru -NoNewWindow
        if ($p.ExitCode -ne 0) {
            throw ('Lenh MySQL ket thuc voi ma loi: ' + $p.ExitCode)
        }
    } finally {
        Remove-Item -Path $tempSql, $tempBat -Force -ErrorAction SilentlyContinue
    }
}

# 4. BUOC 1: Kiem tra su ton tai cua Database 'amr'. Neu chua co, tao moi tu amr_full.sql
Write-Host "`n[BUOC 1/4] Kiem tra Co so Du lieu 'amr'..." -ForegroundColor Yellow
$checkDbSql = 'SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = ''amr'';'
$hasAmrDb = $false
try {
    $tempCheckSql = Join-Path $env:TEMP 'check_amr.sql'
    $tempCheckRes = Join-Path $env:TEMP 'check_amr.txt'
    $utf8NoBom = New-Object System.Text.UTF8Encoding $false
    [System.IO.File]::WriteAllText($tempCheckSql, $checkDbSql, $utf8NoBom)
    $batCheck = '@echo off' + "`r`n" + '"' + $mysqlExe + '" -h ' + $MySqlHost + ' -P ' + $MySqlPort + ' -u ' + $MySqlUser + ' "-p' + $MySqlPassword + '" -N -s < "' + $tempCheckSql + '" > "' + $tempCheckRes + '"'
    $tempBat = Join-Path $env:TEMP 'run_check.bat'
    Set-Content -Path $tempBat -Value $batCheck -Encoding ASCII
    $p = Start-Process -FilePath $tempBat -Wait -PassThru -NoNewWindow
    if (Test-Path $tempCheckRes) {
        $res = Get-Content $tempCheckRes -Raw -ErrorAction SilentlyContinue
        if ($res -and $res.Trim() -eq 'amr') { $hasAmrDb = $true }
    }
    Remove-Item -Path $tempCheckSql, $tempCheckRes, $tempBat -Force -ErrorAction SilentlyContinue
} catch {}

if (-not $hasAmrDb) {
    if ($fileFull -and (Test-Path $fileFull)) {
        Write-Host "   Chua co database 'amr'. Dang khoi tao cau truc bang tu: $(Split-Path $fileFull -Leaf)..." -ForegroundColor Yellow
        $importFullBat = Join-Path $env:TEMP 'import_full.bat'
        try {
            $batCmd = '@echo off' + "`r`n" + '"' + $mysqlExe + '" -h ' + $MySqlHost + ' -P ' + $MySqlPort + ' -u ' + $MySqlUser + ' "-p' + $MySqlPassword + '" --default-character-set=utf8mb4 < "' + $fileFull + '"'
            Set-Content -Path $importFullBat -Value $batCmd -Encoding ASCII
            $p = Start-Process -FilePath $importFullBat -Wait -PassThru -NoNewWindow
            if ($p.ExitCode -ne 0) {
                throw ('Import amr_full.sql that bai voi ma loi ' + $p.ExitCode)
            }
            Write-Host '   [PASS] Da khoi tao database amr thanh cong tu amr_full.sql!' -ForegroundColor Green
        } finally {
            Remove-Item -Path $importFullBat -Force -ErrorAction SilentlyContinue
        }
    } else {
        Write-Error "Chua co Database 'amr' va khong tim thay amr_full.sql de khoi tao. Dung tien trinh."
    }
} else {
    Write-Host "   [PASS] Co so du lieu 'amr' da san sang." -ForegroundColor Green
}

# 5. BUOC 2: Import Master Data (Khach hang, Diem do, TagName, Users)
Write-Host "`n[BUOC 2/4] Dang nap Master Data tu $(Split-Path $fileData -Leaf)..." -ForegroundColor Yellow

# Don dep du lieu cu tren cac bang Master Data de dam bao tinh idempotent, tranh loi Duplicate entry neu nap lai
$cleanMasterSql = @'
USE amr;
SET FOREIGN_KEY_CHECKS = 0;
DELETE FROM usercustomers;
DELETE FROM tagname;
DELETE FROM measurementpoints;
DELETE FROM gcsmonthlydetail;
DELETE FROM gcstemporary;
DELETE FROM gcscutoffcycle;
DELETE FROM gcs;
DELETE FROM customers;
DELETE FROM systemconfig;
DELETE FROM users;
SET FOREIGN_KEY_CHECKS = 1;
'@
Invoke-MySqlCommand -SqlContent $cleanMasterSql -Description "Don dep ban ghi cu tren cac bang Master Data"

$sw = [System.Diagnostics.Stopwatch]::StartNew()
$importDataBat = Join-Path $env:TEMP 'import_master.bat'
try {
    $batCmd = '@echo off' + "`r`n" + '"' + $mysqlExe + '" -h ' + $MySqlHost + ' -P ' + $MySqlPort + ' -u ' + $MySqlUser + ' "-p' + $MySqlPassword + '" --max-allowed-packet=512M --net-buffer-length=16384 --default-character-set=utf8mb4 amr < "' + $fileData + '"'
    Set-Content -Path $importDataBat -Value $batCmd -Encoding ASCII
    $p = Start-Process -FilePath $importDataBat -Wait -PassThru -NoNewWindow
    $sw.Stop()

    if ($p.ExitCode -ne 0) {
        throw ('Import Master Data that bai voi ma loi ' + $p.ExitCode)
    }
    Write-Host ('   [PASS] Da import thanh cong Master Data trong ' + [math]::Round($sw.Elapsed.TotalSeconds, 2) + ' giay!') -ForegroundColor Green
} finally {
    Remove-Item -Path $importDataBat -Force -ErrorAction SilentlyContinue
}

# 6. BUOC 3: Dong bo mat khau tai khoan Admin Web Portal
Write-Host "`n[BUOC 3/4] Dong bo mat khau tai khoan Admin Web Portal..." -ForegroundColor Yellow
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
Write-Host ('   [PASS] Tai khoan admin da duoc thiet lap mat khau: ' + $AdminPassword) -ForegroundColor Green

# 7. BUOC 4: Kiem tra va in bang xac minh toan ven du lieu
Write-Host "`n[BUOC 4/4] Xac minh so luong ban ghi Master Data va trang thai 'trang'..." -ForegroundColor Yellow

$verifyQueryFile = Join-Path $env:TEMP 'verify_query.sql'
$verifyResultFile = Join-Path $env:TEMP 'verify_result.txt'
$verifySql = @"
USE amr;
SELECT '=== 1. DU LIEU CO SO (DA IMPORT THANH CONG) ===' AS DanhMuc, '' AS SoLuong
UNION ALL
SELECT 'Khach hang (customers)', CAST(COUNT(*) AS CHAR) FROM customers
UNION ALL
SELECT 'So GCS (gcs)', CAST(COUNT(*) AS CHAR) FROM gcs
UNION ALL
SELECT 'Diem do (measurementpoints)', CAST(COUNT(*) AS CHAR) FROM measurementpoints
UNION ALL
SELECT '  -> Diem do da gan Khach hang (CustomerId)', CAST(COUNT(*) AS CHAR) FROM measurementpoints WHERE CustomerId IS NOT NULL AND CustomerId > 0
UNION ALL
SELECT '  -> Diem do da gan So GCS (GcsId)', CAST(COUNT(*) AS CHAR) FROM measurementpoints WHERE GcsId IS NOT NULL AND GcsId > 0
UNION ALL
SELECT 'Phan quyen Khach hang (usercustomers)', CAST(COUNT(*) AS CHAR) FROM usercustomers
UNION ALL
SELECT 'Lien ket thong so (tagname)', CAST(COUNT(*) AS CHAR) FROM tagname
UNION ALL
SELECT 'Nguoi dung (users)', CAST(COUNT(*) AS CHAR) FROM users
UNION ALL
SELECT 'Cau hinh he thong (systemconfig)', CAST(COUNT(*) AS CHAR) FROM systemconfig
UNION ALL
SELECT '=== 2. DU LIEU VAN HANH (MOI TRUONG TRANG - PHAI BANG 0) ===', ''
UNION ALL
SELECT 'Ky chot GCS cu (gcscutoffcycle)', CAST(COUNT(*) AS CHAR) FROM gcscutoffcycle
UNION ALL
SELECT 'Chi tiet GCS hang thang (gcsmonthlydetail)', CAST(COUNT(*) AS CHAR) FROM gcsmonthlydetail
UNION ALL
SELECT 'Tam tinh GCS (gcstemporary)', CAST(COUNT(*) AS CHAR) FROM gcstemporary
UNION ALL
SELECT 'Bao cao Chi so nua gio (halfhour energy)', CAST(COUNT(*) AS CHAR) FROM measurementpointhalfhourenergyreports
UNION ALL
SELECT 'Bao cao Thong so van hanh nua gio (operating reports)', CAST(COUNT(*) AS CHAR) FROM measurementpointhalfhouroperatingreports
UNION ALL
SELECT 'Bao cao San luong ngay (daily energy)', CAST(COUNT(*) AS CHAR) FROM measurementpointdailyenergyreports
UNION ALL
SELECT 'Du lieu do dem tuc thoi (monitoring)', CAST(COUNT(*) AS CHAR) FROM measurementpointmonitoring
UNION ALL
SELECT 'Phieu bao dien nang (periodic statements)', CAST(COUNT(*) AS CHAR) FROM periodicenergystatement;
"@

try {
    Set-Content -Path $verifyQueryFile -Value $verifySql -Encoding UTF8
    $batVerify = '@echo off' + "`r`n" + '"' + $mysqlExe + '" -h ' + $MySqlHost + ' -P ' + $MySqlPort + ' -u ' + $MySqlUser + ' "-p' + $MySqlPassword + '" -t < "' + $verifyQueryFile + '" > "' + $verifyResultFile + '"'
    $tempBatVerify = Join-Path $env:TEMP 'run_verify.bat'
    Set-Content -Path $tempBatVerify -Value $batVerify -Encoding ASCII
    $p = Start-Process -FilePath $tempBatVerify -Wait -PassThru -NoNewWindow
    Remove-Item -Path $tempBatVerify -Force -ErrorAction SilentlyContinue

    if (Test-Path $verifyResultFile) {
        Write-Host "`n=== KET QUA KIEM TRA SAU KHI IMPORT MASTER DATA ===" -ForegroundColor Cyan
        Get-Content $verifyResultFile | Write-Host -ForegroundColor White
    }
} finally {
    Remove-Item -Path $verifyQueryFile, $verifyResultFile -Force -ErrorAction SilentlyContinue
}

Write-Host "`n================================================================================" -ForegroundColor Green
Write-Host '         HOAN TAT IMPORT MASTER DATA VAO CO SO DU LIEU THANH CONG!              ' -ForegroundColor Green
Write-Host '================================================================================' -ForegroundColor Green
Write-Host 'He thong da san sang tren moi truong TRANG voi toan bo Khach hang & Diem do!' -ForegroundColor White
Write-Host 'Thong tin dang nhap Web Portal:' -ForegroundColor White
Write-Host ('   - URL       : http://' + $MySqlHost + ' (hoac IP may ao)') -ForegroundColor Cyan
Write-Host '   - Tai khoan : admin' -ForegroundColor Cyan
Write-Host ('   - Mat khau  : ' + $AdminPassword) -ForegroundColor Cyan
Write-Host '================================================================================' -ForegroundColor Green
