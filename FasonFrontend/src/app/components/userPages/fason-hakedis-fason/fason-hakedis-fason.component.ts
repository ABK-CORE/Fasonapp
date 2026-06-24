import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import * as XLSX from 'xlsx';
import { AuthService } from '../../../services/auth.service';
import { FasonFirmaService } from '../../../services/fason-firma.service';

@Component({
  selector: 'app-fason-hakedis-fason',
  templateUrl: './fason-hakedis-fason.component.html',
  styleUrls: ['./fason-hakedis-fason.component.css']
})
export class FasonHakedisFasonComponent {
  showControls = false;
  fasonFirmalar: string[] = [];
  selectedFason: string = '';
  dashboardData: any[] = [];

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
    const url = `https://fasonback.norax.ai/api/Fason/GetFasonDashboardYTDateMiktarliFason?fasonisim=${encodeURIComponent(this.selectedFason)}`;
    this.http.get<any[]>(url).subscribe(data => {
      this.dashboardData = data;
    }, err => {
      alert('Veri alınamadı!');
    });
  }

  excelExport() {
    if (!this.dashboardData.length) {
      alert('Tablo verisi yok!');
      return;
    }
    const ws: XLSX.WorkSheet = XLSX.utils.json_to_sheet(this.dashboardData);
    const wb: XLSX.WorkBook = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, 'FasonHakedisFason');
    XLSX.writeFile(wb, 'fason-hakedis-fason.xlsx');
  }
}
