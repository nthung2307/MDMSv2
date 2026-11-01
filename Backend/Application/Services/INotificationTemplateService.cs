using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.DataTransferObjects.Templates;

namespace Application.Services;

public interface INotificationTemplateService
{
    string Render(string template, IDictionary<string, object?> context);
    Task<(string Subject, string BodyHtml)> RenderNotificationAsync(string code, IDictionary<string, object?> context, CancellationToken cancellationToken = default);
    Task<PreviewNotificationTemplateResponse> PreviewAsync(PreviewNotificationTemplateRequest request, CancellationToken cancellationToken = default);
}
