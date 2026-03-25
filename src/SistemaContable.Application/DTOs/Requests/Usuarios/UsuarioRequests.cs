
namespace SistemaContable.Application.DTOs.Requests.Usuarios
{
    public class CrearUsuarioRequest
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string NombreCompleto { get; set; }
        public string Rol { get; set; } = "Usuario"; // Admin, Contador, Usuario
        public bool Activo { get; set; } = true;
    }

    public class ActualizarUsuarioRequest
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string NombreCompleto { get; set; }
        public string Rol { get; set; }
        public string Password { get; set; } // Opcional, si viene vacío no se cambia
    }
}
