import { Injectable } from '@angular/core';
import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const token = localStorage.getItem('token');
    const expiresAtRaw = localStorage.getItem('tokenExpiresAt');
    const expiresAt = Number(expiresAtRaw);

    const isExpired = !Number.isFinite(expiresAt) || Date.now() >= expiresAt;
    if (token && isExpired) {
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      localStorage.removeItem('tokenExpiresAt');
    }

    if (token) {
      if (!isExpired) {
        req = req.clone({ setHeaders: { Authorization: `Bearer ${token}` }});
      }
    }
    return next.handle(req);
  }
}
