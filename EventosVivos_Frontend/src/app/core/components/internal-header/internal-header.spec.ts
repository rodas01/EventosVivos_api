import { ComponentFixture, TestBed } from '@angular/core/testing';
import { InternalHeader } from './internal-header';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';
import { signal } from '@angular/core';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';

describe('InternalHeader', () => {
  let component: InternalHeader;
  let fixture: ComponentFixture<InternalHeader>;
  let mockAuthService: any;
  let mockRouter: any;

  beforeEach(async () => {
    mockAuthService = {
      token: signal<string | null>(null),
      logout: vi.fn(),
      logoutLocal: vi.fn(),
    };

    mockRouter = {
      navigate: vi.fn(),
    };

    await TestBed.configureTestingModule({
      imports: [InternalHeader],
      providers: [
        { provide: AuthService, useValue: mockAuthService },
        { provide: Router, useValue: mockRouter },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(InternalHeader);
    component = fixture.componentInstance;
    fixture.detectChanges();
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should call AuthService.logout and redirect on successful logout', () => {
    mockAuthService.logout.mockReturnValue(of({}));
    component['logout']();
    expect(mockAuthService.logout).toHaveBeenCalled();
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/eventos']);
  });

  it('should call logoutLocal and redirect on logout failure', () => {
    mockAuthService.logout.mockReturnValue(
      throwError(() => new Error('Server error'))
    );
    component['logout']();
    expect(mockAuthService.logout).toHaveBeenCalled();
    expect(mockAuthService.logoutLocal).toHaveBeenCalled();
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/eventos']);
  });
});

