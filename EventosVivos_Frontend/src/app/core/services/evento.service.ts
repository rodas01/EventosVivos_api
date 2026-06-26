import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { getEndpoint } from '../constants/endpoints';

export interface VenueDto {
  nombre: string;
}

export interface EventoDto {
  eventoId: number;
  nombreEvento: string;
  descripcion: string;
  venue: VenueDto;
  capacidad: number;
  fechaInicio: string;
  fechaFin: string;
  precio: number;
  tipoEventoId: string;
  estadoEventoId: string;
  soldOut: boolean;
  entradasDisponibles: number;
}

export interface FiltrosEvento {
  tipoEvento?: string;
  fechaInicio?: string;
  fechaFin?: string;
  venueId?: number;
  estado?: string;
  titulo?: string;
}

export interface CrearEvento {
  titulo: string;
  descripcion: string;
  idVenue: number;
  capacidadMaxima: number;
  fechaHoraInicio: string;
  fechaHoraFin: string;
  precioEntrada: number;
  tipoEventoId: string;
}

export interface ReporteEventoDto {
  nombreEvento: string;
  descripcion: string;
  venue: VenueDto;
  capacidad: number;
  fechaInicio: string;
  fechaFin: string;
  precio: number;
  tipoEventoId: string;
  estadoEventoId: string;
  ocupacion: number;
  reservasDisponibles: number;
  reservasVendidas: number;
  totalIngresos: number;
}

@Injectable({
  providedIn: 'root',
})
export class EventoService {
  private readonly baseUrl: string = environment.baseUrl;

  constructor(private readonly http: HttpClient) {}

  getEventos(filtros?: FiltrosEvento): Observable<EventoDto[]> {
    let params = new HttpParams();

    if (filtros) {
      if (filtros.tipoEvento) params = params.set('tipoEvento', filtros.tipoEvento);
      if (filtros.fechaInicio) params = params.set('fechaInicio', filtros.fechaInicio);
      if (filtros.fechaFin) params = params.set('fechaFin', filtros.fechaFin);
      if (filtros.venueId !== undefined && filtros.venueId !== null) {
        params = params.set('venueId', filtros.venueId.toString());
      }
      if (filtros.estado) params = params.set('estado', filtros.estado);
      if (filtros.titulo) params = params.set('titulo', filtros.titulo);
    }

    return this.http.get<EventoDto[]>(`${this.baseUrl}${getEndpoint('eventos')}`, { params });
  }

  crearEvento(evento: CrearEvento): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}${getEndpoint('crearEvento')}`, evento);
  }

  getReporteEventos(): Observable<ReporteEventoDto[]> {
    return this.http.get<ReporteEventoDto[]>(`${this.baseUrl}${getEndpoint('reporteEvento')}`);
  }

  getEventoById(id: number): Observable<EventoDto> {
    return this.http.get<EventoDto>(`${this.baseUrl}${getEndpoint('eventoById', id)}`);
  }
}
