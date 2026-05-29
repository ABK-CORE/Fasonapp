import { Injectable } from '@angular/core';
import { ApiService } from './api.service';
import { FasonRaporTarihRow } from '../models/fason-rapor-tarih.models';
import { AuthService } from './auth.service';

@Injectable({ providedIn: 'root' })
export class FasonRaporTarihService {
  private endpoint = 'Fason/GetFasonTakipFasonAppOzet';
  constructor(private api: ApiService, private auth: AuthService) {}

  getRaporTarihBazli(baslamatarihi: string, bitistarihi: string, fasonisim: string, fasonsurec: string) {
    return this.api.get<FasonRaporTarihRow[]>(this.endpoint, {
      baslamatarihi,
      bitistarihi,
      fasonisim,
      fasonsurec
    });
  }
}
