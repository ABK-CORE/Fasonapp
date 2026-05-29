import { Injectable } from '@angular/core';
import { ApiService } from './api.service';
import { FasonDashboardYilMiktarRow } from '../models/fason-dashboard-yil-miktar.models';

@Injectable({ providedIn: 'root' })
export class FasonDashboardYilMiktarService {
  private endpoint = 'Fason/GetFasonDashboardYTDateMiktarli';
  constructor(private api: ApiService) {}

  getDashboardYilMiktar(fasonIsim?: string) {
    const userFason = (fasonIsim || '').trim();
    if (!userFason) {
      return this.api.get<FasonDashboardYilMiktarRow[]>(this.endpoint);
    }

    return this.api.get<FasonDashboardYilMiktarRow[]>(this.endpoint, { fasonIsim: userFason });
  }
}
