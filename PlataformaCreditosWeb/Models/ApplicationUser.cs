using Microsoft.AspNetCore.Identity;

namespace PlataformaCreditosWeb.Models;

public class ApplicationUser : IdentityUser
{
    public string NombreCompleto { get; set; } = string.Empty;
    public Cliente? Cliente { get; set; }
}