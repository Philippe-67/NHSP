using ApiPraticien.Data;
using ApiPraticien.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiPraticien.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PraticienController : ControllerBase
    {
        private readonly PraticienDbContext _context;
        private readonly ILogger<PraticienController> logger;

        public PraticienController(PraticienDbContext context, ILogger<PraticienController> logger)
        {
            _context = context;
            this.logger = logger;

        }

        // GET: api/Praticien
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Praticien>>> GetPraticiens()
        {
            //_logger.LogInformation("Fetching list of Praticiens");
            return await _context.Praticiens.ToListAsync();
            //_logger.LogInformation("Retrieved {count} praticiens", praticiens.Count);

        }
    }
}
