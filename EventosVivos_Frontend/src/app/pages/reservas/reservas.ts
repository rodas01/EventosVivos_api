import { Component, OnInit, signal, inject, computed } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { DatePipe, CurrencyPipe, TitleCasePipe } from '@angular/common';
import { EventoService, EventoDto } from '../../core/services/evento.service';
import { ReservaService, CrearReserva } from '../../core/services/reserva.service';
import { EVENT_TYPES, EVENT_STATUS } from '../../core/constants/event.constants';

@Component({
  selector: 'app-reservas',
  imports: [ReactiveFormsModule, DatePipe, CurrencyPipe, TitleCasePipe],
  templateUrl: './reservas.html',
})
export class Reservas implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);
  private readonly eventoService = inject(EventoService);
  private readonly reservaService = inject(ReservaService);

  public readonly evento = signal<EventoDto | null>(null);
  public readonly maxAllowedTickets = signal<number | null>(null);
  public readonly errorMessage = signal<string | null>(null);
  public readonly successMessage = signal<string | null>(null);
  public readonly isSubmitting = signal<boolean>(false);

  public bookingForm!: FormGroup;
  protected readonly EVENT_TYPES = EVENT_TYPES;
  protected readonly EVENT_STATUS = EVENT_STATUS;

  protected readonly totalPrice = computed(() => {
    const qty = this.bookingForm?.get('quantity')?.value || 0;
    const price = this.evento()?.precio || 0;
    return qty * price;
  });

  public readonly canReserve = computed(() => {
    const ev = this.evento();
    if (!ev) return false;

    if (ev.estadoEventoId.toUpperCase() !== EVENT_STATUS.ACTIVO) {
      return false;
    }

    const start = new Date(ev.fechaInicio).getTime();
    const now = Date.now();
    const diffHours = (start - now) / (1000 * 60 * 60);
    if (diffHours <= 1) {
      return false;
    }

    if (ev.entradasDisponibles <= 0) {
      return false;
    }

    return true;
  });

  public readonly bookingNotAvailableMessage = computed(() => {
    const ev = this.evento();
    if (!ev) return null;

    if (ev.estadoEventoId.toUpperCase() !== EVENT_STATUS.ACTIVO) {
      return 'Este evento no está disponible para realizar reservas porque no se encuentra activo.';
    }

    const start = new Date(ev.fechaInicio).getTime();
    const now = Date.now();
    const diffHours = (start - now) / (1000 * 60 * 60);
    if (diffHours <= 1) {
      return 'No es posible reservar entradas para eventos que inician en menos de una hora.';
    }

    if (ev.entradasDisponibles <= 0) {
      return 'No hay entradas disponibles para este evento.';
    }

    return null;
  });

  constructor() {
    this.initForm();
  }

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      const id = Number(idParam);
      if (Number.isNaN(id)) {
        this.errorMessage.set('ID de evento inválido.');
      } else {
        this.loadEvento(id);
      }
    } else {
      this.errorMessage.set('No se proporcionó un ID de evento.');
    }
  }

  private initForm(): void {
    this.bookingForm = this.fb.group({
      fullName: ['', [Validators.required]],
      email: ['', [Validators.required, Validators.email]],
      quantity: [1, [Validators.required, Validators.min(1)]],
    });
  }

  private loadEvento(id: number): void {
    this.eventoService.getEventoById(id).subscribe({
      next: (ev) => {
        this.evento.set(ev);
        this.calculateLimits(ev);
      },
      error: (err) => {
        console.error(err);
        this.errorMessage.set('Error al cargar el evento.');
      },
    });
  }

  private calculateLimits(ev: EventoDto): void {
    const start = new Date(ev.fechaInicio).getTime();
    const now = Date.now();
    const diffHours = (start - now) / (1000 * 60 * 60);

    let maxQty = null;
    if (diffHours < 24) {
      maxQty = 5;
    } else if (ev.precio > 100) {
      maxQty = 10;
    } else {
      maxQty = ev.entradasDisponibles;
    }

    this.maxAllowedTickets.set(maxQty);

    const qtyControl = this.bookingForm.get('quantity');
    if (qtyControl) {
      const validators = [Validators.required, Validators.min(1)];
      if (maxQty !== null) {
        validators.push(Validators.max(maxQty));
      }
      qtyControl.setValidators(validators);
      qtyControl.updateValueAndValidity();
    }
  }

  protected onSubmit(): void {
    if (this.bookingForm.invalid || !this.evento()) {
      this.bookingForm.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    const formValue = this.bookingForm.value;
    const payload: CrearReserva = {
      eventoId: this.evento()!.eventoId,
      nombreComprador: formValue.fullName,
      emailComprador: formValue.email,
      cantidad: Number(formValue.quantity),
    };

    this.reservaService.realizarReserva(payload).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.successMessage.set('¡Reserva realizada con éxito!');
        setTimeout(() => {
          this.router.navigate(['/mis-reservas']);
        }, 2000);
      },
      error: (err) => {
        this.isSubmitting.set(false);
        const apiMessage = err.error?.message || err.error || 'Error al realizar la reserva.';
        this.errorMessage.set(apiMessage);
      },
    });
  }
}
