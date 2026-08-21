import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatChipsModule } from '@angular/material/chips';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RouterModule } from '@angular/router';
import { ApiService } from '../../core/api.service';

interface InvoiceRow {
  invoiceSuffix: string; invoiceNumber: string | null; merchantId: number | null;
  merchantSearch: string; merchantPhone: string | null; merchantSuggestions: any[];
  callCenterEmployeeName: string; employeeSuggestions: string[];
  amount: number | null; notes: string; isDuplicate: boolean; hasError: boolean;
}

@Component({
  selector: 'app-ajal-entry',
  standalone: true,
  imports: [CommonModule, FormsModule, MatCardModule, MatFormFieldModule, MatInputModule, MatButtonModule,
    MatIconModule, MatSelectModule, MatChipsModule, MatSnackBarModule, MatProgressSpinnerModule,
    MatDatepickerModule, MatNativeDateModule, MatAutocompleteModule, MatTooltipModule, RouterModule],
  template: `
    <div class="page-container">
      <div class="page-header"><h1>✏️ إدخال فواتير — دفتر الآجل</h1></div>

      <!-- Session Header -->
      <mat-card class="session-card">
        <div class="session-fields">
          <mat-form-field appearance="outline">
            <mat-label>تاريخ اليومية</mat-label>
            <input matInput [matDatepicker]="dp" [(ngModel)]="sessionDate">
            <mat-datepicker-toggle matSuffix [for]="dp"></mat-datepicker-toggle>
            <mat-datepicker #dp></mat-datepicker>
            <mat-hint>تاريخ اليومية التي أقفلها المدير</mat-hint>
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>الخط (اختياري)</mat-label>
            <mat-select [(ngModel)]="sessionRouteId">
              <mat-option [value]="null">— بدون خط —</mat-option>
              <mat-option *ngFor="let r of routes" [value]="r.routeId">{{ r.routeName }}</mat-option>
            </mat-select>
          </mat-form-field>
        </div>
      </mat-card>

      <!-- Prefix Bar -->
      <div class="prefix-bar">
        <span>البادئة الحالية:</span>
        <strong class="prefix-badge">{{ currentPrefix }}</strong>
        <span class="prefix-hint">← أدخل فقط آخر 3 أرقام</span>
        <span class="prefix-warn" *ngIf="prefixWarning">⚠️ قاربت على الانتهاء</span>
      </div>

      <!-- Desktop Table -->
      <div class="entry-wrapper desktop-entry">
        <table class="entry-table">
          <thead>
            <tr><th>رقم الفاتورة</th><th>التاجر</th><th>الموظف</th><th>المبلغ (جنيه)</th><th>ملاحظات</th><th></th></tr>
          </thead>
          <tbody>
            <tr *ngFor="let row of rows; let i = index" [class.row-error]="row.hasError" [class.row-dup]="row.isDuplicate">
              <td class="inv-cell">
                <span class="prefix-lbl">{{ currentPrefix }}</span>
                <input class="suffix-input" type="text" maxlength="3" [(ngModel)]="row.invoiceSuffix"
                  (ngModelChange)="onSuffixChange(row)" (blur)="checkDuplicate(row)" placeholder="485"
                  [class.input-error]="row.isDuplicate">
                <span class="full-num" *ngIf="row.invoiceNumber">← {{ row.invoiceNumber }}</span>
                <div class="dup-error" *ngIf="row.isDuplicate">مسجل مسبقاً!</div>
              </td>
              <td>
                <input class="merch-input" [(ngModel)]="row.merchantSearch" (input)="searchMerchants(row)"
                  [matAutocomplete]="autoM" placeholder="اسم التاجر">
                <mat-autocomplete #autoM (optionSelected)="selectMerchant(row, $event)">
                  <mat-option *ngFor="let m of row.merchantSuggestions" [value]="m.merchantName">
                    {{ m.merchantName }} <small *ngIf="m.city">— {{ m.city }}</small>
                  </mat-option>
                </mat-autocomplete>
                <div class="phone-disp" *ngIf="row.merchantPhone">📞 {{ row.merchantPhone }}</div>
              </td>
              <td>
                <input class="emp-input" [(ngModel)]="row.callCenterEmployeeName" (input)="filterEmployees(row)"
                  [matAutocomplete]="autoE" placeholder="اسم الموظف">
                <mat-autocomplete #autoE>
                  <mat-option *ngFor="let e of row.employeeSuggestions" [value]="e">{{ e }}</mat-option>
                </mat-autocomplete>
              </td>
              <td><input class="amt-input" type="number" [(ngModel)]="row.amount" (ngModelChange)="updateTotal()" placeholder="0"></td>
              <td><input class="notes-input" [(ngModel)]="row.notes" placeholder="ملاحظات"></td>
              <td><button mat-icon-button color="warn" (click)="removeRow(i)"><mat-icon>delete</mat-icon></button></td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Mobile Cards -->
      <div class="mobile-entry">
        <mat-card *ngFor="let row of rows; let i = index" class="mobile-inv-card" [class.row-dup]="row.isDuplicate">
          <div class="mobile-field">
            <label>رقم الفاتورة</label>
            <div class="inv-cell">
              <span class="prefix-lbl">{{ currentPrefix }}</span>
              <input class="suffix-input" type="text" maxlength="3" [(ngModel)]="row.invoiceSuffix"
                (ngModelChange)="onSuffixChange(row)" (blur)="checkDuplicate(row)" placeholder="485">
              <span class="full-num" *ngIf="row.invoiceNumber">{{ row.invoiceNumber }}</span>
            </div>
            <div class="dup-error" *ngIf="row.isDuplicate">مسجل مسبقاً!</div>
          </div>
          <div class="mobile-field">
            <label>التاجر</label>
            <input [(ngModel)]="row.merchantSearch" (input)="searchMerchants(row)" [matAutocomplete]="autoMM" placeholder="اسم التاجر">
            <mat-autocomplete #autoMM (optionSelected)="selectMerchant(row, $event)">
              <mat-option *ngFor="let m of row.merchantSuggestions" [value]="m.merchantName">{{ m.merchantName }}</mat-option>
            </mat-autocomplete>
          </div>
          <div class="mobile-field">
            <label>الموظف</label>
            <input [(ngModel)]="row.callCenterEmployeeName" (input)="filterEmployees(row)" [matAutocomplete]="autoEE">
            <mat-autocomplete #autoEE><mat-option *ngFor="let e of row.employeeSuggestions" [value]="e">{{ e }}</mat-option></mat-autocomplete>
          </div>
          <div class="mobile-field">
            <label>المبلغ</label>
            <input type="number" [(ngModel)]="row.amount" (ngModelChange)="updateTotal()">
          </div>
          <button mat-icon-button color="warn" (click)="removeRow(i)" class="mobile-delete"><mat-icon>delete</mat-icon></button>
        </mat-card>
      </div>

      <!-- Controls -->
      <div class="entry-controls">
        <button mat-stroked-button (click)="addRow()" style="border-radius:10px">+ فاتورة</button>
        <button mat-stroked-button (click)="addRows(5)" style="border-radius:10px">+ 5 صفوف</button>
        <span class="total-display">الإجمالي: <strong>{{ totalAmount | number:'1.2-2' }} جنيه</strong> ({{ validRows }} فاتورة)</span>
        <button mat-raised-button color="primary" (click)="saveAll()" [disabled]="!isValid || saving" style="border-radius:10px">
          <mat-spinner *ngIf="saving" diameter="18" style="display:inline-block;margin-left:8px"></mat-spinner>
          {{ saving ? 'جاري الحفظ...' : 'حفظ الفواتير' }}
        </button>
      </div>
    </div>
  `,
  styles: [`
    .session-card { border-radius:16px !important; margin-bottom:16px; padding:16px !important; }
    .session-fields { display:flex; gap:16px; flex-wrap:wrap; }
    .prefix-bar { display:flex; align-items:center; gap:12px; padding:12px 16px; background:#f0f4ff; border-radius:12px; margin-bottom:16px; flex-wrap:wrap; }
    .prefix-badge { background:#667eea; color:#fff; padding:4px 16px; border-radius:10px; font-size:1.2rem; }
    .prefix-hint { color:#6b7280; font-size:0.85rem; }
    .prefix-warn { color:#dc2626; font-weight:700; }
    .entry-table { width:100%; border-collapse:collapse; }
    .entry-table th { background:#f8f9fa; padding:10px 8px; text-align:right; font-weight:600; border-bottom:2px solid #e5e7eb; }
    .entry-table td { padding:6px 4px; border-bottom:1px solid #f3f4f6; vertical-align:top; }
    .inv-cell { display:flex; align-items:center; gap:4px; flex-wrap:wrap; direction:ltr; }
    .prefix-lbl { background:#e0e7ff; color:#4338ca; padding:4px 8px; border-radius:6px; font-weight:700; font-size:0.9rem; }
    .suffix-input { width:60px; padding:6px 8px; border:1px solid #d1d5db; border-radius:6px; text-align:center; font-size:1rem; font-weight:700; }
    .suffix-input.input-error { border-color:#dc2626; background:#fef2f2; }
    .full-num { color:#667eea; font-size:0.8rem; font-weight:600; }
    .dup-error { color:#dc2626; font-size:0.75rem; font-weight:600; }
    .merch-input, .emp-input, .notes-input { width:100%; padding:6px 8px; border:1px solid #d1d5db; border-radius:6px; }
    .amt-input { width:90px; padding:6px 8px; border:1px solid #d1d5db; border-radius:6px; text-align:center; }
    .phone-disp { font-size:0.75rem; color:#667eea; margin-top:2px; }
    .row-error { background:#fef2f2; } .row-dup { background:#fef2f2; }
    .entry-controls { display:flex; align-items:center; gap:12px; flex-wrap:wrap; margin-top:16px; padding:16px; background:#f8f9fa; border-radius:14px; }
    .total-display { flex:1; text-align:left; font-size:1rem; }
    .desktop-entry { display:block; } .mobile-entry { display:none; }
    @media(max-width:768px) { .desktop-entry { display:none; } .mobile-entry { display:block; } }
    .mobile-inv-card { margin-bottom:12px; padding:12px !important; border-radius:12px !important; position:relative; }
    .mobile-field { margin-bottom:8px; }
    .mobile-field label { display:block; font-size:0.8rem; color:#6b7280; margin-bottom:2px; }
    .mobile-field input { width:100%; padding:8px; border:1px solid #d1d5db; border-radius:8px; }
    .mobile-delete { position:absolute; top:4px; left:4px; }
  `]
})
export class AjalEntryComponent implements OnInit {
  sessionDate = new Date();
  sessionRouteId: number | null = null;
  routes: any[] = [];
  currentPrefix = '441';
  prefixWarning = false;
  rows: InvoiceRow[] = [];
  allMerchants: any[] = [];
  allEmployeeNames: string[] = [];
  totalAmount = 0;
  saving = false;

  get validRows() { return this.rows.filter(r => r.invoiceNumber && r.merchantId && r.amount && r.amount > 0 && !r.isDuplicate).length; }
  get isValid() { return this.validRows > 0; }

  constructor(private api: ApiService, private snack: MatSnackBar) {}

  ngOnInit() {
    this.api.getRoutes().subscribe((r: any) => this.routes = r);
    this.api.getMerchants(1, 1000).subscribe((res: any) => this.allMerchants = res.data || []);
    this.api.getAjalEmployeeNames().subscribe(n => this.allEmployeeNames = n);
    this.api.getAjalPrefixSettings().subscribe(s => {
      this.currentPrefix = s.prefix;
      // Check if we need warning
    });
    this.addRows(3);
  }

  addRow() {
    this.rows.push({
      invoiceSuffix: '', invoiceNumber: null, merchantId: null, merchantSearch: '',
      merchantPhone: null, merchantSuggestions: [], callCenterEmployeeName: '',
      employeeSuggestions: [], amount: null, notes: '', isDuplicate: false, hasError: false
    });
  }
  addRows(n: number) { for (let i = 0; i < n; i++) this.addRow(); }
  removeRow(i: number) { this.rows.splice(i, 1); this.updateTotal(); }

  onSuffixChange(row: InvoiceRow) {
    if (row.invoiceSuffix && row.invoiceSuffix.length > 0) {
      const suffix = row.invoiceSuffix.padStart(3, '0');
      row.invoiceNumber = `${this.currentPrefix}${suffix}`;
    } else { row.invoiceNumber = null; }
  }

  checkDuplicate(row: InvoiceRow) {
    if (!row.invoiceNumber) return;
    // Check locally first
    const dupsLocal = this.rows.filter(r => r !== row && r.invoiceNumber === row.invoiceNumber);
    if (dupsLocal.length > 0) { row.isDuplicate = true; return; }
    // Check API (simple search)
    this.api.searchAjal({ invoiceNumber: row.invoiceNumber, pageSize: 1 }).subscribe((res: any) => {
      row.isDuplicate = res.totalCount > 0;
    });
  }

  searchMerchants(row: InvoiceRow) {
    const q = (row.merchantSearch || '').trim().toLowerCase();
    if (q.length < 2) { row.merchantSuggestions = []; return; }
    row.merchantSuggestions = this.allMerchants.filter((m: any) => m.merchantName.toLowerCase().includes(q)).slice(0, 10);
  }

  selectMerchant(row: InvoiceRow, event: any) {
    const m = this.allMerchants.find((x: any) => x.merchantName === event.option.value);
    if (m) { row.merchantId = m.merchantId; row.merchantPhone = m.phoneNumber; row.merchantSearch = m.merchantName; }
  }

  filterEmployees(row: InvoiceRow) {
    const q = (row.callCenterEmployeeName || '').trim().toLowerCase();
    row.employeeSuggestions = q.length > 0 ? this.allEmployeeNames.filter(n => n.toLowerCase().includes(q)) : [...this.allEmployeeNames];
  }

  updateTotal() { this.totalAmount = this.rows.reduce((s, r) => s + (r.amount || 0), 0); }

  saveAll() {
    const valid = this.rows.filter(r => r.invoiceNumber && r.merchantId && r.amount && r.amount > 0 && !r.isDuplicate);
    if (valid.length === 0) { this.snack.open('لا توجد فواتير صالحة للحفظ', 'إغلاق', { duration: 3000 }); return; }
    this.saving = true;
    const data = {
      sessionDate: this.sessionDate.toISOString(),
      routeId: this.sessionRouteId,
      invoices: valid.map(r => ({
        invoiceNumber: r.invoiceNumber, merchantId: r.merchantId,
        callCenterEmployeeName: r.callCenterEmployeeName || null, amount: r.amount, notes: r.notes || null
      }))
    };
    this.api.createAjalInvoices(data).subscribe({
      next: (res: any) => {
        this.saving = false;
        if (res.success) {
          this.snack.open(`✅ تم حفظ ${res.saved} فاتورة بنجاح`, 'حسناً', { duration: 3000 });
          // Add the employee name to autocomplete if new
          valid.forEach(r => {
            if (r.callCenterEmployeeName && !this.allEmployeeNames.includes(r.callCenterEmployeeName))
              this.allEmployeeNames.push(r.callCenterEmployeeName);
          });
          this.rows = []; this.addRows(3); this.totalAmount = 0;
        } else { this.snack.open(res.errors?.join('\n') || 'خطأ', 'إغلاق', { duration: 5000 }); }
      },
      error: (err: any) => {
        this.saving = false;
        const msg = err.error?.errors?.join('\n') || err.error?.message || 'حدث خطأ';
        this.snack.open(msg, 'إغلاق', { duration: 5000 });
      }
    });
  }
}
