using Microsoft.Extensions.Logging;
using Moq;
using SistemaContable.Application.DTOs.Common;
using SistemaContable.Application.DTOs.Requests.Empresa;
using SistemaContable.Application.DTOs.Responses.Empresa;
using SistemaContable.Application.Services.Implementations;
using SistemaContable.Application.Services.Interfaces.IRepository;
using SistemaContable.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.UnitTests
{
    public class EmpresaServiceTests
    {
        private readonly Mock<IEmpresaRepository> _mockRepository;
        private readonly Mock<ILogger<EmpresaService>> _mockLogger;
        private readonly EmpresaService _service;

        public EmpresaServiceTests()
        {
            _mockRepository = new Mock<IEmpresaRepository>();
            _mockLogger = new Mock<ILogger<EmpresaService>>();
            _service = new EmpresaService(_mockRepository.Object, _mockLogger.Object);
        }
        [Fact]
        public async Task ListarEmpresasAsync_ConParametrosValidos_RetornaListaPaginada()
        {
            // Arrange
            var request = new EmpresaQueryRequest
            {
                PageNumber = 1,
                PageSize = 10,
                Activo = true
            };


            var expectedResponse = new PagedResultDto<EEmpresa>
            {
                TotalRecords = 2,
                PageNumber = 1,
                PageSize = 10,
                Data = new List<EEmpresa>  // ✅ Usar List<Empresa>
                    {
                        new EEmpresa
                        {
                            id = Guid.NewGuid(),
                            nombreComercial = "Empresa 1",
                            ruc = "12345678901",
                            razonSocial = "Empresa 1 S.A.C.",
                            activo = true,
                            createdAt = DateTime.UtcNow
                        },
                        new EEmpresa
                        {
                            id = Guid.NewGuid(),
                            nombreComercial = "Empresa 2",
                            ruc = "98765432109",
                            razonSocial = "Empresa 2 S.A.C.",
                            activo = true,
                            createdAt = DateTime.UtcNow
                        }
                    }
            };           

            _mockRepository
                .Setup(x => x.ListarAsync(It.IsAny<EmpresaQueryRequest>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _service.ListarEmpresasAsync(request);

            // Assert
            
            _mockRepository.Verify(x => x.ListarAsync(request), Times.Once);
        }
    }
}
