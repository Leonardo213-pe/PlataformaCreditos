using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using PlataformaCreditosWeb.Data;
using PlataformaCreditosWeb.Models;

namespace PlataformaCreditosWeb.Controllers;

[Authorize(Roles = "Analista")]
public class AnalistaController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IDistributedCache _cache;
    private const string CACHE_PREFIX = "solicitudes_";

    public AnalistaController(
        ApplicationDbContext db,
        IDistributedCache cache)
    {
        _db = db;
        _cache = cache;
    }

    // GET: /Analista
    public async Task<IActionResult> Index()
    {
        var solicitudes = await _db.SolicitudesCredito
            .Include(s => s.Cliente)
            .ThenInclude(c => c.Usuario)
            .Where(s => s.Estado == EstadoSolicitud.Pendiente)
            .OrderBy(s => s.FechaSolicitud)
            .ToListAsync();

        return View(solicitudes);
    }

    // POST: /Analista/Aprobar
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Aprobar(int solicitudId)
    {
        var solicitud = await _db.SolicitudesCredito
            .Include(s => s.Cliente)
            .FirstOrDefaultAsync(s => s.Id == solicitudId);

        if (solicitud == null)
            return NotFound();

        if (solicitud.Estado != EstadoSolicitud.Pendiente)
        {
            TempData["Error"] = "Esta solicitud ya fue procesada anteriormente.";
            return RedirectToAction(nameof(Index));
        }

        var montoMaximo = solicitud.Cliente.IngresosMensuales * 5;
        if (solicitud.MontoSolicitado > montoMaximo)
        {
            TempData["Error"] =
                $"No se puede aprobar. El monto solicitado (S/ {solicitud.MontoSolicitado:N2}) " +
                $"supera 5 veces los ingresos del cliente (S/ {montoMaximo:N2}).";
            return RedirectToAction(nameof(Index));
        }

        solicitud.Estado = EstadoSolicitud.Aprobado;
        await _db.SaveChangesAsync();

        await _cache.RemoveAsync(CACHE_PREFIX + solicitud.Cliente.UsuarioId);

        TempData["Exito"] = $"Solicitud #{solicitud.Id} aprobada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Analista/Rechazar/5
    [HttpGet]
    public async Task<IActionResult> Rechazar(int id)
    {
        var solicitud = await _db.SolicitudesCredito
            .Include(s => s.Cliente)
            .ThenInclude(c => c.Usuario)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (solicitud == null)
            return NotFound();

        if (solicitud.Estado != EstadoSolicitud.Pendiente)
        {
            TempData["Error"] = "Esta solicitud ya fue procesada anteriormente.";
            return RedirectToAction(nameof(Index));
        }

        return View(solicitud);
    }

    // POST: /Analista/Rechazar
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Rechazar(int solicitudId, string motivoRechazo)
    {
        var solicitud = await _db.SolicitudesCredito
            .Include(s => s.Cliente)
            .FirstOrDefaultAsync(s => s.Id == solicitudId);

        if (solicitud == null)
            return NotFound();

        if (solicitud.Estado != EstadoSolicitud.Pendiente)
        {
            TempData["Error"] = "Esta solicitud ya fue procesada anteriormente.";
            return RedirectToAction(nameof(Index));
        }

        if (string.IsNullOrWhiteSpace(motivoRechazo))
        {
            ModelState.AddModelError("motivoRechazo", "El motivo de rechazo es obligatorio.");
            return View(solicitud);
        }

        solicitud.Estado = EstadoSolicitud.Rechazado;
        solicitud.MotivoRechazo = motivoRechazo.Trim();
        await _db.SaveChangesAsync();

        await _cache.RemoveAsync(CACHE_PREFIX + solicitud.Cliente.UsuarioId);

        TempData["Exito"] = $"Solicitud #{solicitud.Id} rechazada correctamente.";
        return RedirectToAction(nameof(Index));
    }
}