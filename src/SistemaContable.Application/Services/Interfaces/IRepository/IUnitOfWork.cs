using System;
using System.Threading.Tasks;

namespace SistemaContable.Application.Services.Interfaces.IRepository
{
    public interface IUnitOfWork : IDisposable, IAsyncDisposable
    {
        IAccountingRepository AccountingRepo { get; }
        ICompraRepository CompraRepo { get; }
        IFacturaElectronicaRepository FacturaRepo { get; }

        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }
}
