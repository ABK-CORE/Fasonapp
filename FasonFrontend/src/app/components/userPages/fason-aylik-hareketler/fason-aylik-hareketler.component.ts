import { Component, OnInit } from '@angular/core';
import { FasonOzetRow } from '../../../models/fason-ozet.models';
import { FasonOzetService } from '../../../services/fason-ozet.service';
import { exportToExcel } from 'src/app/utils/excel-export.util';
import { AuthService } from '../../../services/auth.service';
import { FasonFirmaService } from '../../../services/fason-firma.service';

@Component({
  selector: 'app-fason-aylik-hareketler',
  templateUrl: './fason-aylik-hareketler.component.html'
})
export class FasonAylikHareketlerComponent implements OnInit {
  showFasonCombo: boolean = true;
  fasonFirmalar: string[] = [];
  selectedFason: string = '';
  pendingSelectedFason: string = '';
  fasonSearchTerm: string = '';
  showFasonModal = false;
  firmaAdi: string = '';

  constructor(
    private service: FasonOzetService,
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
    this.currentMonth = this.getTurkishMonthName(new Date());
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

  exportExcel() {
    exportToExcel(this.rows, 'FasonAylikHareketler.xlsx');
  }
  rows: FasonOzetRow[] = [];
  busy = false;
  currentMonth: string = '';


  getTurkishMonthName(date: Date): string {
    const aylar = [
      'Ocak', 'Şubat', 'Mart', 'Nisan', 'Mayıs', 'Haziran',
      'Temmuz', 'Ağustos', 'Eylül', 'Ekim', 'Kasım', 'Aralık'
    ];
    return aylar[date.getMonth()];
  }

  private load(fasonIsim: string){
    this.busy = true;
    this.service.getSummary(fasonIsim).subscribe({
      next: data => {
        this.rows = data;
        this.busy = false;
      },
      error: err => {
        alert(err.message || 'Hata');
        this.busy = false;
      }
    });
  }

  private normalizeText(value: string): string {
    return value
      .toLocaleLowerCase('tr')
      .replace(/ı/g, 'i')
      .normalize('NFD')
      .replace(/[\u0300-\u036f]/g, '');
  }

}
