import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { Header } from './header';
import { AuthService } from '../../services/auth.service';
import { signal } from '@angular/core';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';

describe('Header', () => {
  let component: Header;
  let fixture: ComponentFixture<Header>;
  let mockAuthService: any;

  beforeEach(async () => {
    mockAuthService = {
      token: signal<string | null>(null),
      login: vi.fn(),
      logout: vi.fn(),
      logoutLocal: vi.fn(),
    };

    await TestBed.configureTestingModule({
      imports: [Header],
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: mockAuthService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(Header);
    component = fixture.componentInstance;
    fixture.detectChanges();
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should open modal when clicking the Iniciar Sesión button', async () => {
    mockAuthService.token.set(null);
    fixture.detectChanges();
    await fixture.whenStable();

    component.showLoginModal.set(false);
    
    const element = fixture.nativeElement;
    const loginBtn = element.querySelector('button'); // Iniciar Sesión button
    expect(loginBtn).toBeTruthy();
    expect(loginBtn.textContent).toContain('Iniciar Sesión');
    
    loginBtn.click();
    fixture.detectChanges();

    expect(component.showLoginModal()).toBeTruthy();
  });

  it('should show "Iniciar Sesión" when not logged in, and "Cerrar Sesión" when logged in', async () => {
    // 1. Not logged in
    mockAuthService.token.set(null);
    fixture.detectChanges();
    await fixture.whenStable();

    const textContentOut = fixture.nativeElement.textContent;
    expect(textContentOut).toContain('Iniciar Sesión');
    expect(textContentOut).not.toContain('Cerrar Sesión');

    // 2. Logged in
    mockAuthService.token.set('some-token');
    fixture.detectChanges();
    await fixture.whenStable();

    const textContentIn = fixture.nativeElement.textContent;
    expect(textContentIn).toContain('Cerrar Sesión');
    expect(textContentIn).not.toContain('Iniciar Sesión');
  });

  it('should call AuthService.logout on logout click', async () => {
    mockAuthService.logout.mockReturnValue(of({}));
    mockAuthService.token.set('some-token');
    fixture.detectChanges();
    await fixture.whenStable();

    const element = fixture.nativeElement;
    const logoutBtn = element.querySelector('button'); // Cerrar Sesión button
    expect(logoutBtn).toBeTruthy();
    expect(logoutBtn.textContent).toContain('Cerrar Sesión');
    
    logoutBtn.click();
    fixture.detectChanges();

    expect(mockAuthService.logout).toHaveBeenCalled();
  });

  it('should fallback to logoutLocal on logout failure', async () => {
    mockAuthService.logout.mockReturnValue(
      throwError(() => new Error('Server disconnect'))
    );
    mockAuthService.token.set('some-token');
    fixture.detectChanges();
    await fixture.whenStable();

    const element = fixture.nativeElement;
    const logoutBtn = element.querySelector('button'); // Cerrar Sesión button
    expect(logoutBtn).toBeTruthy();
    
    logoutBtn.click();
    fixture.detectChanges();

    expect(mockAuthService.logout).toHaveBeenCalled();
    expect(mockAuthService.logoutLocal).toHaveBeenCalled();
  });
});


