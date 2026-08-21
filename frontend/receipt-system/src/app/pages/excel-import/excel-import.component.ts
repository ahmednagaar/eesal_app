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
import { MatStepperModule } from '@angular/material/stepper';
import { ApiService } from '../../core/api.service';

@Component({
  selector: 'app-excel-import',
  standalone: true,
  imports: [
    CommonModule, FormsModule, MatCardModule, MatFormFieldModule, MatInputModule,
    MatButtonModule, MatIconModule, MatSelectModule, MatTableModule, MatChipsModule,
    MatSnackBarModule, MatProgressSpinnerModule, MatDatepickerModule, MatNativeDateModule,
    MatStepperModule
  ],
  template: `
    <div class="page-container">
      <div class="page-header">
        <h1>📤 رفع ملف Excel</h1>
      </div>

      <!-- Steps indicator -->
      <div class="steps-bar">
        <div class="step-item" *ngFor="let s of steps; let i = index"
             [class.active]="currentStep === i + 1" [class.done]="currentStep > i + 1">
          <span class="step-num">{{ currentStep > i + 1 ? '✓' : i + 1 }}</span>
          <span class="step-label">{{ s }}</span>
        </div>
      </div>

      <!-- STEP 1: File Upload -->
      <mat-card class="step-card" *ngIf="currentStep === 1">
        <div class="upload-zone" (drop)="onDrop($event)" (dragover)="$event.preventDefault()"
             (click)="fileInput.click()">
          <mat-icon class="upload-icon">upload_file</mat-icon>
          <p class="upload-title">اسحب ملف Excel هنا أو اضغط للاختيار</p>
          <p class="upload-hint">الملف يجب أن يكون بصيغة .xlsx ولا يتجاوز 5 ميجابايت</p>
          <input #fileInput type="file" accept=".xlsx" hidden (change)="onFileSelected($event)">
        </div>
        <div class="loading-container" *ngIf="uploading">
          <mat-spinner diameter="40"></mat-spinner>
          <p>جاري قراءة الملف...</p>
        </div>
      </mat-card>

      <!-- STEP 2: Preview -->
      <mat-card class="step-card" *ngIf="currentStep === 2 && preview">
        <div class="preview-header">
          <mat-icon color="primary">table_view</mat-icon>
          <span>تم قراءة <strong>{{ preview.rowCount }}</strong> صف</span>
          <span class="total-badge">الإجمالي: {{ preview.totalAmount | number:'1.2-2' }} جنيه</span>
        </div>

        <mat-card class="warning-card" *ngIf="newMerchantsCount > 0">
          <mat-icon color="warn">warning</mat-icon>
          ⚠️ يوجد {{ newMerchantsCount }} تاجر غير موجود في النظام — سيتم إنشاؤهم تلقائياً
        </mat-card>

        <mat-card class="danger-card" *ngIf="preview.withoutReceiptCount > 0">
          <mat-icon color="warn">report_problem</mat-icon>
          ⚠️ يوجد {{ preview.withoutReceiptCount }} تاجر دفع بدون إيصال — تحتاج متابعة مع السائق
        </mat-card>

        <!-- Desktop table -->
        <div class="desktop-table">
          <table mat-table [dataSource]="preview.rows">
            <ng-container matColumnDef="rowIndex">
              <th mat-header-cell *matHeaderCellDef>#</th>
              <td mat-cell *matCellDef="let row">{{ row.rowIndex }}</td>
            </ng-container>
            <ng-container matColumnDef="merchantName">
              <th mat-header-cell *matHeaderCellDef>اسم التاجر</th>
              <td mat-cell *matCellDef="let row">
                {{ row.merchantNameRaw }}
                <span class="match-badge existing" *ngIf="!row.isNewMerchant">✓ موجود</span>
                <span class="match-badge new-merchant" *ngIf="row.isNewMerchant">جديد</span>
              </td>
            </ng-container>
            <ng-container matColumnDef="correctedName">
              <th mat-header-cell *matHeaderCellDef>الاسم في النظام</th>
              <td mat-cell *matCellDef="let row">
                <input *ngIf="row.isNewMerchant" class="inline-input" [(ngModel)]="row.newMerchantName">
                <span *ngIf="!row.isNewMerchant">{{ row.matchedMerchantName }}</span>
              </td>
            </ng-container>
            <ng-container matColumnDef="amount">
              <th mat-header-cell *matHeaderCellDef>المبلغ</th>
              <td mat-cell *matCellDef="let row">{{ row.amount | number:'1.2-2' }}</td>
            </ng-container>
            <ng-container matColumnDef="receipt">
              <th mat-header-cell *matHeaderCellDef>الإيصال</th>
              <td mat-cell *matCellDef="let row">
                <span class="match-badge danger" *ngIf="row.isWithoutReceipt">⚠️ بدون</span>
                <span class="match-badge existing" *ngIf="!row.isWithoutReceipt">✓ بإيصال</span>
              </td>
            </ng-container>
            <tr mat-header-row *matHeaderRowDef="previewCols"></tr>
            <tr mat-row *matRowDef="let row; columns: previewCols;"
                [class.new-row]="row.isNewMerchant"
                [class.warning-row]="row.isWithoutReceipt"></tr>
          </table>
        </div>

        <!-- Mobile cards -->
        <div class="mobile-receipt-cards">
          <mat-card *ngFor="let row of preview.rows" class="mobile-card"
                    [class.new-row]="row.isNewMerchant">
            <div class="mobile-row"><strong>#{{ row.rowIndex }}</strong> {{ row.merchantNameRaw }}</div>
            <div class="mobile-row">المبلغ: {{ row.amount | number:'1.2-2' }} جنيه</div>
            <div class="mobile-row" *ngIf="row.isNewMerchant">
              <span class="match-badge new-merchant">تاجر جديد</span>
              <input class="inline-input" [(ngModel)]="row.newMerchantName" placeholder="تصحيح الاسم">
            </div>
            <div class="mobile-row" *ngIf="row.isWithoutReceipt">
              <span class="match-badge danger">⚠️ بدون إيصال</span>
            </div>
          </mat-card>
        </div>

        <div class="step-actions">
          <button mat-stroked-button (click)="resetUpload()" style="border-radius:10px !important">رفع ملف آخر</button>
          <button mat-raised-button color="primary" (click)="currentStep = 3" style="border-radius:10px !important">
            التالي — تفاصيل الجلسة
          </button>
        </div>
      </mat-card>

      <!-- STEP 3: Session Details -->
      <mat-card class="step-card" *ngIf="currentStep === 3">
        <h3 style="margin:0 0 16px;font-weight:700">تفاصيل الجلسة</h3>
        <div class="form-grid">
          <mat-form-field appearance="outline">
            <mat-label>السائق</mat-label>
            <mat-select [(ngModel)]="session.driverId" required>
              <mat-option *ngFor="let d of drivers" [value]="d.driverId">{{ d.fullName }}</mat-option>
            </mat-select>
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>التاريخ</mat-label>
            <input matInput [matDatepicker]="dp" [(ngModel)]="session.sessionDate" required>
            <mat-datepicker-toggle matSuffix [for]="dp"></mat-datepicker-toggle>
            <mat-datepicker #dp></mat-datepicker>
          </mat-form-field>
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>اسم الخط / المنطقة</mat-label>
            <input matInput [(ngModel)]="session.routeArea" placeholder="مثال: المنشية" required>
          </mat-form-field>
        </div>

        <mat-card class="range-card">
          <h4>نطاق أرقام الإيصالات</h4>
          <p class="range-hint">أدخل أول وآخر رقم إيصال. النظام سيرتب التجار تلقائياً بالتسلسل.</p>
          <div class="range-inputs">
            <mat-form-field appearance="outline">
              <mat-label>من رقم</mat-label>
              <input matInput type="number" [(ngModel)]="session.startReceiptNumber" (ngModelChange)="validateRange()">
            </mat-form-field>
            <span class="range-arrow">←</span>
            <mat-form-field appearance="outline">
              <mat-label>إلى رقم</mat-label>
              <input matInput type="number" [(ngModel)]="session.endReceiptNumber" (ngModelChange)="validateRange()">
            </mat-form-field>
          </div>
          <div class="validation-ok" *ngIf="rangeValid">
            ✅ النطاق يتطابق: {{ rangeSize }} إيصال = {{ preview?.rowCount }} تاجر
          </div>
          <div class="validation-err" *ngIf="rangeEntered && !rangeValid">
            ❌ عدد الإيصالات ({{ rangeSize }}) لا يساوي عدد التجار ({{ preview?.rowCount }})
          </div>
        </mat-card>

        <mat-form-field appearance="outline" class="full-width" style="margin-top:12px">
          <mat-label>ملاحظات (اختياري)</mat-label>
          <input matInput [(ngModel)]="session.notes">
        </mat-form-field>

        <div class="step-actions">
          <button mat-stroked-button (click)="currentStep = 2" style="border-radius:10px !important">رجوع</button>
          <button mat-raised-button color="primary" (click)="currentStep = 4"
                  [disabled]="!rangeValid || !session.driverId || !session.routeArea"
                  style="border-radius:10px !important">
            معاينة نهائية
          </button>
        </div>
      </mat-card>

      <!-- STEP 4: Confirm & Save -->
      <mat-card class="step-card" *ngIf="currentStep === 4">
        <h3 style="margin:0 0 16px;font-weight:700">مراجعة نهائية قبل الحفظ</h3>
        <div class="summary-grid">
          <div class="summary-item"><span class="label">السائق:</span><span>{{ selectedDriverName }}</span></div>
          <div class="summary-item"><span class="label">التاريخ:</span><span>{{ session.sessionDate | date:'dd/MM/yyyy' }}</span></div>
          <div class="summary-item"><span class="label">الخط:</span><span>{{ session.routeArea }}</span></div>
          <div class="summary-item"><span class="label">النطاق:</span><span>{{ session.startReceiptNumber }} — {{ session.endReceiptNumber }}</span></div>
          <div class="summary-item"><span class="label">عدد التجار:</span><span>{{ preview?.rowCount }}</span></div>
          <div class="summary-item"><span class="label">الإجمالي:</span><span class="total-val">{{ preview?.totalAmount | number:'1.2-2' }} جنيه</span></div>
        </div>

        <h4 style="margin:16px 0 8px">ترتيب الإيصالات:</h4>
        <div class="assignment-table-wrap">
          <table class="assignment-table">
            <thead><tr><th>رقم الإيصال</th><th>اسم التاجر</th><th>المبلغ</th></tr></thead>
            <tbody>
              <tr *ngFor="let row of preview?.rows; let i = index"
                  [class.warning-row]="row.isWithoutReceipt">
                <td>{{ (session.startReceiptNumber || 0) + i }}</td>
                <td>{{ row.isNewMerchant ? (row.newMerchantName || row.merchantNameRaw) : row.matchedMerchantName }}</td>
                <td>{{ row.amount | number:'1.2-2' }}</td>
              </tr>
            </tbody>
            <tfoot><tr><td colspan="2"><strong>الإجمالي</strong></td><td><strong>{{ preview?.totalAmount | number:'1.2-2' }}</strong></td></tr></tfoot>
          </table>
        </div>

        <div class="step-actions">
          <button mat-stroked-button (click)="currentStep = 3" style="border-radius:10px !important">تعديل</button>
          <button mat-raised-button color="primary" (click)="saveSession()" [disabled]="saving"
                  style="border-radius:10px !important">
            <mat-spinner *ngIf="saving" diameter="20" style="display:inline-block;margin-left:8px"></mat-spinner>
            {{ saving ? 'جاري الحفظ...' : 'حفظ الجلسة نهائياً' }}
          </button>
        </div>
      </mat-card>

      <!-- SUCCESS result -->
      <mat-card class="step-card success-card" *ngIf="currentStep === 5 && saveResult">
        <mat-icon class="big-icon" *ngIf="!saveResult.hasGaps" style="color:#22c55e">check_circle</mat-icon>
        <mat-icon class="big-icon" *ngIf="saveResult.hasGaps" style="color:#dc2626">warning</mat-icon>
        <h2 *ngIf="!saveResult.hasGaps">تم حفظ الجلسة بنجاح ✅</h2>
        <h2 *ngIf="saveResult.hasGaps" style="color:#dc2626">تحذير: تم اكتشاف إيصالات مفقودة!</h2>
        <p>تم إنشاء {{ saveResult.receiptsCreated }} إيصال</p>
        <p *ngIf="saveResult.newMerchantsCreated > 0">تم إنشاء {{ saveResult.newMerchantsCreated }} تاجر جديد</p>
        <div *ngIf="saveResult.hasGaps" class="gap-alert">
          <p>الإيصالات المفقودة: {{ saveResult.detectedGaps.join(', ') }}</p>
        </div>
        <button mat-raised-button color="primary" (click)="resetAll()" style="border-radius:10px !important;margin-top:16px">
          رفع ملف جديد
        </button>
      </mat-card>
    </div>
  `,
  styles: [`
    .steps-bar { display:flex; gap:4px; margin-bottom:20px; flex-wrap:wrap; }
    .step-item { display:flex; align-items:center; gap:6px; padding:8px 16px; border-radius:20px; background:#f3f4f6; font-size:0.85rem; transition:all 0.3s; }
    .step-item.active { background:linear-gradient(135deg,#667eea,#764ba2); color:#fff; font-weight:700; }
    .step-item.done { background:#dcfce7; color:#166534; }
    .step-num { display:inline-flex; align-items:center; justify-content:center; width:24px; height:24px; border-radius:50%; background:rgba(0,0,0,0.1); font-weight:700; font-size:0.8rem; }
    .step-card { border-radius:16px !important; padding:24px !important; margin-bottom:16px; }
    .upload-zone { border:2px dashed #667eea; border-radius:16px; padding:48px; text-align:center; cursor:pointer; transition:all 0.3s; background:#f8faff; }
    .upload-zone:hover { background:#eef2ff; border-color:#4f46e5; }
    .upload-icon { font-size:64px !important; width:64px !important; height:64px !important; color:#667eea; }
    .upload-title { font-size:1.1rem; font-weight:700; margin:16px 0 4px; color:#1a1a2e; }
    .upload-hint { font-size:0.85rem; color:#6b7280; }
    .preview-header { display:flex; align-items:center; gap:12px; margin-bottom:16px; flex-wrap:wrap; }
    .total-badge { background:#dcfce7; color:#166534; padding:4px 12px; border-radius:10px; font-weight:700; font-size:0.9rem; }
    .warning-card { background:#fff7ed !important; border:1px solid #f97316 !important; border-radius:12px !important; padding:12px 16px !important; margin-bottom:12px; display:flex; align-items:center; gap:8px; }
    .danger-card { background:#fef2f2 !important; border:1px solid #dc2626 !important; border-radius:12px !important; padding:12px 16px !important; margin-bottom:12px; display:flex; align-items:center; gap:8px; }
    .match-badge { font-size:0.72rem; padding:2px 8px; border-radius:8px; margin-right:6px; }
    .match-badge.existing { background:#dcfce7; color:#166534; }
    .match-badge.new-merchant { background:#fff7ed; color:#9a3412; }
    .match-badge.danger { background:#fef2f2; color:#dc2626; }
    .inline-input { border:1px solid #d1d5db; border-radius:6px; padding:4px 8px; font-family:Cairo,sans-serif; width:140px; }
    .new-row { background:#fffbeb !important; }
    .warning-row { background:#fef2f2 !important; }
    .form-grid { display:grid; grid-template-columns:1fr 1fr; gap:12px; }
    @media(max-width:768px) { .form-grid { grid-template-columns:1fr; } }
    .full-width { grid-column:1/-1; }
    .range-card { background:#f0f4ff !important; border:2px dashed #667eea !important; border-radius:12px !important; padding:16px !important; margin-top:12px; }
    .range-card h4 { margin:0 0 8px; font-weight:700; color:#1a1a2e; }
    .range-hint { font-size:0.85rem; color:#6b7280; margin:0 0 12px; }
    .range-inputs { display:flex; align-items:center; gap:12px; }
    .range-arrow { font-size:1.5rem; color:#667eea; font-weight:700; }
    .validation-ok { margin-top:8px; padding:8px 12px; background:#dcfce7; border-radius:8px; color:#166534; font-size:0.9rem; }
    .validation-err { margin-top:8px; padding:8px 12px; background:#fef2f2; border-radius:8px; color:#dc2626; font-size:0.9rem; }
    .summary-grid { display:grid; grid-template-columns:1fr 1fr; gap:8px; }
    .summary-item { display:flex; gap:8px; padding:8px 12px; background:#f8f9fa; border-radius:8px; }
    .summary-item .label { font-weight:700; color:#667eea; }
    .total-val { font-weight:700; color:#22c55e; font-size:1.1rem; }
    .assignment-table-wrap { overflow-x:auto; max-height:400px; overflow-y:auto; }
    .assignment-table { width:100%; border-collapse:collapse; font-size:0.9rem; }
    .assignment-table th { background:#667eea; color:#fff; padding:8px 12px; text-align:right; }
    .assignment-table td { padding:8px 12px; border-bottom:1px solid #f3f4f6; }
    .assignment-table tfoot td { background:#f8f9fa; font-weight:700; }
    .step-actions { display:flex; gap:12px; justify-content:flex-end; margin-top:20px; }
    .success-card { text-align:center; }
    .big-icon { font-size:72px !important; width:72px !important; height:72px !important; }
    .gap-alert { background:#fef2f2; border:1px solid #dc2626; border-radius:12px; padding:12px; margin-top:12px; color:#dc2626; font-weight:700; }
    .desktop-table { display:block; } .mobile-receipt-cards { display:none; }
    @media(max-width:768px) { .desktop-table { display:none; } .mobile-receipt-cards { display:block; } }
    .mobile-card { margin-bottom:8px; padding:12px !important; border-radius:10px !important; }
    .mobile-row { margin-bottom:4px; }
  `]
})
export class ExcelImportComponent {
  steps = ['رفع الملف', 'مراجعة البيانات', 'تفاصيل الجلسة', 'تأكيد وحفظ'];
  currentStep = 1;
  uploading = false;
  saving = false;
  preview: any = null;
  saveResult: any = null;
  drivers: any[] = [];
  previewCols = ['rowIndex', 'merchantName', 'correctedName', 'amount', 'receipt'];

  session = {
    driverId: null as number | null,
    sessionDate: new Date(),
    routeArea: '',
    startReceiptNumber: null as number | null,
    endReceiptNumber: null as number | null,
    notes: ''
  };

  rangeValid = false;
  rangeEntered = false;
  rangeSize = 0;

  constructor(private api: ApiService, private snack: MatSnackBar) {}

  ngOnInit() { this.api.getDrivers().subscribe(d => this.drivers = d); }

  get newMerchantsCount() { return this.preview?.rows?.filter((r: any) => r.isNewMerchant).length || 0; }
  get selectedDriverName() { return this.drivers.find(d => d.driverId === this.session.driverId)?.fullName || ''; }

  onFileSelected(event: any) {
    const file = event.target.files[0];
    if (file) this.uploadFile(file);
  }

  onDrop(event: DragEvent) {
    event.preventDefault();
    const file = event.dataTransfer?.files[0];
    if (file) this.uploadFile(file);
  }

  uploadFile(file: File) {
    if (!file.name.endsWith('.xlsx')) {
      this.snack.open('صيغة الملف غير صحيحة — يجب أن يكون .xlsx', 'إغلاق', { duration: 3000 });
      return;
    }
    if (file.size > 5 * 1024 * 1024) {
      this.snack.open('حجم الملف كبير جداً — الحد الأقصى 5 ميجابايت', 'إغلاق', { duration: 3000 });
      return;
    }
    this.uploading = true;
    this.session.notes = '';
    (this.api as any).previewExcel(file).subscribe({
      next: (res: any) => {
        this.uploading = false;
        if (res.success) {
          this.preview = res;
          this.preview.originalFileName = file.name;
          this.currentStep = 2;
        } else {
          this.snack.open(res.error || 'خطأ في قراءة الملف', 'إغلاق', { duration: 5000 });
        }
      },
      error: (err: any) => {
        this.uploading = false;
        this.snack.open(err.error?.error || 'خطأ في رفع الملف', 'إغلاق', { duration: 5000 });
      }
    });
  }

  validateRange() {
    const s = this.session.startReceiptNumber;
    const e = this.session.endReceiptNumber;
    this.rangeEntered = s != null && e != null && s > 0 && e > 0;
    if (this.rangeEntered) {
      this.rangeSize = e! - s! + 1;
      this.rangeValid = this.rangeSize === this.preview?.rowCount;
    } else {
      this.rangeValid = false;
      this.rangeSize = 0;
    }
  }

  saveSession() {
    if (!this.preview || !this.rangeValid) return;
    this.saving = true;
    const body = {
      driverId: this.session.driverId,
      sessionDate: this.session.sessionDate,
      routeArea: this.session.routeArea,
      startReceiptNumber: this.session.startReceiptNumber,
      endReceiptNumber: this.session.endReceiptNumber,
      originalFileName: this.preview.originalFileName,
      notes: this.session.notes || null,
      rows: this.preview.rows
    };
    (this.api as any).saveExcelSession(body).subscribe({
      next: (res: any) => {
        this.saving = false;
        if (res.success) {
          this.saveResult = res;
          this.currentStep = 5;
        } else {
          this.snack.open(res.error || 'حدث خطأ أثناء الحفظ', 'إغلاق', { duration: 5000 });
        }
      },
      error: (err: any) => {
        this.saving = false;
        this.snack.open(err.error?.error || 'حدث خطأ أثناء الحفظ', 'إغلاق', { duration: 5000 });
      }
    });
  }

  resetUpload() { this.preview = null; this.currentStep = 1; }
  resetAll() { this.preview = null; this.saveResult = null; this.currentStep = 1; this.session = { driverId: null, sessionDate: new Date(), routeArea: '', startReceiptNumber: null, endReceiptNumber: null, notes: '' }; this.rangeValid = false; this.rangeEntered = false; }
}
