using System.Collections.Generic;
using System.Threading.Tasks;
using SistemaContable.Domain.Entities;

namespace SistemaContable.Application.Services.Interfaces.IRepository
{
    public interface IAccountingRepository
    {
        /// <summary>
        /// Obtiene las reglas de contabilización activas para un evento específico y empresa.
        /// </summary>
        Task<List<EReglaContable>> ObtenerReglasPorEventoAsync(string codigoEvento, int empresaId);
        
        /// <summary>
        /// Guarda un asiento contable completo (Cabecera + Detalles) de forma transaccional.
        /// </summary>
        Task<int> GuardarAsientoCompletoAsync(EAsientoContable asiento);

        /// <summary>
        /// Busca una cuenta del plan contable por su código.
        /// </summary>
        Task<EPlanContable> ObtenerCuentaPorCodigoAsync(string codigo, int empresaId);
        
        /// <summary>
        /// Obtiene el valor de una configuración contable, o null si no existe.
        /// </summary>
        Task<string> ObtenerConfiguracionAsync(string clave, int empresaId);

        // ===== NUEVOS MÉTODOS DE CONFIGURACIÓN =====
        Task<List<EEventoContableTipo>> ListarEventosDisponiblesAsync();
        Task<bool> GuardarReglaAsync(EReglaContable regla);
        Task<bool> EliminarReglaAsync(int id, int empresaId);
        
        /// <summary>
        /// Importa masivamente un plan de cuentas.
        /// </summary>
        Task<bool> ImportarPlanCuentasAsync(List<EPlanContable> cuentas, int empresaId);

        // ===== CRUD MANUAL PLAN CUENTAS =====
        Task<List<EPlanContable>> ListarPlanCuentasAsync(int empresaId, string busqueda = null);
        Task<bool> GuardarCuentaAsync(EPlanContable cuenta);
        Task<bool> EliminarCuentaAsync(int id, int empresaId);
    }
}
