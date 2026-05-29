import { environment } from 'src/environments/environment';
import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class FasonFirmaService {
  constructor(private http: HttpClient) {}

  getFasonFirmalar(token: string): Observable<any[]> {
  const url = environment.apiBaseUrl + '/Fason/GetFasonFirmaadi';
    const headers = new HttpHeaders({
      'Content-Type': 'application/json',
      'Accept': 'application/json',
      'Authorization': `Bearer ${token}`
    });
    return this.http.get<any[]>(url, { headers });
  }
}
