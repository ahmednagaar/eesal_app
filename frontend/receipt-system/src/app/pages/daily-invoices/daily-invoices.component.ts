import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CdkDragDrop, DragDropModule, moveItemInArray } from '@angular/cdk/drag-drop';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTabsModule } from '@angular/material/tabs';
import { trigger, transition, style, animate } from '@angular/animations';
import { ApiService } from '../../core/api.service';

interface ChecklistItem {
  routeMerchantId: number;
  merchantId: number;
  merchantName: string;
  city: string | null;
  positionOrder: number;
  isSelected: boolean;
  invoiceNumber: string;
  quantity: string;
  amount: number | null;
  notes: string;
  dayInvoiceId: number | null;
}

@Component({
  selector: 'app-daily-invoices',
  standalone: true,
  imports: [
    CommonModule, FormsModule, DragDropModule, MatCardModule, MatFormFieldModule,
    MatInputModule, MatSelectModule, MatButtonModule, MatIconModule, MatCheckboxModule,
    MatDatepickerModule, MatNativeDateModule, MatSnackBarModule, MatProgressSpinnerModule,
    MatTabsModule
  ],
  animations: [
    trigger('slideDown', [
      transition(':enter', [
        style({ height: 0, opacity: 0, overflow: 'hidden' }),
        animate('200ms ease-out', style({ height: '*', opacity: 1 }))
      ]),
      transition(':leave', [
        animate('150ms ease-in', style({ height: 0, opacity: 0, overflow: 'hidden' }))
      ])
    ])
  ],
  template: `
    <div class="page-container">
      <div class="page-header no-print">
        <h1>فواتير اليوم</h1>
      </div>

      <!-- Step 1: Setup -->
      <mat-card class="setup-card no-print" *ngIf="!started">
        <div class="card-section-title">
          <mat-icon>settings</mat-icon>
          <h3>اختر الخط والتاريخ</h3>
        </div>
        <div class="form-row">
          <mat-form-field appearance="outline">
            <mat-label>الخط</mat-label>
            <mat-select [(ngModel)]="selectedRouteId" id="route-select">
              <mat-option *ngFor="let r of routes" [value]="r.routeId">
                {{ r.routeName }} ({{ r.merchantCount }} تاجر)
              </mat-option>
            </mat-select>
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>التاريخ</mat-label>
            <input matInput [matDatepicker]="dp" [(ngModel)]="selectedDate" id="date-input">
            <mat-datepicker-toggle matSuffix [for]="dp"></mat-datepicker-toggle>
            <mat-datepicker #dp></mat-datepicker>
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>السائق (اختياري)</mat-label>
            <input matInput [(ngModel)]="driverName" id="driver-input">
          </mat-form-field>
        </div>
        <button mat-raised-button color="primary" (click)="startDay()"
                [disabled]="!selectedRouteId || !selectedDate || starting"
                class="start-btn">
          <mat-spinner *ngIf="starting" diameter="20"></mat-spinner>
          <span *ngIf="!starting"><mat-icon>play_arrow</mat-icon> بدء تحديد الفواتير</span>
        </button>
      </mat-card>

      <div class="loading-container" *ngIf="loadingChecklist"><mat-spinner diameter="40"></mat-spinner></div>

      <!-- Step 2: Checklist -->
      <ng-container *ngIf="started && !loadingChecklist">
        <mat-tab-group class="no-print" animationDuration="200ms">
          <!-- Tab 1: Checklist -->
          <mat-tab>
            <ng-template mat-tab-label>
              <mat-icon>checklist</mat-icon>
              <span style="margin-right:6px">قائمة التجار</span>
            </ng-template>

            <div class="checklist-header">
              <span class="counter">✅ تم تحديد {{ selectedCount }} من {{ checklist.length }} تاجر</span>
              <mat-form-field appearance="outline" class="search-box">
                <mat-label>🔍 بحث سريع</mat-label>
                <input matInput [(ngModel)]="jumpSearch" (input)="jumpToMerchant()"
                       placeholder="اكتب اسم تاجر للانتقال إليه...">
              </mat-form-field>
            </div>

            <!-- Quick Add Search Bar -->
            <div class="quick-add-container">
              <div class="quick-add-header">
                <span class="quick-add-label">⚡ إضافة سريعة</span>
              </div>
              <div class="quick-add-wrapper">
                <input id="quickAddInput" type="text" class="quick-add-input"
                       [(ngModel)]="quickAddSearch"
                       (input)="onQuickAddInput()"
                       (keydown)="onQuickAddKeydown($event)"
                       (blur)="onQuickAddBlur()"
                       placeholder="🔍 اكتب اسم التاجر واضغط Enter للإضافة..."
                       autocomplete="off" dir="rtl">
                <button class="quick-add-clear" *ngIf="quickAddSearch"
                        (click)="quickAddSearch=''; quickAddResults=[]; showQuickResults=false"
                        type="button">✕</button>
                <div class="quick-add-dropdown" *ngIf="showQuickResults">
                  <div class="quick-add-result"
                       *ngFor="let result of quickAddResults; let i = index"
                       [class.highlighted]="i === quickAddIndex"
                       (mousedown)="selectQuickAdd(result)">
                    <span class="result-position">{{ result.positionOrder }}</span>
                    <span class="result-name">{{ result.merchantName }}</span>
                    <span class="result-city" *ngIf="result.city">{{ result.city }}</span>
                    <span class="result-add-hint">Enter ↵</span>
                  </div>
                  <div class="quick-add-no-result"
                       *ngIf="quickAddResults.length === 0">
                    لا يوجد تاجر بهذا الاسم
                  </div>
                </div>
              </div>
              <div class="quick-add-hint">اكتب جزء من اسم التاجر ← اختر من النتائج أو اضغط Enter مباشرة</div>
            </div>

            <div class="merchant-checklist">
              <div class="checklist-row" *ngFor="let rm of checklist"
                   [class.selected]="rm.isSelected"
                   [id]="'merchant-' + rm.routeMerchantId">
                <mat-checkbox [(ngModel)]="rm.isSelected" (change)="onMerchantToggle(rm)"
                              class="merchant-check"></mat-checkbox>
                <span class="position-num">{{ rm.positionOrder }}</span>
                <span class="check-merchant-name">{{ rm.merchantName }}</span>
                <span class="check-city" *ngIf="rm.city">{{ rm.city }}</span>

                <div class="invoice-fields" *ngIf="rm.isSelected" @slideDown>
                  <mat-form-field appearance="outline" class="inv-field">
                    <mat-label>رقم الفاتورة</mat-label>
                    <input matInput [(ngModel)]="rm.invoiceNumber" placeholder="12345">
                  </mat-form-field>
                  <mat-form-field appearance="outline" class="inv-field">
                    <mat-label>الكمية / البيان</mat-label>
                    <input matInput [(ngModel)]="rm.quantity" placeholder="3 كراتين">
                  </mat-form-field>
                  <mat-form-field appearance="outline" class="inv-field-sm">
                    <mat-label>المبلغ</mat-label>
                    <input matInput type="number" [(ngModel)]="rm.amount">
                  </mat-form-field>
                  <mat-form-field appearance="outline" class="inv-field">
                    <mat-label>ملاحظات</mat-label>
                    <input matInput [(ngModel)]="rm.notes">
                  </mat-form-field>
                </div>
              </div>
            </div>
          </mat-tab>

          <!-- Tab 2: Preview -->
          <mat-tab>
            <ng-template mat-tab-label>
              <mat-icon>preview</mat-icon>
              <span style="margin-right:6px">معاينة وترتيب</span>
            </ng-template>

            <div class="preview-panel" *ngIf="selectedItems.length > 0">
              <h3>ترتيب التسليم — يمكن التعديل لهذا اليوم فقط</h3>
              <p class="note">أي تعديل هنا لن يؤثر على الترتيب الأصلي للخط</p>
              <div cdkDropList (cdkDropListDropped)="onPreviewDrop($event)" class="preview-list">
                <div class="preview-row" *ngFor="let inv of selectedItems; let i = index" cdkDrag>
                  <mat-icon cdkDragHandle class="drag-handle">drag_indicator</mat-icon>
                  <span class="seq">{{ i + 1 }}</span>
                  <span class="preview-name">{{ inv.merchantName }}</span>
                  <span class="preview-inv" *ngIf="inv.invoiceNumber">فاتورة: {{ inv.invoiceNumber }}</span>
                  <span class="preview-qty" *ngIf="inv.quantity">{{ inv.quantity }}</span>
                </div>
              </div>
            </div>
            <div *ngIf="selectedItems.length === 0" class="empty-state" style="padding:48px">
              <mat-icon>playlist_add_check</mat-icon>
              <p>لم يتم تحديد أي تاجر بعد</p>
            </div>
          </mat-tab>
        </mat-tab-group>

        <!-- Sticky Action Bar -->
        <div class="action-bar no-print">
          <span>{{ selectedCount }} تاجر محدد</span>
          <span *ngIf="totalAmount > 0">إجمالي: {{ totalAmount | number:'1.2-2' }} جنيه</span>
          <span class="spacer"></span>
          <button mat-stroked-button (click)="saveAndPrint('delivery')" [disabled]="saving || selectedCount === 0"
                  style="border-radius:10px !important">
            🚚 طباعة قائمة التسليم
          </button>
          <button mat-raised-button color="primary" (click)="saveAndPrint('loading')"
                  [disabled]="saving || selectedCount === 0"
                  style="border-radius:10px !important">
            <mat-spinner *ngIf="saving" diameter="18"></mat-spinner>
            <span *ngIf="!saving">📦 طباعة قائمة التحميل</span>
          </button>
        </div>
      </ng-container>

      <!-- Print: Loading Sheet -->
      <div class="print-sheet loading-sheet-print" *ngIf="printSheet && printMode === 'loading'">
        <div class="sheet-header">
          <h2>قائمة التحميل — للعمال فقط</h2>
          <div class="sheet-meta">
            <span>الخط: {{ printSheet.routeName }}</span>
            <span>التاريخ: {{ printSheet.deliveryDate | date:'dd/MM/yyyy' }}</span>
          </div>
          <div class="sheet-meta" *ngIf="printSheet.assignedDriver">
            <span>السائق: {{ printSheet.assignedDriver }}</span>
            <span>إعداد: {{ printSheet.preparedBy }}</span>
          </div>
          <p class="sheet-warning">⚠️ يُحمَّل من الأعلى للأسفل — الصف الأول يُحمَّل أولاً</p>
        </div>
        <table class="sheet-table">
          <thead>
            <tr><th>م</th><th>اسم التاجر</th><th>الكمية / البيان</th><th>رقم الفاتورة</th><th>✓</th></tr>
          </thead>
          <tbody>
            <tr *ngFor="let line of printSheet.lines">
              <td>{{ line.sequenceNumber }}</td>
              <td>{{ line.merchantName }}</td>
              <td>{{ line.quantity }}</td>
              <td>{{ line.invoiceNumber }}</td>
              <td>☐</td>
            </tr>
          </tbody>
        </table>
        <div class="sheet-footer">
          <span>الإجمالي: {{ printSheet.totalMerchants }} تاجر</span>
          <span>توقيع رئيس العمال: _______________</span>
        </div>
      </div>

      <!-- Print: Delivery Sheet -->
      <div class="print-sheet delivery-sheet-print" *ngIf="printSheet && printMode === 'delivery'">
        <div class="sheet-header">
          <h2>قائمة التسليم — للسائق</h2>
          <div class="sheet-meta">
            <span>الخط: {{ printSheet.routeName }}</span>
            <span>التاريخ: {{ printSheet.deliveryDate | date:'dd/MM/yyyy' }}</span>
          </div>
          <div class="sheet-meta" *ngIf="printSheet.assignedDriver">
            <span>السائق: {{ printSheet.assignedDriver }}</span>
          </div>
        </div>
        <table class="sheet-table">
          <thead>
            <tr><th>م</th><th>اسم التاجر</th><th>الكمية / البيان</th><th>رقم الفاتورة</th><th>✓</th></tr>
          </thead>
          <tbody>
            <tr *ngFor="let line of printSheet.lines">
              <td>{{ line.sequenceNumber }}</td>
              <td>{{ line.merchantName }}</td>
              <td>{{ line.quantity }}</td>
              <td>{{ line.invoiceNumber }}</td>
              <td>☐</td>
            </tr>
          </tbody>
        </table>
        <div class="sheet-footer">
          <span>الإجمالي: {{ printSheet.totalMerchants }} تاجر</span>
          <span>توقيع السائق: _______________</span>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .setup-card { border-radius: 16px !important; padding: 24px !important; border: 1px solid #e5e7eb; }
    .card-section-title {
      display: flex; align-items: center; gap: 10px; margin-bottom: 20px;
      mat-icon { color: #667eea; } h3 { margin: 0; font-weight: 700; }
    }
    .start-btn { border-radius: 12px !important; height: 48px !important; padding: 0 32px !important;
      background: linear-gradient(135deg, #667eea, #764ba2) !important; color: #fff !important;
      font-weight: 700 !important;
    }
    .checklist-header {
      display: flex; align-items: center; justify-content: space-between;
      padding: 16px 0; flex-wrap: wrap; gap: 12px;
    }
    .counter { font-weight: 700; font-size: 1rem; color: #1a1a2e; }
    .search-box { width: 280px; }
    @media (max-width: 599px) { .search-box { width: 100%; } }
    .checklist-row {
      display: flex; align-items: flex-start; gap: 10px; padding: 12px 16px;
      border-bottom: 1px solid #f3f4f6; transition: background 0.15s ease;
      flex-wrap: wrap; min-height: 56px;
      &:hover { background: #fafbff; }
      &.selected { background: #f0fdf4; border-right: 4px solid #16a34a; }
    }
    .merchant-check { flex-shrink: 0; }
    .position-num {
      display: inline-flex; align-items: center; justify-content: center;
      width: 28px; height: 28px; border-radius: 50%; background: #f3f4f6;
      font-size: 0.8rem; font-weight: 700; color: #6b7280; flex-shrink: 0;
    }
    .selected .position-num { background: #16a34a; color: #fff; }
    .check-merchant-name { font-weight: 600; font-size: 0.95rem; }
    .check-city { font-size: 0.78rem; color: #6b7280; }
    .invoice-fields {
      display: flex; gap: 8px; width: 100%; margin-top: 8px; flex-wrap: wrap;
    }
    .inv-field { flex: 1; min-width: 140px; }
    .inv-field-sm { width: 120px; min-width: 100px; }
    @media (max-width: 599px) {
      .invoice-fields { flex-direction: column; }
      .inv-field, .inv-field-sm { min-width: 100%; width: 100%; }
    }
    .action-bar {
      display: flex; align-items: center; gap: 12px; padding: 14px 20px;
      background: #fff; border-radius: 12px; margin-top: 16px;
      box-shadow: 0 2px 8px rgba(0,0,0,0.08); flex-wrap: wrap;
      position: sticky; bottom: 0; z-index: 10;
      span { font-size: 0.9rem; font-weight: 600; }
    }
    @media (max-width: 599px) {
      .action-bar { bottom: 56px; border-radius: 0; margin: 0 -12px; padding: 10px 14px; }
    }
    .preview-panel { padding: 16px;
      h3 { font-weight: 700; margin: 0 0 4px; }
      .note { font-size: 0.82rem; color: #6b7280; margin: 0 0 16px; }
    }
    .preview-row {
      display: flex; align-items: center; gap: 10px; padding: 10px 12px;
      background: #fff; border-bottom: 1px solid #f3f4f6; cursor: move;
    }
    .seq { font-weight: 700; color: #667eea; min-width: 24px; }
    .preview-name { font-weight: 600; }
    .preview-inv, .preview-qty { font-size: 0.82rem; color: #6b7280; }
    .cdk-drag-preview { box-shadow: 0 5px 25px rgba(0,0,0,0.2); border-radius: 8px; background: #fff; }
    .cdk-drag-placeholder { opacity: 0.3; }
    .empty-state {
      text-align: center;
      mat-icon { font-size: 48px; width: 48px; height: 48px; color: #d1d5db; }
      p { color: #6b7280; }
    }
    /* Print sheets hidden on screen */
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
    /* ═══ Quick Add ═══ */
    .quick-add-container {
      position: sticky; top: 0; z-index: 100;
      background: #fafafa; padding: 12px 16px;
      border-bottom: 2px solid #e0e0e0; margin-bottom: 8px;
      border-radius: 8px 8px 0 0; box-shadow: 0 2px 8px rgba(0,0,0,0.08);
    }
    .quick-add-header {
      display: flex; align-items: center; justify-content: space-between; margin-bottom: 8px;
    }
    .quick-add-label { font-weight: 600; font-size: 14px; color: #333; }
    .quick-add-wrapper { position: relative; width: 100%; }
    .quick-add-input {
      width: 100%; padding: 10px 40px 10px 16px; font-size: 15px;
      border: 2px solid #1976d2; border-radius: 8px; outline: none;
      box-sizing: border-box; background: white; color: #333;
      transition: border-color 0.2s, box-shadow 0.2s;
    }
    .quick-add-input:focus {
      border-color: #0d47a1; box-shadow: 0 0 0 3px rgba(25, 118, 210, 0.15);
    }
    .quick-add-input::placeholder { color: #9e9e9e; font-size: 13px; }
    .quick-add-clear {
      position: absolute; left: 10px; top: 50%; transform: translateY(-50%);
      background: none; border: none; cursor: pointer; color: #9e9e9e;
      font-size: 16px; padding: 4px; line-height: 1;
    }
    .quick-add-clear:hover { color: #333; }
    .quick-add-dropdown {
      position: absolute; top: calc(100% + 4px); right: 0; left: 0;
      background: white; border: 1px solid #e0e0e0; border-radius: 8px;
      box-shadow: 0 4px 20px rgba(0,0,0,0.15); z-index: 1000;
      overflow: hidden; max-height: 320px; overflow-y: auto;
    }
    .quick-add-result {
      display: flex; align-items: center; gap: 10px; padding: 10px 16px;
      cursor: pointer; transition: background 0.15s; direction: rtl;
    }
    .quick-add-result:hover, .quick-add-result.highlighted { background: #e3f2fd; }
    .quick-add-result:not(:last-child) { border-bottom: 1px solid #f5f5f5; }
    .result-position {
      font-size: 11px; color: #9e9e9e; background: #f5f5f5;
      padding: 2px 6px; border-radius: 10px; min-width: 28px; text-align: center;
    }
    .result-name { flex: 1; font-weight: 500; font-size: 14px; color: #212121; }
    .result-city { font-size: 12px; color: #757575; }
    .result-add-hint {
      font-size: 11px; color: #1976d2; background: #e3f2fd;
      padding: 2px 8px; border-radius: 10px; white-space: nowrap;
    }
    .quick-add-no-result { padding: 16px; text-align: center; color: #9e9e9e; font-size: 13px; }
    .quick-add-hint { font-size: 11px; color: #9e9e9e; margin-top: 6px; text-align: center; }
    .checklist-row.just-added { animation: flashGreen 1.5s ease; }
    @keyframes flashGreen {
      0%   { background-color: #c8e6c9; }
      50%  { background-color: #a5d6a7; }
      100% { background-color: transparent; }
    }
    @media (max-width: 599px) {
      .quick-add-container { padding: 10px 12px; }
      .quick-add-input { font-size: 16px; padding: 12px 40px 12px 16px; }
      .quick-add-dropdown { max-height: 250px; }
      .result-add-hint { display: none; }
    }
  `]
})
export class DailyInvoicesComponent implements OnInit {
  routes: any[] = [];
  selectedRouteId: number | null = null;
  selectedDate = new Date();
  driverName = '';
  started = false;
  starting = false;
  loadingChecklist = false;
  saving = false;
  deliveryDayId: number | null = null;
  checklist: ChecklistItem[] = [];
  jumpSearch = '';
  quickAddSearch = '';
  quickAddResults: ChecklistItem[] = [];
  quickAddIndex = -1;
  showQuickResults = false;
  printSheet: any = null;
  printMode: 'loading' | 'delivery' = 'loading';

  constructor(private api: ApiService, private snack: MatSnackBar) {}

  ngOnInit() { this.api.getRoutes().subscribe(r => this.routes = r); }

  get selectedCount() { return this.checklist.filter(c => c.isSelected).length; }
  get totalAmount() { return this.checklist.filter(c => c.isSelected).reduce((s, c) => s + (c.amount || 0), 0); }
  get selectedItems() { return this.checklist.filter(c => c.isSelected); }

  startDay() {
    if (!this.selectedRouteId || !this.selectedDate) return;
    this.starting = true;
    const dateStr = this.selectedDate.toISOString().split('T')[0];

    // Create or load the delivery day
    this.api.createDeliveryDay({
      routeId: this.selectedRouteId,
      deliveryDate: this.selectedDate.toISOString(),
      assignedDriver: this.driverName || null
    }).subscribe({
      next: (day: any) => {
        this.deliveryDayId = day.deliveryDayId;
        this.loadChecklist();
      },
      error: (err) => {
        // If already exists, try loading existing days
        this.api.getDeliveryDays(dateStr, this.selectedRouteId!).subscribe({
          next: (days: any[]) => {
            if (days.length > 0) {
              this.deliveryDayId = days[0].deliveryDayId;
              this.loadChecklist();
            } else {
              this.starting = false;
              this.snack.open(err.error?.message || 'حدث خطأ', 'إغلاق', { duration: 3000 });
            }
          },
          error: () => { this.starting = false; }
        });
      }
    });
  }

  loadChecklist() {
    this.loadingChecklist = true;
    this.started = true;
    this.starting = false;

    // Load all merchants for the route
    this.api.getRouteMerchants(this.selectedRouteId!).subscribe({
      next: (merchants: any[]) => {
        // Load existing invoices for this day
        if (this.deliveryDayId) {
          this.api.getDeliveryDay(this.deliveryDayId).subscribe({
            next: (data: any) => {
              const existingInvoices: any[] = data.invoices || [];
              this.checklist = merchants.map(m => {
                const inv = existingInvoices.find((i: any) => i.routeMerchantId === m.routeMerchantId);
                return {
                  routeMerchantId: m.routeMerchantId,
                  merchantId: m.merchantId,
                  merchantName: m.merchantName,
                  city: m.city,
                  positionOrder: m.positionOrder,
                  isSelected: !!inv,
                  invoiceNumber: inv?.invoiceNumber || '',
                  quantity: inv?.quantity || '',
                  amount: inv?.amount || null,
                  notes: inv?.notes || '',
                  dayInvoiceId: inv?.dayInvoiceId || null
                };
              });
              this.loadingChecklist = false;
            },
            error: () => { this.loadingChecklist = false; }
          });
        }
      },
      error: () => { this.loadingChecklist = false; }
    });
  }

  onMerchantToggle(item: ChecklistItem) {
    if (!this.deliveryDayId) return;
    if (item.isSelected) {
      // Add invoice
      this.api.addDayInvoice(this.deliveryDayId, {
        routeMerchantId: item.routeMerchantId,
        merchantId: item.merchantId,
        invoiceNumber: item.invoiceNumber || null,
        quantity: item.quantity || null,
        amount: item.amount,
        notes: item.notes || null
      }).subscribe({
        next: (res: any) => { item.dayInvoiceId = res.dayInvoiceId; },
        error: () => { item.isSelected = false; }
      });
    } else {
      // Remove invoice
      if (item.dayInvoiceId) {
        this.api.removeDayInvoice(this.deliveryDayId, item.dayInvoiceId).subscribe({
          next: () => { item.dayInvoiceId = null; },
          error: () => { item.isSelected = true; }
        });
      }
    }
  }

  jumpToMerchant() {
    if (!this.jumpSearch) return;
    const found = this.checklist.find(c => c.merchantName.includes(this.jumpSearch));
    if (found) {
      const el = document.getElementById('merchant-' + found.routeMerchantId);
      el?.scrollIntoView({ behavior: 'smooth', block: 'center' });
    }
  }

  // ── Quick Add Search ──

  onQuickAddInput(): void {
    const query = this.quickAddSearch.trim();
    if (!query || query.length < 1) {
      this.quickAddResults = [];
      this.showQuickResults = false;
      this.quickAddIndex = -1;
      return;
    }
    this.quickAddResults = this.checklist
      .filter(c => c.merchantName.includes(query) && !c.isSelected)
      .slice(0, 8);
    this.showQuickResults = query.length > 0;
    this.quickAddIndex = -1;
  }

  selectQuickAdd(item: ChecklistItem): void {
    item.isSelected = true;
    this.onMerchantToggle(item);
    setTimeout(() => {
      const el = document.getElementById('merchant-' + item.routeMerchantId);
      if (el) {
        el.scrollIntoView({ behavior: 'smooth', block: 'center' });
        el.classList.add('just-added');
        setTimeout(() => el.classList.remove('just-added'), 1500);
      }
    }, 100);
    this.quickAddSearch = '';
    this.quickAddResults = [];
    this.showQuickResults = false;
    this.quickAddIndex = -1;
    setTimeout(() => {
      const input = document.getElementById('quickAddInput') as HTMLInputElement;
      if (input) input.focus();
    }, 150);
  }

  onQuickAddKeydown(event: KeyboardEvent): void {
    if (!this.showQuickResults || this.quickAddResults.length === 0) {
      if (event.key === 'Escape') {
        this.quickAddSearch = '';
        this.showQuickResults = false;
      }
      return;
    }
    switch (event.key) {
      case 'ArrowDown':
        event.preventDefault();
        this.quickAddIndex = Math.min(this.quickAddIndex + 1, this.quickAddResults.length - 1);
        break;
      case 'ArrowUp':
        event.preventDefault();
        this.quickAddIndex = Math.max(this.quickAddIndex - 1, -1);
        break;
      case 'Enter':
        event.preventDefault();
        if (this.quickAddIndex >= 0) {
          this.selectQuickAdd(this.quickAddResults[this.quickAddIndex]);
        } else if (this.quickAddResults.length === 1) {
          this.selectQuickAdd(this.quickAddResults[0]);
        }
        break;
      case 'Escape':
        this.quickAddSearch = '';
        this.quickAddResults = [];
        this.showQuickResults = false;
        this.quickAddIndex = -1;
        break;
    }
  }

  onQuickAddBlur(): void {
    setTimeout(() => { this.showQuickResults = false; }, 200);
  }

  onPreviewDrop(event: CdkDragDrop<any[]>) {
    const items = this.selectedItems;
    moveItemInArray(items, event.previousIndex, event.currentIndex);
  }

  saveAndPrint(mode: 'loading' | 'delivery') {
    if (!this.deliveryDayId) return;
    this.saving = true;
    this.printMode = mode;

    // Save any updated invoices first
    const selected = this.checklist.filter(c => c.isSelected);
    const saveOps = selected.filter(c => c.dayInvoiceId).map(c =>
      this.api.updateDayInvoice(this.deliveryDayId!, c.dayInvoiceId!, {
        invoiceNumber: c.invoiceNumber || null,
        quantity: c.quantity || null,
        amount: c.amount,
        notes: c.notes || null
      }).toPromise()
    );

    Promise.all(saveOps).then(() => {
      const sheetApi = mode === 'loading'
        ? this.api.getLoadingSheet(this.deliveryDayId!)
        : this.api.getDeliverySheet(this.deliveryDayId!);

      sheetApi.subscribe({
        next: (sheet) => {
          this.printSheet = sheet;
          this.saving = false;
          setTimeout(() => window.print(), 300);
        },
        error: () => {
          this.saving = false;
          this.snack.open('حدث خطأ في توليد القائمة', 'إغلاق', { duration: 3000 });
        }
      });
    });
  }
}
