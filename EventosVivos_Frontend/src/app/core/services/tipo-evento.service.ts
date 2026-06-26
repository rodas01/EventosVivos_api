import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { getEndpoint } from '../constants/endpoints';

export interface TipoEvento {
  tipoEventoId: string;
  descripcion: string;
}

@Injectable({
  providedIn: 'root',
})
export class TipoEventoService {
  private readonly baseUrl: string = environment.baseUrl;

  constructor(private readonly http: HttpClient) {}

  get(): Observable<TipoEvento[]> {
    return this.http.get<TipoEvento[]>(`${this.baseUrl}${getEndpoint('tipoEventos')}`);
  }
}
