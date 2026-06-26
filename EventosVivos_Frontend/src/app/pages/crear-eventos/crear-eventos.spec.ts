import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CrearEventos } from './crear-eventos';
import { provideRouter, Router } from '@angular/router';
import { EventoService } from '../../core/services/evento.service';
import { VenueService, Venue } from '../../core/services/venue.service';
import { TipoEventoService, TipoEvento } from '../../core/services/tipo-evento.service';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';

describe('CrearEventos', () => {
  let component: CrearEventos;
  let fixture: ComponentFixture<CrearEventos>;
  let mockRouter: any;
  let mockEventoService: any;
  let mockVenueService: any;
  let mockTipoEventoService: any;

  const mockVenues: Venue[] = [
    { venueId: 1, nombre: 'Sala A', capacidadMaxima: 100, ubicacion: 'Direccion A' },
    { venueId: 2, nombre: 'Teatro Central', capacidadMaxima: 500, ubicacion: 'Direccion B' }
  ];

  const mockTipoEventos: TipoEvento[] = [
    { tipoEventoId: 'CONCIERTO', descripcion: 'Concierto' },
    { tipoEventoId: 'TALLER', descripcion: 'Taller' }
  ];

  beforeEach(async () => {
    mockRouter = {
      navigate: vi.fn(),
    };

    mockEventoService = {
      crearEvento: vi.fn().mockReturnValue(of({ success: true })),
    };

    mockVenueService = {
      get: vi.fn().mockReturnValue(of(mockVenues)),
    };

    mockTipoEventoService = {
      get: vi.fn().mockReturnValue(of(mockTipoEventos)),
    };

    await TestBed.configureTestingModule({
      imports: [CrearEventos],
      providers: [
        provideRouter([]),
        { provide: Router, useValue: mockRouter },
        { provide: EventoService, useValue: mockEventoService },
        { provide: VenueService, useValue: mockVenueService },
        { provide: TipoEventoService, useValue: mockTipoEventoService },
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(CrearEventos);
    component = fixture.componentInstance;
  });

  it('should create and load initial data', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
    expect(mockVenueService.get).toHaveBeenCalled();
    expect(mockTipoEventoService.get).toHaveBeenCalled();
    expect(component.venues()).toEqual(mockVenues);
    expect(component.tipoEventos()).toEqual(mockTipoEventos);
    // Should pre-select first event type
    expect(component.eventForm.controls['tipoEventoId'].value).toBe('CONCIERTO');
  });

  it('should invalidate form when empty', () => {
    fixture.detectChanges();
    expect(component.eventForm.valid).toBeFalsy();
  });

  it('should validate title boundaries (required, max 100 characters)', () => {
    fixture.detectChanges();
    const titleControl = component.eventForm.controls['titulo'];

    titleControl.setValue('');
    expect(titleControl.hasError('required')).toBeTruthy();

    titleControl.setValue('a'.repeat(101));
    expect(titleControl.hasError('maxlength')).toBeTruthy();

    titleControl.setValue('Valid Event Title');
    expect(titleControl.valid).toBeTruthy();
  });

  it('should validate description boundaries (required, max 500 characters)', () => {
    fixture.detectChanges();
    const descControl = component.eventForm.controls['descripcion'];

    descControl.setValue('');
    expect(descControl.hasError('required')).toBeTruthy();

    descControl.setValue('a'.repeat(501));
    expect(descControl.hasError('maxlength')).toBeTruthy();

    descControl.setValue('Valid Event Description');
    expect(descControl.valid).toBeTruthy();
  });

  it('should validate capacity values (min 1, cannot exceed venue capacity)', () => {
    fixture.detectChanges();
    const capControl = component.eventForm.controls['capacidadMaxima'];
    const venueControl = component.eventForm.controls['idVenue'];

    // Test positive capacity validation
    capControl.setValue(0);
    expect(capControl.hasError('min')).toBeTruthy();

    capControl.setValue(-10);
    expect(capControl.hasError('min')).toBeTruthy();

    // Select venue 1 (capacity limit 100)
    venueControl.setValue(1);
    capControl.setValue(100);
    expect(component.eventForm.hasError('capacityExceeded')).toBeFalsy();

    // Exceed capacity
    capControl.setValue(101);
    // Force validation trigger at form group level
    component.eventForm.updateValueAndValidity();
    expect(component.eventForm.hasError('capacityExceeded')).toBeTruthy();
    expect(component.eventForm.getError('capacityExceeded')).toEqual({ max: 100 });

    // Select venue 2 (capacity limit 500)
    venueControl.setValue(2);
    component.eventForm.updateValueAndValidity();
    expect(component.eventForm.hasError('capacityExceeded')).toBeFalsy();
  });

  it('should validate start date must be a future date', () => {
    fixture.detectChanges();
    const startControl = component.eventForm.controls['fechaHoraInicio'];

    // Past date
    const pastDate = new Date(Date.now() - 24 * 60 * 60 * 1000).toISOString().substring(0, 16);
    startControl.setValue(pastDate);
    expect(startControl.hasError('notFuture')).toBeTruthy();

    // Future date
    const futureDate = new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString().substring(0, 16);
    startControl.setValue(futureDate);
    expect(startControl.hasError('notFuture')).toBeFalsy();
  });

  it('should validate weekend time restriction (Saturdays and Sundays cannot start after 22:00)', () => {
    fixture.detectChanges();
    const startControl = component.eventForm.controls['fechaHoraInicio'];

    // 2026-07-04 is a Saturday (future date)
    // 21:00 is allowed
    startControl.setValue('2026-07-04T21:00');
    expect(startControl.hasError('weekendTimeExceeded')).toBeFalsy();

    // 22:00 is allowed
    startControl.setValue('2026-07-04T22:00');
    expect(startControl.hasError('weekendTimeExceeded')).toBeFalsy();

    // 22:01 is not allowed
    startControl.setValue('2026-07-04T22:01');
    expect(startControl.hasError('weekendTimeExceeded')).toBeTruthy();

    // 23:30 is not allowed
    startControl.setValue('2026-07-04T23:30');
    expect(startControl.hasError('weekendTimeExceeded')).toBeTruthy();

    // 2026-07-06 is a Monday (weekday)
    // 23:30 is allowed
    startControl.setValue('2026-07-06T23:30');
    expect(startControl.hasError('weekendTimeExceeded')).toBeFalsy();
  });

  it('should validate end date must be after start date', () => {
    fixture.detectChanges();
    const startControl = component.eventForm.controls['fechaHoraInicio'];
    const endControl = component.eventForm.controls['fechaHoraFin'];

    const futureStart = new Date(Date.now() + 24 * 60 * 60 * 1000);
    const futureEndBefore = new Date(futureStart.getTime() - 1000 * 60 * 60); // 1h before start
    const futureEndAfter = new Date(futureStart.getTime() + 1000 * 60 * 60);  // 1h after start

    startControl.setValue(futureStart.toISOString().substring(0, 16));
    endControl.setValue(futureEndBefore.toISOString().substring(0, 16));
    component.eventForm.updateValueAndValidity();
    expect(component.eventForm.hasError('endBeforeStart')).toBeTruthy();

    endControl.setValue(futureEndAfter.toISOString().substring(0, 16));
    component.eventForm.updateValueAndValidity();
    expect(component.eventForm.hasError('endBeforeStart')).toBeFalsy();
  });

  it('should validate price must be positive (> 0)', () => {
    fixture.detectChanges();
    const priceControl = component.eventForm.controls['precioEntrada'];

    priceControl.setValue(0);
    expect(priceControl.hasError('min')).toBeTruthy();

    priceControl.setValue(-5);
    expect(priceControl.hasError('min')).toBeTruthy();

    priceControl.setValue(0.01);
    expect(priceControl.valid).toBeTruthy();

    priceControl.setValue(150.50);
    expect(priceControl.valid).toBeTruthy();
  });

  it('should not submit if form is invalid', () => {
    fixture.detectChanges();
    const markAllAsTouchedSpy = vi.spyOn(component.eventForm, 'markAllAsTouched');
    component['onSubmit']();

    expect(markAllAsTouchedSpy).toHaveBeenCalled();
    expect(mockEventoService.crearEvento).not.toHaveBeenCalled();
  });

  it('should submit successfully and navigate to /eventos', () => {
    fixture.detectChanges();
    const futureStart = new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString().substring(0, 16);
    const futureEnd = new Date(Date.now() + 26 * 60 * 60 * 1000).toISOString().substring(0, 16);

    component.eventForm.setValue({
      titulo: 'Concierto de Metal',
      descripcion: 'Gran noche de guitarras distorsionadas',
      idVenue: '1',
      capacidadMaxima: 50,
      fechaHoraInicio: futureStart,
      fechaHoraFin: futureEnd,
      precioEntrada: 25.50,
      tipoEventoId: 'CONCIERTO'
    });

    component['onSubmit']();

    expect(component.isSubmitting()).toBeFalsy();
    expect(mockEventoService.crearEvento).toHaveBeenCalledWith({
      titulo: 'Concierto de Metal',
      descripcion: 'Gran noche de guitarras distorsionadas',
      idVenue: 1,
      capacidadMaxima: 50,
      fechaHoraInicio: new Date(futureStart).toISOString(),
      fechaHoraFin: new Date(futureEnd).toISOString(),
      precioEntrada: 25.5,
      tipoEventoId: 'CONCIERTO'
    });
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/reporte-eventos']);
  });

  it('should handle backend creation error', () => {
    mockEventoService.crearEvento.mockReturnValue(
      throwError(() => ({ error: { message: 'El venue ya está ocupado en ese horario.' } }))
    );
    fixture.detectChanges();

    const futureStart = new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString().substring(0, 16);
    const futureEnd = new Date(Date.now() + 26 * 60 * 60 * 1000).toISOString().substring(0, 16);

    component.eventForm.setValue({
      titulo: 'Concierto de Metal',
      descripcion: 'Gran noche de guitarras distorsionadas',
      idVenue: '1',
      capacidadMaxima: 50,
      fechaHoraInicio: futureStart,
      fechaHoraFin: futureEnd,
      precioEntrada: 25.50,
      tipoEventoId: 'CONCIERTO'
    });

    component['onSubmit']();

    expect(component.isSubmitting()).toBeFalsy();
    expect(component.errorMessage()).toBe('El venue ya está ocupado en ese horario.');
  });

  it('should navigate to /reporte-eventos when cancelled', () => {
    fixture.detectChanges();
    component['cancel']();
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/reporte-eventos']);
  });
});
