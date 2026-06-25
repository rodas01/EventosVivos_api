using EventosVivos_Api.DTO;
using EventosVivos_Api.Models;
using EventosVivos_Api.Util;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventosVivos_Api.Services
{
    public interface IEventoService
    {
        Task<Result<Evento>> CrearEventoAsync(CrearEventoDto eventoDto);
        Task<IEnumerable<EventoDto>> GetEventosAsync(FiltrosEventoDto filtrosDto);
        Task<IEnumerable<ReporteEventoDto>> GetReporteEventosAsync();
    }
}
