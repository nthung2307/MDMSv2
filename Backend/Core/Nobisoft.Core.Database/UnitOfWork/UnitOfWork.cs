using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Nobisoft.Core.Database.UnitOfWork
{
    public class UnitOfWork<TDbContext> : IUnitOfWork<TDbContext> where TDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        private readonly TDbContext _dbContext;

        public UnitOfWork(TDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public void Dispose()
        {
            _dbContext.Dispose();
            GC.SuppressFinalize(this);
        }

        public TDbContext DbContext => _dbContext;

        public virtual IDbContextTransaction BeginTransaction()
        {
            return _dbContext.Database.BeginTransaction();
        }

        public virtual Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            return _dbContext.Database.BeginTransactionAsync(cancellationToken);
        }

        public virtual void CommitTransaction()
        {
            _dbContext.Database.CommitTransaction();
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken)
        {
            await _dbContext.Database.CommitTransactionAsync(cancellationToken);
        }

        public virtual void RollbackTransaction()
        {
            _dbContext.Database.RollbackTransaction();
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken)
        {
            await _dbContext.Database.RollbackTransactionAsync(cancellationToken);
        }

        public virtual Task<bool> CanConnectAsync(CancellationToken cancellationToken = default)
        {
            return _dbContext.Database.CanConnectAsync(cancellationToken);
        }

        public virtual int ExecuteSqlRaw(string sql, params object[] parameters)
        {
            return _dbContext.Database.ExecuteSqlRaw(sql, parameters);
        }

        public virtual async Task<int> ExecuteSqlRawAsync(string sql, params object[] parameters)
        {
            return await _dbContext.Database.ExecuteSqlRawAsync(sql, parameters);
        }

        public virtual int ExecuteSqlInterpolated(FormattableString sql)
        {
            return _dbContext.Database.ExecuteSqlInterpolated(sql);
        }

        public virtual async Task<int> ExecuteSqlInterpolatedAsync(FormattableString sql, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Database.ExecuteSqlInterpolatedAsync(sql, cancellationToken);
        }

        public virtual IDbContextTransaction UseTransaction(DbTransaction transaction)
        {
            return _dbContext.Database.UseTransaction(transaction) ?? throw new InvalidOperationException();
        }

        public virtual async Task<IDbContextTransaction> UseTransactionAsync(DbTransaction transaction, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Database.UseTransactionAsync(transaction, cancellationToken) ?? throw new InvalidOperationException();
        }

        public virtual IDbContextTransaction UseTransaction(IDbContextTransaction transaction)
        {
            return _dbContext.Database.UseTransaction(transaction.GetDbTransaction()) ?? throw new InvalidOperationException();
        }

        public virtual async Task<IDbContextTransaction> UseTransactionAsync(IDbContextTransaction transaction, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Database.UseTransactionAsync(transaction.GetDbTransaction(), cancellationToken) ?? throw new InvalidOperationException();
        }

        public virtual async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            return await _dbContext.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}