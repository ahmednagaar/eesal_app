import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Subject, debounceTime } from 'rxjs';
import { ApiService } from '../../core/api.service';

@Component({
  selector: 'app-merchants',
  standalone: true,
  imports: [
    CommonModule, FormsModule, MatCardModule, MatTableModule, MatButtonModule,
    MatIconModule, MatFormFieldModule, MatInputModule, MatPaginatorModule,
    MatSnackBarModule, MatProgressSpinnerModule
  ],
  template: `
    <div class="page-container">
      <div class="page-header">
        <h1>التجار</h1>
        <button mat-raised-button color="primary" (click)="openForm()" style="border-radius:12px !important">
          <mat-icon>add</mat-icon> إضافة تاجر
        </button>
      </div>

      <!-- Search -->
      <mat-card style="margin-bottom:16px; padding:12px 16px !important; border-radius:16px !important">
        <mat-form-field appearance="outline" class="full-width" style="margin-bottom:-1.25em">
          <mat-label>بحث عن تاجر...</mat-label>
          <input matInput [(ngModel)]="searchQuery" (input)="onSearch()" id="merchant-search">
          <mat-icon matPrefix>search</mat-icon>
        </mat-form-field>
      </mat-card>

      <div class="loading-container" *ngIf="loading"><mat-spinner diameter="40"></mat-spinner></div>

      <mat-card class="data-table-card" *ngIf="!loading">
        <div class="desktop-table">
          <table mat-table [dataSource]="merchants" class="full-width">
            <ng-container matColumnDef="name"><th mat-header-cell *matHeaderCellDef>الاسم</th><td mat-cell *matCellDef="let m">{{ m.merchantName }}</td></ng-container>
            <ng-container matColumnDef="city"><th mat-header-cell *matHeaderCellDef>المدينة</th><td mat-cell *matCellDef="let m">{{ m.city || '—' }}</td></ng-container>
            <ng-container matColumnDef="phone"><th mat-header-cell *matHeaderCellDef>الهاتف</th><td mat-cell *matCellDef="let m">{{ m.phoneNumber || '—' }}</td></ng-container>
            <ng-container matColumnDef="actions">
              <th mat-header-cell *matHeaderCellDef>إجراءات</th>
              <td mat-cell *matCellDef="let m">
                <button mat-icon-button color="primary" (click)="openForm(m)"><mat-icon>edit</mat-icon></button>
              </td>
            </ng-container>
            <tr mat-header-row *matHeaderRowDef="['name','city','phone','actions']"></tr>
            <tr mat-row *matRowDef="let row; columns: ['name','city','phone','actions']"></tr>
          </table>
        </div>

        <div class="mobile-receipt-cards">
          <mat-card *ngFor="let m of merchants" style="margin:8px 0; padding:12px !important; border-radius:12px !important">
            <div style="display:flex; justify-content:space-between; align-items:center">
              <div>
                <strong>{{ m.merchantName }}</strong>
                <div style="font-size:0.8rem; color:#6b7280">{{ m.city || '' }} {{ m.phoneNumber ? '— ' + m.phoneNumber : '' }}</div>
              </div>
              <button mat-icon-button color="primary" (click)="openForm(m)"><mat-icon>edit</mat-icon></button>
            </div>
          </mat-card>
        </div>

        <mat-paginator [length]="totalMerchants" [pageSize]="20" (page)="onPage($event)" [hidePageSize]="true"></mat-paginator>
      </mat-card>

      <!-- Form Dialog -->
      <div class="resolve-overlay" *ngIf="showForm" (click)="showForm=false">
        <mat-card class="resolve-dialog" (click)="$event.stopPropagation()">
          <h3>{{ editing ? 'تعديل تاجر' : 'إضافة تاجر جديد' }}</h3>
          <mat-form-field appearance="outline" class="full-width"><mat-label>اسم التاجر</mat-label><input matInput [(ngModel)]="form.merchantName"></mat-form-field>
          <mat-form-field appearance="outline" class="full-width"><mat-label>المدينة</mat-label><input matInput [(ngModel)]="form.city"></mat-form-field>
          <mat-form-field appearance="outline" class="full-width"><mat-label>الهاتف</mat-label><input matInput [(ngModel)]="form.phoneNumber"></mat-form-field>
          <mat-form-field appearance="outline" class="full-width"><mat-label>ملاحظات</mat-label><textarea matInput rows="2" [(ngModel)]="form.notes"></textarea></mat-form-field>
          <div style="display:flex; gap:8px">
            <button mat-raised-button color="primary" (click)="saveMerchant()" style="border-radius:8px !important">حفظ</button>
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
export class MerchantsComponent implements OnInit {
  merchants: any[] = [];
  loading = true;
  totalMerchants = 0;
  page = 1;
  searchQuery = '';
  showForm = false;
  editing: any = null;
  form = { merchantName: '', city: '', phoneNumber: '', notes: '' };

  constructor(private api: ApiService, private snack: MatSnackBar) {}

  ngOnInit() { this.load(); }

  load() {
    this.loading = true;
    this.api.getMerchants(this.page, 20).subscribe({
      next: (res: any) => { this.merchants = res.data; this.totalMerchants = res.total; this.loading = false; },
      error: () => this.loading = false
    });
  }

  onSearch() {
    if (this.searchQuery.length >= 1) {
      this.api.searchMerchants(this.searchQuery).subscribe(m => { this.merchants = m; this.totalMerchants = m.length; });
    } else { this.load(); }
  }

  onPage(e: PageEvent) { this.page = e.pageIndex + 1; this.load(); }

  openForm(m?: any) {
    this.editing = m || null;
    this.form = m ? { merchantName: m.merchantName, city: m.city || '', phoneNumber: m.phoneNumber || '', notes: m.notes || '' }
                  : { merchantName: '', city: '', phoneNumber: '', notes: '' };
    this.showForm = true;
  }

  saveMerchant() {
    if (!this.form.merchantName) { this.snack.open('اسم التاجر مطلوب', 'إغلاق', { duration: 3000 }); return; }
    const obs = this.editing ? this.api.updateMerchant(this.editing.merchantId, this.form) : this.api.createMerchant(this.form);
    obs.subscribe({
      next: () => { this.snack.open('تم الحفظ ✅', 'إغلاق', { duration: 3000 }); this.showForm = false; this.load(); },
      error: () => this.snack.open('حدث خطأ', 'إغلاق', { duration: 3000 })
    });
  }
}
