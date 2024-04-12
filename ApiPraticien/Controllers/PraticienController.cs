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
        private readonly ILogger<PraticienController> _logger;

        public PraticienController(PraticienDbContext context, ILogger<PraticienController> logger)
        {
            _context = context;
            _logger = logger;

        }

        // GET: api/Praticien
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Praticien>>> GetPraticiens()
        {
            //_logger.LogInformation("Fetching list of Praticiens");
            return await _context.Praticiens.ToListAsync();
            //_logger.LogInformation("Retrieved {count} praticiens", praticiens.Count);

        }
        // GET: api/Praticien/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Praticien>> GetPraticien(int id)
        {
            var praticien = await _context.Praticiens.FindAsync(id);

            if (praticien == null)
            {
                return NotFound();
            }

            return praticien;
        }

        // Post: api/Praticien
        [HttpPost]
        public async Task<ActionResult<Praticien>> Create(Praticien praticien)
        {

            try
            {
                _context.Praticiens.Add(praticien);
                await _context.SaveChangesAsync();

                // return CreatedAtAction(nameof(Praticien), new { id = praticien.Id }, praticien);
                return CreatedAtAction(nameof(GetPraticiens), praticien);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "une erreur est survenue lors de la création du praticien.");
            }
        }
        // PUT: api/Praticien/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPraticien(int id, Praticien praticien)
        {
            if (id != praticien.Id)
            {
                return BadRequest();
            }

            _context.Entry(praticien).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PraticienExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Praticien/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePraticien(int id)
        {
            var praticien = await _context.Praticiens.FindAsync(id);
            if (praticien == null)
            {
                return NotFound();
            }

            _context.Praticiens.Remove(praticien);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PraticienExists(int id)
        {
            return _context.Praticiens.Any(e => e.Id == id);
        }
    }
}
