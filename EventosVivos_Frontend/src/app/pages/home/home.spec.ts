import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Home } from './home';
import { EventoService, ReporteEventoDto } from '../../core/services/evento.service';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { vi } from 'vitest';

describe('Home', () => {
  let component: Home;
  let fixture: ComponentFixture<Home>;
  let mockEventoService: any;

  const mockReportes: ReporteEventoDto[] = [
    {
      nombreEvento: 'Concierto de Rock',
      descripcion: 'Concierto en vivo',
      venue: { nombre: 'Estadio Nacional' },
      capacidad: 100,
      fechaInicio: '2026-06-30T20:00:00',
      fechaFin: '2026-06-30T22:00:00',
      precio: 50,
      tipoEventoId: 'CONCIERTO',
      estadoEventoId: 'ACTIVO',
      ocupacion: 80,
      reservasDisponibles: 20,
      reservasVendidas: 80,
      totalIngresos: 4000,
    },
    {
      nombreEvento: 'Taller de Angular',
      descripcion: 'Curso práctico',
      venue: { nombre: 'Aula 101' },
      capacidad: 10,
      fechaInicio: '2026-07-01T09:00:00',
      fechaFin: '2026-07-01T13:00:00',
      precio: 10,
      tipoEventoId: 'TALLER',
      estadoEventoId: 'ACTIVO',
      ocupacion: 120,
      reservasDisponibles: -2,
      reservasVendidas: 12,
      totalIngresos: 120,
    },
    {
      nombreEvento: 'Charla Motivacional',
      descripcion: 'Conferencia',
      venue: { nombre: 'Auditorio' },
      capacidad: 50,
      fechaInicio: '2026-07-02T18:00:00',
      fechaFin: '2026-07-02T19:30:00',
      precio: 0,
      tipoEventoId: 'CONFERENCIA',
      estadoEventoId: 'COMPLETADO',
      ocupacion: 100,
      reservasDisponibles: 0,
      reservasVendidas: 50,
      totalIngresos: 0,
    }
  ];

  beforeEach(async () => {
    mockEventoService = {
      getReporteEventos: vi.fn().mockReturnValue(of(mockReportes)),
    };

    await TestBed.configureTestingModule({
      imports: [Home],
      providers: [
        { provide: EventoService, useValue: mockEventoService },
        provideRouter([]),
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(Home);
    component = fixture.componentInstance;
    fixture.detectChanges();
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should call getReporteEventos on init and render rows', () => {
    expect(mockEventoService.getReporteEventos).toHaveBeenCalled();
    expect(component.reportes()).toEqual(mockReportes);

    const rows = fixture.nativeElement.querySelectorAll('tbody tr');
    expect(rows.length).toBe(3);

    const firstRowText = rows[0].textContent;
    expect(firstRowText).toContain('Concierto de Rock');
    expect(firstRowText).toContain('Estadio Nacional');
    expect(firstRowText).toContain('80% Ocupación');
    expect(firstRowText).toContain('Activo');

    const secondRowText = rows[1].textContent;
    expect(secondRowText).toContain('120% Excedido');
    expect(secondRowText).toContain('Crítico');
  });

  it('should return correct image depending on event type', () => {
    expect(component['getImagenEvento']('CONCIERTO')).toBe('/assets/img/concierto.webp');
    expect(component['getImagenEvento']('TALLER')).toBe('/assets/img/taller.webp');
    expect(component['getImagenEvento']('CONFERENCIA')).toBe('/assets/img/conferencia.webp');
    expect(component['getImagenEvento']('random')).toBe('/assets/img/conferencia.webp');
  });

  it('should return correct occupancy label', () => {
    expect(component['getTextoOcupacion'](80)).toBe('80% Ocupación');
    expect(component['getTextoOcupacion'](100)).toBe('100% Ocupación');
    expect(component['getTextoOcupacion'](120)).toBe('120% Excedido');
  });

  it('should return correct state label text', () => {
    expect(component['getEstadoTexto']('ACTIVO', 80)).toBe('Activo');
    expect(component['getEstadoTexto']('ACTIVO', 120)).toBe('Crítico');
    expect(component['getEstadoTexto']('COMPLETADO', 100)).toBe('Completado');
  });
});
