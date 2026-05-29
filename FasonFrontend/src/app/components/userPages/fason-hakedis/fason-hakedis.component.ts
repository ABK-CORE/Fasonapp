import { environment } from 'src/environments/environment';
import { Component, OnInit } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { exportToExcel } from 'src/app/utils/excel-export.util';
import { AuthService } from 'src/app/services/auth.service';
import { FasonFirmaService } from 'src/app/services/fason-firma.service';

@Component({
  selector: 'app-fason-hakedis',
  templateUrl: './fason-hakedis.component.html'
})
export class FasonHakedisComponent implements OnInit {
  firmaAdi: string = '';
  showFasonCombo: boolean = true;
  fasonFirmalar: string[] = [];
  selectedFason: string = '';
  pendingSelectedFason: string = '';
  fasonSearchTerm: string = '';
  showFasonModal = false;
  toplamlar = {
    Ocak: 0,
    Subat: 0,
    Mart: 0,
    Nisan: 0,
    Mayis: 0,
    Haziran: 0,
    Temmuz: 0,
    Agustos: 0,
    Eylul: 0,
    // Yeni bölünmüş ay alanları
    Ekim_1_15_Hakedis: 0,
    Ekim_16_Son_Hakedis: 0,
    Ekim_Hakedis: 0,
    Kasim_1_15_Hakedis: 0,
    Kasim_16_Son_Hakedis: 0,
    Kasim_Hakedis: 0,
    Aralik_1_15_Hakedis: 0,
    Aralik_16_Son_Hakedis: 0,
    Aralik_Hakedis: 0,
    // Eski toplam anahtarları geriye dönük uyum için korunabilir (kullanılmıyor olsa da)
    Ekim: 0,
    Kasim: 0,
    Aralik: 0,
    YilToplam_Miktar: 0,
    YilToplam_Hakedis: 0
  };
  rows: any[] = [];
  busy = false;

  constructor(
    private http: HttpClient,
    private auth: AuthService,
    private fasonFirmaService: FasonFirmaService
  ) {
    const user = this.auth.currentUser;
    this.firmaAdi = (user?.CompanyName || '').trim();
    this.selectedFason = this.firmaAdi;
    if (this.auth.token) {
      this.fasonFirmaService.getFasonFirmalar(this.auth.token).subscribe(list => {
        const names = list
          .map(x => (x?.FasonFirmaadi ?? '').toString().trim())
          .filter(x => !!x);
        this.fasonFirmalar = Array.from(new Set(names)).sort((a, b) => a.localeCompare(b, 'tr'));
      });
    }
  }

  ngOnInit() {
    if (this.selectedFason) {
      this.load(this.selectedFason);
    }
  }

  get filteredFasonFirmalar(): string[] {
    if (!this.fasonSearchTerm?.trim()) {
      return this.fasonFirmalar;
    }

    const q = this.normalizeText(this.fasonSearchTerm);
    return this.fasonFirmalar.filter(f => this.normalizeText(f).includes(q));
  }

  openFasonModal() {
    this.pendingSelectedFason = this.selectedFason;
    this.fasonSearchTerm = '';
    this.showFasonModal = true;
  }

  closeFasonModal() {
    this.showFasonModal = false;
    this.fasonSearchTerm = '';
  }

  choosePendingFason(name: string) {
    this.pendingSelectedFason = name;
  }

  confirmFasonSelection() {
    if (!this.pendingSelectedFason?.trim()) {
      alert('Lutfen bir fason firma secin.');
      return;
    }

    this.selectedFason = this.pendingSelectedFason.trim();
    this.closeFasonModal();
  }

  goster() {
    const fason = this.selectedFason?.trim();
    if (!fason) {
      this.rows = [];
      alert('Lutfen fason firma secmek icin Fason Ara butonunu kullanin.');
      return;
    }

    this.load(fason);
  }

  private load(fasonIsim: string) {
    this.busy = true;
    const url = environment.apiBaseUrl + '/Fason/GetFasonDashboardYTDateFiyatli?fasonIsim=' + encodeURIComponent(fasonIsim);
    const headers = new HttpHeaders({
      'Content-Type': 'application/json',
      'Accept': 'application/json',
      'Authorization': 'Bearer ' + (this.auth.token || '')
    });
    this.http.get<any[]>(url, { headers }).subscribe({
      next: data => {
        this.rows = data;
        // Ay toplamlarını hesapla
        this.toplamlar.Ocak = data.reduce((acc, r) => acc + (r.Ocak_Hakedis || 0), 0);
        this.toplamlar.Subat = data.reduce((acc, r) => acc + (r.Subat_Hakedis || 0), 0);
        this.toplamlar.Mart = data.reduce((acc, r) => acc + (r.Mart_Hakedis || 0), 0);
        this.toplamlar.Nisan = data.reduce((acc, r) => acc + (r.Nisan_Hakedis || 0), 0);
        this.toplamlar.Mayis = data.reduce((acc, r) => acc + (r.Mayis_Hakedis || 0), 0);
        this.toplamlar.Haziran = data.reduce((acc, r) => acc + (r.Haziran_Hakedis || 0), 0);
        this.toplamlar.Temmuz = data.reduce((acc, r) => acc + (r.Temmuz_Hakedis || 0), 0);
        this.toplamlar.Agustos = data.reduce((acc, r) => acc + (r.Agustos_Hakedis || 0), 0);
        this.toplamlar.Eylul = data.reduce((acc, r) => acc + (r.Eylul_Hakedis || 0), 0);
  // Yeni bölünmüş ay toplamları
  this.toplamlar.Ekim_1_15_Hakedis = data.reduce((acc, r) => acc + (r.Ekim_1_15_Hakedis || 0), 0);
  this.toplamlar.Ekim_16_Son_Hakedis = data.reduce((acc, r) => acc + (r.Ekim_16_Son_Hakedis || 0), 0);
  this.toplamlar.Ekim_Hakedis = data.reduce((acc, r) => acc + (r.Ekim_Hakedis || 0), 0);
  this.toplamlar.Kasim_1_15_Hakedis = data.reduce((acc, r) => acc + (r.Kasim_1_15_Hakedis || 0), 0);
  this.toplamlar.Kasim_16_Son_Hakedis = data.reduce((acc, r) => acc + (r.Kasim_16_Son_Hakedis || 0), 0);
  this.toplamlar.Kasim_Hakedis = data.reduce((acc, r) => acc + (r.Kasim_Hakedis || 0), 0);
  this.toplamlar.Aralik_1_15_Hakedis = data.reduce((acc, r) => acc + (r.Aralik_1_15_Hakedis || 0), 0);
  this.toplamlar.Aralik_16_Son_Hakedis = data.reduce((acc, r) => acc + (r.Aralik_16_Son_Hakedis || 0), 0);
  this.toplamlar.Aralik_Hakedis = data.reduce((acc, r) => acc + (r.Aralik_Hakedis || 0), 0);
  // Geriye dönük uyum için eski toplam alanları da güncel tutalım
  this.toplamlar.Ekim = this.toplamlar.Ekim_Hakedis;
  this.toplamlar.Kasim = this.toplamlar.Kasim_Hakedis;
  this.toplamlar.Aralik = this.toplamlar.Aralik_Hakedis;
        this.toplamlar.YilToplam_Miktar = data.reduce((acc, r) => acc + (r.YilToplam_Miktar || 0), 0);
        this.toplamlar.YilToplam_Hakedis = data.reduce((acc, r) => acc + (r.YilToplam_Hakedis || 0), 0);
        this.busy = false;
      },
      error: err => { alert('Hata: ' + err.message); this.busy = false; }
    });
  }

  private normalizeText(value: string): string {
    return value
      .toLocaleLowerCase('tr')
      .replace(/ı/g, 'i')
      .normalize('NFD')
      .replace(/[\u0300-\u036f]/g, '');
  }

  exportExcel() {
    exportToExcel(this.rows, 'FasonHakediş.xlsx');
  }

}
