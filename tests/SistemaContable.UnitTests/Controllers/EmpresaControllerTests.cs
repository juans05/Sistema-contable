using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SistemaContable.API.Controllers;
using SistemaContable.Application.DTOs.Common;
using SistemaContable.Application.DTOs.Requests.Contadores;
using SistemaContable.Application.DTOs.Requests.Empresa;
using SistemaContable.Application.DTOs.Responses.Contador;
using SistemaContable.Application.DTOs.Responses.Empresa;
using SistemaContable.Application.Services.Interfaces;
using SistemaContable.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.UnitTests.Controllers
{
    public class EmpresaControllerTests
    {
        private readonly Mock<IEmpresaService> _mockService;
        private readonly Mock<ILogger<EmpresaController>> _mockLogger;
        private readonly EmpresaController _controller;

        public EmpresaControllerTests()
        {
            _mockService = new Mock<IEmpresaService>();
            _mockLogger = new Mock<ILogger<EmpresaController>>();
            _controller = new EmpresaController(_mockService.Object, _mockLogger.Object);

            // Setup User Claims
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Email, "test@test.com")
        };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        [Fact]
        public async Task ListarEmpresas_RetornaOkConListaPaginada()
        {
            // Arrange
            var request = new EmpresaQueryRequest { PageNumber = 1, PageSize = 10 };
            var expectedResponse = new PagedResultDto<EEmpresa>
            {
                TotalRecords = 2,
                Data = new List<EEmpresa>
                {
                    new() { nombreComercial = "Empresa 1", ruc = "20123456789" },
                    new() { nombreComercial = "Empresa 2", ruc = "20987654321" }
                }
            };

            _mockService
                .Setup(x => x.ListarEmpresasAsync(It.IsAny<EmpresaQueryRequest>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.ListarEmpresas(request);

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);

            var response = okResult.Value as PagedEmpresaResponse;
            Assert.NotNull(response);
            Assert.True(response.Success);
            Assert.Equal(2, response.Data!.Data.Count);
        }

        [Fact]
        public async Task ListarEmpresas_CuandoOcurreError_Retorna500()
        {
            // Arrange
            var request = new EmpresaQueryRequest();
            _mockService
                .Setup(x => x.ListarEmpresasAsync(It.IsAny<EmpresaQueryRequest>()))
                .ThrowsAsync(new Exception("Error de prueba"));

            // Act
            var result = await _controller.ListarEmpresas(request);

            // Assert
            var statusResult = result.Result as ObjectResult;
            Assert.NotNull(statusResult);
            Assert.Equal(500, statusResult.StatusCode);
        }
        [Fact]
        public async Task ListarEmpresas_ConFiltros_RetornaResultadosFiltrados()
        {
            // Arrange
            var request = new EmpresaQueryRequest
            {
                Search = "Test",
                Activo = true,
                PageNumber = 1,
                PageSize = 10
            };

            var expectedResponse = new PagedResultDto<EEmpresa>
            {
                TotalRecords = 1,
                Data = new List<EEmpresa>
                {
                    new() { nombreComercial = "Test Empresa", ruc = "20123456789" }
                }
            };

            _mockService
                .Setup(x => x.ListarEmpresasAsync(request))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.ListarEmpresas(request);

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.NotNull(okResult);
            var response = okResult.Value as PagedEmpresaResponse;
            Assert.Equal(1, response!.Data!.Data.Count);
        }


        #region ObtenerEmpresa Tests

        [Fact]
        public async Task ObtenerEmpresa_EmpresaExiste_RetornaOk()
        {
            // Arrange
            var empresaId = Guid.NewGuid();
            var expectedResponse = new EEmpresa
            {
                id = empresaId,
                nombreComercial = "Test Empresa",
                ruc = "20123456789"
            };

            _mockService
                .Setup(x => x.ObtenerEmpresaPorIdAsync(empresaId))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.ObtenerEmpresa(empresaId);

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.NotNull(okResult);
            var response = okResult.Value as EmpresaResponse;
            Assert.Equal(empresaId, response!.Id);
        }

       

        [Fact]
        public async Task ObtenerEmpresa_CuandoOcurreError_Retorna500()
        {
            // Arrange
            var empresaId = Guid.NewGuid();
            _mockService
                .Setup(x => x.ObtenerEmpresaPorIdAsync(empresaId))
                .ThrowsAsync(new Exception("Error"));

            // Act
            var result = await _controller.ObtenerEmpresa(empresaId);

            // Assert
            var statusResult = result.Result as ObjectResult;
            Assert.Equal(500, statusResult!.StatusCode);
        }

        #endregion

        #region CrearEmpresa Tests

        [Fact]
        public async Task CrearEmpresa_ConDatosValidos_RetornaCreated()
        {
            // Arrange
            var request = new CreateEmpresaRequest
            {
                razonSocial = "Nueva Empresa S.A.C.",
                nombreComercial = "Nueva",
                ruc = "20123456789"
            };

            var empresaId = Guid.NewGuid();

            var expectedResponse = new ApiResponseDto<Guid>
            {
                Success = true,
                Message = "Empresa creada exitosamente",
                Data = empresaId  // ✅ Data es Guid, no EmpresaId
            };
            _mockService
                .Setup(x => x.CrearEmpresaAsync(It.IsAny<CreateEmpresaRequest>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var actionResult = await _controller.CrearEmpresa(request);

            // Assert
            // ✅ Forma correcta de extraer el resultado
            Assert.NotNull(actionResult);
            Assert.NotNull(actionResult.Result);

            var createdResult = actionResult.Result as CreatedAtActionResult;
            Assert.NotNull(createdResult);
            Assert.Equal(201, createdResult!.StatusCode);

            var response = createdResult.Value as CreateEmpresaResponse;
            Assert.NotNull(response);
            Assert.True(response!.Success);
            Assert.NotEqual(Guid.Empty, response.EmpresaId);
            Assert.Equal("Empresa creada exitosamente", response.Message);
        }

        [Fact]
        public async Task CrearEmpresa_ConModelStateInvalido_RetornaBadRequest()
        {
            // Arrange
            var request = new CreateEmpresaRequest();
            _controller.ModelState.AddModelError("RazonSocial", "Required");

            // Act
            var result = await _controller.CrearEmpresa(request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task CrearEmpresa_CuandoFalla_RetornaBadRequest()
        {
            // Arrange
            var request = new CreateEmpresaRequest
            {
                razonSocial = "Test",
                nombreComercial = "Test",
                ruc = "20123456789"
            };
            var empresaId = Guid.NewGuid();

            var expectedResponse = new ApiResponseDto<Guid>
            {
                Success = true,
                Message = "Empresa creada exitosamente",
                Data = empresaId  // ✅ Data es Guid, no EmpresaId
            };
           

            _mockService
                .Setup(x => x.CrearEmpresaAsync(request))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.CrearEmpresa(request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task CrearEmpresa_CuandoOcurreExcepcion_Retorna500()
        {
            // Arrange
            var request = new CreateEmpresaRequest
            {
                razonSocial = "Test",
                nombreComercial = "Test",
                ruc = "20123456789"
            };

            _mockService
                .Setup(x => x.CrearEmpresaAsync(request))
                .ThrowsAsync(new Exception("Error"));

            // Act
            var result = await _controller.CrearEmpresa(request);

            // Assert
            var statusResult = result.Result as ObjectResult;
            Assert.Equal(500, statusResult!.StatusCode);
        }

        #endregion

        #region ActualizarEmpresa Tests

        [Fact]
        public async Task ActualizarEmpresa_ConDatosValidos_RetornaOk()
        {
            var empresaId = Guid.NewGuid();
            var request = new UpdateEmpresaRequest
            {
                id = empresaId,
                razonSocial = "Actualizada",
                nombreComercial = "Actualizada",
                ruc = "20123456789"
            };

            // ✅ Crear ApiResponseDto<bool> en lugar de solo bool
            var expectedResponse = new ApiResponseDto<bool>
            {
                Success = true,
                Message = "Empresa actualizada exitosamente",
                Data = true  // ← El valor booleano va aquí
            };

            _mockService
                .Setup(x => x.ActualizarEmpresaAsync(It.IsAny<UpdateEmpresaRequest>()))  // ✅ Usar It.IsAny
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.ActualizarEmpresa(empresaId, request);

            // Assert
            Assert.NotNull(result.Result);
            var okResult = result.Result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult!.StatusCode);

            // ✅ Verificar el contenido del response
            var response = okResult.Value as ApiResponseDto<bool>;
            Assert.NotNull(response);
            Assert.True(response!.Success);
            Assert.True(response.Data);
            Assert.Equal("Empresa actualizada exitosamente", response.Message);

            // ✅ Verificar que se llamó al servicio
            _mockService.Verify(
                x => x.ActualizarEmpresaAsync(It.IsAny<UpdateEmpresaRequest>()),
                Times.Once
            );
        }

        [Fact]
        public async Task ActualizarEmpresa_IdNoCoincide_RetornaBadRequest()
        {
            // Arrange
            var empresaId = Guid.NewGuid();
            var request = new UpdateEmpresaRequest
            {
                id = Guid.NewGuid(),
                razonSocial = "Test",
                nombreComercial = "Test",
                ruc = "20123456789"
            };

            // Act
            var result = await _controller.ActualizarEmpresa(empresaId, request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task ActualizarEmpresa_ModelStateInvalido_RetornaBadRequest()
        {
            // Arrange
            var empresaId = Guid.NewGuid();
            var request = new UpdateEmpresaRequest { id = empresaId };
            _controller.ModelState.AddModelError("RazonSocial", "Required");

            // Act
            var result = await _controller.ActualizarEmpresa(empresaId, request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task ActualizarEmpresa_EmpresaNoExiste_Retorna404()
        {
            // Arrange
            var empresaId = Guid.NewGuid();
            var request = new UpdateEmpresaRequest
            {
                id = empresaId,
                razonSocial = "Test",
                nombreComercial = "Test",
                ruc = "20123456789"
            };

            var expectedResponse = new ApiResponseDto<bool>
            {
                Success = false,
                Message = "Empresa no encontrada",
                Data = false
            };

            _mockService
                .Setup(x => x.ActualizarEmpresaAsync(It.IsAny<UpdateEmpresaRequest>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.ActualizarEmpresa(empresaId, request);

            // Assert
            Assert.NotNull(result.Result);
            var notFoundResult = result.Result as NotFoundObjectResult;
            Assert.NotNull(notFoundResult);
            Assert.Equal(404, notFoundResult!.StatusCode);

            var response = notFoundResult.Value as ApiResponseDto<bool>;
            Assert.NotNull(response);
            Assert.False(response!.Success);
            Assert.Contains("no encontrada", response.Message.ToLower());
        }

        #endregion

        #region EliminarEmpresa Tests

        [Fact]
        public async Task EliminarEmpresa_EmpresaExiste_RetornaOk()
        {
            // Arrange
            var empresaId = Guid.NewGuid();

            // ✅ Cambiar a ApiResponseDto<bool>
            var expectedResponse = new ApiResponseDto<bool>
            {
                Success = true,
                Message = "Empresa eliminada exitosamente",
                Data = true
            };

            _mockService
                .Setup(x => x.EliminarEmpresaAsync(empresaId))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.EliminarEmpresa(empresaId);

            // Assert
            Assert.NotNull(result.Result);
            var okResult = result.Result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult!.StatusCode);

            var response = okResult.Value as ApiResponseDto<bool>;
            Assert.NotNull(response);
            Assert.True(response!.Success);
            Assert.True(response.Data);
            Assert.Equal("Empresa eliminada exitosamente", response.Message);
        }

        [Fact]
        public async Task EliminarEmpresa_EmpresaNoExiste_Retorna404()
        {
            // Arrange
            var empresaId = Guid.NewGuid();

            // ✅ Cambiar a ApiResponseDto<bool>
            var expectedResponse = new ApiResponseDto<bool>
            {
                Success = false,
                Message = "Empresa no encontrada",
                Data = false
            };

            _mockService
                .Setup(x => x.EliminarEmpresaAsync(empresaId))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.EliminarEmpresa(empresaId);

            // Assert
            Assert.NotNull(result.Result);
            var notFoundResult = result.Result as NotFoundObjectResult;
            Assert.NotNull(notFoundResult);
            Assert.Equal(404, notFoundResult!.StatusCode);

            var response = notFoundResult.Value as ApiResponseDto<bool>;
            Assert.NotNull(response);
            Assert.False(response!.Success);
            Assert.Contains("no encontrada", response.Message);
        }

        [Fact]
        public async Task EliminarEmpresa_CuandoOcurreError_Retorna500()
        {
            // Arrange
            var empresaId = Guid.NewGuid();
            _mockService
                .Setup(x => x.EliminarEmpresaAsync(empresaId))
                .ThrowsAsync(new Exception("Error"));

            // Act
            var result = await _controller.EliminarEmpresa(empresaId);

            // Assert
            var statusResult = result.Result as ObjectResult;
            Assert.Equal(500, statusResult!.StatusCode);
        }
        [Fact]
        public async Task ActualizarEmpresa_RucDuplicado_RetornaBadRequest()
        {
            // Arrange
            var empresaId = Guid.NewGuid();
            var request = new UpdateEmpresaRequest
            {
                id = empresaId,
                razonSocial = "Test",
                nombreComercial = "Test",
                ruc = "20123456789"
            };

            var expectedResponse = new ApiResponseDto<bool>
            {
                Success = false,
                Message = "El RUC ya está registrado en otra empresa",
                Data = false
            };

            _mockService
                .Setup(x => x.ActualizarEmpresaAsync(It.IsAny<UpdateEmpresaRequest>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.ActualizarEmpresa(empresaId, request);

            // Assert
            Assert.NotNull(result.Result);
            var badRequestResult = result.Result as BadRequestObjectResult;
            Assert.NotNull(badRequestResult);
            Assert.Equal(400, badRequestResult!.StatusCode);

            var response = badRequestResult.Value as ApiResponseDto<bool>;
            Assert.NotNull(response);
            Assert.False(response!.Success);
            Assert.Contains("RUC", response.Message);
            Assert.NotEmpty(response.Message);
        }
        #endregion

        #region AsignarContador Tests

        [Fact]
        public async Task AsignarContador_ConDatosValidos_RetornaOk()
        {
            // Arrange
            var request = new AsignarContadorRequest
            {
                EmpresaId = Guid.NewGuid(),
                ContadorId = 1
            };

            var expectedResponse = new ApiResponseDto<bool>
            {
                Success = true,
                Message = "Contador asignado exitosamente",
                Data = true
            };

            _mockService
                .Setup(x => x.AsignarContadorAsync(It.IsAny<AsignarContadorRequest>(), It.IsAny<int>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var actionResult = await _controller.AsignarContador(request);

            // Assert
            Assert.NotNull(actionResult.Result);
            var okResult = actionResult.Result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult!.StatusCode);
        }

        [Fact]
        public async Task AsignarContador_ModelStateInvalido_RetornaBadRequest()
        {
            // Arrange
            var request = new AsignarContadorRequest();
            _controller.ModelState.AddModelError("EmpresaId", "Required");

            // Act
            var result = await _controller.AsignarContador(request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task AsignarContador_CuandoFalla_RetornaBadRequest()
        {
            // Arrange
            // Arrange
            var request = new AsignarContadorRequest
            {
                EmpresaId = Guid.NewGuid(),
                ContadorId = 999
            };

            // ✅ Cambiar a ApiResponseDto<bool>
            var expectedResponse = new ApiResponseDto<bool>
            {
                Success = false,
                Message = "El usuario no es un contador válido",
                Data = false
            };

            _mockService
                .Setup(x => x.AsignarContadorAsync(It.IsAny<AsignarContadorRequest>(), It.IsAny<int>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.AsignarContador(request);

            // Assert
            Assert.NotNull(result.Result);
            var badRequestResult = result.Result as BadRequestObjectResult;
            Assert.NotNull(badRequestResult);
            Assert.Equal(400, badRequestResult!.StatusCode);

            var response = badRequestResult.Value as ApiResponseDto<bool>;
            Assert.NotNull(response);
            Assert.False(response!.Success);
            Assert.Contains("contador", response.Message.ToLower());
            Assert.NotEmpty(response.Message);
        }

        #endregion

        #region ListarContadores Tests


        [Fact]
        public async Task ListarContadores_CuandoOcurreError_Retorna500()
        {
            // Arrange
            _mockService
                .Setup(x => x.ListarContadoresAsync())
                .ThrowsAsync(new Exception("Error"));

            // Act
            var result = await _controller.ListarContadores();

            // Assert
            var statusResult = result.Result as ObjectResult;
            Assert.Equal(500, statusResult!.StatusCode);
        }

        #endregion

        #region ObtenerMisEmpresas Tests

        [Fact]
        public async Task ObtenerMisEmpresas_UsuarioAutenticado_RetornaOk()
        {
            // Arrange
            var expectedResponse = new PagedResultDto<EEmpresa>
            {
                TotalRecords = 1,
                Data = new List<EEmpresa>
                {
                    new() { nombreComercial = "Mi Empresa esta actualizar " }
                }
            };

            _mockService
                .Setup(x => x.ListarEmpresasAsync(It.IsAny<EmpresaQueryRequest>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.ObtenerMisEmpresas();

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.NotNull(okResult);
        }

        [Fact]
        public async Task ObtenerMisEmpresas_UsuarioNoAutenticado_RetornaUnauthorized()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.ObtenerMisEmpresas();

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result.Result);
        }

        #endregion

        #region CambiarEstadoEmpresa Tests

        [Fact]
        public async Task CambiarEstadoEmpresa_ConDatosValidos_RetornaOk()
        {
            // Arrange
            var empresaId = Guid.NewGuid();
            var request = new CambiarEstadoRequest
            {
                EmpresaId = empresaId,
                Activo = false
            };

            var expectedResponse = new CambiarEstadoResponse
            {
                Success = true
            };

            _mockService
                .Setup(x => x.CambiarEstadoEmpresaAsync(request))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.CambiarEstadoEmpresa(empresaId, request);

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task CambiarEstadoEmpresa_IdNoCoincide_RetornaBadRequest()
        {
            // Arrange
            var empresaId = Guid.NewGuid();
            var request = new CambiarEstadoRequest
            {
                EmpresaId = Guid.NewGuid(),
                Activo = false
            };

            // Act
            var result = await _controller.CambiarEstadoEmpresa(empresaId, request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task CambiarEstadoEmpresa_EmpresaNoExiste_Retorna404()
        {
            // Arrange
            var empresaId = Guid.NewGuid();
            var request = new CambiarEstadoRequest
            {
                EmpresaId = empresaId,
                Activo = false
            };

            var expectedResponse = new CambiarEstadoResponse
            {
                Success = false,
                Message = "Empresa no encontrada"
            };

            _mockService
                .Setup(x => x.CambiarEstadoEmpresaAsync(request))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.CambiarEstadoEmpresa(empresaId, request);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        #endregion
    }
}
