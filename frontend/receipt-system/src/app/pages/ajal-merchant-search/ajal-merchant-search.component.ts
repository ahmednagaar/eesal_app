import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatChipsModule } from '@angular/material/chips';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { ApiService } from '../../core/api.service';

@Component({
  selector: 'app-ajal-merchant-search',
  standalone: true,
  imports: [CommonModule, FormsModule, MatCardModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatIconModule,
    MatTableModule, MatChipsModule, MatSnackBarModule, MatProgressSpinnerModule, MatDatepickerModule, MatNativeDateModule, MatAutocompleteModule],
  template: `
    <div class="page-container">
      <div class="page-header"><h1>🔍 بحث بالتاجر — دفتر الآجل</h1></div>

      <mat-card class="search-card">
        <div class="search-fields">
          <mat-form-field appearance="outline" class="wide-field">
            <mat-label>ابحث باسم التاجر</mat-label>
            <input matInput [(ngModel)]="merchantSearch" (input)="filterMerchants()" [matAutocomplete]="autoM" placeholder="أول حروف الاسم...">
            <mat-autocomplete #autoM (optionSelected)="selectMerchant($event)">
              <mat-option *ngFor="let m of suggestions" [value]="m.merchantName">{{ m.merchantName }} — {{ m.city || '' }}</mat-option>
            </mat-autocomplete>
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>من تاريخ</mat-label>
            <input matInput [matDatepicker]="dp1" [(ngModel)]="dateFrom">
            <mat-datepicker-toggle matSuffix [for]="dp1"></mat-datepicker-toggle>
            <mat-datepicker #dp1></mat-datepicker>
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>إلى تاريخ</mat-label>
            <input matInput [matDatepicker]="dp2" [(ngModel)]="dateTo">
            <mat-datepicker-toggle matSuffix [for]="dp2"></mat-datepicker-toggle>
            <mat-datepicker #dp2></mat-datepicker>
          </mat-form-field>
          <button mat-raised-button color="primary" (click)="loadHistory()" [disabled]="!selectedMerchantId" style="border-radius:10px">بحث</button>
        </div>
      </mat-card>

      <div *ngIf="loading" style="text-align:center;padding:32px"><mat-spinner diameter="40"></mat-spinner></div>

      <!-- Merchant Summary -->
      <mat-card *ngIf="history" class="summary-card">
        <h2>{{ history.merchantName }}</h2>
        <p>📞 {{ history.phone || 'لا يوجد رقم' }} — 📍 {{ history.city || '—' }}</p>
        <div class="summary-nums">
          <div><span>إجمالي الفواتير</span><strong>{{ history.totalInvoices }}</strong></div>
          <div><span>إجمالي المبيعات</span><strong>{{ history.activeTotal | number:'1.2-2' }} جنيه</strong></div>
        </div>
      </mat-card>

      <!-- History Table -->
      <div class="desktop-table" *ngIf="history?.invoices?.length">
        <table mat-table [dataSource]="history.invoices">
          <ng-container matColumnDef="date"><th mat-header-cell *matHeaderCellDef>التاريخ</th><td mat-cell *matCellDef="let r">{{ r.sessionDate | date:'dd/MM/yyyy' }}</td></ng-container>
          <ng-container matColumnDef="invoiceNum"><th mat-header-cell *matHeaderCellDef>رقم الفاتورة</th><td mat-cell *matCellDef="let r">{{ r.invoiceNumber }}</td></ng-container>
          <ng-container matColumnDef="route"><th mat-header-cell *matHeaderCellDef>الخط</th><td mat-cell *matCellDef="let r">{{ r.routeName || '—' }}</td></ng-container>
          <ng-container matColumnDef="employee"><th mat-header-cell *matHeaderCellDef>الموظف</th><td mat-cell *matCellDef="let r">{{ r.callCenterEmployeeName || '—' }}</td></ng-container>
          <ng-container matColumnDef="amount"><th mat-header-cell *matHeaderCellDef>المبلغ</th><td mat-cell *matCellDef="let r">{{ r.amount | number:'1.2-2' }}</td></ng-container>
          <ng-container matColumnDef="status">
            <th mat-header-cell *matHeaderCellDef>الحالة</th>
            <td mat-cell *matCellDef="let r">
              <span [class]="'status-' + r.invoiceStatus.toLowerCase()">{{ r.invoiceStatus==='Active' ? 'نشطة' : r.invoiceStatus==='Cancelled' ? 'ملغاة' : 'معدّلة' }}</span>
            </td>
          </ng-container>
          <tr mat-header-row *matHeaderRowDef="histCols"></tr>
          <tr mat-row *matRowDef="let r; columns: histCols;" [class.row-cancelled]="r.invoiceStatus==='Cancelled'"></tr>
        </table>
      </div>

      <!-- Mobile Cards -->
      <div class="mobile-cards" *ngIf="history?.invoices?.length">
        <mat-card *ngFor="let inv of history.invoices" class="inv-card" [class.card-cancelled]="inv.invoiceStatus==='Cancelled'">
          <div class="card-row"><strong>{{ inv.invoiceNumber }}</strong><span class="amt">{{ inv.amount | number:'1.2-2' }} جنيه</span></div>
          <div class="card-row">📅 {{ inv.sessionDate | date:'dd/MM/yyyy' }} — 🚛 {{ inv.routeName || '—' }}</div>
          <div class="card-row" *ngIf="inv.callCenterEmployeeName">👤 {{ inv.callCenterEmployeeName }}</div>
        </mat-card>
      </div>
    </div>
  `,
  styles: [`
    .search-card { border-radius:16px !important; padding:16px !important; margin-bottom:16px; }
    .search-fields { display:flex; gap:12px; flex-wrap:wrap; align-items:center; }
    .wide-field { min-width:250px; flex:1; }
    .summary-card { border-radius:16px !important; padding:20px !important; margin-bottom:16px; background:linear-gradient(135deg,#f0f4ff,#e8eeff); }
    .summary-nums { display:flex; gap:32px; margin-top:12px; }
    .summary-nums div { text-align:center; }
    .summary-nums span { display:block; font-size:0.85rem; color:#6b7280; }
    .summary-nums strong { font-size:1.3rem; color:#1e293b; }
    .status-active { color:#22c55e; } .status-cancelled { color:#dc2626; text-decoration:line-through; } .status-modified { color:#f59e0b; }
    .row-cancelled td { opacity:0.5; }
    .card-cancelled { opacity:0.6; border-right:4px solid #f44336 !important; }
    .desktop-table { display:block; } .mobile-cards { display:none; }
    @media(max-width:768px) { .desktop-table { display:none; } .mobile-cards { display:block; } .search-fields { flex-direction:column; } .wide-field { min-width:unset; } }
    .inv-card { margin-bottom:8px; padding:12px !important; border-radius:10px !important; }
    .card-row { margin-bottom:4px; display:flex; justify-content:space-between; flex-wrap:wrap; }
    .amt { font-weight:700; color:#22c55e; }
  `]
})
export class AjalMerchantSearchComponent {
  merchantSearch = '';
  suggestions: any[] = [];
  allMerchants: any[] = [];
  selectedMerchantId: number | null = null;
  dateFrom: Date | null = null; dateTo: Date | null = null;
  history: any = null; loading = false;
  histCols = ['date', 'invoiceNum', 'route', 'employee', 'amount', 'status'];

  constructor(private api: ApiService, private snack: MatSnackBar) {
    this.api.getMerchants(1, 1000).subscribe((res: any) => this.allMerchants = res.data || []);
  }

  filterMerchants() {
    const q = (this.merchantSearch || '').trim().toLowerCase();
    this.suggestions = q.length >= 2 ? this.allMerchants.filter((m: any) => m.merchantName.toLowerCase().includes(q)).slice(0, 10) : [];
  }

  selectMerchant(event: any) {
    const m = this.allMerchants.find((x: any) => x.merchantName === event.option.value);
    if (m) { this.selectedMerchantId = m.merchantId; this.loadHistory(); }
  }

  loadHistory() {
    if (!this.selectedMerchantId) return;
    this.loading = true;
    const from = this.dateFrom ? this.dateFrom.toISOString().split('T')[0] : undefined;
    const to = this.dateTo ? this.dateTo.toISOString().split('T')[0] : undefined;
    this.api.getAjalMerchantHistory(this.selectedMerchantId, from, to).subscribe({
      next: (d: any) => { this.history = d; this.loading = false; },
      error: () => { this.loading = false; this.snack.open('خطأ في تحميل البيانات', 'إغلاق', { duration: 3000 }); }
    });
  }
}
