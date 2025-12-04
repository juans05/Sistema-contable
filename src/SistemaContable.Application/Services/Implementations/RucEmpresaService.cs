using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SistemaContable.Application.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.Application.Services.Implementations
{
    public class RucEmpresaService : IRucEmpresaService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<RucEmpresaService> _logger;

        public RucEmpresaService(
            IHttpContextAccessor httpContextAccessor,
            ILogger<RucEmpresaService> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public string ObtenerRucActual()
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext == null)
            {
                _logger.LogWarning("HttpContext no disponible");
                return null;
            }

            // Intentar obtener de Items
            if (httpContext.Items.TryGetValue("RUC", out var ruc))
            {
                return ruc?.ToString();
            }

            // Fallback: intentar desde Claims
            var rucFromClaims = httpContext.User?.Claims
                .FirstOrDefault(c => c.Type == "RUC")?.Value;

            if (!string.IsNullOrEmpty(rucFromClaims))
            {
                return rucFromClaims;
            }

            _logger.LogWarning("No se pudo obtener RUC de empresa");
            return null;
        }

        public void EstablecerRuc(string ruc)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                httpContext.Items["RUC"] = ruc;
                _logger.LogInformation($"RUC establecido manualmente: {ruc}");
            }
        }
    }
}
