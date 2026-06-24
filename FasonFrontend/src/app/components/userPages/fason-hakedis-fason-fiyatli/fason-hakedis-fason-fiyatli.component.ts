import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import * as XLSX from 'xlsx';
import { AuthService } from '../../../services/auth.service';
import { FasonFirmaService } from '../../../services/fason-firma.service';

@Component({
  selector: 'app-fason-hakedis-fason-fiyatli',
  templateUrl: './fason-hakedis-fason-fiyatli.component.html',
  styleUrls: ['./fason-hakedis-fason-fiyatli.component.css']
})
export class FasonHakedisFasonFiyatliComponent {
  showControls = false;
  fasonFirmalar: string[] = [];
  selectedFason: string = '';
  dashboardData: any[] = [];

  toplamlar: Record<string, number> = {
    Ocak_Hakedis: 0,
    Subat_Hakedis: 0,
    Mart_Hakedis: 0,
    Nisan_Hakedis: 0,
    Mayis_Hakedis: 0,
    Haziran_Hakedis: 0,
    Temmuz_Hakedis: 0,
    Agustos_Hakedis: 0,
    Eylul_Hakedis: 0,
    Ekim_Hakedis: 0,
    Kasim_Hakedis: 0,
    Aralik_Hakedis: 0,
    YilToplam_Miktar: 0,
    YilToplam_Hakedis: 0
  };

  constructor(private auth: AuthService, private fasonFirmaService: FasonFirmaService, private http: HttpClient) {
    const user = this.auth.currentUser;
    this.showControls = user?.CompanyName === 'BalmyUretim';
    if (this.showControls && this.auth.token) {
      this.fasonFirmaService.getFasonFirmalar(this.auth.token).subscribe(list => {
        this.fasonFirmalar = list.map(x => x.FasonFirmaadi);
      });
    }
  }

  goster() {
    const url = `https://fasonback.norax.ai/api/Fason/GetFasonDashboardYTDateMiktarliFasonFiyatli?fasonisim=${encodeURIComponent(this.selectedFason)}`;
    this.http.get<any[]>(url).subscribe(data => {
      this.dashboardData = data;
      this.hesaplaToplamlar();
    }, err => {
      alert('Veri alınamadı!');
    });
  }

  hesaplaToplamlar() {
    const keys = Object.keys(this.toplamlar);
    // Sıfırla
    keys.forEach(k => this.toplamlar[k] = 0);
    for (const row of this.dashboardData) {
      keys.forEach(k => {
        const val = Number(row[k]);
        if (!isNaN(val)) this.toplamlar[k] += val;
      });
    }
  }

  excelExport() {
    if (!this.dashboardData.length) {
      alert('Tablo verisi yok!');
      return;
    }
    const ws: XLSX.WorkSheet = XLSX.utils.json_to_sheet(this.dashboardData);
    const wb: XLSX.WorkBook = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, 'FasonHakedisFasonFiyatli');
    XLSX.writeFile(wb, 'fason-hakedis-fason-fiyatli.xlsx');
  }
}
