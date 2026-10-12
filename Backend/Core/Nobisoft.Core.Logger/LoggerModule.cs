using Autofac;

namespace Nobisoft.Core.Logger;

public class LoggerModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        // Register logger event handler as transient
        base.Load(builder);
    }
}