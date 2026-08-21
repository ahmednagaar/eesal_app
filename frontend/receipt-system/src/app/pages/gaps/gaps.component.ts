import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatExpansionModule } from '@angular/material/expansion';
import { ApiService } from '../../core/api.service';

@Component({
  selector: 'app-gaps',
  standalone: true,
  imports: [
    CommonModule, FormsModule, MatCardModule, MatTableModule, MatButtonModule,
    MatIconModule, MatChipsModule, MatDialogModule, MatFormFieldModule,
    MatInputModule, MatSelectModule, MatSnackBarModule, MatProgressSpinnerModule,
    MatExpansionModule
  ],
  template: `
    <div class="page-container">
      <div class="page-header">
        <h1>الإيصالات المفقودة</h1>
        <div style="display:flex; gap:8px; flex-wrap:wrap">
          <button mat-stroked-button [class.active-filter]="statusFilter===''" (click)="filterByStatus('')">الكل ({{ summary.totalCount }})</button>
          <button mat-stroked-button color="warn" [class.active-filter]="statusFilter==='Open'" (click)="filterByStatus('Open')">مفتوح ({{ summary.openCount }})</button>
          <button mat-stroked-button [class.active-filter]="statusFilter==='UnderInvestigation'" (click)="filterByStatus('UnderInvestigation')">قيد التحقيق ({{ summary.underInvestigationCount }})</button>
          <button mat-stroked-button color="primary" [class.active-filter]="statusFilter==='Resolved'" (click)="filterByStatus('Resolved')">تم الحل ({{ summary.resolvedCount }})</button>
        </div>
      </div>

      <div class="loading-container" *ngIf="loading"><mat-spinner diameter="40"></mat-spinner></div>

      <ng-container *ngIf="!loading">
        <div *ngIf="gaps.length === 0" style="text-align:center; padding:48px">
          <mat-icon style="font-size:64px;width:64px;height:64px;color:#16a34a">verified</mat-icon>
          <h3 style="color:#16a34a">لا توجد إيصالات مفقودة 🎉</h3>
        </div>

        <!-- Desktop Table -->
        <div class="desktop-table" *ngIf="gaps.length > 0">
        <mat-card class="data-table-card">
          <table mat-table [dataSource]="gaps" class="full-width">
            <ng-container matColumnDef="receipt">
              <th mat-header-cell *matHeaderCellDef>رقم الإيصال</th>
              <td mat-cell *matCellDef="let g"><strong>{{ g.missingReceiptNumber }}</strong></td>
            </ng-container>
            <ng-container matColumnDef="driver">
              <th mat-header-cell *matHeaderCellDef>السائق</th>
              <td mat-cell *matCellDef="let g">{{ g.driverName }}</td>
            </ng-container>
            <ng-container matColumnDef="detected">
              <th mat-header-cell *matHeaderCellDef>تاريخ الاكتشاف</th>
              <td mat-cell *matCellDef="let g">{{ g.detectedAt | date:'yyyy/MM/dd HH:mm' }}</td>
            </ng-container>
            <ng-container matColumnDef="status">
              <th mat-header-cell *matHeaderCellDef>الحالة</th>
              <td mat-cell *matCellDef="let g">
                <span class="status-badge" [ngClass]="getStatusClass(g.status)">{{ getStatusLabel(g.status) }}</span>
              </td>
            </ng-container>
            <ng-container matColumnDef="resolution">
              <th mat-header-cell *matHeaderCellDef>الملاحظات</th>
              <td mat-cell *matCellDef="let g">{{ g.resolution || '—' }}</td>
            </ng-container>
            <ng-container matColumnDef="actions">
              <th mat-header-cell *matHeaderCellDef>إجراء</th>
              <td mat-cell *matCellDef="let g">
                <button mat-raised-button color="primary" (click)="openResolve(g)"
                        *ngIf="g.status !== 'Resolved' && g.status !== 'Explained'"
                        style="border-radius:8px !important; font-size:0.8rem">
                  حل المشكلة
                </button>
              </td>
            </ng-container>
            <tr mat-header-row *matHeaderRowDef="['receipt','driver','detected','status','resolution','actions']"></tr>
            <tr mat-row *matRowDef="let row; columns: ['receipt','driver','detected','status','resolution','actions']"></tr>
          </table>
        </mat-card>
        </div>

        <!-- Mobile Accordion -->
        <div class="mobile-receipt-cards" *ngIf="gaps.length > 0">
          <mat-accordion>
            <mat-expansion-panel *ngFor="let g of gaps">
              <mat-expansion-panel-header>
                <mat-panel-title>
                  إيصال {{ g.missingReceiptNumber }} — {{ g.driverName }}
                </mat-panel-title>
                <mat-panel-description>
                  <span class="status-badge" [ngClass]="getStatusClass(g.status)" style="font-size:0.7rem">
                    {{ getStatusLabel(g.status) }}
                  </span>
                </mat-panel-description>
              </mat-expansion-panel-header>
              <p><strong>تاريخ الاكتشاف:</strong> {{ g.detectedAt | date:'yyyy/MM/dd' }}</p>
              <p *ngIf="g.resolution"><strong>ملاحظات:</strong> {{ g.resolution }}</p>
              <button mat-raised-button color="primary" (click)="openResolve(g)"
                      *ngIf="g.status !== 'Resolved' && g.status !== 'Explained'"
                      style="border-radius:8px !important; width:100%">
                حل المشكلة
              </button>
            </mat-expansion-panel>
          </mat-accordion>
        </div>
      </ng-container>

      <!-- Resolve Dialog (inline) -->
      <div class="resolve-overlay" *ngIf="resolving" (click)="resolving=null">
        <mat-card class="resolve-dialog" (click)="$event.stopPropagation()">
          <h3>حل مشكلة الإيصال رقم {{ resolving.missingReceiptNumber }}</h3>
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>الحالة الجديدة</mat-label>
            <mat-select [(ngModel)]="resolveData.status">
              <mat-option value="Resolved">تم الحل</mat-option>
              <mat-option value="Explained">تم التوضيح</mat-option>
              <mat-option value="UnderInvestigation">قيد التحقيق</mat-option>
            </mat-select>
          </mat-form-field>
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>ملاحظات الحل</mat-label>
            <textarea matInput rows="3" [(ngModel)]="resolveData.resolution"
                      placeholder="مثال: السائق أثبت أن التاجر سيدفع الأسبوع القادم"></textarea>
          </mat-form-field>
          <div style="display:flex; gap:8px; justify-content:flex-start">
            <button mat-raised-button color="primary" (click)="submitResolve()" style="border-radius:8px !important">
              حفظ
            </button>
            <button mat-button (click)="resolving=null">إلغاء</button>
          </div>
        </mat-card>
      </div>
    </div>
  `,
  styles: [`
    .active-filter { font-weight: 700 !important; }
    .resolve-overlay {
      position: fixed; inset: 0; background: rgba(0,0,0,0.5); z-index: 1000;
      display: flex; align-items: center; justify-content: center; padding: 16px;
    }
    .resolve-dialog {
      width: 100%; max-width: 500px; padding: 24px !important; border-radius: 16px !important;
      h3 { margin: 0 0 16px; font-weight: 700; }
    }
  `]
})
export class GapsComponent implements OnInit {
  gaps: any[] = [];
  summary = { openCount: 0, underInvestigationCount: 0, resolvedCount: 0, totalCount: 0 };
  loading = true;
  statusFilter = '';
  resolving: any = null;
  resolveData = { status: 'Resolved', resolution: '' };

  constructor(private api: ApiService, private snack: MatSnackBar) {}

  ngOnInit() { this.load(); }

  load() {
    this.loading = true;
    this.api.getGapSummary().subscribe(s => this.summary = s);
    this.api.getGaps(this.statusFilter || undefined).subscribe({
      next: (g) => { this.gaps = g; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  filterByStatus(status: string) { this.statusFilter = status; this.load(); }

  getStatusClass(status: string): string {
    const map: any = { Open: 'open', UnderInvestigation: 'under-investigation', Resolved: 'resolved', Explained: 'explained' };
    return map[status] || '';
  }

  getStatusLabel(status: string): string {
    const map: any = { Open: '🔴 مفتوح', UnderInvestigation: '🟠 قيد التحقيق', Resolved: '🟢 تم الحل', Explained: '🔵 تم التوضيح' };
    return map[status] || status;
  }

  openResolve(gap: any) {
    this.resolving = gap;
    this.resolveData = { status: 'Resolved', resolution: '' };
  }

  submitResolve() {
    this.api.resolveGap(this.resolving.gapId, this.resolveData).subscribe({
      next: () => {
        this.snack.open('تم تحديث الحالة بنجاح ✅', 'إغلاق', { duration: 3000 });
        this.resolving = null;
        this.load();
      },
      error: () => { this.snack.open('حدث خطأ', 'إغلاق', { duration: 3000 }); }
    });
  }
}
