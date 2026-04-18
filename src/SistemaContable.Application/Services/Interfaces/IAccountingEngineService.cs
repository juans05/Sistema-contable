using System.Threading.Tasks;
using SistemaContable.Application.Services.Interfaces.IRepository;

namespace SistemaContable.Application.Services.Interfaces
{
    public interface IAccountingEngineService
    {
        /// <summary>
        /// Genera y contabiliza el asiento automático para una Venta ya registrada.
        /// </summary>
        /// <param name="registroVentaId">ID de la venta en la tabla registro_venta</param>
        /// <returns>ID del asiento generado o 0 si falló</returns>
        Task<int> GenerarAsientoVentaAsync(int registroVentaId, IUnitOfWork tx = null);

        /// <summary>
        /// Genera y contabiliza el asiento automático para una Compra ya registrada.
        /// </summary>
        /// <param name="registroCompraId">ID de la compra en la tabla registro_compra</param>
        /// <returns>ID del asiento generado o 0 si falló</returns>
        Task<int> GenerarAsientoCompraAsync(int registroCompraId, IUnitOfWork tx = null);

        /// <summary>
        /// Importa un plan de cuentas desde un stream de Excel.
        /// </summary>
        Task<bool> ImportarPlanCuentasExcelAsync(System.IO.Stream excelStream, int empresaId);
    }
}
