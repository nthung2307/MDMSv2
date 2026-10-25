<#
.SYNOPSIS
    Kich ban trich xuat Master Data truc tiep tu Co so Du lieu dang chay (Live Database).
.DESCRIPTION
    Dung khi ban co he thong dang hoat dong voi so luong lon khach hang va diem do (hang nghin diem do):
    - Khong can phai dump toan bo 50GB - 100GB du lieu van hanh ra dia.
    - Trich xuat truc tiep chi cac bang Master Data: customers, gcs, measurementpoints, tagname, users, usercustomers, systemconfig.
    - Thoi gian xu ly sieu nhanh: Chi tu 2 den 10 giay!
    - Tap tin amr_master_data.sql sinh ra cuc ky tinh gon (~vai MB) va san sang de mang sang may ao moi.
#>

[CmdletBinding()]
param (
    [string]$MySqlHost = "127.0.0.1",
    [int]$MySqlPort = 3306,
    [string]$MySqlUser = "root",
    [string]$MySqlPassword = "",
    [string]$Database = "amr",
    [string]$OutputPath = ""
)

$ErrorActionPreference = "Stop"

Clear-Host
Write-Host "================================================================================" -ForegroundColor Cyan
Write-Host "   MDMSv2 - XUAT MASTER DATA TRUC TIEP TU CSDL DANG HOAT DONG (LIVE DATABASE)   " -ForegroundColor Cyan
Write-Host "================================================================================" -ForegroundColor Cyan
Write-Host "Thoi gian thuc hien: $(Get-Date -Format 'dd/MM/yyyy HH:mm:ss')" -ForegroundColor Gray

if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $scriptDir = $PSScriptRoot
    $OutputPath = Join-Path $scriptDir "amr_master_data.sql"
}

# 1. Tim kiem mysqldump.exe
function Find-MySqlDumpExe {
    $dumpCmd = Get-Command "mysqldump.exe" -ErrorAction SilentlyContinue
    if ($dumpCmd) { return $dumpCmd.Source }

    $searchPaths = @(
        "C:\Program Files\MySQL\MySQL Server 8.0\bin\mysqldump.exe",
        "C:\Program Files\MySQL\MySQL Server 8.4\bin\mysqldump.exe",
        "C:\Program Files (x86)\MySQL\MySQL Server 8.0\bin\mysqldump.exe"
    )
    foreach ($p in $searchPaths) {
        if (Test-Path $p) { return $p }
    }
    
    $found = Get-ChildItem -Path "C:\Program Files\MySQL" -Filter "mysqldump.exe" -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($found) { return $found.FullName }

    return $null
}

$dumpExe = Find-MySqlDumpExe
if (-not $dumpExe) {
    Write-Host "`n[CANH BAO] Khong tim thay mysqldump.exe trong PATH!" -ForegroundColor Red
    $manualExe = Read-Host "Vui long nhap duong dan day du toi file mysqldump.exe"
    if (Test-Path $manualExe) {
        $dumpExe = $manualExe
    } else {
        Write-Error "Duong dan mysqldump.exe khong hop le. Dung tien trinh."
    }
}
Write-Host "Cong cu mysqldump : $dumpExe" -ForegroundColor Gray

# 2. Thu thap mat khau neu chua co
while ([string]::IsNullOrWhiteSpace($MySqlPassword)) {
    $secPass = Read-Host "Nhap mat khau MySQL root cho tai khoan '$MySqlUser'" -AsSecureString
    $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($secPass)
    $MySqlPassword = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
    [System.Runtime.InteropServices.Marshal]::ZeroFreeBSTR($BSTR)
}

# 3. Thuc thi mysqldump chi tren cac bang Master Data
$masterTables = @("customers", "gcs", "measurementpoints", "tagname", "users", "usercustomers", "systemconfig")
Write-Host "`nDang trich xuat cac bang Master Data tu CSDL '$Database'..." -ForegroundColor Yellow
Write-Host "Danh sach bang: $($masterTables -join ', ')" -ForegroundColor Cyan
Write-Host "(Hoan toan loai bo gcscutoffcycle va cac bang du lieu van hanh/do dem)" -ForegroundColor Gray

$tempRawSql = Join-Path $env:TEMP ("mdms_dump_raw_" + [guid]::NewGuid().ToString("N") + ".sql")
$tempBat = Join-Path $env:TEMP ("mdms_dump_bat_" + [guid]::NewGuid().ToString("N") + ".bat")

$sw = [System.Diagnostics.Stopwatch]::StartNew()
try {
    $tablesArgs = $masterTables -join " "
    $dumpBatContent = "@echo off`r`n`"$dumpExe`" -h $MySqlHost -P $MySqlPort -u $MySqlUser `"-p$MySqlPassword`" $Database $tablesArgs --no-create-info --skip-triggers --default-character-set=utf8mb4 --hex-blob --quick --single-transaction --result-file=`"$tempRawSql`""
    Set-Content -Path $tempBat -Value $dumpBatContent -Encoding ASCII

    $p = Start-Process -FilePath $tempBat -Wait -PassThru -NoNewWindow
    if ($p.ExitCode -ne 0) {
        throw "mysqldump ket thuc voi ma loi: $($p.ExitCode)"
    }

    # 4. Chuyen INSERT INTO thanh REPLACE INTO va them header/footer an toan
    Write-Host "Dang chuan hoa cu phap REPLACE INTO va header FOREIGN_KEY_CHECKS..." -ForegroundColor Yellow
    $encodingUtf8 = [System.Text.UTF8Encoding]::new($false)
    $reader = [System.IO.StreamReader]::new($tempRawSql, [System.Text.Encoding]::UTF8)
    $writer = [System.IO.StreamWriter]::new($OutputPath, $false, $encodingUtf8, 1048576)

    try {
        $writer.WriteLine('-- =====================================================================================')
        $writer.WriteLine('-- DU LIEU CO SO (MASTER DATA): KHACH HANG, SO GCS, DIEM DO, TAGS, USERS')
        $writer.WriteLine('-- Trich xuat truc tiep tu Live Database: ' + $Database + ' (' + $MySqlHost + ':' + $MySqlPort + ')')
        $writer.WriteLine('-- Ngay trich xuat: ' + (Get-Date -Format 'yyyy-MM-dd HH:mm:ss'))
        $writer.WriteLine('-- =====================================================================================')
        $writer.WriteLine()
        $writer.WriteLine('USE `amr`;')
        $writer.WriteLine()
        $writer.WriteLine('SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT;')
        $writer.WriteLine('SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS;')
        $writer.WriteLine('SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION;')
        $writer.WriteLine('SET NAMES utf8mb4;')
        $writer.WriteLine('SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0;')
        $writer.WriteLine('SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0;')
        $writer.WriteLine('SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE=''NO_AUTO_VALUE_ON_ZERO'';')
        $writer.WriteLine('SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0;')
        $writer.WriteLine()

        while ($null -ne ($line = $reader.ReadLine())) {
            if ($line -match '^INSERT\s+(/\*!\d+\s+)?INTO\s+') {
                $line = [System.Text.RegularExpressions.Regex]::Replace($line, '^INSERT\s+(/\*!\d+\s+)?INTO\s+', 'REPLACE INTO ', [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
            }
            $writer.WriteLine($line)
        }

        $writer.WriteLine()
        $writer.WriteLine('SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS;')
        $writer.WriteLine('SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS;')
        $writer.WriteLine('SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT;')
        $writer.WriteLine('SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS;')
        $writer.WriteLine('SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION;')
        $writer.WriteLine('SET SQL_NOTES=@OLD_SQL_NOTES;')
        $writer.WriteLine()
    } finally {
        $reader.Close()
        $writer.Flush()
        $writer.Close()
    }

    $sw.Stop()
    $outSizeKB = [math]::Round(((Get-Item $OutputPath).Length / 1KB), 2)
    Write-Host "`n================================================================================" -ForegroundColor Green
    Write-Host "TRICH XUAT MASTER DATA TU LIVE DATABASE HOAN TAT trong $($sw.Elapsed.TotalSeconds.ToString('F2')) giay!" -ForegroundColor Green
    Write-Host "Tap tin ket qua: $OutputPath ($outSizeKB KB)" -ForegroundColor Cyan
    Write-Host "================================================================================" -ForegroundColor Green
} finally {
    Remove-Item -Path $tempRawSql, $tempBat -Force -ErrorAction SilentlyContinue
}
