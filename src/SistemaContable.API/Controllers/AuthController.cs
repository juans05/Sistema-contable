

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaContable.Application.DTOs.Requests;
using SistemaContable.Application.DTOs.Responses;
using SistemaContable.Application.Services.Interfaces;
using SistemaContable.Domain.Models;
using SistemaContable.Domain.Entities;
using System.Security.Claims;

namespace SistemaContable.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly Application.Services.Interfaces.IRepository.IAuthRepository _authRepository;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, Application.Services.Interfaces.IRepository.IAuthRepository authRepository, ILogger<AuthController> logger)
        {
            _authService = authService;
            _authRepository = authRepository;
            _logger = logger;
        }
        /// <summary>
        /// Endpoint para iniciar sesión
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            try
            {
                _logger.LogInformation("Intento de login para usuario: {Username}", request.Username);

                // Flujo 1: Si no hay RUC, validamos credenciales y devolvemos lista de empresas
                if (string.IsNullOrEmpty(request.RucEmpresa))
                {
                    var empresas = await _authService.PreLoginAsync(request.Username, request.Password);
                    if (empresas == null)
                    {
                         _logger.LogWarning("Login pre-auth fallido para usuario: {Username}", request.Username);
                         return Unauthorized(new { message = "Usuario o contraseña incorrectos" });
                    }
                    
                    return Ok(new { 
                        NeedCompanySelection = true, 
                        Companies = empresas 
                    });
                }
                
                var response = await _authService.LoginAsync(request);

                if (response == null)
                {
                    _logger.LogWarning("Login fallido para usuario: {Username}", request.Username);
                    return Unauthorized(new { message = "Usuario o contraseña incorrectos" });
                }

                _logger.LogInformation("Login exitoso para usuario: {Username}", request.Username);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el login para usuario: {Username}", request.Username);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtiene la información del usuario autenticado
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserResponse>> GetCurrentUser()
        {
            try
            {
                // Extraer UserId del token JWT
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    _logger.LogWarning("Token válido pero sin UserId en claims");
                    return Unauthorized(new { message = "Usuario no identificado en el token" });
                }

                var userId = int.Parse(userIdClaim);

                // Extraer EmpresaId del token o del query parameter
                var empresaIdClaim = User.FindFirst("EmpresaId")?.Value;
                Guid currentEmpresaId;

                if (!string.IsNullOrEmpty(empresaIdClaim) && Guid.TryParse(empresaIdClaim, out var parsedEmpresaId))
                {
                    // Si viene en el token, usar ese
                    currentEmpresaId = parsedEmpresaId;
                }
                else
                {
                    _logger.LogWarning("No se encontró EmpresaId para el usuario: {UserId}", userId);
                    return BadRequest(new { message = "EmpresaId requerido" });
                }

                _logger.LogInformation("Obteniendo información completa del usuario: {UserId}, Empresa: {EmpresaId}",
                    userId, currentEmpresaId);

                // Llamar al servicio que ejecuta sp_me
                var userInfo = await _authService.GetCurrentUserAsync(userId, currentEmpresaId);

                if (userInfo == null)
                {
                    _logger.LogWarning("Usuario no encontrado: {UserId}", userId);
                    return NotFound(new { message = "Usuario no encontrado" });
                }

                _logger.LogInformation("Información obtenida exitosamente para usuario: {UserId}. Menús: {MenuCount}",
                    userId, userInfo.Menus?.Count ?? 0);

                return Ok(userInfo);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Acceso no autorizado al obtener información del usuario");
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener información del usuario");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Refresca el token de acceso
        /// </summary>
        [HttpPost("refresh")]
        [AllowAnonymous] // Cambiar a AllowAnonymous porque el access token puede estar expirado
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<LoginResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request?.RefreshToken))
                {
                    return BadRequest(new { message = "Refresh token requerido" });
                }

                var response = await _authService.RefreshTokenAsync(request.RefreshToken);

                if (!response.Success)
                {
                    return Unauthorized(new { message = response.Message });
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al refrescar token");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Cierra la sesión del usuario (opcional)
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Logout()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _logger.LogInformation("Usuario {UserId} cerró sesión", userId);

                // Aquí podrías invalidar el token en una lista negra si lo deseas
                // await _authService.InvalidateTokenAsync(userId);

                return Ok(new { message = "Sesión cerrada exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cerrar sesión");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }
        /// <summary>
        /// DEBUG: Listar usuarios
        /// </summary>
        [HttpGet("debug-users")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDebugUsers()
        {
            var users = await _authRepository.ListarUsuariosAsync();
            return Ok(users);
        }
    }
}
