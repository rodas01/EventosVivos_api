import { ComponentFixture, TestBed } from '@angular/core/testing';
import { LoginModal } from './login-modal';
import { AuthService } from '../../services/auth.service';
import { signal } from '@angular/core';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';
import { Router } from '@angular/router';

describe('LoginModal', () => {
  let component: LoginModal;
  let fixture: ComponentFixture<LoginModal>;
  let mockAuthService: any;
  let mockRouter: any;

  beforeEach(async () => {
    mockAuthService = {
      token: signal<string | null>(null),
      login: vi.fn(),
      logout: vi.fn(),
      logoutLocal: vi.fn(),
    };

    mockRouter = {
      navigate: vi.fn(),
    };

    await TestBed.configureTestingModule({
      imports: [LoginModal],
      providers: [
        { provide: AuthService, useValue: mockAuthService },
        { provide: Router, useValue: mockRouter },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(LoginModal);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should initialize form with empty inputs and be invalid', () => {
    fixture.detectChanges();
    expect(component.loginForm).toBeDefined();
    expect(component.loginForm.valid).toBeFalsy();
    expect(component.loginForm.controls['username'].value).toBe('');
    expect(component.loginForm.controls['password'].value).toBe('');
  });

  it('should validate form fields as required', () => {
    fixture.detectChanges();
    const username = component.loginForm.controls['username'];
    const password = component.loginForm.controls['password'];

    username.setValue('');
    password.setValue('');
    expect(component.loginForm.valid).toBeFalsy();

    username.setValue('user');
    expect(component.loginForm.valid).toBeFalsy();

    password.setValue('password');
    expect(component.loginForm.valid).toBeTruthy();
  });

  it('should reset form state on ngOnChanges when isOpen becomes true', () => {
    fixture.detectChanges();
    component.loginForm.controls['username'].setValue('someuser');
    component.loginForm.controls['password'].setValue('somepass');
    component.loginErrorMessage.set('Some Error');

    component.isOpen = true;
    component.ngOnChanges();

    expect(component.loginForm.controls['username'].value).toBe('');
    expect(component.loginForm.controls['password'].value).toBe('');
    expect(component.loginErrorMessage()).toBeNull();
  });

  it('should emit isOpenChange(false) and close modal on closeModal()', () => {
    fixture.detectChanges();
    vi.spyOn(component.isOpenChange, 'emit');
    component.isOpen = true;

    component['closeModal']();

    expect(component.isOpen).toBeFalsy();
    expect(component.isOpenChange.emit).toHaveBeenCalledWith(false);
  });

  it('should not authenticate if form is invalid', () => {
    fixture.detectChanges();
    component.loginForm.controls['username'].setValue('');
    component.loginForm.controls['password'].setValue('');

    component['onLoginSubmit']();

    expect(mockAuthService.login).not.toHaveBeenCalled();
  });

  it('should call AuthService.login and close modal on success', () => {
    mockAuthService.login.mockReturnValue(of({ token: 'mock-token' }));
    fixture.detectChanges();
    vi.spyOn(component.isOpenChange, 'emit');
    component.isOpen = true;
    component.loginForm.controls['username'].setValue('user');
    component.loginForm.controls['password'].setValue('password');

    component['onLoginSubmit']();

    expect(mockAuthService.login).toHaveBeenCalledWith({
      username: 'user',
      password: 'password',
    });
    expect(component.isOpen).toBeFalsy();
    expect(component.isOpenChange.emit).toHaveBeenCalledWith(false);
    expect(component.isLogginIn()).toBeFalsy();
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/home']);
  });

  it('should render the modal header and form inputs when isOpen is true', async () => {
    component.isOpen = true;
    fixture.detectChanges();
    await fixture.whenStable();

    const element = fixture.nativeElement;
    const header = element.querySelector('h2');
    expect(header).toBeTruthy();
    expect(header.textContent).toContain('Iniciar Sesión');

    const usernameInput = element.querySelector('input#username');
    const passwordInput = element.querySelector('input#password');
    expect(usernameInput).toBeTruthy();
    expect(passwordInput).toBeTruthy();
  });

  it('should trigger closeModal when clicking the close button', async () => {
    component.isOpen = true;
    fixture.detectChanges();
    await fixture.whenStable();

    const emitSpy = vi.spyOn(component.isOpenChange, 'emit');
    const element = fixture.nativeElement;
    const closeBtn = element.querySelector('button'); // First button is close
    expect(closeBtn).toBeTruthy();
    closeBtn.click();
    fixture.detectChanges();

    expect(component.isOpen).toBeFalsy();
    expect(emitSpy).toHaveBeenCalledWith(false);
  });

  it('should display validation errors when fields are touched and invalid', async () => {
    component.isOpen = true;
    fixture.detectChanges();
    await fixture.whenStable();

    const element = fixture.nativeElement;
    
    // Mark fields as touched
    component.loginForm.controls['username'].markAsTouched();
    component.loginForm.controls['password'].markAsTouched();
    fixture.detectChanges();

    const errorTexts = element.querySelectorAll('.text-error');
    expect(errorTexts.length).toBe(2);
    expect(errorTexts[0].textContent).toContain('El usuario es obligatorio.');
    expect(errorTexts[1].textContent).toContain('La contraseña es obligatoria.');
  });

  it('should display error message on login failure and render it in DOM', async () => {
    mockAuthService.login.mockReturnValue(
      throwError(() => ({ error: { message: 'Invalid credentials' } }))
    );
    component.isOpen = true;
    fixture.detectChanges();
    await fixture.whenStable();

    // Fill form and submit via form submit trigger
    component.loginForm.controls['username'].setValue('user');
    component.loginForm.controls['password'].setValue('password');
    fixture.detectChanges();

    const element = fixture.nativeElement;
    const form = element.querySelector('form');
    form.dispatchEvent(new Event('submit'));
    fixture.detectChanges();

    expect(mockAuthService.login).toHaveBeenCalled();
    expect(component.loginErrorMessage()).toBe('Invalid credentials');

    // Verify error is rendered in DOM
    const errorContainer = element.querySelector('.bg-error-container');
    expect(errorContainer).toBeTruthy();
    expect(errorContainer.textContent).toContain('Invalid credentials');
  });

  it('should fallback to string error and display it in DOM', async () => {
    mockAuthService.login.mockReturnValue(
      throwError(() => ({ error: 'Generic error message' }))
    );
    component.isOpen = true;
    fixture.detectChanges();
    await fixture.whenStable();

    component.loginForm.controls['username'].setValue('user');
    component.loginForm.controls['password'].setValue('password');
    fixture.detectChanges();

    const element = fixture.nativeElement;
    const form = element.querySelector('form');
    form.dispatchEvent(new Event('submit'));
    fixture.detectChanges();

    expect(component.loginErrorMessage()).toBe('Generic error message');

    const errorContainer = element.querySelector('.bg-error-container');
    expect(errorContainer).toBeTruthy();
    expect(errorContainer.textContent).toContain('Generic error message');
  });

  it('should render loading state when logging in', async () => {
    component.isOpen = true;
    fixture.detectChanges();
    await fixture.whenStable();

    component.loginForm.controls['username'].setValue('user');
    component.loginForm.controls['password'].setValue('password');
    component.isLogginIn.set(true);
    fixture.detectChanges();

    const element = fixture.nativeElement;
    const submitBtn = element.querySelector('button[type="submit"]');
    expect(submitBtn).toBeTruthy();
    expect(submitBtn.disabled).toBeTruthy();
    expect(submitBtn.textContent).toContain('Ingresando...');
    expect(element.querySelector('.animate-spin')).toBeTruthy();
  });
});
