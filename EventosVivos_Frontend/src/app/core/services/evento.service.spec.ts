import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { EventoService, EventoDto, FiltrosEvento, CrearEvento, ReporteEventoDto } from './evento.service';
import { environment } from '../../../environments/environment';

describe('EventoService', () => {
  let service: EventoService;
  let httpTestingController: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        EventoService,
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });

    service = TestBed.inject(EventoService);
    httpTestingController = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTestingController.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should get events with parameters', () => {
    const mockData: EventoDto[] = [
      {
        eventoId: 1,
        nombreEvento: 'Rock Concert',
        descripcion: 'Amazing concert',
        venue: { nombre: 'Stadium' },
        capacidad: 1000,
        fechaInicio: '2026-06-25T10:00:00',
        fechaFin: '2026-06-25T12:00:00',
        precio: 50.0,
        tipoEventoId: '1',
        estadoEventoId: 'Active',
        soldOut: false,
        entradasDisponibles: 1000,
      },
    ];

    const filtros: FiltrosEvento = {
      tipoEvento: '1',
      titulo: 'Rock',
      venueId: 2,
      fechaInicio: '2026-06-25T10:00:00',
      fechaFin: '2026-06-25T12:00:00',
      estado: 'Active',
    };

    service.getEventos(filtros).subscribe((res) => {
      expect(res).toEqual(mockData);
    });

    const req = httpTestingController.expectOne((request) => {
      return (
        request.url === `${environment.baseUrl}api/Eventos` &&
        request.params.get('tipoEvento') === '1' &&
        request.params.get('titulo') === 'Rock' &&
        request.params.get('venueId') === '2' &&
        request.params.get('fechaInicio') === '2026-06-25T10:00:00' &&
        request.params.get('fechaFin') === '2026-06-25T12:00:00' &&
        request.params.get('estado') === 'Active'
      );
    });
    expect(req.request.method).toBe('GET');
    req.flush(mockData);
  });

  it('should create an event', () => {
    const mockEvento: CrearEvento = {
      titulo: 'New Event',
      descripcion: 'Event details',
      idVenue: 1,
      capacidadMaxima: 500,
      fechaHoraInicio: '2026-07-01T10:00:00',
      fechaHoraFin: '2026-07-01T12:00:00',
      precioEntrada: 20,
      tipoEventoId: '2',
    };

    service.crearEvento(mockEvento).subscribe((res) => {
      expect(res).toEqual({ success: true });
    });

    const req = httpTestingController.expectOne(`${environment.baseUrl}api/Eventos/crear-evento`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(mockEvento);
    req.flush({ success: true });
  });

  it('should get event reports', () => {
    const mockReports: ReporteEventoDto[] = [
      {
        nombreEvento: 'Rock Concert',
        descripcion: 'Amazing concert',
        venue: { nombre: 'Stadium' },
        capacidad: 1000,
        fechaInicio: '2026-06-25T10:00:00',
        fechaFin: '2026-06-25T12:00:00',
        precio: 50.0,
        tipoEventoId: '1',
        estadoEventoId: 'Active',
        ocupacion: 800,
        reservasDisponibles: 200,
        reservasVendidas: 800,
        totalIngresos: 40000,
      },
    ];

    service.getReporteEventos().subscribe((res) => {
      expect(res).toEqual(mockReports);
    });

    const req = httpTestingController.expectOne(`${environment.baseUrl}api/Eventos/reporte`);
    expect(req.request.method).toBe('GET');
    req.flush(mockReports);
  });

  it('should get a single event by id', () => {
    const mockEvent: EventoDto = {
      eventoId: 12,
      nombreEvento: 'Rock Concert',
      descripcion: 'Amazing concert',
      venue: { nombre: 'Stadium' },
      capacidad: 1000,
      fechaInicio: '2026-06-25T10:00:00',
      fechaFin: '2026-06-25T12:00:00',
      precio: 50.0,
      tipoEventoId: '1',
      estadoEventoId: 'Active',
      soldOut: false,
      entradasDisponibles: 1000,
    };

    service.getEventoById(12).subscribe((res) => {
      expect(res).toEqual(mockEvent);
    });

    const req = httpTestingController.expectOne(`${environment.baseUrl}api/Eventos/12`);
    expect(req.request.method).toBe('GET');
    req.flush(mockEvent);
  });
});
