using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql;
using SistemaContable.Application.Services.Interfaces.IRepository;

namespace SistemaContable.Infrastructure.Data.Repositories.Implementations
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly NpgsqlDataSource _dataSource;
        private readonly ILoggerFactory _loggerFactory;
        private NpgsqlConnection _connection;
        private NpgsqlTransaction _transaction;
        private IAccountingRepository _accountingRepo;
        private ICompraRepository _compraRepo;
        private IFacturaElectronicaRepository _facturaRepo;

        public UnitOfWork(NpgsqlDataSource dataSource, ILoggerFactory loggerFactory)
        {
            _dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        public IAccountingRepository AccountingRepo
        {
            get
            {
                if (_accountingRepo == null)
                {
                    if (_connection == null || _transaction == null)
                        throw new InvalidOperationException("Debe llamar a BeginTransactionAsync() antes de usar los repositorios.");

                    _accountingRepo = new AccountingRepository(_connection, _transaction, _loggerFactory.CreateLogger<AccountingRepository>());
                }
                return _accountingRepo;
            }
        }

        public ICompraRepository CompraRepo
        {
            get
            {
                if (_compraRepo == null)
                {
                    if (_connection == null || _transaction == null)
                        throw new InvalidOperationException("Debe llamar a BeginTransactionAsync() antes de usar los repositorios.");

                    _compraRepo = new CompraRepository(_connection, _transaction, _loggerFactory.CreateLogger<CompraRepository>());
                }
                return _compraRepo;
            }
        }

        public IFacturaElectronicaRepository FacturaRepo
        {
            get
            {
                if (_facturaRepo == null)
                {
                    if (_connection == null || _transaction == null)
                        throw new InvalidOperationException("Debe llamar a BeginTransactionAsync() antes de usar los repositorios.");

                    _facturaRepo = new FacturaElectronicaRepository(_connection, _transaction, _loggerFactory.CreateLogger<FacturaElectronicaRepository>());
                }
                return _facturaRepo;
            }
        }

        public async Task BeginTransactionAsync()
        {
            if (_connection != null) return;

            _connection = await _dataSource.OpenConnectionAsync();
            _transaction = await _connection.BeginTransactionAsync();
        }

        public async Task CommitAsync()
        {
            try
            {
                if (_transaction != null)
                {
                    await _transaction.CommitAsync();
                }
            }
            finally
            {
                await CleanupAsync();
            }
        }

        public async Task RollbackAsync()
        {
            try
            {
                if (_transaction != null)
                {
                    await _transaction.RollbackAsync();
                }
            }
            finally
            {
                await CleanupAsync();
            }
        }

        private async Task CleanupAsync()
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
            if (_connection != null)
            {
                await _connection.DisposeAsync();
                _connection = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _connection?.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            await CleanupAsync();
        }
    }
}
