
import { Component, OnInit } from '@angular/core';
import { exportToExcel } from '../../../utils/excel-export.util';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { FasonIsService } from '../../../services/fason-is.service';
import { FasonFirmaService } from '../../../services/fason-firma.service';
import { AuthService } from '../../../services/auth.service';
import { SurecDegistirLotService, SurecDegistirLotRequest } from '../../../services/surec-degistir-lot.service';
import { DeleteLotService } from '../../../services/delete-lot.service';
import { ReassignLotToNewFasonYuklemeService, ReassignLotToNewFasonYuklemeRequest } from '../../../services/reassign-lot-to-new-fason-yukleme.service';
import { UpdateUretilenMiktarByLotService, UpdateUretilenMiktarByLotRequest } from '../../../services/update-uretilen-miktar-by-lot.service';


@Component({
  selector: 'app-fason-mutabak',
  templateUrl: './fason-mutabak.component.html',
  styleUrls: ['./fason-mutabak.component.css']
})

export class FasonMutabakComponent implements OnInit {
  accessDenied = false;
  exportBosFasonToExcel() {
    if (this.bosFasonList && this.bosFasonList.length) {
      exportToExcel(this.bosFasonList, 'bos-fason-hakedis.xlsx');
    }
  }
  bosFasonForm!: FormGroup;
  bosFasonList: any[] = [];
  bosFasonLoading: boolean = false;
  bosFasonError: string = '';
  bosFasonKeys: string[] = [];
  mutabakForm!: FormGroup;
  lotSilForm!: FormGroup;
  lotTarihiDegistirForm!: FormGroup;
  surecDegistirForm!: FormGroup;
  lotMiktariGuncelleForm!: FormGroup;

  result: any = null;
  loading = false;
  error: string = '';
  activeTab: string = 'lot';

  fasonIsler: any[] = [];
  fasonIslerLoading = false;
  fasonIslerError = '';

  fasonFirmalar: string[] = [];
  fasonFirmalarLoading = false;
  fasonFirmalarError = '';

  lotSilResult: any = null;
  lotSilLoading = false;
  lotSilError = '';

  lotTarihiDegistirResult: any = null;
  lotTarihiDegistirLoading = false;
  lotTarihiDegistirError = '';

  surecDegistirResult: any = null;
  surecDegistirLoading = false;
  surecDegistirError = '';

  lotMiktariGuncelleResult: any = null;
  lotMiktariGuncelleLoading = false;
  lotMiktariGuncelleError = '';

  constructor(
    private fb: FormBuilder,
    private http: HttpClient,
    private fasonIsService: FasonIsService,
    private fasonFirmaService: FasonFirmaService,
    private auth: AuthService,
    private surecDegistirLotService: SurecDegistirLotService,
    private deleteLotService: DeleteLotService,
    private reassignLotToNewFasonYuklemeService: ReassignLotToNewFasonYuklemeService,
    private updateUretilenMiktarByLotService: UpdateUretilenMiktarByLotService
  ) {}

  // access control, form init gibi işlemler ngOnInit'te
 getBosFasonHakedis() {
    if (this.bosFasonForm.invalid) return;
    this.bosFasonLoading = true;
    this.bosFasonError = '';
    this.bosFasonList = [];
    const gunSayisi = this.bosFasonForm.value.gunSayisi;
    this.http.get<any[]>(`https://fasonback.abkcore.com/api/Fason/GetBosFasonHakedis?gunSayisi=${gunSayisi}`).subscribe({
      next: (res) => {
        this.bosFasonList = res;
        this.bosFasonKeys = res && res.length ? Object.keys(res[0]) : [];
        this.bosFasonLoading = false;
      },
      error: (err) => {
        this.bosFasonError = err?.error?.Message || 'Bir hata oluştu';
        this.bosFasonLoading = false;
      }
    });
  }
  ngOnInit() {
  const rawUsername = this.auth.currentUser?.Username ?? '';
  const username = String(rawUsername).toLowerCase().trim();
  const deniedUsers = new Set(['ozkandemirci', 'cuneyt']);
  this.accessDenied = deniedUsers.has(username);
    if (this.accessDenied) {
      return; // erişim yoksa formları başlatma
    }
    this.bosFasonForm = this.fb.group({
      gunSayisi: [40, [Validators.required, Validators.min(1)]]
    });
    // getBosFasonHakedis fonksiyonu class'ın methodları arasında olacak
  // Diğer submit fonksiyonları gibi class'ın methodları arasında
 
    this.mutabakForm = this.fb.group({
      UrunKodu: ['', Validators.required],
      YariMamülKodu: ['', Validators.required],
      PlanlananMiktar: ['', [Validators.required, Validators.pattern('^[0-9]+$')]],
      KoliAdeti: ['', [Validators.required, Validators.pattern('^[0-9]+$')]],
      Fasonis: ['', Validators.required],
      Fasomsirket: ['', Validators.required],
      CreateUser: ['', Validators.required]
    });
    this.lotSilForm = this.fb.group({
      LotNumarasi: ['', Validators.required]
    });
    this.lotTarihiDegistirForm = this.fb.group({
      LotNumarasi: ['', Validators.required],
      YeniYuklemeTarihi: ['', Validators.required],
      CreateUser: ['', Validators.required],
      DeleteEmptyHeader: [false]
    });
    this.surecDegistirForm = this.fb.group({
      LotNumarasi: ['', Validators.required],
      isim: ['', Validators.required],
      Surec: ['', Validators.required],
      YuklemeTarihi: ['', Validators.required]
    });
    this.lotMiktariGuncelleForm = this.fb.group({
      LotNumarasi: ['', Validators.required],
      YeniUretilenMiktar: [null, [Validators.required, Validators.pattern('^[0-9]+$')]],
      UpdateUser: ['', Validators.required]
    });
    this.fetchFasonIsler();
    this.fetchFasonFirmalar();
  }

  submit() {
    if (this.mutabakForm.invalid) return;
    this.loading = true;
    this.error = '';
    this.result = null;
    this.http.post<any>('https://fasonback.abkcore.com/api/Fason/CreateUretimEmriVeLotMutabak', this.mutabakForm.value)
      .subscribe({
        next: (res) => {
          this.result = res;
          this.loading = false;
        },
        error: (err) => {
          this.error = err?.error?.Message || 'Bir hata oluştu';
          this.loading = false;
        }
      });
  }

  submitLotSil() {
    if (this.lotSilForm.invalid) return;
    this.lotSilLoading = true;
    this.lotSilError = '';
    this.lotSilResult = null;
    const lotNumarasi = this.lotSilForm.value.LotNumarasi;
    this.deleteLotService.deleteLotByLotNumarasiManuel(lotNumarasi).subscribe({
      next: (res) => {
        this.lotSilResult = res;
        this.lotSilLoading = false;
      },
      error: (err) => {
        this.lotSilError = err?.error?.Message || 'Bir hata oluştu';
        this.lotSilLoading = false;
      }
    });
  }

  submitLotTarihiDegistir() {
    if (this.lotTarihiDegistirForm.invalid) return;
    this.lotTarihiDegistirLoading = true;
    this.lotTarihiDegistirError = '';
    this.lotTarihiDegistirResult = null;
    const req: ReassignLotToNewFasonYuklemeRequest = this.lotTarihiDegistirForm.value;
    this.reassignLotToNewFasonYuklemeService.reassignLotToNewFasonYukleme(req).subscribe({
      next: (res) => {
        this.lotTarihiDegistirResult = res;
        this.lotTarihiDegistirLoading = false;
      },
      error: (err) => {
        this.lotTarihiDegistirError = err?.error?.Message || 'Bir hata oluştu';
        this.lotTarihiDegistirLoading = false;
      }
    });
  }

  submitSurecDegistir() {
    if (this.surecDegistirForm.invalid) return;
    this.surecDegistirLoading = true;
    this.surecDegistirError = '';
    this.surecDegistirResult = null;
    const req: SurecDegistirLotRequest = this.surecDegistirForm.value;
    this.surecDegistirLotService.surecDegistirLot(req).subscribe({
      next: (res) => {
        this.surecDegistirResult = res;
        this.surecDegistirLoading = false;
      },
      error: (err) => {
        this.surecDegistirError = err?.error?.Message || 'Bir hata oluştu';
        this.surecDegistirLoading = false;
      }
    });
  }

  submitLotMiktariGuncelle() {
    if (this.lotMiktariGuncelleForm.invalid) return;
    this.lotMiktariGuncelleLoading = true;
    this.lotMiktariGuncelleError = '';
    this.lotMiktariGuncelleResult = null;
    const req: UpdateUretilenMiktarByLotRequest = this.lotMiktariGuncelleForm.value;
    this.updateUretilenMiktarByLotService.updateUretilenMiktarByLotManuel(req).subscribe({
      next: (res) => {
        this.lotMiktariGuncelleResult = res;
        this.lotMiktariGuncelleLoading = false;
      },
      error: (err) => {
        this.lotMiktariGuncelleError = err?.error?.Message || 'Bir hata oluştu';
        this.lotMiktariGuncelleLoading = false;
      }
    });
  }

  fetchFasonFirmalar() {
    this.fasonFirmalarLoading = true;
    this.fasonFirmalarError = '';
    const user = this.auth.currentUser;
    const firmaAdi = user?.CompanyName || '';
    if (firmaAdi === 'BalmyUretim' && this.auth.token) {
      this.fasonFirmaService.getFasonFirmalar(this.auth.token).subscribe({
        next: (list) => {
          this.fasonFirmalar = list.map((x: any) => x.FasonFirmaadi);
          this.fasonFirmalarLoading = false;
        },
        error: (err) => {
          this.fasonFirmalarError = err?.error?.Message || 'Fason şirketler alınamadı';
          this.fasonFirmalarLoading = false;
        }
      });
    } else {
      this.fasonFirmalar = [firmaAdi];
      this.fasonFirmalarLoading = false;
    }
  }

  fetchFasonIsler() {
    this.fasonIslerLoading = true;
    this.fasonIslerError = '';
    this.fasonIsService.getFasonIsler().subscribe({
      next: (data) => {
        this.fasonIsler = data;
        this.fasonIslerLoading = false;
      },
      error: (err) => {
        this.fasonIslerError = err?.error?.Message || 'Fason İşler alınamadı';
        this.fasonIslerLoading = false;
      }
    });
  }
}
