import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReporteEventos } from './reporte-eventos';
import { EventoService, ReporteEventoDto } from '../../core/services/evento.service';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { vi } from 'vitest';

describe('ReporteEventos', () => {
  let component: ReporteEventos;
  let fixture: ComponentFixture<ReporteEventos>;
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
      nombreEvento: 'Microteatro Cancelado',
      descripcion: 'Conferencia',
      venue: { nombre: 'Espacio Teatro' },
      capacidad: 50,
      fechaInicio: '2026-07-02T18:00:00',
      fechaFin: '2026-07-02T19:30:00',
      precio: 10,
      tipoEventoId: 'CONFERENCIA',
      estadoEventoId: 'CANCELADO',
      ocupacion: 24,
      reservasDisponibles: 38,
      reservasVendidas: 12,
      totalIngresos: 120,
    }
  ];

  beforeEach(async () => {
    mockEventoService = {
      getReporteEventos: vi.fn().mockReturnValue(of(mockReportes)),
    };

    await TestBed.configureTestingModule({
      imports: [ReporteEventos],
      providers: [
        { provide: EventoService, useValue: mockEventoService },
        provideRouter([]),
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ReporteEventos);
    component = fixture.componentInstance;
    fixture.detectChanges();
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should call getReporteEventos on init and render rows with dollar currency', () => {
    expect(mockEventoService.getReporteEventos).toHaveBeenCalled();
    expect(component.reportes()).toEqual(mockReportes);

    const rows = fixture.nativeElement.querySelectorAll('tbody tr');
    expect(rows.length).toBe(3);

    // Row 1 (Rock Concert): Active, 80% Occupancy, $4,000 Revenue
    const firstRowText = rows[0].textContent;
    expect(firstRowText).toContain('Concierto de Rock');
    expect(firstRowText).toContain('80');
    expect(firstRowText).toContain('20');
    expect(firstRowText).toContain('$4,000');
    expect(firstRowText).toContain('Activo');

    // Row 2 (Taller): Overcapacity, Critical status, $120 Revenue
    const secondRowText = rows[1].textContent;
    expect(secondRowText).toContain('120%');
    expect(secondRowText).toContain('$120');
    expect(secondRowText).toContain('Crítico');

    // Row 3 (Cancelled): Cancelled status, line-through or opacity check
    const thirdRowText = rows[2].textContent;
    expect(thirdRowText).toContain('Microteatro Cancelado');
    expect(thirdRowText).toContain('Cancelado');
    expect(rows[2].classList).toContain('opacity-70');
  });

  it('should return correct image depending on event type', () => {
    expect(component['getImagenEvento']('CONCIERTO')).toBe('/assets/img/concierto.webp');
    expect(component['getImagenEvento']('TALLER')).toBe('/assets/img/taller.webp');
    expect(component['getImagenEvento']('CONFERENCIA')).toBe('/assets/img/conferencia.webp');
  });

  it('should return correct state label text', () => {
    expect(component['getEstadoTexto']('ACTIVO', 80)).toBe('Activo');
    expect(component['getEstadoTexto']('ACTIVO', 120)).toBe('Crítico');
    expect(component['getEstadoTexto']('COMPLETADO', 100)).toBe('Completado');
    expect(component['getEstadoTexto']('CANCELADO', 50)).toBe('Cancelado');
  });
});
