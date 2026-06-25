using EventosVivos_Api.Models;
using EventosVivos_Api.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventosVivos_Api.Services
{
    public interface ITipoEventoService
    {
        Task<IEnumerable<TipoEventoDto>> GetAll();
    }
}
