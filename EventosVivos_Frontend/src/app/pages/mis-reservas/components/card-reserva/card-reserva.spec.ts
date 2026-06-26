import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CardReserva } from './card-reserva';
import { ReservaDto } from '../../../../core/services/reserva.service';
import { vi } from 'vitest';

describe('CardReserva', () => {
  let component: CardReserva;
  let fixture: ComponentFixture<CardReserva>;

  const mockReserva: ReservaDto = {
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
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CardReserva],
    }).compileComponents();

    fixture = TestBed.createComponent(CardReserva);
    component = fixture.componentInstance;
    component.reserva = { ...mockReserva };
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should emit cancel event when clicking the cancel button', () => {
    fixture.detectChanges();
    const confirmSpy = vi.spyOn(window, 'confirm').mockReturnValue(true);
    const cancelSpy = vi.spyOn(component.cancel, 'emit');

    const element = fixture.nativeElement;
    const cancelBtn = element.querySelector('button'); // In COMFIRMADA state, this is the cancel button
    expect(cancelBtn).toBeTruthy();
    cancelBtn.click();
    fixture.detectChanges();

    expect(confirmSpy).toHaveBeenCalledWith('¿Está seguro de que desea cancelar esta reserva?');
    expect(cancelSpy).toHaveBeenCalledWith(1);

    confirmSpy.mockRestore();
  });

  it('should render correct template for COMFIRMADA state', () => {
    component.reserva = { ...mockReserva, estadoReservaId: 'COMFIRMADA' };
    fixture.detectChanges();

    const element = fixture.nativeElement;
    expect(element.textContent).toContain('Confirmada');
    expect(element.textContent).toContain('Cancelar Reserva');
    expect(element.textContent).toContain('Concierto de Rock');
  });

  it('should render correct template for PAGO_PENDIENTE state', () => {
    component.reserva = { ...mockReserva, estadoReservaId: 'PAGO_PENDIENTE' };
    fixture.detectChanges();

    const element = fixture.nativeElement;
    expect(element.textContent).toContain('Pendiente de Pago');
    expect(element.textContent).toContain('Concierto de Rock');
  });

  it('should render correct template for CANCELADA state', () => {
    component.reserva = { ...mockReserva, estadoReservaId: 'CANCELADA' };
    fixture.detectChanges();

    const element = fixture.nativeElement;
    expect(element.textContent).toContain('Cancelada');
    expect(element.textContent).toContain('Esta reserva ya ha sido procesada.');
  });

  it('should render correct template for PERDIDA state', () => {
    component.reserva = { ...mockReserva, estadoReservaId: 'PERDIDA' };
    fixture.detectChanges();

    const element = fixture.nativeElement;
    expect(element.textContent).toContain('Perdida');
    expect(element.textContent).toContain('Esta reserva ya ha sido procesada.');
  });

  it('should return correct image path depending on event type', () => {
    fixture.detectChanges();
    
    // Default mock is CONCIERTO
    expect(component['imagenEvento']).toBe('/assets/img/concierto.webp');

    // TALLER
    component.reserva.tipoEventoId = 'TALLER';
    expect(component['imagenEvento']).toBe('/assets/img/taller.webp');

    // Default (CONFERENCIA or other)
    component.reserva.tipoEventoId = 'CONFERENCIA';
    expect(component['imagenEvento']).toBe('/assets/img/conferencia.webp');
  });
});
