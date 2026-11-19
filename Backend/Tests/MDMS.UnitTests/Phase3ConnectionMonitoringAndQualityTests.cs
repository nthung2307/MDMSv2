using System;
using Application.DataTransferObjects.Responses.MeasurementPointMonitoring;
using Domain.Entities;
using Infrastructure.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MDMS.UnitTests;

[TestClass]
public class Phase3ConnectionMonitoringAndQualityTests
{
    [TestMethod]
    public void EvaluateHealth_WhenSnapshotNull_ReturnsNeverReceived()
    {
        var point = new MeasurementPointsEntity
        {
            Id = 1,
            Code = "M001",
            MeterSerial = "253389001"
        };

        var (status, lastReceived, reason, hasBad) = SnapshotDtoMapper.EvaluateHealth(point, null, timeoutMinutes: 5);

        Assert.AreEqual("NEVER_RECEIVED", status);
        Assert.IsNull(lastReceived);
        Assert.IsTrue(reason?.Contains("Chưa có dữ liệu") == true);
        Assert.IsFalse(hasBad);
    }

    [TestMethod]
    public void EvaluateHealth_WhenReceivedLongAgo_ReturnsStaleStopped()
    {
        var point = new MeasurementPointsEntity
        {
            Id = 2,
            Code = "M002",
            MeterSerial = "253389002"
        };
        var snap = new MeasurementPointCurrentSnapshotEntity
        {
            MeasurementPointId = 2,
            LastTelemetryAt = DateTimeOffset.UtcNow.AddMinutes(-30),
            DienApPhaA = 220.5m,
            DienApPhaAQuality = true
        };

        var (status, lastReceived, reason, hasBad) = SnapshotDtoMapper.EvaluateHealth(point, snap, timeoutMinutes: 5);

        Assert.AreEqual("STALE_STOPPED", status);
        Assert.IsNotNull(lastReceived);
        Assert.IsTrue(reason?.Contains("Ngừng cập nhật") == true);
        Assert.IsFalse(hasBad);
    }

    [TestMethod]
    public void EvaluateHealth_WhenRecentlyReceived_ReturnsOnline()
    {
        var point = new MeasurementPointsEntity
        {
            Id = 3,
            Code = "M003",
            MeterSerial = "253389003"
        };
        var snap = new MeasurementPointCurrentSnapshotEntity
        {
            MeasurementPointId = 3,
            LastTelemetryAt = DateTimeOffset.UtcNow.AddSeconds(-45),
            DienApPhaA = 220.5m,
            DienApPhaAQuality = true
        };

        var (status, lastReceived, reason, hasBad) = SnapshotDtoMapper.EvaluateHealth(point, snap, timeoutMinutes: 5);

        Assert.AreEqual("ONLINE", status);
        Assert.IsNotNull(lastReceived);
        Assert.IsTrue(reason?.Contains("bình thường") == true);
        Assert.IsFalse(hasBad);
    }

    [TestMethod]
    public void EvaluateHealth_WhenAnyQualityIsFalse_DetectsBadQuality()
    {
        var point = new MeasurementPointsEntity
        {
            Id = 4,
            Code = "M004",
            MeterSerial = "253389004"
        };
        var snap = new MeasurementPointCurrentSnapshotEntity
        {
            MeasurementPointId = 4,
            LastTelemetryAt = DateTimeOffset.UtcNow.AddSeconds(-10),
            DienApPhaA = 220.5m,
            DienApPhaAQuality = false // Modbus Bad Quality!
        };

        var (status, lastReceived, reason, hasBad) = SnapshotDtoMapper.EvaluateHealth(point, snap, timeoutMinutes: 5);

        Assert.AreEqual("ONLINE", status);
        Assert.IsTrue(hasBad);
    }

    [TestMethod]
    public void ToListDto_MapsHealthFieldsProperly()
    {
        var point = new MeasurementPointsEntity
        {
            Id = 5,
            Code = "M005",
            MeterSerial = "253389005"
        };
        var snap = new MeasurementPointCurrentSnapshotEntity
        {
            MeasurementPointId = 5,
            LastTelemetryAt = DateTimeOffset.UtcNow.AddMinutes(-12),
            DongDienPhaA = 5.2m,
            DongDienPhaAQuality = false
        };

        var dto = SnapshotDtoMapper.ToListDto(point, snap, timeoutMinutes: 5);

        Assert.AreEqual("M005", dto.MeasurementPointCode);
        Assert.AreEqual("253389005", dto.MeterSerial);
        Assert.AreEqual("STALE_STOPPED", dto.RestStatus);
        Assert.IsTrue(dto.HasBadQuality);
        Assert.IsFalse(dto.DongDienPhaAQuanlity);
    }

    [TestMethod]
    public void HealthSummaryDto_IncludesBadQualityPoints()
    {
        var summary = new MeasurementPointMonitoringHealthSummaryDto
        {
            TotalPoints = 162,
            GoodPoints = 158,
            ModbusLostPoints = 2,
            RestStalePoints = 2,
            BadQualityPoints = 3,
            RestInactivityTimeoutMinutes = 5,
            EvaluatedAt = DateTimeOffset.UtcNow
        };

        Assert.AreEqual(162, summary.TotalPoints);
        Assert.AreEqual(3, summary.BadQualityPoints);
    }
}

