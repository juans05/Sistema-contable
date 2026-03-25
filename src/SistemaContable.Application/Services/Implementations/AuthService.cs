
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SistemaContable.Application.DTOs.Requests;
using SistemaContable.Application.DTOs.Responses;
using SistemaContable.Application.Services.Interfaces;
using SistemaContable.Application.Services.Interfaces.IRepository;
using SistemaContable.Domain.Entities;
using SistemaContable.Domain.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;


namespace SistemaContable.Application.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;     // ✅ Del Domain
        private readonly IPasswordService _passwordService;   // ✅ Del Domain
        private readonly IJwtTokenService _jwtTokenService;   // ✅ Del Application
        private readonly ILogger<AuthService> _logger;
        public AuthService(
            IAuthRepository authRepository,        // ✅ Interfaz del Domain
            IPasswordService passwordService,      // ✅ Interfaz del Domain
            IJwtTokenService jwtTokenService,
            ILogger<AuthService> logger)
        {
            _authRepository = authRepository;
            _passwordService = passwordService;
            _jwtTokenService = jwtTokenService;
            _logger = logger;
        }

        public  async Task<MeResponse?> GetCurrentUserAsync(int pusuario_id, Guid ruc_empresa_id)
        {
            try
            {
                _logger.LogInformation("Obteniendo perfil del usuario: {username}", pusuario_id);

                // ✅ Llamar al repositorio del Domain
                var profileData = await _authRepository.GetUserMeAsync(pusuario_id, ruc_empresa_id);

                // Mapear de Domain Model a Application DTO
                return MapToMeResponse(profileData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo perfil del usuario");
                throw;
            }
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            try
            {
                _logger.LogInformation("Procesando login para: {Email}", request.Username);

                // ✅ Llamar al repositorio del Domain
                var loginData = await _authRepository.LoginAsync(request);

                if (!loginData.Success)
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = loginData.Message
                    };
                }

                //// Validar password
                //if (!_passwordService.VerifyPassword(request.Password, loginData.password_hash))
                //{
                //    _logger.LogWarning("Contraseña incorrecta para: {Email}", request.Username);
                //    return new LoginResponse
                //    {
                //        Success = false,
                //        Message = "Contraseña incorrecta"
                //    };
                //}

                // Actualizar último acceso
                await _authRepository.UpdateLastAccessAsync(loginData.usuario_id);

                // Generar tokens
                var token = _jwtTokenService.GenerateToken(loginData);
                var refreshToken = _jwtTokenService.GenerateRefreshToken();

                // Guardar refresh token
                var tokenHash = _passwordService.HashPassword(refreshToken);
                await _authRepository.SaveRefreshTokenAsync(
                    loginData.usuario_id,
                    tokenHash,
                    DateTime.UtcNow.AddDays(7));

                _logger.LogInformation("Login exitoso para: {Email}", request.Username);

                return new LoginResponse
                {
                    Success = true,
                    Message = "Login exitoso",
                    Token = token,
                    RefreshToken = refreshToken,
                    UserData = new UserSessionData
                    {
                        Username = loginData.username,
                        Email = loginData.Email,
                        NombreCompleto = loginData.NombreCompleto,
                        ruc = loginData.Ruc,
                        EmpresaNombre = loginData.EmpresaNombre,
                        Rol = loginData.Rol
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el login");
                throw;
            }
        }

        public async Task LogoutAsync(int usuarioId)
        {
            await _authRepository.InvalidateRefreshTokensAsync(usuarioId);
            _logger.LogInformation("Usuario {UsuarioId} cerró sesión", usuarioId);
        }

        public async Task<LoginResponse> RefreshTokenAsync(string refreshToken)
        {
            return await _authRepository.RefreshTokenAsync(refreshToken);
        }

        public async Task<LoginResponse> SwitchEmpresaAsync(int pusuario_id, string email, Guid ruc_empresa_id)
        {
            try
            {
                _logger.LogInformation("Usuario {UsuarioId} cambiando a empresa: {EmpresaId}",
                    pusuario_id, ruc_empresa_id);

                // Obtener perfil en nueva empresa
                var profileData = await _authRepository.GetUserMeAsync(pusuario_id, ruc_empresa_id);

                // Generar nuevo token
                var loginData = new LoginData
                {
                    Success = true,
                    usuario_id = profileData.UsuarioId,
                    Email = profileData.Email,
                    NombreCompleto = profileData.NombreCompleto,
                    EmpresaNombre = profileData.EmpresaNombre,
                    Rol = profileData.Rol,
                    Activo = profileData.Activo
                };

                var token = _jwtTokenService.GenerateToken(loginData);
                var refreshToken = _jwtTokenService.GenerateRefreshToken();

                var tokenHash = _passwordService.HashPassword(refreshToken);
                await _authRepository.SaveRefreshTokenAsync(
                    profileData.UsuarioId, tokenHash, DateTime.UtcNow.AddDays(7));

                return new LoginResponse
                {
                    Success = true,
                    Message = "Empresa cambiada exitosamente",
                    Token = token,
                    RefreshToken = refreshToken,
                    UserData = new UserSessionData
                    {
                        UsuarioId = profileData.UsuarioId,                        
                        Email = profileData.Email,
                        NombreCompleto = profileData.NombreCompleto,
                        EmpresaNombre = profileData.EmpresaNombre,
                        Rol = profileData.Rol
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar de empresa");
                throw;
            }
        }

       

        private MeResponse MapToMeResponse(MeResponse profileData)
        {
            return new MeResponse
            {
                UsuarioId = profileData.UsuarioId,
                Email = profileData.Email,
                Username = profileData.Username,
                Activo = profileData.Activo,
                FechaCreacion = profileData.FechaCreacion,
                UltimoAcceso = profileData.UltimoAcceso,

                Ruc = profileData.Ruc,
                EmpresaNombre = profileData.EmpresaNombre,
                EmpresaLogo = profileData.EmpresaLogo,
                Rol = profileData.Rol,
                RolDescripcion = profileData.RolDescripcion,

                Permisos = profileData.Permisos,
                Menus = profileData.Menus.Select(m => new MenuPermiso
                {
                    MenuId = m.MenuId,
                    MenuKey = m.MenuKey,
                    Titulo = m.Titulo,
                    Descripcion = m.Descripcion,
                    Icono = m.Icono,
                    ParentId = m.ParentId,
                    Orden = m.Orden,
                    Ruta = m.Ruta,
                    Tipo = m.Tipo,
                    PuedeVer = m.PuedeVer,
                    PuedeCrear = m.PuedeCrear,
                    PuedeEditar = m.PuedeEditar,
                    PuedeEliminar = m.PuedeEliminar,
                    PuedeExportar = m.PuedeExportar,
                    EsFavorito = m.EsFavorito
                }).ToList(),
                EmpresasDisponibles = profileData.EmpresasDisponibles.Select(e => new EmpresaDisponible
                {
                    EmpresaId = e.EmpresaId,
                    EmpresaNombre = e.EmpresaNombre,
                    EmpresaLogo = e.EmpresaLogo,
                    Rol = e.Rol,
                    EsPrincipal = e.EsPrincipal
                }).ToList(),
                Configuracion = new ConfiguracionUsuario
                {
                    Tema = profileData.Configuracion.Tema,
                    Idioma = profileData.Configuracion.Idioma,
                    NotificacionesEmail = profileData.Configuracion.NotificacionesEmail,
                    NotificacionesPush = profileData.Configuracion.NotificacionesPush,
                    Timezone = profileData.Configuracion.Timezone
                }
            };
        }



        public async Task<List<EmpresaDisponible>> PreLoginAsync(string username, string password)
        {
            var result = await _authRepository.ValidarCredencialesAsync(username, password);
            return result.Success ? result.Empresas : null;
        }
    }
}
