import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CardEvento } from './card-evento';

describe('CardEvento', () => {
  let component: CardEvento;
  let fixture: ComponentFixture<CardEvento>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CardEvento],
    }).compileComponents();

    fixture = TestBed.createComponent(CardEvento);
    component = fixture.componentInstance;
    component.evento = {
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
    };
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
