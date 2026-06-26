import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { getEndpoint } from '../constants/endpoints';

export interface Venue {
  venueId: number;
  nombre: string;
  capacidadMaxima: number;
  ubicacion: string;
}

@Injectable({
  providedIn: 'root',
})
export class VenueService {
  private readonly baseUrl: string = environment.baseUrl;

  constructor(private readonly http: HttpClient) {}

  get(): Observable<Venue[]> {
    return this.http.get<Venue[]>(`${this.baseUrl}${getEndpoint('venues')}`);
  }
}
