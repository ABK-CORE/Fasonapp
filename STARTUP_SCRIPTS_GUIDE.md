# Otomatik Başlatma ve Durdurma Scriptleri

Bu rehber, Fason Backend ve Frontend projelerini tek komutla başlatma ve durdurma script'lerini açıklar.

## 📜 Script Dosyaları

- **start-all.bat** - Backend ve Frontend'i başlatır
- **stop-all.bat** - Tüm servisleri durdurur

## 🚀 Kullanım

### Tüm Servisleri Başlatma

```cmd
start-all.bat
```

**Script şunları yapar:**

1. ✅ Node.js ve npm kurulu mu kontrol eder
2. ✅ Angular paketlerini kontrol eder
   - `node_modules` yoksa → `npm install` çalıştırır
   - `package.json` güncellenmiş mi kontrol eder → Gerekirse `npm install` yapar
3. ✅ Backend'i yeni bir pencerede başlatır (Port 7047)
4. ✅ Frontend'i yeni bir pencerede başlatır (Port 4200)
5. ✅ Tarayıcıyı otomatik açar (--open flag)

**Başlatma Sonrası:**
- 2 yeni CMD penceresi açılır:
  - "Fason Backend" - Backend logs
  - "Fason Frontend" - Angular dev server logs
- Log dosyaları `logs/` klasörüne kaydedilir

### Tüm Servisleri Durdurma

```cmd
stop-all.bat
```

**Script şunları yapar:**

1. ✅ Backend pencerelerini kapatır
2. ✅ Frontend pencerelerini kapatır
3. ✅ Port 7047'deki tüm process'leri durdurur (Backend)
4. ✅ Port 4200'deki tüm process'leri durdurur (Angular)
5. ✅ Kalan node.exe ve dotnet.exe process'lerini temizler

## 📊 Çıktı Örnekleri

### start-all.bat Başarılı Çıktı

```
====================================================
   Fason Backend & Frontend Başlatılıyor...
====================================================

[1/4] Node.js ve npm kontrolü yapılıyor...
[OK] Node.js versiyon:
v18.16.0
[OK] npm versiyon:
9.5.1

[2/4] Frontend Angular paketleri kontrol ediliyor...
[OK] node_modules mevcut.
[OK] Paketler güncel.

[3/4] Backend (.NET 8.0) başlatılıyor...
Backend Port: 7047 (HTTPS)
Log: logs\backend.log
[OK] Backend başlatıldı.

[4/4] Frontend (Angular) başlatılıyor...
Frontend Port: 4200 (HTTP)
Log: logs\frontend.log
[OK] Frontend başlatıldı.

====================================================
   Tüm Servisler Başlatıldı!
====================================================

Backend:   https://localhost:7047
Swagger:   https://localhost:7047/fason/index.html
Frontend:  http://localhost:4200

Servisleri durdurmak için stop-all.bat çalıştırın.
```

### stop-all.bat Başarılı Çıktı

```
====================================================
   Fason Backend & Frontend Durduruluyor...
====================================================

[1/4] Backend .NET process'leri durduruluyor...
[OK] Backend penceresi kapatıldı.
[OK] Port 7047'deki process durduruldu.

[2/4] Frontend Angular process'leri durduruluyor...
[OK] Frontend penceresi kapatıldı.
[OK] Port 4200'deki Angular process durduruldu.

[3/4] Kalan Node.js process'leri kontrol ediliyor...
[OK] Node.js process'leri zaten temiz.

[4/4] Port temizliği yapılıyor...
[OK] Port 7047 temiz.
[OK] Port 4200 temiz.

====================================================
   Tüm Servisler Durduruldu!
====================================================

Port 7047 (Backend):  Kapalı
Port 4200 (Frontend): Kapalı
```

## 🎯 Özellikler

### start-all.bat Özellikleri

✅ **Akıllı Paket Yönetimi**
- node_modules yoksa otomatik `npm install`
- package.json değişmiş mi kontrol eder
- Güncellenmiş paketleri otomatik yükler

✅ **Hata Kontrolleri**
- Node.js kurulu mu?
- npm kurulu mu?
- npm install başarılı mı?

✅ **Log Yönetimi**
- Backend logs: `logs/backend.log`
- Frontend logs: `logs/frontend.log`
- Tüm output hem ekranda hem log dosyasında

✅ **Otomatik Tarayıcı**
- Frontend başlatılınca tarayıcı otomatik açılır
- http://localhost:4200 adresi

### stop-all.bat Özellikleri

✅ **Kapsamlı Temizlik**
- Window title bazlı process durdurma
- Port bazlı process durdurma
- PID bazlı process durdurma

✅ **Güvenli Durdurma**
- Önce pencereler kapatılır
- Sonra process'ler durdurulur
- Son olarak portlar temizlenir

✅ **Çoklu Kontrol**
- Birden fazla metotla process bulma
- Kalan process'leri temizleme
- Port çakışması önleme

## 🔧 Sorun Giderme

### "Node.js bulunamadı" Hatası

```
[HATA] Node.js bulunamadı! Lütfen Node.js yükleyin.
```

**Çözüm:**
1. Node.js'i indirin: https://nodejs.org/
2. Kurulum sonrası terminal'i yeniden açın
3. `node --version` ile kontrol edin

### "npm install başarısız" Hatası

```
[HATA] npm install başarısız oldu!
```

**Çözüm:**
1. Manuel olarak kontrol edin:
   ```cmd
   cd FasonFrontend
   npm install
   ```
2. Hata mesajını okuyun
3. Genelde internet bağlantısı veya registry sorunu

### Port Çakışması

```
[!] Port 7047 hala kullanımda
```

**Çözüm:**
1. `stop-all.bat` tekrar çalıştırın
2. Manuel kontrol:
   ```cmd
   netstat -ano | findstr "7047"
   netstat -ano | findstr "4200"
   ```
3. PID'leri not alıp manuel durdurun:
   ```cmd
   taskkill /PID <PID> /F
   ```

### Backend/Frontend Başlamıyor

**Kontrol Listesi:**
1. ✅ .NET 8.0 SDK kurulu mu? → `dotnet --version`
2. ✅ Node.js kurulu mu? → `node --version`
3. ✅ npm kurulu mu? → `npm --version`
4. ✅ Portlar boş mu? → `netstat -ano | findstr "7047"`
5. ✅ Proje root klasöründe misiniz?

### Pencereler Açılmıyor

**Manuel Başlatma:**

Backend:
```cmd
cd C:\Projeler\FasonBackend-master
dotnet run --project WebApi
```

Frontend:
```cmd
cd C:\Projeler\FasonBackend-master\FasonFrontend
ng serve --open
```

## 📁 Log Dosyaları

Script'ler otomatik log dosyaları oluşturur:

```
FasonBackend-master/
├── logs/
│   ├── backend.log      # Backend çıktıları
│   └── frontend.log     # Angular dev server çıktıları
├── start-all.bat
└── stop-all.bat
```

**Log İnceleme:**
```cmd
# Backend logları
type logs\backend.log

# Frontend logları
type logs\frontend.log

# Son 20 satır
powershell "Get-Content logs\backend.log -Tail 20"
```

## 🔄 Yeniden Başlatma

Servisleri yeniden başlatmak için:

```cmd
stop-all.bat
timeout /t 2
start-all.bat
```

Veya tek komutta:
```cmd
stop-all.bat && timeout /t 2 && start-all.bat
```

## 💡 İpuçları

1. **Hızlı Başlatma:** `start-all.bat` dosyasına çift tıklayın
2. **Hızlı Durdurma:** `stop-all.bat` dosyasına çift tıklayın
3. **Masaüstü Kısayolu:** Script'lere masaüstünden kısayol oluşturun
4. **Görev Çubuğuna Sabitle:** Hızlı erişim için
5. **Log İzleme:** Ayrı bir terminal'de `tail -f logs/backend.log` ile canlı izleyin

## 🚨 Önemli Notlar

### npm Paket Yönetimi

Script otomatik olarak şunları yapar:
- ✅ `node_modules` yoksa → `npm install`
- ✅ `package.json` değiştiyse → `npm install`
- ✅ Paketler güncel → Direkt başlatır

### Manuel npm install

Eğer manuel `npm install` yapmak isterseniz:

```cmd
cd FasonFrontend
npm install
cd ..
start-all.bat
```

### Angular CLI

Script `ng serve` komutunu kullanır. Bu nedence:
- Angular CLI global yüklü olmalı: `npm install -g @angular/cli`
- Veya local kullanır: `FasonFrontend/node_modules/.bin/ng`

### Backend SSL Sertifikası

İlk çalıştırmada tarayıcı SSL uyarısı verebilir:

```cmd
dotnet dev-certs https --trust
```

## 📖 İlgili Dokümantasyon

- Ana README: `README.md`
- Entegrasyon Raporu: `INTEGRATION_SETUP_COMPLETE.md`
- Proje Yapısı: `README.md` → Proje Yapısı Detayı

## 🆘 Destek

Sorun yaşarsanız:
1. Log dosyalarını kontrol edin (`logs/`)
2. Manuel başlatma deneyin
3. Port kullanımını kontrol edin
4. Process'leri manuel temizleyin

---

**Script Versiyonu:** 1.0  
**Oluşturma Tarihi:** 2026-01-04  
**Test Ortamı:** Windows 10/11, PowerShell 5.1+

