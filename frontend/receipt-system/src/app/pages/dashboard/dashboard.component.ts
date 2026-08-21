import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Router } from '@angular/router';
import { ApiService } from '../../core/api.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, MatCardModule, MatIconModule, MatButtonModule, MatTableModule, MatProgressSpinnerModule],
  template: `
    <div class="page-container">
      <div class="page-header">
        <h1>لوحة التحكم</h1>
      </div>

      <!-- Gap Warning -->
      <div class="gap-warning" *ngIf="dashboard?.openGapsCount > 0">
        <span>⚠️ تحذير: يوجد {{ dashboard.openGapsCount }} إيصال مفقود يحتاج متابعة</span>
        <button mat-button routerLink="/gaps">عرض التفاصيل</button>
      </div>

      <!-- Loading -->
      <div class="loading-container" *ngIf="loading">
        <mat-spinner diameter="40"></mat-spinner>
      </div>

      <ng-container *ngIf="!loading && dashboard">
        <!-- Stat Cards -->
        <div class="stats-grid">
          <div class="stat-card green">
            <mat-icon class="stat-icon">payments</mat-icon>
            <div class="stat-label">إجمالي التحصيل اليوم</div>
            <div class="stat-value">{{ dashboard.totalAmountToday | number:'1.2-2' }} <small>جنيه</small></div>
          </div>
          <div class="stat-card blue">
            <mat-icon class="stat-icon">local_shipping</mat-icon>
            <div class="stat-label">عدد السائقين اليوم</div>
            <div class="stat-value">{{ dashboard.driversToday }}</div>
          </div>
          <div class="stat-card red">
            <mat-icon class="stat-icon">report_problem</mat-icon>
            <div class="stat-label">إيصالات مفقودة</div>
            <div class="stat-value">{{ dashboard.openGapsCount }}</div>
          </div>
          <div class="stat-card orange">
            <mat-icon class="stat-icon">receipt</mat-icon>
            <div class="stat-label">عدد الإيصالات اليوم</div>
            <div class="stat-value">{{ dashboard.receiptsToday }}</div>
          </div>
          <div class="stat-card purple" *ngIf="ajalSummary" (click)="goToAjal()" style="cursor:pointer">
            <mat-icon class="stat-icon">auto_stories</mat-icon>
            <div class="stat-label">📄 فواتير الكول سنتر</div>
            <div class="stat-value">{{ ajalSummary.invoiceCount }}</div>
            <div class="stat-sub">{{ ajalSummary.totalAmount | number:'1.2-2' }} جنيه</div>
          </div>
        </div>

        <!-- Today's Sessions -->
        <mat-card class="data-table-card">
          <mat-card-header>
            <mat-card-title>جلسات اليوم</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <div *ngIf="dashboard.todaySessions.length === 0" class="empty-state">
              <mat-icon>inbox</mat-icon>
              <p>لا توجد جلسات لليوم بعد</p>
              <button mat-raised-button color="primary" routerLink="/sessions/new">
                <mat-icon>add</mat-icon> إضافة جلسة جديدة
              </button>
            </div>

            <!-- Desktop Table -->
            <div class="desktop-table" *ngIf="dashboard.todaySessions.length > 0">
              <table mat-table [dataSource]="dashboard.todaySessions" class="full-width">
                <ng-container matColumnDef="driver">
                  <th mat-header-cell *matHeaderCellDef>السائق</th>
                  <td mat-cell *matCellDef="let s">{{ s.driverName }}</td>
                </ng-container>
                <ng-container matColumnDef="route">
                  <th mat-header-cell *matHeaderCellDef>المنطقة</th>
                  <td mat-cell *matCellDef="let s">{{ s.routeArea }}</td>
                </ng-container>
                <ng-container matColumnDef="receipts">
                  <th mat-header-cell *matHeaderCellDef>الإيصالات</th>
                  <td mat-cell *matCellDef="let s">{{ s.firstReceiptNumber }} - {{ s.lastReceiptNumber }} ({{ s.totalReceiptsCount }})</td>
                </ng-container>
                <ng-container matColumnDef="amount">
                  <th mat-header-cell *matHeaderCellDef>المبلغ</th>
                  <td mat-cell *matCellDef="let s">{{ s.totalAmountCollected | number:'1.2-2' }} جنيه</td>
                </ng-container>
                <ng-container matColumnDef="gaps">
                  <th mat-header-cell *matHeaderCellDef>الحالة</th>
                  <td mat-cell *matCellDef="let s">
                    <span class="status-badge" [class.open]="s.hasGaps" [class.resolved]="!s.hasGaps">
                      {{ s.hasGaps ? '⚠️ يوجد فجوات' : '✅ مكتمل' }}
                    </span>
                  </td>
                </ng-container>
                <tr mat-header-row *matHeaderRowDef="['driver','route','receipts','amount','gaps']"></tr>
                <tr mat-row *matRowDef="let row; columns: ['driver','route','receipts','amount','gaps']"></tr>
              </table>
            </div>

            <!-- Mobile Cards -->
            <div class="mobile-receipt-cards" *ngIf="dashboard.todaySessions.length > 0">
              <mat-card *ngFor="let s of dashboard.todaySessions" class="session-mobile-card"
                        [class.has-gaps]="s.hasGaps">
                <div class="card-row">
                  <strong>{{ s.driverName }}</strong>
                  <span class="status-badge" [class.open]="s.hasGaps" [class.resolved]="!s.hasGaps">
                    {{ s.hasGaps ? 'فجوات' : 'مكتمل' }}
                  </span>
                </div>
                <div class="card-row">
                  <span>{{ s.routeArea }}</span>
                  <span>{{ s.totalReceiptsCount }} إيصال</span>
                </div>
                <div class="card-row amount-row">
                  <span>{{ s.totalAmountCollected | number:'1.2-2' }} جنيه</span>
                </div>
              </mat-card>
            </div>
          </mat-card-content>
        </mat-card>
      </ng-container>
    </div>
  `,
  styles: [`
    .empty-state {
      text-align: center; padding: 40px;
      mat-icon { font-size: 48px; width: 48px; height: 48px; color: #d1d5db; }
      p { color: #6b7280; margin: 12px 0; }
    }
    .session-mobile-card {
      margin-bottom: 8px; padding: 12px !important;
      border-radius: 12px !important; border-right: 4px solid #16a34a;
      &.has-gaps { border-right-color: #dc2626; }
    }
    .card-row {
      display: flex; justify-content: space-between; align-items: center;
      padding: 4px 0; font-size: 0.9rem;
    }
    .amount-row { font-weight: 700; color: #1a1a2e; font-size: 1rem; }
    small { font-size: 0.7rem; font-weight: 400; }
    .stat-card.purple { background: linear-gradient(135deg, #667eea, #764ba2); }
    .stat-sub { font-size: 0.85rem; opacity: 0.9; margin-top: 4px; }
  `]
})
export class DashboardComponent implements OnInit {
  dashboard: any = null;
  ajalSummary: any = null;
  loading = true;

  constructor(private api: ApiService, private router: Router) {}

  ngOnInit() {
    this.api.getDashboard().subscribe({
      next: (data) => { this.dashboard = data; this.loading = false; },
      error: () => { this.loading = false; }
    });
    this.api.getAjalDashboardSummary().subscribe({
      next: (data) => this.ajalSummary = data,
      error: () => {}
    });
  }

  goToAjal() { this.router.navigate(['/ajal/daily']); }
}
