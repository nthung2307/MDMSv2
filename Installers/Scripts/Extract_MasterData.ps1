<#
.SYNOPSIS
    Kich ban trich xuat du lieu Master Data (Khach hang, So GCS, Diem do, TagName, Users, UserCustomers, SystemConfig)
    tu tap tin amr_data.sql (dung luong bat ky: 1GB, 10GB, 50GB+) thanh amr_master_data.sql sieu nhe (~3-10 MB).
.DESCRIPTION
    Dac diem noi bat:
    1. Bo qua toan bo du lieu do dem van hanh, bao cao va cac ky chot GCS cu (gcscutoffcycle, gcsmonthlydetail).
    2. Chi giu lai So GCS goc (gcs) va toan bo quan he Khach hang (CustomerId) - So GCS (GcsId) tren Diem do.
    3. Doc file dang Stream (khong ton RAM), ho tro file dump kich thuoc khong gioi han tu he thong dang hoat dong.
    4. Tu dong nhan dien moi dinh dang mysqldump (co dau backtick, nhay don hoac khong dau nhay).
    5. Tu dong chuyen doi INSERT INTO thanh REPLACE INTO de tranh xung dot khi nap vao CSDL.
    6. Hien thi thanh tien do va thong tin bang dang xu ly theo thoi gian thuc.
#>

[CmdletBinding()]
param (
    [string]$InputPath = '',
    [string]$OutputPath = ''
)

$scriptDir = $PSScriptRoot

if ([string]::IsNullOrWhiteSpace($InputPath)) {
    $candidates = @(
        (Join-Path $scriptDir '..\Database\amr_data.sql'),
        (Join-Path $scriptDir 'amr_data.sql'),
        'C:\MDMSv2\Database\amr_data.sql'
    )
    foreach ($c in $candidates) {
        if (Test-Path $c) { $InputPath = (Resolve-Path $c).Path; break }
    }
}

if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path (Split-Path $InputPath -Parent) 'amr_master_data.sql'
}

if (-not (Test-Path $InputPath)) {
    Write-Error "Khong tim thay tap tin du lieu nguon: $InputPath"
    exit 1
}

$inputFileItem = Get-Item $InputPath
$totalBytes = $inputFileItem.Length
$totalMB = [math]::Round($totalBytes / 1MB, 2)

Write-Host "================================================================================" -ForegroundColor Cyan
Write-Host "     MDMSv2 - TRICH XUAT MASTER DATA (KHACH HANG, SO GCS, DIEM DO, TAGS, USERS) " -ForegroundColor Cyan
Write-Host "================================================================================" -ForegroundColor Cyan
Write-Host "Tap tin nguon : $InputPath ($totalMB MB)" -ForegroundColor White
Write-Host "Tap tin dich  : $OutputPath" -ForegroundColor White
Write-Host "Tieu chi loc  : GIU LAI Khach hang, So GCS, Diem do, Tags, Users." -ForegroundColor Green
Write-Host "                LOAI BO Ky chot GCS (gcscutoffcycle) va toan bo du lieu van hanh/do dem." -ForegroundColor Yellow
Write-Host "--------------------------------------------------------------------------------" -ForegroundColor Gray

# Danh sach cac bang Master Data can giu lai (KHONG lay gcscutoffcycle hay gcsmonthlydetail)
$targetTables = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
$null = $targetTables.Add('customers')
$null = $targetTables.Add('gcs')
$null = $targetTables.Add('measurementpoints')
$null = $targetTables.Add('tagname')
$null = $targetTables.Add('users')
$null = $targetTables.Add('usercustomers')
$null = $targetTables.Add('systemconfig')

$extractedTables = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)

$encodingUtf8 = [System.Text.UTF8Encoding]::new($false)
$fileStream = [System.IO.File]::Open($InputPath, [System.IO.FileMode]::Open, [System.IO.FileAccess]::Read, [System.IO.FileShare]::ReadWrite)
$reader = [System.IO.StreamReader]::new($fileStream, [System.Text.Encoding]::UTF8, $true, 1048576)
$writer = [System.IO.StreamWriter]::new($OutputPath, $false, $encodingUtf8, 1048576)

$sw = [System.Diagnostics.Stopwatch]::StartNew()
$lastReportTime = [System.Diagnostics.Stopwatch]::StartNew()

try {
    $writer.WriteLine('-- =====================================================================================')
    $writer.WriteLine('-- DU LIEU CO SO (MASTER DATA): KHACH HANG, SO GCS, DIEM DO, LIEN KET DU LIEU (TAGS), USERS')
    $writer.WriteLine('-- Trich xuat tu: ' + (Split-Path $InputPath -Leaf) + ' (Tong dung luong goc: ' + $totalMB + ' MB)')
    $writer.WriteLine('-- Luu y: Da loai bo ky chot GCS (gcscutoffcycle) va toan bo du lieu van hanh / do dem')
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

    $capturing = $false
    $currentTable = ''
    $processedLines = 0

    while ($null -ne ($line = $reader.ReadLine())) {
        $processedLines++

        # Báo cáo tiến độ mỗi 5 giây
        if ($lastReportTime.ElapsedMilliseconds -ge 5000) {
            $currentPos = $fileStream.Position
            $pct = if ($totalBytes -gt 0) { [math]::Round(($currentPos / $totalBytes) * 100, 1) } else { 0 }
            $currMB = [math]::Round($currentPos / 1MB, 1)
            $speedMBs = if ($sw.Elapsed.TotalSeconds -gt 0) { [math]::Round($currMB / $sw.Elapsed.TotalSeconds, 1) } else { 0 }
            Write-Host "   [Tien do] Da doc: $currMB MB / $totalMB MB ($pct%) - Toc do: $speedMBs MB/s..." -ForegroundColor Gray
            $lastReportTime.Restart()
        }

        # Nhan dien tieu de bang theo cu phap chuan cua mysqldump
        if ($line -match '^--\s+Dumping\s+data\s+for\s+table\s+[`''"]?([a-zA-Z0-9_]+)[`''"]?') {
            $tableName = $matches[1]
            if ($targetTables.Contains($tableName)) {
                $capturing = $true
                $currentTable = $tableName
                $null = $extractedTables.Add($tableName)
                Write-Host ('   [+] Phat hien bang Master Data: ' + $currentTable) -ForegroundColor Green
                $writer.WriteLine()
                $writer.WriteLine('-- ------------------------------------------------------')
                $writer.WriteLine('-- Dumping data for table `' + $currentTable + '`')
                $writer.WriteLine('-- ------------------------------------------------------')
            } else {
                $capturing = $false
                $currentTable = ''
            }
        } elseif ($capturing) {
            # Kiem tra ket thuc block du lieu cua bang
            if ($line -match '^--\s+(Table\s+structure|Dumping\s+data)\s+for\s+table') {
                $capturing = $false
                $currentTable = ''
            } else {
                # Chuyen INSERT INTO thanh REPLACE INTO de dam bao tinh idempotent
                if ($line -match '^INSERT\s+(/\*!\d+\s+)?INTO\s+') {
                    $line = [System.Text.RegularExpressions.Regex]::Replace($line, '^INSERT\s+(/\*!\d+\s+)?INTO\s+', 'REPLACE INTO ', [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
                }
                $writer.WriteLine($line)
                if ($line.StartsWith('UNLOCK TABLES;', [System.StringComparison]::OrdinalIgnoreCase)) {
                    $capturing = $false
                    $currentTable = ''
                }
            }
        }
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
    $fileStream.Close()
    $writer.Flush()
    $writer.Close()
}

$sw.Stop()
$outSizeKB = [math]::Round(((Get-Item $OutputPath).Length / 1KB), 2)

Write-Host "--------------------------------------------------------------------------------" -ForegroundColor Gray
Write-Host "TRICH XUAT THANH CONG trong $($sw.Elapsed.TotalSeconds.ToString('F1')) giay!" -ForegroundColor Green
Write-Host "Tap tin ket qua     : $OutputPath ($outSizeKB KB)" -ForegroundColor Cyan
Write-Host "Cac bang da xuat    : $($extractedTables -join ', ')" -ForegroundColor White
Write-Host "================================================================================" -ForegroundColor Cyan
