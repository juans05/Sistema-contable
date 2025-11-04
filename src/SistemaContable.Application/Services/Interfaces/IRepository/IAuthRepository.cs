using SistemaContable.Application.DTOs.Requests;
using SistemaContable.Application.DTOs.Responses;
using SistemaContable.Domain.Models;
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
    }
}
