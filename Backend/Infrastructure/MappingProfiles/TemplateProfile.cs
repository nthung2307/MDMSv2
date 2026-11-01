using Application.DataTransferObjects.Templates;
using AutoMapper;
using Domain.Entities;

namespace Infrastructure.MappingProfiles;

public class TemplateProfile : Profile
{
    public TemplateProfile()
    {
        CreateMap<NotificationTemplateEntity, NotificationTemplateDto>();
        CreateMap<CreateNotificationTemplateRequest, NotificationTemplateEntity>();
        CreateMap<UpdateNotificationTemplateRequest, NotificationTemplateEntity>();

        CreateMap<StatementPdfTemplateEntity, StatementPdfTemplateDto>();
        CreateMap<CreateStatementPdfTemplateRequest, StatementPdfTemplateEntity>();
        CreateMap<UpdateStatementPdfTemplateRequest, StatementPdfTemplateEntity>();
    }
}
