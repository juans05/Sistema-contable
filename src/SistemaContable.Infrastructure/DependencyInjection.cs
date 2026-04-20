using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using SistemaContable.Application.Services.Interfaces.IRepository;
using SistemaContable.Infrastructure.Data.Repositories.Implementations;
using SistemaContable.Infrastructurxe.Data.Repositories.Implementations;

namespace SistemaContable.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configurar NpgsqlDataSource para el Connection Pooling de PostgreSQL
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' no encontrada o vacía.");

        services.AddNpgsqlDataSource(connectionString);

        // Repositories
        services.AddScoped<IAuthRepository, UserRepository>();
        services.AddScoped<IVentaRepository, VentaRepository>();
        services.AddScoped<IProductoRepository, ProductoRepository>();
        services.AddScoped<ICategoriaRepository, CategoriaRepository>();
        services.AddScoped<IMarcaRepository, MarcaRepository>();
        services.AddScoped<IPlanContableRepository, PlanContableRepository>();
        services.AddScoped<ITipoComprobanteRepository, TipoComprobanteRepository>();
        services.AddScoped<IAnexoRepository, AnexoRepository>();
        services.AddScoped<IFacturaElectronicaRepository, FacturaElectronicaRepository>();
        services.AddScoped<IEmpresaRepository, EmpresaRepository>();
        services.AddScoped<ICompraRepository, CompraRepository>();
        services.AddScoped<IAccountingRepository, AccountingRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
