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
using Nobisoft.Core.Constants;
using Nobisoft.Core.Database.DbContext;
using Nobisoft.Core.Database.Repository;
using Nobisoft.Core.Interfaces;

namespace Infrastructure.Repositories.ReadOnly;

public class MeasurementPointHalfHourOperatingReportReadOnlyRepository :
    RestfulRepository<MeasurementPointHalfHourOperatingReportEntity, AppReadOnlyDbContext>,
    IMeasurementPointHalfHourOperatingReportReadOnlyRepository
{
    private readonly AppReadOnlyDbContext _dbContext;
    private readonly DbSet<MeasurementPointHalfHourOperatingReportEntity> _dbSet;
    private readonly DbSet<MeasurementPointMonitoringHourlyEntity> _hourlySet;
    private readonly DbSet<MeasurementPointMonitoringEntity> _monitoringSet;
    private readonly IMapper _mapper;

    public MeasurementPointHalfHourOperatingReportReadOnlyRepository(
        AppReadOnlyDbContext context,
        ISnowflakeIdService snowflakeIdService,
        IMapper mapper) : base(context, snowflakeIdService)
    {
        _dbContext = context;
        _dbSet = context.Set<MeasurementPointHalfHourOperatingReportEntity>();
        _hourlySet = context.Set<MeasurementPointMonitoringHourlyEntity>();
        _monitoringSet = context.Set<MeasurementPointMonitoringEntity>();
        _mapper = mapper;
    }

    public async Task<CursorPaginationResponse<OperatingParameterReportResponse>> GetOperatingParameterReportsAsync(
        OperatingParameterReportRequest request,
        CancellationToken cancellationToken)
    {
        var configuredVoltageUnit = await GetConfiguredVoltageUnitAsync(request.MeasurementPointId, cancellationToken);
        try
        {
            var query = _dbSet.AsNoTracking()
                .Where(m => m.MeasurementPointId == request.MeasurementPointId)
                .Where(m => m.CreatedTime >= request.StartTime && m.CreatedTime <= request.EndTime);

            var result = await query.PaginateWithAscAndAsync<MeasurementPointHalfHourOperatingReportEntity, OperatingParameterReportResponse>(
                request, _mapper, cancellationToken);

            if (result.Data != null && result.Data.Any())
            {
                foreach (var it in result.Data)
                {
                    if (string.IsNullOrWhiteSpace(it.DienApPhaAUnit)) it.DienApPhaAUnit = configuredVoltageUnit;
                    if (string.IsNullOrWhiteSpace(it.DienApPhaBUnit)) it.DienApPhaBUnit = configuredVoltageUnit;
                    if (string.IsNullOrWhiteSpace(it.DienApPhaCUnit)) it.DienApPhaCUnit = configuredVoltageUnit;
                }
                return result;
            }
        }
        catch
        {
            // Bảng chưa được tạo hoặc lỗi DB -> fallback sang dữ liệu thực tế
        }

        // Fallback: Lấy dữ liệu từ MeasurementPointMonitoringHourly hoặc MeasurementPointMonitoring
        var fallbackList = await GetFallbackOperatingReportsAsync(request, cancellationToken);
        return PaginateInMemory(fallbackList, request);
    }

    public async Task<List<OperatingParameterReportResponse>> GetAllOperatingParameterReportsAsync(
        OperatingParameterReportRequest request,
        CancellationToken cancellationToken)
    {
        var configuredVoltageUnit = await GetConfiguredVoltageUnitAsync(request.MeasurementPointId, cancellationToken);
        try
        {
            var query = _dbSet.AsNoTracking()
                .Where(m => m.MeasurementPointId == request.MeasurementPointId)
                .Where(m => m.CreatedTime >= request.StartTime && m.CreatedTime <= request.EndTime)
                .OrderBy(m => m.CreatedTime);

            var list = await query.ProjectTo<OperatingParameterReportResponse>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            if (list.Any())
            {
                foreach (var it in list)
                {
                    if (string.IsNullOrWhiteSpace(it.DienApPhaAUnit)) it.DienApPhaAUnit = configuredVoltageUnit;
                    if (string.IsNullOrWhiteSpace(it.DienApPhaBUnit)) it.DienApPhaBUnit = configuredVoltageUnit;
                    if (string.IsNullOrWhiteSpace(it.DienApPhaCUnit)) it.DienApPhaCUnit = configuredVoltageUnit;
                }
                return list;
            }
        }
        catch
        {
            // Bảng chưa có hoặc lỗi DB -> fallback
        }

        return await GetFallbackOperatingReportsAsync(request, cancellationToken);
    }

    private async Task<string> GetConfiguredVoltageUnitAsync(long measurementPointId, CancellationToken cancellationToken)
    {
        try
        {
            var unit = await _dbContext.Set<TagNameEntity>().AsNoTracking()
                .Where(t => t.MeasurementPointId == measurementPointId &&
                            (t.Description == ListDataMeasurementPoint.DienApPhaA || t.Description.Contains("Điện áp") || t.Description.Contains("dien ap")) &&
                            !t.Deleted && !string.IsNullOrEmpty(t.Unit))
                .Select(t => t.Unit)
                .FirstOrDefaultAsync(cancellationToken);
            if (!string.IsNullOrWhiteSpace(unit)) return unit;
        }
        catch { }
        return "kV";
    }

    private async Task<List<OperatingParameterReportResponse>> GetFallbackOperatingReportsAsync(
        OperatingParameterReportRequest request,
        CancellationToken cancellationToken)
    {
        var cultureInfo = CultureInfo.InvariantCulture;
        var configuredVoltageUnit = await GetConfiguredVoltageUnitAsync(request.MeasurementPointId, cancellationToken);

        // 1. Kiểm tra bảng MeasurementPointMonitoringHourly
        try
        {
            var hourlyData = await _hourlySet.AsNoTracking()
                .Where(h => h.MeasurementPointId == request.MeasurementPointId &&
                            h.CreatedTime >= request.StartTime && h.CreatedTime <= request.EndTime)
                .OrderBy(h => h.CreatedTime)
                .ToListAsync(cancellationToken);

            if (hourlyData.Any())
            {
                var responseList = new List<OperatingParameterReportResponse>();
                foreach (var h in hourlyData)
                {
                    double? p = double.TryParse(h.TrungBinhCongSuatTacDungTong3Pha, NumberStyles.Any, cultureInfo, out var pVal) ? pVal : null;
                    double? q = double.TryParse(h.TrungBinhCongSuatPhanKhangTong3Pha, NumberStyles.Any, cultureInfo, out var qVal) ? qVal : null;
                    double? pf = null;
                    if (p.HasValue && q.HasValue)
                    {
                        var s = Math.Sqrt(p.Value * p.Value + q.Value * q.Value);
                        if (s > 0) pf = Math.Round(Math.Abs(p.Value) / s, 3);
                    }

                    responseList.Add(new OperatingParameterReportResponse
                    {
                        Id = h.Id,
                        CreatedTime = h.CreatedTime,
                        DienApPhaA = double.TryParse(h.TrungBinhDienApPhaA, NumberStyles.Any, cultureInfo, out var ua) ? ua : null,
                        DienApPhaAUnit = !string.IsNullOrWhiteSpace(h.TrungBinhDienApPhaAUnit) ? h.TrungBinhDienApPhaAUnit : configuredVoltageUnit,
                        DienApPhaB = double.TryParse(h.TrungBinhDienApPhaB, NumberStyles.Any, cultureInfo, out var ub) ? ub : null,
                        DienApPhaBUnit = !string.IsNullOrWhiteSpace(h.TrungBinhDienApPhaBUnit) ? h.TrungBinhDienApPhaBUnit : configuredVoltageUnit,
                        DienApPhaC = double.TryParse(h.TrungBinhDienApPhaC, NumberStyles.Any, cultureInfo, out var uc) ? uc : null,
                        DienApPhaCUnit = !string.IsNullOrWhiteSpace(h.TrungBinhDienApPhaCUnit) ? h.TrungBinhDienApPhaCUnit : configuredVoltageUnit,
                        DongDienPhaA = double.TryParse(h.TrungBinhDongDienPhaA, NumberStyles.Any, cultureInfo, out var ia) ? ia : null,
                        DongDienPhaAUnit = !string.IsNullOrWhiteSpace(h.TrungBinhDongDienPhaAUnit) ? h.TrungBinhDongDienPhaAUnit : "A",
                        DongDienPhaB = double.TryParse(h.TrungBinhDongDienPhaB, NumberStyles.Any, cultureInfo, out var ib) ? ib : null,
                        DongDienPhaBUnit = !string.IsNullOrWhiteSpace(h.TrungBinhDongDienPhaBUnit) ? h.TrungBinhDongDienPhaBUnit : "A",
                        DongDienPhaC = double.TryParse(h.TrungBinhDongDienPhaC, NumberStyles.Any, cultureInfo, out var ic) ? ic : null,
                        DongDienPhaCUnit = !string.IsNullOrWhiteSpace(h.TrungBinhDongDienPhaCUnit) ? h.TrungBinhDongDienPhaCUnit : "A",
                        CongSuatTacDungTong3Pha = p,
                        CongSuatTacDungTong3PhaUnit = !string.IsNullOrWhiteSpace(h.CongSuatTacDungTong3PhaUnit) ? h.CongSuatTacDungTong3PhaUnit : "kW",
                        CongSuatPhanKhangTong3Pha = q,
                        CongSuatPhanKhangTong3PhaUnit = !string.IsNullOrWhiteSpace(h.TrungBinhCongSuatPhanKhangTong3PhaUnit) ? h.TrungBinhCongSuatPhanKhangTong3PhaUnit : "kVAr",
                        HeSoCongSuatPF = pf,
                        TanSoHeThong = 50.0,
                        TanSoHeThongUnit = "Hz"
                    });
                }
                return responseList;
            }
        }
        catch
        {
            // Bỏ qua lỗi hourly nếu có
        }

        // 2. Fallback sang dữ liệu đo đếm chi tiết MeasurementPointMonitoring
        try
        {
            var rawList = await _monitoringSet.AsNoTracking()
                .Where(x => x.MeasurementPointsId == request.MeasurementPointId &&
                            x.CreatedTime >= request.StartTime && x.CreatedTime <= request.EndTime &&
                            x.Value != null && x.Value != "")
                .Include(x => x.TagName)
                .OrderBy(x => x.CreatedTime)
                .ToListAsync(cancellationToken);

            if (rawList.Any())
            {
                // Nhóm theo mốc chu kỳ 30 phút
                var grouped = rawList
                    .GroupBy(x =>
                    {
                        var t = x.CreatedTime!.Value;
                        var min = t.Minute >= 30 ? 30 : 0;
                        return new DateTimeOffset(t.Year, t.Month, t.Day, t.Hour, min, 0, t.Offset);
                    })
                    .OrderBy(g => g.Key);

                var responseList = new List<OperatingParameterReportResponse>();
                foreach (var g in grouped)
                {
                    double? GetTagAvg(string tagDesc)
                    {
                        var vals = g.Where(x => x.TagNameDescription == tagDesc && double.TryParse(x.Value, NumberStyles.Any, cultureInfo, out _))
                            .Select(x => double.Parse(x.Value!, cultureInfo))
                            .ToList();
                        return vals.Any() ? Math.Round(vals.Average(), 2) : null;
                    }

                    string? GetTagUnit(string tagDesc, string defaultUnit)
                    {
                        return g.FirstOrDefault(x => x.TagNameDescription == tagDesc)?.TagName?.Unit ?? defaultUnit;
                    }

                    var p = GetTagAvg(ListDataMeasurementPoint.CongSuatTacDungTong3Pha);
                    var q = GetTagAvg(ListDataMeasurementPoint.CongSuatPhanKhangTong3Pha);
                    var pf = GetTagAvg(ListDataMeasurementPoint.HeSoCongSuatPFTrungBinh3Pha);
                    if (!pf.HasValue && p.HasValue && q.HasValue)
                    {
                        var s = Math.Sqrt(p.Value * p.Value + q.Value * q.Value);
                        if (s > 0) pf = Math.Round(Math.Abs(p.Value) / s, 3);
                    }

                    responseList.Add(new OperatingParameterReportResponse
                    {
                        Id = g.First().Id,
                        CreatedTime = g.Key,
                        DienApPhaA = GetTagAvg(ListDataMeasurementPoint.DienApPhaA),
                        DienApPhaAUnit = GetTagUnit(ListDataMeasurementPoint.DienApPhaA, configuredVoltageUnit),
                        DienApPhaB = GetTagAvg(ListDataMeasurementPoint.DienApPhaB),
                        DienApPhaBUnit = GetTagUnit(ListDataMeasurementPoint.DienApPhaB, configuredVoltageUnit),
                        DienApPhaC = GetTagAvg(ListDataMeasurementPoint.DienApPhaC),
                        DienApPhaCUnit = GetTagUnit(ListDataMeasurementPoint.DienApPhaC, configuredVoltageUnit),
                        DongDienPhaA = GetTagAvg(ListDataMeasurementPoint.DongDienPhaA),
                        DongDienPhaAUnit = GetTagUnit(ListDataMeasurementPoint.DongDienPhaA, "A"),
                        DongDienPhaB = GetTagAvg(ListDataMeasurementPoint.DongDienPhaB),
                        DongDienPhaBUnit = GetTagUnit(ListDataMeasurementPoint.DongDienPhaB, "A"),
                        DongDienPhaC = GetTagAvg(ListDataMeasurementPoint.DongDienPhaC),
                        DongDienPhaCUnit = GetTagUnit(ListDataMeasurementPoint.DongDienPhaC, "A"),
                        CongSuatTacDungTong3Pha = p,
                        CongSuatTacDungTong3PhaUnit = GetTagUnit(ListDataMeasurementPoint.CongSuatTacDungTong3Pha, "kW"),
                        CongSuatPhanKhangTong3Pha = q,
                        CongSuatPhanKhangTong3PhaUnit = GetTagUnit(ListDataMeasurementPoint.CongSuatPhanKhangTong3Pha, "kVAr"),
                        HeSoCongSuatPF = pf,
                        TanSoHeThong = GetTagAvg(ListDataMeasurementPoint.TanSoHeThong) ?? 50.0,
                        TanSoHeThongUnit = GetTagUnit(ListDataMeasurementPoint.TanSoHeThong, "Hz")
                    });
                }
                return responseList;
            }
        }
        catch
        {
            // Bỏ qua lỗi
        }

        return new List<OperatingParameterReportResponse>();
    }

    private static CursorPaginationResponse<OperatingParameterReportResponse> PaginateInMemory(
        List<OperatingParameterReportResponse> items,
        OperatingParameterReportRequest request)
    {
        var pageSize = request.PageSize > 0 ? request.PageSize : 48;
        var pagedItems = items.Take(pageSize).ToList();
        var hasNext = items.Count > pageSize;

        return new CursorPaginationResponse<OperatingParameterReportResponse>
        {
            Data = pagedItems,
            HasNext = hasNext,
            HasPrevious = false,
            TotalCount = items.Count
        };
    }
}
