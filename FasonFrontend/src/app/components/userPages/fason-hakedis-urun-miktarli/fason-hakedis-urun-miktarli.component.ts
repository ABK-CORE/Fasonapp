import { Component } from '@angular/core';
import { AuthService } from '../../../services/auth.service';
import { HttpClient } from '@angular/common/http';
import { FasonFirmaService } from '../../../services/fason-firma.service';
import { environment } from '../../../../environments/environment';


@Component({
  selector: 'app-fason-hakedis-urun-miktarli',
  templateUrl: './fason-hakedis-urun-miktarli.component.html',
  styleUrls: ['./fason-hakedis-urun-miktarli.component.css']
})
export class FasonHakedisUrunMiktarliComponent {
  fasonFirmalar: string[] = [];
  selectedFason: string = '';
  pendingSelectedFason: string = '';
  fasonSearchTerm: string = '';
  showFasonModal = false;
  showFasonSearch = false;
  urunGruplari: any[] = [];
  firmaAdi: string = '';
  loading = false;

  constructor(private auth: AuthService, private fasonFirmaService: FasonFirmaService, private http: HttpClient) {
    const user = this.auth.currentUser;
    this.firmaAdi = (user?.CompanyName || '').trim();
    this.showFasonSearch = true;
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

  goster(): void {
    const fason = this.selectedFason?.trim();
    if (!fason) {
      this.urunGruplari = [];
      alert('Lutfen fason firma secmek icin Fason Ara butonunu kullanin.');
      return;
    }

    this.loading = true;
    const url = `${environment.apiBaseUrl}/Fason/GetFasonDashboardYTDateMiktarli?fasonIsim=${encodeURIComponent(fason)}`;
    this.http.get<any[]>(url).subscribe(data => {
      this.urunGruplari = data;
      this.loading = false;
    }, () => {
      this.loading = false;
      this.urunGruplari = [];
      alert('Veri alınamadı!');
    });
  }

  private normalizeText(value: string): string {
    return value
      .toLocaleLowerCase('tr')
      .replace(/ı/g, 'i')
      .normalize('NFD')
      .replace(/[\u0300-\u036f]/g, '');
  }

  get toplamAylik() {
      const aylar = [
        'Ocak_Miktar', 'Subat_Miktar', 'Mart_Miktar', 'Nisan_Miktar', 'Mayis_Miktar',
        'Haziran_Miktar', 'Temmuz_Miktar', 'Agustos_Miktar', 'Eylul_Miktar',
        'Ekim_Miktar', 'Kasim_Miktar', 'Aralik_Miktar'
      ];
      return aylar.map(ay => this.urunGruplari.reduce((sum, ug) => sum + (ug[ay] || 0), 0));
  }
    get yilToplam() {
      return this.urunGruplari.reduce((sum, ug) => sum + (ug.YilToplam_Miktar || 0), 0);
    }
    get yilToplamHakedis() {
      return this.urunGruplari.reduce((sum, ug) => sum + (ug.YilToplam_Hakedis || 0), 0);
    }
  excelExport() {
    // Excel export fonksiyonu buraya eklenecek
    alert('Excel export!');
  }
}
