using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Nobisoft.Core.Domain.Entities;
using Nobisoft.Core.Domain.Interfaces.Audit;
using Nobisoft.Core.Interfaces;

namespace Nobisoft.Core.Database.Repository
{
    /// <summary>
    /// This class is for Restful api as its intention.
    /// It can use connection pool to reduce the new instances
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TDbContext"></typeparam>
    public class RestfulRepository<TEntity, TDbContext> : IRepository<TEntity>
        where TEntity : class, IEntity
        where TDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        private readonly TDbContext _dbContext;
        private readonly ISnowflakeIdService _snowflakeIdService;
        public DbSet<TEntity> Entity { get; set; }
        public TDbContext DbContext => _dbContext;
        public RestfulRepository(TDbContext context, ISnowflakeIdService snowflakeIdService)
        {
            _snowflakeIdService = snowflakeIdService;
            _dbContext = context ?? throw new ArgumentNullException(nameof(context));
            Entity = _dbContext.Set<TEntity>();
        }

        public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            if (IsInheritsFrom(entity, typeof(ICreationAuditEntity)))
            {
                var createdTimeProp = entity.GetType().GetProperty("CreatedTime");
                var createdTime = Truncate(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
                createdTimeProp?.SetValue(entity, Convert.ChangeType(createdTime, createdTimeProp.PropertyType), null);
            }

            // Generate id
            // Get list of properties of entity
            var properties = entity.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            // Loop thru all properties to find the property with [DatabaseGenerated(DatabaseGeneratedOption.Identity)] with DatabaseGeneratedOption.Identity = 1
            foreach (var property in properties)
            {
                var customAttributes = property.CustomAttributes.ToList();
                if (customAttributes.Any(c => c.AttributeType == typeof(DatabaseGeneratedAttribute) && c.ConstructorArguments.Any(a => a.ArgumentType == typeof(DatabaseGeneratedOption) && a.Value != null && a.Value.ToString() == "1")))
                {
                    var propertyValue = property.GetValue(entity, null);
                    switch (propertyValue)
                    {
                        // If property is long (Int64) and its value is 0, generate a new one
                        case Int64 currentValue:
                            if (currentValue == 0)
                            {
                                currentValue = await _snowflakeIdService.GenerateId(cancellationToken);
                                property.SetValue(entity, Convert.ChangeType(currentValue, property.PropertyType), null);
                            }

                            break;
                    }
                }
            }
            var entityEntry = await Entity.AddAsync(entity, cancellationToken);
            return entityEntry.Entity;
        }

        public async Task<IEnumerable<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            var result = new Collection<TEntity>();
            foreach (var entity in entities)
                result.Add(await AddAsync(entity, cancellationToken));
            return result;
        }

        public async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            if (IsInheritsFrom(entity, typeof(IModificationAuditEntity)))
            {
                var modifiedTimeProp = entity.GetType().GetProperty("ModifiedTime");
                var modifiedTime = Truncate(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
                modifiedTimeProp?.SetValue(entity, Convert.ChangeType(modifiedTime, modifiedTimeProp.PropertyType), null);
            }

            PreSaveChange(entity);
            Entity.Attach(entity);
            var entityEntry = Entity.Update(entity);
            return await Task.FromResult(entityEntry.Entity);
        }

        public async Task<IEnumerable<TEntity>> UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            var result = new Collection<TEntity>();
            foreach (var entity in entities)
                result.Add(await UpdateAsync(entity, cancellationToken));
            return result;
        }

        public async Task<TEntity> RemoveAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            if (IsInheritsFrom(entity, typeof(IDeletionAuditEntity)))
            {
                var deletedTimeProp = entity.GetType().GetProperty("DeletedTime");
                var deletedTime = Truncate(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
                deletedTimeProp?.SetValue(entity, Convert.ChangeType(deletedTime, deletedTimeProp.PropertyType), null);
                var isDeleted = entity.GetType().GetProperty("Deleted");
                isDeleted?.SetValue(entity, Convert.ChangeType(true, isDeleted.PropertyType), null);
                return await UpdateAsync(entity, cancellationToken);
            }
            else
            {
                Entity.Attach(entity);
                var entityEntry = Entity.Remove(entity);
                return await Task.FromResult(entityEntry.Entity);
            }
        }

        public async Task<IEnumerable<TEntity>> RemoveRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            var result = new Collection<TEntity>();
            foreach (var entity in entities)
                result.Add(await RemoveAsync(entity, cancellationToken));
            return result;
        }

        public IQueryable<TEntity> Queryable()
        {
            return _dbContext.Set<TEntity>();
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            _dbContext.ChangeTracker.DetectChanges();
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task ReloadAsync(TEntity entity, CancellationToken cancellationToken = default(CancellationToken))
        {
            await _dbContext.Entry(entity).ReloadAsync(cancellationToken);
        }

        public void Dispose()
        {
            _dbContext.Dispose();
            GC.SuppressFinalize(this);
        }

        #region Private methods

        private void PreSaveChange(TEntity entity)
        {
            var properties = entity.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var property in properties)
            {
                var value = property.GetValue(entity, null);
                switch (value)
                {
                    case DateTime dateTime:
                        dateTime = Truncate(dateTime, TimeSpan.FromSeconds(1));
                        property.SetValue(entity, Convert.ChangeType(dateTime, property.PropertyType), null);
                        break;
                    case DateTimeOffset dateTimeOffset:
                        dateTimeOffset = Truncate(dateTimeOffset, TimeSpan.FromSeconds(1));
                        // property.SetValue(entity, Convert.ChangeType(dateTimeOffset, property.PropertyType), null);
                        var t = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                        var safeValue = Convert.ChangeType(dateTimeOffset, t);
                        property.SetValue(entity, safeValue, null);
                        break;
                }
            }
        }

        private static DateTime Truncate(DateTime dateTime, TimeSpan timeSpan)
        {
            if (timeSpan == TimeSpan.Zero)
                return dateTime; // Or could throw an ArgumentException
            if (dateTime == DateTime.MinValue || dateTime == DateTime.MaxValue)
                return dateTime; // do not modify "guard" values
            return dateTime.AddTicks(-(dateTime.Ticks % timeSpan.Ticks));
        }

        private static DateTimeOffset Truncate(DateTimeOffset dateTime, TimeSpan timeSpan)
        {
            if (timeSpan == TimeSpan.Zero)
                return dateTime; // Or could throw an ArgumentException
            if (dateTime == DateTimeOffset.MinValue || dateTime == DateTimeOffset.MaxValue)
                return dateTime; // do not modify "guard" values
            return dateTime.AddTicks(-(dateTime.Ticks % timeSpan.Ticks));
        }

        private bool IsInheritsFrom(object source, Type baseType)
        {
            var type = source.GetType();

            // only interface or object can have null base type
            if (baseType == null)
                return type.IsInterface || type == typeof(object);

            // check implemented interfaces
            if (baseType.IsInterface)
                return type.GetInterfaces().Contains(baseType);

            // check all base types
            var currentType = type;
            while (true)
            {
                if (currentType.BaseType == baseType)
                    return true;
                currentType = currentType.BaseType;
                return IsInheritsFrom(source, currentType);
            }
        }

        #endregion
    }
}