import { Component } from '@angular/core';
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
import { MatExpansionModule } from '@angular/material/expansion';
import { MatStepperModule } from '@angular/material/stepper';
import { ApiService } from '../../core/api.service';

@Component({
  selector: 'app-ajal-excel-import',
  standalone: true,
  imports: [CommonModule, FormsModule, MatCardModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatIconModule,
    MatSelectModule, MatChipsModule, MatSnackBarModule, MatProgressSpinnerModule, MatDatepickerModule, MatNativeDateModule,
    MatExpansionModule, MatStepperModule],
  template: `
    <div class="page-container">
      <div class="page-header"><h1>📤 رفع Excel — دفتر الآجل</h1></div>

      <!-- AI Prompt -->
      <mat-expansion-panel class="prompt-panel">
        <mat-expansion-panel-header><mat-panel-title>📋 البرومت اليومي لـ ChatGPT / Gemini</mat-panel-title></mat-expansion-panel-header>
        <pre class="ai-prompt">{{ aiPrompt }}</pre>
        <button mat-stroked-button (click)="copyPrompt()" style="border-radius:10px;margin-top:8px">📋 نسخ البرومت</button>
      </mat-expansion-panel>

      <!-- Step 1: Upload -->
      <mat-card class="step-card" *ngIf="step===1">
        <h3>الخطوة 1: رفع الملف</h3>
        <div class="drop-zone" (dragover)="$event.preventDefault()" (drop)="onDrop($event)"
          (click)="fileInput.click()">
          <mat-icon style="font-size:48px;width:48px;height:48px;color:#667eea">cloud_upload</mat-icon>
          <p>اسحب ملف Excel هنا أو اضغط للاختيار</p>
          <input #fileInput type="file" accept=".xlsx" (change)="onFileSelect($event)" hidden>
        </div>
        <div *ngIf="uploading" style="text-align:center;padding:16px"><mat-spinner diameter="40"></mat-spinner></div>
      </mat-card>

      <!-- Step 2: Preview -->
      <mat-card class="step-card" *ngIf="step===2 && preview">
        <h3>الخطوة 2: معاينة البيانات</h3>
        <div class="preview-stats">
          <span class="stat-chip">{{ preview.rowCount }} فاتورة</span>
          <span class="stat-chip primary">{{ preview.totalAmount | number:'1.2-2' }} جنيه</span>
          <span class="stat-chip warn" *ngIf="preview.duplicateCount > 0">⚠️ {{ preview.duplicateCount }} مكررة</span>
        </div>
        <div class="warnings" *ngIf="preview.warnings?.length">
          <div *ngFor="let w of preview.warnings" class="warn-item">⚠️ {{ w }}</div>
        </div>
        <div style="overflow-x:auto;margin-top:12px">
          <table class="preview-table">
            <thead><tr><th>#</th><th>رقم الفاتورة</th><th>التاجر</th><th>المبلغ</th><th>الحالة</th></tr></thead>
            <tbody>
              <tr *ngFor="let r of preview.rows" [class.row-dup]="r.isDuplicate" [class.row-new]="r.isNewMerchant">
                <td>{{ r.rowIndex }}</td><td>{{ r.invoiceNumber }}</td>
                <td>{{ r.matchedMerchantName || r.merchantNameRaw }} <span class="new-badge" *ngIf="r.isNewMerchant">جديد</span></td>
                <td>{{ r.amount | number:'1.2-2' }}</td>
                <td>
                  <span *ngIf="r.isDuplicate" class="dup-badge">مكرر</span>
                  <span *ngIf="!r.isDuplicate && !r.isNewMerchant">✅</span>
                  <span *ngIf="r.isNewMerchant && !r.isDuplicate" class="new-badge">تاجر جديد</span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
        <div class="step-actions">
          <button mat-button (click)="step=1">رجوع</button>
          <button mat-raised-button color="primary" (click)="step=3" style="border-radius:10px">التالي</button>
        </div>
      </mat-card>

      <!-- Step 3: Session Details -->
      <mat-card class="step-card" *ngIf="step===3">
        <h3>الخطوة 3: تفاصيل الجلسة</h3>
        <div class="session-fields">
          <mat-form-field appearance="outline">
            <mat-label>تاريخ اليومية</mat-label>
            <input matInput [matDatepicker]="dp" [(ngModel)]="sessionDate">
            <mat-datepicker-toggle matSuffix [for]="dp"></mat-datepicker-toggle>
            <mat-datepicker #dp></mat-datepicker>
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>الخط</mat-label>
            <mat-select [(ngModel)]="sessionRouteId">
              <mat-option [value]="null">— بدون خط —</mat-option>
              <mat-option *ngFor="let r of routes" [value]="r.routeId">{{ r.routeName }}</mat-option>
            </mat-select>
          </mat-form-field>
        </div>
        <div class="step-actions">
          <button mat-button (click)="step=2">رجوع</button>
          <button mat-raised-button color="primary" (click)="saveExcel()" [disabled]="saving" style="border-radius:10px">
            <mat-spinner *ngIf="saving" diameter="18" style="display:inline-block;margin-left:8px"></mat-spinner>
            {{ saving ? 'جاري الحفظ...' : 'حفظ الفواتير' }}
          </button>
        </div>
      </mat-card>

      <!-- Step 4: Done -->
      <mat-card class="step-card done-card" *ngIf="step===4">
        <mat-icon style="font-size:64px;width:64px;height:64px;color:#22c55e">check_circle</mat-icon>
        <h3>تم الاستيراد بنجاح!</h3>
        <p>{{ result?.saved }} فاتورة تم حفظها</p>
        <p *ngIf="result?.skipped">{{ result.skipped }} فاتورة مكررة تم تجاهلها</p>
        <p *ngIf="result?.newMerchantsCreated">{{ result.newMerchantsCreated }} تاجر جديد تم إنشاؤه</p>
        <button mat-raised-button color="primary" (click)="reset()" style="border-radius:10px;margin-top:16px">رفع ملف آخر</button>
      </mat-card>
    </div>
  `,
  styles: [`
    .step-card { border-radius:16px !important; padding:24px !important; margin-bottom:16px; }
    .prompt-panel { border-radius:16px !important; margin-bottom:16px; }
    .ai-prompt { background:#1e1e2e; color:#a6e3a1; padding:16px; border-radius:10px; font-size:0.85rem; white-space:pre-wrap; direction:rtl; line-height:1.8; }
    .drop-zone { border:2px dashed #d1d5db; border-radius:16px; padding:48px; text-align:center; cursor:pointer; transition:all 0.3s; }
    .drop-zone:hover { border-color:#667eea; background:#f0f4ff; }
    .preview-stats { display:flex; gap:8px; flex-wrap:wrap; margin-bottom:12px; }
    .stat-chip { padding:4px 14px; border-radius:12px; background:#f3f4f6; font-weight:600; }
    .stat-chip.primary { background:#dcfce7; color:#166534; }
    .stat-chip.warn { background:#fef2f2; color:#dc2626; }
    .preview-table { width:100%; border-collapse:collapse; }
    .preview-table th { background:#f8f9fa; padding:8px; text-align:right; }
    .preview-table td { padding:6px 8px; border-bottom:1px solid #f3f4f6; }
    .row-dup { background:#fef2f2; opacity:0.6; }
    .row-new { background:#fffbeb; }
    .dup-badge { background:#fecaca; color:#dc2626; padding:2px 8px; border-radius:6px; font-size:0.75rem; }
    .new-badge { background:#fef3c7; color:#92400e; padding:2px 8px; border-radius:6px; font-size:0.75rem; }
    .warnings { background:#fffbeb; border-radius:10px; padding:12px; margin-bottom:12px; }
    .warn-item { padding:4px 0; font-size:0.85rem; color:#92400e; }
    .session-fields { display:flex; gap:16px; flex-wrap:wrap; }
    .step-actions { display:flex; gap:8px; justify-content:flex-end; margin-top:16px; }
    .done-card { text-align:center; }
  `]
})
export class AjalExcelImportComponent {
  step = 1;
  uploading = false; saving = false;
  preview: any = null; result: any = null;
  sessionDate = new Date(); sessionRouteId: number | null = null;
  routes: any[] = [];

  aiPrompt = `أنا أرسل لك صور من فواتير مبيعات مطبوعة.
استخرج البيانات التالية من كل فاتورة:

1. رقم الفاتورة — الرقم المطبوع في أعلى الفاتورة (مثال: 441485)
2. اسم العميل — مكتوب بخط اليد في أعلى الفاتورة
3. إجمالي الفاتورة — آخر رقم في جدول الأسعار (الإجمالي)
4. تاريخ الفاتورة — إن وجد

النتيجة: ملف Excel بهذه الأعمدة فقط:
| رقم الفاتورة | اسم العميل | الإجمالي | التاريخ |

قواعد مهمة:
- رقم الفاتورة: 6 أرقام كاملة بدون مسافات
- الإجمالي: رقم فقط بدون رموز
- اسم العميل: كما هو مكتوب بالعربي بدون تعديل
- رتب الصفوف بترتيب أرقام الفواتير من الأصغر للأكبر
- لا تضيف أعمدة إضافية`;

  constructor(private api: ApiService, private snack: MatSnackBar) {
    this.api.getRoutes().subscribe((r: any) => this.routes = r);
  }

  copyPrompt() { navigator.clipboard.writeText(this.aiPrompt); this.snack.open('تم نسخ البرومت!', '', { duration: 1500 }); }

  onDrop(e: DragEvent) { e.preventDefault(); const f = e.dataTransfer?.files?.[0]; if (f) this.upload(f); }
  onFileSelect(e: any) { const f = e.target.files?.[0]; if (f) this.upload(f); }

  upload(file: File) {
    if (!file.name.endsWith('.xlsx')) { this.snack.open('يجب رفع ملف .xlsx', 'إغلاق', { duration: 3000 }); return; }
    this.uploading = true;
    this.api.previewAjalExcel(file).subscribe({
      next: (res: any) => { this.uploading = false; if (res.success) { this.preview = res; this.step = 2; } else { this.snack.open(res.error || 'خطأ', 'إغلاق', { duration: 4000 }); } },
      error: () => { this.uploading = false; this.snack.open('خطأ في رفع الملف', 'إغلاق', { duration: 3000 }); }
    });
  }

  saveExcel() {
    this.saving = true;
    const data = {
      sessionDate: this.sessionDate.toISOString(), routeId: this.sessionRouteId,
      rows: this.preview.rows.map((r: any) => ({
        invoiceNumber: r.invoiceNumber, merchantId: r.matchedMerchantId,
        isNewMerchant: r.isNewMerchant, newMerchantName: r.merchantNameRaw,
        amount: r.amount, callCenterEmployeeName: r.callCenterEmployeeName
      }))
    };
    this.api.saveAjalExcel(data).subscribe({
      next: (res: any) => { this.saving = false; this.result = res; this.step = 4; },
      error: () => { this.saving = false; this.snack.open('خطأ في الحفظ', 'إغلاق', { duration: 3000 }); }
    });
  }

  reset() { this.step = 1; this.preview = null; this.result = null; }
}
