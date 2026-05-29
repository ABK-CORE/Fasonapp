import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class DeleteLotService {
  constructor(private http: HttpClient) {}

  deleteLotByLotNumarasiManuel(lotNumarasi: string): Observable<any> {
    const request = { LotNumarasi: lotNumarasi };
    return this.http.post<any>('https://fasonback.abkcore.com/api/Fason/DeleteLotByLotNumarasiManuel', request, {
      headers: { 'Content-Type': 'application/json' }
    });
  }
}
