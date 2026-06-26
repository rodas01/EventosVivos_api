export interface EndpointConfig {
  name: string;
  endpoint: string;
}

export const ENDPOINTS: EndpointConfig[] = [
  { name: 'login', endpoint: 'api/Auth/login' },
  { name: 'loging', endpoint: 'api/Auth/login' },
  { name: 'logout', endpoint: 'api/Auth/logout' },
  { name: 'tipoEventos', endpoint: 'api/TipoEventos' },
  { name: 'venues', endpoint: 'api/Venues' },
  { name: 'eventos', endpoint: 'api/Eventos' },
  { name: 'eventoById', endpoint: 'api/Eventos/*' },
  { name: 'crearEvento', endpoint: 'api/Eventos/crear-evento' },
  { name: 'reporteEvento', endpoint: 'api/Eventos/reporte' },
  { name: 'reservas', endpoint: 'api/Reservas' },
  { name: 'consultarReservaCliente', endpoint: 'api/Reservas/cliente/*' },
  { name: 'cancelarReserva', endpoint: 'api/Reservas/*/cancelar' },
  { name: 'confirmarPago', endpoint: 'api/Reservas/*/confirmar-pago' }
];

export function getEndpoint(name: string, ...args: (string | number)[]): string {
  const config = ENDPOINTS.find((e) => e.name === name);
  if (!config) {
    throw new Error(`Endpoint with name ${name} not found`);
  }
  let path = config.endpoint;
  for (const arg of args) {
    path = path.replace('*', arg.toString());
  }
  return path;
}
