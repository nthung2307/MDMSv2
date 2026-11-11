using System.Globalization;
using Application.Common.ValueObjects.CursorBased;
using Application.DataTransferObjects.Requests.Report;
using Application.DataTransferObjects.Responses.Report;
using Application.Extensions;
using Application.Repositories.ReadOnly;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities;
using Domain.Enums;
using Domain.Helpers;
using Microsoft.EntityFrameworkCore;
using Nobisoft.Core.Authorization;
using Nobisoft.Core.Authorization.Interfaces;
using Nobisoft.Core.Database.DbContext;
using Nobisoft.Core.Database.Repository;
using Nobisoft.Core.Interfaces;

namespace Infrastructure.Repositories.ReadOnly;

public class MeasurementPointHalfHourEnergyReportReadOnlyRepository :
    RestfulRepository<MeasurementPointHalfHourEnergyReportEntity, AppReadOnlyDbContext>, IMeasurementPointHalfHourEnergyReportReadOnlyRepository
{
    private readonly DbSet<MeasurementPointHalfHourEnergyReportEntity> _measurementPointHalfHourEnergyReport;
    private readonly AppReadOnlyDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService? _currentUserService;
    
    public MeasurementPointHalfHourEnergyReportReadOnlyRepository(
        AppReadOnlyDbContext context, 
        ISnowflakeIdService snowflakeIdService, 
        IMapper mapper,
        ICurrentUserService? currentUserService = null) : base(context, snowflakeIdService)
    {
        _context = context;
        _measurementPointHalfHourEnergyReport = context.Set<MeasurementPointHalfHourEnergyReportEntity>();
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    private async Task<(bool IsCustomer, List<long> LinkedCustomerIds)> GetCustomerAccessFilterAsync(CancellationToken cancellationToken)
    {
        if (_currentUserService == null) return (false, new List<long>());
        try
        {
            var userId = _currentUserService.UserId;
            if (userId <= 0) return (false, new List<long>());

            var user = await _context.Set<UserEntity>().AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId && !u.Deleted, cancellationToken);

            if (user != null && string.Equals(user.RoleName, DefaultRoles.Customer, StringComparison.OrdinalIgnoreCase))
            {
                var customerIds = await _context.Set<UserCustomerEntity>().AsNoTracking()
                    .Where(uc => uc.UserId == userId)
                    .Select(uc => uc.CustomerId)
                    .ToListAsync(cancellationToken);

                return (true, customerIds);
            }
        }
        catch
        {
            // ignored
        }
        return (false, new List<long>());
    }

    public async Task<CursorPaginationResponse<FinalizedEnergyReportResponse>> GetFinalizedEnergyReportsAsync(
        HalfHourFinalizedEnergyReportRequest request, 
        CancellationToken cancellationToken)
    {
        var measurementPointHalfHourEnergyReportEntity = _measurementPointHalfHourEnergyReport.AsNoTracking();
        var result = await measurementPointHalfHourEnergyReportEntity
            .Where(m => m.MeasurementPoint.CustomerId == request.CustomerId)
            .Where(m => m.MeasurementPointId == request.MeasurementPointId)
            .Where(m => m.CreatedTime >= request.StartTime && m.CreatedTime <= request.EndTime)
            .PaginateWithAscMappedAsync<MeasurementPointHalfHourEnergyReportEntity, FinalizedEnergyReportResponse>(request, _mapper, cancellationToken);
        return result;
    }

    private static DateTimeOffset GetNearestHalfHourSlot(DateTimeOffset time)
    {
        int minute = time.Minute;
        int roundedMinute = minute < 15 ? 0 : (minute < 45 ? 30 : 0);
        var baseTime = minute >= 45 ? time.AddHours(1) : time;
        return new DateTimeOffset(baseTime.Year, baseTime.Month, baseTime.Day, baseTime.Hour, roundedMinute, 0, baseTime.Offset);
    }

    private static string? FormatEnergyValue(decimal? value, int suspectedMask, EnergySuspectedFlags flag)
    {
        return MeteringValueHelper.FormatEnergyReportValue(value, suspectedMask, flag, null);
    }

    public async Task<List<ActivePointsEnergyReportResponse>> GetActivePointsEnergyReportAsync(
        ActivePointsEnergyReportRequest request, 
        CancellationToken cancellationToken)
    {
        var targetSlot = GetNearestHalfHourSlot(request.SelectedTime);
        var slotStart = targetSlot.AddMinutes(-5);
        var slotEnd = targetSlot.AddMinutes(5);

        var pointQuery = _context.Set<MeasurementPointsEntity>()
            .AsNoTracking()
            .Include(p => p.Customer)
            .Where(p => !p.Deleted && p.Status == "ACTIVE");

        var customerFilter = await GetCustomerAccessFilterAsync(cancellationToken);
        if (customerFilter.IsCustomer)
        {
            pointQuery = pointQuery.Where(p => customerFilter.LinkedCustomerIds.Contains(p.CustomerId));
        }

        if (request.CustomerId.HasValue && request.CustomerId.Value > 0)
        {
            pointQuery = pointQuery.Where(p => p.CustomerId == request.CustomerId.Value);
        }

        var activePoints = await pointQuery
            .OrderBy(p => p.CreatedTime)
            .ThenBy(p => p.Id)
            .ToListAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            activePoints = activePoints.Where(p =>
                (p.Code != null && p.Code.ToLower().Contains(search)) ||
                (p.Description != null && p.Description.ToLower().Contains(search)) ||
                (p.Customer != null && p.Customer.CustomerCode != null && p.Customer.CustomerCode.ToLower().Contains(search)) ||
                (p.Customer != null && p.Customer.FullName != null && p.Customer.FullName.ToLower().Contains(search))
            ).ToList();
        }

        if (!activePoints.Any())
        {
            return new List<ActivePointsEnergyReportResponse>();
        }

        var pointIds = activePoints.Select(p => p.Id).ToList();

        var energyRecords = await _measurementPointHalfHourEnergyReport.AsNoTracking()
            .Where(r => pointIds.Contains(r.MeasurementPointId) && r.CreatedTime >= slotStart && r.CreatedTime <= slotEnd)
            .OrderByDescending(r => r.CreatedTime)
            .ToListAsync(cancellationToken);

        var energyDict = energyRecords
            .GroupBy(r => r.MeasurementPointId)
            .ToDictionary(g => g.Key, g => g.First());

        // Đối với điểm đo mất kết nối: lấy số liệu Valid gần nhất trong quá khứ <= targetSlot và gắn cờ nghi ngờ (*)
        var missingPointIds = pointIds.Where(id => !energyDict.ContainsKey(id)).ToList();
        if (missingPointIds.Any())
        {
            var pastRecords = await _measurementPointHalfHourEnergyReport.AsNoTracking()
                .Where(r => missingPointIds.Contains(r.MeasurementPointId) && r.CreatedTime <= targetSlot)
                .OrderByDescending(r => r.CreatedTime)
                .ToListAsync(cancellationToken);

            var pastDict = pastRecords
                .GroupBy(r => r.MeasurementPointId)
                .ToDictionary(g => g.Key, g => g.First());

            foreach (var kvp in pastDict)
            {
                var pastEntity = kvp.Value;
                energyDict[kvp.Key] = new MeasurementPointHalfHourEnergyReportEntity
                {
                    MeasurementPointId = pastEntity.MeasurementPointId,
                    CreatedTime = pastEntity.CreatedTime,
                    ChiSoTichLuyDienNangTacDungAPTongKenhGiao = pastEntity.ChiSoTichLuyDienNangTacDungAPTongKenhGiao,
                    ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao = pastEntity.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao,
                    ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao = pastEntity.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao,
                    ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao = pastEntity.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao,
                    ChiSoTichLuyDienNangPhanKhangAQTongKenhGiao = pastEntity.ChiSoTichLuyDienNangPhanKhangAQTongKenhGiao,
                    ChiSoTichLuyDienNangTacDungAPTongKenhNhan = pastEntity.ChiSoTichLuyDienNangTacDungAPTongKenhNhan,
                    ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan = pastEntity.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan,
                    ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan = pastEntity.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan,
                    ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan = pastEntity.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan,
                    ChiSoTichLuyDienNangPhanKhangAQTongKenhNhan = pastEntity.ChiSoTichLuyDienNangPhanKhangAQTongKenhNhan,
                    SuspectedMask = 0x3FF,
                    IsSuspected = true
                };
            }
        }

        var result = new List<ActivePointsEnergyReportResponse>();
        foreach (var p in activePoints)
        {
            energyDict.TryGetValue(p.Id, out var energy);
            var mask = energy?.SuspectedMask ?? 0;

            var item = new ActivePointsEnergyReportResponse
            {
                MeasurementPointId = p.Id,
                MeasurementPointCode = p.Code ?? string.Empty,
                MeasurementPointName = p.Description ?? p.Code ?? string.Empty,
                CustomerId = p.CustomerId,
                CustomerCode = p.Customer?.CustomerCode ?? string.Empty,
                CustomerName = p.Customer?.FullName ?? string.Empty,
                MultiplierFactor = p.MultiplierFactor,
                CreatedTime = targetSlot,

                ChiSoTichLuyDienNangTacDungAPTongKenhGiao = FormatEnergyValue(energy?.ChiSoTichLuyDienNangTacDungAPTongKenhGiao, mask, EnergySuspectedFlags.ChiSoTichLuyDienNangTacDungAPTongKenhGiao),
                ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao = FormatEnergyValue(energy?.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao, mask, EnergySuspectedFlags.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao),
                ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao = FormatEnergyValue(energy?.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao, mask, EnergySuspectedFlags.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao),
                ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao = FormatEnergyValue(energy?.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao, mask, EnergySuspectedFlags.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao),
                ChiSoTichLuyDienNangPhanKhangAQTongKenhGiao = FormatEnergyValue(energy?.ChiSoTichLuyDienNangPhanKhangAQTongKenhGiao, mask, EnergySuspectedFlags.ChiSoTichLuyDienNangPhanKhangAQTongKenhGiao),

                ChiSoTichLuyDienNangTacDungAPTongKenhNhan = FormatEnergyValue(energy?.ChiSoTichLuyDienNangTacDungAPTongKenhNhan, mask, EnergySuspectedFlags.ChiSoTichLuyDienNangTacDungAPTongKenhNhan),
                ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan = FormatEnergyValue(energy?.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan, mask, EnergySuspectedFlags.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan),
                ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan = FormatEnergyValue(energy?.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan, mask, EnergySuspectedFlags.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan),
                ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan = FormatEnergyValue(energy?.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan, mask, EnergySuspectedFlags.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan),
                ChiSoTichLuyDienNangPhanKhangAQTongKenhNhan = FormatEnergyValue(energy?.ChiSoTichLuyDienNangPhanKhangAQTongKenhNhan, mask, EnergySuspectedFlags.ChiSoTichLuyDienNangPhanKhangAQTongKenhNhan),
            };
            result.Add(item);
        }

        return result;
    }
}