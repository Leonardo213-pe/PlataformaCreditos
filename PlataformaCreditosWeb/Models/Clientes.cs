using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlataformaCreditosWeb.Models;

public class Cliente
{
    public int Id { get; set; }

    [Required]
    public string UsuarioId { get; set; } = string.Empty;
    public ApplicationUser Usuario { get; set; } = null!;

    [Range(1, double.MaxValue,
        ErrorMessage = "Los ingresos deben ser mayores a 0")]
    public decimal IngresosMensuales { get; set; }

    public bool Activo { get; set; } = true;

    public ICollection<SolicitudCredito> Solicitudes { get; set; }
        = new List<SolicitudCredito>();

    [NotMapped]
    public decimal MontoMaximo => IngresosMensuales * 5;
}