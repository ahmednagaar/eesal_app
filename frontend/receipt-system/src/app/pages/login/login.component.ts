import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '../../core/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, MatCardModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatIconModule, MatProgressSpinnerModule],
  template: `
    <div class="login-wrapper">
      <div class="login-bg"></div>
      <mat-card class="login-card">
        <div class="login-header">
          <mat-icon class="login-logo">receipt_long</mat-icon>
          <h1>نظام إدارة الإيصالات</h1>
          <p>شركه البسطاوي للتجاره والتوزيع</p>
        </div>

        <mat-card-content>
          <div *ngIf="error" class="error-msg">
            <mat-icon>error</mat-icon> {{ error }}
          </div>

          <mat-form-field appearance="outline" class="full-width">
            <mat-label>اسم المستخدم</mat-label>
            <input matInput [(ngModel)]="username" (keyup.enter)="login()" id="username-input">
            <mat-icon matPrefix>person</mat-icon>
          </mat-form-field>

          <mat-form-field appearance="outline" class="full-width">
            <mat-label>كلمة المرور</mat-label>
            <input matInput [type]="hidePassword ? 'password' : 'text'"
                   [(ngModel)]="password" (keyup.enter)="login()" id="password-input">
            <mat-icon matPrefix>lock</mat-icon>
            <button mat-icon-button matSuffix (click)="hidePassword = !hidePassword">
              <mat-icon>{{ hidePassword ? 'visibility_off' : 'visibility' }}</mat-icon>
            </button>
          </mat-form-field>

          <button mat-raised-button color="primary" class="login-btn"
                  (click)="login()" [disabled]="loading" id="login-button">
            <mat-spinner *ngIf="loading" diameter="20"></mat-spinner>
            <span *ngIf="!loading">تسجيل الدخول</span>
          </button>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .login-wrapper {
      height: 100vh; display: flex; align-items: center; justify-content: center;
      position: relative; overflow: hidden;
    }
    .login-bg {
      position: absolute; inset: 0;
      background: linear-gradient(135deg, #1a1a2e 0%, #16213e 50%, #0f3460 100%);
    }
    .login-card {
      position: relative; z-index: 1; width: 100%; max-width: 420px; margin: 16px;
      border-radius: 20px !important; padding: 32px !important;
      box-shadow: 0 20px 60px rgba(0,0,0,0.3) !important;
    }
    .login-header {
      text-align: center; margin-bottom: 24px;
      h1 { font-size: 1.4rem; font-weight: 800; color: #1a1a2e; margin: 8px 0 4px; }
      p { color: #6b7280; font-size: 0.85rem; margin: 0; }
    }
    .login-logo { font-size: 56px; width: 56px; height: 56px; color: #667eea; }
    .login-btn {
      width: 100%; height: 48px; font-size: 1rem; font-weight: 700;
      border-radius: 12px !important; margin-top: 8px;
    }
    .error-msg {
      display: flex; align-items: center; gap: 8px; padding: 12px;
      background: #fee2e2; color: #dc2626; border-radius: 8px;
      margin-bottom: 16px; font-size: 0.85rem;
    }
  `]
})
export class LoginComponent {
  username = '';
  password = '';
  hidePassword = true;
  loading = false;
  error = '';

  constructor(private auth: AuthService, private router: Router) {
    if (auth.isLoggedIn) this.router.navigate(['/dashboard']);
  }

  login() {
    if (!this.username || !this.password) { this.error = 'يرجى إدخال اسم المستخدم وكلمة المرور'; return; }
    this.loading = true;
    this.error = '';
    this.auth.login(this.username, this.password).subscribe({
      next: () => { this.router.navigate(['/dashboard']); },
      error: () => { this.error = 'اسم المستخدم أو كلمة المرور غير صحيحة'; this.loading = false; }
    });
  }
}
