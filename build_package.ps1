<#
.SYNOPSIS
    Kich ban tu dong hoa dong goi bo cai dat MDMSv2 cho Windows Server 2019.
.DESCRIPTION
    Script thuc hien:
    1. Kiem tra moi truong build (.NET SDK, Node.js/Yarn/npm).
    2. Build Backend (.NET 8 WebApi) dang Self-Contained (win-x64).
    3. Build Frontend (React 18 + Vite).
    4. Gom nhom toan bo Database script, Installer script, MySQL, Kepware va Prerequisites.
    5. Xuat ra thu muc 'Publish/MDMSv2_InstallPackage' san sang copy sang may ao.
#>

[CmdletBinding()]
param (
    [string]$Configuration = "Release",
    [string]$OutputDir = "$PSScriptRoot\Publish\MDMSv2_InstallPackage"
)

$ErrorActionPreference = "Stop"

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "   BAT DAU DONG GOI BO CAI DAT MDMSv2 CHO WINDOWS SERVER  " -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

# 1. Don dep thu muc Output cu
if (Test-Path $OutputDir) {
    Write-Host "`n[1/5] Don dep thu muc dong goi cu: $OutputDir..." -ForegroundColor Yellow
    Remove-Item -Path $OutputDir -Recurse -Force
}
New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
New-Item -ItemType Directory -Path "$OutputDir\Backend" -Force | Out-Null
New-Item -ItemType Directory -Path "$OutputDir\Frontend" -Force | Out-Null
New-Item -ItemType Directory -Path "$OutputDir\Database" -Force | Out-Null
New-Item -ItemType Directory -Path "$OutputDir\Scripts" -Force | Out-Null
New-Item -ItemType Directory -Path "$OutputDir\MySQL" -Force | Out-Null
New-Item -ItemType Directory -Path "$OutputDir\Kepware" -Force | Out-Null
New-Item -ItemType Directory -Path "$OutputDir\Prerequisites" -Force | Out-Null

# 2. Build & Publish Backend (.NET 8 WebApi Self-Contained win-x64)
Write-Host "`n[2/5] Dang bien dich va publish Backend (.NET 8 WebApi win-x64)..." -ForegroundColor Green
$backendProj = "$PSScriptRoot\Backend\WebApi\WebApi.csproj"
$backendOutput = "$OutputDir\Backend"

# Self-contained giup may ao Windows Server khong bat buoc phai cai truoc .NET 8 SDK
dotnet publish $backendProj `
    -c $Configuration `
    -r win-x64 `
    --self-contained true `
    /p:PublishSingleFile=false `
    /p:DebugType=None `
    -o $backendOutput

# Tao cau hinh appsettings.Production.json toi uu cho may ao
# Dung Here-string nhay don de bao toan 100% tung ky tu cua SymmetricSecurityKey (khong bi mat bien $L)
$localConfig = @'
{
  "Urls": "http://0.0.0.0:19089",
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Hangfire": "Information"
    }
  },
  "JwtConfigurations": {
    "Issuer": "api-amr.nobisoft.vn",
    "Audience": "api-amr.nobisoft.vn",
    "SymmetricSecurityKey": "ymF#R80S6;XHg.[*g9E+O>-Y%qzR&8sw<z5M$L#J0R*gr3Ud#7A=>7w:^EUf_=,",
    "Expires": 28800,
    "RefreshTokenExpires": 10080
  },
  "AppDb": {
    "Server": "127.0.0.1",
    "UserName": "root",
    "Password": "",
    "Database": "amr",
    "Port": "3306"
  },
  "Email": {
    "Templates": [],
    "Host": "smtp.gmail.com",
    "Provider": "GMAIL",
    "Sender": "dev.amr@local.com",
    "DisplayName": "AMR System",
    "Password": "",
    "IsBodyHtml": true,
    "EnableSsl": true,
    "Port": 587
  },
  "BaseUrlGetDataFromServerIoT": "http://127.0.0.1:39320/iotgateway/read",
  "FrontEndUrl": "http://localhost:80"
}
'@
Set-Content -Path "$backendOutput\appsettings.Production.json" -Value $localConfig -Encoding UTF8

# 3. Build Frontend (React 18 + Vite)
Write-Host "`n[3/5] Dang bien dich Frontend (React 18 + Vite)..." -ForegroundColor Green
$frontendDir = "$PSScriptRoot\Frontend"
$frontendDist = "$frontendDir\dist"

Push-Location $frontendDir
try {
    $hasYarn = [bool](Get-Command yarn -ErrorAction SilentlyContinue)
    $hasNpm = [bool](Get-Command npm -ErrorAction SilentlyContinue)

    if (-not (Test-Path "node_modules")) {
        Write-Host "Dang cai dat dependencies Frontend..." -ForegroundColor Yellow
        if ($hasYarn) {
            yarn install --frozen-lockfile --ignore-scripts
        } elseif ($hasNpm) {
            npm install --ignore-scripts
        } else {
            Write-Warning "Khong tim thay Yarn hoac npm trong PATH. Vui long cai dat Node.js/Yarn."
        }
    }
    
    Write-Host "Dang build static bundle cho Production..." -ForegroundColor Yellow
    if ($hasYarn) {
        yarn build:uat
    } elseif ($hasNpm) {
        npm run build:uat
    } else {
        Write-Warning "Khong the build Frontend vi thieu Yarn/npm."
    }
}
finally {
    Pop-Location
}

if (Test-Path $frontendDist) {
    Copy-Item -Path "$frontendDist\*" -Destination "$OutputDir\Frontend" -Recurse -Force
} else {
    Write-Warning "Khong tim thay thu muc dist Frontend ($frontendDist). Vui long kiem tra lai lenh build."
}

# Tao web.config cho IIS ho tro HTML5 History Mode va Reverse Proxy API
$webConfigContent = @"
<?xml version="1.0" encoding="UTF-8"?>
<configuration>
  <system.webServer>
    <defaultDocument enabled="true">
      <files>
        <clear />
        <add value="index.html" />
      </files>
    </defaultDocument>
    <rewrite>
      <rules>
        <!-- Rule 1: Chuyen tiep tat ca cac HTTP method nghiep vu (POST, PUT, DELETE, PATCH, OPTIONS) sang Backend port 19089 -->
        <rule name="ReverseProxyNonGet" stopProcessing="true">
          <match url=".*" />
          <conditions logicalGrouping="MatchAll">
            <add input="{REQUEST_METHOD}" pattern="^(POST|PUT|DELETE|PATCH|OPTIONS)$" />
          </conditions>
          <action type="Rewrite" url="http://127.0.0.1:19089/{R:0}" appendQueryString="true" />
        </rule>

        <!-- Rule 2: Chuyen tiep Swagger va cac ket noi SignalR WebSockets sang Backend port 19089 -->
        <rule name="ReverseProxySpecial" stopProcessing="true">
          <match url="^(swagger|.*hub)(/.*)?$" ignoreCase="true" />
          <action type="Rewrite" url="http://127.0.0.1:19089/{R:0}" appendQueryString="true" />
        </rule>

        <!-- Rule 3: Chuyen tiep cac API GET nghiep vu sang Backend (chi khi khong phai yeu cau tai trang HTML cua trinh duyet) -->
        <rule name="ReverseProxyGetApi" stopProcessing="true">
          <match url="^(api|auth|user|customers|customer|measurement-points|measurement-point-monitoring|gcs|gcs-cutoff-cycle|gcs-monthly-detail|periodic-energy-statement|tag-name|system-config|report|iot|half-hour-finalized-energy|daily-finalized-energy|export-pdf|export-csv)(/.*)?$" ignoreCase="true" />
          <conditions logicalGrouping="MatchAll">
            <add input="{HTTP_ACCEPT}" pattern="text/html" negate="true" />
          </conditions>
          <action type="Rewrite" url="http://127.0.0.1:19089/{R:0}" appendQueryString="true" />
        </rule>

        <!-- Rule 4: React SPA Fallback cho tat ca cac dieu huong trang giao dien (HTML5 History Mode) -->
        <rule name="ReactRoutes" stopProcessing="true">
          <match url=".*" />
          <conditions logicalGrouping="MatchAll">
            <add input="{REQUEST_FILENAME}" matchType="IsFile" negate="true" />
            <add input="{REQUEST_FILENAME}" matchType="IsDirectory" negate="true" />
          </conditions>
          <action type="Rewrite" url="/" />
        </rule>
      </rules>
    </rewrite>
    <staticContent>
      <remove fileExtension=".json" />
      <mimeMap fileExtension=".json" mimeType="application/json" />
      <remove fileExtension=".woff" />
      <mimeMap fileExtension=".woff" mimeType="application/font-woff" />
      <remove fileExtension=".woff2" />
      <mimeMap fileExtension=".woff2" mimeType="application/font-woff2" />
    </staticContent>
  </system.webServer>
</configuration>
"@
Set-Content -Path "$OutputDir\Frontend\web.config" -Value $webConfigContent -Encoding UTF8

# 4. Sao chep Database Scripts, MySQL, Kepware, Prerequisites & Setup Scripts
Write-Host "`n[4/5] Dang chuan bi scripts cai dat, co so du lieu va thu muc bo cai..." -ForegroundColor Green
if (Test-Path "$PSScriptRoot\Installers") {
    Copy-Item -Path "$PSScriptRoot\Installers\Scripts\*" -Destination "$OutputDir\Scripts" -Recurse -Force
    Copy-Item -Path "$PSScriptRoot\Installers\Database\*" -Destination "$OutputDir\Database" -Recurse -Force
    Copy-Item -Path "$PSScriptRoot\Installers\Kepware\*" -Destination "$OutputDir\Kepware" -Recurse -Force
    Copy-Item -Path "$PSScriptRoot\Installers\MySQL\*" -Destination "$OutputDir\MySQL" -Recurse -Force
    
    if (Test-Path "$PSScriptRoot\Installers\Prerequisites") {
        Copy-Item -Path "$PSScriptRoot\Installers\Prerequisites\*" -Destination "$OutputDir\Prerequisites" -Recurse -Force
    }

    # Dat kich ban Wizard tong the SetupMDMS.ps1 ngay tai thu muc goc cua goi cai dat
    $masterWizard = "$PSScriptRoot\Installers\Scripts\SetupMDMS.ps1"
    if (Test-Path $masterWizard) {
        Copy-Item -Path $masterWizard -Destination "$OutputDir\SetupMDMS.ps1" -Force
    }
}

# 5. Hoan tat & Huong dan tiep theo
Write-Host "`n[5/5] DONG GOI THANH CONG!" -ForegroundColor Green
Write-Host "Vi tri bo cai dat: $OutputDir" -ForegroundColor Cyan

$totalBytes = (Get-ChildItem $OutputDir -Recurse | Measure-Object -Property Length -Sum).Sum
if ($totalBytes) {
    $totalMb = [math]::Round(($totalBytes / 1MB), 2)
    Write-Host "Dung luong thu muc: $totalMb MB" -ForegroundColor Cyan
}

Write-Host "Kich ban cai dat he thong tai: SetupMDMS.ps1" -ForegroundColor Yellow
