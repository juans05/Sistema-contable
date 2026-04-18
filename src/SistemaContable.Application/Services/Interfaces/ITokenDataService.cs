using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace SistemaContable.Application.Services.Interfaces
{
    public interface ITokenDataService
    {
        int GetUsuarioId();
        string GetEmail();
        string GetNombreCompleto();
        int GetEmpresaId();
        string GetRuc();
        string GetEmpresaNombre();
        string GetRol();
    }
}
