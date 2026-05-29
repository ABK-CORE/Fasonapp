import { Component } from '@angular/core';
import { AuthService } from '../../../services/auth.service';
import { FasonFirmaService } from '../../../services/fason-firma.service';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import * as XLSX from 'xlsx';

@Component({
  selector: 'app-fason-hakedis-kolili',
  templateUrl: './fason-hakedis-kolili.component.html',
  styleUrls: ['./fason-hakedis-kolili.component.css']
})
export class FasonHakedisKoliliComponent {
  showControls = true;
  fasonFirmalar: string[] = [];
  selectedFason: string = '';
  pendingSelectedFason: string = '';
  fasonSearchTerm: string = '';
  showFasonModal = false;
  showFasonSearch = false;
  baslamaTarihi: string = '';
  bitisTarihi: string = '';
  koliOzetData: any[] = [];
  firmaAdi: string = '';
  loading = false;

  constructor(private auth: AuthService, private fasonFirmaService: FasonFirmaService, private http: HttpClient) {
    const user = this.auth.currentUser;
    this.firmaAdi = (user?.CompanyName || '').trim();
    const now = new Date();
    const last30 = new Date(now);
    last30.setDate(last30.getDate() - 30);
    this.baslamaTarihi = this.toLocalDateInputValue(last30);
    this.bitisTarihi = this.toLocalDateInputValue(now);

    // CompanyName bazen token tarafinda bos gelebiliyor.
    // Bu durumda da firma listesini yukleyip secimden sonra sorgu yap.
    const shouldLoadFirmList = this.firmaAdi === 'BalmyUretim' || !this.firmaAdi;

    this.showFasonSearch = shouldLoadFirmList;

    if (shouldLoadFirmList && this.auth.token) {
      this.fasonFirmaService.getFasonFirmalar(this.auth.token).subscribe(list => {
        const names = list
          .map(x => (x?.FasonFirmaadi ?? '').toString().trim())
          .filter(x => !!x);
        this.fasonFirmalar = Array.from(new Set(names)).sort((a, b) => a.localeCompare(b, 'tr'));
      }, () => {
        this.koliOzetData = [];
      });
    } else {
      // Diger firmalar icin kendi firma adini secip tabloyu otomatik doldur.
      this.fasonFirmalar = [this.firmaAdi];
      this.selectedFason = this.firmaAdi;
      this.goster();
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

  private normalizeText(value: string): string {
    return value
      .toLocaleLowerCase('tr')
      .replace(/ı/g, 'i')
      .normalize('NFD')
      .replace(/[\u0300-\u036f]/g, '');
  }

  private toLocalDateInputValue(date: Date): string {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  excelExport() {
    if (!this.koliOzetData.length) {
      alert('Tablo verisi yok!');
      return;
    }
    const ws: XLSX.WorkSheet = XLSX.utils.json_to_sheet(this.koliOzetData);
    const wb: XLSX.WorkBook = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, 'FasonHakedisKolili');
    XLSX.writeFile(wb, 'fason-hakedis-kolili.xlsx');
  }

  goster() {
    if (!this.selectedFason?.trim()) {
      this.koliOzetData = [];
      alert('Lutfen fason firma secmek icin Fason Ara butonunu kullanin.');
      return;
    }

    this.loading = true;

    // Her iki durumda da seçili fason firma ile istek atılır
    const params = [
      `baslamatarihi=${this.baslamaTarihi}`,
      `bitistarihi=${this.bitisTarihi}`,
      `fasonsurec=FASONDANTESLIMAL`,
      `fasonisim=${encodeURIComponent(this.selectedFason.trim())}`
    ].join('&');
  const url = `${environment.apiBaseUrl}/Fason/GetFasonTakipFasonAppOzetKoli?${params}`;
    this.http.get<any[]>(url).subscribe(data => {
      this.koliOzetData = data;
      this.loading = false;
    }, err => {
      this.loading = false;
      this.koliOzetData = [];
      alert('Veri alınamadı!');
    });
  }
}
