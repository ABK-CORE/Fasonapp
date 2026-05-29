import { Component } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { LoginCredentials } from '../../models/auth.models';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {
  busy = false;
  form = this.fb.group({
    username: ['', Validators.required],
    password: ['', Validators.required]
  });

  constructor(private fb: FormBuilder, private auth: AuthService, private router: Router){}

  submit() {
    if (this.form.invalid) return;
    this.busy = true;
    const payload = new LoginCredentials(
      this.form.value.username!,
      this.form.value.password!
    );
    this.auth.login(payload).subscribe({
      next: _ => { this.busy = false; this.router.navigate(['/dashboard']); },
      error: err => { this.busy = false; alert(err.message || 'Giriş hatası'); }
    });
  }
}
