import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Reservas } from './reservas';
import { ActivatedRoute, Router } from '@angular/router';
import { EventoService, EventoDto } from '../../core/services/evento.service';
import { ReservaService } from '../../core/services/reserva.service';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';

describe('Reservas', () => {
  let component: Reservas;
  let fixture: ComponentFixture<Reservas>;
  let mockActivatedRoute: any;
  let mockRouter: any;
  let mockEventoService: any;
  let mockReservaService: any;

  const mockEvent: EventoDto = {
    eventoId: 123,
    nombreEvento: 'Midnight Jazz',
    descripcion: 'Serie Cultural Curada',
    venue: { nombre: 'The Blue Note Lounge' },
    capacidad: 100,
    fechaInicio: new Date(Date.now() + 48 * 60 * 60 * 1000).toISOString(), // 48h from now (not urgent)
    fechaFin: new Date(Date.now() + 50 * 60 * 60 * 1000).toISOString(),
    precio: 120, // expensive (> 100)
    tipoEventoId: 'CONCIERTO',
    estadoEventoId: 'ACTIVO',
    soldOut: false,
    entradasDisponibles: 100,
  };

  beforeEach(async () => {
    mockActivatedRoute = {
      snapshot: {
        paramMap: {
          get: (key: string) => (key === 'id' ? '123' : null),
        },
      },
    };

    mockRouter = {
      navigate: vi.fn(),
    };

    mockEventoService = {
      getEventoById: vi.fn().mockReturnValue(of(mockEvent)),
    };

    mockReservaService = {
      realizarReserva: vi.fn().mockReturnValue(of({ success: true })),
    };

    await TestBed.configureTestingModule({
      imports: [Reservas],
      providers: [
        { provide: ActivatedRoute, useValue: mockActivatedRoute },
        { provide: Router, useValue: mockRouter },
        { provide: EventoService, useValue: mockEventoService },
        { provide: ReservaService, useValue: mockReservaService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(Reservas);
    component = fixture.componentInstance;
  });

  it('should create and load event', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
    expect(mockEventoService.getEventoById).toHaveBeenCalledWith(123);
    expect(component.evento()).toEqual(mockEvent);
  });

  it('should invalidate form when fields are empty', () => {
    fixture.detectChanges();
    expect(component.bookingForm.valid).toBeFalsy();
    
    component.bookingForm.controls['fullName'].setValue('');
    component.bookingForm.controls['email'].setValue('invalid-email');
    expect(component.bookingForm.valid).toBeFalsy();
  });

  it('should validate form when values are correct', () => {
    fixture.detectChanges();
    component.bookingForm.controls['fullName'].setValue('John Doe');
    component.bookingForm.controls['email'].setValue('john@example.com');
    component.bookingForm.controls['quantity'].setValue(2);
    expect(component.bookingForm.valid).toBeTruthy();
  });

  it('should apply limit of 10 if event price is > 100 and start time is >= 24h', () => {
    fixture.detectChanges();
    
    expect(component.maxAllowedTickets()).toBe(10);
    
    component.bookingForm.controls['fullName'].setValue('John Doe');
    component.bookingForm.controls['email'].setValue('john@example.com');
    
    component.bookingForm.controls['quantity'].setValue(10);
    expect(component.bookingForm.controls['quantity'].valid).toBeTruthy();
    
    component.bookingForm.controls['quantity'].setValue(11);
    expect(component.bookingForm.controls['quantity'].valid).toBeFalsy();
  });

  it('should apply limit of 5 if event starts in < 24h even if price is > 100', () => {
    const urgentEvent = {
      ...mockEvent,
      fechaInicio: new Date(Date.now() + 12 * 60 * 60 * 1000).toISOString(),
    };
    mockEventoService.getEventoById.mockReturnValue(of(urgentEvent));
    
    fixture.detectChanges();
    
    expect(component.maxAllowedTickets()).toBe(5);
    
    component.bookingForm.controls['fullName'].setValue('John Doe');
    component.bookingForm.controls['email'].setValue('john@example.com');
    
    component.bookingForm.controls['quantity'].setValue(5);
    expect(component.bookingForm.controls['quantity'].valid).toBeTruthy();
    
    component.bookingForm.controls['quantity'].setValue(6);
    expect(component.bookingForm.controls['quantity'].valid).toBeFalsy();
  });

  it('should limit to available tickets if event is cheap and not urgent', () => {
    const cheapEvent = {
      ...mockEvent,
      precio: 50,
      fechaInicio: new Date(Date.now() + 48 * 60 * 60 * 1000).toISOString(),
      entradasDisponibles: 42,
    };
    mockEventoService.getEventoById.mockReturnValue(of(cheapEvent));
    
    fixture.detectChanges();
    
    expect(component.maxAllowedTickets()).toBe(42);
    
    component.bookingForm.controls['fullName'].setValue('John Doe');
    component.bookingForm.controls['email'].setValue('john@example.com');
    
    // Quantity 42 should be valid
    component.bookingForm.controls['quantity'].setValue(42);
    expect(component.bookingForm.controls['quantity'].valid).toBeTruthy();
    
    // Quantity 43 should be invalid
    component.bookingForm.controls['quantity'].setValue(43);
    expect(component.bookingForm.controls['quantity'].valid).toBeFalsy();
  });

  it('should submit booking successfully and redirect', () => {
    vi.useFakeTimers();
    fixture.detectChanges();
    component.bookingForm.controls['fullName'].setValue('John Doe');
    component.bookingForm.controls['email'].setValue('john@example.com');
    component.bookingForm.controls['quantity'].setValue(2);
    
    expect(component.bookingForm.valid).toBeTruthy();
    
    component['onSubmit']();
    
    expect(mockReservaService.realizarReserva).toHaveBeenCalledWith({
      eventoId: 123,
      nombreComprador: 'John Doe',
      emailComprador: 'john@example.com',
      cantidad: 2
    });
    
    vi.advanceTimersByTime(2000);
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/mis-reservas']);
    vi.useRealTimers();
  });

  it('should handle submission errors from api', () => {
    mockReservaService.realizarReserva.mockReturnValue(
      throwError(() => ({ error: { message: 'No hay entradas disponibles' } }))
    );
    
    fixture.detectChanges();
    component.bookingForm.controls['fullName'].setValue('John Doe');
    component.bookingForm.controls['email'].setValue('john@example.com');
    component.bookingForm.controls['quantity'].setValue(2);
    
    component['onSubmit']();
    
    expect(component.errorMessage()).toBe('No hay entradas disponibles');
  });

  it('should not allow reservation if event is not active', () => {
    const cancelledEvent = {
      ...mockEvent,
      estadoEventoId: 'CANCELADO',
    };
    mockEventoService.getEventoById.mockReturnValue(of(cancelledEvent));
    fixture.detectChanges();

    expect(component.canReserve()).toBeFalsy();
    expect(component.bookingNotAvailableMessage()).toBe('Este evento no está disponible para realizar reservas porque no se encuentra activo.');
  });

  it('should not allow reservation if event starts in less than 1 hour', () => {
    const soonEvent = {
      ...mockEvent,
      fechaInicio: new Date(Date.now() + 30 * 60 * 1000).toISOString(),
    };
    mockEventoService.getEventoById.mockReturnValue(of(soonEvent));
    fixture.detectChanges();

    expect(component.canReserve()).toBeFalsy();
    expect(component.bookingNotAvailableMessage()).toBe('No es posible reservar entradas para eventos que inician en menos de una hora.');
  });

  it('should not allow reservation if there are no available tickets', () => {
    const noTicketsEvent = {
      ...mockEvent,
      entradasDisponibles: 0,
    };
    mockEventoService.getEventoById.mockReturnValue(of(noTicketsEvent));
    fixture.detectChanges();

    expect(component.canReserve()).toBeFalsy();
    expect(component.bookingNotAvailableMessage()).toBe('No hay entradas disponibles para este evento.');
  });
});
