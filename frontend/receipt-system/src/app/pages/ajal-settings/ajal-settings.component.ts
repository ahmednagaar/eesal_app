import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ApiService } from '../../core/api.service';

@Component({
  selector: 'app-ajal-settings',
  standalone: true,
  imports: [CommonModule, FormsModule, MatCardModule, MatFormFieldModule, MatInputModule,
    MatButtonModule, MatIconModule, MatDividerModule, MatSnackBarModule],
  template: `
    <div class="page-container">
      <div class="page-header"><h1>⚙️ إعدادات رقم الفاتورة</h1></div>

      <mat-card class="settings-card" *ngIf="settings">
        <div class="current-info">
          <div class="info-row"><span>البادئة الحالية:</span><strong class="prefix-val">{{ settings.prefix }}</strong></div>
          <div class="info-row"><span>نطاق الأرقام:</span><strong>{{ settings.prefix }}000 ← {{ settings.prefix }}999</strong></div>
          <div class="info-row"><span>البادئة التالية:</span><strong>{{ settings.nextPrefix }}</strong></div>
          <div class="info-row"><span>حد التحذير:</span><strong>{{ settings.prefix }}{{ settings.warningThreshold }}</strong></div>
        </div>

        <mat-divider style="margin:20px 0"></mat-divider>

        <h3>تغيير البادئة</h3>
        <div class="warning-hint">
          <mat-icon color="warn" style="vertical-align:middle">warning</mat-icon>
          غيّر البادئة فقط عندما تصل فواتير اليوم للرقم {{ settings.prefix }}999
          وتبدأ بالانتقال لـ {{ settings.nextPrefix }}000
        </div>

        <mat-form-field appearance="outline" class="prefix-field">
          <mat-label>البادئة الجديدة (3 أرقام)</mat-label>
          <input matInput type="text" [(ngModel)]="newPrefix" maxlength="3" [placeholder]="settings.nextPrefix">
          <mat-hint>مثال: {{ settings.nextPrefix }}</mat-hint>
        </mat-form-field>

        <button mat-raised-button color="warn" [disabled]="!newPrefix || newPrefix.length < 3" (click)="changePrefix()" style="border-radius:10px">
          تغيير البادئة إلى {{ newPrefix || '...' }}
        </button>
      </mat-card>
    </div>
  `,
  styles: [`
    .settings-card { border-radius:16px !important; padding:24px !important; max-width:600px; }
    .current-info { background:#f0f4ff; border-radius:12px; padding:16px; }
    .info-row { display:flex; justify-content:space-between; padding:6px 0; }
    .prefix-val { background:#667eea; color:#fff; padding:4px 16px; border-radius:10px; font-size:1.3rem; }
    .warning-hint { background:#fef2f2; border-radius:10px; padding:12px; margin-bottom:16px; color:#dc2626; font-size:0.9rem; }
    .prefix-field { margin-top:12px; margin-bottom:12px; }
  `]
})
export class AjalSettingsComponent implements OnInit {
  settings: any = null;
  newPrefix = '';

  constructor(private api: ApiService, private snack: MatSnackBar) {}

  ngOnInit() {
    this.api.getAjalPrefixSettings().subscribe(s => {
      this.settings = s;
      this.newPrefix = s.nextPrefix;
    });
  }

  changePrefix() {
    if (!this.newPrefix || this.newPrefix.length < 3) return;
    this.api.updateAjalPrefix(this.newPrefix).subscribe({
      next: () => {
        this.snack.open(`✅ تم تغيير البادئة إلى ${this.newPrefix}`, 'حسناً', { duration: 3000 });
        this.settings.prefix = this.newPrefix;
        this.settings.nextPrefix = (parseInt(this.newPrefix) + 1).toString();
        this.newPrefix = this.settings.nextPrefix;
      },
      error: () => this.snack.open('خطأ في تغيير البادئة', 'إغلاق', { duration: 3000 })
    });
  }
}
