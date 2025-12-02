namespace SistemaContable.API.Middlewares
{
    public class RucEmpresaMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RucEmpresaMiddleware> _logger;

        public RucEmpresaMiddleware(RequestDelegate next, ILogger<RucEmpresaMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Capturar RUC desde Header, Query, o Token
            var rucEmpresa = context.Request.Headers["X-RUC-Empresa"].FirstOrDefault()
                          ?? context.Request.Query["rucEmpresa"].FirstOrDefault()
                          ?? ExtraerRucDesdeToken(context);

            if (!string.IsNullOrEmpty(rucEmpresa))
            {
                // Guardar en HttpContext.Items para usar en toda la petición
                context.Items["RucEmpresa"] = rucEmpresa;
                _logger.LogInformation($"RUC Empresa capturado: {rucEmpresa}");
            }
            else
            {
                _logger.LogWarning("No se proporcionó RUC de empresa en la petición");
            }

            await _next(context);
        }

        private string ExtraerRucDesdeToken(HttpContext context)
        {
            // Si usas JWT, extrae el RUC del token
            var user = context.User;
            return user?.Claims.FirstOrDefault(c => c.Type == "ruc_empresa")?.Value;
        }
    }
}
