using System.Globalization;
using Application.Common.ValueObjects.CursorBased;
using Application.DataTransferObjects.Requests.Gcs;
using Application.DataTransferObjects.Requests.IoT;
using Application.DataTransferObjects.Responses.Gcs;
using Application.DataTransferObjects.Responses.IoT;
using Application.Extensions;
using Application.Repositories.ReadWrite;
using AutoMapper;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Nobisoft.Core.Database.DbContext;
using Nobisoft.Core.Database.Repository;
using Nobisoft.Core.Interfaces;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace Infrastructure.Repositories.ReadWrite
{
    public class GcsTemporaryReadWriteRepository : RestfulRepository<GcsTemporaryEntity, AppReadWriteDbContext>, IGcsTemporaryReadWriteRepository
    {
        private static bool _columnsChecked = false;
        private static readonly SemaphoreSlim _checkLock = new SemaphoreSlim(1, 1);
        private readonly DbSet<GcsTemporaryEntity> _gcsTemporaryEntities;
        private readonly IMapper _mapper;

        public GcsTemporaryReadWriteRepository(AppReadWriteDbContext applicationDbContext, ISnowflakeIdService snowflakeIdService, IMapper mapper) : base(applicationDbContext, snowflakeIdService)
        {
            _mapper = mapper;
            _gcsTemporaryEntities = applicationDbContext.Set<GcsTemporaryEntity>();
        }

        public static double? CalculateAverageCosPhi(double? ap, double? aq)
        {
            if (!ap.HasValue && !aq.HasValue) return null;
            var p = ap ?? 0;
            var q = Math.Max(0, aq ?? 0);
            if (p < 0) return null;
            if (p == 0 && q == 0) return ap.HasValue ? 0.0 : null;
            var s = Math.Sqrt((p * p) + (q * q));
            if (s <= 0) return 0.0;
            return Math.Round(p / s, 2, MidpointRounding.AwayFromZero);
        }

        public static void RecalculatePointConsumptionAndCosPhi(IEnumerable<GcsTemporaryEntity> pointRecords)
        {
            var list = pointRecords.ToList();
            if (list.Count == 0) return;

            foreach (var rec in list)
            {
                if (string.Equals(rec.MeterReadingSet, "Pmax", StringComparison.OrdinalIgnoreCase))
                {
                    rec.Consumption = null;
                }
                else
                {
                    var effectiveCurr = rec.NewCurrentReading ?? rec.CurrentReading;
                    var effectivePrev = rec.NewPreviousReading ?? rec.PreviousReading ?? 0;
                    if (effectiveCurr.HasValue)
                    {
                        rec.Consumption = Math.Round((effectiveCurr.Value - effectivePrev) * rec.MultiplierFactor, 2, MidpointRounding.AwayFromZero);
                    }
                    else
                    {
                        rec.Consumption = null;
                    }
                }
            }

            var sgRec = list.FirstOrDefault(x => string.Equals(x.MeterReadingSet, "SG", StringComparison.OrdinalIgnoreCase));
            var vcRec = list.FirstOrDefault(x => string.Equals(x.MeterReadingSet, "VC", StringComparison.OrdinalIgnoreCase));
            var btRec = list.FirstOrDefault(x => string.Equals(x.MeterReadingSet, "BT", StringComparison.OrdinalIgnoreCase));
            var cdRec = list.FirstOrDefault(x => string.Equals(x.MeterReadingSet, "CD", StringComparison.OrdinalIgnoreCase));
            var tdRec = list.FirstOrDefault(x => string.Equals(x.MeterReadingSet, "TD", StringComparison.OrdinalIgnoreCase));

            double? ap = sgRec?.Consumption;
            if ((!ap.HasValue || ap.Value <= 0) && (btRec?.Consumption != null || cdRec?.Consumption != null || tdRec?.Consumption != null))
            {
                var sumTariff = (btRec?.Consumption ?? 0) + (cdRec?.Consumption ?? 0) + (tdRec?.Consumption ?? 0);
                if (sumTariff > 0 || !ap.HasValue)
                {
                    ap = sumTariff;
                }
            }

            double? aq = vcRec?.Consumption;
            var cosPhi = CalculateAverageCosPhi(ap, aq);

            foreach (var rec in list)
            {
                rec.CosPhi = cosPhi;
            }
        }

        public async Task EnsureColumnsExistAsync(CancellationToken cancellationToken = default)
        {
            if (_columnsChecked) return;
            await _checkLock.WaitAsync(cancellationToken);
            try
            {
                if (_columnsChecked) return;
                var alterStatements = new[]
                {
                    "ALTER TABLE `GcsTemporary` ADD COLUMN `NewPreviousReading` double DEFAULT NULL;",
                    "ALTER TABLE `GcsTemporary` ADD COLUMN `IsSuspicious` tinyint(1) NOT NULL DEFAULT '0';",
                    "ALTER TABLE `GcsTemporary` ADD COLUMN `SuspiciousReason` varchar(500) DEFAULT NULL;",
                    "ALTER TABLE `GcsTemporary` ADD COLUMN `CosPhi` double DEFAULT NULL;",
                    "ALTER TABLE `GcsTemporary` ADD COLUMN `PmaxTime` datetime(6) DEFAULT NULL;",
                    "ALTER TABLE `GcsTemporary` ADD COLUMN `IsChange` tinyint(1) NOT NULL DEFAULT '0';",
                    "ALTER TABLE `gcstemporary` ADD COLUMN `NewPreviousReading` double DEFAULT NULL;",
                    "ALTER TABLE `gcstemporary` ADD COLUMN `IsSuspicious` tinyint(1) NOT NULL DEFAULT '0';",
                    "ALTER TABLE `gcstemporary` ADD COLUMN `SuspiciousReason` varchar(500) DEFAULT NULL;",
                    "ALTER TABLE `gcstemporary` ADD COLUMN `CosPhi` double DEFAULT NULL;",
                    "ALTER TABLE `gcstemporary` ADD COLUMN `PmaxTime` datetime(6) DEFAULT NULL;",
                    "ALTER TABLE `gcstemporary` ADD COLUMN `IsChange` tinyint(1) NOT NULL DEFAULT '0';",
                    "ALTER TABLE `GcsMonthlyDetail` ADD COLUMN `CosPhi` double DEFAULT NULL;",
                    "ALTER TABLE `GcsMonthlyDetail` ADD COLUMN `ReadingTime` datetime(6) DEFAULT NULL;",
                    "ALTER TABLE `GcsMonthlyDetail` ADD COLUMN `PmaxTime` datetime(6) DEFAULT NULL;",
                    "ALTER TABLE `gcsmonthlydetail` ADD COLUMN `CosPhi` double DEFAULT NULL;",
                    "ALTER TABLE `gcsmonthlydetail` ADD COLUMN `ReadingTime` datetime(6) DEFAULT NULL;",
                    "ALTER TABLE `gcsmonthlydetail` ADD COLUMN `PmaxTime` datetime(6) DEFAULT NULL;",
                    "ALTER TABLE `PeriodicEnergyStatement` ADD COLUMN `CosPhi` double DEFAULT NULL;",
                    "ALTER TABLE `PeriodicEnergyStatement` ADD COLUMN `PmaxTime` datetime(6) DEFAULT NULL;",
                    "ALTER TABLE `PeriodicEnergyStatement` ADD COLUMN `PreviousPmaxTime` datetime(6) DEFAULT NULL;",
                    "ALTER TABLE `PeriodicEnergyStatement` ADD COLUMN `BillingTimestamp` datetime(6) DEFAULT NULL;",
                    "ALTER TABLE `periodicenergystatement` ADD COLUMN `CosPhi` double DEFAULT NULL;",
                    "ALTER TABLE `periodicenergystatement` ADD COLUMN `PmaxTime` datetime(6) DEFAULT NULL;",
                    "ALTER TABLE `periodicenergystatement` ADD COLUMN `PreviousPmaxTime` datetime(6) DEFAULT NULL;",
                    "ALTER TABLE `periodicenergystatement` ADD COLUMN `BillingTimestamp` datetime(6) DEFAULT NULL;"
                };

                foreach (var sql in alterStatements)
                {
                    try
                    {
                        await DbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
                    }
                    catch
                    {
                        // Column may already exist or table name case does not match, ignore safely
                    }
                }
                _columnsChecked = true;
            }
            finally
            {
                _checkLock.Release();
            }
        }

        private async Task AutoHealFalselyFlaggedSuspiciousRecordsAsync(long gcsId, int month, int year, int periodNumber, CancellationToken cancellationToken)
        {
            try
            {
                var allCycleRecords = await _gcsTemporaryEntities
                    .Where(x => x.GcsId == gcsId && x.Month == month && x.Year == year && x.PeriodNumber == periodNumber)
                    .ToListAsync(cancellationToken);

                bool anyChanged = false;
                foreach (var item in allCycleRecords)
                {
                    if (item.IsSuspicious && item.SuspiciousReason != null && item.SuspiciousReason.Contains("lệch so với kỳ chốt (> 1 ngày)"))
                    {
                        item.IsSuspicious = false;
                        item.SuspiciousReason = null;
                        anyChanged = true;
                    }
                }

                // Đảm bảo các điểm đo đã có Consumption nhưng chưa tính CosPhi được cập nhật đầy đủ
                foreach (var group in allCycleRecords.GroupBy(x => x.MeasurementPointId))
                {
                    var sg = group.FirstOrDefault(x => x.MeterReadingSet == "SG");
                    if (sg != null && sg.Consumption.HasValue && !sg.CosPhi.HasValue)
                    {
                        RecalculatePointConsumptionAndCosPhi(group);
                        anyChanged = true;
                    }
                }

                if (anyChanged)
                {
                    await SaveChangesAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AutoHeal] Warning: {ex.Message}");
            }
        }

        public async Task<CursorPaginationResponse<GetDataLatchResponse>> GetListGcsTemporaryWithPaginationAsync(GetDataLatchRequest request, CancellationToken cancellationToken)
        {
            await EnsureColumnsExistAsync(cancellationToken);
            await AutoHealFalselyFlaggedSuspiciousRecordsAsync(request.GcsId, request.Month, request.Year, request.PeriodNumber, cancellationToken);
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

            if (request.IsSuspicious.HasValue)
            {
                if (request.IsSuspicious.Value)
                {
                    gcsTemporary = gcsTemporary.Where(x => x.IsSuspicious && x.NewCurrentReading == null && x.NewPreviousReading == null);
                }
                else
                {
                    gcsTemporary = gcsTemporary.Where(x => !x.IsSuspicious || x.NewCurrentReading != null || x.NewPreviousReading != null);
                }
            }

            var baseCycleScope = _gcsTemporaryEntities.AsNoTracking()
                .Where(x => x.GcsId == request.GcsId && x.Month == request.Month && x.Year == request.Year && x.PeriodNumber == request.PeriodNumber);

            var totalMeasurementPoints = await baseCycleScope.Select(x => x.MeasurementPointId).Distinct().CountAsync(cancellationToken);
            var totalSuspiciousCount = await baseCycleScope.CountAsync(x => x.IsSuspicious && x.NewCurrentReading == null && x.NewPreviousReading == null, cancellationToken);

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
                TotalCount = result.TotalCount,
                TotalMeasurementPoints = totalMeasurementPoints,
                TotalSuspiciousCount = totalSuspiciousCount,
            };
        }

        public async Task<List<GcsTemporaryEntity>> GetListGcsTemporaryAsync(GetDataLatchRequest request, CancellationToken cancellationToken)
        {
            await EnsureColumnsExistAsync(cancellationToken);
            return await _gcsTemporaryEntities
                .Where(x => x.GcsId == request.GcsId && x.PeriodNumber == request.PeriodNumber && x.Month == request.Month && x.Year == request.Year)
                .OrderByDescending(x => x.Id)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<GcsTemporaryEntity>> GetListGcsTemporaryToUpdateAsync(List<UpdateGcsTemporaryRequest> request, long gcsId, int month, int year, int periodNumber, CancellationToken cancellationToken)
        {
            await EnsureColumnsExistAsync(cancellationToken);
            var requestIds = request.Select(r => r.Id).ToList();
            return await _gcsTemporaryEntities
                .Where(x => requestIds.Contains(x.Id) && x.GcsId == gcsId && x.Month == month && x.Year == year && x.PeriodNumber == periodNumber)
                .Include(x => x.MeasurementPoint)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<GcsTemporaryEntity>> GetListGcsTemporaryWithGcsAsync(long gcsId, int month, int year, int periodNumber, CancellationToken cancellationToken)
        {
            await EnsureColumnsExistAsync(cancellationToken);
            return await _gcsTemporaryEntities
                .Where(x => x.GcsId == gcsId && x.PeriodNumber == periodNumber && x.Month == month && x.Year == year)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> CheckExistGcsTemporaryAsync(long gcsId, int month, int year, int periodNumber, CancellationToken cancellationToken)
        {
            await EnsureColumnsExistAsync(cancellationToken);
            await AutoHealFalselyFlaggedSuspiciousRecordsAsync(gcsId, month, year, periodNumber, cancellationToken);
            return await _gcsTemporaryEntities
                .AsNoTracking()
                .Where(x => x.GcsId == gcsId && x.Month == month && x.Year == year && x.PeriodNumber == periodNumber)
                .AnyAsync(x => (x.Quanlity == null && x.NewCurrentReading == null) 
                            || (x.Quanlity == false && x.NewCurrentReading == null)
                            || (x.IsSuspicious && x.NewCurrentReading == null && x.NewPreviousReading == null)
                            || (x.CurrentReading == null && x.NewCurrentReading == null), cancellationToken);
        }

        public async Task<byte[]> ExportGcsTemporaryExcelAsync(long gcsId, int month, int year, int periodNumber, bool? onlySuspicious, CancellationToken cancellationToken)
        {
            await EnsureColumnsExistAsync(cancellationToken);
            await AutoHealFalselyFlaggedSuspiciousRecordsAsync(gcsId, month, year, periodNumber, cancellationToken);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var query = _gcsTemporaryEntities.AsNoTracking()
                .Where(x => x.GcsId == gcsId && x.Month == month && x.Year == year && x.PeriodNumber == periodNumber)
                .Include(x => x.MeasurementPoint)
                .ThenInclude(x => x.Customer)
                .OrderBy(x => x.MeasurementPoint.CreatedTime)
                .ThenBy(x => x.MeasurementPointId)
                .ThenBy(x => x.Id);

            var items = await query.ToListAsync(cancellationToken);
            if (onlySuspicious == true)
            {
                items = items.Where(x => x.IsSuspicious && x.NewCurrentReading == null && x.NewPreviousReading == null).ToList();
            }

            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Chi_So_GCS");
            ws.View.ShowGridLines = true;

            // Title
            ws.Cells["A1:E1"].Merge = true;
            ws.Cells["A1"].Value = "HỆ THỐNG QUẢN LÝ DỮ LIỆU ĐO ĐẾM CÔNG TƠ ĐIỆN TỬ (MDMS / AMR)";
            ws.Cells["A1"].Style.Font.Italic = true;
            ws.Cells["A1"].Style.Font.Size = 10;
            ws.Cells["A1"].Style.Font.Color.SetColor(System.Drawing.Color.FromArgb(100, 100, 100));
            ws.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            ws.Cells["A2:E2"].Merge = true;
            ws.Cells["A2"].Value = "BẢNG KÊ CHỈ SỐ GHI CÔNG TƠ";
            ws.Cells["A2"].Style.Font.Bold = true;
            ws.Cells["A2"].Style.Font.Size = 14;
            ws.Cells["A2"].Style.Font.Color.SetColor(System.Drawing.Color.FromArgb(180, 40, 40));
            ws.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            ws.Cells["A3:E3"].Merge = true;
            ws.Cells["A3"].Value = $"Kỳ {periodNumber} - Tháng {month:D2}/{year} (Tổng cộng: {items.Count} bản ghi)";
            ws.Cells["A3"].Style.Font.Italic = true;
            ws.Cells["A3"].Style.Font.Size = 11;
            ws.Cells["A3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            // Instructions
            ws.Cells["A4:E4"].Merge = true;
            ws.Cells["A4"].Value = "Hướng dẫn: 1. Có thể sửa Chỉ số cũ (Cột D) và Chỉ số mới (Cột E). 2. Ô màu nền hồng nhạt là số liệu nghi ngờ / chưa cập nhật (chữ số vẫn là số chuẩn). 3. Sau khi kiểm tra/sửa tay, chuyển màu nền ô về không màu/màu trắng (Normal) để xác nhận. 4. Dùng tính năng 'Filter by Color' trong Excel để lọc nhanh các ô màu hồng.";
            ws.Cells["A4"].Style.Font.Italic = true;
            ws.Cells["A4"].Style.Font.Size = 9;
            ws.Cells["A4"].Style.Font.Color.SetColor(System.Drawing.Color.FromArgb(30, 64, 175));
            ws.Cells["A4"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

            // Headers on row 5 (5 columns) - uniform style across all headers
            var headers = new[]
            {
                "Mã điểm đo",
                "Tên khách hàng",
                "Bộ chỉ số",
                "Chỉ số cũ",
                "Chỉ số mới"
            };

            for (int col = 1; col <= headers.Length; col++)
            {
                var cell = ws.Cells[5, col];
                cell.Value = headers[col - 1];
                cell.Style.Font.Bold = true;
                cell.Style.Font.Color.SetColor(System.Drawing.Color.White);
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(30, 58, 138)); // Dark blue uniform
                cell.Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.FromArgb(200, 200, 200));
            }
            ws.Row(5).Height = 28;
            ws.Cells["A5:E5"].AutoFilter = true;

            int currentRow = 6;
            for (int i = 0; i < items.Count; i++)
            {
                var it = items[i];
                bool hasUserConfirmed = it.NewCurrentReading != null || it.NewPreviousReading != null;
                bool isSuspicious = it.IsSuspicious && !hasUserConfirmed;

                // Col 1: Mã điểm đo
                ws.Cells[currentRow, 1].Value = it.MeasurementPoint?.Code ?? "";
                // Col 2: Tên khách hàng
                ws.Cells[currentRow, 2].Value = it.MeasurementPoint?.Customer?.FullName ?? "";
                // Col 3: Bộ chỉ số
                ws.Cells[currentRow, 3].Value = it.MeterReadingSet;
                ws.Cells[currentRow, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // Col 4: Chỉ số cũ (làm tròn đến hàng đơn vị, không hiển thị số thập phân)
                var oldVal = it.NewPreviousReading ?? it.PreviousReading;
                if (oldVal.HasValue)
                {
                    ws.Cells[currentRow, 4].Value = Math.Round(oldVal.Value, 0);
                    ws.Cells[currentRow, 4].Style.Numberformat.Format = "#,##0";
                }
                else
                {
                    ws.Cells[currentRow, 4].Value = "-";
                    ws.Cells[currentRow, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                // Col 5: Chỉ số mới (số liệu nghi ngờ: giá trị số chuẩn, font chữ bình thường, nền hồng nhạt #FFC7CE)
                var newCell = ws.Cells[currentRow, 5];
                if (it.NewCurrentReading != null)
                {
                    newCell.Value = Math.Round(it.NewCurrentReading.Value, 0);
                    newCell.Style.Numberformat.Format = "#,##0";
                    // Confirmed: Normal style (white background)
                }
                else if (!it.CurrentReading.HasValue)
                {
                    newCell.Value = "-";
                    newCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    newCell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    newCell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(255, 199, 206)); // Light pink #FFC7CE
                    if (newCell.Comment == null)
                    {
                        newCell.AddComment("Chưa có chỉ số chốt từ công tơ (chưa từng được cập nhật)", "MDMS");
                    }
                }
                else if (isSuspicious)
                {
                    newCell.Value = Math.Round(it.CurrentReading.Value, 0);
                    newCell.Style.Numberformat.Format = "#,##0";
                    newCell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    newCell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(255, 199, 206)); // Light pink #FFC7CE
                    if (newCell.Comment == null)
                    {
                        newCell.AddComment($"Số liệu nghi ngờ: {it.SuspiciousReason}", "MDMS");
                    }
                }
                else
                {
                    newCell.Value = Math.Round(it.CurrentReading.Value, 0);
                    newCell.Style.Numberformat.Format = "#,##0";
                    // Valid reading: Normal style (white background)
                }

                for (int c = 1; c <= 5; c++)
                {
                    ws.Cells[currentRow, c].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    ws.Cells[currentRow, c].Style.Border.Bottom.Color.SetColor(System.Drawing.Color.FromArgb(229, 231, 235));
                }

                currentRow++;
            }

            ws.Cells.AutoFitColumns();
            ws.Column(1).Width = 24;
            ws.Column(2).Width = 40;
            ws.Column(3).Width = 14;
            ws.Column(4).Width = 22;
            ws.Column(5).Width = 22;

            return await package.GetAsByteArrayAsync(cancellationToken);
        }

        public async Task<ImportGcsTemporaryExcelResponse> ImportGcsTemporaryExcelAsync(Stream fileStream, long gcsId, int month, int year, int periodNumber, CancellationToken cancellationToken)
        {
            await EnsureColumnsExistAsync(cancellationToken);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var response = new ImportGcsTemporaryExcelResponse();

            using var package = new ExcelPackage(fileStream);
            var ws = package.Workbook.Worksheets.FirstOrDefault();
            if (ws == null || ws.Dimension == null)
            {
                response.Errors.Add("File Excel rỗng hoặc không đúng định dạng.");
                response.Message = "File Excel không có dữ liệu.";
                return response;
            }

            var endRow = ws.Dimension.End.Row;
            var endCol = ws.Dimension.End.Column;
            if (endRow < 6)
            {
                response.Errors.Add("Không tìm thấy dòng dữ liệu trong file Excel (bắt đầu từ dòng 6).");
                response.Message = "Không có dòng dữ liệu nào để xử lý.";
                return response;
            }

            // Determine template format:
            // 5-column format: Col 1: Mã điểm đo, Col 3: Bộ chỉ số, Col 4: Chỉ số cũ, Col 5: Chỉ số mới
            // 15-column format: Col 1: ID, Col 3: Mã điểm đo, Col 6: Bộ chỉ số, Col 9: Chỉ số cũ, Col 11: Chỉ số mới
            bool isFiveColFormat = endCol <= 8;
            int colPointCode = isFiveColFormat ? 1 : 3;
            int colReadingSet = isFiveColFormat ? 3 : 6;
            int colOld = isFiveColFormat ? 4 : (endCol >= 15 ? 9 : 8);
            int colNew = isFiveColFormat ? 5 : (endCol >= 15 ? 11 : 10);
            int colId = isFiveColFormat ? -1 : 1;

            // Load existing temporary records for this cycle into memory
            var existingRecords = await _gcsTemporaryEntities
                .Where(x => x.GcsId == gcsId && x.Month == month && x.Year == year && x.PeriodNumber == periodNumber)
                .Include(x => x.MeasurementPoint)
                .ToListAsync(cancellationToken);

            var recordDictById = existingRecords.ToDictionary(x => x.Id);

            int updatedCount = 0;
            int skippedCount = 0;

            static (bool isValid, double? val, bool hasExclamation, string? error) ValidateAndParseCell(
                object? cellObj, string colName, int row, string? pointCode, string? readingSet)
            {
                if (cellObj == null) return (true, null, false, null);
                if (cellObj is double d)
                {
                    if (d < 0) return (false, null, false, $"Dòng {row}: Ô '{colName}' ({pointCode} - {readingSet}) có giá trị âm ({d}). Chỉ số công tơ không thể là số âm.");
                    return (true, d, false, null);
                }
                if (cellObj is int i)
                {
                    if (i < 0) return (false, null, false, $"Dòng {row}: Ô '{colName}' ({pointCode} - {readingSet}) có giá trị âm ({i}). Chỉ số công tơ không thể là số âm.");
                    return (true, (double)i, false, null);
                }
                if (cellObj is long l)
                {
                    if (l < 0) return (false, null, false, $"Dòng {row}: Ô '{colName}' ({pointCode} - {readingSet}) có giá trị âm ({l}). Chỉ số công tơ không thể là số âm.");
                    return (true, (double)l, false, null);
                }
                if (cellObj is decimal dec)
                {
                    if (dec < 0) return (false, null, false, $"Dòng {row}: Ô '{colName}' ({pointCode} - {readingSet}) có giá trị âm ({dec}). Chỉ số công tơ không thể là số âm.");
                    return (true, (double)dec, false, null);
                }
                if (cellObj is float f)
                {
                    if (f < 0) return (false, null, false, $"Dòng {row}: Ô '{colName}' ({pointCode} - {readingSet}) có giá trị âm ({f}). Chỉ số công tơ không thể là số âm.");
                    return (true, (double)f, false, null);
                }

                var str = cellObj.ToString()?.Trim() ?? "";
                if (str == "-" || str == "-*" || string.IsNullOrWhiteSpace(str))
                {
                    return (true, null, false, null);
                }

                bool hasEx = str.Contains("!");
                var clean = str.Replace("'", "").Replace("!", "").Trim().Replace(",", ".");
                if (clean.Contains(".") && clean.Count(c => c == '.') > 1)
                {
                    clean = clean.Replace(".", "");
                }

                if (!double.TryParse(clean, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
                {
                    return (false, null, false, $"Dòng {row}: Ô '{colName}' ({pointCode} - {readingSet}) chứa giá trị không hợp lệ '{str}'. Giá trị phải là số thực hoặc số nguyên.");
                }

                if (parsed < 0)
                {
                    return (false, null, false, $"Dòng {row}: Ô '{colName}' ({pointCode} - {readingSet}) có giá trị âm ({parsed}). Chỉ số công tơ không thể là số âm.");
                }

                return (true, parsed, hasEx, null);
            }

            static bool IsCellBadStyle(ExcelRange cell)
            {
                if (cell.Style.Fill.PatternType == ExcelFillStyle.None) return false;
                var rgb = cell.Style.Fill.BackgroundColor?.Rgb;
                if (string.IsNullOrEmpty(rgb)) return false;
                return rgb.EndsWith("FFC7CE", StringComparison.OrdinalIgnoreCase);
            }

            for (int row = 6; row <= endRow; row++)
            {
                var pointCode = ws.Cells[row, colPointCode].Value?.ToString()?.Trim();
                var readingSet = ws.Cells[row, colReadingSet].Value?.ToString()?.Trim();
                if (string.IsNullOrWhiteSpace(pointCode) && string.IsNullOrWhiteSpace(readingSet))
                {
                    continue;
                }

                response.TotalRows++;

                List<GcsTemporaryEntity> matchedEntities = new();

                if (colId > 0)
                {
                    var idObj = ws.Cells[row, colId].Value;
                    if (idObj != null && long.TryParse(idObj.ToString()?.Trim(), out var id) && recordDictById.TryGetValue(id, out var byId))
                    {
                        matchedEntities.Add(byId);
                    }
                }

                if (matchedEntities.Count == 0)
                {
                    matchedEntities = existingRecords
                        .Where(x => string.Equals(x.MeasurementPoint?.Code?.Trim(), pointCode, StringComparison.OrdinalIgnoreCase)
                                 && string.Equals(x.MeterReadingSet?.Trim(), readingSet, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                if (matchedEntities.Count == 0)
                {
                    response.Errors.Add($"Dòng {row}: Không tìm thấy bản ghi chỉ số ({pointCode} - {readingSet}) tương ứng trong hệ thống.");
                    skippedCount++;
                    continue;
                }

                var cellOld = ws.Cells[row, colOld];
                var cellNew = ws.Cells[row, colNew];
                var (oldValid, parsedOld, oldEx, oldErr) = ValidateAndParseCell(cellOld.Value, "Chỉ số cũ", row, pointCode, readingSet);
                var (newValid, parsedNew, newEx, newErr) = ValidateAndParseCell(cellNew.Value, "Chỉ số mới", row, pointCode, readingSet);

                if (!oldValid || !newValid)
                {
                    if (!oldValid && !string.IsNullOrEmpty(oldErr)) response.Errors.Add(oldErr);
                    if (!newValid && !string.IsNullOrEmpty(newErr)) response.Errors.Add(newErr);
                    skippedCount++;
                    continue;
                }

                bool isOldBad = IsCellBadStyle(cellOld);
                bool isNewBad = IsCellBadStyle(cellNew);

                // Check if user set cell style back to "Normal" (cleared the Bad color #FFC7CE)
                bool oldColorNormal = !isOldBad;
                bool newColorNormal = !isNewBad;

                bool rowUpdated = false;

                foreach (var entity in matchedEntities)
                {
                    bool isRecordUpdated = false;

                    // 1. Xử lý Chỉ số cũ:
                    if (parsedOld.HasValue)
                    {
                        var originalPrev = entity.PreviousReading;
                        bool isDiff = !originalPrev.HasValue || Math.Abs(parsedOld.Value - originalPrev.Value) > 0.0001;
                        if (isDiff || oldEx)
                        {
                            entity.NewPreviousReading = parsedOld.Value;
                            entity.IsChange = oldEx;
                            entity.ModifiedTime = DateTimeOffset.Now;
                            isRecordUpdated = true;
                        }
                    }

                    // 2. Xử lý Chỉ số mới:
                    // Trường hợp 2.1: Bản ghi vốn bình thường (như Kyocera & Zeon) và giá trị trong Excel không đổi:
                    // entity.IsSuspicious == false, và parsedNew bằng đúng entity.CurrentReading:
                    // -> TUYỆT ĐỐI KHÔNG làm gì, giữ nguyên để không bị gán cờ IsChange = true!
                    bool isNormalUnchanged = !entity.IsSuspicious && entity.Quanlity == true && entity.CurrentReading.HasValue
                        && parsedNew.HasValue && Math.Abs(parsedNew.Value - entity.CurrentReading.Value) <= 0.0001;

                    if (!isNormalUnchanged)
                    {
                        // Trường hợp 2.2: Người dùng sửa/cập nhật giá trị mới:
                        if (parsedNew.HasValue && (!entity.CurrentReading.HasValue || Math.Abs(parsedNew.Value - entity.CurrentReading.Value) > 0.0001 || newEx))
                        {
                            entity.NewCurrentReading = parsedNew.Value;
                            entity.Quanlity = true;
                            entity.IsSuspicious = false;
                            entity.SuspiciousReason = "Đã xác nhận từ file Excel";
                            entity.IsChange = newEx;
                            entity.ModifiedTime = DateTimeOffset.Now;
                            isRecordUpdated = true;
                        }
                        // Trường hợp 2.3: Bản ghi bị nghi ngờ / chưa xác nhận, người dùng xác nhận số liệu công tơ hợp lệ (bằng số liệu trong Excel hoặc đổi màu nền sang Normal):
                        else if (entity.IsSuspicious && (newColorNormal || parsedNew.HasValue) && (entity.CurrentReading.HasValue || parsedNew.HasValue))
                        {
                            entity.Quanlity = true;
                            entity.IsSuspicious = false;
                            entity.SuspiciousReason = "Đã xác nhận chỉ số công tơ hợp lệ";
                            entity.IsChange = false;
                            // Không gán NewCurrentReading nếu bằng CurrentReading để tránh coi là sửa tay
                            if (parsedNew.HasValue && entity.CurrentReading.HasValue && Math.Abs(parsedNew.Value - entity.CurrentReading.Value) <= 0.0001)
                            {
                                entity.NewCurrentReading = null;
                            }
                            entity.ModifiedTime = DateTimeOffset.Now;
                            isRecordUpdated = true;
                        }
                    }

                    if (isRecordUpdated)
                    {
                        rowUpdated = true;
                    }
                }

                if (rowUpdated)
                {
                    updatedCount++;
                }
                else
                {
                    skippedCount++;
                }
            }

            // Tính toán lại Sản lượng (AP, AQ, BT, CD, TD) và Hệ số Cosphi (PF) trung bình cho toàn bộ các điểm đo trong sổ
            foreach (var pointGroup in existingRecords.GroupBy(x => x.MeasurementPointId))
            {
                RecalculatePointConsumptionAndCosPhi(pointGroup);
            }

            if (updatedCount > 0 || existingRecords.Count > 0)
            {
                await SaveChangesAsync(cancellationToken);
            }

            response.UpdatedCount = updatedCount;
            response.SkippedCount = skippedCount;
            response.Message = $"Đã xử lý {response.TotalRows} dòng: cập nhật & tính toán lại Sản lượng, Cosphi (PF) thành công cho {updatedCount} bản ghi, giữ nguyên {skippedCount} bản ghi.";
            return response;
        }
    }
}
