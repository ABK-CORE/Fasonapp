# Fason Backend & Frontend Starter Script (PowerShell)
# UTF-8 konsol cikisi (TR karakter bozulmasini azaltmak icin)
try {
    chcp 65001 | Out-Null
}
catch {
    # no-op
}

try {
    $utf8NoBom = New-Object System.Text.UTF8Encoding($false)
    [Console]::OutputEncoding = $utf8NoBom
    [Console]::InputEncoding = $utf8NoBom
    $OutputEncoding = $utf8NoBom
}
catch {
    # no-op
}

Write-Host "====================================================" -ForegroundColor Cyan
Write-Host "   Fason Backend & Frontend Başlatılıyor..." -ForegroundColor Cyan
Write-Host "====================================================" -ForegroundColor Cyan
Write-Host ""

# Proje root dizinine git
Set-Location $PSScriptRoot

# Node.js kontrolü
Write-Host "[1/4] Node.js ve npm kontrolü yapılıyor..." -ForegroundColor Yellow
try {
    $nodeVersion = node --version
    Write-Host "[OK] Node.js versiyon: $nodeVersion" -ForegroundColor Green
    
    $npmVersion = npm --version
    Write-Host "[OK] npm versiyon: $npmVersion" -ForegroundColor Green
}
catch {
    Write-Host "[HATA] Node.js bulunamadı! Lütfen Node.js yükleyin." -ForegroundColor Red
    Write-Host "https://nodejs.org/" -ForegroundColor Yellow
    pause
    exit 1
}
Write-Host ""

# Backend build ve başlat
Write-Host "[2/4] Backend (.NET 8.0) build ediliyor..." -ForegroundColor Yellow
try {
    $buildOutput = dotnet build WebApi/WebApi.csproj --configuration Release --nologo 2>&1 | Out-String
    if ($LASTEXITCODE -ne 0) {
        Write-Host "[HATA] Build başarısız!" -ForegroundColor Red
        Write-Host $buildOutput
        pause
        exit 1
    }
    Write-Host "[OK] Build başarılı." -ForegroundColor Green
}
catch {
    Write-Host "[HATA] Build sırasında hata: $_" -ForegroundColor Red
    pause
    exit 1
}
Write-Host ""

Write-Host "[3/4] Backend başlatılıyor..." -ForegroundColor Yellow

# launchSettings.json icindeki profile ayarlarini kullan
$launchSettingsPath = Join-Path $PSScriptRoot "WebApi\Properties\launchSettings.json"
$launchProfile = $null
if (Test-Path -LiteralPath $launchSettingsPath) {
    try {
        $launchSettings = Get-Content -LiteralPath $launchSettingsPath -Raw | ConvertFrom-Json
        if ($launchSettings.profiles) {
            # "Project" profile'larini tercih et (IIS Express yerine)
            $projectProfiles = @(
                $launchSettings.profiles.PSObject.Properties |
                Where-Object { $_.Value.commandName -eq 'Project' }
            )

            $preferred = $projectProfiles | Where-Object { $_.Name -eq 'WebApi' } | Select-Object -First 1
            if (-not $preferred) {
                $preferred = $projectProfiles | Select-Object -First 1
            }

            if ($preferred) {
                $launchProfile = $preferred.Name
            }
        }
    }
    catch {
        $launchProfile = $null
    }
}

if (-not $launchProfile) {
    $launchProfile = 'WebApi'
}

Write-Host "Backend launch profile: $launchProfile" -ForegroundColor Gray
Write-Host "Backend Port: 7047 (HTTPS)" -ForegroundColor Gray
try {
    $backendCmd = "chcp 65001>nul & cls & cd /d `"$PSScriptRoot`" & dotnet watch --project WebApi run --configuration Release --launch-profile `"$launchProfile`""
    Start-Process -FilePath "cmd.exe" -ArgumentList "/k", $backendCmd -WindowStyle Normal
    Write-Host "[OK] Backend penceresi açıldı." -ForegroundColor Green
}
catch {
    Write-Host "[HATA] Backend başlatılamadı: $_" -ForegroundColor Red
    pause
    exit 1
}
Write-Host ""

# Kısa bekleme
Start-Sleep -Seconds 2

# Frontend başlat
Write-Host "[4/4] Frontend (Angular) başlatılıyor..." -ForegroundColor Yellow
Write-Host "Frontend Port: 4200 (HTTP)" -ForegroundColor Gray
try {
    $frontendDir = Join-Path $PSScriptRoot "FasonFrontend"
    $frontendCmd = "chcp 65001>nul & cls & cd /d `"$frontendDir`" & ng serve --open"
    Start-Process -FilePath "cmd.exe" -ArgumentList "/k", $frontendCmd -WindowStyle Normal
    Write-Host "[OK] Frontend penceresi açıldı." -ForegroundColor Green
}
catch {
    Write-Host "[HATA] Frontend başlatılamadı: $_" -ForegroundColor Red
    pause
    exit 1
}
Write-Host ""

Write-Host "====================================================" -ForegroundColor Cyan
Write-Host "   Tüm Servisler Başlatıldı!" -ForegroundColor Green
Write-Host "====================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Backend:   https://localhost:7047" -ForegroundColor White
Write-Host "Swagger:   https://localhost:7047/fason/index.html" -ForegroundColor White
Write-Host "Frontend:  http://localhost:4200" -ForegroundColor White
Write-Host ""
Write-Host "Servisleri durdurmak için stop-all.ps1 veya stop-all.bat çalıştırın." -ForegroundColor Yellow
Write-Host ""
Write-Host "NOT: Backend ve Frontend ayrı CMD pencerelerinde çalışıyor." -ForegroundColor Cyan
Write-Host "     Logları görmek için o pencerelere bakın." -ForegroundColor Cyan
Write-Host ""
Write-Host "Bu pencere 5 saniye sonra kapanacak..." -ForegroundColor Gray
Start-Sleep -Seconds 5

