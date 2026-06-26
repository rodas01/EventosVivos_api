import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { getEndpoint } from '../constants/endpoints';

export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  expiresAt: string;
  tokenType: string;
}

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly baseUrl: string = environment.baseUrl;
  readonly token = signal<string | null>(this.getStoredToken());

  constructor(private readonly http: HttpClient) {}

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.baseUrl}${getEndpoint('login')}`, request).pipe(
      tap((response) => {
        this.setSession(response.token);
      })
    );
  }

  logout(): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}${getEndpoint('logout')}`, {}).pipe(
      tap(() => {
        this.clearSession();
      })
    );
  }

  isLoggedIn(): boolean {
    return !!this.token();
  }

  logoutLocal(): void {
    this.clearSession();
  }


  private getStoredToken(): string | null {
    if (typeof window !== 'undefined') {
      return localStorage.getItem('token');
    }
    return null;
  }

  private setSession(token: string): void {
    if (typeof window !== 'undefined') {
      localStorage.setItem('token', token);
    }
    this.token.set(token);
  }

  private clearSession(): void {
    if (typeof window !== 'undefined') {
      localStorage.removeItem('token');
    }
    this.token.set(null);
  }
}
