using Application.Common.ValueObjects.CursorBased;
using Application.DataTransferObjects.Requests.Customers;
using Application.DataTransferObjects.Responses.Customers;
using Application.Extensions;
using Application.Repositories.ReadOnly;
using AutoMapper;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Nobisoft.Core.Authorization;
using Nobisoft.Core.Authorization.Interfaces;
using Nobisoft.Core.Database.DbContext;
using Nobisoft.Core.Database.Repository;
using Nobisoft.Core.Interfaces;

namespace Infrastructure.Repositories.ReadOnly;

public class CustomerReadOnlyRepository : RestfulRepository<CustomerEntity, AppReadOnlyDbContext>, ICustomerReadOnlyRepository
{
    private readonly DbSet<CustomerEntity> _customerEntities;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserReadOnlyRepository _userReadOnlyRepository;

    public CustomerReadOnlyRepository(AppReadOnlyDbContext applicationDbContext, ISnowflakeIdService snowflakeIdService, IMapper mapper, ICurrentUserService currentUserService, IUserReadOnlyRepository userReadOnlyRepository) : base(applicationDbContext, snowflakeIdService)
    {
        _mapper = mapper;
        _currentUserService = currentUserService;
        _userReadOnlyRepository = userReadOnlyRepository;
        _customerEntities = applicationDbContext.Set<CustomerEntity>();
    }

    public async Task<CursorPaginationResponse<CustomerDto>> GetListCustomersAsync(GetListCustomersRequest request, CancellationToken cancellationToken)
    {
        var customer = _customerEntities.AsNoTracking();
        var result = await customer
            .Include(x => x.UserCustomer)
            .ThenInclude(x => x.User)
            .Where(x => !x.Deleted)
            .PaginateWithAndAsync<CustomerEntity, CustomerDto>(request, _mapper, cancellationToken);
        return new CursorPaginationResponse<CustomerDto>()
        {
            Data = result.Data,
            HasNext = result.HasNext,
            HasPrevious = result.HasPrevious,
        };
    }

    public async Task<CursorPaginationResponse<CustomerDto>> GetListCustomersForReportAsync(GetListCustomersForReportRequest request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        var customer = _customerEntities
            .AsNoTracking();
        var user = await _userReadOnlyRepository.GetUserDetailAsync(userId, cancellationToken);
        if (user.RoleName == DefaultRoles.Customer)
        {
            customer = customer.Where(c => c.UserCustomer.Any(uc => uc.UserId == userId));
        }
        var result = await customer
            .Include(x => x.UserCustomer)
            .ThenInclude(x => x.User)
            .Where(x => !x.Deleted)
            .PaginateWithAndAsync<CustomerEntity, CustomerDto>(request, _mapper, cancellationToken);
        return new CursorPaginationResponse<CustomerDto>()
        {
            Data = result.Data,
            HasNext = result.HasNext,
            HasPrevious = result.HasPrevious,
        };
    }

    public async Task<CustomerDto> GetCustomerDetailAsync(long id, CancellationToken cancellationToken)
    {
        var user = await _customerEntities
            .AsNoTracking()
            .Include(x => x.UserCustomer)
            .ThenInclude(x => x.User)
            .Include(x => x.MeasurePoints)
            .ThenInclude(x => x.Gcs)
            .Where(x => x.Id == id && !x.Deleted)
            .FirstOrDefaultAsync(cancellationToken);
        return _mapper.Map<CustomerDto>(user);
    }

    public async Task<bool> IsExistedCustomerCode(string code, long id, CancellationToken cancellationToken)
    {
        if (id > 0)
            return await _customerEntities.AsNoTracking().Where(x => x.CustomerCode == code && x.Id != id).AnyAsync(cancellationToken);
        return await _customerEntities.AsNoTracking().Where(x => x.CustomerCode == code).AnyAsync(cancellationToken);
    }
}