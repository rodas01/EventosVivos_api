import { Component, Input } from '@angular/core';
import { DatePipe, CurrencyPipe, TitleCasePipe, UpperCasePipe } from '@angular/common';
import { EventoDto } from '../../../../core/services/evento.service';
import { EVENT_TYPES, EVENT_STATUS } from '../../../../core/constants/event.constants';

@Component({
  selector: 'app-card-evento',
  imports: [DatePipe, CurrencyPipe, TitleCasePipe, UpperCasePipe],
  templateUrl: './card-evento.html',
})
export class CardEvento {
  @Input({ required: true }) evento!: EventoDto;

  protected readonly EVENT_STATUS = EVENT_STATUS;
  protected readonly EVENT_TYPES = EVENT_TYPES;
}
