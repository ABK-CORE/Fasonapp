import { Component } from '@angular/core';
import { environment } from 'src/environments/environment';
import { AuthService } from '../../../services/auth.service';
import { FasonFirmaService } from '../../../services/fason-firma.service';
import { HttpClient } from '@angular/common/http';
import * as XLSX from 'xlsx';

@Component({
  selector: 'app-fason-hakedis-fiyatlari',
  templateUrl: './fason-hakedis-fiyatlari.component.html',
  styleUrls: ['./fason-hakedis-fiyatlari.component.css']
})
export class FasonHakedisFiyatlariComponent {
  showControls = false;
  fasonFirmalar: string[] = [];
  selectedFason: string = '';
  pendingSelectedFason: string = '';
  fasonSearchTerm: string = '';
  showFasonModal = false;
  fiyatData: any[] = [];
  baslangicTarihi: string = '';
  bitisTarihi: string = '';

  constructor(private auth: AuthService, private fasonFirmaService: FasonFirmaService, private http: HttpClient) {
    this.showControls = true;
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

  private normalizeText(value: string): string {
    return value
      .toLocaleLowerCase('tr')
      .replace(/ı/g, 'i')
      .normalize('NFD')
      .replace(/[\u0300-\u036f]/g, '');
  }

  goster() {
    const params: string[] = [];
    if (this.baslangicTarihi) params.push(`BaslangicTarihi=${this.baslangicTarihi}`);
    if (this.bitisTarihi) params.push(`BitisTarihi=${this.bitisTarihi}`);
    let url = '';
    if (!this.selectedFason) {
      url = `${environment.apiBaseUrl}/Fason/GetFasonHakedisAll`;
    } else {
      const q = [`fasonisim=${encodeURIComponent(this.selectedFason)}`].concat(params).join('&');
      url = `${environment.apiBaseUrl}/Fason/GetFasonHakedisByFasonisim?${q}`;
    }
    this.http.get<any[]>(url).subscribe(data => {
      this.fiyatData = data;
    }, err => {
      alert('Veri alınamadı!');
    });
  }

  excelExport() {
    if (!this.fiyatData.length) {
      alert('Tablo verisi yok!');
      return;
    }
    const ws: XLSX.WorkSheet = XLSX.utils.json_to_sheet(this.fiyatData);
    const wb: XLSX.WorkBook = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, 'FasonHakedisFiyatlari');
    XLSX.writeFile(wb, 'fason-hakedis-fiyatlari.xlsx');
  }
}
