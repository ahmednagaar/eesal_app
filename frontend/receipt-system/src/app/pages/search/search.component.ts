import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule } from '@angular/material/table';
import { MatChipsModule } from '@angular/material/chips';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RouterModule } from '@angular/router';
import { ApiService } from '../../core/api.service';

@Component({
  selector: 'app-search',
  standalone: true,
  imports: [
    CommonModule, FormsModule, MatCardModule, MatFormFieldModule, MatInputModule,
    MatButtonModule, MatIconModule, MatSelectModule, MatTableModule, MatChipsModule,
    MatSnackBarModule, MatProgressSpinnerModule, MatDatepickerModule, MatNativeDateModule,
    MatPaginatorModule, MatExpansionModule, MatTooltipModule, RouterModule
  ],
  template: `
    <div class="page-container">
      <div class="page-header"><h1>🔍 البحث المتقدم</h1></div>

      <!-- Filters -->
      <mat-expansion-panel [expanded]="true" class="filter-panel">
        <mat-expansion-panel-header>
          <mat-panel-title>خيارات البحث</mat-panel-title>
        </mat-expansion-panel-header>
        <div class="filters-grid">
          <mat-form-field appearance="outline">
            <mat-label>اسم التاجر</mat-label>
            <input matInput [(ngModel)]="filters.merchantName" placeholder="اكتب جزء من الاسم...">
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>السائق</mat-label>
            <mat-select [(ngModel)]="filters.driverId">
              <mat-option [value]="null">الكل</mat-option>
              <mat-option *ngFor="let d of drivers" [value]="d.driverId">{{ d.fullName }}</mat-option>
            </mat-select>
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>الخط / المنطقة</mat-label>
            <input matInput [(ngModel)]="filters.routeArea">
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>من تاريخ</mat-label>
            <input matInput [matDatepicker]="dp1" [(ngModel)]="filters.dateFrom">
            <mat-datepicker-toggle matSuffix [for]="dp1"></mat-datepicker-toggle>
            <mat-datepicker #dp1></mat-datepicker>
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>إلى تاريخ</mat-label>
            <input matInput [matDatepicker]="dp2" [(ngModel)]="filters.dateTo">
            <mat-datepicker-toggle matSuffix [for]="dp2"></mat-datepicker-toggle>
            <mat-datepicker #dp2></mat-datepicker>
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>رقم الإيصال</mat-label>
            <input matInput type="number" [(ngModel)]="filters.receiptNumber">
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>من مبلغ</mat-label>
            <input matInput type="number" [(ngModel)]="filters.amountMin">
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>إلى مبلغ</mat-label>
            <input matInput type="number" [(ngModel)]="filters.amountMax">
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>مصدر البيانات</mat-label>
            <mat-select [(ngModel)]="filters.importSource">
              <mat-option [value]="null">الكل</mat-option>
              <mat-option value="Manual">إدخال يدوي</mat-option>
              <mat-option value="Excel">ملف Excel</mat-option>
            </mat-select>
          </mat-form-field>
        </div>
        <div class="filter-actions">
          <button mat-stroked-button (click)="clearFilters()" style="border-radius:10px !important">مسح الفلاتر</button>
          <button mat-raised-button color="primary" (click)="search()" style="border-radius:10px !important">🔍 بحث</button>
        </div>
      </mat-expansion-panel>

      <!-- Loading -->
      <div class="loading-container" *ngIf="searching">
        <mat-spinner diameter="40"></mat-spinner>
      </div>

      <!-- Results summary -->
      <div class="results-summary" *ngIf="results && !searching">
        <span>نتائج البحث: <strong>{{ results.totalCount }}</strong> إيصال</span>
        <span class="total-badge">إجمالي المبالغ: {{ results.totals.totalAmount | number:'1.2-2' }} جنيه</span>
        <button mat-stroked-button (click)="exportExcel()" style="border-radius:10px !important">
          <mat-icon>download</mat-icon> تصدير Excel
        </button>
      </div>

      <!-- Desktop table -->
      <div class="desktop-table" *ngIf="results && !searching">
        <table mat-table [dataSource]="results.results">
          <ng-container matColumnDef="receiptNumber">
            <th mat-header-cell *matHeaderCellDef>رقم الإيصال</th>
            <td mat-cell *matCellDef="let r">{{ r.receiptNumber }}</td>
          </ng-container>
          <ng-container matColumnDef="merchantName">
            <th mat-header-cell *matHeaderCellDef>التاجر</th>
            <td mat-cell *matCellDef="let r">{{ r.merchantName }}</td>
          </ng-container>
          <ng-container matColumnDef="driverName">
            <th mat-header-cell *matHeaderCellDef>السائق</th>
            <td mat-cell *matCellDef="let r">{{ r.driverName }}</td>
          </ng-container>
          <ng-container matColumnDef="routeArea">
            <th mat-header-cell *matHeaderCellDef>الخط</th>
            <td mat-cell *matCellDef="let r">{{ r.routeArea }}</td>
          </ng-container>
          <ng-container matColumnDef="collectionDate">
            <th mat-header-cell *matHeaderCellDef>التاريخ</th>
            <td mat-cell *matCellDef="let r">{{ r.collectionDate | date:'dd/MM/yyyy' }}</td>
          </ng-container>
          <ng-container matColumnDef="amount">
            <th mat-header-cell *matHeaderCellDef>المبلغ</th>
            <td mat-cell *matCellDef="let r">{{ r.amount | number:'1.2-2' }}</td>
          </ng-container>
          <ng-container matColumnDef="source">
            <th mat-header-cell *matHeaderCellDef>المصدر</th>
            <td mat-cell *matCellDef="let r">
              <span class="source-badge" [class.excel]="r.importSource==='Excel'">
                {{ r.importSource === 'Excel' ? 'Excel' : 'يدوي' }}
              </span>
              <span class="without-badge" *ngIf="r.isWithoutReceipt">⚠️</span>
            </td>
          </ng-container>
          <tr mat-header-row *matHeaderRowDef="searchCols"></tr>
          <tr mat-row *matRowDef="let r; columns: searchCols;" [class.warning-row]="r.isWithoutReceipt"></tr>
        </table>
        <mat-paginator [length]="results.totalCount" [pageSize]="50" (page)="onPage($event)"></mat-paginator>
      </div>

      <!-- Mobile cards -->
      <div class="mobile-receipt-cards" *ngIf="results && !searching">
        <mat-card *ngFor="let r of results.results" class="mobile-card" [class.warning-row]="r.isWithoutReceipt">
          <div class="mobile-row"><strong>{{ r.merchantName }}</strong>
            <span class="amount-val">{{ r.amount | number:'1.2-2' }} جنيه</span></div>
          <div class="mobile-row">📅 {{ r.collectionDate | date:'dd/MM/yyyy' }} — 🚛 {{ r.driverName }}</div>
          <div class="mobile-row">🧾 إيصال #{{ r.receiptNumber }} — {{ r.routeArea }}</div>
          <div class="mobile-row">
            <span class="source-badge" [class.excel]="r.importSource==='Excel'">
              {{ r.importSource === 'Excel' ? 'Excel' : 'يدوي' }}
            </span>
            <span class="without-badge" *ngIf="r.isWithoutReceipt">⚠️ بدون إيصال</span>
          </div>
        </mat-card>
      </div>
    </div>
  `,
  styles: [`
    .filter-panel { border-radius:16px !important; margin-bottom:16px; }
    .filters-grid { display:grid; grid-template-columns:repeat(3,1fr); gap:8px; }
    @media(max-width:768px) { .filters-grid { grid-template-columns:1fr; } }
    .filter-actions { display:flex; gap:12px; justify-content:flex-end; margin-top:8px; }
    .results-summary { display:flex; align-items:center; gap:16px; flex-wrap:wrap; margin-bottom:16px; padding:12px 16px; background:#f8f9fa; border-radius:12px; }
    .total-badge { background:#dcfce7; color:#166534; padding:4px 12px; border-radius:10px; font-weight:700; }
    .source-badge { font-size:0.75rem; padding:2px 8px; border-radius:8px; background:#f3f4f6; color:#6b7280; }
    .source-badge.excel { background:#e0e7ff; color:#4338ca; }
    .without-badge { color:#dc2626; font-size:0.85rem; margin-right:4px; }
    .warning-row { background:#fef2f2 !important; }
    .desktop-table { display:block; } .mobile-receipt-cards { display:none; }
    @media(max-width:768px) { .desktop-table { display:none; } .mobile-receipt-cards { display:block; } }
    .mobile-card { margin-bottom:8px; padding:12px !important; border-radius:10px !important; }
    .mobile-row { margin-bottom:4px; display:flex; justify-content:space-between; align-items:center; flex-wrap:wrap; }
    .amount-val { font-weight:700; color:#22c55e; }
    .loading-container { text-align:center; padding:32px; }
  `]
})
export class SearchComponent {
  drivers: any[] = [];
  results: any = null;
  searching = false;
  searchCols = ['receiptNumber', 'merchantName', 'driverName', 'routeArea', 'collectionDate', 'amount', 'source'];

  filters: any = {
    merchantName: null, driverId: null, routeArea: null,
    dateFrom: null, dateTo: null, receiptNumber: null,
    amountMin: null, amountMax: null, importSource: null, page: 1, pageSize: 50
  };

  constructor(private api: ApiService, private snack: MatSnackBar) {}
  ngOnInit() { this.api.getDrivers().subscribe(d => this.drivers = d); }

  search() {
    this.searching = true;
    this.filters.page = 1;
    (this.api as any).searchReceipts(this.filters).subscribe({
      next: (res: any) => { this.results = res; this.searching = false; },
      error: () => { this.searching = false; this.snack.open('حدث خطأ في البحث', 'إغلاق', { duration: 3000 }); }
    });
  }

  onPage(e: PageEvent) {
    this.filters.page = e.pageIndex + 1;
    this.filters.pageSize = e.pageSize;
    this.search();
  }

  clearFilters() {
    this.filters = { merchantName: null, driverId: null, routeArea: null, dateFrom: null, dateTo: null, receiptNumber: null, amountMin: null, amountMax: null, importSource: null, page: 1, pageSize: 50 };
    this.results = null;
  }

  exportExcel() {
    (this.api as any).exportSearchResults(this.filters).subscribe({
      next: (blob: Blob) => {
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `search_results_${new Date().toISOString().slice(0,10)}.xlsx`;
        a.click();
        URL.revokeObjectURL(url);
      },
      error: () => this.snack.open('خطأ في تصدير الملف', 'إغلاق', { duration: 3000 })
    });
  }
}
