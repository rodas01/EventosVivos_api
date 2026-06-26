import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe, CurrencyPipe } from '@angular/common';
import { EventoService, ReporteEventoDto } from '../../core/services/evento.service';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-reporte-eventos',
  imports: [DatePipe, CurrencyPipe, RouterLink],
  templateUrl: './reporte-eventos.html',
})
export class ReporteEventos implements OnInit {
  private readonly eventoService = inject(EventoService);

  public readonly reportes = signal<ReporteEventoDto[]>([]);

  ngOnInit(): void {
    this.eventoService.getReporteEventos().subscribe({
      next: (data) => {
        this.reportes.set(data);
      },
      error: (err) => {
        console.error('Error fetching event reports:', err);
      },
    });
  }

  protected getImagenEvento(tipoEventoId: string): string {
    switch (tipoEventoId?.toUpperCase()) {
      case 'CONCIERTO':
        return '/assets/img/concierto.webp';
      case 'TALLER':
        return '/assets/img/taller.webp';
      default:
        return '/assets/img/conferencia.webp';
    }
  }

  protected getEstadoTexto(estadoEventoId: string, ocupacion: number): string {
    if (ocupacion > 100) {
      return 'Crítico';
    }
    switch (estadoEventoId?.toUpperCase()) {
      case 'ACTIVO':
        return 'Activo';
      case 'COMPLETADO':
        return 'Completado';
      case 'CANCELADO':
        return 'Cancelado';
      default:
        return estadoEventoId || 'Desconocido';
    }
  }

  protected getEstadoClass(estadoEventoId: string, ocupacion: number): string {
    if (ocupacion > 100) {
      return 'bg-[#fee2e2] text-[#991b1b]';
    }
    switch (estadoEventoId?.toUpperCase()) {
      case 'ACTIVO':
        return 'bg-[#d1fae5] text-[#065f46]';
      case 'COMPLETADO':
        return 'bg-surface-container-highest text-on-secondary-fixed';
      case 'CANCELADO':
        return 'bg-[#fee2e2] text-[#991b1b]';
      default:
        return 'bg-surface-container-highest text-on-secondary-fixed';
    }
  }
}
