import { Component, Input, Output, EventEmitter } from '@angular/core';
import { DatePipe, CurrencyPipe } from '@angular/common';
import { ReservaDto } from '../../../../core/services/reserva.service';

@Component({
  selector: 'app-card-reserva',
  imports: [DatePipe, CurrencyPipe],
  templateUrl: './card-reserva.html',
})
export class CardReserva {
  @Input({ required: true }) reserva!: ReservaDto;
  @Output() cancel = new EventEmitter<number>();

  protected get imagenEvento(): string {
    switch (this.reserva.tipoEventoId?.toUpperCase()) {
      case 'TALLER':
        return '/assets/img/taller.webp';
      case 'CONCIERTO':
        return '/assets/img/concierto.webp';
      default:
        return '/assets/img/conferencia.webp';
    }
  }

  protected cancelReserva(): void {
    if (confirm('¿Está seguro de que desea cancelar esta reserva?')) {
      this.cancel.emit(this.reserva.reservaId);
    }
  }
}
