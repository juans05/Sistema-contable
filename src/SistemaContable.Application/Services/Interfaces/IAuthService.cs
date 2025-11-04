
using SistemaContable.Application.DTOs.Requests;
using SistemaContable.Application.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.Application.Services.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task<MeResponse> GetCurrentUserAsync(int pusuario_id, Guid ruc_empresa_id);
        Task<LoginResponse> SwitchEmpresaAsync(int pusuario_id, string email, Guid ruc_empresa_id);
        Task LogoutAsync(int usuarioId);
        Task<LoginResponse> RefreshTokenAsync(string refreshToken);
    }
}
