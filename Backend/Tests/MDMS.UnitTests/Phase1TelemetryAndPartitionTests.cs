using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using Application.DataTransferObjects.Responses.Report;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Domain.Helpers;
using Infrastructure.Helpers;
using Infrastructure.MappingProfiles;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nobisoft.Core.Constants;

namespace MDMS.UnitTests;

/// <summary>
/// Phase 1 Verification Suite: Covers numeric telemetry parsing, SuspectedMask bit-packing,
/// vi-VN energy report formatting, monthly RANGE partition boundary & 12-month retention math,
/// compile-time TelemetryMetadataRegistry consistency, Snapshot DTO projections, and AutoMapper profiles.
/// </summary>
[TestClass]
public class Phase1TelemetryAndPartitionTests
{
    [TestMethod]
    [DataRow("1234.56", "1234.56", false)]
    [DataRow("1234.56789", "1234.5679", false)]
    [DataRow("1234.56*", "1234.56", true)]
    [DataRow("9876.54 (Not Topical)", "9876.54", true)]
    [DataRow("1234,5", "1234.5", false)]
    [DataRow("1.234,56*", "1234.56", true)]
    [DataRow("1,234.56*", "1234.56", true)]
    [DataRow("1.234.567", "1234567", false)]
    [DataRow("(Not Topical)", null, true)]
    [DataRow("N/A", null, false)]
    [DataRow("", null, false)]
    public void ParseTelemetryDecimal_HandlesRawSuspectedAndCultureFormats(
        string? raw,
        string? expectedDecimalStr,
        bool expectedSuspected)
    {
        var (value, isSuspected) = MeteringValueHelper.ParseTelemetryDecimal(raw);
        decimal? expected = expectedDecimalStr != null
            ? decimal.Parse(expectedDecimalStr, CultureInfo.InvariantCulture)
            : null;

        Assert.AreEqual(expected, value, $"Parsed decimal mismatch for input '{raw}'");
        Assert.AreEqual(expectedSuspected, isSuspected, $"Suspected flag mismatch for input '{raw}'");
    }

    [TestMethod]
    public void SuspectedBitmask_PacksUnpacksAndFormatsEnergyValuesPreservingViVnPresentation()
    {
        var mask = 0;
        mask = MeteringValueHelper.SetSuspectedFlag(mask, EnergySuspectedFlags.ChiSoTichLuyDienNangTacDungAPTongKenhGiao, true);
        mask = MeteringValueHelper.SetSuspectedFlag(mask, EnergySuspectedFlags.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao, true);
        mask = MeteringValueHelper.SetSuspectedFlag(mask, EnergySuspectedFlags.ChiSoTichLuyDienNangPhanKhangAQTongKenhNhan, true);

        Assert.IsTrue(MeteringValueHelper.HasSuspectedFlag(mask, EnergySuspectedFlags.ChiSoTichLuyDienNangTacDungAPTongKenhGiao));
        Assert.IsTrue(MeteringValueHelper.HasSuspectedFlag(mask, EnergySuspectedFlags.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao));
        Assert.IsTrue(MeteringValueHelper.HasSuspectedFlag(mask, EnergySuspectedFlags.ChiSoTichLuyDienNangPhanKhangAQTongKenhNhan));
        Assert.IsFalse(MeteringValueHelper.HasSuspectedFlag(mask, EnergySuspectedFlags.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao));

        // Clear one bit without affecting remaining flags
        mask = MeteringValueHelper.SetSuspectedFlag(mask, EnergySuspectedFlags.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao, false);
        Assert.IsFalse(MeteringValueHelper.HasSuspectedFlag(mask, EnergySuspectedFlags.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao));
        Assert.IsTrue(MeteringValueHelper.HasSuspectedFlag(mask, EnergySuspectedFlags.ChiSoTichLuyDienNangTacDungAPTongKenhGiao));
        Assert.IsTrue(MeteringValueHelper.HasSuspectedFlag(mask, EnergySuspectedFlags.ChiSoTichLuyDienNangPhanKhangAQTongKenhNhan));

        // Verify exact vi-VN N0 / N2 formatting and '*' indicator preservation
        var formattedCleanInt = MeteringValueHelper.FormatEnergyReportValue(
            1234m,
            mask,
            EnergySuspectedFlags.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao);
        var formattedSuspectedDec = MeteringValueHelper.FormatEnergyReportValue(
            1234.5678m,
            mask,
            EnergySuspectedFlags.ChiSoTichLuyDienNangTacDungAPTongKenhGiao);
        var formattedNull = MeteringValueHelper.FormatEnergyReportValue(
            null,
            mask,
            EnergySuspectedFlags.ChiSoTichLuyDienNangTacDungAPTongKenhGiao,
            "fallback");

        Assert.AreEqual("1.234", formattedCleanInt);
        Assert.AreEqual("1.234,57*", formattedSuspectedDec);
        Assert.AreEqual("fallback", formattedNull);
    }

    [TestMethod]
    [DataRow("2026-10-31T16:59:59+00:00", 202610)] // 23:59:59 UTC+7 -> still Oct 2026
    [DataRow("2026-10-31T17:00:00+00:00", 202611)] // 00:00:00 UTC+7 -> Nov 2026
    [DataRow("2026-12-31T17:00:00+00:00", 202701)] // 00:00:00 UTC+7 -> Jan 2027
    [DataRow("2026-01-01T00:00:00+07:00", 202601)]
    public void ComputePartitionYm_AlignsStrictVietnamUtcPlus7MonthBoundaries(string isoTimestamp, int expectedYm)
    {
        var ts = DateTimeOffset.Parse(isoTimestamp, CultureInfo.InvariantCulture);
        var actualYm = MeteringValueHelper.ComputePartitionYm(ts);

        Assert.AreEqual(expectedYm, actualYm, $"PartitionYm mismatch for timestamp {isoTimestamp}");
    }

    [TestMethod]
    [DataRow("2026-11-11T05:00:00+00:00", 12, 202512)] // Keeps 202512..202611 (12 months), drops < 202512
    [DataRow("2026-03-15T10:00:00+07:00", 12, 202504)] // Keeps 202504..202603 (12 months)
    [DataRow("2026-01-01T00:00:00+07:00", 3, 202511)]
    public void GetRetentionCutoffPartitionYm_ComputesExact12MonthCutoff(string isoUtcNow, int retentionMonths, int expectedCutoffYm)
    {
        var now = DateTimeOffset.Parse(isoUtcNow, CultureInfo.InvariantCulture);
        var cutoff = MeteringValueHelper.GetRetentionCutoffPartitionYm(now, retentionMonths);

        Assert.AreEqual(expectedCutoffYm, cutoff);
    }

    [TestMethod]
    public void TelemetryMetadataRegistry_MatchesAllSnapshotAndTimeSeriesColumns()
    {
        var snapshotProps = typeof(MeasurementPointCurrentSnapshotEntity)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(p => p.Name)
            .ToHashSet(StringComparer.Ordinal);
        var tsProps = typeof(MeasurementPointTimeSeriesEntity)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(p => p.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.IsTrue(TelemetryMetadataRegistry.AllSpecs.Count >= 75, "Expected full telemetry, status, and datetime registry specs.");

        foreach (var spec in TelemetryMetadataRegistry.AllSpecs)
        {
            Assert.IsTrue(snapshotProps.Contains(spec.ValueColumn), $"Missing snapshot property '{spec.ValueColumn}' for '{spec.Description}'.");
            Assert.IsTrue(snapshotProps.Contains(spec.QualityColumn), $"Missing snapshot quality property '{spec.QualityColumn}' for '{spec.Description}'.");

            if (spec.UnitColumn != null)
            {
                Assert.IsTrue(snapshotProps.Contains(spec.UnitColumn), $"Missing snapshot unit property '{spec.UnitColumn}'.");
            }

            if (spec.UpdatedAtColumn != null)
            {
                Assert.IsTrue(snapshotProps.Contains(spec.UpdatedAtColumn), $"Missing snapshot UpdatedAt property '{spec.UpdatedAtColumn}'.");
            }

            if (spec.Kind == SnapshotFieldKind.NumericMetric)
            {
                Assert.IsFalse(string.IsNullOrEmpty(spec.TimeSeriesColumn), $"NumericMetric '{spec.Description}' must map to TimeSeriesColumn.");
                Assert.IsTrue(tsProps.Contains(spec.TimeSeriesColumn!), $"Missing TimeSeries property '{spec.TimeSeriesColumn}'.");
            }
        }
    }

    [TestMethod]
    public void IoTPacketParserHelper_ParsesJsonElementsPrimitivesAndBooleans()
    {
        using var doc = JsonDocument.Parse("{\"num\": 220.45, \"str\": \"456.78\", \"bTrue\": true, \"bFalse\": false, \"n\": null, \"qInt\": 192}");
        var root = doc.RootElement;

        Assert.AreEqual("220.45", IoTPacketParserHelper.ConvertToString(root.GetProperty("num")));
        Assert.AreEqual("456.78", IoTPacketParserHelper.ConvertToString(root.GetProperty("str")));
        Assert.AreEqual("true", IoTPacketParserHelper.ConvertToString(root.GetProperty("bTrue")));
        Assert.AreEqual("null", IoTPacketParserHelper.ConvertToString(root.GetProperty("n")));
        Assert.AreEqual("plain", IoTPacketParserHelper.ConvertToString("plain"));

        Assert.IsTrue(IoTPacketParserHelper.ParseQuality(root.GetProperty("bTrue")));
        Assert.IsFalse(IoTPacketParserHelper.ParseQuality(root.GetProperty("bFalse")));
        Assert.IsTrue(IoTPacketParserHelper.ParseQuality(root.GetProperty("qInt")));
        Assert.IsTrue(IoTPacketParserHelper.ParseQuality("1"));
        Assert.IsFalse(IoTPacketParserHelper.ParseQuality("0"));
        Assert.IsFalse(IoTPacketParserHelper.ParseQuality(null));
    }

    [TestMethod]
    public void SnapshotDtoMapper_ProjectsListDetailAndIoTDtosWithRoutingFallbackAndBooleanSemantics()
    {
        var point = new MeasurementPointsEntity
        {
            Id = 1001L,
            Code = "VSIP-MP-01",
            Description = "Tram TBA 1",
            Customer = new CustomerEntity
            {
                CustomerCode = "KH001",
                FullName = "Cong ty TNHH Test"
            }
        };

        var updatedAt = new DateTimeOffset(2026, 11, 11, 6, 30, 0, TimeSpan.Zero);
        var snap = new MeasurementPointCurrentSnapshotEntity
        {
            Id = 9001L,
            MeasurementPointId = 1001L,
            UpdatedAt = updatedAt,
            LastTelemetryAt = updatedAt,
            DongDienPhaA = 125.50m,
            DongDienPhaAQuality = true,
            DienApPhaA = 22.15m,
            DienApPhaAUnit = "kV",
            DienApPhaAQuality = true,
            CongSuatTacDungTong3Pha = 450.25m,
            CongSuatTacDungTong3PhaUnit = "kW",
            CongSuatTacDungTong3PhaQuality = true,
            TanSoHeThong = 50.01m,
            TanSoHeThongUnit = "Hz",
            TanSoHeThongQuality = true,
            MatKetNoi = true, // true == Mất kết nối ('lost')
            MatKetNoiQuality = true,
            CanhBao01 = false,
            CanhBao01Quality = true,
            MeterYear = 2026,
            MeterMonth = 11,
            MeterDay = 11,
            MeterHour = 13,
            MeterMinute = 30,
            MeterSecond = 45
        };

        var routing = new PointRoutingInfo
        {
            MeasurementPointId = 1001L,
            Code = "VSIP-MP-01",
            TagsByDescription = new Dictionary<string, TagRoutingEntry>(StringComparer.OrdinalIgnoreCase)
            {
                [ListDataMeasurementPoint.DongDienPhaA] = new TagRoutingEntry { TagName = "Channel.Meter.Ia", Description = ListDataMeasurementPoint.DongDienPhaA, Unit = "A" },
                [ListDataMeasurementPoint.CanhBao01] = new TagRoutingEntry { TagName = "Channel.Meter.Warn1", Description = ListDataMeasurementPoint.CanhBao01, CustomName = "Qua dong pha A" }
            }
        };

        var listDto = SnapshotDtoMapper.ToListDto(point, snap, routing);
        Assert.AreEqual(125.50, listDto.DongDienPhaA);
        Assert.AreEqual("A", listDto.DongDienPhaAUnit); // Resolved from routing info fallback
        Assert.AreEqual("kV", listDto.DienApPhaAUnit);
        Assert.AreEqual(true, listDto.MatKetNoi); // True preserves lost-connection semantic

        var detailDto = SnapshotDtoMapper.ToDetailDto(point, snap, routing);
        Assert.AreEqual("Qua dong pha A", detailDto.CanhBao01CustomName);
        Assert.AreEqual("2026", detailDto.CurrentYear);
        Assert.AreEqual("11", detailDto.CurrentMonth);
        Assert.AreEqual("30", detailDto.CurrentMinute);

        var iotDto = SnapshotDtoMapper.ToIoTDto(point, snap, routing);
        Assert.AreEqual(450.25, iotDto.CongSuatTacDungTong3Pha);
        Assert.AreEqual(50.01, iotDto.TanSoHeThong);
    }

    [TestMethod]
    public void EnergyReportAutoMapperProfiles_FormatSuspectedChannelsAndPreserveUnits()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MeasurementPointHalfHourEnergyReportProfile>();
            cfg.AddProfile<MeasurementPointDailyEnergyReportProfile>();
            cfg.AddProfile<MeasurementPointMonitoringProfile>();
        });
        var mapper = config.CreateMapper();

        var mask = MeteringValueHelper.SetSuspectedFlag(
            0,
            EnergySuspectedFlags.ChiSoTichLuyDienNangTacDungAPTongKenhGiao,
            true);

        var halfHourEntity = new MeasurementPointHalfHourEnergyReportEntity
        {
            Id = 5001L,
            MeasurementPointId = 1001L,
            CreatedTime = new DateTimeOffset(2026, 11, 11, 6, 0, 0, TimeSpan.Zero),
            ChiSoTichLuyDienNangTacDungAPTongKenhGiao = 15230.7500m,
            ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao = 8000.0000m,
            ChiSoTichLuyDienNangPhanKhangAQTongKenhGiao = 320.5000m,
            SuspectedMask = mask,
            IsSuspected = true
        };

        var mapped = mapper.Map<FinalizedEnergyReportResponse>(halfHourEntity);

        Assert.AreEqual("15.230,75*", mapped.ChiSoTichLuyDienNangTacDungAPTongKenhGiao);
        Assert.AreEqual("8.000", mapped.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao);
        Assert.AreEqual("320,50", mapped.ChiSoTichLuyDienNangPhanKhangAQTongKenhGiao);
        Assert.AreEqual("kWh", mapped.ChiSoTichLuyDienNangTacDungAPTongKenhGiaoUnit);
        Assert.AreEqual("kVArh", mapped.ChiSoTichLuyDienNangPhanKhangAQTongKenhGiaoUnit);
    }

    [TestMethod]
    public void HalfHourOperatingReport_DropsEntityUnitColumnsWhilePreservingResponseContract()
    {
        var entityUnitProps = typeof(MeasurementPointHalfHourOperatingReportEntity)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.Name.EndsWith("Unit", StringComparison.Ordinal))
            .Select(p => p.Name)
            .ToList();
        Assert.AreEqual(0, entityUnitProps.Count, $"Unexpected entity *Unit properties: {string.Join(", ", entityUnitProps)}");

        var dtoUnitProps = typeof(Application.DataTransferObjects.Responses.MeasurementPointMonitoring.MeasurementPointOperatingReportDto)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.Name.EndsWith("Unit", StringComparison.Ordinal))
            .Select(p => p.Name)
            .ToList();
        Assert.AreEqual(0, dtoUnitProps.Count, $"Unexpected DTO *Unit properties: {string.Join(", ", dtoUnitProps)}");

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MeasurementPointHalfHourOperatingReportProfile>();
        });
        config.AssertConfigurationIsValid();

        var mapper = config.CreateMapper();
        var entity = new MeasurementPointHalfHourOperatingReportEntity
        {
            Id = 7001L,
            MeasurementPointId = 1001L,
            CreatedTime = new DateTimeOffset(2026, 11, 11, 7, 30, 0, TimeSpan.Zero),
            DienApPhaA = 22.4,
            DienApPhaB = 22.5,
            DienApPhaC = 22.3,
            DongDienPhaA = 110.2,
            CongSuatTacDungTong3Pha = 3850.5,
            CongSuatPhanKhangTong3Pha = 920.1,
            HeSoCongSuatPF = 0.97,
            TanSoHeThong = 50.01
        };

        var response = mapper.Map<OperatingParameterReportResponse>(entity);
        var configuredUnits = new ConfiguredOperatingUnits(
            VoltageA: "V",
            VoltageB: "V",
            VoltageC: "V",
            CurrentA: "kA",
            CurrentB: "kA",
            CurrentC: "kA",
            ActivePower: "MW",
            ReactivePower: "MVAr",
            Frequency: "Hz");

        OperatingReportFallbackHelper.ApplyConfiguredUnits(new[] { response }, configuredUnits);

        Assert.AreEqual(22.4, response.DienApPhaA);
        Assert.AreEqual("V", response.DienApPhaAUnit);
        Assert.AreEqual("V", response.DienApPhaBUnit);
        Assert.AreEqual("V", response.DienApPhaCUnit);
        Assert.AreEqual("kA", response.DongDienPhaAUnit);
        Assert.AreEqual("kA", response.DongDienPhaBUnit);
        Assert.AreEqual("kA", response.DongDienPhaCUnit);
        Assert.AreEqual("MW", response.CongSuatTacDungTong3PhaUnit);
        Assert.AreEqual("MVAr", response.CongSuatPhanKhangTong3PhaUnit);
        Assert.AreEqual("Hz", response.TanSoHeThongUnit);
    }

    [TestMethod]
    public void IoTDataRequest_AllowsNullableTagFieldsWithoutModelBindingFailure()
    {
        var nullabilityContext = new NullabilityInfoContext();
        var valuesProp = typeof(Application.DataTransferObjects.Requests.IoT.IoTDataRequest).GetProperty(nameof(Application.DataTransferObjects.Requests.IoT.IoTDataRequest.Values))!;
        var idProp = typeof(Application.DataTransferObjects.Requests.IoT.IoTDataValue).GetProperty(nameof(Application.DataTransferObjects.Requests.IoT.IoTDataValue.Id))!;
        var vProp = typeof(Application.DataTransferObjects.Requests.IoT.IoTDataValue).GetProperty(nameof(Application.DataTransferObjects.Requests.IoT.IoTDataValue.V))!;
        var qProp = typeof(Application.DataTransferObjects.Requests.IoT.IoTDataValue).GetProperty(nameof(Application.DataTransferObjects.Requests.IoT.IoTDataValue.Q))!;

        Assert.AreEqual(NullabilityState.Nullable, nullabilityContext.Create(valuesProp).ReadState);
        Assert.AreEqual(NullabilityState.Nullable, nullabilityContext.Create(idProp).ReadState);
        Assert.AreEqual(NullabilityState.Nullable, nullabilityContext.Create(vProp).ReadState);
        Assert.AreEqual(NullabilityState.Nullable, nullabilityContext.Create(qProp).ReadState);
    }

    [TestMethod]
    public void MeasurementPointEventCommandHandler_EvaluatesBufferedTransitionsSafely()
    {
        var pointId = 1386109002637312L;
        var tagId1 = 1425612032696341L;
        var tagId2 = 1425612032696351L;

        var routingMap = new Dictionary<string, TagRoutingEntry>(StringComparer.OrdinalIgnoreCase)
        {
            ["_AdvancedTags.M006.Status.Efa01"] = new TagRoutingEntry
            {
                TagNameId = tagId1,
                MeasurementPointId = pointId,
                MeasurementPointCode = "M006",
                TagName = "_AdvancedTags.M006.Status.Efa01",
                Description = ListDataMeasurementPoint.CanhBao01,
                CustomName = "Asymmetric Power",
                Type = "WARNING"
            },
            ["_AdvancedTags.M006.Status.Error"] = new TagRoutingEntry
            {
                TagNameId = tagId2,
                MeasurementPointId = pointId,
                MeasurementPointCode = "M006",
                TagName = "_AdvancedTags.M006.Status.Error",
                Description = ListDataMeasurementPoint.MatKetNoi,
                Type = "WARNING"
            }
        };

        var tagEntities = new List<TagNameEntity>
        {
            new()
            {
                Id = tagId1,
                MeasurementPointId = pointId,
                Description = ListDataMeasurementPoint.CanhBao01,
                TagName = "_AdvancedTags.M006.Status.Efa01",
                CustomName = "Asymmetric Power",
                Type = "WARNING",
                Status = EntityStatus.Active,
                WarningValue = false
            },
            new()
            {
                Id = tagId2,
                MeasurementPointId = pointId,
                Description = ListDataMeasurementPoint.MatKetNoi,
                TagName = "_AdvancedTags.M006.Status.Error",
                Type = "WARNING",
                Status = EntityStatus.Active,
                WarningValue = false
            }
        };

        var orphanedAlert = new MeasurementPointsAlertEntity
        {
            Id = 80001L,
            MeasurementPointId = pointId,
            TagNameId = null,
            ParameterCode = ListDataMeasurementPoint.MatKetNoi,
            EventName = "Trạng thái kết nối",
            Status = "ALARM",
            Value = "true",
            CreatedTime = DateTimeOffset.UtcNow.AddMinutes(-5)
        };
        var existingAlerts = new List<MeasurementPointsAlertEntity> { orphanedAlert };

        var addedEvents = new List<MeasurementPointsEventEntity>();
        var addedAlerts = new List<MeasurementPointsAlertEntity>();
        var removedAlerts = new List<MeasurementPointsAlertEntity>();
        var updatedTags = new List<TagNameEntity>();
        var enqueuedItems = new List<TelemetryIngestionItem>();
        long nextId = 900001L;

        var snowflake = TestProxy<Nobisoft.Core.Interfaces.ISnowflakeIdService>.Create(
            (name, _) => name == nameof(Nobisoft.Core.Interfaces.ISnowflakeIdService.GenerateId)
                ? Task.FromResult(Interlocked.Increment(ref nextId))
                : null);

        var tagRoutingCache = TestProxy<ITagRoutingCacheService>.Create(
            (name, _) => name == nameof(ITagRoutingCacheService.GetTagMapAsync)
                ? Task.FromResult<IReadOnlyDictionary<string, TagRoutingEntry>>(routingMap)
                : null);

        var bufferedIngestion = TestProxy<IBufferedTelemetryIngestionService>.Create(
            (name, args) =>
            {
                if (name == nameof(IBufferedTelemetryIngestionService.EnqueueAsync))
                {
                    enqueuedItems.Add((TelemetryIngestionItem)args![0]!);
                    return ValueTask.CompletedTask;
                }
                return null;
            });

        var logger = TestProxy<Nobisoft.Core.Logger.Interfaces.ILoggerService>.Create((_, _) => null);

        var eventRw = TestProxy<Application.Repositories.ReadWrite.IMeasurementPointEventReadWriteRepository>.Create(
            (name, args) =>
            {
                if (name == nameof(Nobisoft.Core.Database.Repository.IRepository<MeasurementPointsEventEntity>.AddAsync))
                {
                    var entity = (MeasurementPointsEventEntity)args![0]!;
                    addedEvents.Add(entity);
                    return Task.FromResult(entity);
                }
                if (name == nameof(Nobisoft.Core.Database.Repository.IRepository<MeasurementPointsEventEntity>.SaveChangesAsync))
                {
                    return Task.FromResult(addedEvents.Count);
                }
                return null;
            });

        var alertRw = TestProxy<Application.Repositories.ReadWrite.IMeasurementPointAlertReadWriteRepository>.Create(
            (name, args) =>
            {
                switch (name)
                {
                    case nameof(Application.Repositories.ReadWrite.IMeasurementPointAlertReadWriteRepository.GetAlertsForMeasurementPointsAsync):
                        return Task.FromResult(existingAlerts.ToList());
                    case nameof(Nobisoft.Core.Database.Repository.IRepository<MeasurementPointsAlertEntity>.AddAsync):
                        var added = (MeasurementPointsAlertEntity)args![0]!;
                        addedAlerts.Add(added);
                        existingAlerts.Add(added);
                        return Task.FromResult(added);
                    case nameof(Nobisoft.Core.Database.Repository.IRepository<MeasurementPointsAlertEntity>.RemoveAsync):
                        var removed = (MeasurementPointsAlertEntity)args![0]!;
                        removedAlerts.Add(removed);
                        existingAlerts.Remove(removed);
                        return Task.FromResult(removed);
                    case nameof(Nobisoft.Core.Database.Repository.IRepository<MeasurementPointsAlertEntity>.SaveChangesAsync):
                        return Task.FromResult(addedAlerts.Count + removedAlerts.Count);
                    default:
                        return null;
                }
            });

        var tagRw = TestProxy<Application.Repositories.ReadWrite.ITagNameReadWriteRepository>.Create(
            (name, args) =>
            {
                switch (name)
                {
                    case nameof(Application.Repositories.ReadWrite.ITagNameReadWriteRepository.GetTagNamesForMeasurementPointsAsync):
                        return Task.FromResult(tagEntities);
                    case nameof(Nobisoft.Core.Database.Repository.IRepository<TagNameEntity>.UpdateAsync):
                        var updated = (TagNameEntity)args![0]!;
                        updatedTags.Add(updated);
                        return Task.FromResult(updated);
                    case nameof(Nobisoft.Core.Database.Repository.IRepository<TagNameEntity>.SaveChangesAsync):
                        return Task.FromResult(updatedTags.Count);
                    default:
                        return null;
                }
            });

        var clientProxy = TestProxy<Microsoft.AspNetCore.SignalR.IClientProxy>.Create(
            (_, _) => Task.FromException(new InvalidOperationException("Simulated transient SignalR failure")));
        var hubClients = TestProxy<Microsoft.AspNetCore.SignalR.IHubClients>.Create(
            (name, _) => name == nameof(Microsoft.AspNetCore.SignalR.IHubClients.All) ? clientProxy : null);
        var hub = TestProxy<Microsoft.AspNetCore.SignalR.IHubContext<Infrastructure.Services.NotificationEventHub>>.Create(
            (name, _) => name == nameof(Microsoft.AspNetCore.SignalR.IHubContext<Infrastructure.Services.NotificationEventHub>.Clients) ? hubClients : null);

        var alertRo = TestProxy<Application.Repositories.ReadOnly.IMeasurementPointAlertReadOnlyRepository>.Create(
            (name, _) => name == nameof(Application.Repositories.ReadOnly.IMeasurementPointAlertReadOnlyRepository.CountMeasurementPointAlert)
                ? Task.FromResult(existingAlerts.Count)
                : null);

        var handler = new Infrastructure.Handlers.Commands.IoT.MeasurementPointEventCommandHandler(
            tagRoutingCache,
            bufferedIngestion,
            logger,
            eventRw,
            snowflake,
            alertRw,
            tagRw,
            hub,
            alertRo);

        // Scenario 1: Out-of-order intra-packet oscillation (t=2000 false arrives before t=1000 true) + null V + orphaned alert cleanup
        using var jsonDoc = JsonDocument.Parse("""
        {
          "timestamp": 1762870002000,
          "values": [
            {"id":"_AdvancedTags.M006.Status.Efa01","v":false,"q":"true","t":1762870002000},
            {"id":"_AdvancedTags.M006.Status.Efa01","v":true,"q":"true","t":1762870001000},
            {"id":"_AdvancedTags.M006.Status.Efa01","v":null,"q":"false","t":1762870002500},
            {"id":"_AdvancedTags.M006.Status.Error","v":false,"q":"true","t":1762870002000},
            {"id":"_AdvancedTags.M006.Status.Error","v":false,"q":"true","t":1762870003000}
          ]
        }
        """);
        var root = jsonDoc.RootElement;
        var req = new Application.DataTransferObjects.Requests.IoT.IoTDataRequest
        {
            Timestamp = root.GetProperty("timestamp").GetInt64(),
            Values = root.GetProperty("values").EnumerateArray().Select(el => new Application.DataTransferObjects.Requests.IoT.IoTDataValue
            {
                Id = el.GetProperty("id").GetString(),
                V = el.GetProperty("v"),
                Q = el.GetProperty("q").GetString(),
                T = el.GetProperty("t").GetInt64()
            }).ToList()
        };

        var result = handler.Handle(
            new Application.Commands.IoT.MeasurementPointEventCommand
            {
                Request = req,
                Tracker = new Nobisoft.Core.ValueObjects.Tracker("test")
            },
            CancellationToken.None).GetAwaiter().GetResult();

        Assert.IsTrue(result.Success);
        // Chronological evaluation produces ALARM (t=1000) then NORMAL (t=2000) without ever AddAsync+RemoveAsync on alert
        Assert.AreEqual(2, addedEvents.Count);
        Assert.AreEqual("ALARM", addedEvents[0].Status);
        Assert.AreEqual("NORMAL", addedEvents[1].Status);
        Assert.AreEqual(0, addedAlerts.Count, "Transient intra-packet alert must not be added then removed.");
        Assert.AreEqual(1, removedAlerts.Count, "Orphaned stale alert with TagNameId=null must be removed.");
        Assert.AreSame(orphanedAlert, removedAlerts[0]);
        Assert.AreEqual(false, tagEntities[0].WarningValue);
        Assert.AreEqual(1, updatedTags.Count, "Dirty tag should only be submitted once.");
        // Snapshot samples deduplicated to latest per parameter
        Assert.AreEqual(1, enqueuedItems.Count);
        Assert.AreEqual(2, enqueuedItems[0].Samples.Count);
        Assert.IsFalse(enqueuedItems[0].Samples.Single(s => s.Description == ListDataMeasurementPoint.CanhBao01).BoolValue!.Value);
        Assert.AreEqual(DateTimeOffset.FromUnixTimeMilliseconds(1762870003000),
            enqueuedItems[0].Samples.Single(s => s.Description == ListDataMeasurementPoint.MatKetNoi).Timestamp);

        // Scenario 2: Subsequent steady ALARM persists new alert cleanly
        addedEvents.Clear();
        removedAlerts.Clear();
        updatedTags.Clear();
        enqueuedItems.Clear();

        using var alarmDoc = JsonDocument.Parse("{\"v\":true}");
        var req2 = new Application.DataTransferObjects.Requests.IoT.IoTDataRequest
        {
            Timestamp = 1762870005000,
            Values =
            [
                new Application.DataTransferObjects.Requests.IoT.IoTDataValue
                {
                    Id = "_AdvancedTags.M006.Status.Efa01",
                    V = alarmDoc.RootElement.GetProperty("v"),
                    Q = "true",
                    T = 1762870005000
                }
            ]
        };

        var result2 = handler.Handle(
            new Application.Commands.IoT.MeasurementPointEventCommand
            {
                Request = req2,
                Tracker = new Nobisoft.Core.ValueObjects.Tracker("test2")
            },
            CancellationToken.None).GetAwaiter().GetResult();

        Assert.IsTrue(result2.Success);
        Assert.AreEqual(1, addedEvents.Count);
        Assert.AreEqual("ALARM", addedEvents[0].Status);
        Assert.AreEqual(1, addedAlerts.Count);
        Assert.AreEqual(true, tagEntities[0].WarningValue);
    }

    private class TestProxy<T> : DispatchProxy where T : class
    {
        private Func<string, object?[]?, object?> _handler = null!;

        public static T Create(Func<string, object?[]?, object?> handler)
        {
            var proxy = Create<T, TestProxy<T>>();
            ((TestProxy<T>)(object)proxy)._handler = handler;
            return proxy;
        }

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            var result = _handler(targetMethod!.Name, args);
            if (result != null) return result;
            if (targetMethod.ReturnType == typeof(void)) return null;
            if (targetMethod.ReturnType == typeof(Task)) return Task.CompletedTask;
            if (targetMethod.ReturnType.IsValueType) return Activator.CreateInstance(targetMethod.ReturnType);
            return null;
        }
    }
}
