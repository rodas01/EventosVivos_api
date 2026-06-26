import { Component, OnInit, signal, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CardReserva } from './components/card-reserva/card-reserva';
import { ReservaService, ReservaDto } from '../../core/services/reserva.service';

@Component({
  selector: 'app-mis-reservas',
  imports: [CardReserva, ReactiveFormsModule],
  templateUrl: './mis-reservas.html',
})
export class MisReservas implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly reservaService = inject(ReservaService);

  public searchForm!: FormGroup;
  public readonly reservas = signal<ReservaDto[]>([]);
  public readonly hasSearched = signal<boolean>(false);
  public readonly isSearching = signal<boolean>(false);
  public readonly errorMessage = signal<string | null>(null);

  ngOnInit(): void {
    this.searchForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
    });
  }

  protected onSearch(): void {
    if (this.searchForm.invalid) {
      this.searchForm.markAllAsTouched();
      return;
    }

    const email = this.searchForm.value.email;
    this.fetchReservas(email);
  }

  private fetchReservas(email: string): void {
    this.isSearching.set(true);
    this.errorMessage.set(null);

    this.reservaService.getReservasByEmail(email).subscribe({
      next: (data) => {
        this.reservas.set(data);
        this.hasSearched.set(true);
        this.isSearching.set(false);
      },
      error: (err) => {
        console.error(err);
        const msg = err.error?.message || err.error || 'Error al consultar las reservas.';
        this.errorMessage.set(msg);
        this.isSearching.set(false);
      }
    });
  }

  protected onCancelReserva(reservaId: number): void {
    this.reservaService.cancelarReserva(reservaId).subscribe({
      next: () => {
        const email = this.searchForm.get('email')?.value;
        if (email) {
          this.fetchReservas(email);
        }
      },
      error: (err) => {
        const msg = err.error?.message || err.error || 'Error al cancelar la reserva.';
        this.errorMessage.set(msg);
      }
    });
  }
}
