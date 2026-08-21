import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ApiService } from '../../core/api.service';

@Component({
  selector: 'app-sessions',
  standalone: true,
  imports: [
    CommonModule, FormsModule, RouterModule, MatCardModule, MatTableModule, MatButtonModule,
    MatIconModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatDatepickerModule,
    MatNativeDateModule, MatPaginatorModule, MatProgressSpinnerModule
  ],
  template: `
    <div class="page-container">
      <div class="page-header">
        <h1>الجلسات</h1>
        <button mat-raised-button color="primary" routerLink="/sessions/new" style="border-radius:12px !important">
          <mat-icon>add</mat-icon> جلسة جديدة
        </button>
      </div>

      <!-- Filters -->
      <mat-card style="margin-bottom:16px; padding:16px !important; border-radius:16px !important">
        <div class="form-row">
          <mat-form-field appearance="outline">
            <mat-label>تصفية بالتاريخ</mat-label>
            <input matInput [matDatepicker]="fp" [(ngModel)]="filterDate" (dateChange)="loadSessions()">
            <mat-datepicker-toggle matSuffix [for]="fp"></mat-datepicker-toggle>
            <mat-datepicker #fp></mat-datepicker>
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>تصفية بالسائق</mat-label>
            <mat-select [(ngModel)]="filterDriverId" (selectionChange)="loadSessions()">
              <mat-option [value]="null">الكل</mat-option>
              <mat-option *ngFor="let d of drivers" [value]="d.driverId">{{ d.fullName }}</mat-option>
            </mat-select>
          </mat-form-field>
          <button mat-button color="primary" (click)="clearFilters()">
            <mat-icon>clear</mat-icon> مسح التصفية
          </button>
        </div>
      </mat-card>

      <div class="loading-container" *ngIf="loading"><mat-spinner diameter="40"></mat-spinner></div>

      <mat-card class="data-table-card" *ngIf="!loading">
        <!-- Desktop Table -->
        <div class="desktop-table">
          <table mat-table [dataSource]="sessions" class="full-width">
            <ng-container matColumnDef="date">
              <th mat-header-cell *matHeaderCellDef>التاريخ</th>
              <td mat-cell *matCellDef="let s">{{ s.sessionDate | date:'yyyy/MM/dd' }}</td>
            </ng-container>
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
              <td mat-cell *matCellDef="let s">{{ s.firstReceiptNumber }}-{{ s.lastReceiptNumber }} ({{ s.totalReceiptsCount }})</td>
            </ng-container>
            <ng-container matColumnDef="amount">
              <th mat-header-cell *matHeaderCellDef>المبلغ</th>
              <td mat-cell *matCellDef="let s">{{ s.totalAmountCollected | number:'1.2-2' }} جنيه</td>
            </ng-container>
            <ng-container matColumnDef="status">
              <th mat-header-cell *matHeaderCellDef>الحالة</th>
              <td mat-cell *matCellDef="let s">
                <span class="status-badge" [class.open]="s.hasGaps" [class.resolved]="!s.hasGaps">
                  {{ s.hasGaps ? '⚠️ فجوات' : '✅ مكتمل' }}
                </span>
              </td>
            </ng-container>
            <tr mat-header-row *matHeaderRowDef="['date','driver','route','receipts','amount','status']"></tr>
            <tr mat-row *matRowDef="let row; columns: ['date','driver','route','receipts','amount','status']"></tr>
          </table>
        </div>

        <!-- Mobile Cards -->
        <div class="mobile-receipt-cards">
          <mat-card *ngFor="let s of sessions" style="margin:8px 0; padding:12px !important; border-radius:12px !important; border-right:4px solid"
                    [style.border-right-color]="s.hasGaps ? '#dc2626' : '#16a34a'">
            <div style="display:flex; justify-content:space-between; margin-bottom:4px">
              <strong>{{ s.driverName }}</strong>
              <span style="font-size:0.8rem; color:#6b7280">{{ s.sessionDate | date:'MM/dd' }}</span>
            </div>
            <div style="font-size:0.85rem; color:#6b7280; margin-bottom:4px">{{ s.routeArea }}</div>
            <div style="display:flex; justify-content:space-between">
              <span>{{ s.totalReceiptsCount }} إيصال</span>
              <strong>{{ s.totalAmountCollected | number:'1.2-2' }} جنيه</strong>
            </div>
          </mat-card>
        </div>

        <mat-paginator [length]="totalSessions" [pageSize]="20" (page)="onPage($event)"
                       [hidePageSize]="true"></mat-paginator>
      </mat-card>
    </div>
  `
})
export class SessionsComponent implements OnInit {
  sessions: any[] = [];
  drivers: any[] = [];
  loading = true;
  totalSessions = 0;
  page = 1;
  filterDate: Date | null = null;
  filterDriverId: number | null = null;

  constructor(private api: ApiService) {}

  ngOnInit() {
    this.api.getDrivers().subscribe(d => this.drivers = d);
    this.loadSessions();
  }

  loadSessions() {
    this.loading = true;
    const dateStr = this.filterDate ? this.filterDate.toISOString().split('T')[0] : undefined;
    this.api.getSessions(this.page, 20, dateStr, this.filterDriverId || undefined).subscribe({
      next: (res: any) => { this.sessions = res.data; this.totalSessions = res.total; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  onPage(e: PageEvent) { this.page = e.pageIndex + 1; this.loadSessions(); }
  clearFilters() { this.filterDate = null; this.filterDriverId = null; this.loadSessions(); }
}
