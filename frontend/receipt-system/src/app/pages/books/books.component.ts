import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ApiService } from '../../core/api.service';

@Component({
  selector: 'app-books',
  standalone: true,
  imports: [
    CommonModule, FormsModule, MatCardModule, MatTableModule, MatButtonModule,
    MatIconModule, MatFormFieldModule, MatInputModule, MatSelectModule,
    MatDatepickerModule, MatNativeDateModule, MatSnackBarModule, MatProgressSpinnerModule
  ],
  template: `
    <div class="page-container">
      <div class="page-header">
        <h1>دفاتر الإيصالات</h1>
        <button mat-raised-button color="primary" (click)="openBookForm()" style="border-radius:12px !important">
          <mat-icon>add</mat-icon> إضافة دفتر
        </button>
      </div>

      <div class="loading-container" *ngIf="loading"><mat-spinner diameter="40"></mat-spinner></div>

      <mat-card class="data-table-card" *ngIf="!loading">
        <div class="desktop-table">
          <table mat-table [dataSource]="books" class="full-width">
            <ng-container matColumnDef="number"><th mat-header-cell *matHeaderCellDef>رقم الدفتر</th><td mat-cell *matCellDef="let b">{{ b.bookNumber }}</td></ng-container>
            <ng-container matColumnDef="range"><th mat-header-cell *matHeaderCellDef>من / إلى</th><td mat-cell *matCellDef="let b">{{ b.startReceiptNumber }} — {{ b.endReceiptNumber }}</td></ng-container>
            <ng-container matColumnDef="driver"><th mat-header-cell *matHeaderCellDef>السائق</th><td mat-cell *matCellDef="let b">{{ b.driverName || '—' }}</td></ng-container>
            <ng-container matColumnDef="assigned"><th mat-header-cell *matHeaderCellDef>تاريخ التسليم</th><td mat-cell *matCellDef="let b">{{ b.assignedDate ? (b.assignedDate | date:'yyyy/MM/dd') : '—' }}</td></ng-container>
            <ng-container matColumnDef="status">
              <th mat-header-cell *matHeaderCellDef>الحالة</th>
              <td mat-cell *matCellDef="let b">
                <span class="status-badge" [ngClass]="getStatusClass(b.status)">{{ getStatusLabel(b.status) }}</span>
              </td>
            </ng-container>
            <ng-container matColumnDef="actions">
              <th mat-header-cell *matHeaderCellDef>إجراءات</th>
              <td mat-cell *matCellDef="let b">
                <button mat-raised-button color="accent" (click)="openAssignForm(b)"
                        *ngIf="b.status === 'Available'" style="border-radius:8px !important; font-size:0.8rem">
                  تسليم لسائق
                </button>
              </td>
            </ng-container>
            <tr mat-header-row *matHeaderRowDef="['number','range','driver','assigned','status','actions']"></tr>
            <tr mat-row *matRowDef="let row; columns: ['number','range','driver','assigned','status','actions']"></tr>
          </table>
        </div>

        <div class="mobile-receipt-cards">
          <mat-card *ngFor="let b of books" style="margin:8px 0; padding:12px !important; border-radius:12px !important">
            <div style="display:flex; justify-content:space-between; margin-bottom:4px">
              <strong>دفتر {{ b.bookNumber }}</strong>
              <span class="status-badge" [ngClass]="getStatusClass(b.status)" style="font-size:0.7rem">{{ getStatusLabel(b.status) }}</span>
            </div>
            <div style="font-size:0.85rem; color:#6b7280">إيصالات: {{ b.startReceiptNumber }} — {{ b.endReceiptNumber }}</div>
            <div style="font-size:0.85rem" *ngIf="b.driverName">السائق: {{ b.driverName }}</div>
            <button mat-stroked-button color="primary" (click)="openAssignForm(b)"
                    *ngIf="b.status === 'Available'" style="margin-top:8px; width:100%; border-radius:8px !important">
              تسليم لسائق
            </button>
          </mat-card>
        </div>
      </mat-card>

      <!-- Add Book Dialog -->
      <div class="resolve-overlay" *ngIf="showBookForm" (click)="showBookForm=false">
        <mat-card class="resolve-dialog" (click)="$event.stopPropagation()">
          <h3>إضافة دفتر جديد</h3>
          <mat-form-field appearance="outline" class="full-width"><mat-label>رقم الدفتر</mat-label><input matInput type="number" [(ngModel)]="bookForm.bookNumber"></mat-form-field>
          <div class="form-row">
            <mat-form-field appearance="outline"><mat-label>من إيصال</mat-label><input matInput type="number" [(ngModel)]="bookForm.startReceiptNumber"></mat-form-field>
            <mat-form-field appearance="outline"><mat-label>إلى إيصال</mat-label><input matInput type="number" [(ngModel)]="bookForm.endReceiptNumber"></mat-form-field>
          </div>
          <div style="display:flex; gap:8px">
            <button mat-raised-button color="primary" (click)="saveBook()" style="border-radius:8px !important">حفظ</button>
            <button mat-button (click)="showBookForm=false">إلغاء</button>
          </div>
        </mat-card>
      </div>

      <!-- Assign Dialog -->
      <div class="resolve-overlay" *ngIf="showAssignForm" (click)="showAssignForm=false">
        <mat-card class="resolve-dialog" (click)="$event.stopPropagation()">
          <h3>تسليم دفتر {{ assigningBook?.bookNumber }} لسائق</h3>
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>اختر السائق</mat-label>
            <mat-select [(ngModel)]="assignForm.driverId">
              <mat-option *ngFor="let d of drivers" [value]="d.driverId">{{ d.fullName }}</mat-option>
            </mat-select>
          </mat-form-field>
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>تاريخ التسليم</mat-label>
            <input matInput [matDatepicker]="ap" [(ngModel)]="assignForm.assignedDate">
            <mat-datepicker-toggle matSuffix [for]="ap"></mat-datepicker-toggle>
            <mat-datepicker #ap></mat-datepicker>
          </mat-form-field>
          <div style="display:flex; gap:8px">
            <button mat-raised-button color="primary" (click)="assignBook()" style="border-radius:8px !important">تسليم</button>
            <button mat-button (click)="showAssignForm=false">إلغاء</button>
          </div>
        </mat-card>
      </div>
    </div>
  `,
  styles: [`
    .resolve-overlay { position:fixed; inset:0; background:rgba(0,0,0,0.5); z-index:1000; display:flex; align-items:center; justify-content:center; padding:16px; }
    .resolve-dialog { width:100%; max-width:450px; padding:24px !important; border-radius:16px !important; }
    .resolve-dialog h3 { margin:0 0 16px; font-weight:700; }
  `]
})
export class BooksComponent implements OnInit {
  books: any[] = [];
  drivers: any[] = [];
  loading = true;
  showBookForm = false;
  showAssignForm = false;
  assigningBook: any = null;
  bookForm = { bookNumber: null as number|null, startReceiptNumber: null as number|null, endReceiptNumber: null as number|null };
  assignForm = { driverId: null as number|null, assignedDate: new Date() };

  constructor(private api: ApiService, private snack: MatSnackBar) {}

  ngOnInit() {
    this.api.getDrivers().subscribe(d => this.drivers = d);
    this.load();
  }

  load() {
    this.loading = true;
    this.api.getBooks().subscribe({ next: (b) => { this.books = b; this.loading = false; }, error: () => this.loading = false });
  }

  getStatusClass(s: string): string {
    const map: any = { Available: 'available', Assigned: 'assigned', InProgress: 'in-progress', Completed: 'completed' };
    return map[s] || '';
  }

  getStatusLabel(s: string): string {
    const map: any = { Available: 'متاح', Assigned: 'مُسلَّم', InProgress: 'قيد الاستخدام', Completed: 'مكتمل' };
    return map[s] || s;
  }

  openBookForm() { this.bookForm = { bookNumber: null, startReceiptNumber: null, endReceiptNumber: null }; this.showBookForm = true; }

  saveBook() {
    if (!this.bookForm.bookNumber || !this.bookForm.startReceiptNumber || !this.bookForm.endReceiptNumber) {
      this.snack.open('أكمل جميع البيانات', 'إغلاق', { duration: 3000 }); return;
    }
    this.api.createBook(this.bookForm).subscribe({
      next: () => { this.snack.open('تم إضافة الدفتر ✅', 'إغلاق', { duration: 3000 }); this.showBookForm = false; this.load(); },
      error: (e) => this.snack.open(e.error?.message || 'حدث خطأ', 'إغلاق', { duration: 3000 })
    });
  }

  openAssignForm(book: any) {
    this.assigningBook = book;
    this.assignForm = { driverId: null, assignedDate: new Date() };
    this.showAssignForm = true;
  }

  assignBook() {
    if (!this.assignForm.driverId) { this.snack.open('اختر السائق', 'إغلاق', { duration: 3000 }); return; }
    this.api.assignBook(this.assigningBook.bookId, this.assignForm).subscribe({
      next: () => { this.snack.open('تم تسليم الدفتر ✅', 'إغلاق', { duration: 3000 }); this.showAssignForm = false; this.load(); },
      error: () => this.snack.open('حدث خطأ', 'إغلاق', { duration: 3000 })
    });
  }
}
