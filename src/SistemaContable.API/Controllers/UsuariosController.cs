using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SistemaContable.Application.DTOs.Requests.Usuarios;
using SistemaContable.Application.Services.Interfaces.IRepository;
using SistemaContable.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SistemaContable.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Require Auth
    public class UsuariosController : ControllerBase
    {
        private readonly IAuthRepository _userRepository;
        private readonly ILogger<UsuariosController> _logger;

        public UsuariosController(IAuthRepository userRepository, ILogger<UsuariosController> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<EUsuario>>> ListarUsuarios()
        {
            try
            {
                var usuarios = await _userRepository.ListarUsuariosAsync();
                return Ok(usuarios);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listando usuarios");
                return StatusCode(500, "Error listando usuarios");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EUsuario>> ObtenerUsuario(int id)
        {
            try
            {
                var usuario = await _userRepository.ObtenerUsuarioPorIdAsync(id);
                if (usuario == null) return NotFound("Usuario no encontrado");
                
                // Ocultar hash
                usuario.PasswordHash = null; 
                return Ok(usuario);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo usuario {Id}", id);
                return StatusCode(500, "Error interno");
            }
        }

        [HttpPost]
        public async Task<ActionResult> CrearUsuario([FromBody] CrearUsuarioRequest request)
        {
            try
            {
                var usuario = new EUsuario
                {
                    Username = request.Username,
                    Email = request.Email,
                    NombreCompleto = request.NombreCompleto,
                    Rol = request.Rol,
                    Activo = request.Activo
                };

                var id = await _userRepository.CrearUsuarioAsync(usuario, request.Password);
                return CreatedAtAction(nameof(ObtenerUsuario), new { id }, new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando usuario {Usuario}", request.Username);
                return StatusCode(500, "Error creando usuario");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> ActualizarUsuario(int id, [FromBody] ActualizarUsuarioRequest request)
        {
            try
            {
                var usuario = await _userRepository.ObtenerUsuarioPorIdAsync(id);
                if (usuario == null) return NotFound("Usuario no encontrado");

                usuario.Username = request.Username;
                usuario.Email = request.Email;
                usuario.NombreCompleto = request.NombreCompleto;
                usuario.Rol = request.Rol;

                var result = await _userRepository.ActualizarUsuarioAsync(usuario, request.Password);
                if (!result) return BadRequest("No se pudo actualizar el usuario");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando usuario {Id}", id);
                return StatusCode(500, "Error actualizando usuario");
            }
        }

        [HttpPut("{id}/bloquear")]
        public async Task<ActionResult> CambiarEstado(int id, [FromBody] bool activo)
        {
            try
            {
                var result = await _userRepository.CambiarEstadoUsuarioAsync(id, activo);
                if (!result) return NotFound("Usuario no encontrado o no actualizado");
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cambiando estado de usuario {Id}", id);
                return StatusCode(500, "Error cambiando estado");
            }
        }
    }
}
