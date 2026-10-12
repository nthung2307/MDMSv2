using System.Data.Common;
using Microsoft.EntityFrameworkCore.Storage;
using Nobisoft.Core.Lifetimes;

namespace Nobisoft.Core.Database.UnitOfWork
{
    public interface IUnitOfWork<TDbContext> : IDisposable, IScopedDependency
        where TDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public TDbContext DbContext { get; }

        IDbContextTransaction BeginTransaction();

        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

        void CommitTransaction();
        Task CommitTransactionAsync(CancellationToken cancellationToken);

        void RollbackTransaction();
        Task RollbackTransactionAsync(CancellationToken cancellationToken);


        Task<bool> CanConnectAsync(CancellationToken cancellationToken = default);

        int ExecuteSqlRaw(string sql, params object[] parameters);

        Task<int> ExecuteSqlRawAsync(string sql, params object[] parameters);

        int ExecuteSqlInterpolated(FormattableString sql);

        Task<int> ExecuteSqlInterpolatedAsync(FormattableString sql, CancellationToken cancellationToken = default);

        IDbContextTransaction UseTransaction(DbTransaction transaction);

        Task<IDbContextTransaction> UseTransactionAsync(DbTransaction transaction, CancellationToken cancellationToken = default);

        IDbContextTransaction UseTransaction(IDbContextTransaction transaction);

        Task<IDbContextTransaction> UseTransactionAsync(IDbContextTransaction transaction, CancellationToken cancellationToken = default);

        Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default);

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}