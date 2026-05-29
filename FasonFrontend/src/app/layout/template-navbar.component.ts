import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { environment } from 'src/environments/environment';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-template-navbar',
  templateUrl: './template-navbar.component.html'
})
export class TemplateNavbarComponent {
  environment = environment;
  constructor(public auth: AuthService, private router: Router) {}
  logout(){ this.auth.logout(); this.router.navigate(['/login']); }
}
