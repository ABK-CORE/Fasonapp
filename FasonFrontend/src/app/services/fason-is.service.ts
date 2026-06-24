import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class FasonIsService {
  constructor(private http: HttpClient) {}

  getFasonIsler(): Observable<any[]> {
    return this.http.get<any[]>('https://fasonback.norax.ai/api/Fason/GetFasonIsler');
  }
}
