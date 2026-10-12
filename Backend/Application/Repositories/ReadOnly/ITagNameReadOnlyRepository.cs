using Application.Common.ValueObjects.CursorBased;
using Application.Common.ValueObjects.OffsetBased;
using Application.DataTransferObjects.Requests.Customers;
using Application.DataTransferObjects.Requests.TagName;
using Application.DataTransferObjects.Requests.Users;
using Application.DataTransferObjects.Responses.Customers;
using Application.DataTransferObjects.Responses.TagName;
using Application.DataTransferObjects.Responses.Users;
using Domain.Entities;
using Nobisoft.Core.Database.Repository;

namespace Application.Repositories.ReadOnly;

public interface ITagNameReadOnlyRepository : IRepository<TagNameEntity>
{
    Task<List<TagNameDto>> GetListTagNamesAsync(long measurementPointId, string? type, CancellationToken cancellationToken);
    Task<bool> IsExistedTagName(string description, long measurementPointId, CancellationToken cancellationToken);
}