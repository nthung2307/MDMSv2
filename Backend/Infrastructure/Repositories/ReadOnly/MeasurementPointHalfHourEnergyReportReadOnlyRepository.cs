using System.Globalization;
using Application.Common.ValueObjects.CursorBased;
using Application.DataTransferObjects.Requests.Report;
using Application.DataTransferObjects.Responses.Report;
using Application.Extensions;
using Application.Repositories.ReadOnly;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
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
    
    public MeasurementPointHalfHourEnergyReportReadOnlyRepository(AppReadOnlyDbContext context, ISnowflakeIdService snowflakeIdService, IMapper mapper) : base(context, snowflakeIdService)
    {
        _context = context;
        _measurementPointHalfHourEnergyReport = context.Set<MeasurementPointHalfHourEnergyReportEntity>();
        _mapper = mapper;
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
            .PaginateWithAscAndAsync<MeasurementPointHalfHourEnergyReportEntity, FinalizedEnergyReportResponse>(request, _mapper, cancellationToken);
        return result;
    }

    private static DateTimeOffset GetNearestHalfHourSlot(DateTimeOffset time)
    {
        int minute = time.Minute;
        int roundedMinute = minute < 15 ? 0 : (minute < 45 ? 30 : 0);
        var baseTime = minute >= 45 ? time.AddHours(1) : time;
        return new DateTimeOffset(baseTime.Year, baseTime.Month, baseTime.Day, baseTime.Hour, roundedMinute, 0, baseTime.Offset);
    }

    private static string? FormatEnergyValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        bool isNotTopical = value.EndsWith("*") || value.Contains("Not Topical");
        string cleanValue = value.Replace("*", "").Replace("(Not Topical)", "").Trim();

        if (double.TryParse(cleanValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double valueNumber))
        {
            var viCulture = CultureInfo.GetCultureInfo("vi-VN");
            string formatted = valueNumber % 1 == 0
                ? valueNumber.ToString("N0", viCulture)
                : valueNumber.ToString("N2", viCulture);
            return isNotTopical ? $"{formatted}*" : formatted;
        }
        return isNotTopical ? $"{cleanValue}*" : cleanValue;
    }

    private static string? EnsureSuspected(string? val)
    {
        if (string.IsNullOrWhiteSpace(val)) return null;
        val = val.Trim();
        return val.EndsWith("*") ? val : $"{val}*";
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
                    ChiSoTichLuyDienNangTacDungAPTongKenhGiao = EnsureSuspected(pastEntity.ChiSoTichLuyDienNangTacDungAPTongKenhGiao),
                    ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao = EnsureSuspected(pastEntity.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao),
                    ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao = EnsureSuspected(pastEntity.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao),
                    ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao = EnsureSuspected(pastEntity.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao),
                    ChiSoTichLuyDienNangPhanKhangAQTongKenhGiao = EnsureSuspected(pastEntity.ChiSoTichLuyDienNangPhanKhangAQTongKenhGiao),
                    ChiSoTichLuyDienNangTacDungAPTongKenhNhan = EnsureSuspected(pastEntity.ChiSoTichLuyDienNangTacDungAPTongKenhNhan),
                    ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan = EnsureSuspected(pastEntity.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan),
                    ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan = EnsureSuspected(pastEntity.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan),
                    ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan = EnsureSuspected(pastEntity.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan),
                    ChiSoTichLuyDienNangPhanKhangAQTongKenhNhan = EnsureSuspected(pastEntity.ChiSoTichLuyDienNangPhanKhangAQTongKenhNhan),
                };
            }
        }

        var result = new List<ActivePointsEnergyReportResponse>();
        foreach (var p in activePoints)
        {
            energyDict.TryGetValue(p.Id, out var energy);

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

                ChiSoTichLuyDienNangTacDungAPTongKenhGiao = FormatEnergyValue(energy?.ChiSoTichLuyDienNangTacDungAPTongKenhGiao),
                ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao = FormatEnergyValue(energy?.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao),
                ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao = FormatEnergyValue(energy?.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao),
                ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao = FormatEnergyValue(energy?.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao),
                ChiSoTichLuyDienNangPhanKhangAQTongKenhGiao = FormatEnergyValue(energy?.ChiSoTichLuyDienNangPhanKhangAQTongKenhGiao),

                ChiSoTichLuyDienNangTacDungAPTongKenhNhan = FormatEnergyValue(energy?.ChiSoTichLuyDienNangTacDungAPTongKenhNhan),
                ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan = FormatEnergyValue(energy?.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan),
                ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan = FormatEnergyValue(energy?.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan),
                ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan = FormatEnergyValue(energy?.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan),
                ChiSoTichLuyDienNangPhanKhangAQTongKenhNhan = FormatEnergyValue(energy?.ChiSoTichLuyDienNangPhanKhangAQTongKenhNhan),
            };
            result.Add(item);
        }

        return result;
    }
}