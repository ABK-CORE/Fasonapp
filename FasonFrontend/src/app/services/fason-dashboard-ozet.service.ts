import { Injectable } from '@angular/core';
import { ApiService } from './api.service';
import { FasonDashboardOzetRow } from '../models/fason-dashboard-ozet.models';

@Injectable({ providedIn: 'root' })
export class FasonDashboardOzetService {
  private endpoint = 'Fason/GetFasonDashboardOzet';
  constructor(private api: ApiService) {}

  getDashboardOzet(fasonIsim?: string) {
    const targetFason = (fasonIsim || '').trim();
    if (!targetFason) {
      return this.api.get<FasonDashboardOzetRow[]>(this.endpoint);
    }

    return this.api.get<FasonDashboardOzetRow[]>(this.endpoint, { fasonIsim: targetFason });
  }
}
