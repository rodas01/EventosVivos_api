import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { ReservaService, CrearReserva, ReservaDto } from './reserva.service';
import { environment } from '../../../environments/environment';

describe('ReservaService', () => {
  let service: ReservaService;
  let httpTestingController: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        ReservaService,
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });

    service = TestBed.inject(ReservaService);
    httpTestingController = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTestingController.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should make a reservation', () => {
    const mockDto: CrearReserva = {
      eventoId: 1,
      cantidad: 2,
      nombreComprador: 'John Doe',
      emailComprador: 'john@example.com',
    };

    service.realizarReserva(mockDto).subscribe((res) => {
      expect(res).toEqual({ success: true });
    });

    const req = httpTestingController.expectOne(`${environment.baseUrl}api/Reservas`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(mockDto);
    req.flush({ success: true });
  });

  it('should get all reservations', () => {
    const mockData: ReservaDto[] = [
      {
        reservaId: 1,
        codigoReserva: 'CODE123',
        fechaReserva: '2026-06-25T10:00:00',
        cantidadEntradas: 2,
        precioReserva: 100,
        estadoReservaId: 'Confirmed',
        nombreCliente: 'John Doe',
        correoCliente: 'john@example.com',
        tituloEvento: 'Concert',
        fechaEvento: '2026-07-01T20:00:00',
        nombreVenue: 'Stadium',
        tipoEventoId: 'CONCIERTO',
      },
    ];

    service.getReservas().subscribe((res) => {
      expect(res).toEqual(mockData);
    });

    const req = httpTestingController.expectOne(`${environment.baseUrl}api/Reservas`);
    expect(req.request.method).toBe('GET');
    req.flush(mockData);
  });

  it('should get reservations by email', () => {
    const mockData: ReservaDto[] = [];

    service.getReservasByEmail('john@example.com').subscribe((res) => {
      expect(res).toEqual(mockData);
    });

    const req = httpTestingController.expectOne(`${environment.baseUrl}api/Reservas/cliente/john@example.com`);
    expect(req.request.method).toBe('GET');
    req.flush(mockData);
  });

  it('should cancel a reservation', () => {
    service.cancelarReserva(12).subscribe((res) => {
      expect(res).toEqual({ cancelled: true });
    });

    const req = httpTestingController.expectOne(`${environment.baseUrl}api/Reservas/12/cancelar`);
    expect(req.request.method).toBe('POST');
    req.flush({ cancelled: true });
  });

  it('should confirm payment', () => {
    service.confirmarPago(12).subscribe((res) => {
      expect(res).toEqual({ paid: true });
    });

    const req = httpTestingController.expectOne(`${environment.baseUrl}api/Reservas/12/confirmar-pago`);
    expect(req.request.method).toBe('POST');
    req.flush({ paid: true });
  });
});
