import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

const API = environment.apiUrl;

@Injectable({ providedIn: 'root' })
export class ApiService {
  constructor(private http: HttpClient) {}

  // ── Dashboard ──
  getDashboard(): Observable<any> { return this.http.get(`${API}/reports/dashboard`); }

  // ── Drivers ──
  getDrivers(): Observable<any[]> { return this.http.get<any[]>(`${API}/drivers`); }
  getDriver(id: number): Observable<any> { return this.http.get(`${API}/drivers/${id}`); }
  createDriver(data: any): Observable<any> { return this.http.post(`${API}/drivers`, data); }
  updateDriver(id: number, data: any): Observable<any> { return this.http.put(`${API}/drivers/${id}`, data); }
  deleteDriver(id: number): Observable<any> { return this.http.delete(`${API}/drivers/${id}`); }

  // ── Merchants ──
  getMerchants(page = 1, pageSize = 20): Observable<any> {
    return this.http.get(`${API}/merchants`, { params: { page, pageSize } });
  }
  searchMerchants(q: string): Observable<any[]> {
    return this.http.get<any[]>(`${API}/merchants/search`, { params: { q } });
  }
  createMerchant(data: any): Observable<any> { return this.http.post(`${API}/merchants`, data); }
  updateMerchant(id: number, data: any): Observable<any> { return this.http.put(`${API}/merchants/${id}`, data); }

  // ── Books ──
  getBooks(): Observable<any[]> { return this.http.get<any[]>(`${API}/books`); }
  getAvailableBooks(): Observable<any[]> { return this.http.get<any[]>(`${API}/books/available`); }
  createBook(data: any): Observable<any> { return this.http.post(`${API}/books`, data); }
  assignBook(id: number, data: any): Observable<any> { return this.http.put(`${API}/books/${id}/assign`, data); }

  // ── Sessions ──
  getSessions(page = 1, pageSize = 20, date?: string, driverId?: number): Observable<any> {
    let params: any = { page, pageSize };
    if (date) params.date = date;
    if (driverId) params.driverId = driverId;
    return this.http.get(`${API}/sessions`, { params });
  }
  getTodaySessions(): Observable<any[]> { return this.http.get<any[]>(`${API}/sessions/today`); }
  getSession(id: number): Observable<any> { return this.http.get(`${API}/sessions/${id}`); }
  getLastSessionForDriver(driverId: number): Observable<any> {
    return this.http.get(`${API}/sessions/driver/${driverId}/last`);
  }
  createSession(data: any): Observable<any> { return this.http.post(`${API}/sessions`, data); }

  // ── Receipts ──
  checkDuplicate(number: number): Observable<any> {
    return this.http.get(`${API}/receipts/check-duplicate/${number}`);
  }

  // ── Gaps ──
  getGaps(status?: string): Observable<any[]> {
    let params: Record<string, string> = {};
    if (status) params['status'] = status;
    return this.http.get<any[]>(`${API}/gaps`, { params });
  }
  getGapSummary(): Observable<any> { return this.http.get(`${API}/gaps/summary`); }
  resolveGap(id: number, data: any): Observable<any> { return this.http.put(`${API}/gaps/${id}/resolve`, data); }

  // ── Reports ──
  getDailyReport(date: string): Observable<any> {
    return this.http.get(`${API}/reports/daily`, { params: { date } });
  }

  // ══════════════════════════════════
  // MODULE 2: Routes & Loading Sheets
  // ══════════════════════════════════

  // ── Routes ──
  getRoutes(): Observable<any[]> { return this.http.get<any[]>(`${API}/routes`); }
  createRoute(data: any): Observable<any> { return this.http.post(`${API}/routes`, data); }
  updateRoute(id: number, data: any): Observable<any> { return this.http.put(`${API}/routes/${id}`, data); }
  deleteRoute(id: number): Observable<any> { return this.http.delete(`${API}/routes/${id}`); }

  // ── Route Merchants ──
  getRouteMerchants(routeId: number): Observable<any[]> {
    return this.http.get<any[]>(`${API}/routes/${routeId}/merchants`);
  }
  addMerchantToRoute(routeId: number, data: any): Observable<any> {
    return this.http.post(`${API}/routes/${routeId}/merchants`, data);
  }
  updateMerchantPosition(routeId: number, rmId: number, data: any): Observable<any> {
    return this.http.put(`${API}/routes/${routeId}/merchants/${rmId}/position`, data);
  }
  removeMerchantFromRoute(routeId: number, rmId: number): Observable<any> {
    return this.http.delete(`${API}/routes/${routeId}/merchants/${rmId}`);
  }

  // ── Delivery Days ──
  getDeliveryDays(date?: string, routeId?: number): Observable<any[]> {
    let params: any = {};
    if (date) params.date = date;
    if (routeId) params.routeId = routeId;
    return this.http.get<any[]>(`${API}/delivery-days`, { params });
  }
  createDeliveryDay(data: any): Observable<any> {
    return this.http.post(`${API}/delivery-days`, data);
  }
  getDeliveryDay(id: number): Observable<any> {
    return this.http.get(`${API}/delivery-days/${id}`);
  }
  confirmDeliveryDay(id: number): Observable<any> {
    return this.http.put(`${API}/delivery-days/${id}/confirm`, {});
  }

  // ── Day Invoices ──
  addDayInvoice(dayId: number, data: any): Observable<any> {
    return this.http.post(`${API}/delivery-days/${dayId}/invoices`, data);
  }
  updateDayInvoice(dayId: number, invoiceId: number, data: any): Observable<any> {
    return this.http.put(`${API}/delivery-days/${dayId}/invoices/${invoiceId}`, data);
  }
  removeDayInvoice(dayId: number, invoiceId: number): Observable<any> {
    return this.http.delete(`${API}/delivery-days/${dayId}/invoices/${invoiceId}`);
  }
  reorderDayInvoices(dayId: number, reorders: any[]): Observable<any> {
    return this.http.put(`${API}/delivery-days/${dayId}/reorder`, reorders);
  }

  // ── Print Sheets ──
  getLoadingSheet(dayId: number): Observable<any> {
    return this.http.get(`${API}/delivery-days/${dayId}/loading-sheet`);
  }
  getDeliverySheet(dayId: number): Observable<any> {
    return this.http.get(`${API}/delivery-days/${dayId}/delivery-sheet`);
  }

  // ══════════════════════════════════
  // MODULE 3: Excel Import & Search
  // ══════════════════════════════════

  previewExcel(file: File): Observable<any> {
    const fd = new FormData();
    fd.append('file', file);
    return this.http.post(`${API}/sessions/excel/preview`, fd);
  }
  saveExcelSession(data: any): Observable<any> {
    return this.http.post(`${API}/sessions/excel/save`, data);
  }

  searchReceipts(filters: any): Observable<any> {
    let params: any = {};
    Object.keys(filters).forEach(k => { if (filters[k] != null && filters[k] !== '') params[k] = filters[k]; });
    return this.http.get(`${API}/search/receipts`, { params });
  }
  exportSearchResults(filters: any): Observable<Blob> {
    let params: any = {};
    Object.keys(filters).forEach(k => { if (filters[k] != null && filters[k] !== '') params[k] = filters[k]; });
    return this.http.get(`${API}/search/receipts/export`, { params, responseType: 'blob' });
  }
  getMerchantPaymentHistory(id: number, from?: string, to?: string): Observable<any> {
    let params: any = {};
    if (from) params.from = from;
    if (to) params.to = to;
    return this.http.get(`${API}/search/merchants/${id}/payment-history`, { params });
  }

  // ══════════════════════════════════
  // MODULE 4: دفتر الآجل
  // ══════════════════════════════════

  getAjalDaily(date: string): Observable<any> { return this.http.get(`${API}/ajal/daily`, { params: { date } }); }
  createAjalInvoices(data: any): Observable<any> { return this.http.post(`${API}/ajal/invoices`, data); }
  editAjalInvoice(id: number, data: any): Observable<any> { return this.http.put(`${API}/ajal/invoices/${id}`, data); }
  cancelAjalInvoice(id: number, reason: string): Observable<any> { return this.http.put(`${API}/ajal/invoices/${id}/cancel`, { reason }); }
  getAjalMerchantHistory(merchantId: number, from?: string, to?: string): Observable<any> {
    let params: any = {}; if (from) params.from = from; if (to) params.to = to;
    return this.http.get(`${API}/ajal/merchants/${merchantId}/history`, { params });
  }
  getAjalEmployeePerformance(from: string, to: string): Observable<any> {
    return this.http.get(`${API}/ajal/employees/performance`, { params: { from, to } });
  }
  searchAjal(filters: any): Observable<any> {
    let params: any = {};
    Object.keys(filters).forEach(k => { if (filters[k] != null && filters[k] !== '') params[k] = filters[k]; });
    return this.http.get(`${API}/ajal/search`, { params });
  }
  exportAjalSearch(filters: any): Observable<Blob> {
    let params: any = {};
    Object.keys(filters).forEach(k => { if (filters[k] != null && filters[k] !== '') params[k] = filters[k]; });
    return this.http.get(`${API}/ajal/search/export`, { params, responseType: 'blob' });
  }
  exportAjalDaily(date: string): Observable<Blob> {
    return this.http.get(`${API}/ajal/daily/export`, { params: { date }, responseType: 'blob' });
  }
  exportAjalEmployees(from: string, to: string): Observable<Blob> {
    return this.http.get(`${API}/ajal/employees/export`, { params: { from, to }, responseType: 'blob' });
  }
  previewAjalExcel(file: File): Observable<any> {
    const fd = new FormData(); fd.append('file', file);
    return this.http.post(`${API}/ajal/excel/preview`, fd);
  }
  saveAjalExcel(data: any): Observable<any> { return this.http.post(`${API}/ajal/excel/save`, data); }
  getAjalPrefixSettings(): Observable<any> { return this.http.get(`${API}/ajal/settings/invoice-prefix`); }
  updateAjalPrefix(newPrefix: string): Observable<any> { return this.http.put(`${API}/ajal/settings/invoice-prefix`, { newPrefix }); }
  getAjalDashboardSummary(): Observable<any> { return this.http.get(`${API}/ajal/dashboard/today-summary`); }
  getAjalEmployeeNames(): Observable<string[]> { return this.http.get<string[]>(`${API}/ajal/employee-names`); }
}
