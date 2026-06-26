import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { getEndpoint } from '../constants/endpoints';

export interface CrearReserva {
  eventoId: number;
  cantidad: number;
  nombreComprador: string;
  emailComprador: string;
}

export interface ReservaDto {
  reservaId: number;
  codigoReserva?: string;
  fechaReserva: string;
  cantidadEntradas: number;
  precioReserva: number;
  estadoReservaId: string;
  nombreCliente: string;
  correoCliente: string;
  tituloEvento: string;
  fechaEvento: string;
  nombreVenue: string;
  tipoEventoId: string;
}

@Injectable({
  providedIn: 'root',
})
export class ReservaService {
  private readonly baseUrl: string = environment.baseUrl;

  constructor(private readonly http: HttpClient) {}

  realizarReserva(dto: CrearReserva): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}${getEndpoint('reservas')}`, dto);
  }

  getReservas(): Observable<ReservaDto[]> {
    return this.http.get<ReservaDto[]>(`${this.baseUrl}${getEndpoint('reservas')}`);
  }

  getReservasByEmail(email: string): Observable<ReservaDto[]> {
    return this.http.get<ReservaDto[]>(`${this.baseUrl}${getEndpoint('consultarReservaCliente', email)}`);
  }

  cancelarReserva(id: number): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}${getEndpoint('cancelarReserva', id)}`, {});
  }

  confirmarPago(id: number): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}${getEndpoint('confirmarPago', id)}`, {});
  }
}
