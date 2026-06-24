import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface SurecDegistirLotRequest {
  LotNumarasi: string;
  isim: string;
  Surec: string;
  YuklemeTarihi: string;
}

@Injectable({ providedIn: 'root' })
export class SurecDegistirLotService {
  constructor(private http: HttpClient) {}

  surecDegistirLot(request: SurecDegistirLotRequest): Observable<any> {
    return this.http.post<any>('https://fasonback.norax.ai/api/Fason/SurecDegistirLot', request);
  }
}
