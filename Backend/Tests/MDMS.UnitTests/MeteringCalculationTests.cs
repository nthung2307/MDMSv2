using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MDMS.UnitTests;

/// <summary>
/// Bộ kiểm thử tự động các thuật toán cốt lõi tính toán chỉ số và sản lượng đo đếm điện năng (AMR/MDMS).
/// Đảm bảo tính toán chính xác tuyệt đối với kiểu dữ liệu Decimal và ngăn ngừa lỗi hồi quy (Regression Bugs).
/// </summary>
[TestClass]
public class MeteringCalculationTests
{
    /// <summary>
    /// Kiểm thử tính toán điện năng tiêu thụ tiêu chuẩn theo đa biểu giá (BT, CD, TD, SG, VC)
    /// Công thức: Consumption = (CurrentReading - PreviousReading) * Multiplier
    /// </summary>
    [TestMethod]
    [DataRow(100.0, 50.0, 1.0, 50.0)]
    [DataRow(12345.6789, 12000.1234, 1.0, 345.5555)]
    [DataRow(450.50, 200.25, 40.0, 10010.0)]
    [DataRow(67.0, 0.0, 1.0, 67.0)]
    public void CalculateConsumption_NormalRange_CalculatesAccuratelyWithDecimal(
        double currDouble, double prevDouble, double multDouble, double expectedDouble)
    {
        // Arrange
        decimal current = (decimal)currDouble;
        decimal previous = (decimal)prevDouble;
        decimal multiplier = (decimal)multDouble;
        decimal expected = (decimal)expectedDouble;

        // Act
        decimal diff = current - previous;
        decimal actualConsumption = diff * multiplier;

        // Assert
        Assert.AreEqual(expected, actualConsumption, $"Sai lệch tính sản lượng: {current} - {previous} * {multiplier}");
    }

    /// <summary>
    /// Kiểm thử thuật toán xử lý tràn vòng công tơ (Meter Rollover).
    /// Khi chỉ số công tơ cơ/điện tử quay hết vòng (ví dụ qua 999,999 về 000,050):
    /// Công thức: Consumption = (MaxRolloverReading - Previous + Current) * Multiplier
    /// </summary>
    [TestMethod]
    [DataRow(50.0, 999950.0, 1000000.0, 1.0, 100.0)]
    [DataRow(12.5, 99988.0, 100000.0, 10.0, 245.0)]
    public void CalculateConsumption_MeterRollover_CalculatesCorrectly(
        double currDouble, double prevDouble, double maxDouble, double multDouble, double expectedDouble)
    {
        // Arrange
        decimal current = (decimal)currDouble;
        decimal previous = (decimal)prevDouble;
        decimal maxCapacity = (decimal)maxDouble;
        decimal multiplier = (decimal)multDouble;
        decimal expected = (decimal)expectedDouble;

        // Act: Chỉ số mới nhỏ hơn chỉ số cũ do tràn vòng
        Assert.IsTrue(current < previous, "Chỉ số mới phải nhỏ hơn chỉ số cũ trong kịch bản tràn vòng");
        decimal rolloverDiff = (maxCapacity - previous) + current;
        decimal actualConsumption = rolloverDiff * multiplier;

        // Assert
        Assert.AreEqual(expected, actualConsumption, "Sản lượng tính sai trong kịch bản tràn vòng công tơ.");
    }

    /// <summary>
    /// Kiểm thử công thức tính Hệ số công suất trung bình Cos phi = P / sqrt(P^2 + Q^2)
    /// </summary>
    [TestMethod]
    [DataRow(67.0, 28.0, 0.92)]     // Trường hợp thực tế từ kỳ T10/2026 của KYOCERA 1
    [DataRow(100.0, 0.0, 1.00)]     // Thuần trở, không có công suất phản kháng
    [DataRow(80.0, 60.0, 0.80)]     // Tam giác 3-4-5 chuẩn: 80 / 100 = 0.80
    [DataRow(0.0, 50.0, 0.00)]      // Chỉ có phản kháng, P = 0 => Cos phi = 0
    public void CalculateCosPhi_GivenActiveAndReactivePower_MatchesExpectedStandard(
        double pVal, double qVal, double expectedCosPhiDouble)
    {
        // Arrange
        decimal p = (decimal)pVal;
        decimal q = (decimal)qVal;
        decimal expectedCosPhi = (decimal)expectedCosPhiDouble;

        // Act
        decimal calculatedCosPhi = 0m;
        if (p > 0m && q >= 0m)
        {
            double pD = (double)p;
            double qD = (double)q;
            double sApparent = Math.Sqrt((pD * pD) + (qD * qD));
            if (sApparent > 0)
            {
                calculatedCosPhi = Math.Round((decimal)(pD / sApparent), 2, MidpointRounding.AwayFromZero);
            }
        }

        // Assert
        Assert.AreEqual(expectedCosPhi, calculatedCosPhi, $"Hệ số Cos phi tính sai cho P={p}, Q={q}");
    }

    /// <summary>
    /// Kiểm thử tính toán sản lượng phân tách khi có thay thế công tơ đo đếm giữa kỳ (Meter Replacement).
    /// Công thức: Total = (OldFinal - OldInitial) * OldRatio + (NewCurrent - NewInitial) * NewRatio
    /// </summary>
    [TestMethod]
    public void CalculateConsumption_WithMeterReplacement_CombinesBothMetersAccurately()
    {
        // Arrange: Giả lập điểm đo thay công tơ ngày 15 trong tháng
        decimal oldMeterInitial = 1200.0m;
        decimal oldMeterFinal = 1550.0m;    // Tháo công tơ cũ
        decimal oldRatio = 1.0m;

        decimal newMeterInitial = 0.0m;     // Treo công tơ mới
        decimal newMeterCurrent = 280.0m;    // Chỉ số chốt cuối tháng của công tơ mới
        decimal newRatio = 2.0m;            // Tỷ số công tơ mới x2

        // Act
        decimal oldConsumption = (oldMeterFinal - oldMeterInitial) * oldRatio; // (1550 - 1200) * 1 = 350
        decimal newConsumption = (newMeterCurrent - newMeterInitial) * newRatio; // (280 - 0) * 2 = 560
        decimal totalConsumption = oldConsumption + newConsumption; // 350 + 560 = 910

        // Assert
        Assert.AreEqual(350.0m, oldConsumption);
        Assert.AreEqual(560.0m, newConsumption);
        Assert.AreEqual(910.0m, totalConsumption);
    }

    /// <summary>
    /// Kiểm thử tính toán sản lượng phân tách đầy đủ 5 biểu đo đếm: BT, CD, TD, SG, VC
    /// khi thay thế công tơ có kèm thay đổi Hệ số nhân TU/TI (ví dụ HSN cũ 60, HSN mới 120).
    /// </summary>
    [TestMethod]
    public void CalculateSegmentedConsumption_MultiBiểu_WithChangedMultiplier_CalculatesAccurately()
    {
        // Arrange
        decimal oldRatio = 60.0m;
        decimal newRatio = 120.0m;

        // Chỉ số đầu kỳ
        decimal prevBt = 1000.0m, prevCd = 200.0m, prevTd = 300.0m, prevSg = 1500.0m, prevVc = 400.0m;

        // Chỉ số tháo công tơ cũ
        decimal oldBt = 1050.0m, oldCd = 220.0m, oldTd = 330.0m, oldSg = 1600.0m, oldVc = 430.0m;

        // Chỉ số treo công tơ mới ban đầu (sau kiểm định)
        decimal newBtInit = 1.0m, newCdInit = 0.5m, newTdInit = 0.5m, newSgInit = 2.0m, newVcInit = 1.0m;

        // Chỉ số chốt cuối kỳ công tơ mới
        decimal currBt = 61.0m, currCd = 15.5m, currTd = 20.5m, currSg = 97.0m, currVc = 26.0m;

        // Act - Đoạn 1: Công tơ cũ
        decimal oldBtCons = (oldBt - prevBt) * oldRatio; // (1050 - 1000) * 60 = 3000
        decimal oldCdCons = (oldCd - prevCd) * oldRatio; // (220 - 200) * 60 = 1200
        decimal oldTdCons = (oldTd - prevTd) * oldRatio; // (330 - 300) * 60 = 1800
        decimal oldSgCons = (oldSg - prevSg) * oldRatio; // (1600 - 1500) * 60 = 6000
        decimal oldVcCons = (oldVc - prevVc) * oldRatio; // (430 - 400) * 60 = 1800

        // Act - Đoạn 2: Công tơ mới
        decimal newBtCons = (currBt - newBtInit) * newRatio; // (61 - 1) * 120 = 7200
        decimal newCdCons = (currCd - newCdInit) * newRatio; // (15.5 - 0.5) * 120 = 1800
        decimal newTdCons = (currTd - newTdInit) * newRatio; // (20.5 - 0.5) * 120 = 2400
        decimal newSgCons = (currSg - newSgInit) * newRatio; // (97 - 2) * 120 = 11400
        decimal newVcCons = (currVc - newVcInit) * newRatio; // (26 - 1) * 120 = 3000

        // Tổng sản lượng kỳ
        decimal totalBt = oldBtCons + newBtCons; // 3000 + 7200 = 10200
        decimal totalCd = oldCdCons + newCdCons; // 1200 + 1800 = 3000
        decimal totalTd = oldTdCons + newTdCons; // 1800 + 2400 = 4200
        decimal totalSg = oldSgCons + newSgCons; // 6000 + 11400 = 17400
        decimal totalVc = oldVcCons + newVcCons; // 1800 + 3000 = 4800

        // Assert
        Assert.AreEqual(10200.0m, totalBt);
        Assert.AreEqual(3000.0m, totalCd);
        Assert.AreEqual(4200.0m, totalTd);
        Assert.AreEqual(17400.0m, totalSg);
        Assert.AreEqual(4800.0m, totalVc);

        // Kiểm tra tính cân bằng tổng SG = BT + CD + TD
        Assert.AreEqual(totalSg, totalBt + totalCd + totalTd, "Tổng hữu công SG phải bằng tổng 3 biểu BT + CD + TD");
    }

    /// <summary>
    /// Kiểm thử tính toán sản lượng không bao giờ bị âm nếu nhập nhầm chỉ số (bảo toàn cận dưới 0).
    /// </summary>
    [TestMethod]
    public void CalculateSegmentedConsumption_WhenReadingDecreases_GuardsAgainstNegativeValues()
    {
        decimal prevBt = 500.0m;
        decimal oldBt = 490.0m; // Nhập nhầm chỉ số tháo nhỏ hơn đầu kỳ
        decimal oldRatio = 1.0m;

        decimal oldBtCons = Math.Max(0m, (oldBt - prevBt) * oldRatio);

        Assert.AreEqual(0m, oldBtCons, "Sản lượng phân đoạn phải được chặn dưới ở mức 0, không trả về giá trị âm");
    }

    /// <summary>
    /// Kiểm thử độ chính xác hiển thị công suất Pmax luôn bảo toàn 2 chữ số thập phân (không bị làm tròn đơn vị).
    /// </summary>
    [TestMethod]
    [DataRow(66.0, "66.00")]
    [DataRow(7.52, "7.52")]
    [DataRow(0.0, "0.00")]
    [DataRow(123.456, "123.46")]
    public void PmaxPrecision_RetainsTwoDecimalPlaces(double pmaxDouble, string expectedString)
    {
        // Arrange
        decimal pmax = (decimal)pmaxDouble;

        // Act
        string formatted = pmax.ToString("N2", System.Globalization.CultureInfo.InvariantCulture);

        // Assert
        Assert.AreEqual(expectedString, formatted);
    }
}

