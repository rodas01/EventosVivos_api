import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { Eventos } from './eventos';
import { TipoEventoService } from '../../core/services/tipo-evento.service';
import { VenueService } from '../../core/services/venue.service';
import { EventoService } from '../../core/services/evento.service';
import { ReactiveFormsModule } from '@angular/forms';

describe('Eventos', () => {
  let component: Eventos;
  let fixture: ComponentFixture<Eventos>;

  beforeEach(async () => {
    const mockTipoEventoService = {
      get: () => of([]),
    };
    const mockVenueService = {
      get: () => of([]),
    };
    const mockEventoService = {
      getEventos: () => of([]),
    };

    await TestBed.configureTestingModule({
      imports: [Eventos, ReactiveFormsModule],
      providers: [
        { provide: TipoEventoService, useValue: mockTipoEventoService },
        { provide: VenueService, useValue: mockVenueService },
        { provide: EventoService, useValue: mockEventoService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(Eventos);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
