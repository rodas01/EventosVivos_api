import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { InternalMenu } from './internal-menu';

describe('InternalMenu', () => {
  let component: InternalMenu;
  let fixture: ComponentFixture<InternalMenu>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [InternalMenu],
      providers: [provideRouter([])]
    }).compileComponents();

    fixture = TestBed.createComponent(InternalMenu);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
