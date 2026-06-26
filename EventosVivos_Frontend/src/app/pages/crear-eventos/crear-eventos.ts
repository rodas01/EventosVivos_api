import { Component, OnInit, inject, signal } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  Validators,
  ReactiveFormsModule,
  AbstractControl,
  ValidationErrors,
  ValidatorFn,
} from '@angular/forms';
import { Router } from '@angular/router';
import { EventoService } from '../../core/services/evento.service';
import { VenueService, Venue } from '../../core/services/venue.service';
import { TipoEventoService, TipoEvento } from '../../core/services/tipo-evento.service';

export const futureDateValidator: ValidatorFn = (
  control: AbstractControl,
): ValidationErrors | null => {
  if (!control.value) {
    return null;
  }
  const selectedDate = new Date(control.value);
  const now = new Date();
  if (selectedDate <= now) {
    return { notFuture: true };
  }
  return null;
};

export const weekendTimeValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  if (!control.value) {
    return null;
  }
  const selectedDate = new Date(control.value);
  const day = selectedDate.getDay(); // 0 = Sunday, 6 = Saturday
  if (day === 0 || day === 6) {
    const hours = selectedDate.getHours();
    const minutes = selectedDate.getMinutes();
    if (hours > 22 || (hours === 22 && minutes > 0)) {
      return { weekendTimeExceeded: true };
    }
  }
  return null;
};

export const dateComparisonValidator: ValidatorFn = (
  control: AbstractControl,
): ValidationErrors | null => {
  const startVal = control.get('fechaHoraInicio')?.value;
  const endVal = control.get('fechaHoraFin')?.value;

  if (!startVal || !endVal) {
    return null;
  }

  const start = new Date(startVal);
  const end = new Date(endVal);

  if (end <= start) {
    return { endBeforeStart: true };
  }
  return null;
};

export const capacityValidator = (getVenues: () => Venue[]): ValidatorFn => {
  return (control: AbstractControl): ValidationErrors | null => {
    const idVenue = control.get('idVenue')?.value;
    const capacity = control.get('capacidadMaxima')?.value;

    if (!idVenue || !capacity) {
      return null;
    }

    const selectedVenue = getVenues().find((v) => v.venueId === Number(idVenue));
    if (selectedVenue && capacity > selectedVenue.capacidadMaxima) {
      return { capacityExceeded: { max: selectedVenue.capacidadMaxima } };
    }

    return null;
  };
};

@Component({
  selector: 'app-crear-eventos',
  imports: [ReactiveFormsModule],
  templateUrl: './crear-eventos.html',
})
export class CrearEventos implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);
  private readonly eventoService = inject(EventoService);
  private readonly venueService = inject(VenueService);
  private readonly tipoEventoService = inject(TipoEventoService);

  public eventForm!: FormGroup;
  public readonly venues = signal<Venue[]>([]);
  public readonly tipoEventos = signal<TipoEvento[]>([]);
  public readonly isSubmitting = signal<boolean>(false);
  public readonly errorMessage = signal<string | null>(null);

  ngOnInit(): void {
    this.eventForm = this.fb.group(
      {
        titulo: ['', [Validators.required, Validators.maxLength(100)]],
        descripcion: ['', [Validators.required, Validators.maxLength(500)]],
        idVenue: ['', [Validators.required]],
        capacidadMaxima: ['', [Validators.required, Validators.min(1)]],
        fechaHoraInicio: ['', [Validators.required, futureDateValidator, weekendTimeValidator]],
        fechaHoraFin: ['', [Validators.required]],
        precioEntrada: ['', [Validators.required, Validators.min(0.01)]],
        tipoEventoId: ['', [Validators.required]],
      },
      {
        validators: [dateComparisonValidator, capacityValidator(() => this.venues())],
      },
    );

    this.loadInitialData();
  }

  private loadInitialData(): void {
    this.venueService.get().subscribe({
      next: (data) => this.venues.set(data),
      error: (err) => console.error('Error loading venues:', err),
    });

    this.tipoEventoService.get().subscribe({
      next: (data) => {
        this.tipoEventos.set(data);
        if (data.length > 0) {
          this.eventForm.patchValue({ tipoEventoId: data[0].tipoEventoId });
        }
      },
      error: (err) => console.error('Error loading event types:', err),
    });
  }

  protected cancel(): void {
    this.router.navigate(['/reporte-eventos']);
  }

  protected onSubmit(): void {
    if (this.eventForm.invalid) {
      this.eventForm.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    const formValue = this.eventForm.value;
    const payload = {
      titulo: formValue.titulo,
      descripcion: formValue.descripcion,
      idVenue: Number(formValue.idVenue),
      capacidadMaxima: Number(formValue.capacidadMaxima),
      fechaHoraInicio: new Date(formValue.fechaHoraInicio).toISOString(),
      fechaHoraFin: new Date(formValue.fechaHoraFin).toISOString(),
      precioEntrada: Number(formValue.precioEntrada),
      tipoEventoId: formValue.tipoEventoId,
    };

    this.eventoService.crearEvento(payload).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.router.navigate(['/reporte-eventos']);
      },
      error: (err) => {
        console.error('Error creating event:', err);
        this.isSubmitting.set(false);
        const msg = err.error?.message || err.error || 'Ocurrió un error al crear el evento.';
        this.errorMessage.set(msg);
      },
    });
  }
}
