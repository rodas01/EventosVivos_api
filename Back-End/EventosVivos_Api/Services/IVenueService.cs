using EventosVivos_Api.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventosVivos_Api.Services
{
    public interface IVenueService
    {
        Task<IEnumerable<Venue>> GetAllVenues();
    }
}
