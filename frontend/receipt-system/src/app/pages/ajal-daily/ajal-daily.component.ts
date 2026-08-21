import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatSelectModule } from '@angular/material/select';
import { RouterModule } from '@angular/router';
import { ApiService } from '../../core/api.service';

@Component({
  selector: 'app-ajal-daily',
  standalone: true,
  imports: [CommonModule, FormsModule, MatCardModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatIconModule,
    MatChipsModule, MatExpansionModule, MatTableModule, MatTooltipModule, MatSnackBarModule, MatProgressSpinnerModule,
    MatDatepickerModule, MatNativeDateModule, MatDialogModule, MatSelectModule, RouterModule],
  template: `
    <div class="page-container">
      <div class="page-header"><h1>📒 السجل اليومي — دفتر الآجل</h1></div>

      <!-- Header -->
      <div class="daily-header">
        <mat-form-field appearance="outline">
          <mat-label>تاريخ اليومية</mat-label>
          <input matInput [matDatepicker]="dp" [(ngModel)]="selectedDate" (dateChange)="loadData()">
          <mat-datepicker-toggle matSuffix [for]="dp"></mat-datepicker-toggle>
          <mat-datepicker #dp></mat-datepicker>
        </mat-form-field>
        <div class="chips-row" *ngIf="data">
          <span class="stat-chip">{{ data.totalInvoices }} فاتورة</span>
          <span class="stat-chip primary">{{ data.totalAmount | number:'1.2-2' }} جنيه</span>
          <span class="stat-chip warn" *ngIf="data.cancelledInvoices > 0">{{ data.cancelledInvoices }} ملغاة</span>
        </div>
        <span class="spacer"></span>
        <button mat-stroked-button (click)="exportExcel()" *ngIf="data?.totalInvoices" style="border-radius:10px">📥 Excel</button>
        <button mat-raised-button color="primary" (click)="printPage()" *ngIf="data?.totalInvoices" style="border-radius:10px">🖨️ طباعة</button>
      </div>

      <div class="loading-container" *ngIf="loading"><mat-spinner diameter="40"></mat-spinner></div>

      <!-- Empty state -->
      <div class="empty-state" *ngIf="!loading && (!data || data.totalInvoices === 0)">
        <mat-icon style="font-size:64px;width:64px;height:64px;color:#ccc">description</mat-icon>
        <p>لا توجد فواتير مسجلة لهذا اليوم</p>
        <button mat-raised-button color="primary" routerLink="/ajal/entry" style="border-radius:10px">إدخال فواتير</button>
      </div>

      <!-- Routes Accordion -->
      <div id="print-content" *ngIf="data?.totalInvoices">
        <div class="print-header">
          <h2>شركة البسطاوي للتجارة والتوزيع</h2>
          <h3>سجل فواتير الكول سنتر — دفتر الآجل</h3>
          <p>التاريخ: {{ selectedDate | date:'dd/MM/yyyy' }}</p>
        </div>
        <mat-accordion multi>
          <mat-expansion-panel *ngFor="let route of data.routes" [expanded]="true" class="route-panel">
            <mat-expansion-panel-header>
              <mat-panel-title>🚛 {{ route.routeName }}</mat-panel-title>
              <mat-panel-description>{{ route.invoiceCount }} فاتورة — {{ route.routeTotal | number:'1.2-2' }} جنيه</mat-panel-description>
            </mat-expansion-panel-header>

            <!-- Desktop table -->
            <div class="desktop-table">
              <table mat-table [dataSource]="route.invoices">
                <ng-container matColumnDef="invoiceNumber">
                  <th mat-header-cell *matHeaderCellDef>رقم الفاتورة</th>
                  <td mat-cell *matCellDef="let r" [class.cancelled-text]="r.invoiceStatus==='Cancelled'" [class.modified-text]="r.invoiceStatus==='Modified'">
                    {{ r.invoiceNumber }}
                    <mat-icon *ngIf="r.invoiceStatus==='Cancelled'" color="warn" [matTooltip]="r.modificationNote || ''" style="font-size:16px;vertical-align:middle">cancel</mat-icon>
                    <mat-icon *ngIf="r.invoiceStatus==='Modified'" color="accent" [matTooltip]="r.modificationNote || ''" style="font-size:16px;vertical-align:middle">edit_note</mat-icon>
                  </td>
                </ng-container>
                <ng-container matColumnDef="merchant">
                  <th mat-header-cell *matHeaderCellDef>التاجر</th>
                  <td mat-cell *matCellDef="let r">{{ r.merchantName }}<br><small *ngIf="r.merchantPhone" class="phone-hint">📞 {{ r.merchantPhone }}</small></td>
                </ng-container>
                <ng-container matColumnDef="employee">
                  <th mat-header-cell *matHeaderCellDef>الموظف</th>
                  <td mat-cell *matCellDef="let r">{{ r.callCenterEmployeeName || '—' }}</td>
                </ng-container>
                <ng-container matColumnDef="amount">
                  <th mat-header-cell *matHeaderCellDef>المبلغ</th>
                  <td mat-cell *matCellDef="let r">
                    {{ r.amount | number:'1.2-2' }}
                    <div *ngIf="r.originalAmount" class="original-hint">الأصلي: {{ r.originalAmount | number:'1.2-2' }}</div>
                  </td>
                </ng-container>
                <ng-container matColumnDef="actions">
                  <th mat-header-cell *matHeaderCellDef></th>
                  <td mat-cell *matCellDef="let r">
                    <button mat-icon-button (click)="editInvoice(r)" matTooltip="تعديل"><mat-icon>edit</mat-icon></button>
                    <button mat-icon-button color="warn" *ngIf="r.invoiceStatus!=='Cancelled'" (click)="cancelInvoice(r)" matTooltip="إلغاء"><mat-icon>cancel</mat-icon></button>
                  </td>
                </ng-container>
                <tr mat-header-row *matHeaderRowDef="columns"></tr>
                <tr mat-row *matRowDef="let r; columns: columns;" [class.row-cancelled]="r.invoiceStatus==='Cancelled'"></tr>
              </table>
            </div>

            <!-- Mobile cards -->
            <div class="mobile-cards">
              <mat-card *ngFor="let inv of route.invoices" [class.card-cancelled]="inv.invoiceStatus==='Cancelled'" class="inv-card">
                <div class="card-row"><strong>{{ inv.invoiceNumber }}</strong><span class="amt">{{ inv.amount | number:'1.2-2' }} جنيه</span></div>
                <div class="card-row">🏪 {{ inv.merchantName }}</div>
                <div class="card-row" *ngIf="inv.merchantPhone"><a [href]="'tel:'+inv.merchantPhone">📞 {{ inv.merchantPhone }}</a></div>
                <div class="card-row" *ngIf="inv.callCenterEmployeeName">👤 {{ inv.callCenterEmployeeName }}</div>
                <div class="card-actions">
                  <button mat-button (click)="editInvoice(inv)">تعديل</button>
                  <button mat-button color="warn" *ngIf="inv.invoiceStatus!=='Cancelled'" (click)="cancelInvoice(inv)">إلغاء</button>
                </div>
              </mat-card>
            </div>

            <div class="route-footer">إجمالي {{ route.routeName }}: <strong>{{ route.routeTotal | number:'1.2-2' }} جنيه</strong></div>
          </mat-expansion-panel>
        </mat-accordion>

        <div class="grand-total">
          إجمالي اليوم: <strong>{{ data.activeInvoices }}</strong> فاتورة — <strong>{{ data.totalAmount | number:'1.2-2' }} جنيه</strong>
        </div>
      </div>

      <!-- Edit Dialog -->
      <div *ngIf="editMode" class="overlay" (click)="editMode=false">
        <div class="dialog-card" (click)="$event.stopPropagation()">
          <h3>تعديل فاتورة {{ editForm.invoiceNumber }}</h3>
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>المبلغ</mat-label>
            <input matInput type="number" [(ngModel)]="editForm.amount"><span matSuffix>جنيه</span>
          </mat-form-field>
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>موظف الكول سنتر</mat-label>
            <input matInput [(ngModel)]="editForm.callCenterEmployeeName">
          </mat-form-field>
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>الخط</mat-label>
            <mat-select [(ngModel)]="editForm.routeId">
              <mat-option [value]="null">غير محدد</mat-option>
              <mat-option *ngFor="let r of routes" [value]="r.routeId">{{ r.routeName }}</mat-option>
            </mat-select>
          </mat-form-field>
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>سبب التعديل</mat-label>
            <textarea matInput rows="2" [(ngModel)]="editForm.modificationNote"></textarea>
          </mat-form-field>
          <div class="dialog-actions">
            <button mat-button (click)="editMode=false">إلغاء</button>
            <button mat-raised-button color="primary" (click)="saveEdit()" style="border-radius:10px">حفظ</button>
          </div>
        </div>
      </div>

      <!-- Cancel Dialog -->
      <div *ngIf="cancelMode" class="overlay" (click)="cancelMode=false">
        <div class="dialog-card" (click)="$event.stopPropagation()">
          <h3>إلغاء فاتورة {{ cancelTarget?.invoiceNumber }}</h3>
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>سبب الإلغاء</mat-label>
            <textarea matInput rows="2" [(ngModel)]="cancelReason"></textarea>
          </mat-form-field>
          <div class="dialog-actions">
            <button mat-button (click)="cancelMode=false">تراجع</button>
            <button mat-raised-button color="warn" (click)="confirmCancel()" style="border-radius:10px">تأكيد الإلغاء</button>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .daily-header { display:flex; align-items:center; gap:12px; flex-wrap:wrap; margin-bottom:16px; }
    .chips-row { display:flex; gap:8px; flex-wrap:wrap; }
    .stat-chip { padding:4px 14px; border-radius:12px; background:#f3f4f6; font-weight:600; font-size:0.9rem; }
    .stat-chip.primary { background:#dcfce7; color:#166534; }
    .stat-chip.warn { background:#fef2f2; color:#dc2626; }
    .spacer { flex:1; }
    .route-panel { border-radius:16px !important; margin-bottom:12px; }
    .cancelled-text { text-decoration:line-through; color:#999; }
    .modified-text { color:#FF8F00; }
    .row-cancelled td { opacity:0.5; }
    .card-cancelled { opacity:0.6; border-right:4px solid #f44336 !important; }
    .original-hint { color:#999; font-size:11px; }
    .phone-hint { color:#667eea; font-size:0.8rem; }
    .route-footer { padding:12px 16px; background:#f8f9fa; border-radius:0 0 12px 12px; text-align:left; font-size:0.95rem; }
    .grand-total { padding:16px; background:linear-gradient(135deg,#667eea,#764ba2); color:#fff; border-radius:14px; text-align:center; font-size:1.1rem; margin-top:16px; }
    .desktop-table { display:block; } .mobile-cards { display:none; }
    @media(max-width:768px) { .desktop-table { display:none; } .mobile-cards { display:block; } }
    .inv-card { margin-bottom:8px; padding:12px !important; border-radius:10px !important; }
    .card-row { margin-bottom:4px; display:flex; justify-content:space-between; flex-wrap:wrap; }
    .card-actions { display:flex; gap:8px; margin-top:4px; }
    .amt { font-weight:700; color:#22c55e; }
    .overlay { position:fixed; top:0;left:0;right:0;bottom:0; background:rgba(0,0,0,0.5); z-index:1000; display:flex; align-items:center; justify-content:center; }
    .dialog-card { background:#fff; border-radius:16px; padding:24px; width:90%; max-width:480px; }
    .dialog-actions { display:flex; gap:8px; justify-content:flex-end; margin-top:12px; }
    .full-width { width:100%; }
    .empty-state { text-align:center; padding:48px; }
    .loading-container { text-align:center; padding:32px; }
    .print-header { display:none; text-align:center; }
    @media print { .print-header { display:block !important; } .daily-header, .overlay, .card-actions button, mat-expansion-panel-header mat-icon { display:none !important; } .mobile-cards { display:none !important; } .desktop-table { display:block !important; } }
  `]
})
export class AjalDailyComponent implements OnInit {
  selectedDate = new Date();
  data: any = null;
  loading = false;
  columns = ['invoiceNumber', 'merchant', 'employee', 'amount', 'actions'];
  routes: any[] = [];
  editMode = false; editForm: any = {};
  cancelMode = false; cancelTarget: any = null; cancelReason = '';

  constructor(private api: ApiService, private snack: MatSnackBar) {}

  ngOnInit() {
    this.loadData();
    this.api.getRoutes().subscribe((r: any) => this.routes = r);
  }

  loadData() {
    this.loading = true;
    const ds = this.formatDate(this.selectedDate);
    this.api.getAjalDaily(ds).subscribe({ next: d => { this.data = d; this.loading = false; }, error: () => { this.loading = false; } });
  }

  editInvoice(inv: any) {
    this.editForm = { id: inv.ajalInvoiceId, invoiceNumber: inv.invoiceNumber, amount: inv.amount,
      callCenterEmployeeName: inv.callCenterEmployeeName, routeId: null, modificationNote: '', invoiceStatus: 'Modified' };
    this.editMode = true;
  }

  saveEdit() {
    this.api.editAjalInvoice(this.editForm.id, this.editForm).subscribe({
      next: () => { this.editMode = false; this.loadData(); this.snack.open('تم تعديل الفاتورة', 'حسناً', { duration: 2000 }); },
      error: () => this.snack.open('خطأ في التعديل', 'إغلاق', { duration: 3000 })
    });
  }

  cancelInvoice(inv: any) { this.cancelTarget = inv; this.cancelReason = ''; this.cancelMode = true; }

  confirmCancel() {
    this.api.cancelAjalInvoice(this.cancelTarget.ajalInvoiceId, this.cancelReason).subscribe({
      next: () => { this.cancelMode = false; this.loadData(); this.snack.open('تم إلغاء الفاتورة', 'حسناً', { duration: 2000 }); },
      error: () => this.snack.open('خطأ في الإلغاء', 'إغلاق', { duration: 3000 })
    });
  }

  exportExcel() {
    this.api.exportAjalDaily(this.formatDate(this.selectedDate)).subscribe(blob => {
      const url = URL.createObjectURL(blob); const a = document.createElement('a');
      a.href = url; a.download = `ajal_${this.formatDate(this.selectedDate)}.xlsx`; a.click(); URL.revokeObjectURL(url);
    });
  }

  printPage() { window.print(); }
  private formatDate(d: Date): string { return d.toISOString().split('T')[0]; }
}
