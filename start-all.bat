@echo off
chcp 65001 >nul
echo ====================================================
echo    Fason Backend ^& Frontend Başlatılıyor...
echo ====================================================
echo.

REM Proje root dizinine git
cd /d "%~dp0"

REM Log dizini
if not exist "logs" (
    mkdir "logs" >nul 2>&1
)

set "FRONTEND_DIR="
set "BACKEND_DIR=%~dp0"

echo [1/4] Gerekli araclar kontrol ediliyor...
where node >nul 2>&1
if %errorlevel% neq 0 (
    echo [HATA] Node.js bulunamadı! Lütfen Node.js yükleyin.
    echo https://nodejs.org/
    pause
    exit /b 1
)

where npm >nul 2>&1
if %errorlevel% neq 0 (
    echo [HATA] npm bulunamadı! Lütfen Node.js yükleyin.
    pause
    exit /b 1
)

where dotnet >nul 2>&1
if %errorlevel% neq 0 (
    echo [HATA] dotnet bulunamadı! Lütfen .NET 8.0 SDK yükleyin.
    echo https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

echo [OK] Node.js versiyon:
node --version
echo [OK] npm versiyon:
npm --version
echo [OK] dotnet versiyon:
dotnet --version
echo.

REM Frontend klasoru otomatik tespit et
if not "%~1"=="" (
    set "FRONTEND_DIR=%~1"
) else (
    if exist "%~dp0FasonFrontend\package.json" (
        set "FRONTEND_DIR=%~dp0FasonFrontend"
    ) else if exist "%~dp0..\FasonFrontend-main\FasonFrontend-main\package.json" (
        set "FRONTEND_DIR=%~dp0..\FasonFrontend-main\FasonFrontend-main"
    )
)

if "%FRONTEND_DIR%"=="" (
    echo [HATA] Frontend klasoru bulunamadi.
    echo - Beklenen: "%~dp0FasonFrontend" veya "%~dp0..\FasonFrontend-main\FasonFrontend-main"
    echo - Alternatif: start-all.bat "C:\\Tam\\Yol\\Frontend"
    pause
    exit /b 1
)

echo [OK] Frontend dizini: "%FRONTEND_DIR%"
echo.

echo [2/4] Backend (.NET 8.0) build ediliyor...
dotnet build WebApi/WebApi.csproj --configuration Release --nologo >nul 2>&1
if %errorlevel% neq 0 (
    echo [HATA] Build başarısız!
    pause
    exit /b 1
)
echo [OK] Build başarılı.
echo.

echo [3/4] Backend başlatılıyor...
echo Backend Port: 7047 (HTTPS)
start "Fason Backend" cmd /k "cls ^& cd /d \"%BACKEND_DIR%\" ^& dotnet run --project WebApi --no-build --configuration Release 2^>^&1 ^| powershell -NoProfile -ExecutionPolicy Bypass -Command \"$input ^| Tee-Object -FilePath 'logs\\backend.log'\""
if %errorlevel% neq 0 (
    echo [HATA] Backend başlatılamadı!
    pause
    exit /b 1
)
echo [OK] Backend penceresi açıldı.
echo.

REM Backend'in başlaması için kısa bekleme
timeout /t 2 /nobreak >nul

echo [4/4] Frontend (Angular) başlatılıyor...
echo Frontend Port: 4200 (HTTP)
start "Fason Frontend" cmd /k "cls ^& cd /d \"%FRONTEND_DIR%\" ^& if not exist node_modules (echo [i] node_modules yok: npm install calisiyor... ^& if exist package-lock.json (npm ci) else (npm install)) ^& npm start 2^>^&1 ^| powershell -NoProfile -ExecutionPolicy Bypass -Command \"$input ^| Tee-Object -FilePath '%BACKEND_DIR%logs\\frontend.log'\""
if %errorlevel% neq 0 (
    echo [HATA] Frontend başlatılamadı!
    pause
    exit /b 1
)
echo [OK] Frontend penceresi açıldı.
echo.

echo ====================================================
echo    Tüm Servisler Başlatıldı!
echo ====================================================
echo.
echo Backend:   https://localhost:7047
echo Swagger:   https://localhost:7047/fason/index.html
echo Frontend:  http://localhost:4200
echo.
echo Servisleri durdurmak için stop-all.bat çalıştırın.
echo.
echo NOT: Backend ve Frontend ayrı CMD pencerelerinde çalışıyor.
echo      Logları görmek için o pencerelere bakın.
echo.
echo Bu pencereyi kapatabilirsiniz.
timeout /t 5
