# Fason Backend & Frontend

Fason Purchase Order Management System - Full Stack Application

## Proje Yapısı

- **Backend:** .NET 8.0 Web API (Port: 7047)
- **Frontend:** Angular 16 SPA (Port: 4200)

### Mimari

Backend: N-Katmanlı (N-Layer) Modüler Mimari
- **WebApi** - Presentation Layer (Controllers, Middleware)
- **Business** - Business Logic Layer (Services, Managers)
- **DataAccess** - Data Access Layer (Repositories, EF Core + Dapper)
- **Entities** - Entity & DTO Layer
- **Core** - Shared Infrastructure Layer
- **Infrastructure** - Cross-cutting Concerns (Middleware)
- **Shared** - Common Utilities (Caching, Constants)

Frontend: Angular 16 with TypeScript
- Component-based architecture
- JWT Authentication with interceptors
- Reactive forms and RxJS
- Bootstrap 5 UI

## Hızlı Başlangıç (Otomatik)

### Tüm Servisleri Tek Komutla Başlatma

```cmd
start-all.bat
```

Bu script:
- ✅ Node.js ve npm kontrolü yapar
- ✅ Angular paketlerini otomatik kontrol eder ve günceller
- ✅ Backend'i başlatır (Port 7047)
- ✅ Frontend'i başlatır (Port 4200)
- ✅ Tarayıcıyı otomatik açar

### Tüm Servisleri Durdurma

```cmd
stop-all.bat
```

**Detaylı bilgi:** `STARTUP_SCRIPTS_GUIDE.md`

## Geliştirme Ortamı Kurulumu

### Gereksinimler

- .NET 8.0 SDK
- Node.js 18+ ve npm
- SQL Server
- Visual Studio 2022 veya VS Code
- Angular CLI: `npm install -g @angular/cli`

### Backend Kurulumu

1. Visual Studio 2022 veya VS Code ile `FasonBackend.sln` açın
2. NuGet paketlerini restore edin
3. `WebApi/appsettings.json` dosyasını yapılandırın (connection string, JWT settings)
4. Database migration'ları uygulayın (gerekirse)
5. F5 ile çalıştırın veya terminal'den:
   ```bash
   cd WebApi
   dotnet run
   ```
6. Backend şu adreste çalışacak: https://localhost:7047

### Frontend Kurulumu

1. Frontend klasörüne gidin:
   ```bash
   cd FasonFrontend
   ```

2. Bağımlılıkları yükleyin:
   ```bash
   npm install
   ```

3. Development server'ı başlatın:
   ```bash
   npm start
   # veya
   ng serve
   ```

4. Frontend şu adreste çalışacak: http://localhost:4200

### İlk Çalıştırma

**Terminal 1 - Backend:**
```bash
cd C:\Projeler\FasonBackend-master
dotnet run --project WebApi
```

**Terminal 2 - Frontend:**
```bash
cd C:\Projeler\FasonBackend-master\FasonFrontend
ng serve
```

**Erişim:**
- Frontend: http://localhost:4200
- Backend API: https://localhost:7047/api
- Swagger: https://localhost:7047/fason/index.html (sadece DEBUG modunda)

## Production Build

### Backend

1. Visual Studio'da Release configuration seçin
2. Publish yapın veya:
   ```bash
   dotnet publish -c Release
   ```

### Frontend

```bash
cd FasonFrontend
ng build --configuration production
```

Build çıktısı: `FasonFrontend/dist/fason-app/` klasörüne oluşur.

## API İletişimi

- **Development:** Angular → https://localhost:7047/api
- **Production:** Angular → https://fasonback.abkcore.com/api

## Authentication

JWT Bearer token authentication kullanılmaktadır:
- Token localStorage'da saklanır
- Her request'e AuthInterceptor tarafından otomatik eklenir
- Token expiration: 1140 dakika (19 saat)

### Login Flow

1. Kullanıcı email/password ile giriş yapar
2. Backend JWT token ve kullanıcı bilgilerini döner
3. Frontend token'ı localStorage'a kaydeder
4. Sonraki tüm request'lerde Authorization header'ına token eklenir

## Teknolojiler

### Backend

- .NET 8.0
- ASP.NET Core Web API
- Entity Framework Core 8.0
- Dapper 2.1
- Autofac (Dependency Injection)
- AutoMapper
- JWT Bearer Authentication
- Serilog (Logging)
- Swagger/OpenAPI
- SQL Server

### Frontend

- Angular 16
- TypeScript 5.1
- Bootstrap 5.3
- RxJS 7.8
- Chart.js 4.4 (ng2-charts)
- XLSX (Excel export)
- QRCode generation

## Proje Yapısı Detayı

```
FasonBackend-master/
├── Business/              # Business logic layer
│   ├── Abstract/         # Service interfaces
│   ├── Concrete/         # Manager implementations
│   └── DependencyRepository/
├── Core/                 # Shared infrastructure
│   ├── DataAccess/       # Generic repository pattern
│   ├── Entities/         # Base entity interfaces
│   └── Utilities/        # Security, JWT, Result pattern
├── DataAccess/           # Data access layer
│   ├── Abstract/         # Repository interfaces
│   └── Concrete/
│       ├── EntityFramework/  # EF Core repositories
│       └── Dapper/           # Dapper repositories
├── Entities/             # Domain entities & DTOs
│   ├── Concrete/         # Entity classes
│   └── Dtos/             # Data transfer objects
├── WebApi/               # API presentation layer
│   ├── Controllers/      # API endpoints
│   └── appsettings.json  # Configuration
├── Infrastructure/       # Middleware & cross-cutting
├── Shared/              # Shared utilities
├── FasonFrontend/       # Angular frontend
│   ├── src/
│   │   ├── app/
│   │   │   ├── components/    # UI components
│   │   │   ├── services/      # API services
│   │   │   ├── guards/        # Route guards
│   │   │   ├── interceptors/  # HTTP interceptors
│   │   │   ├── models/        # TypeScript models
│   │   │   └── layout/        # Shell, navbar, sidebar
│   │   └── environments/      # Environment configs
│   ├── angular.json
│   └── package.json
└── FasonBackend.sln     # Solution file
```

## API Response Format

Backend'den dönen tüm response'lar standart formatta:

```typescript
{
  success: boolean,
  message: string,
  data: T
}
```

## CORS Yapılandırması

Backend `appsettings.json` içinde CORS origins tanımlı:

```json
{
  "Cors": {
    "Origins": [
      "http://localhost:4200",
      "http://fason.abkcore.com",
      "https://fason.abkcore.com"
    ]
  }
}
```

## Troubleshooting

### CORS Hatası
- Backend'in çalıştığından emin olun
- `appsettings.json` CORS ayarlarını kontrol edin
- Browser console'da hata detaylarını inceleyin

### 401 Unauthorized
- Token'ın localStorage'da olduğunu kontrol edin (F12 → Application → Local Storage)
- Token expire olmuş olabilir, tekrar login yapın
- Backend'de JWT ayarlarını kontrol edin

### API Connection Error
- Backend'in çalıştığından emin olun (https://localhost:7047/api)
- Frontend environment dosyasındaki API URL'ini kontrol edin
- Network tab'da request'leri inceleyin

### SSL Certificate Uyarısı
Development'ta backend HTTPS kullanıyor. Tarayıcıdan:
1. "Advanced" → "Proceed to localhost" yapın
2. Veya development sertifikasını trust edin:
   ```bash
   dotnet dev-certs https --trust
   ```

### Port Çakışması
- Backend için 7047 portu kullanımda olmamalı
- Frontend için 4200 portu kullanımda olmamalı
- Gerekirse `angular.json` veya `launchSettings.json` dosyalarından port değiştirin

## Ortam Konfigürasyonları

### Backend

- **Debug** - Development ortamı (Swagger aktif)
- **Developer** - Developer ortamı
- **LIVE** - Production ortamı
- **Release** - Release build

### Frontend

- **Development** - `environment.ts` (localhost:7047)
- **Production** - `environment.prod.ts` (fasonback.abkcore.com)

## Güvenlik

- Connection string ve JWT SecurityKey AES ile şifrelenmiş
- Password'ler hash'lenmiş olarak saklanır
- JWT token ile stateless authentication
- CORS policy ile origin kontrolü
- Global exception handling
- Request/response logging
- User activity logging

## Lisans

Private - ABK Core Teknoloji

## İletişim

- Company: ABK Core Teknoloji
- Email: info@abkcore.com
