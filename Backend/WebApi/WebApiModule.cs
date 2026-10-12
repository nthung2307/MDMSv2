using Autofac;
using Microsoft.EntityFrameworkCore;
using Nobisoft.Core.Database.DbContext;

namespace WebApi;

public class WebApiModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        base.Load(builder);
        builder.Register(context =>
        {
            var factory = context.Resolve<IDbContextFactory<AppReadWriteDbContext>>();
            return factory.CreateDbContext();
        });
        builder.Register(context =>
        {
            var factory = context.Resolve<IDbContextFactory<AppReadOnlyDbContext>>();
            return factory.CreateDbContext();
        });
        base.Load(builder);
    }
}