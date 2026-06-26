import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { VenueService, Venue } from './venue.service';
import { environment } from '../../../environments/environment';

describe('VenueService', () => {
  let service: VenueService;
  let httpTestingController: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        VenueService,
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });

    service = TestBed.inject(VenueService);
    httpTestingController = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTestingController.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should get venues', () => {
    const mockData: Venue[] = [
      { venueId: 1, nombre: 'Stadium', capacidadMaxima: 50000, ubicacion: 'City Center' },
      { venueId: 2, nombre: 'Theater', capacidadMaxima: 1000, ubicacion: 'Downtown' },
    ];

    service.get().subscribe((res) => {
      expect(res).toEqual(mockData);
    });

    const req = httpTestingController.expectOne(`${environment.baseUrl}api/Venues`);
    expect(req.request.method).toBe('GET');
    req.flush(mockData);
  });
});
