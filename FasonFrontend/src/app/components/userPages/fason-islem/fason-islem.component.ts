
import { Component, OnInit } from '@angular/core';
import * as QRCode from 'qrcode';
import { HttpClient } from '@angular/common/http';
import { environment } from 'src/environments/environment';
import { FasonUrunListeService } from '../../../services/fason-urun-liste.service';
import { FasonUrun } from '../../../models/fason-urun.models';

interface FasonIslemRow {
  UretimEmriNo: string;
  UretimTipi: string;
  UretilecekUrun: string;
  StokAdi: string;
  KoliciAdeti: number;
  Fasonis: string;
  FasonFirmaadi: string;
  LotNumarasi: string;
  Etiketsayisi: number;
  Barkodyazdi: boolean;
}

@Component({
  selector: 'app-fason-islem',
  templateUrl: './fason-islem.component.html'
})
export class FasonIslemComponent implements OnInit {
  showFasonCombo = false;
  fasonFirmalar: string[] = [];
  selectedFason: string = '';
  // Uygulama başlığındaki firma adı (ör: ANADOLU TEKSTİL)
  firmaBaslik: string = '';
  rows: FasonIslemRow[] = [];
  busy = false;
  showUrunModal = false;
  urunler: FasonUrun[] = [];
  urunlerBusy = false;

  // Barkod modalı ve yazıcı seçimi için ek alanlar
  showBarkodModal = false;
  barkodData: any = null;
  yazicilar: string[] = [];
  seciliYazici: string = '';

  form = {
    firmaAdi: '',
    stokKodu: '',
    stokAdi: '',
    koliIciAdet: 0,
    fasonIsi: '',
    kacKoli: 1,
    tarih: new Date().toISOString().slice(0,10)
  };

  fasonIsleri: string[] = [];

  constructor(private urunListeServis: FasonUrunListeService, private http: HttpClient) {}

  ngOnInit(): void {
    // Firma adı kontrolü (örnek: BalmyUretim)
    const userFirma = localStorage.getItem('FirmaAdı') || '';
    this.showFasonCombo = userFirma === 'BalmyUretim';
    if (this.showFasonCombo) {
      // Fason firma listesini API'den çek
      // Örnek: FasonFirmaService ile
      // this.fasonFirmaService.getFasonFirmalar().subscribe(firmalar => this.fasonFirmalar = firmalar);
      this.fasonFirmalar = ['ARZU ÇAVDAR', 'FASON2', 'FASON3']; // Dummy data, replace with API
    }
    // Firma adını üst başlıktan veya localStorage'dan al
    const title = document.querySelector('title')?.textContent || '';
    // Örnek başlık: FasonApp - anadolutekstil - ANADOLU TEKSTİL
    const match = title.match(/- ([^-]+)$/);
    this.firmaBaslik = match ? match[1].trim() : '';

    this.http.get<any[]>(environment.apiBaseUrl + '/api/FasonIslem/GetFasonIslemler').subscribe({
      next: data => {
        this.fasonIsleri = data.map(x => x.IslemAdi);
      },
      error: err => {
        this.fasonIsleri = [];
      }
    });
  }

  onFasonChange() {
    // Filtreleme için API çağrısı
    this.listele();
  }

  // Tarayıcıda yazıcıları listele (WebUSB/getPrinters API destekliyorsa)
  getYazicilar() {
    if ((navigator as any).getPrinters) {
      (navigator as any).getPrinters().then((printers: any[]) => {
        this.yazicilar = printers.map((p: any) => p.name);
        if (this.yazicilar.length > 0) this.seciliYazici = this.yazicilar[0];
      });
    } else {
      this.yazicilar = [];
      this.seciliYazici = '';
    }
  }

  barkodYaz(row: any) {
    this.barkodData = {
      stokKodu: row.UretilecekUrun,
      stokAdi: row.StokAdi,
      fasonis: row.Fasonis,
      lotNumarasi: row.LotNumarasi,
      firmaadi: this.firmaBaslik || row.FasonFirmaadi,
      miktar: row.KoliciAdeti,
      birim: 'ADET',
      etiketSayisi: row.Etiketsayisi || 1
    };
    this.getYazicilar();
    this.showBarkodModal = true;
    setTimeout(() => {
      // QR kodu lotNumarasi ile oluştur
      const canvas: any = document.getElementById('etiketQrCanvas');
      if (canvas && this.barkodData.lotNumarasi) {
        QRCode.toCanvas(canvas, this.barkodData.lotNumarasi, { width: 100, margin: 1 }, function (error: any) {
          if (error) console.error(error);
        });
      }
    }, 100);
  }

  kapatBarkodModal() {
    this.showBarkodModal = false;
    this.barkodData = null;
  }

  yazdirEtiket() {
    const printArea = document.getElementById('etiketPrintArea');
    if (!printArea) return;

    // Canvas'ı img'ye çevir
    const canvas = document.getElementById('etiketQrCanvas') as HTMLCanvasElement;
    let qrImgHtml = '';
    if (canvas) {
      const dataUrl = canvas.toDataURL();
      qrImgHtml = `<img src="${dataUrl}" style="display:block;margin:auto"/>`;
    }

    // Canvas'ı img ile değiştir
    const tempDiv = document.createElement('div');
    tempDiv.innerHTML = printArea.innerHTML.replace('<canvas id="etiketQrCanvas"></canvas>', qrImgHtml);

    const win = window.open('', '', 'width=400,height=600');
    if (win) {
      win.document.write('<html><head><title>Barkod Yazdır</title>');
      win.document.write('<style>body{margin:0;padding:0;} .etiket{font-family:Arial,sans-serif;}</style>');
      win.document.write('</head><body>');
      win.document.write(tempDiv.innerHTML);
      win.document.write('</body></html>');
      win.document.close();
      setTimeout(() => { win.print(); win.close(); }, 500);
    }
  }

  ara() {
    this.showUrunModal = true;
    this.urunlerBusy = true;
    this.urunListeServis.getUrunler().subscribe({
      next: data => { this.urunler = data; this.urunlerBusy = false; },
      error: err => { alert(err.message || 'Hata'); this.urunlerBusy = false; }
    });
  }

  kapatUrunModal() {
    this.showUrunModal = false;
  }

  ekle() {
    const body = {
      UrunKodu: this.form.stokKodu,
      YariMamulkodu: this.form.stokKodu,
      PlanlananMiktar: this.form.koliIciAdet,
      KoliAdeti: this.form.kacKoli,
      Fasonis: this.form.fasonIsi,
      Fasomsirket: this.form.firmaAdi,
      CreateUser: 'kullanici' // Giriş yapan kullanıcıdan alınabilir
    };
  this.http.post<any>(environment.apiBaseUrl + '/api/FasonUretim/CreateUretimEmriVeLot', body).subscribe({
      next: (res) => {
        alert('Kayıt başarıyla eklendi.');
        this.listele();
      },
      error: err => {
        alert('Kayıt eklenemedi: ' + (err.error?.Message || err.message));
      }
    });
  }

  listele() {
    const body = {
      Tarih: this.form.tarih
    };
  this.http.post<any[]>(environment.apiBaseUrl + '/api/FasonUretimGunluk/GetFasonUretimEmriGunluk', body).subscribe({
      next: (data) => {
        this.rows = (data || []).map(x => ({
          UretimEmriNo: x.UretimEmriNo,
          UretimTipi: x.UretimTipi || 'FASON',
          UretilecekUrun: x.UrunKodu,
          StokAdi: x.StokAdi,
          KoliciAdeti: x.PlanlananMiktar,
          Fasonis: x.Fasonis,
          FasonFirmaadi: x.Fasomsirket,
          LotNumarasi: x.LotNumarasi,
          Etiketsayisi: x.Etiketsayisi || 1,
          Barkodyazdi: !!x.Barkodyazdi
        }));
      },
      error: err => {
        this.rows = [];
        alert('Kayıtlar alınamadı: ' + (err.error?.Message || err.message));
      }
    });
  }

}
