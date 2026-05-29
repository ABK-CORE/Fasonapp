@echo off
chcp 65001 >nul
echo ====================================================
echo    Startup Scripts Test
echo ====================================================
echo.
echo Bu script start-all.bat ve stop-all.bat dosyalarını test eder.
echo.
echo Test adımları:
echo 1. Gerekli araçların kontrolü (Node.js, npm, .NET)
echo 2. Script dosyalarının varlığı kontrolü
echo 3. Port kullanım kontrolü
echo.
pause
echo.

REM Proje root dizinine git
cd /d "%~dp0"

echo [Test 1/5] Node.js kontrolü...
where node >nul 2>&1
if %errorlevel% neq 0 (
    echo [BAŞARISIZ] Node.js bulunamadı!
    echo Lütfen Node.js yükleyin: https://nodejs.org/
    goto :error
)
node --version
echo [OK] Node.js kurulu.
echo.

echo [Test 2/5] npm kontrolü...
where npm >nul 2>&1
if %errorlevel% neq 0 (
    echo [BAŞARISIZ] npm bulunamadı!
    goto :error
)
npm --version
echo [OK] npm kurulu.
echo.

echo [Test 3/5] .NET SDK kontrolü...
where dotnet >nul 2>&1
if %errorlevel% neq 0 (
    echo [BAŞARISIZ] .NET SDK bulunamadı!
    echo Lütfen .NET 8.0 SDK yükleyin: https://dotnet.microsoft.com/download
    goto :error
)
dotnet --version
echo [OK] .NET SDK kurulu.
echo.

echo [Test 4/5] Script dosyaları kontrolü...
if not exist "start-all.bat" (
    echo [BAŞARISIZ] start-all.bat bulunamadı!
    goto :error
)
echo [OK] start-all.bat mevcut.

if not exist "stop-all.bat" (
    echo [BAŞARISIZ] stop-all.bat bulunamadı!
    goto :error
)
echo [OK] stop-all.bat mevcut.
echo.

echo [Test 5/5] Port kullanım kontrolü...
netstat -ano | findstr ":7047" | findstr "LISTENING" >nul
if %errorlevel% equ 0 (
    echo [UYARI] Port 7047 kullanımda! Backend zaten çalışıyor olabilir.
    echo          Durdurmak için: stop-all.bat
) else (
    echo [OK] Port 7047 boş.
)

netstat -ano | findstr ":4200" | findstr "LISTENING" >nul
if %errorlevel% equ 0 (
    echo [UYARI] Port 4200 kullanımda! Angular dev server zaten çalışıyor olabilir.
    echo          Durdurmak için: stop-all.bat
) else (
    echo [OK] Port 4200 boş.
)
echo.

echo ====================================================
echo    Tüm Testler Tamamlandı!
echo ====================================================
echo.
echo ✅ Node.js: Kurulu
echo ✅ npm: Kurulu
echo ✅ .NET SDK: Kurulu
echo ✅ Script dosyaları: Mevcut
echo ✅ Portlar: Kontrol edildi
echo.
echo Servisleri başlatmak için: start-all.bat
echo Servisleri durdurmak için: stop-all.bat
echo.
goto :end

:error
echo.
echo ====================================================
echo    Test Başarısız!
echo ====================================================
echo.
echo Lütfen yukarıdaki hata mesajlarını kontrol edin.
echo.
pause
exit /b 1

:end
pause

