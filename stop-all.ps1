# Fason Backend & Frontend Stopper Script (PowerShell)
Write-Host "====================================================" -ForegroundColor Cyan
Write-Host "   Fason Backend & Frontend Durduruluyor..." -ForegroundColor Cyan
Write-Host "====================================================" -ForegroundColor Cyan
Write-Host ""

# Backend process'lerini durdur
Write-Host "[1/3] Backend .NET process'leri durduruluyor..." -ForegroundColor Yellow

# Port 7047'deki process'leri bul
$port7047 = Get-NetTCPConnection -LocalPort 7047 -ErrorAction SilentlyContinue | Select-Object -ExpandProperty OwningProcess -Unique
if ($port7047) {
    foreach ($pid in $port7047) {
        try {
            Stop-Process -Id $pid -Force -ErrorAction SilentlyContinue
            Write-Host "[OK] Port 7047'deki process (PID: $pid) durduruldu." -ForegroundColor Green
        } catch {
            Write-Host "[!] Process $pid durdurulamadı." -ForegroundColor Yellow
        }
    }
} else {
    Write-Host "[!] Port 7047'de çalışan process bulunamadı." -ForegroundColor Gray
}

# dotnet.exe process'lerini kontrol et
$dotnetProcesses = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue
if ($dotnetProcesses) {
    Write-Host "[!] $($dotnetProcesses.Count) adet dotnet process bulundu, durduruluyor..." -ForegroundColor Yellow
    $dotnetProcesses | Stop-Process -Force -ErrorAction SilentlyContinue
    Write-Host "[OK] dotnet process'leri durduruldu." -ForegroundColor Green
}
Write-Host ""

# Frontend process'lerini durdur
Write-Host "[2/3] Frontend Angular process'leri durduruluyor..." -ForegroundColor Yellow

# Port 4200'deki process'leri bul
$port4200 = Get-NetTCPConnection -LocalPort 4200 -ErrorAction SilentlyContinue | Select-Object -ExpandProperty OwningProcess -Unique
if ($port4200) {
    foreach ($pid in $port4200) {
        try {
            Stop-Process -Id $pid -Force -ErrorAction SilentlyContinue
            Write-Host "[OK] Port 4200'deki process (PID: $pid) durduruldu." -ForegroundColor Green
        } catch {
            Write-Host "[!] Process $pid durdurulamadı." -ForegroundColor Yellow
        }
    }
} else {
    Write-Host "[!] Port 4200'de çalışan process bulunamadı." -ForegroundColor Gray
}

# node.exe process'lerini kontrol et
$nodeProcesses = Get-Process -Name "node" -ErrorAction SilentlyContinue
if ($nodeProcesses) {
    Write-Host "[!] $($nodeProcesses.Count) adet node process bulundu." -ForegroundColor Yellow
    # Sadece Angular ile ilişkili olanları durdur (port 4200 kullananlar)
    foreach ($proc in $nodeProcesses) {
        $connections = Get-NetTCPConnection -OwningProcess $proc.Id -ErrorAction SilentlyContinue | Where-Object { $_.LocalPort -eq 4200 }
        if ($connections) {
            Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
            Write-Host "[OK] Angular node process (PID: $($proc.Id)) durduruldu." -ForegroundColor Green
        }
    }
}
Write-Host ""

# Port kontrolü
Write-Host "[3/3] Port temizliği kontrol ediliyor..." -ForegroundColor Yellow

$port7047Check = Get-NetTCPConnection -LocalPort 7047 -ErrorAction SilentlyContinue
if ($port7047Check) {
    Write-Host "[!] Port 7047 hala kullanımda!" -ForegroundColor Red
} else {
    Write-Host "[OK] Port 7047 temiz." -ForegroundColor Green
}

$port4200Check = Get-NetTCPConnection -LocalPort 4200 -ErrorAction SilentlyContinue
if ($port4200Check) {
    Write-Host "[!] Port 4200 hala kullanımda!" -ForegroundColor Red
} else {
    Write-Host "[OK] Port 4200 temiz." -ForegroundColor Green
}
Write-Host ""

Write-Host "====================================================" -ForegroundColor Cyan
Write-Host "   Tüm Servisler Durduruldu!" -ForegroundColor Green
Write-Host "====================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Port 7047 (Backend):  Kapalı" -ForegroundColor White
Write-Host "Port 4200 (Frontend): Kapalı" -ForegroundColor White
Write-Host ""
Write-Host "Servisleri tekrar başlatmak için start-all.ps1 çalıştırın." -ForegroundColor Yellow
Write-Host ""
pause

