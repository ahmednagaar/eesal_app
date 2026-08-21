import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';

@Component({
  selector: 'app-gap-alert-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule, MatIconModule, MatListModule],
  template: `
    <div class="gap-alert-header">⚠️ تحذير: إيصالات مفقودة!</div>
    <mat-dialog-content>
      <p>تم اكتشاف الإيصالات التالية مفقودة للسائق <strong>{{ data.driverName }}</strong>:</p>
      <mat-list>
        <mat-list-item *ngFor="let num of data.missingReceipts">
          <mat-icon matListItemIcon color="warn">warning</mat-icon>
          <span matListItemTitle>الإيصال رقم <strong>{{ num }}</strong> — مفقود</span>
        </mat-list-item>
      </mat-list>
      <div class="alert-note">
        يجب مراجعة السائق وحل هذه المشكلة في صفحة الإيصالات المفقودة.
      </div>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-raised-button color="warn" (click)="close()" style="border-radius:12px !important">
        حسناً — سأتابع الأمر
      </button>
    </mat-dialog-actions>
  `
})
export class GapAlertDialogComponent {
  constructor(
    @Inject(MAT_DIALOG_DATA) public data: { missingReceipts: number[]; driverName: string },
    private ref: MatDialogRef<GapAlertDialogComponent>
  ) {}
  close() { this.ref.close(); }
}
