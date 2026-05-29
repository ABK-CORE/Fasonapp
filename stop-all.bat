@echo off
chcp 65001 >nul
echo ====================================================
echo    Fason Backend ^& Frontend Durduruluyor...
echo ====================================================
echo.

REM Proje root dizinine git
cd /d "%~dp0"

echo [1/4] Backend .NET process'leri durduruluyor...

REM WebApi çalışan process'i bul ve durdur
tasklist /FI "WINDOWTITLE eq Fason Backend*" 2>nul | find /i "cmd.exe" >nul
if %errorlevel% equ 0 (
    taskkill /FI "WINDOWTITLE eq Fason Backend*" /T /F >nul 2>&1
    echo [OK] Backend penceresi kapatıldı.
) else (
    echo [!] Backend penceresi bulunamadı.
)

REM Dotnet process'lerini durdur (WebApi için)
tasklist | find /i "dotnet.exe" >nul
if %errorlevel% equ 0 (
    for /f "tokens=2" %%i in ('netstat -ano ^| findstr ":7047" ^| findstr "LISTENING"') do (
        taskkill /PID %%i /F >nul 2>&1
        echo [OK] Port 7047'deki process durduruldu.
    )
)
echo.

echo [2/4] Frontend Angular process'leri durduruluyor...

REM Angular dev server penceresini bul ve durdur
tasklist /FI "WINDOWTITLE eq Fason Frontend*" 2>nul | find /i "cmd.exe" >nul
if %errorlevel% equ 0 (
    taskkill /FI "WINDOWTITLE eq Fason Frontend*" /T /F >nul 2>&1
    echo [OK] Frontend penceresi kapatıldı.
) else (
    echo [!] Frontend penceresi bulunamadı.
)

REM Angular dev server'ı port 4200'den durdur
for /f "tokens=5" %%i in ('netstat -ano ^| findstr ":4200" ^| findstr "LISTENING"') do (
    taskkill /PID %%i /F >nul 2>&1
    echo [OK] Port 4200'deki Angular process durduruldu.
)
echo.

echo [3/4] Kalan Node.js process'leri kontrol ediliyor...

REM ng serve veya node process'lerini temizle
tasklist | find /i "node.exe" >nul
if %errorlevel% equ 0 (
    echo [!] Node.js process'leri bulundu.
    REM Sadece Angular ile ilgili node process'lerini durdur
    for /f "tokens=2" %%i in ('tasklist /FI "IMAGENAME eq node.exe" /FO LIST ^| findstr "PID:"') do (
        netstat -ano | findstr "%%i" | findstr ":4200" >nul 2>&1
        if !errorlevel! equ 0 (
            taskkill /PID %%i /F >nul 2>&1
            echo [OK] Angular node process (PID: %%i) durduruldu.
        )
    )
) else (
    echo [OK] Node.js process'leri zaten temiz.
)
echo.

echo [4/4] Port temizliği yapılıyor...

REM Port 7047 kontrolü
netstat -ano | findstr ":7047" | findstr "LISTENING" >nul
if %errorlevel% equ 0 (
    echo [!] Port 7047 hala kullanımda, temizleniyor...
    for /f "tokens=5" %%i in ('netstat -ano ^| findstr ":7047" ^| findstr "LISTENING"') do (
        taskkill /PID %%i /F >nul 2>&1
    )
    echo [OK] Port 7047 temizlendi.
) else (
    echo [OK] Port 7047 temiz.
)

REM Port 4200 kontrolü
netstat -ano | findstr ":4200" | findstr "LISTENING" >nul
if %errorlevel% equ 0 (
    echo [!] Port 4200 hala kullanımda, temizleniyor...
    for /f "tokens=5" %%i in ('netstat -ano ^| findstr ":4200" ^| findstr "LISTENING"') do (
        taskkill /PID %%i /F >nul 2>&1
    )
    echo [OK] Port 4200 temizlendi.
) else (
    echo [OK] Port 4200 temiz.
)
echo.

echo ====================================================
echo    Tüm Servisler Durduruldu!
echo ====================================================
echo.
echo Port 7047 (Backend):  Kapalı
echo Port 4200 (Frontend): Kapalı
echo.
echo Servisleri tekrar başlatmak için start-all.bat çalıştırın.
echo.
pause

