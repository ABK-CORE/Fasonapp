import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { FasonUrun } from '../models/fason-urun.models';
import { environment } from 'src/environments/environment';

@Injectable({ providedIn: 'root' })
export class FasonUrunListeService {
  private endpoint = environment.apiBaseUrl + '/FasonUrunListe/GetFasonUrunListe';

  constructor(private http: HttpClient) {}

  getUrunler(): Observable<FasonUrun[]> {
    return this.http.get<FasonUrun[]>(this.endpoint);
  }
}
