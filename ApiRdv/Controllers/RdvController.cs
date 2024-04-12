using ApiRdv.Data;
using ApiRdv.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

//[Authorize(Roles="admin,praticien")]
[Route("api/[controller]")]
[ApiController]
public class RdvController : ControllerBase
{
    private readonly RdvDbContext _context;
    private static SemaphoreSlim _reservationSemaphore = new SemaphoreSlim(200, 200);
    private static SemaphoreSlim _doubleBookingSemaphore = new SemaphoreSlim(1, 1);

    public RdvController(RdvDbContext context)
    {
        _context = context;
    }

    // GET: api/Rdv
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Rdv>>> GetRdvs()
    {
        return await _context.Rdvs.ToListAsync();
    }
    // GET: api/Rdv/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Rdv>> GetRdv(int id)
    {
        var rdv = await _context.Rdvs.FindAsync(id);

        if (rdv == null)
        {
            return NotFound();
        }

        return rdv;
    }

    [HttpPost]
    public async Task<ActionResult<Rdv>> Create(Rdv rdv)
    {
        await _reservationSemaphore.WaitAsync();
        await _doubleBookingSemaphore.WaitAsync();

        try
        {

            //// Logique de réservation (vérification de disponibilité, etc.)
            if (!IsDayAvailable(rdv.Date, rdv.NomPraticien))
            {
                return BadRequest("La journée spécifiée n'est plus disponible pour la réservation.");
            }

            // Simulation d'une opération prenant moins d'une minute
            await Task.Delay(TimeSpan.FromSeconds(1));

            _context.Rdvs.Add(rdv);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRdv), new { id = rdv.Id }, rdv);
        }
        finally
        {
            _doubleBookingSemaphore.Release();
            _reservationSemaphore.Release();
        }
    }
    private bool IsDayAvailable(DateTime date, string nomPraticien)
    {
        // Logique de vérification de disponibilité de la journée
        // Par exemple, vérifier si la journée est déjà réservée
        //, return !_context.Rdvs.Any(r => r.Date.Date == date.Date);
        return !_context.Rdvs.Any(r => r.Date.Date == date.Date && r.NomPraticien == nomPraticien);
    }




    [Authorize(Roles = "praticien")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var rdv = await _context.Rdvs.FindAsync(id);
        if (rdv == null)
        {
            return NotFound();
        }

        await _reservationSemaphore.WaitAsync();

        try
        {
            _context.Rdvs.Remove(rdv);
            await _context.SaveChangesAsync();
        }
        finally
        {
            _reservationSemaphore.Release();
        }

        return NoContent();
    }

}




