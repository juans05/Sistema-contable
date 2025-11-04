using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using SistemaContable.Application.DTOs.Requests;
using SistemaContable.Application.DTOs.Responses;
using SistemaContable.Application.Services.Interfaces.IRepository;
using SistemaContable.Domain.Entities;
using SistemaContable.Domain.Models;
using SistemaContable.Infrastructure.Data.Repositories.Interfaces;
using SistemaContable.Infrastructure.Models;
using System.Text.Json;

namespace SistemaContable.Infrastructure.Data.Repositories.Implementations
{
    public class UserRepository : IAuthRepository
    {
        private readonly string _connectionString;

        public UserRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string no configurada");
        }

        public Task<RefreshTokenData?> GetRefreshTokenAsync(string tokenHash)
        {
            throw new NotImplementedException();
        }

        public async Task<MeResponse?> GetUserMeAsync(int usuarioId, Guid empresaId)
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                var result = await connection.QueryFirstOrDefaultAsync<MeDbResult>(
                     "SELECT * FROM \"suizaConta\".sp_me4(@p_usuario_id, @p_empresa_id)",
                     new { p_usuario_id = usuarioId, p_empresa_id = empresaId }
                 );
                if (result == null)
                {
                    throw new UnauthorizedAccessException("Usuario no tiene acceso");
                }

                // Mapear resultado interno → Domain Model
                return new MeResponse
                {
                    UsuarioId = result.usuario_id,
                    Email = result.email,
                    Username = result.username,
                    Activo = result.activo,
                    FechaCreacion = result.fecha_creacion,
                    UltimoAcceso = result.ultimo_acceso,


                    NombreCompleto = result.nombre_completo,

                    Telefono = result.telefono,
                    Ruc = result.empresa_ruc,
                    EmpresaNombre = result.empresa_nombre,
                    EmpresaLogo = result.empresa_logo,
                    Rol = result.Rol,
                    RolDescripcion = result.RolDescripcion,

                    // Parsear JSONB a objetos
                    Permisos = ParseJson<Dictionary<string, bool>>(result.Permisos),
                    Menus = ParseJson<List<MenuPermiso>>(result.Menus),
                    EmpresasDisponibles = ParseJson<List<EmpresaDisponible>>(result.EmpresasDisponibles),
                    Configuracion = ParseJson<ConfiguracionUsuario>(result.Configuracion)
                };
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task InvalidateRefreshTokensAsync(int usuarioId)
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.ExecuteAsync(
                    "UPDATE \"suizaConta\".refresh_tokens SET activo = FALSE WHERE usuario_id = @UsuarioId",
                    new { UsuarioId = usuarioId },
                    commandTimeout: 10
                );
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error invalidando refresh tokens");
                throw;
            }
        }

        public async Task<LoginData> LoginAsync(LoginRequest request)
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            try
            {
                var result = await connection.QueryFirstOrDefaultAsync<LoginDbResult>(
                " select * from  \"suizaConta\".sp_login(@p_username, @p_ruc)",
                new { p_username = request.Username, p_ruc = request.RucEmpresa }
            );

                if (result == null)
                {
                    return new LoginData
                    {
                        Success = false,
                        Message = "Usuario no encontrado"
                    };
                }

                // Mapear de DbResult (interno) a Domain Model
                return new LoginData
                {
                    Success = result.Success,
                    Message = result.Message,
                    username = result.username,
                    Email = result.Email,
                    NombreCompleto = result.nombre_completo,
                    password_hash = result.password_hash,
                    Ruc = result.empresa_ruc,
                    EmpresaNombre = result.empresa_nombre,
                    Rol = result.rol,
                    empresa_id =  result.empresa_id,
                    usuario_id= result.usuario_id,
                    Activo = result.activo
                };
            }
            catch (Exception ex)
            {

                throw;
            }
        }

      

        public async Task SaveRefreshTokenAsync(int usuarioId, string tokenHash, DateTime expiration)
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.ExecuteAsync(@"
                INSERT INTO ""suizaConta"".refresh_tokens (usuario_id, token_hash, fecha_expiracion, activo)
                VALUES (@UsuarioId, @TokenHash, @Expiration, TRUE)",
                    new
                    {
                        UsuarioId = usuarioId,
                        TokenHash = tokenHash,
                        Expiration = expiration
                    },
                    commandTimeout: 10
                );
            }
            catch (Exception ex)
            {
               // _logger.LogError(ex, "Error guardando refresh token");
                throw;
            }
        }

        public async Task UpdateLastAccessAsync(int usuarioId)
        {
            try
            {
                int idUsuario = usuarioId;
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.ExecuteAsync(
                    "UPDATE \"suizaConta\".usuarios SET ultimo_acceso = NOW() WHERE id = @UsuarioId",
                    new { UsuarioId = idUsuario },
                    commandTimeout: 10
                );
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error actualizando último acceso");
                // No lanzar excepción, es un update no crítico
            }
        }

        private T ParseJson<T>(string? json) where T : new()
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return new T();
            }

            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                return JsonSerializer.Deserialize<T>(json, options) ?? new T();
            }
            catch (Exception ex)
            {
               // _logger.LogWarning(ex, "Error parseando JSON: {Json}", json);
                return new T();
            }
        }
    }
}
