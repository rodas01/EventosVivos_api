import { getEndpoint } from './endpoints';

describe('Endpoints', () => {
  it('should return the correct endpoint path', () => {
    expect(getEndpoint('login')).toBe('api/Auth/login');
  });

  it('should replace asterisk with arguments', () => {
    expect(getEndpoint('eventoById', 123)).toBe('api/Eventos/123');
  });

  it('should throw an error for unknown endpoint name', () => {
    expect(() => getEndpoint('unknown-endpoint')).toThrow('Endpoint with name unknown-endpoint not found');
  });
});
