using Microsoft.AspNetCore.Http;
using SistemaContable.Application.Services.Interfaces;
using System.Security.Claims;

namespace SistemaContable.Application.Services.Implementations
{
    public class TokenDataService : ITokenDataService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TokenDataService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ClaimsPrincipal User => _httpContextAccessor.HttpContext?.User 
            ?? throw new UnauthorizedAccessException("El contexto HTTP no tiene un usuario autenticado.");

        public int GetUsuarioId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(value, out int id))
                return id;
            throw new UnauthorizedAccessException("El token no contiene un ID de usuario válido.");
        }

        public string GetEmail() => User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;

        public string GetNombreCompleto() => User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;

        public int GetEmpresaId()
        {
            var value = User.FindFirstValue("EmpresaId");
            if (int.TryParse(value, out int id))
                return id;
            return 0; // O lanzar excepción dependiendo de tu lógica de negocio
        }

        public string GetRuc() => User.FindFirstValue("RUC") ?? string.Empty;

        public string GetEmpresaNombre() => User.FindFirstValue("EmpresaNombre") ?? string.Empty;

        public string GetRol() => User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
    }
}
