import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MisReservas } from './mis-reservas';
import { ReservaService, ReservaDto } from '../../core/services/reserva.service';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';

describe('MisReservas', () => {
  let component: MisReservas;
  let fixture: ComponentFixture<MisReservas>;
  let mockReservaService: any;

  const mockReservas: ReservaDto[] = [
    {
      reservaId: 1,
      codigoReserva: 'EV-123456',
      fechaReserva: '2026-06-25T10:00:00',
      cantidadEntradas: 2,
      precioReserva: 100,
      estadoReservaId: 'COMFIRMADA',
      nombreCliente: 'John Doe',
      correoCliente: 'john@example.com',
      tituloEvento: 'Concierto de Rock',
      fechaEvento: '2026-07-01T20:00:00',
      nombreVenue: 'Estadio Nacional',
      tipoEventoId: 'CONCIERTO',
    },
  ];

  beforeEach(async () => {
    mockReservaService = {
      getReservasByEmail: vi.fn().mockReturnValue(of(mockReservas)),
      cancelarReserva: vi.fn().mockReturnValue(of({ success: true })),
    };

    await TestBed.configureTestingModule({
      imports: [MisReservas],
      providers: [
        { provide: ReservaService, useValue: mockReservaService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(MisReservas);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize form with empty email and be invalid', () => {
    expect(component.searchForm.valid).toBeFalsy();
    expect(component.searchForm.controls['email'].value).toBe('');
  });

  it('should validate email format', () => {
    const emailControl = component.searchForm.controls['email'];
    emailControl.setValue('invalid-email');
    expect(emailControl.valid).toBeFalsy();

    emailControl.setValue('test@example.com');
    expect(emailControl.valid).toBeTruthy();
  });

  it('should fetch reservations on search', () => {
    component.searchForm.controls['email'].setValue('john@example.com');
    component['onSearch']();

    expect(mockReservaService.getReservasByEmail).toHaveBeenCalledWith('john@example.com');
    expect(component.reservas()).toEqual(mockReservas);
    expect(component.hasSearched()).toBeTruthy();
    expect(component.errorMessage()).toBeNull();
  });

  it('should handle search api error', () => {
    mockReservaService.getReservasByEmail.mockReturnValue(
      throwError(() => ({ error: { message: 'Error de servidor' } }))
    );

    component.searchForm.controls['email'].setValue('john@example.com');
    component['onSearch']();

    expect(component.reservas()).toEqual([]);
    expect(component.errorMessage()).toBe('Error de servidor');
  });

  it('should call cancel endpoint and refresh list on cancel reservation event', () => {
    component.searchForm.controls['email'].setValue('john@example.com');
    component.reservas.set(mockReservas);

    component['onCancelReserva'](1);

    expect(mockReservaService.cancelarReserva).toHaveBeenCalledWith(1);
    expect(mockReservaService.getReservasByEmail).toHaveBeenCalledWith('john@example.com');
  });
});
