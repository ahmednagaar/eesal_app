import { Routes } from '@angular/router';
import { authGuard } from './core/auth.guard';

export const routes: Routes = [
  { path: 'login', loadComponent: () => import('./pages/login/login.component').then(m => m.LoginComponent) },
  {
    path: '',
    canActivate: [authGuard],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', loadComponent: () => import('./pages/dashboard/dashboard.component').then(m => m.DashboardComponent) },
      { path: 'sessions/new', loadComponent: () => import('./pages/new-session/new-session.component').then(m => m.NewSessionComponent) },
      { path: 'sessions', loadComponent: () => import('./pages/sessions/sessions.component').then(m => m.SessionsComponent) },
      { path: 'gaps', loadComponent: () => import('./pages/gaps/gaps.component').then(m => m.GapsComponent) },
      { path: 'reports', loadComponent: () => import('./pages/reports/reports.component').then(m => m.ReportsComponent) },
      { path: 'drivers', loadComponent: () => import('./pages/drivers/drivers.component').then(m => m.DriversComponent) },
      { path: 'merchants', loadComponent: () => import('./pages/merchants/merchants.component').then(m => m.MerchantsComponent) },
      { path: 'books', loadComponent: () => import('./pages/books/books.component').then(m => m.BooksComponent) },
      // Module 2: Routes & Loading Sheets
      { path: 'routes', loadComponent: () => import('./pages/route-setup/route-setup.component').then(m => m.RouteSetupComponent) },
      { path: 'daily-invoices', loadComponent: () => import('./pages/daily-invoices/daily-invoices.component').then(m => m.DailyInvoicesComponent) },
      { path: 'route-history', loadComponent: () => import('./pages/route-history/route-history.component').then(m => m.RouteHistoryComponent) },
      // Module 3: Excel Import & Search
      { path: 'sessions/excel-import', loadComponent: () => import('./pages/excel-import/excel-import.component').then(m => m.ExcelImportComponent) },
      { path: 'search', loadComponent: () => import('./pages/search/search.component').then(m => m.SearchComponent) },
      // Module 4: Ajal Register (دفتر الآجل)
      { path: 'ajal/daily', loadComponent: () => import('./pages/ajal-daily/ajal-daily.component').then(m => m.AjalDailyComponent) },
      { path: 'ajal/entry', loadComponent: () => import('./pages/ajal-entry/ajal-entry.component').then(m => m.AjalEntryComponent) },
      { path: 'ajal/excel-import', loadComponent: () => import('./pages/ajal-excel-import/ajal-excel-import.component').then(m => m.AjalExcelImportComponent) },
      { path: 'ajal/merchant-search', loadComponent: () => import('./pages/ajal-merchant-search/ajal-merchant-search.component').then(m => m.AjalMerchantSearchComponent) },
      { path: 'ajal/employees', loadComponent: () => import('./pages/ajal-employees/ajal-employees.component').then(m => m.AjalEmployeesComponent) },
      { path: 'ajal/settings', loadComponent: () => import('./pages/ajal-settings/ajal-settings.component').then(m => m.AjalSettingsComponent) },
    ]
  },
  { path: '**', redirectTo: 'dashboard' }
];
