using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using SistemaContable.Application.DTOs.Requests;
using SistemaContable.Application.DTOs.Responses;
using SistemaContable.Application.Services.Implementations;
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

        public async Task<RefreshTokenData?> GetRefreshTokenAsync(string tokenHash)
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                return await connection.QueryFirstOrDefaultAsync<RefreshTokenData>(
                    @"SELECT 
                    id AS Id, 
                    usuario_id AS UsuarioId, 
                    token_hash AS TokenHash, 
                    fecha_expiracion AS FechaExpiracion, 
                    activo AS Activo,
                    fecha_creacion AS FechaCreacion
                  FROM ""suizaConta"".refresh_tokens
                  WHERE token_hash = @TokenHash
                    AND activo = TRUE
                    AND fecha_expiracion > NOW()",
                    new { TokenHash = tokenHash },
                    commandTimeout: 10
                );
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error obteniendo refresh token");
                return null;
            }
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




        public async Task<LoginResponse> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                // _logger.LogDebug("Procesando refresh token");

                // Verificar el refresh token directamente (sin hashear para comparar)
                // Nota: Buscaremos por el token sin hashear primero
                var tokenData = await connection.QueryFirstOrDefaultAsync<RefreshTokenData>(
                    @"SELECT 
                    id AS Id, 
                    usuario_id AS UsuarioId, 
                    token_hash AS TokenHash, 
                    fecha_expiracion AS FechaExpiracion, 
                    activo AS Activo,
                    fecha_creacion AS FechaCreacion
                  FROM ""suizaConta"".refresh_tokens
                  WHERE activo = TRUE
                    AND fecha_expiracion > NOW()
                    AND usuario_id IN (
                        SELECT id FROM ""suizaConta"".usuarios WHERE activo = TRUE
                    )
                  ORDER BY fecha_creacion DESC
                  LIMIT 20",  // Obtener los últimos 20 tokens activos
                    commandTimeout: 10
                );

                // Verificar el token con BCrypt entre los resultados
                RefreshTokenData? validToken = null;
                if (tokenData != null)
                {
                    // Buscar todos los tokens activos del sistema y verificar
                    var allActiveTokens = await connection.QueryAsync<RefreshTokenData>(
                        @"SELECT 
                        id AS Id, 
                        usuario_id AS UsuarioId, 
                        token_hash AS TokenHash, 
                        fecha_expiracion AS FechaExpiracion, 
                        activo AS Activo,
                        fecha_creacion AS FechaCreacion
                      FROM ""suizaConta"".refresh_tokens
                      WHERE activo = TRUE
                        AND fecha_expiracion > NOW()",
                        commandTimeout: 10
                    );

                    foreach (var token in allActiveTokens)
                    {
                        if (_passwordService.VerifyPassword(refreshToken, token.TokenHash))
                        {
                            validToken = token;
                            break;
                        }
                    }
                }

                if (validToken == null)
                {
                    // _logger.LogWarning("Refresh token no encontrado o expirado");
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Refresh token inválido o expirado"
                    };
                }

                // Obtener datos del usuario
                var usuario = await connection.QueryFirstOrDefaultAsync<UsuarioDataResult>(
                    @"SELECT 
                    u.id AS UsuarioId,
                    u.email AS Email,
                    u.username AS Username,
                    u.nombre_completo AS NombreCompleto,
                    u.es_contador AS EsContador,
                    u.es_super_admin AS EsSuperAdmin,
                    u.activo AS Activo
                  FROM ""suizaConta"".usuarios u
                  WHERE u.id = @UsuarioId
                    AND u.activo = TRUE
                    AND u.deleted_at IS NULL",
                    new { UsuarioId = validToken.UsuarioId },
                    commandTimeout: 10
                );

                if (usuario == null)
                {
                    //_logger.LogWarning("Usuario no encontrado o inactivo para refresh token");
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Usuario no encontrado o inactivo"
                    };
                }

                // Obtener la empresa principal del usuario
                var empresaData = await connection.QueryFirstOrDefaultAsync<EmpresaUsuarioResult>(
                    @"SELECT 
                    eu.empresa_id AS EmpresaId,
                    e.nombre_comercial AS EmpresaNombre,
                    eu.rol AS Rol
                  FROM ""suizaConta"".empresa_usuarios eu
                  INNER JOIN ""suizaConta"".empresas e ON e.id = eu.empresa_id
                  WHERE eu.usuario_id = @UsuarioId
                    AND eu.activo = TRUE
                    AND e.activo = TRUE
                    AND (eu.fecha_fin IS NULL OR eu.fecha_fin >= CURRENT_DATE)
                  ORDER BY eu.fecha_inicio DESC
                  LIMIT 1",
                    new { UsuarioId = validToken.UsuarioId },
                    commandTimeout: 10
                );

                if (empresaData == null)
                {
                    //   _logger.LogWarning("Usuario sin empresas asignadas");
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Usuario sin empresas asignadas"
                    };
                }

                // Invalidar el refresh token usado (one-time use)
                await connection.ExecuteAsync(
                    @"UPDATE ""suizaConta"".refresh_tokens 
                  SET activo = FALSE 
                  WHERE id = @Id",
                    new { Id = validToken.Id },
                    commandTimeout: 10
                );

                // Generar nuevos tokens
                var userData = new UserSessionData
                {
                    UsuarioId = usuario.UsuarioId,
                    Email = usuario.Email,
                    Username = usuario.Username,
                    NombreCompleto = usuario.NombreCompleto,
                    EsContador = usuario.EsContador,
                    EsSuperAdmin = usuario.EsSuperAdmin,
                    EmpresaId = empresaData.EmpresaId,
                    EmpresaNombre = empresaData.EmpresaNombre,
                    Rol = empresaData.Rol |
                };

                var newToken = _jwtTokenService.GenerateToken(userData);
                var newRefreshToken = _jwtTokenService.GenerateRefreshToken();

                // Guardar el nuevo refresh token
                var newTokenHash = PasswordService.HashPassword(newRefreshToken);
                await SaveRefreshTokenAsync(
                    usuario.UsuarioId,
                    newTokenHash,
                    DateTime.UtcNow.AddDays(7)
                );

                // Actualizar último acceso
                await UpdateLastAccessAsync(usuario.UsuarioId);

                //_logger.LogInformation("Refresh token exitoso para usuario: {UsuarioId}", usuario.UsuarioId);

                return new LoginResponse
                {
                    Success = true,
                    Message = "Token renovado exitosamente",
                    Token = newToken,
                    RefreshToken = newRefreshToken,
                    UserData = userData
                };
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error procesando refresh token");
                throw;
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
