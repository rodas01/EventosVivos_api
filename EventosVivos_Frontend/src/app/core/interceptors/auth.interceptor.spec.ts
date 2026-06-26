import { TestBed } from '@angular/core/testing';
import { provideHttpClient, withInterceptors, HttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { vi } from 'vitest';
import { authInterceptor } from './auth.interceptor';
import { AuthService } from '../services/auth.service';
import { environment } from '../../../environments/environment';

describe('authInterceptor', () => {
  let httpTestingController: HttpTestingController;
  let httpClient: HttpClient;
  let authService: AuthService;
  let mockRouter: any;

  beforeEach(() => {
    mockRouter = {
      navigate: vi.fn(),
    };

    TestBed.configureTestingModule({
      providers: [
        AuthService,
        { provide: Router, useValue: mockRouter },
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
      ],
    });

    httpClient = TestBed.inject(HttpClient);
    httpTestingController = TestBed.inject(HttpTestingController);
    authService = TestBed.inject(AuthService);

    if (typeof window !== 'undefined') {
      localStorage.clear();
    }
  });

  afterEach(() => {
    httpTestingController.verify();
  });

  it('should not add Authorization header if there is no token in localStorage', () => {
    httpClient.get(`${environment.baseUrl}api/Venues`).subscribe();

    const req = httpTestingController.expectOne(`${environment.baseUrl}api/Venues`);
    expect(req.request.headers.has('Authorization')).toBeFalsy();
    req.flush({});
  });

  it('should add Authorization header if there is a token and it is not expired', () => {
    const futureTime = Math.floor(Date.now() / 1000) + 3600;
    const header = btoa(JSON.stringify({ alg: 'HS256', typ: 'JWT' }));
    const payload = btoa(JSON.stringify({ exp: futureTime }));
    const token = `${header}.${payload}.signature`;

    if (typeof window !== 'undefined') {
      localStorage.setItem('token', token);
    }
    authService.token.set(token);

    httpClient.get(`${environment.baseUrl}api/Venues`).subscribe();

    const req = httpTestingController.expectOne(`${environment.baseUrl}api/Venues`);
    expect(req.request.headers.has('Authorization')).toBeTruthy();
    expect(req.request.headers.get('Authorization')).toBe(`Bearer ${token}`);
    req.flush({});
  });

  it('should clear token and redirect to "/" if token is expired (and not logout)', () => {
    const pastTime = Math.floor(Date.now() / 1000) - 3600;
    const header = btoa(JSON.stringify({ alg: 'HS256', typ: 'JWT' }));
    const payload = btoa(JSON.stringify({ exp: pastTime }));
    const token = `${header}.${payload}.signature`;

    if (typeof window !== 'undefined') {
      localStorage.setItem('token', token);
    }
    authService.token.set(token);

    const logoutSpy = vi.spyOn(authService, 'logoutLocal');

    httpClient.get(`${environment.baseUrl}api/Venues`).subscribe({
      error: (err) => {
        expect(err.status).toBe(401);
      },
    });

    httpTestingController.expectNone(`${environment.baseUrl}api/Venues`);

    expect(logoutSpy).toHaveBeenCalled();
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/']);
  });

  it('should proceed with token and not validate expiration if the request is to the logout endpoint', () => {
    const pastTime = Math.floor(Date.now() / 1000) - 3600;
    const header = btoa(JSON.stringify({ alg: 'HS256', typ: 'JWT' }));
    const payload = btoa(JSON.stringify({ exp: pastTime }));
    const token = `${header}.${payload}.signature`;

    if (typeof window !== 'undefined') {
      localStorage.setItem('token', token);
    }
    authService.token.set(token);

    const logoutSpy = vi.spyOn(authService, 'logoutLocal');

    httpClient.post(`${environment.baseUrl}api/Auth/logout`, {}).subscribe();

    const req = httpTestingController.expectOne(`${environment.baseUrl}api/Auth/logout`);
    expect(req.request.headers.has('Authorization')).toBeTruthy();
    expect(req.request.headers.get('Authorization')).toBe(`Bearer ${token}`);
    req.flush({});

    expect(logoutSpy).not.toHaveBeenCalled();
    expect(mockRouter.navigate).not.toHaveBeenCalled();
  });

  it('should treat invalid token as expired and call logoutLocal', () => {
    const token = 'abc.def.ghi';

    if (typeof window !== 'undefined') {
      localStorage.setItem('token', token);
    }
    authService.token.set(token);

    const logoutSpy = vi.spyOn(authService, 'logoutLocal');

    httpClient.get(`${environment.baseUrl}api/Venues`).subscribe({
      error: (err) => {
        expect(err.status).toBe(401);
      },
    });

    expect(logoutSpy).toHaveBeenCalled();
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/']);
  });

  it('should parse path correctly if request URL starts with a slash', () => {
    const futureTime = Math.floor(Date.now() / 1000) + 3600;
    const header = btoa(JSON.stringify({ alg: 'HS256', typ: 'JWT' }));
    const payload = btoa(JSON.stringify({ exp: futureTime }));
    const token = `${header}.${payload}.signature`;

    if (typeof window !== 'undefined') {
      localStorage.setItem('token', token);
    }
    authService.token.set(token);

    httpClient.get('/api/Venues').subscribe();

    const req = httpTestingController.expectOne('/api/Venues');
    expect(req.request.headers.has('Authorization')).toBeTruthy();
    req.flush({});
  });
});
