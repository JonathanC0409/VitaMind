using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VitaMind.Models;

namespace VitaMind.Apii.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MetricaSistemasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MetricaSistemasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/MetricaSistemas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MetricaSistema>>> GetMetricaSistema()
        {
            return await _context.MetricaSistemas.ToListAsync();
        }

        // GET: api/MetricaSistemas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MetricaSistema>> GetMetricaSistema(Guid id)
        {
            var metricaSistema = await _context.MetricaSistemas.FindAsync(id);

            if (metricaSistema == null)
            {
                return NotFound();
            }

            return metricaSistema;
        }

        // PUT: api/MetricaSistemas/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMetricaSistema(Guid id, MetricaSistema metricaSistema)
        {
            if (id != metricaSistema.IdMetrica)
            {
                return BadRequest();
            }

            _context.Entry(metricaSistema).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MetricaSistemaExists(id))
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

        // POST: api/MetricaSistemas
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<MetricaSistema>> PostMetricaSistema(MetricaSistema metricaSistema)
        {
            _context.MetricaSistemas.Add(metricaSistema);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMetricaSistema", new { id = metricaSistema.IdMetrica }, metricaSistema);
        }

        // DELETE: api/MetricaSistemas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMetricaSistema(Guid id)
        {
            var metricaSistema = await _context.MetricaSistemas.FindAsync(id);
            if (metricaSistema == null)
            {
                return NotFound();
            }

            _context.MetricaSistemas.Remove(metricaSistema);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MetricaSistemaExists(Guid id)
        {
            return _context.MetricaSistemas.Any(e => e.IdMetrica == id);
        }
    }
}
