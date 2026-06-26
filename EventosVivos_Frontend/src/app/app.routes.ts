import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'eventos',
    pathMatch: 'full',
  },
  {
    path: 'eventos',
    loadComponent: () => import('./pages/eventos/eventos').then((m) => m.Eventos),
    data: { internal: false },
  },
  {
    path: 'mis-reservas',
    loadComponent: () => import('./pages/mis-reservas/mis-reservas').then((m) => m.MisReservas),
    data: { internal: false },
  },
  {
    path: 'reservar/:id',
    loadComponent: () => import('./pages/reservas/reservas').then((m) => m.Reservas),
    data: { internal: false },
  },
  {
    path: 'home',
    loadComponent: () => import('./pages/home/home').then((m) => m.Home),
    canActivate: [authGuard],
    data: { internal: true },
  },
  {
    path: 'reporte-eventos',
    loadComponent: () =>
      import('./pages/reporte-eventos/reporte-eventos').then((m) => m.ReporteEventos),
    canActivate: [authGuard],
    data: { internal: true },
  },
  {
    path: 'crear-evento',
    loadComponent: () => import('./pages/crear-eventos/crear-eventos').then((m) => m.CrearEventos),
    canActivate: [authGuard],
    data: { internal: true },
  },
  {
    path: 'gestion-reservas',
    loadComponent: () =>
      import('./pages/gestion-reservas/gestion-reservas').then((m) => m.GestionReservas),
    canActivate: [authGuard],
    data: { internal: true },
  },
];
