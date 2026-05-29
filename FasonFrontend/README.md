# FasonApp (Angular 16.2) — Login-First

Bu proje, Login + Dashboard + **Fason Özet** sayfalarını içerir.
Giriş yapılmadan menü/uygulama görünmez. Login sonrası rota: **/dashboard**.

## Başlangıç
```bash
npm i -g @angular/cli@16
npm install
npm start   # ng serve
```
Uygulama: http://localhost:4200

## Rotalar
- **/login** (herkese açık, giriş yaptıysan otomatik **/dashboard**'a yönlendirir)
- **/dashboard**, **/fason-ozet** → Shell layout altında, **AuthGuard** ile korunur

## Konfig
- API tabanı: `src/environments/environment.ts` → `apiBaseUrl`
- Login endpoint: `POST /api/user/login` (AuthService → `user/login`)

## Güvenlik
- JWT token `localStorage.token`'da tutulur, `AuthInterceptor` otomatik **Authorization: Bearer** header'ı ekler.
