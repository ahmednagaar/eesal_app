import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ApiService } from '../../core/api.service';

@Component({
  selector: 'app-drivers',
  standalone: true,
  imports: [
    CommonModule, FormsModule, MatCardModule, MatTableModule, MatButtonModule,
    MatIconModule, MatFormFieldModule, MatInputModule, MatDialogModule,
    MatSnackBarModule, MatProgressSpinnerModule
  ],
  template: `
    <div class="page-container">
      <div class="page-header">
        <h1>السائقون</h1>
        <button mat-raised-button color="primary" (click)="openForm()" style="border-radius:12px !important">
          <mat-icon>add</mat-icon> إضافة سائق
        </button>
      </div>

      <div class="loading-container" *ngIf="loading"><mat-spinner diameter="40"></mat-spinner></div>

      <mat-card class="data-table-card" *ngIf="!loading">
        <!-- Desktop -->
        <div class="desktop-table">
          <table mat-table [dataSource]="drivers" class="full-width">
            <ng-container matColumnDef="name"><th mat-header-cell *matHeaderCellDef>الاسم</th><td mat-cell *matCellDef="let d">{{ d.fullName }}</td></ng-container>
            <ng-container matColumnDef="phone"><th mat-header-cell *matHeaderCellDef>الهاتف</th><td mat-cell *matCellDef="let d">{{ d.phoneNumber }}</td></ng-container>
            <ng-container matColumnDef="gaps">
              <th mat-header-cell *matHeaderCellDef>فجوات مفتوحة</th>
              <td mat-cell *matCellDef="let d">
                <span class="status-badge" [class.open]="d.openGapsCount>0" [class.resolved]="d.openGapsCount===0">
                  {{ d.openGapsCount }}
                </span>
              </td>
            </ng-container>
            <ng-container matColumnDef="actions">
              <th mat-header-cell *matHeaderCellDef>إجراءات</th>
              <td mat-cell *matCellDef="let d">
                <button mat-icon-button color="primary" (click)="openForm(d)"><mat-icon>edit</mat-icon></button>
                <button mat-icon-button color="warn" (click)="deleteDriver(d)"><mat-icon>delete</mat-icon></button>
              </td>
            </ng-container>
            <tr mat-header-row *matHeaderRowDef="['name','phone','gaps','actions']"></tr>
            <tr mat-row *matRowDef="let row; columns: ['name','phone','gaps','actions']"></tr>
          </table>
        </div>

        <!-- Mobile -->
        <div class="mobile-receipt-cards">
          <mat-card *ngFor="let d of drivers" style="margin:8px 0; padding:12px !important; border-radius:12px !important">
            <div style="display:flex; justify-content:space-between; align-items:center">
              <div>
                <strong>{{ d.fullName }}</strong>
                <div style="font-size:0.8rem; color:#6b7280">{{ d.phoneNumber }}</div>
              </div>
              <div>
                <button mat-icon-button color="primary" (click)="openForm(d)"><mat-icon>edit</mat-icon></button>
                <button mat-icon-button color="warn" (click)="deleteDriver(d)"><mat-icon>delete</mat-icon></button>
              </div>
            </div>
          </mat-card>
        </div>
      </mat-card>

      <!-- Form Dialog -->
      <div class="resolve-overlay" *ngIf="showForm" (click)="showForm=false">
        <mat-card class="resolve-dialog" (click)="$event.stopPropagation()">
          <h3>{{ editing ? 'تعديل سائق' : 'إضافة سائق جديد' }}</h3>
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>الاسم الكامل</mat-label>
            <input matInput [(ngModel)]="form.fullName">
          </mat-form-field>
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>رقم الهاتف</mat-label>
            <input matInput [(ngModel)]="form.phoneNumber">
          </mat-form-field>
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>ملاحظات</mat-label>
            <textarea matInput rows="2" [(ngModel)]="form.notes"></textarea>
          </mat-form-field>
          <div style="display:flex; gap:8px">
            <button mat-raised-button color="primary" (click)="saveDriver()" style="border-radius:8px !important">حفظ</button>
            <button mat-button (click)="showForm=false">إلغاء</button>
          </div>
        </mat-card>
      </div>
    </div>
  `,
  styles: [`
    .resolve-overlay { position:fixed; inset:0; background:rgba(0,0,0,0.5); z-index:1000; display:flex; align-items:center; justify-content:center; padding:16px; }
    .resolve-dialog { width:100%; max-width:450px; padding:24px !important; border-radius:16px !important; }
    .resolve-dialog h3 { margin:0 0 16px; font-weight:700; }
  `]
})
export class DriversComponent implements OnInit {
  drivers: any[] = [];
  loading = true;
  showForm = false;
  editing: any = null;
  form = { fullName: '', phoneNumber: '', notes: '' };

  constructor(private api: ApiService, private snack: MatSnackBar) {}

  ngOnInit() { this.load(); }

  load() {
    this.loading = true;
    this.api.getDrivers().subscribe({ next: (d) => { this.drivers = d; this.loading = false; }, error: () => this.loading = false });
  }

  openForm(driver?: any) {
    this.editing = driver || null;
    this.form = driver ? { fullName: driver.fullName, phoneNumber: driver.phoneNumber, notes: driver.notes || '' }
                       : { fullName: '', phoneNumber: '', notes: '' };
    this.showForm = true;
  }

  saveDriver() {
    if (!this.form.fullName || !this.form.phoneNumber) { this.snack.open('أكمل البيانات المطلوبة', 'إغلاق', { duration: 3000 }); return; }
    const obs = this.editing ? this.api.updateDriver(this.editing.driverId, this.form) : this.api.createDriver(this.form);
    obs.subscribe({
      next: () => { this.snack.open('تم الحفظ بنجاح ✅', 'إغلاق', { duration: 3000 }); this.showForm = false; this.load(); },
      error: () => this.snack.open('حدث خطأ', 'إغلاق', { duration: 3000 })
    });
  }

  deleteDriver(d: any) {
    if (!confirm(`هل تريد حذف السائق ${d.fullName}؟`)) return;
    this.api.deleteDriver(d.driverId).subscribe({
      next: () => { this.snack.open('تم الحذف ✅', 'إغلاق', { duration: 3000 }); this.load(); },
      error: () => this.snack.open('حدث خطأ', 'إغلاق', { duration: 3000 })
    });
  }
}
