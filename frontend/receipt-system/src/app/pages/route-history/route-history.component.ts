import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ApiService } from '../../core/api.service';

@Component({
  selector: 'app-route-history',
  standalone: true,
  imports: [
    CommonModule, FormsModule, MatCardModule, MatFormFieldModule, MatInputModule,
    MatSelectModule, MatButtonModule, MatIconModule, MatTableModule, MatDatepickerModule,
    MatNativeDateModule, MatProgressSpinnerModule, MatSnackBarModule
  ],
  template: `
    <div class="page-container">
      <div class="page-header no-print">
        <h1>سجل الخطوط</h1>
        <div style="display:flex; gap:8px; align-items:center; flex-wrap:wrap">
          <mat-form-field appearance="outline" style="width:180px">
            <mat-label>الخط</mat-label>
            <mat-select [(ngModel)]="filterRouteId" (selectionChange)="loadHistory()">
              <mat-option [value]="null">الكل</mat-option>
              <mat-option *ngFor="let r of routes" [value]="r.routeId">{{ r.routeName }}</mat-option>
            </mat-select>
          </mat-form-field>
          <mat-form-field appearance="outline" style="width:180px">
            <mat-label>التاريخ</mat-label>
            <input matInput [matDatepicker]="hp" [(ngModel)]="filterDate" (dateChange)="loadHistory()">
            <mat-datepicker-toggle matSuffix [for]="hp"></mat-datepicker-toggle>
            <mat-datepicker #hp></mat-datepicker>
          </mat-form-field>
          <button mat-stroked-button (click)="clearFilters()" style="border-radius:10px !important">
            <mat-icon>clear</mat-icon> مسح
          </button>
        </div>
      </div>

      <div class="loading-container" *ngIf="loading"><mat-spinner diameter="40"></mat-spinner></div>

      <mat-card class="data-table-card" *ngIf="!loading">
        <!-- Desktop Table -->
        <div class="desktop-table" *ngIf="days.length > 0">
          <table mat-table [dataSource]="days" class="full-width">
            <ng-container matColumnDef="date">
              <th mat-header-cell *matHeaderCellDef>التاريخ</th>
              <td mat-cell *matCellDef="let d">{{ d.deliveryDate | date:'dd/MM/yyyy' }}</td>
            </ng-container>
            <ng-container matColumnDef="route">
              <th mat-header-cell *matHeaderCellDef>الخط</th>
              <td mat-cell *matCellDef="let d">{{ d.routeName }}</td>
            </ng-container>
            <ng-container matColumnDef="driver">
              <th mat-header-cell *matHeaderCellDef>السائق</th>
              <td mat-cell *matCellDef="let d">{{ d.assignedDriver || '—' }}</td>
            </ng-container>
            <ng-container matColumnDef="count">
              <th mat-header-cell *matHeaderCellDef>عدد التجار</th>
              <td mat-cell *matCellDef="let d">{{ d.invoiceCount }}</td>
            </ng-container>
            <ng-container matColumnDef="amount">
              <th mat-header-cell *matHeaderCellDef>المبلغ</th>
              <td mat-cell *matCellDef="let d">{{ d.totalAmount | number:'1.2-2' }}</td>
            </ng-container>
            <ng-container matColumnDef="status">
              <th mat-header-cell *matHeaderCellDef>الحالة</th>
              <td mat-cell *matCellDef="let d">
                <span class="status-badge"
                      [class.resolved]="d.status === 'Confirmed'"
                      [class.in-progress]="d.status === 'Draft'"
                      [class.assigned]="d.status === 'Printed'">
                  {{ d.status === 'Confirmed' ? 'مؤكد' : d.status === 'Printed' ? 'مطبوع' : 'مسودة' }}
                </span>
              </td>
            </ng-container>
            <ng-container matColumnDef="actions">
              <th mat-header-cell *matHeaderCellDef></th>
              <td mat-cell *matCellDef="let d">
                <button mat-icon-button (click)="reprintLoading(d)" matTooltip="طباعة تحميل">
                  <mat-icon>inventory_2</mat-icon>
                </button>
                <button mat-icon-button (click)="reprintDelivery(d)" matTooltip="طباعة تسليم">
                  <mat-icon>local_shipping</mat-icon>
                </button>
              </td>
            </ng-container>
            <tr mat-header-row *matHeaderRowDef="columns"></tr>
            <tr mat-row *matRowDef="let row; columns: columns;"></tr>
          </table>
        </div>

        <!-- Mobile Cards -->
        <div class="mobile-receipt-cards" *ngIf="days.length > 0">
          <mat-card *ngFor="let d of days" class="history-card">
            <div class="card-row">
              <strong>{{ d.routeName }}</strong>
              <span class="status-badge"
                    [class.resolved]="d.status === 'Confirmed'"
                    [class.in-progress]="d.status === 'Draft'">
                {{ d.status === 'Confirmed' ? 'مؤكد' : 'مسودة' }}
              </span>
            </div>
            <div class="card-row">
              <span>{{ d.deliveryDate | date:'dd/MM/yyyy' }}</span>
              <span>{{ d.invoiceCount }} تاجر</span>
            </div>
            <div class="card-row" *ngIf="d.assignedDriver">
              <span>السائق: {{ d.assignedDriver }}</span>
            </div>
            <div class="card-row" style="margin-top:8px; gap:6px">
              <button mat-stroked-button (click)="reprintLoading(d)" style="border-radius:8px !important; font-size:0.8rem">
                📦 تحميل
              </button>
              <button mat-stroked-button (click)="reprintDelivery(d)" style="border-radius:8px !important; font-size:0.8rem">
                🚚 تسليم
              </button>
            </div>
          </mat-card>
        </div>

        <div *ngIf="days.length === 0" class="empty-state" style="padding:48px; text-align:center">
          <mat-icon style="font-size:64px;width:64px;height:64px;color:#d1d5db">history</mat-icon>
          <p style="color:#6b7280">لا يوجد سجل خطوط</p>
        </div>
      </mat-card>

      <!-- Print Area -->
      <div class="print-sheet" *ngIf="printSheet">
        <div class="sheet-header">
          <h2>{{ printMode === 'loading' ? 'قائمة التحميل — للعمال فقط' : 'قائمة التسليم — للسائق' }}</h2>
          <div class="sheet-meta">
            <span>الخط: {{ printSheet.routeName }}</span>
            <span>التاريخ: {{ printSheet.deliveryDate | date:'dd/MM/yyyy' }}</span>
          </div>
          <div class="sheet-meta" *ngIf="printSheet.assignedDriver">
            <span>السائق: {{ printSheet.assignedDriver }}</span>
          </div>
          <p class="sheet-warning" *ngIf="printMode === 'loading'">⚠️ يُحمَّل من الأعلى للأسفل — الصف الأول يُحمَّل أولاً</p>
        </div>
        <table class="sheet-table">
          <thead><tr><th>م</th><th>اسم التاجر</th><th>الكمية</th><th>رقم الفاتورة</th><th>✓</th></tr></thead>
          <tbody>
            <tr *ngFor="let l of printSheet.lines">
              <td>{{ l.sequenceNumber }}</td><td>{{ l.merchantName }}</td>
              <td>{{ l.quantity }}</td><td>{{ l.invoiceNumber }}</td><td>☐</td>
            </tr>
          </tbody>
        </table>
        <div class="sheet-footer">
          <span>الإجمالي: {{ printSheet.totalMerchants }} تاجر</span>
          <span>{{ printMode === 'loading' ? 'توقيع رئيس العمال' : 'توقيع السائق' }}: _______________</span>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .history-card {
      margin-bottom: 8px; padding: 12px !important;
      border-radius: 12px !important; border-right: 4px solid #667eea;
    }
    .card-row {
      display: flex; justify-content: space-between; align-items: center;
      padding: 3px 0; font-size: 0.9rem;
    }
    .print-sheet { display: none; }
    .sheet-header { text-align: center; padding: 16px; border-bottom: 3px double #1a1a2e;
      h2 { margin: 0; font-size: 1.3rem; font-weight: 800; }
    }
    .sheet-meta { display: flex; justify-content: space-between; font-size: 0.9rem; color: #495057; margin-top: 8px; }
    .sheet-warning { background: #fef3cd; padding: 8px; border-radius: 6px; font-size: 0.85rem; color: #92400e; margin-top: 12px; }
    .sheet-table { width: 100%; border-collapse: collapse; margin-top: 16px;
      th, td { padding: 8px 12px; border: 1px solid #dee2e6; font-size: 0.88rem; text-align: right; }
      th { background: #f1f3f5; font-weight: 700; }
    }
    .sheet-footer { display: flex; justify-content: space-between; margin-top: 24px; padding-top: 16px;
      border-top: 3px double #1a1a2e; font-size: 0.9rem;
    }
  `]
})
export class RouteHistoryComponent implements OnInit {
  routes: any[] = [];
  days: any[] = [];
  loading = true;
  filterRouteId: number | null = null;
  filterDate: Date | null = null;
  columns = ['date', 'route', 'driver', 'count', 'amount', 'status', 'actions'];
  printSheet: any = null;
  printMode: 'loading' | 'delivery' = 'loading';

  constructor(private api: ApiService, private snack: MatSnackBar) {}

  ngOnInit() {
    this.api.getRoutes().subscribe(r => this.routes = r);
    this.loadHistory();
  }

  loadHistory() {
    this.loading = true;
    const dateStr = this.filterDate ? this.filterDate.toISOString().split('T')[0] : undefined;
    this.api.getDeliveryDays(dateStr, this.filterRouteId || undefined).subscribe({
      next: (d) => { this.days = d; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  clearFilters() {
    this.filterRouteId = null;
    this.filterDate = null;
    this.loadHistory();
  }

  reprintLoading(day: any) { this.reprint(day, 'loading'); }
  reprintDelivery(day: any) { this.reprint(day, 'delivery'); }

  private reprint(day: any, mode: 'loading' | 'delivery') {
    this.printMode = mode;
    const api$ = mode === 'loading'
      ? this.api.getLoadingSheet(day.deliveryDayId)
      : this.api.getDeliverySheet(day.deliveryDayId);

    api$.subscribe({
      next: (sheet) => {
        this.printSheet = sheet;
        setTimeout(() => window.print(), 300);
      },
      error: () => this.snack.open('حدث خطأ', 'إغلاق', { duration: 3000 })
    });
  }
}
