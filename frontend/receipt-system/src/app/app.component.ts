import { Component, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, NavigationEnd } from '@angular/router';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { MatSidenavModule, MatSidenav } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatBadgeModule } from '@angular/material/badge';
import { MatMenuModule } from '@angular/material/menu';
import { Subscription, filter, interval } from 'rxjs';
import { AuthService } from './core/auth.service';
import { ApiService } from './core/api.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule, RouterModule, MatSidenavModule, MatToolbarModule,
    MatListModule, MatIconModule, MatButtonModule, MatBadgeModule, MatMenuModule
  ],
  template: `
    <ng-container *ngIf="auth.isLoggedIn; else loginView">
      <mat-sidenav-container class="app-container">
        <mat-sidenav #sidenav [mode]="isMobile ? 'over' : 'side'"
                     [opened]="!isMobile" position="end"
                     class="app-sidenav">
          <div class="sidenav-header">
            <div class="logo-icon">📋</div>
            <div class="logo-text">نظام الإيصالات</div>
            <div class="company-name">شركه البسطاوي للتجاره والتوزيع</div>
          </div>
          <mat-nav-list class="sidenav-list">
            <a mat-list-item routerLink="/dashboard" routerLinkActive="active-link"
               (click)="isMobile && sidenav.close()" class="nav-item">
              <mat-icon class="nav-item-icon">dashboard</mat-icon>
              <span class="nav-item-text">لوحة التحكم</span>
            </a>
            <a mat-list-item routerLink="/sessions/new" routerLinkActive="active-link"
               (click)="isMobile && sidenav.close()" class="nav-item">
              <mat-icon class="nav-item-icon">add_circle</mat-icon>
              <span class="nav-item-text">جلسة جديدة</span>
            </a>
            <a mat-list-item routerLink="/sessions" routerLinkActive="active-link"
               [routerLinkActiveOptions]="{exact: true}"
               (click)="isMobile && sidenav.close()" class="nav-item">
              <mat-icon class="nav-item-icon">receipt_long</mat-icon>
              <span class="nav-item-text">الجلسات</span>
            </a>
            <a mat-list-item routerLink="/gaps" routerLinkActive="active-link"
               (click)="isMobile && sidenav.close()" class="nav-item">
              <mat-icon class="nav-item-icon">warning</mat-icon>
              <span class="nav-item-text">إيصالات مفقودة</span>
              <span class="gap-count" *ngIf="gapCount > 0">{{ gapCount }}</span>
            </a>
            <a mat-list-item routerLink="/reports" routerLinkActive="active-link"
               (click)="isMobile && sidenav.close()" class="nav-item">
              <mat-icon class="nav-item-icon">print</mat-icon>
              <span class="nav-item-text">التقارير</span>
            </a>
            <a mat-list-item routerLink="/sessions/excel-import" routerLinkActive="active-link"
               (click)="isMobile && sidenav.close()" class="nav-item">
              <mat-icon class="nav-item-icon">upload_file</mat-icon>
              <span class="nav-item-text">رفع ملف Excel</span>
            </a>
            <a mat-list-item routerLink="/search" routerLinkActive="active-link"
               (click)="isMobile && sidenav.close()" class="nav-item">
              <mat-icon class="nav-item-icon">search</mat-icon>
              <span class="nav-item-text">البحث المتقدم</span>
            </a>

            <div class="nav-divider"></div>

            <a mat-list-item routerLink="/drivers" routerLinkActive="active-link"
               (click)="isMobile && sidenav.close()" class="nav-item">
              <mat-icon class="nav-item-icon">person</mat-icon>
              <span class="nav-item-text">السائقون</span>
            </a>
            <a mat-list-item routerLink="/merchants" routerLinkActive="active-link"
               (click)="isMobile && sidenav.close()" class="nav-item">
              <mat-icon class="nav-item-icon">store</mat-icon>
              <span class="nav-item-text">التجار</span>
            </a>
            <a mat-list-item routerLink="/books" routerLinkActive="active-link"
               (click)="isMobile && sidenav.close()" class="nav-item">
              <mat-icon class="nav-item-icon">menu_book</mat-icon>
              <span class="nav-item-text">دفاتر الإيصالات</span>
            </a>

            <div class="nav-divider"></div>
            <div class="nav-section-label">📦 نظام الخطوط</div>

            <a mat-list-item routerLink="/routes" routerLinkActive="active-link"
               (click)="isMobile && sidenav.close()" class="nav-item">
              <mat-icon class="nav-item-icon">alt_route</mat-icon>
              <span class="nav-item-text">الخطوط والتجار</span>
            </a>
            <a mat-list-item routerLink="/daily-invoices" routerLinkActive="active-link"
               (click)="isMobile && sidenav.close()" class="nav-item">
              <mat-icon class="nav-item-icon">fact_check</mat-icon>
              <span class="nav-item-text">فواتير اليوم</span>
            </a>
            <a mat-list-item routerLink="/route-history" routerLinkActive="active-link"
               (click)="isMobile && sidenav.close()" class="nav-item">
              <mat-icon class="nav-item-icon">history</mat-icon>
              <span class="nav-item-text">سجل الخطوط</span>
            </a>

            <div class="nav-divider"></div>
            <div class="nav-section-label">📒 دفتر الآجل</div>

            <a mat-list-item routerLink="/ajal/daily" routerLinkActive="active-link"
               (click)="isMobile && sidenav.close()" class="nav-item">
              <mat-icon class="nav-item-icon">auto_stories</mat-icon>
              <span class="nav-item-text">السجل اليومي</span>
            </a>
            <a mat-list-item routerLink="/ajal/entry" routerLinkActive="active-link"
               (click)="isMobile && sidenav.close()" class="nav-item">
              <mat-icon class="nav-item-icon">edit_note</mat-icon>
              <span class="nav-item-text">إدخال فواتير</span>
            </a>
            <a mat-list-item routerLink="/ajal/excel-import" routerLinkActive="active-link"
               (click)="isMobile && sidenav.close()" class="nav-item">
              <mat-icon class="nav-item-icon">upload_file</mat-icon>
              <span class="nav-item-text">رفع Excel</span>
            </a>
            <a mat-list-item routerLink="/ajal/merchant-search" routerLinkActive="active-link"
               (click)="isMobile && sidenav.close()" class="nav-item">
              <mat-icon class="nav-item-icon">person_search</mat-icon>
              <span class="nav-item-text">بحث بالتاجر</span>
            </a>
            <a mat-list-item routerLink="/ajal/employees" routerLinkActive="active-link"
               (click)="isMobile && sidenav.close()" class="nav-item">
              <mat-icon class="nav-item-icon">leaderboard</mat-icon>
              <span class="nav-item-text">أداء الموظفين</span>
            </a>
            <a mat-list-item routerLink="/ajal/settings" routerLinkActive="active-link"
               (click)="isMobile && sidenav.close()" class="nav-item">
              <mat-icon class="nav-item-icon">settings</mat-icon>
              <span class="nav-item-text">إعدادات البادئة</span>
            </a>
          </mat-nav-list>

          <div class="sidenav-footer">
            <button mat-button (click)="auth.logout()" class="logout-btn">
              <mat-icon>logout</mat-icon> تسجيل خروج
            </button>
          </div>
        </mat-sidenav>

        <mat-sidenav-content>
          <mat-toolbar class="app-toolbar">
            <button *ngIf="isMobile" mat-icon-button (click)="sidenav.toggle()">
              <mat-icon>menu</mat-icon>
            </button>
            <span class="toolbar-title">نظام إدارة الإيصالات</span>
            <span class="spacer"></span>
            <button mat-icon-button routerLink="/gaps"
                    [matBadge]="gapCount > 0 ? gapCount : null" matBadgeColor="warn" matBadgeSize="small">
              <mat-icon>notification_important</mat-icon>
            </button>
            <button mat-icon-button [matMenuTriggerFor]="userMenu">
              <mat-icon>account_circle</mat-icon>
            </button>
            <mat-menu #userMenu="matMenu">
              <div class="user-menu-header" mat-menu-item disabled>
                <strong>{{ auth.currentUser?.fullName }}</strong>
              </div>
              <button mat-menu-item (click)="auth.logout()">
                <mat-icon>logout</mat-icon> تسجيل خروج
              </button>
            </mat-menu>
          </mat-toolbar>

          <main class="main-content" [class.mobile-padded]="isMobile">
            <router-outlet></router-outlet>
          </main>
        </mat-sidenav-content>
      </mat-sidenav-container>

      <!-- Mobile bottom nav -->
      <nav class="bottom-nav" *ngIf="isMobile">
        <a routerLink="/dashboard" routerLinkActive="active">
          <mat-icon class="nav-icon">dashboard</mat-icon>
          <span>رئيسي</span>
        </a>
        <a routerLink="/sessions" routerLinkActive="active" [routerLinkActiveOptions]="{exact: true}">
          <mat-icon class="nav-icon">receipt_long</mat-icon>
          <span>جلسات</span>
        </a>
        <a routerLink="/sessions/new" class="fab-btn">
          <mat-icon>add</mat-icon>
        </a>
        <a routerLink="/gaps" routerLinkActive="active">
          <mat-icon class="nav-icon">warning</mat-icon>
          <span>مفقودة</span>
        </a>
        <a routerLink="/reports" routerLinkActive="active">
          <mat-icon class="nav-icon">print</mat-icon>
          <span>تقارير</span>
        </a>
      </nav>
    </ng-container>

    <ng-template #loginView>
      <router-outlet></router-outlet>
    </ng-template>
  `,
  styles: [`
    .app-container { height: 100vh; }

    .app-sidenav {
      width: 270px;
      background: linear-gradient(180deg, #1a1a2e 0%, #16213e 50%, #0f3460 100%);
      border-left: none;
      overflow-x: hidden;
    }

    .sidenav-header {
      padding: 28px 20px 20px;
      text-align: center;
      border-bottom: 1px solid rgba(255,255,255,0.12);
    }

    .logo-icon {
      font-size: 2rem;
      margin-bottom: 8px;
    }

    .logo-text {
      font-size: 1.35rem;
      font-weight: 800;
      color: #fff;
      letter-spacing: -0.5px;
    }

    .company-name {
      font-size: 0.72rem;
      color: rgba(255,255,255,0.55);
      margin-top: 6px;
    }

    /* ── Fix sidebar nav items text clipping ── */
    .sidenav-list {
      padding: 8px 0 70px;
    }

    .nav-item {
      margin: 3px 10px !important;
      border-radius: 10px !important;
      height: 48px !important;
      transition: all 0.2s ease !important;
    }

    .nav-item-icon {
      color: rgba(255,255,255,0.7) !important;
      margin-left: 12px !important;
      font-size: 22px !important;
      width: 22px !important;
      height: 22px !important;
    }

    .nav-item-text {
      color: rgba(255,255,255,0.85) !important;
      font-size: 0.92rem !important;
      font-weight: 600 !important;
      white-space: nowrap !important;
      overflow: visible !important;
      text-overflow: unset !important;
    }

    .nav-item:hover {
      background: rgba(255,255,255,0.1) !important;
    }

    .nav-item:hover .nav-item-icon,
    .nav-item:hover .nav-item-text {
      color: #fff !important;
    }

    .nav-item.active-link {
      background: rgba(102, 126, 234, 0.35) !important;
    }

    .nav-item.active-link .nav-item-icon,
    .nav-item.active-link .nav-item-text {
      color: #fff !important;
    }

    .gap-count {
      background: #dc2626;
      color: #fff;
      padding: 2px 8px;
      border-radius: 12px;
      font-size: 0.72rem;
      font-weight: 700;
      margin-right: auto;
      margin-left: 8px;
    }

    .nav-divider {
      height: 1px;
      background: rgba(255,255,255,0.1);
      margin: 10px 20px;
    }

    .nav-section-label {
      padding: 6px 20px 4px;
      font-size: 0.72rem;
      font-weight: 700;
      color: rgba(255,255,255,0.4);
      letter-spacing: 0.5px;
    }

    .sidenav-footer {
      position: absolute;
      bottom: 0;
      width: 100%;
      padding: 14px;
      border-top: 1px solid rgba(255,255,255,0.1);
      text-align: center;
    }

    .logout-btn {
      color: rgba(255,255,255,0.6) !important;
      font-size: 0.85rem !important;
      width: 100%;
    }

    .logout-btn:hover {
      color: #f87171 !important;
    }

    .app-toolbar {
      background: #fff;
      color: #1a1a2e;
      box-shadow: 0 1px 4px rgba(0,0,0,0.08);
      position: sticky;
      top: 0;
      z-index: 10;
    }

    .toolbar-title {
      font-weight: 700;
      font-size: 1.1rem;
    }

    .main-content {
      min-height: calc(100vh - 64px);
      background: #f0f2f5;
    }

    .main-content.mobile-padded {
      padding-bottom: 60px;
    }

    .user-menu-header {
      padding: 8px 16px;
      opacity: 1 !important;
    }
  `]
})
export class AppComponent implements OnInit, OnDestroy {
  @ViewChild('sidenav') sidenav!: MatSidenav;
  isMobile = false;
  gapCount = 0;
  private subs: Subscription[] = [];

  constructor(
    public auth: AuthService,
    private api: ApiService,
    private breakpoint: BreakpointObserver,
    private router: Router
  ) {}

  ngOnInit() {
    this.subs.push(
      this.breakpoint.observe(['(max-width: 599px)']).subscribe(result => {
        this.isMobile = result.matches;
      })
    );

    // Load gap count
    this.loadGapCount();

    // Refresh gap count every 60 seconds
    this.subs.push(interval(60000).subscribe(() => this.loadGapCount()));

    // Refresh on navigation
    this.subs.push(
      this.router.events.pipe(filter(e => e instanceof NavigationEnd)).subscribe(() => {
        this.loadGapCount();
      })
    );
  }

  loadGapCount() {
    if (!this.auth.isLoggedIn) return;
    this.api.getGapSummary().subscribe({
      next: (s: any) => this.gapCount = (s.openCount || 0) + (s.underInvestigationCount || 0),
      error: () => {}
    });
  }

  ngOnDestroy() {
    this.subs.forEach(s => s.unsubscribe());
  }
}
