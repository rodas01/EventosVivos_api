import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { AuthService, LoginRequest, LoginResponse } from './auth.service';
import { environment } from '../../../environments/environment';

describe('AuthService', () => {
  let service: AuthService;
  let httpTestingController: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        AuthService,
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });

    service = TestBed.inject(AuthService);
    httpTestingController = TestBed.inject(HttpTestingController);
    if (typeof window !== 'undefined') {
      localStorage.clear();
    }
  });

  afterEach(() => {
    httpTestingController.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should execute login and set token', () => {
    const mockRequest: LoginRequest = { username: 'testuser', password: 'password' };
    const mockResponse: LoginResponse = { token: 'mockToken', expiresAt: '2026-06-25T10:00:00Z', tokenType: 'Bearer' };

    service.login(mockRequest).subscribe((res) => {
      expect(res).toEqual(mockResponse);
      expect(service.token()).toBe('mockToken');
    });

    const req = httpTestingController.expectOne(`${environment.baseUrl}api/Auth/login`);
    expect(req.request.method).toBe('POST');
    req.flush(mockResponse);
  });

  it('should execute logout and clear token', () => {
    service.token.set('some-token');
    if (typeof window !== 'undefined') {
      localStorage.setItem('token', 'some-token');
    }

    service.logout().subscribe(() => {
      expect(service.token()).toBeNull();
      if (typeof window !== 'undefined') {
        expect(localStorage.getItem('token')).toBeNull();
      }
    });

    const req = httpTestingController.expectOne(`${environment.baseUrl}api/Auth/logout`);
    expect(req.request.method).toBe('POST');
    req.flush({});
  });

  it('should execute logoutLocal and clear session', () => {
    service.token.set('some-token');
    if (typeof window !== 'undefined') {
      localStorage.setItem('token', 'some-token');
    }

    service.logoutLocal();

    expect(service.token()).toBeNull();
    if (typeof window !== 'undefined') {
      expect(localStorage.getItem('token')).toBeNull();
    }
  });

  it('should return false for isLoggedIn when token is null, and true when token is present', () => {
    service.token.set(null);
    expect(service.isLoggedIn()).toBeFalsy();

    service.token.set('some-token');
    expect(service.isLoggedIn()).toBeTruthy();
  });

  it('should return null from getStoredToken when window is undefined', () => {
    const originalWindow = (globalThis as any).window;
    try {
      delete (globalThis as any).window;
      const manualService = new AuthService(null as any);
      expect(manualService.token()).toBeNull();
    } finally {
      (globalThis as any).window = originalWindow;
    }
  });
});
