using EventosVivos_Api.Data;
using EventosVivos_Api.DTO;
using EventosVivos_Api.Models;
using Microsoft.EntityFrameworkCore;


namespace EventosVivos_Api.Services
{
    public class TipoEventoService : ITipoEventoService
    {
        private readonly ApplicationDbContext _context;

        public TipoEventoService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TipoEventoDto>> GetAll()
        {
            var tipos_eventos = await _context.TiposEventos.ToListAsync();
            return tipos_eventos.Select(te => new TipoEventoDto
            {
                TipoEventoId = te.TipoEventoId,
                Descripcion = te.Descripcion
            }); ;
        }
    }
}
