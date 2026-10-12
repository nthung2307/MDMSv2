using Autofac;
using Nobisoft.Core.Mailer.Handlers;

namespace Nobisoft.Core.Mailer;

public class MailerModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        // Register mailing event handler as transient
        builder.RegisterType<MailingEventHandler>().InstancePerDependency();
        base.Load(builder);
    }
}