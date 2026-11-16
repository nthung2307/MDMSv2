using System;
using System.Collections.Generic;
using System.Linq;
using Application.Common;
using Application.Helpers;
using Application.Services;
using Infrastructure.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nobisoft.Core.Constants;

namespace MDMS.UnitTests;

/// <summary>
/// Phase 2 Verification Suite: Covers 60-tag OBIS catalog integrity, IEC 62056-61 normalization,
/// grouped Date/Time 0_9_1..0_9_8 parsing, Kepware legacy alias backward compatibility,
/// 2-tier per-point / global API key hashing & AES-256 reversible encryption, unmatched serial tracking,
/// and 3-layer HYBRID/AUTO/MANUAL tag routing precedence.
/// </summary>
[TestClass]
public class Phase2ObisAndAuthTests
{
    [TestMethod]
    public void ObisCatalog_Contains60UniqueCanonicalTagsAndMatchesVoltageProfiles()
    {
        Assert.AreEqual(60, ObisTelemetryCatalog.AllEntries.Count, "Expected exactly 60 canonical OBIS entries.");
        Assert.AreEqual(60, ObisTelemetryCatalog.AllEntries.Select(e => e.ObisCode).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.AreEqual(60, ObisTelemetryCatalog.AllEntries.Select(e => e.ParameterCode).Distinct(StringComparer.OrdinalIgnoreCase).Count());

        // Verify MV vs LV phase voltage unit differentiation
        var ua = ObisTelemetryCatalog.ByObisCode["32_7_0"];
        Assert.AreEqual("kV", ua.MvUnit);
        Assert.AreEqual("V", ua.LvUnit);

        // Verify IEC 62056-61 clock ordering: 0_9_1 = Time, 0_9_2 = Date
        Assert.AreEqual(ObisCompositeDateTimeTarget.MeterTime, ObisTelemetryCatalog.ByObisCode["0_9_1"].CompositeDateTime);
        Assert.AreEqual(ObisCompositeDateTimeTarget.MeterDate, ObisTelemetryCatalog.ByObisCode["0_9_2"].CompositeDateTime);

        // Verify extended alert & connection codes
        Assert.AreEqual(ListDataMeasurementPoint.MatKetNoi, ObisTelemetryCatalog.ByObisCode["96_5_0"].ParameterCode);
        Assert.AreEqual(ListDataMeasurementPoint.CanhBao01, ObisTelemetryCatalog.ByObisCode["97_97_1"].ParameterCode);
        Assert.AreEqual(ListDataMeasurementPoint.CanhBao10, ObisTelemetryCatalog.ByObisCode["97_97_10"].ParameterCode);
    }

    [TestMethod]
    [DataRow("1_8_0", "1_8_0")]
    [DataRow("1.8.0", "1_8_0")]
    [DataRow("1-0:1.8.0*255", "1_8_0")]
    [DataRow("1-0:1.8.0*1", "1_8_0_1")]
    [DataRow("_AdvancedTags.M010.31_7_0", "31_7_0")]
    [DataRow("SubstationA.Bay1.1.8.0*1", "1_8_0_1")]
    [DataRow("0-0:0.9.1", "0_9_1")]
    [DataRow("0-0:97.97.10", "97_97_10")]
    [DataRow("_AdvancedTags.M010.Energy.PTRU", null)]
    [DataRow("RandomTagName", null)]
    public void NormalizeObisCode_ExtractsCanonicalObisAcrossDelimitersAndPrefixes(string rawTagId, string? expectedObis)
    {
        var normalized = ObisTelemetryCatalog.NormalizeObisCode(rawTagId);
        Assert.AreEqual(expectedObis, normalized);
    }

    [TestMethod]
    [DataRow(ObisCompositeDateTimeTarget.MeterTime, "14:35:08", (short)14, (short)35, (short)8)]
    [DataRow(ObisCompositeDateTimeTarget.MeterTime, "09:05", (short)9, (short)5, (short)0)]
    [DataRow(ObisCompositeDateTimeTarget.MeterTime, "235959", (short)23, (short)59, (short)59)]
    [DataRow(ObisCompositeDateTimeTarget.MeterDate, "2026-11-12", (short)2026, (short)11, (short)12)]
    [DataRow(ObisCompositeDateTimeTarget.MeterDate, "12/11/2026", (short)2026, (short)11, (short)12)]
    [DataRow(ObisCompositeDateTimeTarget.ClosingDate, "20261101", (short)2026, (short)11, (short)1)]
    [DataRow(ObisCompositeDateTimeTarget.PmaxCurrentTime, "18:45:30", (short)18, (short)45, (short)30)]
    public void TryDecomposeCompositeDateTime_ParsesSupportedFormats(
        ObisCompositeDateTimeTarget target,
        string rawValue,
        short expected1,
        short expected2,
        short expected3)
    {
        var ok = ObisTelemetryCatalog.TryDecomposeCompositeDateTime(target, rawValue, out var parts);
        Assert.IsTrue(ok, $"Expected '{rawValue}' to parse for target {target}");
        Assert.AreEqual((expected1, expected2, expected3), parts);
    }

    [TestMethod]
    [DataRow(ObisCompositeDateTimeTarget.MeterDate, "1995-01-01")]
    [DataRow(ObisCompositeDateTimeTarget.MeterDate, "not-a-date")]
    [DataRow(ObisCompositeDateTimeTarget.MeterTime, "99:99:99")]
    public void TryDecomposeCompositeDateTime_RejectsInvalidOrOutOfRangeValues(
        ObisCompositeDateTimeTarget target,
        string rawValue)
    {
        var ok = ObisTelemetryCatalog.TryDecomposeCompositeDateTime(target, rawValue, out _);
        Assert.IsFalse(ok);
    }

    [TestMethod]
    [DataRow("_AdvancedTags.M010.Energy.PTRU", ListDataMeasurementPoint.ChiSoTichLuyDienNangTacDungAPTong, null)]
    [DataRow("M010.Meas.PF", ListDataMeasurementPoint.HeSoCongSuatPFTrungBinh3Pha, null)]
    [DataRow("_AdvancedTags.M010.Status.Efa01", ListDataMeasurementPoint.CanhBao01, null)]
    [DataRow("_AdvancedTags.M010.Status.Error", ListDataMeasurementPoint.MatKetNoi, null)]
    [DataRow("_AdvancedTags.M010.DT.Current.Year", ListDataMeasurementPoint.NamHienTai, ListDataMeasurementPoint.NamHienTai)]
    [DataRow("M010.DT.MDP.Second", ListDataMeasurementPoint.GiayPmaxKyTruoc, ListDataMeasurementPoint.GiayPmaxKyTruoc)]
    public void TryMatchLegacySuffix_PreservesFullKepwareBackwardCompatibility(
        string rawTagId,
        string expectedParamCode,
        string? expectedLegacyDateTimeParam)
    {
        var matched = ObisTelemetryCatalog.TryMatchLegacySuffix(rawTagId, out var code, out _, out var dtParam);
        Assert.IsTrue(matched);
        Assert.AreEqual(expectedParamCode, code);
        Assert.AreEqual(expectedLegacyDateTimeParam, dtParam);
    }

    [TestMethod]
    public void IoTSecurityManager_TwoTierKeysHashingAesAndUnmatchedSerialsWorkEndToEnd()
    {
        var key = IoTSecurityManager.GenerateSecureKey("MDMS_PT");
        Assert.IsTrue(key.StartsWith("MDMS_PT_", StringComparison.Ordinal));

        var hash = IoTSecurityManager.HashSecret(key);
        Assert.AreEqual(64, hash.Length);
        Assert.IsTrue(IoTSecurityManager.VerifySecretHash(key, hash));
        Assert.IsFalse(IoTSecurityManager.VerifySecretHash(key + "_tampered", hash));

        var encrypted = IoTSecurityManager.EncryptPointSecret(key);
        Assert.AreNotEqual(key, encrypted);
        var decrypted = IoTSecurityManager.DecryptPointSecret(encrypted);
        Assert.AreEqual(key, decrypted);

        var prefix = IoTSecurityManager.ExtractKeyPrefix(key);
        Assert.IsTrue(prefix.StartsWith("MDMS_PT_", StringComparison.Ordinal));
        Assert.IsTrue(prefix.EndsWith("...", StringComparison.Ordinal));

        var serial = $"SN_TEST_{Guid.NewGuid():N}"[..16];
        IoTSecurityManager.RecordUnmatchedSerial(serial, "10.0.0.5");
        IoTSecurityManager.RecordUnmatchedSerial(serial, "10.0.0.6");
        var recorded = IoTSecurityManager.GetRecentUnmatchedSerials().FirstOrDefault(x => x.MeterSerial == serial);
        Assert.IsNotNull(recorded);
        Assert.AreEqual(2L, recorded.HitCount);
        Assert.AreEqual("10.0.0.6", recorded.RemoteIp);

        IoTSecurityManager.RemoveUnmatchedSerial(serial);
        Assert.IsNull(IoTSecurityManager.GetRecentUnmatchedSerials().FirstOrDefault(x => x.MeterSerial == serial));
    }

    [TestMethod]
    public void ResolveTagForPoint_EnforcesThreeLayerPrecedenceByMappingMode()
    {
        var resolver = new TagRoutingCacheService(null!, null!);

        PointRoutingInfo BuildPoint(string mappingMode)
        {
            var point = new PointRoutingInfo
            {
                MeasurementPointId = 99,
                Code = "MP099",
                Status = EntityStatus.Active,
                MeterSerial = "20269999",
                MappingMode = mappingMode
            };

            // Tier 1 manual override maps "_AdvancedTags.M010.1_8_0" -> reactive energy override to prove precedence
            point.ManualTagsByRawId["_AdvancedTags.M010.1_8_0"] = new TagRoutingEntry
            {
                TagNameId = 501,
                MeasurementPointId = 99,
                TagName = "_AdvancedTags.M010.1_8_0",
                Description = "MANUAL_OVERRIDE_PARAM"
            };

            // Tier 2a OBIS profile maps "1_8_0"
            point.ProfileObisMap["1_8_0"] = new TagRoutingEntry
            {
                TagNameId = 0,
                MeasurementPointId = 99,
                TagName = "1_8_0",
                ObisCode = "1_8_0",
                Description = ListDataMeasurementPoint.ChiSoTichLuyDienNangTacDungAPTong
            };

            // Tier 2b Legacy suffix maps "Energy.PTRU"
            point.ProfileAliasMap["Energy.PTRU"] = new TagRoutingEntry
            {
                TagNameId = 0,
                MeasurementPointId = 99,
                TagName = "1_8_0",
                ObisCode = "1_8_0",
                Description = ListDataMeasurementPoint.ChiSoTichLuyDienNangTacDungAPTong
            };

            return point;
        }

        // HYBRID: Manual override wins over OBIS; clean OBIS & legacy alias & legacy DT also resolve
        var hybrid = BuildPoint(Constants.MappingMode.Hybrid);
        Assert.AreEqual("MANUAL_OVERRIDE_PARAM", resolver.ResolveTagForPoint(hybrid, "_AdvancedTags.M010.1_8_0")?.Description);
        Assert.AreEqual(ListDataMeasurementPoint.ChiSoTichLuyDienNangTacDungAPTong, resolver.ResolveTagForPoint(hybrid, "M020.1-0:1.8.0*255")?.Description);
        Assert.AreEqual(ListDataMeasurementPoint.ChiSoTichLuyDienNangTacDungAPTong, resolver.ResolveTagForPoint(hybrid, "_AdvancedTags.M010.Energy.PTRU")?.Description);
        Assert.AreEqual(ListDataMeasurementPoint.NamHienTai, resolver.ResolveTagForPoint(hybrid, "_AdvancedTags.M010.DT.Current.Year")?.Description);

        // AUTO: Ignores Tier 1 manual override and resolves by canonical OBIS
        var autoPoint = BuildPoint(Constants.MappingMode.Auto);
        Assert.AreEqual(ListDataMeasurementPoint.ChiSoTichLuyDienNangTacDungAPTong, resolver.ResolveTagForPoint(autoPoint, "_AdvancedTags.M010.1_8_0")?.Description);

        // MANUAL: Only accepts exact Tier 1 manual tags
        var manualPoint = BuildPoint(Constants.MappingMode.Manual);
        Assert.AreEqual("MANUAL_OVERRIDE_PARAM", resolver.ResolveTagForPoint(manualPoint, "_AdvancedTags.M010.1_8_0")?.Description);
        Assert.IsNull(resolver.ResolveTagForPoint(manualPoint, "1_8_0"));
        Assert.IsNull(resolver.ResolveTagForPoint(manualPoint, "_AdvancedTags.M010.Energy.PTRU"));
    }

    [TestMethod]
    public void MeterTelemetryIngestionService_EnqueuesStatusTransitionsNonBlockingWithoutScopedRepositories()
    {
        var pointId = 777001L;
        var serial = "SN_ASYNC_777001";
        var pointInfo = new PointRoutingInfo
        {
            MeasurementPointId = pointId,
            Code = "MP_ASYNC_01",
            Status = EntityStatus.Active,
            MeterSerial = serial,
            MappingMode = Constants.MappingMode.Hybrid
        };
        var entry = new TagRoutingEntry
        {
            TagNameId = 0,
            MeasurementPointId = pointId,
            MeasurementPointCode = "MP_ASYNC_01",
            TagName = "97_97_1",
            ObisCode = "97_97_1",
            Description = ListDataMeasurementPoint.CanhBao01,
            Type = "WARNING"
        };
        pointInfo.ProfileObisMap["97_97_1"] = entry;
        pointInfo.ProfileAliasMap["Status.Efa01"] = entry;
        pointInfo.TagsByDescription[ListDataMeasurementPoint.CanhBao01] = entry;

        var pointMap = new Dictionary<long, PointRoutingInfo> { [pointId] = pointInfo };
        var enqueuedSnapshots = new List<TelemetryIngestionItem>();
        var enqueuedStatusBatches = new List<IReadOnlyList<TelemetryStatusTransitionItem>>();

        var tagRoutingCache = new StubTagRoutingCache(pointInfo, pointMap);
        var bufferedService = new StubBufferedTelemetryService(enqueuedSnapshots, enqueuedStatusBatches);

        // Use lightweight 3-arg constructor (zero Scoped repositories)
        var ingestionService = new MeterTelemetryIngestionService(tagRoutingCache, bufferedService, null!);

        var req = new Application.DataTransferObjects.Requests.IoT.IoTDataRequest
        {
            Timestamp = 1763000010000L,
            Values = new List<Application.DataTransferObjects.Requests.IoT.IoTDataValue>
            {
                new() { Id = "_AdvancedTags.M001.Status.Efa01", V = true, Q = "true", T = 1763000010000L }
            }
        };

        var res = ingestionService.IngestAsync(req, serial, TelemetrySourceKind.Unified)
            .GetAwaiter().GetResult();

        Assert.IsTrue(res.Success);
        Assert.AreEqual(1, enqueuedSnapshots.Count);
        Assert.AreEqual(1, enqueuedStatusBatches.Count);
        Assert.AreEqual(1, enqueuedStatusBatches[0].Count);
        Assert.IsTrue(enqueuedStatusBatches[0][0].ParsedBool);
        Assert.AreEqual(ListDataMeasurementPoint.CanhBao01, enqueuedStatusBatches[0][0].Tag.Description);
    }

    private sealed class StubTagRoutingCache : ITagRoutingCacheService
    {
        private readonly PointRoutingInfo _point;
        private readonly IReadOnlyDictionary<long, PointRoutingInfo> _pointMap;

        public StubTagRoutingCache(PointRoutingInfo point, IReadOnlyDictionary<long, PointRoutingInfo> pointMap)
        {
            _point = point;
            _pointMap = pointMap;
        }

        public Task<IReadOnlyDictionary<string, TagRoutingEntry>> GetTagMapAsync(System.Threading.CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyDictionary<string, TagRoutingEntry>>(new Dictionary<string, TagRoutingEntry>());

        public Task<IReadOnlyDictionary<long, PointRoutingInfo>> GetPointMapAsync(System.Threading.CancellationToken cancellationToken = default)
            => Task.FromResult(_pointMap);

        public Task<PointRoutingInfo?> GetPointBySerialAsync(string meterSerial, System.Threading.CancellationToken cancellationToken = default)
            => Task.FromResult<PointRoutingInfo?>(_point);

        public Task<IReadOnlyList<MeterProfileCachedInfo>> GetMeterProfilesAsync(System.Threading.CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<MeterProfileCachedInfo>>(Array.Empty<MeterProfileCachedInfo>());

        public TagRoutingEntry? ResolveTagForPoint(PointRoutingInfo point, string rawTagId)
        {
            var resolver = new TagRoutingCacheService(null!, null!);
            return resolver.ResolveTagForPoint(point, rawTagId);
        }

        public void Invalidate() { }
    }

    private sealed class StubBufferedTelemetryService : IBufferedTelemetryIngestionService
    {
        private readonly List<TelemetryIngestionItem> _snapshots;
        private readonly List<IReadOnlyList<TelemetryStatusTransitionItem>> _statusBatches;

        public StubBufferedTelemetryService(
            List<TelemetryIngestionItem> snapshots,
            List<IReadOnlyList<TelemetryStatusTransitionItem>> statusBatches)
        {
            _snapshots = snapshots;
            _statusBatches = statusBatches;
        }

        public System.Threading.Tasks.ValueTask EnqueueAsync(TelemetryIngestionItem item, System.Threading.CancellationToken cancellationToken = default)
        {
            _snapshots.Add(item);
            return System.Threading.Tasks.ValueTask.CompletedTask;
        }

        public bool TryEnqueueStatusTransitions(IReadOnlyList<TelemetryStatusTransitionItem> items)
        {
            _statusBatches.Add(items);
            return true;
        }

        public System.Threading.Tasks.Task FlushPendingAsync(System.Threading.CancellationToken cancellationToken = default)
            => System.Threading.Tasks.Task.CompletedTask;
    }
}

