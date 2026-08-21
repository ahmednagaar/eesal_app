import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CdkDragDrop, DragDropModule, moveItemInArray } from '@angular/cdk/drag-drop';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Subject, debounceTime, switchMap } from 'rxjs';
import { ApiService } from '../../core/api.service';

@Component({
  selector: 'app-route-setup',
  standalone: true,
  imports: [
    CommonModule, FormsModule, DragDropModule, MatCardModule, MatFormFieldModule,
    MatInputModule, MatButtonModule, MatIconModule, MatListModule, MatDialogModule,
    MatAutocompleteModule, MatSnackBarModule, MatProgressSpinnerModule, MatTooltipModule
  ],
  template: `
    <div class="page-container">
      <div class="page-header">
        <h1>إعداد الخطوط والتجار</h1>
        <button mat-raised-button color="primary" (click)="showAddRoute = true"
                style="border-radius:12px !important">
          <mat-icon>add</mat-icon> إضافة خط جديد
        </button>
      </div>

      <div class="loading-container" *ngIf="loading"><mat-spinner diameter="40"></mat-spinner></div>

      <div class="route-layout" *ngIf="!loading">
        <!-- Routes Panel -->
        <mat-card class="routes-panel">
          <div class="panel-title">
            <mat-icon>map</mat-icon>
            <h3>الخطوط</h3>
          </div>

          <!-- Add Route Inline -->
          <div class="add-route-form" *ngIf="showAddRoute">
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>اسم الخط الجديد</mat-label>
              <input matInput [(ngModel)]="newRouteName" placeholder="مثال: المنشية" id="new-route-name">
            </mat-form-field>
            <div style="display:flex; gap:8px">
              <button mat-raised-button color="primary" (click)="addRoute()"
                      [disabled]="!newRouteName" style="border-radius:8px !important">
                <mat-icon>check</mat-icon> إضافة
              </button>
              <button mat-stroked-button (click)="showAddRoute = false; newRouteName = ''"
                      style="border-radius:8px !important">
                إلغاء
              </button>
            </div>
          </div>

          <mat-nav-list class="route-list">
            <a mat-list-item *ngFor="let r of routes" (click)="selectRoute(r)"
               [class.active-route]="selectedRoute?.routeId === r.routeId"
               class="route-item">
              <div class="route-item-content">
                <span class="route-name">{{ r.routeName }}</span>
                <span class="merchant-count">{{ r.merchantCount }} تاجر</span>
              </div>
            </a>
          </mat-nav-list>

          <div *ngIf="routes.length === 0" class="empty-state" style="padding:24px">
            <mat-icon>route</mat-icon>
            <p>لا توجد خطوط بعد</p>
          </div>
        </mat-card>

        <!-- Merchants Panel -->
        <mat-card class="merchants-panel" *ngIf="selectedRoute">
          <div class="panel-title">
            <mat-icon>store</mat-icon>
            <h3>{{ selectedRoute.routeName }}</h3>
            <span class="spacer"></span>
            <button mat-raised-button color="primary" (click)="showAddMerchant = true"
                    style="border-radius:8px !important">
              <mat-icon>person_add</mat-icon> إضافة تاجر
            </button>
          </div>

          <!-- Search in route -->
          <mat-form-field appearance="outline" class="full-width" style="margin-bottom:8px">
            <mat-label>بحث في تجار الخط</mat-label>
            <input matInput [(ngModel)]="merchantFilter" placeholder="اكتب اسم التاجر...">
            <mat-icon matSuffix>search</mat-icon>
          </mat-form-field>

          <!-- Add Merchant Dialog Inline -->
          <mat-card class="add-merchant-card" *ngIf="showAddMerchant">
            <h4>إضافة تاجر إلى خط {{ selectedRoute.routeName }}</h4>
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>ابحث عن التاجر</mat-label>
              <input matInput [matAutocomplete]="merchantAuto"
                     [(ngModel)]="merchantSearchInput"
                     (input)="searchMerchants()"
                     placeholder="اكتب أول حروف اسم التاجر...">
              <mat-autocomplete #merchantAuto (optionSelected)="onMerchantSelected($event)">
                <mat-option *ngFor="let m of merchantResults" [value]="m.merchantName">
                  {{ m.merchantName }} <small *ngIf="m.city">— {{ m.city }}</small>
                </mat-option>
              </mat-autocomplete>
            </mat-form-field>
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>ضعه في الترتيب رقم</mat-label>
              <input matInput type="number" [(ngModel)]="insertPosition"
                     [min]="1" [max]="routeMerchants.length + 1">
              <mat-hint>يوجد {{ routeMerchants.length }} تاجر حالياً في هذا الخط</mat-hint>
            </mat-form-field>
            <p class="hint-text" *ngIf="insertPosition && selectedMerchantId">
              💡 التاجر سيُدرج في الموضع {{ insertPosition }}
              وسيتم تحريك باقي التجار تلقائياً
            </p>
            <div style="display:flex; gap:8px; margin-top:12px">
              <button mat-raised-button color="primary" (click)="addMerchantToRoute()"
                      [disabled]="!selectedMerchantId || !insertPosition"
                      style="border-radius:8px !important">
                <mat-icon>check</mat-icon> إضافة
              </button>
              <button mat-stroked-button (click)="cancelAddMerchant()"
                      style="border-radius:8px !important">
                إلغاء
              </button>
            </div>
          </mat-card>

          <!-- Drag and Drop Merchant List -->
          <div cdkDropList (cdkDropListDropped)="onDrop($event)" class="merchant-drop-list"
               *ngIf="routeMerchants.length > 0">
            <div class="merchant-row" *ngFor="let rm of filteredMerchants; let i = index" cdkDrag
                 [cdkDragData]="rm">
              <mat-icon cdkDragHandle class="drag-handle">drag_indicator</mat-icon>
              <span class="position-badge">{{ rm.positionOrder }}</span>
              <span class="merchant-name-text">{{ rm.merchantName }}</span>
              <span class="merchant-city" *ngIf="rm.city">{{ rm.city }}</span>
              <span class="spacer"></span>
              <button mat-icon-button color="warn" (click)="removeMerchant(rm)"
                      matTooltip="حذف من الخط">
                <mat-icon>delete_outline</mat-icon>
              </button>
            </div>
          </div>

          <div *ngIf="routeMerchants.length === 0 && !loadingMerchants" class="empty-state" style="padding:24px">
            <mat-icon>person_off</mat-icon>
            <p>لا يوجد تجار في هذا الخط بعد</p>
          </div>

          <div *ngIf="loadingMerchants" class="loading-container"><mat-spinner diameter="30"></mat-spinner></div>
        </mat-card>

        <!-- No route selected -->
        <mat-card class="merchants-panel" *ngIf="!selectedRoute && routes.length > 0">
          <div class="empty-state" style="padding:48px">
            <mat-icon style="font-size:64px;width:64px;height:64px">touch_app</mat-icon>
            <p>اختر خط من القائمة لعرض التجار</p>
          </div>
        </mat-card>
      </div>
    </div>
  `,
  styles: [`
    .route-layout {
      display: grid;
      grid-template-columns: 300px 1fr;
      gap: 20px;
      min-height: 500px;
    }

    @media (max-width: 959px) {
      .route-layout { grid-template-columns: 1fr; }
    }

    .routes-panel, .merchants-panel {
      border-radius: 16px !important;
      padding: 20px !important;
    }

    .panel-title {
      display: flex;
      align-items: center;
      gap: 10px;
      margin-bottom: 16px;
      mat-icon { color: #667eea; font-size: 24px; }
      h3 { margin: 0; font-weight: 700; font-size: 1.1rem; color: #1a1a2e; }
    }

    .add-route-form {
      padding: 12px;
      background: #f8f9fa;
      border-radius: 12px;
      margin-bottom: 12px;
    }

    .route-item {
      border-radius: 10px !important;
      margin: 3px 0 !important;
      transition: all 0.2s ease !important;
    }

    .route-item-content {
      display: flex;
      justify-content: space-between;
      align-items: center;
      width: 100%;
    }

    .route-name { font-weight: 600; font-size: 0.95rem; }

    .merchant-count {
      font-size: 0.78rem;
      color: #6b7280;
      background: #f3f4f6;
      padding: 2px 8px;
      border-radius: 10px;
    }

    .active-route {
      background: rgba(102, 126, 234, 0.12) !important;
      .route-name { color: #667eea; }
    }

    .add-merchant-card {
      padding: 16px !important;
      margin-bottom: 16px;
      border-radius: 12px !important;
      border: 2px dashed #667eea;
      background: #f0f4ff;
      h4 { margin: 0 0 12px; font-weight: 700; color: #1a1a2e; }
    }

    .hint-text {
      font-size: 0.85rem;
      color: #667eea;
      background: #dbeafe;
      padding: 8px 12px;
      border-radius: 8px;
    }

    .merchant-drop-list {
      border-radius: 10px;
      overflow: hidden;
    }

    .merchant-row {
      display: flex;
      align-items: center;
      gap: 10px;
      padding: 10px 12px;
      background: #fff;
      border-bottom: 1px solid #f3f4f6;
      cursor: move;
      transition: all 0.2s ease;

      &:hover { background: #f0f7ff; }
    }

    .drag-handle {
      color: #d1d5db;
      cursor: grab;
      &:active { cursor: grabbing; }
    }

    .position-badge {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      width: 30px;
      height: 30px;
      border-radius: 50%;
      background: linear-gradient(135deg, #667eea, #764ba2);
      color: #fff;
      font-size: 0.8rem;
      font-weight: 700;
      flex-shrink: 0;
    }

    .merchant-name-text { font-weight: 600; font-size: 0.92rem; }
    .merchant-city { font-size: 0.78rem; color: #6b7280; }

    .empty-state {
      text-align: center;
      mat-icon { font-size: 48px; width: 48px; height: 48px; color: #d1d5db; }
      p { color: #6b7280; margin: 12px 0; }
    }

    /* CDK Drag styles */
    .cdk-drag-preview {
      box-shadow: 0 5px 25px rgba(0,0,0,0.2);
      border-radius: 8px;
      background: #fff;
    }
    .cdk-drag-placeholder { opacity: 0.3; }
    .cdk-drag-animating { transition: transform 250ms cubic-bezier(0, 0, 0.2, 1); }
  `]
})
export class RouteSetupComponent implements OnInit {
  routes: any[] = [];
  selectedRoute: any = null;
  routeMerchants: any[] = [];
  merchantFilter = '';
  loading = true;
  loadingMerchants = false;

  // Add route
  showAddRoute = false;
  newRouteName = '';

  // Add merchant
  showAddMerchant = false;
  merchantSearchInput = '';
  merchantResults: any[] = [];
  selectedMerchantId: number | null = null;
  insertPosition: number | null = null;

  private searchSubject = new Subject<string>();

  constructor(private api: ApiService, private snack: MatSnackBar) {}

  ngOnInit() {
    this.loadRoutes();
    this.searchSubject.pipe(
      debounceTime(300),
      switchMap(q => this.api.searchMerchants(q))
    ).subscribe(m => this.merchantResults = m);
  }

  loadRoutes() {
    this.loading = true;
    this.api.getRoutes().subscribe({
      next: (r) => { this.routes = r; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  selectRoute(route: any) {
    this.selectedRoute = route;
    this.loadRouteMerchants();
  }

  loadRouteMerchants() {
    if (!this.selectedRoute) return;
    this.loadingMerchants = true;
    this.api.getRouteMerchants(this.selectedRoute.routeId).subscribe({
      next: (m) => { this.routeMerchants = m; this.loadingMerchants = false; },
      error: () => { this.loadingMerchants = false; }
    });
  }

  get filteredMerchants() {
    if (!this.merchantFilter) return this.routeMerchants;
    return this.routeMerchants.filter(rm =>
      rm.merchantName.includes(this.merchantFilter) ||
      (rm.city && rm.city.includes(this.merchantFilter))
    );
  }

  addRoute() {
    if (!this.newRouteName) return;
    this.api.createRoute({ routeName: this.newRouteName }).subscribe({
      next: () => {
        this.snack.open('تم إضافة الخط بنجاح ✅', 'إغلاق', { duration: 3000 });
        this.newRouteName = '';
        this.showAddRoute = false;
        this.loadRoutes();
      },
      error: (err) => this.snack.open(err.error?.message || 'حدث خطأ', 'إغلاق', { duration: 3000 })
    });
  }

  // ── Add Merchant ──
  searchMerchants() {
    if (this.merchantSearchInput && this.merchantSearchInput.length >= 1)
      this.searchSubject.next(this.merchantSearchInput);
  }

  onMerchantSelected(event: any) {
    const m = this.merchantResults.find(x => x.merchantName === event.option.value);
    if (m) {
      this.selectedMerchantId = m.merchantId;
      this.insertPosition = this.routeMerchants.length + 1;
    }
  }

  addMerchantToRoute() {
    if (!this.selectedMerchantId || !this.insertPosition || !this.selectedRoute) return;
    this.api.addMerchantToRoute(this.selectedRoute.routeId, {
      merchantId: this.selectedMerchantId,
      position: this.insertPosition
    }).subscribe({
      next: () => {
        this.snack.open('تم إضافة التاجر للخط بنجاح ✅', 'إغلاق', { duration: 3000 });
        this.cancelAddMerchant();
        this.loadRouteMerchants();
        this.loadRoutes();
      },
      error: (err) => this.snack.open(err.error?.message || 'حدث خطأ', 'إغلاق', { duration: 3000 })
    });
  }

  cancelAddMerchant() {
    this.showAddMerchant = false;
    this.merchantSearchInput = '';
    this.selectedMerchantId = null;
    this.insertPosition = null;
    this.merchantResults = [];
  }

  removeMerchant(rm: any) {
    if (!confirm(`حذف "${rm.merchantName}" من الخط؟`)) return;
    this.api.removeMerchantFromRoute(this.selectedRoute.routeId, rm.routeMerchantId).subscribe({
      next: () => {
        this.snack.open('تم حذف التاجر من الخط ✅', 'إغلاق', { duration: 3000 });
        this.loadRouteMerchants();
        this.loadRoutes();
      },
      error: (err) => this.snack.open(err.error?.message || 'حدث خطأ', 'إغلاق', { duration: 3000 })
    });
  }

  onDrop(event: CdkDragDrop<any[]>) {
    if (event.previousIndex === event.currentIndex) return;
    const item = this.routeMerchants[event.previousIndex];
    const newPos = this.routeMerchants[event.currentIndex].positionOrder;

    moveItemInArray(this.routeMerchants, event.previousIndex, event.currentIndex);

    this.api.updateMerchantPosition(this.selectedRoute.routeId, item.routeMerchantId, {
      newPosition: newPos
    }).subscribe({
      next: () => {
        this.snack.open('تم تحديث الترتيب ✅', 'إغلاق', { duration: 2000 });
        this.loadRouteMerchants();
      },
      error: () => {
        this.snack.open('حدث خطأ في تحديث الترتيب', 'إغلاق', { duration: 3000 });
        this.loadRouteMerchants();
      }
    });
  }
}
