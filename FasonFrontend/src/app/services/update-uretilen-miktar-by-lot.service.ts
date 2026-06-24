import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface UpdateUretilenMiktarByLotRequest {
  LotNumarasi: string;
  YeniUretilenMiktar: number;
  UpdateUser: string;
}

@Injectable({ providedIn: 'root' })
export class UpdateUretilenMiktarByLotService {
  constructor(private http: HttpClient) {}

  updateUretilenMiktarByLotManuel(request: UpdateUretilenMiktarByLotRequest): Observable<any> {
    return this.http.post<any>('https://fasonback.norax.ai/api/Fason/UpdateUretilenMiktarByLotManuel', request);
  }
}
