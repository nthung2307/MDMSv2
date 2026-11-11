using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.ValueObjects.OffsetBased;
using Application.DataTransferObjects.Requests.MeterReplacement;
using Application.DataTransferObjects.Responses.MeterReplacement;
using Application.Repositories.ReadOnly;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Nobisoft.Core.Database.DbContext;
using Nobisoft.Core.Database.Repository;
using Nobisoft.Core.Interfaces;

namespace Infrastructure.Repositories.ReadOnly;

public class MeterReplacementReadOnlyRepository : RestfulRepository<MeterReplacementEntity, AppReadOnlyDbContext>,
    IMeterReplacementReadOnlyRepository
{
    private readonly DbSet<MeterReplacementEntity> _entities;
    private readonly AppReadOnlyDbContext _dbContext;

    public MeterReplacementReadOnlyRepository(
        AppReadOnlyDbContext applicationDbContext,
        ISnowflakeIdService snowflakeIdService) : base(applicationDbContext, snowflakeIdService)
    {
        _dbContext = applicationDbContext;
        _entities = applicationDbContext.Set<MeterReplacementEntity>();
    }

    public async Task<OffsetPaginationResponse<MeterReplacementDto>> GetPagedListAsync(
        GetListMeterReplacementRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = _entities.AsNoTracking()
            .Include(r => r.MeasurementPoint)
                .ThenInclude(p => p.Customer)
            .AsQueryable();

        if (request.MeasurementPointId.HasValue && request.MeasurementPointId.Value > 0)
        {
            query = query.Where(r => r.MeasurementPointId == request.MeasurementPointId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.PointCode))
        {
            var pCode = request.PointCode.Trim();
            query = query.Where(r => r.MeasurementPoint != null && r.MeasurementPoint.Code.Contains(pCode));
        }

        if (!string.IsNullOrWhiteSpace(request.CustomerName))
        {
            var cName = request.CustomerName.Trim();
            query = query.Where(r => r.MeasurementPoint != null && r.MeasurementPoint.Customer != null && r.MeasurementPoint.Customer.FullName.Contains(cName));
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            query = query.Where(r => r.Status == request.Status);
        }

        if (request.FromDate.HasValue)
        {
            query = query.Where(r => r.ReplacementDate >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(r => r.ReplacementDate <= request.ToDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var kw = request.Keyword.Trim();
            query = query.Where(r =>
                r.OldMeterSerial.Contains(kw) ||
                r.NewMeterSerial.Contains(kw) ||
                (r.MeasurementPoint != null && r.MeasurementPoint.Code.Contains(kw)) ||
                (r.MeasurementPoint != null && r.MeasurementPoint.Description != null && r.MeasurementPoint.Description.Contains(kw)) ||
                (r.MeasurementPoint != null && r.MeasurementPoint.Customer != null && r.MeasurementPoint.Customer.FullName.Contains(kw)));
        }

        var total = await query.LongCountAsync(cancellationToken);

        var pageSize = request.PageSize > 0 ? request.PageSize : 30;
        var currentPage = request.CurrentPage > 0 ? request.CurrentPage : 1;
        var skip = (currentPage - 1) * pageSize;

        var items = await query
            .OrderByDescending(r => r.ReplacementDate)
            .Skip(skip)
            .Take(pageSize)
            .Select(r => new MeterReplacementDto
            {
                Id = r.Id,
                MeasurementPointId = r.MeasurementPointId,
                MeasurementPointCode = r.MeasurementPoint != null ? r.MeasurementPoint.Code : string.Empty,
                MeasurementPointDescription = r.MeasurementPoint != null ? (r.MeasurementPoint.Description ?? string.Empty) : string.Empty,
                CustomerCode = r.MeasurementPoint != null && r.MeasurementPoint.Customer != null ? r.MeasurementPoint.Customer.CustomerCode : string.Empty,
                CustomerName = r.MeasurementPoint != null && r.MeasurementPoint.Customer != null ? r.MeasurementPoint.Customer.FullName : string.Empty,
                ReplacementDate = r.ReplacementDate,
                Reason = r.Reason,
                OldMeterSerial = r.OldMeterSerial,
                OldMeterMultiplier = r.OldMeterMultiplier,
                OldBt = r.OldBt,
                OldCd = r.OldCd,
                OldTd = r.OldTd,
                OldSg = r.OldSg,
                OldVc = r.OldVc,
                OldPmax = r.OldPmax,
                NewMeterSerial = r.NewMeterSerial,
                NewMeterMultiplier = r.NewMeterMultiplier,
                NewBt = r.NewBt,
                NewCd = r.NewCd,
                NewTd = r.NewTd,
                NewSg = r.NewSg,
                NewVc = r.NewVc,
                NewPmax = r.NewPmax,
                DocumentUrl = r.DocumentUrl,
                Status = r.Status,
                CreatedTime = r.CreatedTime,
                CreatedBy = r.CreatedBy,
                CreatedName = r.CreatedName
            })
            .ToListAsync(cancellationToken);

        return new OffsetPaginationResponse<MeterReplacementDto>
        {
            CurrentPage = currentPage,
            PageSize = pageSize,
            Total = total,
            Data = items
        };
    }

    public async Task<MeterReplacementEntity?> GetByIdWithPointAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _entities.AsNoTracking()
            .Include(r => r.MeasurementPoint)
                .ThenInclude(p => p.Customer)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<MeterReplacementEntity?> GetApprovedReplacementInPeriodAsync(
        long measurementPointId,
        DateTimeOffset fromDate,
        DateTimeOffset toDate,
        CancellationToken cancellationToken = default)
    {
        return await _entities.AsNoTracking()
            .Where(r => r.MeasurementPointId == measurementPointId &&
                        r.Status == "APPROVED" &&
                        r.ReplacementDate >= fromDate &&
                        r.ReplacementDate <= toDate)
            .OrderByDescending(r => r.ReplacementDate)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PointMeterInfoDto?> GetPointMeterInfoAsync(long pointId, CancellationToken cancellationToken = default)
    {
        var point = await _dbContext.Set<MeasurementPointsEntity>()
            .AsNoTracking()
            .Include(p => p.Customer)
            .Include(p => p.TagNames)
            .FirstOrDefaultAsync(p => p.Id == pointId, cancellationToken);

        if (point == null) return null;

        // Tìm serial gần nhất: ưu tiên từ biên bản thay gần nhất (nếu có), hoặc từ TagNames, hoặc mã điểm đo
        var latestReplacement = await _entities.AsNoTracking()
            .Where(r => r.MeasurementPointId == pointId && r.Status == "APPROVED")
            .OrderByDescending(r => r.ReplacementDate)
            .FirstOrDefaultAsync(cancellationToken);

        var currentSerial = latestReplacement?.NewMeterSerial;
        if (string.IsNullOrWhiteSpace(currentSerial))
        {
            var meterTag = point.TagNames?.FirstOrDefault(t =>
                !string.IsNullOrWhiteSpace(t.TagName) &&
                (t.TagName.Contains("METER") || t.TagName.Contains("SERIAL") || t.Type == "METER"));
            currentSerial = meterTag?.CustomName ?? point.Code;
        }

        // Lấy bộ chỉ số chốt gần nhất từ PeriodicEnergyStatement
        var latestStatement = await _dbContext.Set<PeriodicEnergyStatementEntity>()
            .AsNoTracking()
            .Where(s => s.MeasurementPointId == pointId)
            .OrderByDescending(s => s.Year)
            .ThenByDescending(s => s.Month)
            .ThenByDescending(s => s.PeriodNumber)
            .FirstOrDefaultAsync(cancellationToken);

        return new PointMeterInfoDto
        {
            MeasurementPointId = point.Id,
            PointCode = point.Code,
            PointName = point.Description ?? point.Code,
            CustomerCode = point.Customer?.CustomerCode ?? string.Empty,
            CustomerName = point.Customer?.FullName ?? string.Empty,
            CurrentMultiplier = point.MultiplierFactor > 0m ? point.MultiplierFactor : 1.0m,
            CurrentSerial = currentSerial ?? point.Code,
            LatestBt = latestStatement?.Bt ?? 0m,
            LatestCd = latestStatement?.Cd ?? 0m,
            LatestTd = latestStatement?.Td ?? 0m,
            LatestSg = latestStatement?.Sg ?? 0m,
            LatestVc = latestStatement?.Vc ?? 0m,
            LatestPmax = latestStatement?.Pmax ?? 0m,
            LatestReadingTime = latestStatement?.BillingTimestamp
        };
    }
}

