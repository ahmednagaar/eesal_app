import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { ApiService } from '../../core/api.service';

@Component({
  selector: 'app-ajal-employees',
  standalone: true,
  imports: [CommonModule, FormsModule, MatCardModule, MatFormFieldModule, MatInputModule, MatButtonModule,
    MatIconModule, MatButtonToggleModule, MatSnackBarModule, MatProgressSpinnerModule, MatDatepickerModule, MatNativeDateModule],
  template: `
    <div class="page-container">
      <div class="page-header"><h1>📊 أداء الموظفين — دفتر الآجل</h1></div>

      <!-- Period Toggle -->
      <div class="period-bar">
        <mat-button-toggle-group [(ngModel)]="period" (change)="onPeriodChange()">
          <mat-button-toggle value="today">اليوم</mat-button-toggle>
          <mat-button-toggle value="week">هذا الأسبوع</mat-button-toggle>
          <mat-button-toggle value="month">هذا الشهر</mat-button-toggle>
          <mat-button-toggle value="custom">تاريخ محدد</mat-button-toggle>
        </mat-button-toggle-group>
        <div *ngIf="period==='custom'" class="custom-range">
          <mat-form-field appearance="outline">
            <mat-label>من</mat-label>
            <input matInput [matDatepicker]="dp1" [(ngModel)]="customFrom">
            <mat-datepicker-toggle matSuffix [for]="dp1"></mat-datepicker-toggle>
            <mat-datepicker #dp1></mat-datepicker>
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>إلى</mat-label>
            <input matInput [matDatepicker]="dp2" [(ngModel)]="customTo">
            <mat-datepicker-toggle matSuffix [for]="dp2"></mat-datepicker-toggle>
            <mat-datepicker #dp2></mat-datepicker>
          </mat-form-field>
          <button mat-raised-button color="primary" (click)="load()" style="border-radius:10px">تحميل</button>
        </div>
      </div>

      <div *ngIf="loading" style="text-align:center;padding:32px"><mat-spinner diameter="40"></mat-spinner></div>

      <!-- Grand Totals -->
      <div class="grand-totals" *ngIf="data && !loading">
        <span>إجمالي: <strong>{{ data.grandTotalInvoices }}</strong> فاتورة</span>
        <span><strong>{{ data.grandTotalAmount | number:'1.2-2' }}</strong> جنيه</span>
        <span class="spacer"></span>
        <button mat-stroked-button (click)="exportExcel()" style="border-radius:10px">📥 Excel</button>
      </div>

      <!-- Employee Cards -->
      <div class="employee-grid" *ngIf="data && !loading">
        <mat-card *ngFor="let emp of data.employees; let i = index" class="emp-card" [class.top-card]="i===0">
          <mat-card-header>
            <mat-card-title>{{ emp.employeeName }}</mat-card-title>
            <mat-card-subtitle *ngIf="i===0">🏆 الأعلى مبيعاً</mat-card-subtitle>
          </mat-card-header>
          <mat-card-content>
            <div class="emp-stats">
              <div class="emp-stat"><span>عدد الفواتير</span><strong>{{ emp.invoiceCount }}</strong></div>
              <div class="emp-stat"><span>إجمالي المبيعات</span><strong class="big-num">{{ emp.totalAmount | number:'1.2-2' }} جنيه</strong></div>
              <div class="emp-stat"><span>متوسط الفاتورة</span><strong>{{ emp.averageInvoice | number:'1.2-2' }} جنيه</strong></div>
            </div>
          </mat-card-content>
          <mat-card-actions>
            <button mat-button (click)="emp.showDetail=!emp.showDetail">{{ emp.showDetail ? 'إخفاء' : 'عرض' }} التفاصيل اليومية</button>
          </mat-card-actions>
          <div class="daily-detail" *ngIf="emp.showDetail">
            <table>
              <tr *ngFor="let d of emp.dailyBreakdown">
                <td>{{ d.date | date:'dd/MM' }}</td><td>{{ d.count }} فاتورة</td><td>{{ d.amount | number:'1.2-2' }} جنيه</td>
              </tr>
            </table>
          </div>
        </mat-card>
      </div>

      <!-- Empty state -->
      <div *ngIf="data && data.employees?.length === 0 && !loading" style="text-align:center;padding:48px">
        <mat-icon style="font-size:64px;width:64px;height:64px;color:#ccc">people</mat-icon>
        <p>لا توجد بيانات للفترة المحددة</p>
      </div>
    </div>
  `,
  styles: [`
    .period-bar { display:flex; align-items:center; gap:12px; flex-wrap:wrap; margin-bottom:16px; }
    .custom-range { display:flex; gap:8px; align-items:center; flex-wrap:wrap; }
    .grand-totals { display:flex; align-items:center; gap:16px; flex-wrap:wrap; padding:12px 16px; background:#f8f9fa; border-radius:12px; margin-bottom:16px; font-size:1rem; }
    .spacer { flex:1; }
    .employee-grid { display:grid; grid-template-columns:repeat(auto-fill,minmax(320px,1fr)); gap:16px; }
    .emp-card { border-radius:16px !important; overflow:hidden; }
    .top-card { border:2px solid #f59e0b; background:linear-gradient(135deg,#fffbeb,#fef3c7); }
    .emp-stats { display:flex; gap:16px; flex-wrap:wrap; margin-top:12px; }
    .emp-stat { text-align:center; flex:1; min-width:80px; }
    .emp-stat span { display:block; font-size:0.8rem; color:#6b7280; }
    .emp-stat strong { font-size:1rem; color:#1e293b; }
    .big-num { color:#22c55e !important; font-size:1.15rem !important; }
    .daily-detail { padding:12px; background:#f8f9fa; border-radius:0 0 12px 12px; }
    .daily-detail table { width:100%; border-collapse:collapse; }
    .daily-detail td { padding:6px 8px; border-bottom:1px solid #e5e7eb; font-size:0.9rem; }
    @media(max-width:600px) { .employee-grid { grid-template-columns:1fr; } }
  `]
})
export class AjalEmployeesComponent implements OnInit {
  period = 'month';
  customFrom = new Date(); customTo = new Date();
  data: any = null; loading = false;

  constructor(private api: ApiService, private snack: MatSnackBar) {}
  ngOnInit() { this.onPeriodChange(); }

  onPeriodChange() {
    if (this.period !== 'custom') this.load();
  }

  load() {
    const { from, to } = this.getRange();
    this.loading = true;
    this.api.getAjalEmployeePerformance(from, to).subscribe({
      next: (d: any) => { this.data = d; this.data.employees?.forEach((e: any) => e.showDetail = false); this.loading = false; },
      error: () => { this.loading = false; this.snack.open('خطأ في تحميل البيانات', 'إغلاق', { duration: 3000 }); }
    });
  }

  exportExcel() {
    const { from, to } = this.getRange();
    this.api.exportAjalEmployees(from, to).subscribe(blob => {
      const url = URL.createObjectURL(blob); const a = document.createElement('a');
      a.href = url; a.download = `ajal_employees_${from}_${to}.xlsx`; a.click(); URL.revokeObjectURL(url);
    });
  }

  private getRange(): { from: string; to: string } {
    const today = new Date();
    let from: Date, to: Date;
    switch (this.period) {
      case 'today': from = to = today; break;
      case 'week': from = new Date(today); from.setDate(from.getDate() - from.getDay()); to = today; break;
      case 'month': from = new Date(today.getFullYear(), today.getMonth(), 1); to = today; break;
      default: from = this.customFrom; to = this.customTo; break;
    }
    return { from: from.toISOString().split('T')[0], to: to.toISOString().split('T')[0] };
  }
}
