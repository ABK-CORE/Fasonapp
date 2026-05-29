import { Injectable } from '@angular/core';
import { ApiService } from './api.service';
import { FasonOzetRow } from '../models/fason-ozet.models';
import { map } from 'rxjs/operators';
import { AuthService } from './auth.service';

@Injectable({ providedIn: 'root' })
export class FasonOzetService {
  private endpoint = 'Fason/GetFasonSurecOzet'; // Backend endpoint: /api/Fason/GetFasonSurecOzet
  constructor(private api: ApiService, private auth: AuthService) {}

  getSummary(fasonIsim?: string) {
    const user = this.auth.currentUser;
    const companyName = (user?.CompanyName || '').trim();
    const targetFason = (fasonIsim || '').trim() || companyName;

    if (!targetFason) {
      throw new Error('Lutfen bir fason firma secin.');
    }

    return this.api.get<FasonOzetRow[]>(this.endpoint, { fasonIsim: targetFason });
  }
}
