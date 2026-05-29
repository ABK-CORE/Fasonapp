import { Component, OnInit } from '@angular/core';
import { exportToExcel } from '../../../utils/excel-export.util';
import { FasonRaporTarihService } from '../../../services/fason-rapor-tarih.service';
import { FasonRaporTarihRow } from '../../../models/fason-rapor-tarih.models';
import { AuthService } from '../../../services/auth.service';
import { FasonFirmaService } from '../../../services/fason-firma.service';

@Component({
  selector: 'app-fason-ozet',
  templateUrl: './fason-ozet.component.html'
})
export class FasonOzetComponent implements OnInit {
  showFasonCombo: boolean = false;
  fasonFirmalar: string[] = [];
  selectedFason: string = '';
  firmaAdi: string = '';
  baslamaTarihi: string = '';
  bitisTarihi: string = '';
  fasonIsim: string = '';
  fasonSurec: string = 'FASONDANTESLIMAL';
  rows: FasonRaporTarihRow[] = [];
  busy = false;

  constructor(
    private service: FasonRaporTarihService,
    private auth: AuthService,
    private fasonFirmaService: FasonFirmaService
  ) {
    // Default both dates to today
    const today = new Date();
    const todayStr = this.formatDate(today);
    this.baslamaTarihi = todayStr;
    this.bitisTarihi = todayStr;
    const user = this.auth.currentUser;
    this.firmaAdi = user?.CompanyName || '';
    this.showFasonCombo = this.firmaAdi === 'BalmyUretim';
    if (this.showFasonCombo && this.auth.token) {
      this.fasonFirmaService.getFasonFirmalar(this.auth.token).subscribe(list => {
        this.fasonFirmalar = list.map((x: any) => x.FasonFirmaadi);
      });
    }
  }

  ngOnInit() {
  // Sayfa ilk açıldığında otomatik request atılmasın
  }

  onFasonChange() {
    this.fasonIsim = this.selectedFason;
    this.load();
  }

  formatDate(date: Date): string {
    return date.toISOString().slice(0, 10);
  }
  exportExcel() {
    if (this.rows && this.rows.length > 0) {
      exportToExcel(this.rows, 'FasonRaporTarihBazli');
    } else {
      alert('Export edilecek veri yok!');
    }
  }
  load(){
    let fasonIsimToSend = '';
    if (this.showFasonCombo) {
      // FirmaAdı BalmyUretim ise combobox'tan seçilen değeri gönder
      fasonIsimToSend = this.selectedFason;
    } else {
      // FirmaAdı BalmyUretim değilse, CompanyName'i gönder
      fasonIsimToSend = this.firmaAdi;
    }
    this.busy = true;
    this.service.getRaporTarihBazli(this.baslamaTarihi, this.bitisTarihi, fasonIsimToSend, this.fasonSurec).subscribe({
      next: (data: FasonRaporTarihRow[]) => { this.rows = data; this.busy = false; },
      error: (err: any) => { alert(err.message || 'Hata'); this.busy = false; }
    });
  }

}
