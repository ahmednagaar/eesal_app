import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatTableModule } from '@angular/material/table';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Subject, debounceTime, switchMap } from 'rxjs';
import { ApiService } from '../../core/api.service';
import { GapAlertDialogComponent } from './gap-alert-dialog.component';

interface ReceiptRow {
  receiptNumber: number | null;
  bookId: number | null;
  merchantId: number | null;
  merchantSearch: string;
  amount: number | null;
  isPartialPayment: boolean;
  notes: string;
  isDuplicate: boolean;
}

@Component({
  selector: 'app-new-session',
  standalone: true,
  imports: [
    CommonModule, FormsModule, MatCardModule, MatFormFieldModule, MatInputModule,
    MatSelectModule, MatButtonModule, MatIconModule, MatDatepickerModule,
    MatNativeDateModule, MatTableModule, MatAutocompleteModule, MatCheckboxModule,
    MatDialogModule, MatSnackBarModule, MatProgressSpinnerModule, MatTooltipModule
  ],
  template: `
    <div class="page-container">
      <div class="page-header">
        <h1>إدخال تحصيل جديد</h1>
      </div>

      <!-- Step 1: Session Info -->
      <mat-card class="session-card">
        <div class="card-section-title">
          <mat-icon>person</mat-icon>
          <h3>بيانات الجلسة</h3>
        </div>
        <div class="form-row">
          <mat-form-field appearance="outline">
            <mat-label>السائق / التباع</mat-label>
            <mat-select [(ngModel)]="session.driverId" (selectionChange)="onDriverChange()" id="driver-select">
              <mat-option *ngFor="let d of drivers" [value]="d.driverId">{{ d.fullName }}</mat-option>
            </mat-select>
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>التاريخ</mat-label>
            <input matInput [matDatepicker]="picker" [(ngModel)]="session.sessionDate" id="date-input">
            <mat-datepicker-toggle matSuffix [for]="picker"></mat-datepicker-toggle>
            <mat-datepicker #picker></mat-datepicker>
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>المنطقة / الخط</mat-label>
            <input matInput [(ngModel)]="session.routeArea" id="route-input">
          </mat-form-field>
        </div>

        <div *ngIf="lastSession" class="last-session-info">
          <mat-icon>info</mat-icon>
          آخر إيصال للسائق: <strong>{{ lastSession.lastReceiptNumber }}</strong>
          — يجب أن يبدأ الإيصال التالي من <strong>{{ lastSession.lastReceiptNumber + 1 }}</strong>
        </div>
      </mat-card>

      <!-- Step 2: Bulk Receipt Range Generator -->
      <mat-card class="session-card">
        <div class="card-section-title">
          <mat-icon>playlist_add</mat-icon>
          <h3>تنزيل مجموعة إيصالات</h3>
          <span class="section-hint">أدخل نطاق الأرقام وهيتم إنشاء كل الإيصالات مرة واحدة</span>
        </div>

        <div class="range-row">
          <mat-form-field appearance="outline" class="range-field">
            <mat-label>من رقم</mat-label>
            <input matInput type="number" [(ngModel)]="rangeFrom" id="range-from"
                   placeholder="مثال: 4523">
            <mat-icon matPrefix>first_page</mat-icon>
          </mat-form-field>

          <div class="range-arrow">
            <mat-icon>arrow_back</mat-icon>
          </div>

          <mat-form-field appearance="outline" class="range-field">
            <mat-label>إلى رقم</mat-label>
            <input matInput type="number" [(ngModel)]="rangeTo" id="range-to"
                   placeholder="مثال: 4533">
            <mat-icon matPrefix>last_page</mat-icon>
          </mat-form-field>

          <mat-form-field appearance="outline" style="width:160px" *ngIf="books.length > 0">
            <mat-label>الدفتر</mat-label>
            <mat-select [(ngModel)]="selectedBookId">
              <mat-option *ngFor="let b of books" [value]="b.bookId">
                {{ b.bookNumber }} ({{ b.startReceiptNumber }}-{{ b.endReceiptNumber }})
              </mat-option>
            </mat-select>
          </mat-form-field>

          <button mat-raised-button color="primary" (click)="generateRange()"
                  class="generate-btn"
                  [disabled]="!rangeFrom || !rangeTo || rangeTo < rangeFrom">
            <mat-icon>bolt</mat-icon>
            تنزيل {{ rangeFrom && rangeTo && rangeTo >= rangeFrom ? (rangeTo - rangeFrom + 1) : 0 }} إيصال
          </button>
        </div>

        <div *ngIf="rangeFrom && rangeTo && rangeTo >= rangeFrom" class="range-preview">
          <mat-icon>info_outline</mat-icon>
          سيتم إنشاء <strong>{{ rangeTo - rangeFrom + 1 }}</strong> إيصال
          (من {{ rangeFrom }} إلى {{ rangeTo }})
          — كل اللي عليك تملا المبلغ واسم التاجر جمب كل إيصال
        </div>

        <div *ngIf="rangeTo && rangeFrom && (rangeTo - rangeFrom) > 100" class="range-warning">
          <mat-icon>warning</mat-icon>
          عدد الإيصالات كبير ({{ rangeTo - rangeFrom + 1 }}) — تأكد إن الأرقام صحيحة
        </div>
      </mat-card>

      <!-- Or add single receipt -->
      <div class="or-divider" *ngIf="receipts.length === 0">
        <span>أو</span>
      </div>

      <div class="single-add-btn" *ngIf="receipts.length === 0">
        <button mat-stroked-button (click)="addRow()">
          <mat-icon>add</mat-icon> إضافة إيصال واحد
        </button>
      </div>

      <!-- Step 3: Receipt List -->
      <mat-card class="session-card" *ngIf="receipts.length > 0">
        <div class="card-section-title">
          <mat-icon>receipt_long</mat-icon>
          <h3>الإيصالات ({{ receipts.length }})</h3>
          <span class="spacer"></span>
          <button mat-stroked-button (click)="addRow()" style="border-radius:8px">
            <mat-icon>add</mat-icon> إضافة إيصال
          </button>
        </div>

        <!-- Desktop Table -->
        <div class="desktop-table">
          <table mat-table [dataSource]="receipts" class="full-width receipt-table">
            <ng-container matColumnDef="num">
              <th mat-header-cell *matHeaderCellDef style="width:45px">#</th>
              <td mat-cell *matCellDef="let r; let i = index">
                <span class="row-number">{{ i + 1 }}</span>
              </td>
            </ng-container>
            <ng-container matColumnDef="receiptNumber">
              <th mat-header-cell *matHeaderCellDef>رقم الإيصال</th>
              <td mat-cell *matCellDef="let r">
                <mat-form-field appearance="outline" style="width:110px">
                  <input matInput type="number" [(ngModel)]="r.receiptNumber"
                         (blur)="checkDuplicate(r)">
                  <mat-error *ngIf="r.isDuplicate">مكرر!</mat-error>
                </mat-form-field>
              </td>
            </ng-container>
            <ng-container matColumnDef="merchant">
              <th mat-header-cell *matHeaderCellDef>التاجر</th>
              <td mat-cell *matCellDef="let r">
                <mat-form-field appearance="outline" style="width:200px">
                  <input matInput [matAutocomplete]="autoM" [(ngModel)]="r.merchantSearch"
                         (input)="searchMerchant(r.merchantSearch)" placeholder="ابحث عن التاجر...">
                  <mat-autocomplete #autoM="matAutocomplete" (optionSelected)="selectMerchant(r, $event)">
                    <mat-option *ngFor="let m of filteredMerchants" [value]="m.merchantName">
                      {{ m.merchantName }} <small *ngIf="m.city">- {{ m.city }}</small>
                    </mat-option>
                  </mat-autocomplete>
                </mat-form-field>
              </td>
            </ng-container>
            <ng-container matColumnDef="amount">
              <th mat-header-cell *matHeaderCellDef>المبلغ</th>
              <td mat-cell *matCellDef="let r">
                <mat-form-field appearance="outline" style="width:130px">
                  <input matInput type="number" [(ngModel)]="r.amount" placeholder="0.00">
                  <span matSuffix>جنيه</span>
                </mat-form-field>
              </td>
            </ng-container>
            <ng-container matColumnDef="notes">
              <th mat-header-cell *matHeaderCellDef>ملاحظات</th>
              <td mat-cell *matCellDef="let r">
                <mat-form-field appearance="outline" style="width:140px">
                  <input matInput [(ngModel)]="r.notes" placeholder="اختياري">
                </mat-form-field>
              </td>
            </ng-container>
            <ng-container matColumnDef="actions">
              <th mat-header-cell *matHeaderCellDef style="width:50px"></th>
              <td mat-cell *matCellDef="let r; let i = index">
                <button mat-icon-button color="warn" (click)="removeRow(i)"
                        matTooltip="حذف الإيصال">
                  <mat-icon>delete_outline</mat-icon>
                </button>
              </td>
            </ng-container>
            <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
            <tr mat-row *matRowDef="let row; columns: displayedColumns;"
                [class.duplicate-row]="row.isDuplicate"
                [class.filled-row]="row.amount && row.merchantId"></tr>
          </table>
        </div>

        <!-- Mobile Cards -->
        <div class="mobile-receipt-cards">
          <mat-card *ngFor="let r of receipts; let i = index" class="receipt-card" [class.has-error]="r.isDuplicate">
            <mat-card-header>
              <span style="font-weight:700">إيصال {{ r.receiptNumber || (i + 1) }}</span>
              <button mat-icon-button color="warn" (click)="removeRow(i)"><mat-icon>close</mat-icon></button>
            </mat-card-header>
            <mat-card-content style="padding:12px">
              <mat-form-field appearance="outline" class="full-width">
                <mat-label>رقم الإيصال</mat-label>
                <input matInput type="number" [(ngModel)]="r.receiptNumber" (blur)="checkDuplicate(r)">
              </mat-form-field>
              <mat-form-field appearance="outline" class="full-width">
                <mat-label>التاجر</mat-label>
                <input matInput [matAutocomplete]="autoMob" [(ngModel)]="r.merchantSearch"
                       (input)="searchMerchant(r.merchantSearch)">
                <mat-autocomplete #autoMob="matAutocomplete" (optionSelected)="selectMerchant(r, $event)">
                  <mat-option *ngFor="let m of filteredMerchants" [value]="m.merchantName">
                    {{ m.merchantName }}
                  </mat-option>
                </mat-autocomplete>
              </mat-form-field>
              <mat-form-field appearance="outline" class="full-width">
                <mat-label>المبلغ (جنيه)</mat-label>
                <input matInput type="number" [(ngModel)]="r.amount">
              </mat-form-field>
            </mat-card-content>
          </mat-card>
        </div>
      </mat-card>

      <!-- Totals Bar -->
      <div class="totals-bar" *ngIf="receipts.length > 0">
        <span>عدد الإيصالات: <strong>{{ receipts.length }}</strong></span>
        <span>مكتمل: <strong>{{ completedCount }}</strong> / {{ receipts.length }}</span>
        <span>الإجمالي: <strong>{{ totalAmount | number:'1.2-2' }} جنيه</strong></span>
        <button mat-raised-button color="primary" (click)="saveSession()"
                [disabled]="saving" class="save-btn">
          <mat-spinner *ngIf="saving" diameter="20"></mat-spinner>
          <span *ngIf="!saving"><mat-icon>save</mat-icon> حفظ الجلسة</span>
        </button>
      </div>
    </div>
  `,
  styles: [`
    .session-card {
      margin-bottom: 16px;
      padding: 24px !important;
      border-radius: 16px !important;
      border: 1px solid #e5e7eb;
    }

    .card-section-title {
      display: flex;
      align-items: center;
      gap: 10px;
      margin-bottom: 20px;

      mat-icon { color: #667eea; font-size: 24px; }
      h3 { margin: 0; font-weight: 700; font-size: 1.1rem; color: #1a1a2e; }
    }

    .section-hint {
      font-size: 0.8rem;
      color: #9ca3af;
      font-weight: 400;
      margin-right: auto;
    }

    /* ── Range Entry ── */
    .range-row {
      display: flex;
      align-items: center;
      gap: 12px;
      flex-wrap: wrap;
    }

    .range-field { flex: 1; min-width: 140px; }

    .range-arrow {
      display: flex;
      align-items: center;
      color: #667eea;
      mat-icon { font-size: 28px; width: 28px; height: 28px; }
    }

    .generate-btn {
      height: 56px !important;
      border-radius: 12px !important;
      font-size: 0.95rem !important;
      font-weight: 700 !important;
      padding: 0 24px !important;
      background: linear-gradient(135deg, #667eea, #764ba2) !important;
      color: #fff !important;
    }

    .range-preview {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 14px 18px;
      background: #dbeafe;
      border-radius: 10px;
      font-size: 0.88rem;
      color: #1e40af;
      margin-top: 14px;
      line-height: 1.7;

      mat-icon { font-size: 20px; width: 20px; height: 20px; flex-shrink: 0; }
    }

    .range-warning {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 12px 18px;
      background: #fef3cd;
      border-radius: 10px;
      font-size: 0.85rem;
      color: #92400e;
      margin-top: 10px;

      mat-icon { font-size: 20px; width: 20px; height: 20px; color: #f59e0b; }
    }

    /* ── Or Divider ── */
    .or-divider {
      text-align: center;
      margin: 16px 0;
      position: relative;

      span {
        background: #f0f2f5;
        padding: 0 16px;
        color: #9ca3af;
        font-size: 0.85rem;
        font-weight: 600;
      }

      &::before {
        content: '';
        position: absolute;
        top: 50%;
        left: 0;
        right: 0;
        height: 1px;
        background: #e5e7eb;
        z-index: 0;
      }

      span { position: relative; z-index: 1; }
    }

    .single-add-btn {
      text-align: center;
      margin-bottom: 16px;
    }

    /* ── Receipt Table ── */
    .receipt-table {
      .row-number {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        width: 28px;
        height: 28px;
        border-radius: 50%;
        background: #f3f4f6;
        font-size: 0.8rem;
        font-weight: 700;
        color: #6b7280;
      }
    }

    .filled-row {
      background: #f0fdf4 !important;
    }

    .duplicate-row { background: #fee2e2 !important; }

    .last-session-info {
      display: flex; align-items: center; gap: 8px; padding: 14px;
      background: #dbeafe; border-radius: 10px; font-size: 0.88rem; color: #1e40af;
      margin-top: 8px;
      mat-icon { font-size: 20px; width: 20px; height: 20px; }
    }

    .empty-state mat-icon { opacity: 0.5; }

    .save-btn {
      border-radius: 12px !important;
      padding: 0 24px !important;
    }

    @media (max-width: 599px) {
      .range-row { flex-direction: column; }
      .range-arrow { transform: rotate(90deg); }
      .range-field { min-width: 100%; }
      .generate-btn { width: 100% !important; }
      .section-hint { display: none; }
    }
  `]
})
export class NewSessionComponent implements OnInit {
  drivers: any[] = [];
  books: any[] = [];
  selectedBookId: number | null = null;
  lastSession: any = null;
  filteredMerchants: any[] = [];
  saving = false;

  // Bulk range fields
  rangeFrom: number | null = null;
  rangeTo: number | null = null;

  session = { driverId: null as number | null, sessionDate: new Date(), routeArea: '', notes: '' };
  receipts: ReceiptRow[] = [];
  displayedColumns = ['num', 'receiptNumber', 'merchant', 'amount', 'notes', 'actions'];

  private searchSubject = new Subject<string>();

  constructor(private api: ApiService, private dialog: MatDialog, private snack: MatSnackBar, private router: Router) {}

  ngOnInit() {
    this.api.getDrivers().subscribe(d => this.drivers = d);
    this.api.getBooks().subscribe(b => this.books = b);
    this.searchSubject.pipe(
      debounceTime(300),
      switchMap(q => this.api.searchMerchants(q))
    ).subscribe(m => this.filteredMerchants = m);
  }

  onDriverChange() {
    if (!this.session.driverId) return;
    this.api.getLastSessionForDriver(this.session.driverId).subscribe({
      next: (s: any) => { this.lastSession = s.lastReceiptNumber ? s : null; },
      error: () => { this.lastSession = null; }
    });
    const driverBook = this.books.find(b => b.assignedToDriverId === this.session.driverId);
    if (driverBook) this.selectedBookId = driverBook.bookId;
  }

  // ── BULK RANGE GENERATOR ──
  generateRange() {
    if (!this.rangeFrom || !this.rangeTo || this.rangeTo < this.rangeFrom) return;

    const count = this.rangeTo - this.rangeFrom + 1;
    if (count > 200) {
      this.snack.open('الحد الأقصى 200 إيصال في المرة الواحدة', 'إغلاق', { duration: 3000 });
      return;
    }

    const newReceipts: ReceiptRow[] = [];
    for (let num = this.rangeFrom; num <= this.rangeTo; num++) {
      newReceipts.push({
        receiptNumber: num,
        bookId: this.selectedBookId,
        merchantId: null,
        merchantSearch: '',
        amount: null,
        isPartialPayment: false,
        notes: '',
        isDuplicate: false
      });
    }

    this.receipts = [...this.receipts, ...newReceipts];
    this.snack.open(`تم إنشاء ${count} إيصال — املا المبلغ واسم التاجر لكل إيصال ✅`, 'إغلاق', { duration: 4000 });

    // Clear range inputs
    this.rangeFrom = null;
    this.rangeTo = null;
  }

  addRow() {
    const nextNum = this.getNextReceiptNumber();
    this.receipts = [...this.receipts, {
      receiptNumber: nextNum, bookId: this.selectedBookId, merchantId: null,
      merchantSearch: '', amount: null, isPartialPayment: false, notes: '', isDuplicate: false
    }];
  }

  removeRow(index: number) {
    this.receipts = this.receipts.filter((_, i) => i !== index);
  }

  get totalAmount(): number {
    return this.receipts.reduce((sum, r) => sum + (r.amount || 0), 0);
  }

  get completedCount(): number {
    return this.receipts.filter(r => r.amount && r.merchantId).length;
  }

  getNextReceiptNumber(): number | null {
    if (this.receipts.length > 0) {
      const last = this.receipts[this.receipts.length - 1].receiptNumber;
      return last ? last + 1 : null;
    }
    if (this.lastSession) return this.lastSession.lastReceiptNumber + 1;
    return null;
  }

  searchMerchant(query: string) {
    if (query && query.length >= 1) this.searchSubject.next(query);
  }

  selectMerchant(row: ReceiptRow, event: any) {
    const m = this.filteredMerchants.find(m => m.merchantName === event.option.value);
    if (m) { row.merchantId = m.merchantId; row.merchantSearch = m.merchantName; }
  }

  checkDuplicate(row: ReceiptRow) {
    if (!row.receiptNumber) { row.isDuplicate = false; return; }
    const dupeInList = this.receipts.filter(r => r.receiptNumber === row.receiptNumber).length > 1;
    if (dupeInList) { row.isDuplicate = true; return; }
    this.api.checkDuplicate(row.receiptNumber).subscribe({
      next: (res: any) => { row.isDuplicate = res.exists; },
      error: () => {}
    });
  }

  saveSession() {
    if (!this.session.driverId) { this.snack.open('اختر السائق', 'إغلاق', { duration: 3000 }); return; }
    if (!this.session.routeArea) { this.snack.open('أدخل المنطقة', 'إغلاق', { duration: 3000 }); return; }
    if (this.receipts.length === 0) { this.snack.open('أضف إيصال واحد على الأقل', 'إغلاق', { duration: 3000 }); return; }

    const invalid = this.receipts.some(r => !r.receiptNumber || !r.merchantId || !r.amount);
    if (invalid) { this.snack.open('أكمل بيانات جميع الإيصالات (رقم + تاجر + مبلغ)', 'إغلاق', { duration: 3000 }); return; }

    if (this.receipts.some(r => r.isDuplicate)) { this.snack.open('يوجد أرقام إيصالات مكررة', 'إغلاق', { duration: 3000 }); return; }

    this.saving = true;
    const payload = {
      driverId: this.session.driverId,
      sessionDate: this.session.sessionDate.toISOString(),
      routeArea: this.session.routeArea,
      notes: this.session.notes,
      receipts: this.receipts.map(r => ({
        receiptNumber: r.receiptNumber, bookId: r.bookId || this.selectedBookId || 1,
        merchantId: r.merchantId, amount: r.amount,
        isPartialPayment: r.isPartialPayment, notes: r.notes
      }))
    };

    this.api.createSession(payload).subscribe({
      next: (res: any) => {
        this.saving = false;
        if (res.hasGaps && res.missingReceipts?.length > 0) {
          const driverName = this.drivers.find(d => d.driverId === this.session.driverId)?.fullName || '';
          this.dialog.open(GapAlertDialogComponent, {
            data: { missingReceipts: res.missingReceipts, driverName },
            width: '500px', disableClose: true
          }).afterClosed().subscribe(() => this.router.navigate(['/dashboard']));
        } else {
          this.snack.open('تم حفظ الجلسة بنجاح ✅', 'إغلاق', { duration: 3000 });
          this.router.navigate(['/dashboard']);
        }
      },
      error: (err) => {
        this.saving = false;
        this.snack.open(err.error?.message || 'حدث خطأ', 'إغلاق', { duration: 5000 });
      }
    });
  }
}
