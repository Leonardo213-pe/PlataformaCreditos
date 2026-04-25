using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using PlataformaCreditosWeb.Data;
using PlataformaCreditosWeb.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PlataformaCreditosWeb.Controllers;

[Authorize]
public class SolicitudesController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IDistributedCache _cache;
    private const string CACHE_PREFIX = "solicitudes_";

    private static readonly JsonSerializerOptions _jsonOpts = new()
    {
        ReferenceHandler = ReferenceHandler.Preserve
    };

    public SolicitudesController(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        IDistributedCache cache)
    {
        _db = db;
        _userManager = userManager;
        _cache = cache;
    }

    // GET: /Solicitudes/MisSolicitudes
    public async Task<IActionResult> MisSolicitudes(
        string? estado,
        decimal? montoMin,
        decimal? montoMax,
        string? fechaDesde,
        string? fechaHasta)
    {
        var userId = _userManager.GetUserId(User)!;
        var cacheKey = CACHE_PREFIX + userId;

        // Verificar si hay filtros
        bool hayFiltros = !string.IsNullOrEmpty(estado)
            || montoMin.HasValue
            || montoMax.HasValue
            || !string.IsNullOrEmpty(fechaDesde)
            || !string.IsNullOrEmpty(fechaHasta);

        // Validación server-side: rango de fechas inválido
        if (!string.IsNullOrEmpty(fechaDesde) && !string.IsNullOrEmpty(fechaHasta))
        {
            if (DateTime.TryParse(fechaDesde, out var dDesde) &&
                DateTime.TryParse(fechaHasta, out var dHasta))
            {
                if (dDesde > dHasta)
                {
                    ModelState.AddModelError("",
                        "La fecha de inicio no puede ser mayor a la fecha fin.");
                    TempData["Error"] =
                        "La fecha de inicio no puede ser mayor a la fecha fin.";
                }
            }
        }

        List<SolicitudCredito> todasSolicitudes;

        if (!hayFiltros)
        {
            // Intentar desde caché
            var cached = await _cache.GetStringAsync(cacheKey);
            if (cached != null)
            {
                todasSolicitudes = JsonSerializer
                    .Deserialize<List<SolicitudCredito>>(cached, _jsonOpts)
                    ?? new List<SolicitudCredito>();
            }
            else
            {
                todasSolicitudes = await ObtenerSolicitudesUsuario(userId);
                var opciones = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60)
                };
                await _cache.SetStringAsync(
                    cacheKey,
                    JsonSerializer.Serialize(todasSolicitudes, _jsonOpts),
                    opciones);
            }
        }
        else
        {
            todasSolicitudes = await ObtenerSolicitudesUsuario(userId);
        }

        // Aplicar filtros en memoria
        var solicitudes = todasSolicitudes.AsEnumerable();

        if (!string.IsNullOrEmpty(estado) &&
            Enum.TryParse<EstadoSolicitud>(estado, out var estadoEnum))
            solicitudes = solicitudes.Where(s => s.Estado == estadoEnum);

        // Validación: no aceptar montos negativos
        if (montoMin.HasValue)
        {
            if (montoMin < 0)
            {
                TempData["Error"] = "El monto mínimo no puede ser negativo.";
                montoMin = null;
            }
            else
            {
                solicitudes = solicitudes.Where(s => s.MontoSolicitado >= montoMin.Value);
            }
        }

        if (montoMax.HasValue)
        {
            if (montoMax < 0)
            {
                TempData["Error"] = "El monto máximo no puede ser negativo.";
                montoMax = null;
            }
            else
            {
                solicitudes = solicitudes.Where(s => s.MontoSolicitado <= montoMax.Value);
            }
        }

        if (!string.IsNullOrEmpty(fechaDesde) &&
            DateTime.TryParse(fechaDesde, out var desde))
            solicitudes = solicitudes.Where(s => s.FechaSolicitud >= desde);

        if (!string.IsNullOrEmpty(fechaHasta) &&
            DateTime.TryParse(fechaHasta, out var hasta))
            solicitudes = solicitudes.Where(s => s.FechaSolicitud <= hasta.AddDays(1));

        ViewBag.Estado = estado;
        ViewBag.MontoMin = montoMin;
        ViewBag.MontoMax = montoMax;
        ViewBag.FechaDesde = fechaDesde;
        ViewBag.FechaHasta = fechaHasta;

        return View(solicitudes.ToList());
    }

    // GET: /Solicitudes/Detalle/5
    public async Task<IActionResult> Detalle(int id)
    {
        var userId = _userManager.GetUserId(User)!;

        var solicitud = await _db.SolicitudesCredito
            .Include(s => s.Cliente)
            .FirstOrDefaultAsync(s =>
                s.Id == id && s.Cliente.UsuarioId == userId);

        if (solicitud == null)
            return NotFound();

        // Guardar última solicitud visitada en sesión
        HttpContext.Session.SetString("UltimaSolicitudId", id.ToString());
        HttpContext.Session.SetString("UltimaSolicitudMonto",
            solicitud.MontoSolicitado.ToString("N2"));

        return View(solicitud);
    }

    // Método privado reutilizable
    private async Task<List<SolicitudCredito>> ObtenerSolicitudesUsuario(string userId)
    {
        return await _db.SolicitudesCredito
            .Include(s => s.Cliente)
            .Where(s => s.Cliente.UsuarioId == userId)
            .OrderByDescending(s => s.FechaSolicitud)
            .ToListAsync();
    }
    // GET: /Solicitudes/Nueva
[HttpGet]
public async Task<IActionResult> Nueva()
{
    var userId = _userManager.GetUserId(User)!;

    // Verificar si el usuario tiene un cliente activo
    var cliente = await _db.Clientes
        .FirstOrDefaultAsync(c => c.UsuarioId == userId);

    if (cliente == null)
    {
        TempData["Error"] =
            "No tienes un perfil de cliente registrado. " +
            "Contacta al administrador.";
        return RedirectToAction(nameof(MisSolicitudes));
    }

    if (!cliente.Activo)
    {
        TempData["Error"] = "Tu cuenta de cliente está inactiva.";
        return RedirectToAction(nameof(MisSolicitudes));
    }

    ViewBag.IngresosMensuales = cliente.IngresosMensuales;
    ViewBag.MontoMaximo = cliente.IngresosMensuales * 10;

    return View();
}

// POST: /Solicitudes/Nueva
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Nueva(decimal montoSolicitado)
{
    var userId = _userManager.GetUserId(User)!;

    // Obtener cliente del usuario
    var cliente = await _db.Clientes
        .Include(c => c.Solicitudes)
        .FirstOrDefaultAsync(c => c.UsuarioId == userId);

    // Validación 1: cliente debe existir
    if (cliente == null)
    {
        TempData["Error"] = "No tienes un perfil de cliente registrado.";
        return RedirectToAction(nameof(MisSolicitudes));
    }

    // Validación 2: cliente debe estar activo
    if (!cliente.Activo)
    {
        TempData["Error"] = "Tu cuenta de cliente está inactiva.";
        return RedirectToAction(nameof(MisSolicitudes));
    }

    // Validación 3: monto no puede ser negativo ni cero
    if (montoSolicitado <= 0)
    {
        TempData["Error"] = "El monto solicitado debe ser mayor a 0.";
        ViewBag.IngresosMensuales = cliente.IngresosMensuales;
        ViewBag.MontoMaximo = cliente.IngresosMensuales * 10;
        return View();
    }

    // Validación 4: no puede tener más de una solicitud Pendiente
    var tienePendiente = cliente.Solicitudes
        .Any(s => s.Estado == EstadoSolicitud.Pendiente);

    if (tienePendiente)
    {
        TempData["Error"] =
            "Ya tienes una solicitud en estado Pendiente. " +
            "Espera que sea procesada antes de crear una nueva.";
        return RedirectToAction(nameof(MisSolicitudes));
    }

    // Validación 5: monto no puede superar 10 veces los ingresos
    var montoMaximo = cliente.IngresosMensuales * 10;
    if (montoSolicitado > montoMaximo)
    {
        TempData["Error"] =
            $"El monto solicitado (S/ {montoSolicitado:N2}) no puede superar " +
            $"10 veces tus ingresos mensuales (S/ {montoMaximo:N2}).";
        ViewBag.IngresosMensuales = cliente.IngresosMensuales;
        ViewBag.MontoMaximo = montoMaximo;
        return View();
    }

    // Todo OK: crear solicitud en estado Pendiente
    var solicitud = new SolicitudCredito
    {
        ClienteId = cliente.Id,
        MontoSolicitado = montoSolicitado,
        Estado = EstadoSolicitud.Pendiente,
        FechaSolicitud = DateTime.UtcNow
    };

    _db.SolicitudesCredito.Add(solicitud);
    await _db.SaveChangesAsync();

    // Invalidar caché del usuario
    await _cache.RemoveAsync(CACHE_PREFIX + userId);

    TempData["Exito"] =
        $"Solicitud de S/ {montoSolicitado:N2} registrada correctamente. " +
        $"Estado: Pendiente.";

    return RedirectToAction(nameof(MisSolicitudes));
}
}