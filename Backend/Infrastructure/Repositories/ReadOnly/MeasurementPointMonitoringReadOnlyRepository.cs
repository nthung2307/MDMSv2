using System.Globalization;
using System.Text;
using Application.Common.ValueObjects.CursorBased;
using Application.DataTransferObjects.Requests.MeasurementPointMonitoring;
using Application.DataTransferObjects.Responses.MeasurementPointMonitoring;
using Application.Repositories.ReadOnly;
using Application.Services;
using CsvHelper;
using Domain.Entities;
using Domain.Enums;
using Domain.Helpers;
using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using Nobisoft.Core.Authorization;
using Nobisoft.Core.Authorization.Interfaces;
using Nobisoft.Core.Constants;
using Nobisoft.Core.Database.DbContext;
using Nobisoft.Core.Database.Repository;
using Nobisoft.Core.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Infrastructure.Repositories.ReadOnly;

public class MeasurementPointMonitoringReadOnlyRepository :
    RestfulRepository<MeasurementPointCurrentSnapshotEntity, AppReadOnlyDbContext>,
    IMeasurementPointMonitoringReadOnlyRepository
{
    private readonly DbSet<MeasurementPointsEntity> _measurementPointsEntities;
    private readonly DbSet<MeasurementPointCurrentSnapshotEntity> _snapshotEntities;
    private readonly DbSet<MeasurementPointTimeSeriesEntity> _timeSeriesEntities;
    private readonly DbSet<UserEntity> _userEntities;
    private readonly DbSet<UserCustomerEntity> _userCustomerEntities;
    private readonly ITagRoutingCacheService _tagRoutingCacheService;
    private readonly ICurrentUserService? _currentUserService;

    public MeasurementPointMonitoringReadOnlyRepository(
        AppReadOnlyDbContext applicationDbContext,
        ISnowflakeIdService snowflakeIdService,
        ITagRoutingCacheService tagRoutingCacheService,
        ICurrentUserService? currentUserService = null) : base(applicationDbContext, snowflakeIdService)
    {
        _measurementPointsEntities = applicationDbContext.Set<MeasurementPointsEntity>();
        _snapshotEntities = applicationDbContext.Set<MeasurementPointCurrentSnapshotEntity>();
        _timeSeriesEntities = applicationDbContext.Set<MeasurementPointTimeSeriesEntity>();
        _userEntities = applicationDbContext.Set<UserEntity>();
        _userCustomerEntities = applicationDbContext.Set<UserCustomerEntity>();
        _tagRoutingCacheService = tagRoutingCacheService;
        _currentUserService = currentUserService;
    }

    private async Task<(bool IsCustomer, List<long> LinkedCustomerIds)> GetCustomerAccessFilterAsync(CancellationToken cancellationToken)
    {
        if (_currentUserService == null) return (false, new List<long>());
        try
        {
            var userId = _currentUserService.UserId;
            if (userId <= 0) return (false, new List<long>());

            var user = await _userEntities.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId && !u.Deleted, cancellationToken);

            if (user != null && string.Equals(user.RoleName, DefaultRoles.Customer, StringComparison.OrdinalIgnoreCase))
            {
                var customerIds = await _userCustomerEntities.AsNoTracking()
                    .Where(uc => uc.UserId == userId)
                    .Select(uc => uc.CustomerId)
                    .ToListAsync(cancellationToken);

                return (true, customerIds);
            }
        }
        catch
        {
            // Ignore lookup failures when invoked outside HTTP request context
        }

        return (false, new List<long>());
    }

    public async Task<CursorPaginationResponse<ListMeasurementPointMonitoringDto>> GetListMeasurementPointsMonitoringAsync(
        GetListMeasurementPointMonitoringRequest request,
        CancellationToken cancellationToken)
    {
        var query = from p in _measurementPointsEntities.AsNoTracking().Include(x => x.Customer)
                    join s in _snapshotEntities.AsNoTracking() on p.Id equals s.MeasurementPointId into snapGroup
                    from snap in snapGroup.DefaultIfEmpty()
                    where p.Status == EntityStatus.Active && !p.Deleted
                    select new { Point = p, Snapshot = snap };

        var (isCustomer, linkedCustomerIds) = await GetCustomerAccessFilterAsync(cancellationToken);
        if (isCustomer)
        {
            query = query.Where(x => linkedCustomerIds.Contains(x.Point.CustomerId));
        }

        if (!string.IsNullOrEmpty(request.CustomerName))
        {
            query = query.Where(x => x.Point.Customer != null &&
                                     x.Point.Customer.FullName != null &&
                                     x.Point.Customer.FullName.Contains(request.CustomerName));
        }

        if (!string.IsNullOrEmpty(request.MeasurementPointCode))
        {
            query = query.Where(x => x.Point.Code != null && x.Point.Code.Contains(request.MeasurementPointCode));
        }

        if (request.Status.HasValue)
        {
            var isLost = request.Status.Value;
            query = isLost
                ? query.Where(x => x.Snapshot != null && x.Snapshot.MatKetNoi == true)
                : query.Where(x => x.Snapshot != null && x.Snapshot.MatKetNoi == false);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var pageSize = request.PageSize > 0 ? request.PageSize : 10;

        List<(MeasurementPointsEntity Point, MeasurementPointCurrentSnapshotEntity? Snapshot)> pageRows;
        bool hasNext;
        bool hasPrevious;

        if (request.After.HasValue)
        {
            var raw = await query.Where(x => x.Point.Id > request.After.Value)
                .OrderBy(x => x.Point.Id)
                .Take(pageSize + 1)
                .ToListAsync(cancellationToken);

            hasPrevious = true;
            hasNext = raw.Count > pageSize;
            pageRows = raw.Take(pageSize).Select(x => (x.Point, x.Snapshot)).ToList();
        }
        else if (request.Before.HasValue)
        {
            var raw = await query.Where(x => x.Point.Id < request.Before.Value)
                .OrderByDescending(x => x.Point.Id)
                .Take(pageSize + 1)
                .ToListAsync(cancellationToken);

            hasNext = true;
            hasPrevious = raw.Count > pageSize;
            var taken = raw.Take(pageSize).Reverse().ToList();
            pageRows = taken.Select(x => (x.Point, x.Snapshot)).ToList();
        }
        else if (request.PageNumber is > 0)
        {
            var pageNum = request.PageNumber.Value;
            var raw = await query.OrderBy(x => x.Point.Id)
                .Skip((pageNum - 1) * pageSize)
                .Take(pageSize + 1)
                .ToListAsync(cancellationToken);

            hasPrevious = pageNum > 1 && totalCount > 0;
            hasNext = raw.Count > pageSize;
            pageRows = raw.Take(pageSize).Select(x => (x.Point, x.Snapshot)).ToList();
        }
        else
        {
            var raw = await query.OrderBy(x => x.Point.Id)
                .Take(pageSize + 1)
                .ToListAsync(cancellationToken);

            hasPrevious = false;
            hasNext = raw.Count > pageSize;
            pageRows = raw.Take(pageSize).Select(x => (x.Point, x.Snapshot)).ToList();
        }

        var pointMap = await _tagRoutingCacheService.GetPointMapAsync(cancellationToken);
        var resultList = new List<ListMeasurementPointMonitoringDto>(pageRows.Count);
        foreach (var (point, snap) in pageRows)
        {
            pointMap.TryGetValue(point.Id, out var routing);
            resultList.Add(SnapshotDtoMapper.ToListDto(point, snap, routing));
        }

        return new CursorPaginationResponse<ListMeasurementPointMonitoringDto>
        {
            TotalCount = totalCount,
            HasNext = hasNext,
            HasPrevious = hasPrevious,
            Data = resultList
        };
    }

    private async Task<List<ListMeasurementPointMonitoringDto>> GetAllForExportAsync(CancellationToken cancellationToken)
    {
        var query = from p in _measurementPointsEntities.AsNoTracking().Include(x => x.Customer)
                    join s in _snapshotEntities.AsNoTracking() on p.Id equals s.MeasurementPointId into snapGroup
                    from snap in snapGroup.DefaultIfEmpty()
                    where p.Status == EntityStatus.Active && !p.Deleted
                    orderby p.Id
                    select new { Point = p, Snapshot = snap };

        var (isCustomer, linkedCustomerIds) = await GetCustomerAccessFilterAsync(cancellationToken);
        if (isCustomer)
        {
            query = query.Where(x => linkedCustomerIds.Contains(x.Point.CustomerId));
        }

        var rows = await query.ToListAsync(cancellationToken);
        var pointMap = await _tagRoutingCacheService.GetPointMapAsync(cancellationToken);

        return rows.Select(r =>
        {
            pointMap.TryGetValue(r.Point.Id, out var routing);
            return SnapshotDtoMapper.ToListDto(r.Point, r.Snapshot, routing);
        }).ToList();
    }

    public async Task<byte[]> ExportPdfMeasurementPointsMonitoringAsync(CancellationToken cancellationToken)
    {
        var list = await GetAllForExportAsync(cancellationToken);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(20);

                page.Header().Row(row =>
                {
                    row.RelativeItem().Text("- Hệ thống quản lý dữ liệu điểm đo")
                        .FontFamily(Fonts.Arial).Style(TextStyle.Default.Size(12));
                    row.RelativeItem().AlignRight().Text("Quản lý thông số tổng hợp")
                        .FontFamily(Fonts.Arial).Style(TextStyle.Default.Size(12));
                });

                page.Content().PaddingTop(14).Column(column =>
                {
                    column.Item().AlignCenter().PaddingBottom(14)
                        .Text("Quản lý thông số tổng hợp")
                        .FontFamily(Fonts.Arial).Style(TextStyle.Default.Size(16).Bold());

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(35);
                            columns.RelativeColumn(1.2f);
                            columns.RelativeColumn(2.0f);
                            columns.RelativeColumn(1.1f);
                            columns.RelativeColumn(0.8f);
                            columns.RelativeColumn(0.8f);
                            columns.RelativeColumn(0.8f);
                            columns.RelativeColumn(0.8f);
                            columns.RelativeColumn(0.6f);
                            columns.RelativeColumn(0.6f);
                        });

                        var headerStyle = TextStyle.Default.Size(10).Bold();
                        table.Header(header =>
                        {
                            header.Cell().Border(1).Background("#dcdcdc").BorderColor(Colors.Black).PaddingVertical(4).PaddingHorizontal(6).Text("STT").FontFamily(Fonts.Arial).Style(headerStyle);
                            header.Cell().Border(1).Background("#dcdcdc").BorderColor(Colors.Black).PaddingVertical(4).PaddingHorizontal(6).Text("Mã điểm đo").FontFamily(Fonts.Arial).Style(headerStyle);
                            header.Cell().Border(1).Background("#dcdcdc").BorderColor(Colors.Black).PaddingVertical(4).PaddingHorizontal(6).Text("Tên khách hàng").FontFamily(Fonts.Arial).Style(headerStyle);
                            header.Cell().Border(1).Background("#dcdcdc").BorderColor(Colors.Black).PaddingVertical(4).PaddingHorizontal(6).Text("Trạng thái kết nối").FontFamily(Fonts.Arial).Style(headerStyle);
                            header.Cell().Border(1).Background("#dcdcdc").BorderColor(Colors.Black).PaddingVertical(4).PaddingHorizontal(6).Text("U").FontFamily(Fonts.Arial).Style(headerStyle);
                            header.Cell().Border(1).Background("#dcdcdc").BorderColor(Colors.Black).PaddingVertical(4).PaddingHorizontal(6).Text("I").FontFamily(Fonts.Arial).Style(headerStyle);
                            header.Cell().Border(1).Background("#dcdcdc").BorderColor(Colors.Black).PaddingVertical(4).PaddingHorizontal(6).Text("P").FontFamily(Fonts.Arial).Style(headerStyle);
                            header.Cell().Border(1).Background("#dcdcdc").BorderColor(Colors.Black).PaddingVertical(4).PaddingHorizontal(6).Text("Q").FontFamily(Fonts.Arial).Style(headerStyle);
                            header.Cell().Border(1).Background("#dcdcdc").BorderColor(Colors.Black).PaddingVertical(4).PaddingHorizontal(6).Text("PF").FontFamily(Fonts.Arial).Style(headerStyle);
                            header.Cell().Border(1).Background("#dcdcdc").BorderColor(Colors.Black).PaddingVertical(4).PaddingHorizontal(6).Text("F").FontFamily(Fonts.Arial).Style(headerStyle);
                        });

                        var rowStyle = TextStyle.Default.Size(10);
                        for (var i = 0; i < list.Count; i++)
                        {
                            var customer = list[i];
                            var connectionText = customer.MatKetNoi == null
                                ? "-"
                                : (customer.MatKetNoi.Value ? "Mất kết nối" : "Kết nối");

                            table.Cell().Border(1).BorderColor(Colors.Black).PaddingVertical(4).PaddingHorizontal(6).Text((i + 1).ToString()).FontFamily(Fonts.Arial).Style(rowStyle);
                            table.Cell().Border(1).BorderColor(Colors.Black).PaddingVertical(4).PaddingHorizontal(6).Text(customer.MeasurementPointCode ?? "").FontFamily(Fonts.Arial).Style(rowStyle);
                            table.Cell().Border(1).BorderColor(Colors.Black).PaddingVertical(4).PaddingHorizontal(6).Text(customer.CustomerName ?? "").FontFamily(Fonts.Arial).Style(rowStyle);
                            table.Cell().Border(1).BorderColor(Colors.Black).PaddingVertical(4).PaddingHorizontal(6).Text(connectionText).FontFamily(Fonts.Arial).Style(rowStyle);
                            table.Cell().Border(1).BorderColor(Colors.Black).PaddingVertical(4).PaddingHorizontal(6).Text(customer.DienApPhaA != null ? $"{customer.DienApPhaA} {customer.DienApPhaAUnit}".Trim() : "-").FontFamily(Fonts.Arial).Style(rowStyle);
                            table.Cell().Border(1).BorderColor(Colors.Black).PaddingVertical(4).PaddingHorizontal(6).Text(customer.DongDienPhaA != null ? $"{customer.DongDienPhaA} {customer.DongDienPhaAUnit}".Trim() : "-").FontFamily(Fonts.Arial).Style(rowStyle);
                            table.Cell().Border(1).BorderColor(Colors.Black).PaddingVertical(4).PaddingHorizontal(6).Text(customer.CongSuatTacDungTong3Pha != null ? $"{customer.CongSuatTacDungTong3Pha} {customer.CongSuatTacDungTong3PhaUnit}".Trim() : "-").FontFamily(Fonts.Arial).Style(rowStyle);
                            table.Cell().Border(1).BorderColor(Colors.Black).PaddingVertical(4).PaddingHorizontal(6).Text(customer.CongSuatPhanKhangTong3Pha != null ? $"{customer.CongSuatPhanKhangTong3Pha} {customer.CongSuatPhanKhangTong3PhaUnit}".Trim() : "-").FontFamily(Fonts.Arial).Style(rowStyle);
                            table.Cell().Border(1).BorderColor(Colors.Black).PaddingVertical(4).PaddingHorizontal(6).Text(customer.HeSoCongSuatPFTrungBinh3Pha?.ToString(CultureInfo.InvariantCulture) ?? "-").FontFamily(Fonts.Arial).Style(rowStyle);
                            table.Cell().Border(1).BorderColor(Colors.Black).PaddingVertical(4).PaddingHorizontal(6).Text(customer.TanSoHeThong != null ? $"{customer.TanSoHeThong} {customer.TanSoHeThongUnit}".Trim() : "-").FontFamily(Fonts.Arial).Style(rowStyle);
                        }
                    });
                });

                page.Footer().PaddingTop(12).Row(row =>
                {
                    row.RelativeItem().Text(text =>
                    {
                        text.Span($"Thời gian: {DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7)):HH:mm:ss, dd/MM/yyyy}")
                            .FontFamily(Fonts.Arial).Style(TextStyle.Default.FontSize(10));
                    });
                    row.RelativeItem().AlignRight().Text(text =>
                    {
                        text.Span("Trang ").FontFamily(Fonts.Arial).Style(TextStyle.Default.FontSize(10));
                        text.CurrentPageNumber().FontFamily(Fonts.Arial).Style(TextStyle.Default.FontSize(10));
                        text.Span(" / ").FontFamily(Fonts.Arial).Style(TextStyle.Default.FontSize(10));
                        text.TotalPages().FontFamily(Fonts.Arial).Style(TextStyle.Default.FontSize(10));
                    });
                });
            });
        });

        return document.GeneratePdf();
    }

    public async Task<byte[]> ExportCsvMeasurementPointsMonitoringAsync(CancellationToken cancellationToken)
    {
        var list = await GetAllForExportAsync(cancellationToken);

        using var memoryStream = new MemoryStream();
        await using var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8);
        await using var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);

        csvWriter.WriteField("STT");
        csvWriter.WriteField("Mã điểm đo");
        csvWriter.WriteField("Tên khách hàng");
        csvWriter.WriteField("Trạng thái kết nối");
        csvWriter.WriteField("U");
        csvWriter.WriteField("I");
        csvWriter.WriteField("P");
        csvWriter.WriteField("Q");
        csvWriter.WriteField("PF");
        csvWriter.WriteField("F");
        await csvWriter.NextRecordAsync();

        for (var i = 0; i < list.Count; i++)
        {
            var customer = list[i];
            var connectionText = customer.MatKetNoi == null
                ? "-"
                : (customer.MatKetNoi.Value ? "Mất kết nối" : "Kết nối");

            csvWriter.WriteField(i + 1);
            csvWriter.WriteField(customer.MeasurementPointCode ?? "");
            csvWriter.WriteField(customer.CustomerName ?? "");
            csvWriter.WriteField(connectionText);
            csvWriter.WriteField(customer.DienApPhaA != null ? $"{customer.DienApPhaA} {customer.DienApPhaAUnit}".Trim() : "-");
            csvWriter.WriteField(customer.DongDienPhaA != null ? $"{customer.DongDienPhaA} {customer.DongDienPhaAUnit}".Trim() : "-");
            csvWriter.WriteField(customer.CongSuatTacDungTong3Pha != null ? $"{customer.CongSuatTacDungTong3Pha} {customer.CongSuatTacDungTong3PhaUnit}".Trim() : "-");
            csvWriter.WriteField(customer.CongSuatPhanKhangTong3Pha != null ? $"{customer.CongSuatPhanKhangTong3Pha} {customer.CongSuatPhanKhangTong3PhaUnit}".Trim() : "-");
            csvWriter.WriteField(customer.HeSoCongSuatPFTrungBinh3Pha != null ? customer.HeSoCongSuatPFTrungBinh3Pha.ToString() : "-");
            csvWriter.WriteField(customer.TanSoHeThong != null ? $"{customer.TanSoHeThong} {customer.TanSoHeThongUnit}".Trim() : "-");
            await csvWriter.NextRecordAsync();
        }

        await streamWriter.FlushAsync(cancellationToken);
        return memoryStream.ToArray();
    }

    public async Task<MeasurementPointMonitoringDto?> GetMeasurementPointDetailAsync(string tagName, CancellationToken cancellationToken)
    {
        var point = await _measurementPointsEntities.AsNoTracking()
            .Include(x => x.Customer)
            .Where(x => x.Code == tagName && x.Status == EntityStatus.Active && !x.Deleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (point == null) return null;

        var snap = await _snapshotEntities.AsNoTracking()
            .FirstOrDefaultAsync(s => s.MeasurementPointId == point.Id, cancellationToken);

        var pointMap = await _tagRoutingCacheService.GetPointMapAsync(cancellationToken);
        pointMap.TryGetValue(point.Id, out var routing);

        return SnapshotDtoMapper.ToDetailDto(point, snap, routing);
    }

    public async Task<MeasurementPointMonitoringByIoTDto?> GetMeasurementPointDetailByIoTAsync(string tagName, CancellationToken cancellationToken)
    {
        var point = await _measurementPointsEntities.AsNoTracking()
            .Include(x => x.Customer)
            .Where(x => x.Code == tagName && x.Status == EntityStatus.Active && !x.Deleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (point == null) return null;

        var snap = await _snapshotEntities.AsNoTracking()
            .FirstOrDefaultAsync(s => s.MeasurementPointId == point.Id, cancellationToken);

        var pointMap = await _tagRoutingCacheService.GetPointMapAsync(cancellationToken);
        pointMap.TryGetValue(point.Id, out var routing);

        return SnapshotDtoMapper.ToIoTDto(point, snap, routing);
    }

    public Task<List<MeasurementPointEnergyReportDto>> GetListMeasurementEnergyReportAsync(CancellationToken cancellationToken)
    {
        return GetListMeasurementEnergyReportWithLookbackAsync(120, cancellationToken);
    }

    public async Task<List<MeasurementPointEnergyReportDto>> GetListMeasurementEnergyReportWithLookbackAsync(
        int lookbackMinutes,
        CancellationToken cancellationToken,
        DateTimeOffset? targetTime = null)
    {
        if (lookbackMinutes <= 0) lookbackMinutes = 120;

        var target = targetTime ?? GetNearestHalfHourSlot(DateTimeOffset.UtcNow.AddMinutes(-5));
        var windowStart = target.AddMinutes(-5);
        var windowEnd = target.AddMinutes(5);
        var lookbackStart = target.AddMinutes(-Math.Max(lookbackMinutes, 60));

        var activePointIds = await _measurementPointsEntities
            .AsNoTracking()
            .Where(x => !x.Deleted && x.Status == EntityStatus.Active)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        if (activePointIds.Count == 0)
        {
            return new List<MeasurementPointEnergyReportDto>();
        }

        var minYm = MeteringValueHelper.ComputePartitionYm(lookbackStart);
        var maxYm = MeteringValueHelper.ComputePartitionYm(windowEnd);

        var tsRows = await _timeSeriesEntities
            .AsNoTracking()
            .Where(t => activePointIds.Contains(t.MeasurementPointId) &&
                        t.PartitionYm >= minYm &&
                        t.PartitionYm <= maxYm &&
                        t.RecordedAt >= lookbackStart &&
                        t.RecordedAt <= windowEnd &&
                        (t.ChiSoTichLuyDienNangTacDungAPTong.HasValue ||
                         t.ChiSoTichLuyDienNangTacDungAPBieuGioBT.HasValue ||
                         t.ChiSoTichLuyDienNangTacDungAPBieuGioCD.HasValue ||
                         t.ChiSoTichLuyDienNangTacDungAPBieuGioTD.HasValue ||
                         t.ChiSoTichLuyDienNangPhanKhangAQTong.HasValue ||
                         t.ChiSoTichLuyDienNangTacDungAPTongChieuNhan.HasValue ||
                         t.ChiSoTichLuyDienNangTacDungAPBieuGioBTChieuNhan.HasValue ||
                         t.ChiSoTichLuyDienNangTacDungAPBieuGioCDChieuNhan.HasValue ||
                         t.ChiSoTichLuyDienNangTacDungAPBieuGioTDChieuNhan.HasValue ||
                         t.ChiSoTichLuyDienNangPhanKhangAQTongChieuNhan.HasValue))
            .Select(t => new
            {
                t.MeasurementPointId,
                t.RecordedAt,
                t.QualityFlag,
                t.ChiSoTichLuyDienNangTacDungAPTong,
                t.ChiSoTichLuyDienNangTacDungAPBieuGioBT,
                t.ChiSoTichLuyDienNangTacDungAPBieuGioCD,
                t.ChiSoTichLuyDienNangTacDungAPBieuGioTD,
                t.ChiSoTichLuyDienNangPhanKhangAQTong,
                t.ChiSoTichLuyDienNangTacDungAPTongChieuNhan,
                t.ChiSoTichLuyDienNangTacDungAPBieuGioBTChieuNhan,
                t.ChiSoTichLuyDienNangTacDungAPBieuGioCDChieuNhan,
                t.ChiSoTichLuyDienNangTacDungAPBieuGioTDChieuNhan,
                t.ChiSoTichLuyDienNangPhanKhangAQTongChieuNhan
            })
            .ToListAsync(cancellationToken);

        var snapshots = await _snapshotEntities
            .AsNoTracking()
            .Where(s => activePointIds.Contains(s.MeasurementPointId))
            .ToListAsync(cancellationToken);

        var tsByPoint = tsRows.GroupBy(r => r.MeasurementPointId).ToDictionary(g => g.Key, g => g.ToList());
        var snapByPoint = snapshots.ToDictionary(s => s.MeasurementPointId, s => s);

        var result = new List<MeasurementPointEnergyReportDto>();

        foreach (var pointId in activePointIds)
        {
            tsByPoint.TryGetValue(pointId, out var pointTs);
            snapByPoint.TryGetValue(pointId, out var snap);

            var mask = 0;

            decimal? ResolveChannel<T>(
                Func<dynamic, decimal?> tsSelector,
                decimal? snapVal,
                bool? snapQuality,
                DateTimeOffset? snapUpdatedAt,
                EnergySuspectedFlags flag)
            {
                if (pointTs != null)
                {
                    var inExactWindow = pointTs
                        .Where(r => r.RecordedAt >= windowStart && r.RecordedAt <= windowEnd && tsSelector(r).HasValue)
                        .OrderBy(r => Math.Abs((r.RecordedAt - target).TotalSeconds))
                        .FirstOrDefault();

                    if (inExactWindow != null)
                    {
                        if (!inExactWindow.QualityFlag)
                        {
                            mask = MeteringValueHelper.SetSuspectedFlag(mask, flag, true);
                        }
                        return tsSelector(inExactWindow);
                    }

                    var inLookback = pointTs
                        .Where(r => tsSelector(r).HasValue)
                        .OrderBy(r => Math.Abs((r.RecordedAt - target).TotalSeconds))
                        .FirstOrDefault();

                    if (inLookback != null)
                    {
                        mask = MeteringValueHelper.SetSuspectedFlag(mask, flag, true);
                        return tsSelector(inLookback);
                    }
                }

                if (snapVal.HasValue)
                {
                    var isFreshAndGood = snapUpdatedAt.HasValue &&
                                         snapUpdatedAt.Value >= windowStart &&
                                         snapUpdatedAt.Value <= windowEnd &&
                                         snapQuality == true;
                    if (!isFreshAndGood)
                    {
                        mask = MeteringValueHelper.SetSuspectedFlag(mask, flag, true);
                    }
                    return snapVal;
                }

                return null;
            }

            var dto = new MeasurementPointEnergyReportDto
            {
                MeasurementPointId = pointId,
                CreatedTime = target,
                ChiSoTichLuyDienNangTacDungAPTongKenhGiao = ResolveChannel<object>(r => r.ChiSoTichLuyDienNangTacDungAPTong, snap?.ChiSoTichLuyDienNangTacDungAPTong, snap?.ChiSoTichLuyDienNangTacDungAPTongQuality, snap?.ChiSoTichLuyDienNangTacDungAPTongUpdatedAt, EnergySuspectedFlags.ChiSoTichLuyDienNangTacDungAPTongKenhGiao),
                ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao = ResolveChannel<object>(r => r.ChiSoTichLuyDienNangTacDungAPBieuGioBT, snap?.ChiSoTichLuyDienNangTacDungAPBieuGioBT, snap?.ChiSoTichLuyDienNangTacDungAPBieuGioBTQuality, snap?.ChiSoTichLuyDienNangTacDungAPBieuGioBTUpdatedAt, EnergySuspectedFlags.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao),
                ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao = ResolveChannel<object>(r => r.ChiSoTichLuyDienNangTacDungAPBieuGioCD, snap?.ChiSoTichLuyDienNangTacDungAPBieuGioCD, snap?.ChiSoTichLuyDienNangTacDungAPBieuGioCDQuality, snap?.ChiSoTichLuyDienNangTacDungAPBieuGioCDUpdatedAt, EnergySuspectedFlags.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao),
                ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao = ResolveChannel<object>(r => r.ChiSoTichLuyDienNangTacDungAPBieuGioTD, snap?.ChiSoTichLuyDienNangTacDungAPBieuGioTD, snap?.ChiSoTichLuyDienNangTacDungAPBieuGioTDQuality, snap?.ChiSoTichLuyDienNangTacDungAPBieuGioTDUpdatedAt, EnergySuspectedFlags.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao),
                ChiSoTichLuyDienNangPhanKhangAQTongKenhGiao = ResolveChannel<object>(r => r.ChiSoTichLuyDienNangPhanKhangAQTong, snap?.ChiSoTichLuyDienNangPhanKhangAQTong, snap?.ChiSoTichLuyDienNangPhanKhangAQTongQuality, snap?.ChiSoTichLuyDienNangPhanKhangAQTongUpdatedAt, EnergySuspectedFlags.ChiSoTichLuyDienNangPhanKhangAQTongKenhGiao),
                ChiSoTichLuyDienNangTacDungAPTongKenhNhan = ResolveChannel<object>(r => r.ChiSoTichLuyDienNangTacDungAPTongChieuNhan, snap?.ChiSoTichLuyDienNangTacDungAPTongChieuNhan, snap?.ChiSoTichLuyDienNangTacDungAPTongChieuNhanQuality, snap?.ChiSoTichLuyDienNangTacDungAPTongChieuNhanUpdatedAt, EnergySuspectedFlags.ChiSoTichLuyDienNangTacDungAPTongKenhNhan),
                ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan = ResolveChannel<object>(r => r.ChiSoTichLuyDienNangTacDungAPBieuGioBTChieuNhan, snap?.ChiSoTichLuyDienNangTacDungAPBieuGioBTChieuNhan, snap?.ChiSoTichLuyDienNangTacDungAPBieuGioBTChieuNhanQuality, snap?.ChiSoTichLuyDienNangTacDungAPBieuGioBTChieuNhanUpdatedAt, EnergySuspectedFlags.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan),
                ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan = ResolveChannel<object>(r => r.ChiSoTichLuyDienNangTacDungAPBieuGioCDChieuNhan, snap?.ChiSoTichLuyDienNangTacDungAPBieuGioCDChieuNhan, snap?.ChiSoTichLuyDienNangTacDungAPBieuGioCDChieuNhanQuality, snap?.ChiSoTichLuyDienNangTacDungAPBieuGioCDChieuNhanUpdatedAt, EnergySuspectedFlags.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan),
                ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan = ResolveChannel<object>(r => r.ChiSoTichLuyDienNangTacDungAPBieuGioTDChieuNhan, snap?.ChiSoTichLuyDienNangTacDungAPBieuGioTDChieuNhan, snap?.ChiSoTichLuyDienNangTacDungAPBieuGioTDChieuNhanQuality, snap?.ChiSoTichLuyDienNangTacDungAPBieuGioTDChieuNhanUpdatedAt, EnergySuspectedFlags.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan),
                ChiSoTichLuyDienNangPhanKhangAQTongKenhNhan = ResolveChannel<object>(r => r.ChiSoTichLuyDienNangPhanKhangAQTongChieuNhan, snap?.ChiSoTichLuyDienNangPhanKhangAQTongChieuNhan, snap?.ChiSoTichLuyDienNangPhanKhangAQTongChieuNhanQuality, snap?.ChiSoTichLuyDienNangPhanKhangAQTongChieuNhanUpdatedAt, EnergySuspectedFlags.ChiSoTichLuyDienNangPhanKhangAQTongKenhNhan),
                SuspectedMask = mask,
                IsSuspected = mask != 0
            };

            if (dto.ChiSoTichLuyDienNangTacDungAPTongKenhGiao.HasValue ||
                dto.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao.HasValue ||
                dto.ChiSoTichLuyDienNangTacDungAPTongKenhNhan.HasValue)
            {
                result.Add(dto);
            }
        }

        return result;
    }

    public async Task<List<MeasurementPointOperatingReportDto>> GetListMeasurementOperatingReportWithLookbackAsync(
        int lookbackMinutes,
        CancellationToken cancellationToken,
        DateTimeOffset? targetTime = null)
    {
        if (lookbackMinutes <= 0) lookbackMinutes = 120;

        var target = targetTime ?? GetNearestHalfHourSlot(DateTimeOffset.UtcNow.AddMinutes(-5));
        var windowEnd = target.AddMinutes(5);
        var lookbackStart = target.AddMinutes(-Math.Max(lookbackMinutes, 60));

        var activePointIds = await _measurementPointsEntities
            .AsNoTracking()
            .Where(x => !x.Deleted && x.Status == EntityStatus.Active)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        if (activePointIds.Count == 0)
        {
            return new List<MeasurementPointOperatingReportDto>();
        }

        var minYm = MeteringValueHelper.ComputePartitionYm(lookbackStart);
        var maxYm = MeteringValueHelper.ComputePartitionYm(windowEnd);

        var tsRows = await _timeSeriesEntities
            .AsNoTracking()
            .Where(t => activePointIds.Contains(t.MeasurementPointId) &&
                        t.PartitionYm >= minYm &&
                        t.PartitionYm <= maxYm &&
                        t.RecordedAt >= lookbackStart &&
                        t.RecordedAt <= windowEnd &&
                        (t.DienApPhaA.HasValue || t.DongDienPhaA.HasValue || t.CongSuatTacDungTong3Pha.HasValue || t.TanSoHeThong.HasValue))
            .OrderBy(t => t.RecordedAt)
            .Select(t => new
            {
                t.MeasurementPointId,
                t.RecordedAt,
                t.DienApPhaA,
                t.DienApPhaB,
                t.DienApPhaC,
                t.DongDienPhaA,
                t.DongDienPhaB,
                t.DongDienPhaC,
                t.CongSuatTacDungTong3Pha,
                t.CongSuatPhanKhangTong3Pha,
                t.HeSoCongSuatPFTrungBinh3Pha,
                t.TanSoHeThong
            })
            .ToListAsync(cancellationToken);

        var tsByPoint = tsRows.GroupBy(r => r.MeasurementPointId).ToDictionary(g => g.Key, g => g.ToList());
        var snapshots = await _snapshotEntities.AsNoTracking()
            .Where(s => activePointIds.Contains(s.MeasurementPointId))
            .ToDictionaryAsync(s => s.MeasurementPointId, s => s, cancellationToken);

        var result = new List<MeasurementPointOperatingReportDto>();

        foreach (var pointId in activePointIds)
        {
            tsByPoint.TryGetValue(pointId, out var rows);
            snapshots.TryGetValue(pointId, out var snap);

            double? PickClosest(Func<dynamic, decimal?> selector, decimal? snapFallback, DateTimeOffset? snapAt)
            {
                if (rows != null)
                {
                    var match = rows.Where(r => selector(r).HasValue)
                        .OrderBy(r => Math.Abs((r.RecordedAt - target).TotalSeconds))
                        .FirstOrDefault();
                    if (match != null) return (double)selector(match)!.Value;
                }

                if (snapFallback.HasValue && snapAt.HasValue && snapAt.Value >= lookbackStart && snapAt.Value <= windowEnd)
                {
                    return (double)snapFallback.Value;
                }

                return null;
            }

            var p = PickClosest(r => r.CongSuatTacDungTong3Pha, snap?.CongSuatTacDungTong3Pha, snap?.CongSuatTacDungTong3PhaUpdatedAt);
            var q = PickClosest(r => r.CongSuatPhanKhangTong3Pha, snap?.CongSuatPhanKhangTong3Pha, snap?.CongSuatPhanKhangTong3PhaUpdatedAt);
            var pf = PickClosest(r => r.HeSoCongSuatPFTrungBinh3Pha, snap?.HeSoCongSuatPFTrungBinh3Pha, snap?.HeSoCongSuatPFTrungBinh3PhaUpdatedAt);
            if (!pf.HasValue && p.HasValue && q.HasValue)
            {
                var s = Math.Sqrt(p.Value * p.Value + q.Value * q.Value);
                if (s > 0) pf = Math.Round(Math.Abs(p.Value) / s, 3);
            }

            var ua = PickClosest(r => r.DienApPhaA, snap?.DienApPhaA, snap?.DienApPhaAUpdatedAt);
            var ia = PickClosest(r => r.DongDienPhaA, snap?.DongDienPhaA, snap?.DongDienPhaAUpdatedAt);

            if (!ua.HasValue && !ia.HasValue && !p.HasValue)
            {
                continue;
            }

            result.Add(new MeasurementPointOperatingReportDto
            {
                MeasurementPointId = pointId,
                CreatedTime = target,
                DienApPhaA = ua,
                DienApPhaB = PickClosest(r => r.DienApPhaB, snap?.DienApPhaB, snap?.DienApPhaBUpdatedAt),
                DienApPhaC = PickClosest(r => r.DienApPhaC, snap?.DienApPhaC, snap?.DienApPhaCUpdatedAt),
                DongDienPhaA = ia,
                DongDienPhaB = PickClosest(r => r.DongDienPhaB, snap?.DongDienPhaB, snap?.DongDienPhaBUpdatedAt),
                DongDienPhaC = PickClosest(r => r.DongDienPhaC, snap?.DongDienPhaC, snap?.DongDienPhaCUpdatedAt),
                CongSuatTacDungTong3Pha = p,
                CongSuatPhanKhangTong3Pha = q,
                HeSoCongSuatPF = pf,
                TanSoHeThong = PickClosest(r => r.TanSoHeThong, snap?.TanSoHeThong, snap?.TanSoHeThongUpdatedAt) ?? 50.0
            });
        }

        return result;
    }

    private static DateTimeOffset GetNearestHalfHourSlot(DateTimeOffset time)
    {
        var minute = time.Minute;
        var roundedMinute = minute < 15 ? 0 : (minute < 45 ? 30 : 0);
        var baseTime = minute >= 45 ? time.AddHours(1) : time;
        return new DateTimeOffset(baseTime.Year, baseTime.Month, baseTime.Day, baseTime.Hour, roundedMinute, 0, baseTime.Offset);
    }
}
