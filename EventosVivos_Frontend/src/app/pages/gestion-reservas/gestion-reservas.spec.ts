import { ComponentFixture, TestBed } from '@angular/core/testing';
import { GestionReservas } from './gestion-reservas';
import { ReservaService, ReservaDto } from '../../core/services/reserva.service';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';

describe('GestionReservas', () => {
  let component: GestionReservas;
  let fixture: ComponentFixture<GestionReservas>;
  let mockReservaService: any;

  const mockReservas: ReservaDto[] = [
    {
      reservaId: 8810,
      codigoReserva: 'EV-A92K11',
      fechaReserva: '2023-10-10T10:00:00',
      cantidadEntradas: 1,
      precioReserva: 75.00,
      estadoReservaId: 'COMFIRMADA',
      nombreCliente: 'Roberto Mejía',
      correoCliente: 'roberto@example.com',
      tituloEvento: 'Concierto Sinfónico',
      fechaEvento: '2023-11-15T20:00:00',
      nombreVenue: 'Teatro Mayor',
      tipoEventoId: 'CONCIERTO'
    },
    {
      reservaId: 8821,
      fechaReserva: '2023-10-12T10:00:00',
      cantidadEntradas: 2,
      precioReserva: 120.00,
      estadoReservaId: 'PAGO_PENDIENTE',
      nombreCliente: 'Mariana Alarcón',
      correoCliente: 'mariana@example.com',
      tituloEvento: 'Festival Jazz Primavera',
      fechaEvento: '2023-11-12T19:00:00',
      nombreVenue: 'Parque Principal',
      tipoEventoId: 'CONCIERTO'
    },
    {
      reservaId: 8799,
      fechaReserva: '2023-10-08T10:00:00',
      cantidadEntradas: 3,
      precioReserva: 210.00,
      estadoReservaId: 'CANCELADA',
      nombreCliente: 'Elena Luna',
      correoCliente: 'elena@example.com',
      tituloEvento: 'Workshop de Escultura',
      fechaEvento: '2023-11-20T17:00:00',
      nombreVenue: 'Centro Cultural',
      tipoEventoId: 'TALLER'
    }
  ];

  beforeEach(async () => {
    mockReservaService = {
      getReservas: vi.fn().mockReturnValue(of(mockReservas)),
      confirmarPago: vi.fn().mockReturnValue(of({ success: true })),
      cancelarReserva: vi.fn().mockReturnValue(of({ success: true }))
    };

    await TestBed.configureTestingModule({
      imports: [GestionReservas],
      providers: [
        { provide: ReservaService, useValue: mockReservaService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(GestionReservas);
    component = fixture.componentInstance;
  });

  it('should create and load reservations initially sorting pending payment first', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
    expect(mockReservaService.getReservas).toHaveBeenCalled();
    
    const loaded = component.reservas();
    expect(loaded.length).toBe(3);
    expect(loaded[0].reservaId).toBe(8821);
    expect(loaded[0].estadoReservaId).toBe('PAGO_PENDIENTE');
    expect(loaded[1].reservaId).toBe(8810);
    expect(loaded[2].reservaId).toBe(8799);

    // Verify row rendering count
    const rows = fixture.nativeElement.querySelectorAll('tbody tr');
    expect(rows.length).toBe(3);
  });

  it('should format initials correctly', () => {
    fixture.detectChanges();
    expect(component['getClienteInitials']('Mariana Alarcón')).toBe('MA');
    expect(component['getClienteInitials']('Roberto Carlos Mejía')).toBe('RC');
    expect(component['getClienteInitials']('Elena')).toBe('E');
    expect(component['getClienteInitials']('')).toBe('');
    expect(component['getClienteInitials']('   ')).toBe('');
  });

  it('should open and close the confirmation modal', () => {
    fixture.detectChanges();
    const reserva = mockReservas[0];

    // Open modal
    component['openConfirmModal'](reserva, 'confirmPayment');
    expect(component.isModalOpen()).toBeTruthy();
    expect(component.modalType()).toBe('confirmPayment');
    expect(component.selectedReserva()).toEqual(reserva);

    // Close modal
    component['closeConfirmModal']();
    expect(component.isModalOpen()).toBeFalsy();
    expect(component.modalType()).toBeNull();
    expect(component.selectedReserva()).toBeNull();
  });

  it('should execute confirmPayment action successfully and reload reservations', () => {
    fixture.detectChanges();
    const reserva = mockReservas[0];

    // Set modal state
    component['openConfirmModal'](reserva, 'confirmPayment');

    // Trigger action
    component['confirmAction']();

    expect(mockReservaService.confirmarPago).toHaveBeenCalledWith(reserva.reservaId);
    expect(component.isProcessing()).toBeFalsy();
    expect(component.isModalOpen()).toBeFalsy();
    // Verify reload was called
    expect(mockReservaService.getReservas).toHaveBeenCalledTimes(2);
  });

  it('should execute cancelBooking action successfully and reload reservations', () => {
    fixture.detectChanges();
    const reserva = mockReservas[1];

    // Set modal state
    component['openConfirmModal'](reserva, 'cancelBooking');

    // Trigger action
    component['confirmAction']();

    expect(mockReservaService.cancelarReserva).toHaveBeenCalledWith(reserva.reservaId);
    expect(component.isProcessing()).toBeFalsy();
    expect(component.isModalOpen()).toBeFalsy();
    // Verify reload was called
    expect(mockReservaService.getReservas).toHaveBeenCalledTimes(2);
  });

  it('should handle action errors gracefully', () => {
    mockReservaService.confirmarPago.mockReturnValue(
      throwError(() => ({ error: { message: 'El banco rechazó la transacción.' } }))
    );
    fixture.detectChanges();
    const reserva = mockReservas[0];

    // Set modal state
    component['openConfirmModal'](reserva, 'confirmPayment');

    // Trigger action
    component['confirmAction']();

    expect(mockReservaService.confirmarPago).toHaveBeenCalledWith(reserva.reservaId);
    expect(component.isProcessing()).toBeFalsy();
    expect(component.isModalOpen()).toBeTruthy(); // Modal remains open
    expect(component.errorMessage()).toBe('El banco rechazó la transacción.');
  });

  it('should handle error when loading reservations on init', () => {
    mockReservaService.getReservas.mockReturnValue(
      throwError(() => ({ error: { message: 'Error de red' } }))
    );
    fixture.detectChanges();
    
    expect(mockReservaService.getReservas).toHaveBeenCalled();
    expect(component.errorMessage()).toBe('Error de red');
  });
});
