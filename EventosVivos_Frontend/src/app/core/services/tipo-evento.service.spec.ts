import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { TipoEventoService, TipoEvento } from './tipo-evento.service';
import { environment } from '../../../environments/environment';

describe('TipoEventoService', () => {
  let service: TipoEventoService;
  let httpTestingController: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        TipoEventoService,
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });

    service = TestBed.inject(TipoEventoService);
    httpTestingController = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTestingController.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should get event types', () => {
    const mockData: TipoEvento[] = [
      { tipoEventoId: '1', descripcion: 'Concierto' },
      { tipoEventoId: '2', descripcion: 'Conferencia' },
    ];

    service.get().subscribe((res) => {
      expect(res).toEqual(mockData);
    });

    const req = httpTestingController.expectOne(`${environment.baseUrl}api/TipoEventos`);
    expect(req.request.method).toBe('GET');
    req.flush(mockData);
  });
});
