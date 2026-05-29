# Domain ve CORS Konfigurasyonu (Deploy Etmeden)

Bu dokuman, su hedefe gore ayarlanmistir:

- Frontend domain: https://fason.abkcore.com
- Backend API domain: https://fasonback.abkcore.com

Bu asamada deploy yapilmayacak. Sadece konfigurasyon ve kontrol adimlari netlestirilmistir.

## 1) Incelenen Dosyalar

- deploy log/notu: deployement.txt
- backend config: WebApi/appsettings.json
- backend startup/cors policy: WebApi/Program.cs
- frontend production env: FasonFrontend/src/environments/environment.prod.ts

## 2) Mevcut Durum Ozeti

### Frontend -> Backend endpoint

`FasonFrontend/src/environments/environment.prod.ts` icinde su an:

- apiBaseUrl: https://fasonback.abkcore.com/api

Hedef ile uyumlu.

### Backend CORS

`WebApi/appsettings.json` icinde su an:

- http://localhost:4200
- http://fason.abkcore.com
- https://fason.abkcore.com

`WebApi/Program.cs` tarafinda CORS, `Cors:Origins` listesinden okunup `WithOrigins(...)` ile uygulanmis.

Bu yapi, frontend domaini icin CORS tarafinda dogru yaklasimdir.

### deployement.txt bulgulari

`deployement.txt` notlarina gore:

- fason.abkcore.com: 200 donuyor (frontend acik)
- fasonback.abkcore.com: 443/80 ulasilabilir, root istekte 404 goruluyor

Not: Backend API icin root path 404 olmasi tek basina sorun degildir. API endpoint (ornek: `/api/...`) ile test edilmelidir.

## 3) Hedef Ayar (CORS Problemi Olusturmamak Icin)

Asagidaki degerler korunmali/uygulanmali:

### Frontend production

Dosya: `FasonFrontend/src/environments/environment.prod.ts`

```ts
apiBaseUrl: 'https://fasonback.abkcore.com/api'
```

### Backend CORS origins

Dosya: `WebApi/appsettings.json`

```json
"Cors": {
	"Origins": [
		"http://localhost:4200",
		"https://fason.abkcore.com",
		"http://fason.abkcore.com"
	]
}
```

Uretimde HTTPS kullanimina zorlanacaksa, ileride `http://fason.abkcore.com` kaldirilabilir.

## 4) Deploy Etmeden Dogrulama Checklist

1. `environment.prod.ts` icinde backend URL `https://fasonback.abkcore.com/api` mi?
2. `appsettings.json` icinde `https://fason.abkcore.com` CORS listesinde var mi?
3. `Program.cs` CORS policy `Cors:Origins` uzerinden mi yukleniyor?
4. DNS ve SSL notlari `deployement.txt` ile uyumlu mu?
5. Backend testini root yerine API endpoint ile planladik mi?

## 5) API Test Notu (Deploy Sonrasi Kullanilacak)

Deploy sonrasinda CORS testini su sekilde yapin:

- Browser Network tab: Origin `https://fason.abkcore.com`
- Request URL: `https://fasonback.abkcore.com/api/...`
- Beklenen: preflight (OPTIONS) ve asil request 2xx/401 (is kurali geregi)
- CORS headerlari: `Access-Control-Allow-Origin` degeri origin ile uyumlu olmali

---

Durum: Bu dokuman deploy yapmadan hazirlandi. Kodda aktif deploy islemi yapilmadi.

## 6) Canliya Gecis Oncesi 2 Dakikalik Hizli Dogrulama

Asagidaki adimlari sirasiyla kontrol edin:

1. Frontend production degiskeni dogru mu?
	- `FasonFrontend/src/environments/environment.prod.ts` -> `apiBaseUrl = https://fasonback.abkcore.com/api`
2. Backend CORS domaini dogru mu?
	- `WebApi/appsettings.json` -> `Cors:Origins` icinde `https://fason.abkcore.com` var
3. API endpoint gercekten cevap veriyor mu?
	- Sadece root degil, bir `/api/...` endpointi test edin
4. Tarayici preflight (OPTIONS) basarili mi?
	- Status 204/200 kabul
5. CORS header dogru mu?
	- `Access-Control-Allow-Origin: https://fason.abkcore.com`
6. Login akisi calisiyor mu?
	- Token aliniyor, sonraki isteklerde Authorization header gidiyor
7. Karisik icerik (mixed content) var mi?
	- Frontend HTTPS iken HTTP endpoint cagrisi olmamali

Bu 7 adim temizse, CORS kaynakli kesinti riski belirgin sekilde azalir.
