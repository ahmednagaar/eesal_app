import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { Router } from '@angular/router';
import { environment } from '../../environments/environment';

export interface TokenResponse {
  token: string;
  fullName: string;
  role: string;
  userId: number;
  expiresAt: string;
}

export interface UserInfo {
  userId: number;
  username: string;
  fullName: string;
  role: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private apiUrl = `${environment.apiUrl}/auth`;
  private currentUser$ = new BehaviorSubject<UserInfo | null>(null);

  constructor(private http: HttpClient, private router: Router) {
    this.loadFromStorage();
  }

  get user$(): Observable<UserInfo | null> { return this.currentUser$.asObservable(); }
  get currentUser(): UserInfo | null { return this.currentUser$.value; }
  get isLoggedIn(): boolean { return !!this.getToken(); }
  get token(): string | null { return this.getToken(); }

  login(username: string, password: string): Observable<TokenResponse> {
    return this.http.post<TokenResponse>(`${this.apiUrl}/login`, { username, password }).pipe(
      tap(res => {
        localStorage.setItem('token', res.token);
        localStorage.setItem('user', JSON.stringify({
          userId: res.userId, username, fullName: res.fullName, role: res.role
        }));
        this.currentUser$.next({
          userId: res.userId, username, fullName: res.fullName, role: res.role
        });
      })
    );
  }

  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    this.currentUser$.next(null);
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  private loadFromStorage(): void {
    const userStr = localStorage.getItem('user');
    if (userStr) {
      try { this.currentUser$.next(JSON.parse(userStr)); } catch { }
    }
  }
}
