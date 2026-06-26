import { TestBed, ComponentFixture } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { App } from './app';
import { Component, signal } from '@angular/core';
import { AuthService } from './core/services/auth.service';

@Component({
  selector: 'app-dummy',
  template: '<div>Dummy</div>'
})
class DummyComponent {}

describe('App', () => {
  let fixture: ComponentFixture<App>;
  let app: App;
  let router: Router;
  let mockAuthService: any;

  beforeEach(async () => {
    mockAuthService = {
      token: signal<string | null>(null),
      login: () => {},
      logout: () => {},
      logoutLocal: () => {},
    };

    await TestBed.configureTestingModule({
      imports: [App],
      providers: [
        { provide: AuthService, useValue: mockAuthService },
        provideRouter([
          {
            path: 'visible',
            component: DummyComponent,
            data: { showHeader: true, showFooter: true }
          },
          {
            path: 'hidden',
            component: DummyComponent,
            data: { showHeader: false, showFooter: false }
          },
          {
            path: 'default',
            component: DummyComponent
          },
          {
            path: 'internal',
            component: DummyComponent,
            data: { internal: true }
          }
        ])
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(App);
    app = fixture.componentInstance;
    router = TestBed.inject(Router);
    fixture.detectChanges();
    await fixture.whenStable();
  });

  it('should create the app', () => {
    expect(app).toBeTruthy();
  });

  it(`should have the 'EventosVivos_Frontend' title`, () => {
    expect(app['title']()).toEqual('EventosVivos_Frontend');
  });

  it('should default showHeader and showFooter to true', () => {
    expect(app.showHeader()).toBe(true);
    expect(app.showFooter()).toBe(true);
  });

  it('should update showHeader and showFooter to false when navigating to hidden route', async () => {
    await router.navigate(['/hidden']);
    fixture.detectChanges();
    expect(app.showHeader()).toBe(false);
    expect(app.showFooter()).toBe(false);
  });

  it('should update showHeader and showFooter to true when navigating to visible route', async () => {
    await router.navigate(['/hidden']);
    fixture.detectChanges();
    expect(app.showHeader()).toBe(false);

    await router.navigate(['/visible']);
    fixture.detectChanges();
    expect(app.showHeader()).toBe(true);
    expect(app.showFooter()).toBe(true);
  });

  it('should fallback to true when data is not defined', async () => {
    await router.navigate(['/hidden']);
    fixture.detectChanges();
    expect(app.showHeader()).toBe(false);

    await router.navigate(['/default']);
    fixture.detectChanges();
    expect(app.showHeader()).toBe(true);
    expect(app.showFooter()).toBe(true);
  });

  it('should render internal layout elements when data.internal is true', async () => {
    await router.navigate(['/internal']);
    fixture.detectChanges();
    expect(app.internal()).toBe(true);

    const element = fixture.nativeElement;
    expect(element.querySelector('app-internal-menu')).toBeTruthy();
    expect(element.querySelector('app-internal-header')).toBeTruthy();
  });
});
