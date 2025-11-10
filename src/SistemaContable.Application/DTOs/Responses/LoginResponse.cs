using SistemaContable.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.Application.DTOs.Responses
{
    public class LoginResponse
    {
        public bool Success { get; init; }
        public string Message { get; init; } = string.Empty;
        public string Token { get; init; } = string.Empty;
        public string RefreshToken { get; init; } = string.Empty;
        public UserSessionData? UserData { get; init; }
    }
    public record UserSessionData
    {
        public int UsuarioId { get; init; }
        public string Username { get; init; }
        public string Email { get; init; } = string.Empty;
        public string NombreCompleto { get; init; } = string.Empty;
        public string ruc { get; init; }
        public string EmpresaNombre { get; init; } = string.Empty;
        public string Rol { get; init; } = string.Empty;
    }
}
