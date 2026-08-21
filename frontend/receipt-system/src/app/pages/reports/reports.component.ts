import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ApiService } from '../../core/api.service';

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [
    CommonModule, FormsModule, MatCardModule, MatFormFieldModule, MatInputModule,
    MatButtonModule, MatIconModule, MatDatepickerModule, MatNativeDateModule,
    MatProgressSpinnerModule
  ],
  template: `
    <div class="page-container">
      <div class="page-header no-print">
        <h1>التقرير اليومي</h1>
        <div style="display:flex; gap:8px; align-items:center; flex-wrap:wrap">
          <mat-form-field appearance="outline" style="width:200px">
            <mat-label>اختر التاريخ</mat-label>
            <input matInput [matDatepicker]="rp" [(ngModel)]="reportDate">
            <mat-datepicker-toggle matSuffix [for]="rp"></mat-datepicker-toggle>
            <mat-datepicker #rp></mat-datepicker>
          </mat-form-field>
          <button mat-raised-button color="primary" (click)="loadReport()" style="border-radius:12px !important">
            <mat-icon>search</mat-icon> عرض التقرير
          </button>
          <button mat-raised-button (click)="printReport()" *ngIf="report" style="border-radius:12px !important">
            <mat-icon>print</mat-icon> طباعة
          </button>
        </div>
      </div>

      <div class="loading-container" *ngIf="loading"><mat-spinner diameter="40"></mat-spinner></div>

      <!-- Print Report -->
      <div class="print-report" *ngIf="report && !loading" id="printable-report">
        <div class="report-header">
          <h2>شركه البسطاوي للتجاره والتوزيع</h2>
          <h3>تقرير التحصيل اليومي</h3>
          <div class="report-meta">
            <span>التاريخ: {{ report.reportDate | date:'yyyy/MM/dd' }}</span>
            <span>إعداد: {{ report.preparedBy }}</span>
          </div>
        </div>

        <div class="report-summary">
          <h4>الملخص العام</h4>
          <table class="summary-table">
            <tr><td>إجمالي السائقين اليوم:</td><td><strong>{{ report.totalDrivers }}</strong></td></tr>
            <tr><td>إجمالي الإيصالات:</td><td><strong>{{ report.totalReceipts }}</strong></td></tr>
            <tr><td>إجمالي المبلغ المحصّل:</td><td><strong>{{ report.totalAmount | number:'1.2-2' }} جنيه</strong></td></tr>
            <tr><td>إيصالات مفقودة:</td><td><strong class="missing">{{ report.missingReceiptsCount }}</strong></td></tr>
          </table>
        </div>

        <div *ngFor="let dr of report.driverReports; let last = last" class="driver-section">
          <div class="driver-header">
            <span>السائق: <strong>{{ dr.driverName }}</strong></span>
            <span>المنطقة: {{ dr.routeArea }}</span>
          </div>
          <table class="receipts-print-table">
            <thead>
              <tr><th>رقم الإيصال</th><th>التاجر</th><th>المبلغ</th></tr>
            </thead>
            <tbody>
              <tr *ngFor="let r of dr.receipts">
                <td>{{ r.receiptNumber }}</td>
                <td>{{ r.merchantName }}</td>
                <td>{{ r.amount | number:'1.2-2' }}</td>
              </tr>
            </tbody>
            <tfoot>
              <tr>
                <td colspan="2">إجمالي السائق: {{ dr.receiptCount }} إيصال</td>
                <td><strong>{{ dr.totalAmount | number:'1.2-2' }} جنيه</strong></td>
              </tr>
            </tfoot>
          </table>
          <div class="missing-alert" *ngIf="dr.missingReceipts.length > 0">
            ⚠️ إيصالات مفقودة: {{ dr.missingReceipts.join('، ') }}
          </div>
          <hr *ngIf="!last" class="page-break">
        </div>

        <div class="report-footer">
          <div class="signature-line">توقيع المسؤول: _________________</div>
          <div class="signature-line">التاريخ: _________________</div>
        </div>
      </div>

      <div *ngIf="!report && !loading" style="text-align:center; padding:48px">
        <mat-icon style="font-size:64px;width:64px;height:64px;color:#d1d5db">print</mat-icon>
        <p style="color:#6b7280">اختر التاريخ ثم اضغط "عرض التقرير"</p>
      </div>
    </div>
  `,
  styles: [`
    .report-header {
      text-align: center; padding: 24px; border-bottom: 3px double #1a1a2e;
      h2 { margin: 0; font-size: 1.4rem; font-weight: 800; }
      h3 { margin: 4px 0 12px; font-size: 1.1rem; font-weight: 600; color: #495057; }
    }
    .report-meta { display: flex; justify-content: space-between; font-size: 0.9rem; color: #6b7280; }
    .report-summary {
      padding: 16px; margin: 16px 0; background: #f8f9fa; border-radius: 8px;
      h4 { margin: 0 0 12px; font-weight: 700; }
    }
    .summary-table { width: 100%; border-collapse: collapse; }
    .summary-table td { padding: 6px 0; font-size: 0.95rem; }
    .summary-table td:last-child { text-align: left; }
    .missing { color: #dc2626; }
    .driver-section { margin: 20px 0; }
    .driver-header {
      display: flex; justify-content: space-between; padding: 10px 16px;
      background: #e8edf3; border-radius: 8px 8px 0 0; font-size: 0.95rem;
    }
    .receipts-print-table {
      width: 100%; border-collapse: collapse;
      th, td { padding: 8px 12px; border: 1px solid #dee2e6; font-size: 0.85rem; text-align: right; }
      th { background: #f1f3f5; font-weight: 700; }
      tfoot td { background: #f8f9fa; font-weight: 600; }
    }
    .missing-alert {
      padding: 8px 16px; background: #fee2e2; color: #dc2626; border-radius: 0 0 8px 8px;
      font-weight: 600; font-size: 0.85rem;
    }
    .report-footer {
      margin-top: 40px; padding-top: 20px; border-top: 3px double #1a1a2e;
      display: flex; justify-content: space-between;
    }
    .signature-line { font-size: 0.9rem; }
  `]
})
export class ReportsComponent {
  reportDate = new Date();
  report: any = null;
  loading = false;

  constructor(private api: ApiService) {}

  loadReport() {
    this.loading = true;
    const dateStr = this.reportDate.toISOString().split('T')[0];
    this.api.getDailyReport(dateStr).subscribe({
      next: (r) => { this.report = r; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  printReport() {
    window.print();
  }
}
