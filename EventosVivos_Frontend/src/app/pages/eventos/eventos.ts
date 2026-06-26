import { Component, OnInit, signal, inject, DestroyRef } from '@angular/core';
import { TitleCasePipe } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CardEvento } from './components/card-evento/card-evento';
import { TipoEventoService, TipoEvento } from '../../core/services/tipo-evento.service';
import { VenueService, Venue } from '../../core/services/venue.service';
import { EventoService, EventoDto, FiltrosEvento } from '../../core/services/evento.service';
import { debounceTime, distinctUntilChanged, startWith } from 'rxjs';

@Component({
  selector: 'app-eventos',
  imports: [CardEvento, TitleCasePipe, ReactiveFormsModule],
  templateUrl: './eventos.html',
})
export class Eventos implements OnInit {
  private readonly tipoEventoService = inject(TipoEventoService);
  private readonly venueService = inject(VenueService);
  private readonly eventoService = inject(EventoService);
  private readonly fb = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly tipoEventos = signal<TipoEvento[]>([]);
  protected readonly venues = signal<Venue[]>([]);
  protected readonly eventos = signal<EventoDto[]>([]);

  protected filterForm!: FormGroup;

  constructor() {
    this.initForm();
  }

  ngOnInit(): void {
    // Load event types
    this.tipoEventoService.get().subscribe((data) => {
      this.tipoEventos.set(data);
      const tipoEventosGroup = this.filterForm.get('tipoEventos') as FormGroup;
      data.forEach((t) => {
        if (!tipoEventosGroup.contains(t.tipoEventoId)) {
          tipoEventosGroup.addControl(t.tipoEventoId, this.fb.control(false));
        }
      });
    });

    // Load venues
    this.venueService.get().subscribe((data) => {
      this.venues.set(data);
    });

    // Setup subscription to form changes to query events
    this.filterForm.valueChanges
      .pipe(
        startWith(this.filterForm.value),
        debounceTime(300),
        distinctUntilChanged((prev, curr) => JSON.stringify(prev) === JSON.stringify(curr)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe((value) => {
        this.fetchFilteredEvents(value);
      });
  }

  private initForm(): void {
    this.filterForm = this.fb.group({
      titulo: [''],
      tipoEventos: this.fb.group({}),
      fechaInicio: [''],
      fechaFin: [''],
      venueId: [''],
      estado: ['Activo'],
    });
  }

  protected setEstado(estado: 'Activo' | 'Completado'): void {
    this.filterForm.patchValue({ estado });
  }

  protected clearFilters(): void {
    // Reset inputs
    this.filterForm.patchValue({
      titulo: '',
      fechaInicio: '',
      fechaFin: '',
      venueId: '',
      estado: 'Activo',
    });

    // Reset checkboxes
    const tipoEventosGroup = this.filterForm.get('tipoEventos') as FormGroup;
    Object.keys(tipoEventosGroup.controls).forEach((key) => {
      tipoEventosGroup.get(key)?.setValue(false);
    });
  }

  private fetchFilteredEvents(formValue: any): void {
    // Gather selected event type IDs from the checkbox group
    const selectedTypes = Object.keys(formValue.tipoEventos || {}).filter(
      (key) => formValue.tipoEventos[key] === true
    );

    const filtros: FiltrosEvento = {
      titulo: formValue.titulo || undefined,
      tipoEvento: selectedTypes.length > 0 ? selectedTypes[0] : undefined,
      fechaInicio: formValue.fechaInicio || undefined,
      fechaFin: formValue.fechaFin || undefined,
      venueId: formValue.venueId ? Number(formValue.venueId) : undefined,
      estado: formValue.estado || undefined,
    };

    this.eventoService.getEventos(filtros).subscribe((data) => {
      this.eventos.set(data);
    });
  }
}
