using EventosVivos_Api.Models;
using EventosVivos_Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventosVivos_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VenuesController : ControllerBase
    {
        private readonly IVenueService _venueService;

        public VenuesController(IVenueService venueService)
        {
            _venueService = venueService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Venue>>> GetVenues()
        {
            var venues = await _venueService.GetAllVenues();
            return Ok(venues);
        }
    }
}
