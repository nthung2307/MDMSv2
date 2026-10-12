using Application.Common.ValueObjects.CursorBased;
using Application.DataTransferObjects.Requests.Gcs;
using Application.DataTransferObjects.Requests.IoT;
using Application.DataTransferObjects.Responses.IoT;
using Application.Extensions;
using Application.Repositories.ReadWrite;
using AutoMapper;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Nobisoft.Core.Database.DbContext;
using Nobisoft.Core.Database.Repository;
using Nobisoft.Core.Interfaces;

namespace Infrastructure.Repositories.ReadWrite
{
    public class GcsTemporaryReadWriteRepository : RestfulRepository<GcsTemporaryEntity, AppReadWriteDbContext>, IGcsTemporaryReadWriteRepository
    {
        private readonly DbSet<GcsTemporaryEntity> _gcsTemporaryEntities;
        private readonly IMapper _mapper;

        public GcsTemporaryReadWriteRepository(AppReadWriteDbContext applicationDbContext, ISnowflakeIdService snowflakeIdService, IMapper mapper) : base(applicationDbContext, snowflakeIdService)
        {
            _mapper = mapper;
            _gcsTemporaryEntities = applicationDbContext.Set<GcsTemporaryEntity>();
        }

        public async Task<CursorPaginationResponse<GetDataLatchResponse>> GetListGcsTemporaryWithPaginationAsync(GetDataLatchRequest request, CancellationToken cancellationToken)
        {
            var gcsTemporary = _gcsTemporaryEntities.AsNoTracking();
            switch (request.Quanlity)
            {
                case true:
                    gcsTemporary = gcsTemporary.Where(x => x.Quanlity == request.Quanlity);
                    break;
                case false:
                    gcsTemporary = gcsTemporary.Where(x => x.Quanlity == request.Quanlity || x.Quanlity == null);
                    break;
            }

            var result = await gcsTemporary
                .Where(x => x.GcsId == request.GcsId && x.Month == request.Month && x.Year == request.Year && x.PeriodNumber == request.PeriodNumber)
                .Include(x => x.MeasurementPoint)
                .ThenInclude(x => x.Customer)
                .PaginateWithAndAsync<GcsTemporaryEntity, GetDataLatchResponse>(request, _mapper, cancellationToken);

            return new CursorPaginationResponse<GetDataLatchResponse>()
            {
                Data = result.Data,
                HasNext = result.HasNext,
                HasPrevious = result.HasPrevious,
            };
        }

        public async Task<List<GcsTemporaryEntity>> GetListGcsTemporaryAsync(GetDataLatchRequest request, CancellationToken cancellationToken)
        {
            return await _gcsTemporaryEntities
                .Where(x => x.GcsId == request.GcsId && x.PeriodNumber == request.PeriodNumber && x.Month == request.Month && x.Year == request.Year)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<GcsTemporaryEntity>> GetListGcsTemporaryToUpdateAsync(List<UpdateGcsTemporaryRequest> request, long gcsId, int month, int year, int periodNumber, CancellationToken cancellationToken)
        {
            var requestIds = request.Select(r => r.Id).ToList();
            return await _gcsTemporaryEntities
                .Where(x => requestIds.Contains(x.Id) && x.GcsId == gcsId && x.Month == month && x.Year == year && x.PeriodNumber == periodNumber)
                .Include(x => x.MeasurementPoint)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<GcsTemporaryEntity>> GetListGcsTemporaryWithGcsAsync(long gcsId, int month, int year, int periodNumber, CancellationToken cancellationToken)
        {
            return await _gcsTemporaryEntities
                .Where(x => x.GcsId == gcsId && x.PeriodNumber == periodNumber && x.Month == month && x.Year == year)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> CheckExistGcsTemporaryAsync(long gcsId, int month, int year, int periodNumber, CancellationToken cancellationToken)
        {
            return await _gcsTemporaryEntities
                .AsNoTracking()
                .Where(x => x.GcsId == gcsId && x.Month == month && x.Year == year && x.PeriodNumber == periodNumber)
                .AnyAsync(x => (x.Quanlity == null && x.NewCurrentReading == null) || (x.Quanlity == false && x.NewCurrentReading == null), cancellationToken);
        }
    }
}
