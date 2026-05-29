import { Component } from '@angular/core';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-template-sidebar',
  templateUrl: './template-sidebar.component.html'
})
export class TemplateSidebarComponent {
  constructor(public auth: AuthService){}
}
