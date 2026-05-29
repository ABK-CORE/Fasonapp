import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { ApiResponse } from '../models/api-response';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private base = environment.apiBaseUrl;
  constructor(private http: HttpClient) {}
  get<T>(url: string, params?: Record<string, any>) {
    const hp = new HttpParams({ fromObject: params ?? {} });
    return this.http.get<T>(`${this.base}/${url}`, { params: hp });
  }
  post<T>(url: string, body: any) {
    return this.http.post<ApiResponse<T>>(`${this.base}/${url}`, body);
  }
  put<T>(url: string, body: any) {
    return this.http.put<ApiResponse<T>>(`${this.base}/${url}`, body);
  }
  delete<T>(url: string) {
    return this.http.delete<ApiResponse<T>>(`${this.base}/${url}`);
  }
}
