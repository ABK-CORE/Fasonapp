import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface ReassignLotToNewFasonYuklemeRequest {
  LotNumarasi: string;
  YeniYuklemeTarihi: string;
  CreateUser: string;
  DeleteEmptyHeader: boolean;
}

@Injectable({ providedIn: 'root' })
export class ReassignLotToNewFasonYuklemeService {
  constructor(private http: HttpClient) {}

  reassignLotToNewFasonYukleme(request: ReassignLotToNewFasonYuklemeRequest): Observable<any> {
    return this.http.post<any>('https://fasonback.norax.ai/api/Fason/ReassignLotToNewFasonYukleme', request);
  }
}
