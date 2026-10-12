using Microsoft.Extensions.Logging;
using Nobisoft.Core.HttpContextAccessor.Interfaces;
using Nobisoft.Core.Mailer.Events;

namespace Nobisoft.Core.Mailer.Handlers;

public class MailingEventHandler
{
    private readonly ILogger<MailingEventHandler> _logger;
    private readonly IHttpContextAccessorService _httpContextAccessorService;


    public MailingEventHandler(
        ILogger<MailingEventHandler> logger, IHttpContextAccessorService httpContextAccessorService)
    {
        _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        _httpContextAccessorService = httpContextAccessorService;
    }

    public async Task Handle(MailingEvent @event)
    {
        Console.WriteLine($"----- Handling integration event: {@event} --> via mailing module");
        //throw new ItemAlreadyDeletedException("", "", "", @event.Tracker);
        // using (LogContext.PushProperty("IntegrationEventContext", $"{@event.Id}-XXX"))
        // {
        //     _logger.LogInformation("----- Handling integration event: {IntegrationEventId} at {AppName} - ({@IntegrationEvent})", @event.Id, "XXX", @event);
        //
        //
        //     await Task.CompletedTask;
        // }
    }
}