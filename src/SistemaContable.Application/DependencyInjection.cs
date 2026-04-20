using Mapster;
using Microsoft.Extensions.DependencyInjection;
using SistemaContable.Application.Services.Implementations;
using SistemaContable.Application.Services.Interfaces;
using System.Reflection;

namespace SistemaContable.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Configuración y Registro de Mapster
        var config = TypeAdapterConfig.GlobalSettings;
        config.Scan(Assembly.GetExecutingAssembly());
        services.AddSingleton(config);

        // Registrar el IHttpContextAccessor necesario para TokenDataService
        services.AddHttpContextAccessor();

        // Registrar servicios de la aplicación
        services.AddScoped<IAccountingEngineService, AccountingEngineService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICompraService, CompraService>();
        services.AddScoped<IEmpresaService, EmpresaService>();
        services.AddScoped<IFinancialDashboardService, FinancialDashboardService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPasswordService, PasswordService>();        
        services.AddScoped<IProductoService, ProductoService>();
        services.AddScoped<ICategoriaService, CategoriaService>();
        services.AddScoped<IMarcaService, MarcaService>();
        services.AddScoped<IPlanContableService, PlanContableService>();
        services.AddScoped<ITipoComprobanteService, TipoComprobanteService>();
        services.AddScoped<IAnexoService, AnexoService>();
        services.AddScoped<IRucEmpresaService, RucEmpresaService>();
        services.AddScoped<ISireService, SireService>();
        services.AddScoped<ITokenDataService, TokenDataService>();
        services.AddScoped<IVentaElectronicaService, VentaElectronicaService>();        

        return services;
    }
}
