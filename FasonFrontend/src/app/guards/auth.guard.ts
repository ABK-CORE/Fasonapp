import { Injectable } from '@angular/core';
import { CanActivate, CanActivateChild, ActivatedRouteSnapshot, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Injectable({ providedIn: 'root' })
export class AuthGuard implements CanActivate, CanActivateChild {
  constructor(private auth: AuthService, private router: Router) {}

  canActivate(route: ActivatedRouteSnapshot): boolean {
    if (!this.auth.isAuthenticated()) {
      this.router.navigate(['/login']);
      return false;
    }
    const roles: string[] = route.data?.['roles'] || [];
    if (roles.length && !roles.some(r => this.auth.hasRole(r))) {
      this.router.navigate(['/dashboard']);
      return false;
    }
    return true;
  }

  canActivateChild(): boolean {
    return this.canActivate({} as ActivatedRouteSnapshot);
  }
}
