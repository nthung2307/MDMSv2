using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.ValueObjects.OffsetBased;
using Application.DataTransferObjects.Requests.MeterReplacement;
using Application.DataTransferObjects.Responses.MeterReplacement;
using Application.Repositories.ReadOnly;
using Application.Repositories.ReadWrite;
using Application.Services;
using Domain.Entities;
using Nobisoft.Core.Authorization.Interfaces;
using Nobisoft.Core.Interfaces;

namespace Infrastructure.Services;

public class MeterReplacementService : IMeterReplacementService
{
    private readonly IMeterReplacementReadOnlyRepository _readOnlyRepo;
    private readonly IMeterReplacementReadWriteRepository _readWriteRepo;
    private readonly IMeasurementPointsReadWriteRepository _pointsRepo;
    private readonly ISnowflakeIdService _snowflakeIdService;
    private readonly ICurrentUserService _currentUserService;

    public MeterReplacementService(
        IMeterReplacementReadOnlyRepository readOnlyRepo,
        IMeterReplacementReadWriteRepository readWriteRepo,
        IMeasurementPointsReadWriteRepository pointsRepo,
        ISnowflakeIdService snowflakeIdService,
        ICurrentUserService currentUserService)
    {
        _readOnlyRepo = readOnlyRepo;
        _readWriteRepo = readWriteRepo;
        _pointsRepo = pointsRepo;
        _snowflakeIdService = snowflakeIdService;
        _currentUserService = currentUserService;
    }

    public async Task<OffsetPaginationResponse<MeterReplacementDto>> GetListAsync(
        GetListMeterReplacementRequest request,
        CancellationToken cancellationToken = default)
    {
        return await _readOnlyRepo.GetPagedListAsync(request, cancellationToken);
    }

    public async Task<MeterReplacementDto?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _readOnlyRepo.GetByIdWithPointAsync(id, cancellationToken);
        if (entity == null) return null;

        return MapToDto(entity);
    }

    public async Task<MeterReplacementDto> CreateAsync(
        CreateMeterReplacementRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.MeasurementPointId <= 0)
            throw new ArgumentException("Mã điểm đo không hợp lệ.");

        if (string.IsNullOrWhiteSpace(request.OldMeterSerial))
            throw new ArgumentException("Số công tơ tháo không được để trống.");

        if (string.IsNullOrWhiteSpace(request.NewMeterSerial))
            throw new ArgumentException("Số công tơ treo mới không được để trống.");

        var point = await _pointsRepo.GetMeasurementPointByIdAsync(request.MeasurementPointId, cancellationToken);
        if (point == null)
            throw new KeyNotFoundException($"Không tìm thấy điểm đo ID {request.MeasurementPointId}");

        var now = DateTimeOffset.Now;
        var status = string.Equals(request.Status, "DRAFT", StringComparison.OrdinalIgnoreCase) ? "DRAFT" : "APPROVED";

        var entity = new MeterReplacementEntity
        {
            Id = await _snowflakeIdService.GenerateId(cancellationToken),
            MeasurementPointId = request.MeasurementPointId,
            ReplacementDate = request.ReplacementDate != default ? request.ReplacementDate : now,
            Reason = request.Reason?.Trim() ?? string.Empty,
            OldMeterSerial = request.OldMeterSerial.Trim(),
            OldMeterMultiplier = request.OldMeterMultiplier > 0m ? request.OldMeterMultiplier : 1.0m,
            OldBt = Math.Round(request.OldBt, 4, MidpointRounding.AwayFromZero),
            OldCd = Math.Round(request.OldCd, 4, MidpointRounding.AwayFromZero),
            OldTd = Math.Round(request.OldTd, 4, MidpointRounding.AwayFromZero),
            OldSg = Math.Round(request.OldSg, 4, MidpointRounding.AwayFromZero),
            OldVc = Math.Round(request.OldVc, 4, MidpointRounding.AwayFromZero),
            OldPmax = Math.Round(request.OldPmax, 4, MidpointRounding.AwayFromZero),
            NewMeterSerial = request.NewMeterSerial.Trim(),
            NewMeterMultiplier = request.NewMeterMultiplier > 0m ? request.NewMeterMultiplier : 1.0m,
            NewBt = Math.Round(request.NewBt, 4, MidpointRounding.AwayFromZero),
            NewCd = Math.Round(request.NewCd, 4, MidpointRounding.AwayFromZero),
            NewTd = Math.Round(request.NewTd, 4, MidpointRounding.AwayFromZero),
            NewSg = Math.Round(request.NewSg, 4, MidpointRounding.AwayFromZero),
            NewVc = Math.Round(request.NewVc, 4, MidpointRounding.AwayFromZero),
            NewPmax = Math.Round(request.NewPmax, 4, MidpointRounding.AwayFromZero),
            DocumentUrl = request.DocumentUrl?.Trim(),
            Status = status,
            CreatedTime = now,
            CreatedBy = _currentUserService.UserId,
            CreatedName = _currentUserService.UserName
        };

        await _readWriteRepo.AddAsync(entity, cancellationToken);
        await _readWriteRepo.SaveChangesAsync(cancellationToken);

        // Nếu trạng thái là APPROVED -> Tự động cập nhật hệ số nhân mới cho điểm đo
        if (status == "APPROVED")
        {
            point.MultiplierFactor = entity.NewMeterMultiplier;
            point.IsNewChange = true;
            await _pointsRepo.UpdateAsync(point, cancellationToken);
            await _pointsRepo.SaveChangesAsync(cancellationToken);
        }

        entity.MeasurementPoint = point;
        return MapToDto(entity);
    }

    public async Task<MeterReplacementDto> UpdateAsync(
        long id,
        UpdateMeterReplacementRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _readWriteRepo.GetByIdAsync(id, cancellationToken);
        if (entity == null)
            throw new KeyNotFoundException($"Không tìm thấy biên bản treo tháo ID {id}");

        if (entity.Status != "DRAFT")
            throw new InvalidOperationException("Chỉ được phép chỉnh sửa biên bản ở trạng thái Dự thảo (DRAFT).");

        entity.ReplacementDate = request.ReplacementDate != default ? request.ReplacementDate : entity.ReplacementDate;
        entity.Reason = request.Reason?.Trim() ?? string.Empty;
        entity.OldMeterSerial = !string.IsNullOrWhiteSpace(request.OldMeterSerial) ? request.OldMeterSerial.Trim() : entity.OldMeterSerial;
        entity.OldMeterMultiplier = request.OldMeterMultiplier > 0m ? request.OldMeterMultiplier : 1.0m;
        entity.OldBt = Math.Round(request.OldBt, 4, MidpointRounding.AwayFromZero);
        entity.OldCd = Math.Round(request.OldCd, 4, MidpointRounding.AwayFromZero);
        entity.OldTd = Math.Round(request.OldTd, 4, MidpointRounding.AwayFromZero);
        entity.OldSg = Math.Round(request.OldSg, 4, MidpointRounding.AwayFromZero);
        entity.OldVc = Math.Round(request.OldVc, 4, MidpointRounding.AwayFromZero);
        entity.OldPmax = Math.Round(request.OldPmax, 4, MidpointRounding.AwayFromZero);
        entity.NewMeterSerial = !string.IsNullOrWhiteSpace(request.NewMeterSerial) ? request.NewMeterSerial.Trim() : entity.NewMeterSerial;
        entity.NewMeterMultiplier = request.NewMeterMultiplier > 0m ? request.NewMeterMultiplier : 1.0m;
        entity.NewBt = Math.Round(request.NewBt, 4, MidpointRounding.AwayFromZero);
        entity.NewCd = Math.Round(request.NewCd, 4, MidpointRounding.AwayFromZero);
        entity.NewTd = Math.Round(request.NewTd, 4, MidpointRounding.AwayFromZero);
        entity.NewSg = Math.Round(request.NewSg, 4, MidpointRounding.AwayFromZero);
        entity.NewVc = Math.Round(request.NewVc, 4, MidpointRounding.AwayFromZero);
        entity.NewPmax = Math.Round(request.NewPmax, 4, MidpointRounding.AwayFromZero);
        entity.DocumentUrl = request.DocumentUrl?.Trim();

        // Nếu chuyển sang APPROVED
        if (string.Equals(request.Status, "APPROVED", StringComparison.OrdinalIgnoreCase))
        {
            entity.Status = "APPROVED";
            var point = await _pointsRepo.GetMeasurementPointByIdAsync(entity.MeasurementPointId, cancellationToken);
            if (point != null)
            {
                point.MultiplierFactor = entity.NewMeterMultiplier;
                point.IsNewChange = true;
                await _pointsRepo.UpdateAsync(point, cancellationToken);
                await _pointsRepo.SaveChangesAsync(cancellationToken);
            }
        }

        await _readWriteRepo.UpdateAsync(entity, cancellationToken);
        await _readWriteRepo.SaveChangesAsync(cancellationToken);

        var refreshed = await _readOnlyRepo.GetByIdWithPointAsync(id, cancellationToken);
        return MapToDto(refreshed ?? entity);
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _readWriteRepo.GetByIdAsync(id, cancellationToken);
        if (entity == null) return false;

        if (entity.Status != "DRAFT")
            throw new InvalidOperationException("Chỉ được phép xóa biên bản ở trạng thái Dự thảo (DRAFT).");

        await _readWriteRepo.RemoveAsync(entity, cancellationToken);
        await _readWriteRepo.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<MeterReplacementDto> ApproveAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _readWriteRepo.GetByIdAsync(id, cancellationToken);
        if (entity == null)
            throw new KeyNotFoundException($"Không tìm thấy biên bản treo tháo ID {id}");

        if (entity.Status != "DRAFT")
            throw new InvalidOperationException("Biên bản này không ở trạng thái Dự thảo.");

        entity.Status = "APPROVED";
        await _readWriteRepo.UpdateAsync(entity, cancellationToken);
        await _readWriteRepo.SaveChangesAsync(cancellationToken);

        var point = await _pointsRepo.GetMeasurementPointByIdAsync(entity.MeasurementPointId, cancellationToken);
        if (point != null)
        {
            point.MultiplierFactor = entity.NewMeterMultiplier;
            point.IsNewChange = true;
            await _pointsRepo.UpdateAsync(point, cancellationToken);
            await _pointsRepo.SaveChangesAsync(cancellationToken);
        }

        var refreshed = await _readOnlyRepo.GetByIdWithPointAsync(id, cancellationToken);
        return MapToDto(refreshed ?? entity);
    }

    public async Task<MeterReplacementDto> CancelAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _readWriteRepo.GetByIdAsync(id, cancellationToken);
        if (entity == null)
            throw new KeyNotFoundException($"Không tìm thấy biên bản treo tháo ID {id}");

        if (entity.Status != "DRAFT")
            throw new InvalidOperationException("Chỉ được phép hủy biên bản ở trạng thái Dự thảo (DRAFT).");

        entity.Status = "CANCELLED";
        await _readWriteRepo.UpdateAsync(entity, cancellationToken);
        await _readWriteRepo.SaveChangesAsync(cancellationToken);

        var refreshed = await _readOnlyRepo.GetByIdWithPointAsync(id, cancellationToken);
        return MapToDto(refreshed ?? entity);
    }

    public async Task<PointMeterInfoDto?> GetPointMeterInfoAsync(long pointId, CancellationToken cancellationToken = default)
    {
        return await _readOnlyRepo.GetPointMeterInfoAsync(pointId, cancellationToken);
    }

    private static MeterReplacementDto MapToDto(MeterReplacementEntity r)
    {
        return new MeterReplacementDto
        {
            Id = r.Id,
            MeasurementPointId = r.MeasurementPointId,
            MeasurementPointCode = r.MeasurementPoint?.Code ?? string.Empty,
            MeasurementPointDescription = r.MeasurementPoint?.Description ?? string.Empty,
            CustomerCode = r.MeasurementPoint?.Customer?.CustomerCode ?? string.Empty,
            CustomerName = r.MeasurementPoint?.Customer?.FullName ?? string.Empty,
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
        };
    }
}
