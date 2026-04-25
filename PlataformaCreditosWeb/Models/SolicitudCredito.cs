using System.ComponentModel.DataAnnotations;

namespace PlataformaCreditosWeb.Models;

public enum EstadoSolicitud { Pendiente, Aprobado, Rechazado }

public class SolicitudCredito
{
    public int Id { get; set; }

    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    [Range(1, double.MaxValue,
        ErrorMessage = "El monto debe ser mayor a 0")]
    public decimal MontoSolicitado { get; set; }

    public DateTime FechaSolicitud { get; set; } = DateTime.UtcNow;

    public EstadoSolicitud Estado { get; set; } = EstadoSolicitud.Pendiente;

    [StringLength(500)]
    public string? MotivoRechazo { get; set; }
}