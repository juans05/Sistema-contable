using SistemaContable.Application.DTOs.Requests;
using SistemaContable.Application.DTOs.Responses;
using SistemaContable.Domain.Models;
using SistemaContable.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.Application.Services.Interfaces.IRepository
{
    public  interface IAuthRepository
    {
        Task<LoginData> LoginAsync(LoginRequest request);
        Task<MeResponse?> GetUserMeAsync(int usuarioId, Guid empresaId);
        Task UpdateLastAccessAsync(int usuarioId);
        Task SaveRefreshTokenAsync(int usuarioId, string tokenHash, DateTime expiration);
        Task InvalidateRefreshTokensAsync(int usuarioId);
        Task<RefreshTokenData?> GetRefreshTokenAsync(string tokenHash);

        Task<LoginResponse> RefreshTokenAsync(string refreshToken);
        Task<List<EUsuario>> ListarUsuariosAsync();
        Task<EUsuario> ObtenerUsuarioPorIdAsync(int id);
        Task<int> CrearUsuarioAsync(EUsuario usuario, string password);
        Task<bool> ActualizarUsuarioAsync(EUsuario usuario, string password = null);
        Task<bool> CambiarEstadoUsuarioAsync(int id, bool activo);
        Task<(bool Success, int? UsuarioId, List<EmpresaDisponible> Empresas)> ValidarCredencialesAsync(string username, string password);
    }
}
