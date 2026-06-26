import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe, CurrencyPipe, NgClass } from '@angular/common';
import { ReservaService, ReservaDto } from '../../core/services/reserva.service';

@Component({
  selector: 'app-gestion-reservas',
  imports: [DatePipe, CurrencyPipe, NgClass],
  templateUrl: './gestion-reservas.html',
})
export class GestionReservas implements OnInit {
  private readonly reservaService = inject(ReservaService);

  public readonly reservas = signal<ReservaDto[]>([]);
  public readonly isModalOpen = signal<boolean>(false);
  public readonly modalType = signal<'confirmPayment' | 'cancelBooking' | null>(null);
  public readonly selectedReserva = signal<ReservaDto | null>(null);
  public readonly isProcessing = signal<boolean>(false);
  public readonly errorMessage = signal<string | null>(null);

  ngOnInit(): void {
    this.loadReservas();
  }

  public loadReservas(): void {
    this.reservaService.getReservas().subscribe({
      next: (data) => {
        const sorted = [...data].sort((a, b) => {
          const aPending = a.estadoReservaId?.toUpperCase() === 'PAGO_PENDIENTE';
          const bPending = b.estadoReservaId?.toUpperCase() === 'PAGO_PENDIENTE';
          if (aPending && !bPending) return -1;
          if (!aPending && bPending) return 1;
          return 0;
        });
        this.reservas.set(sorted);
      },
      error: (err) => {
        console.error('Error loading reservations:', err);
        this.errorMessage.set(err.error?.message || err.error || 'Error al cargar las reservas.');
      }
    });
  }

  protected getClienteInitials(name: string): string {
    if (!name) return '';
    const parts = name.trim().split(/\s+/);
    if (parts.length >= 2) {
      return (parts[0][0] + parts[1][0]).toUpperCase();
    }
    return parts[0][0]?.toUpperCase() || '';
  }

  protected openConfirmModal(reserva: ReservaDto, type: 'confirmPayment' | 'cancelBooking'): void {
    this.selectedReserva.set(reserva);
    this.modalType.set(type);
    this.isModalOpen.set(true);
    this.errorMessage.set(null);
  }

  protected closeConfirmModal(): void {
    this.isModalOpen.set(false);
    this.modalType.set(null);
    this.selectedReserva.set(null);
  }

  protected confirmAction(): void {
    const reserva = this.selectedReserva();
    const type = this.modalType();
    if (!reserva || !type) return;

    this.isProcessing.set(true);
    this.errorMessage.set(null);

    const action$ = type === 'confirmPayment'
      ? this.reservaService.confirmarPago(reserva.reservaId)
      : this.reservaService.cancelarReserva(reserva.reservaId);

    action$.subscribe({
      next: () => {
        this.isProcessing.set(false);
        this.closeConfirmModal();
        this.loadReservas();
      },
      error: (err) => {
        console.error('Error performing booking action:', err);
        this.isProcessing.set(false);
        this.errorMessage.set(err.error?.message || err.error || 'Error al procesar la solicitud.');
      }
    });
  }
}
