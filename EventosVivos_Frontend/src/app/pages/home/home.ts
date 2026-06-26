import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { EventoService, ReporteEventoDto } from '../../core/services/evento.service';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-home',
  imports: [DatePipe, RouterLink],
  templateUrl: './home.html',
})
export class Home implements OnInit {
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

  protected getTextoOcupacion(ocupacion: number): string {
    if (ocupacion > 100) {
      return `${ocupacion}% Excedido`;
    }
    return `${ocupacion}% Ocupación`;
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
      default:
        return estadoEventoId || 'Desconocido';
    }
  }

  protected getEstadoClass(estadoEventoId: string, ocupacion: number): string {
    if (ocupacion > 100) {
      return 'bg-error-container text-error border-error/20';
    }
    switch (estadoEventoId?.toUpperCase()) {
      case 'ACTIVO':
        return 'bg-tertiary/10 text-tertiary border-tertiary/20';
      case 'COMPLETADO':
        return 'bg-outline-variant/30 text-on-surface-variant border-outline-variant';
      default:
        return 'bg-outline-variant/30 text-on-surface-variant border-outline-variant';
    }
  }
}
