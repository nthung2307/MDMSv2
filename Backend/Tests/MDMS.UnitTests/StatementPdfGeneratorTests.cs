using System;
using System.Collections.Generic;
using Domain.Entities;
using Infrastructure.Services.Templates;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MDMS.UnitTests;

/// <summary>
/// Bộ kiểm thử cấu hình layout động và khả năng kết xuất tệp PDF của StatementPdfGenerator.
/// </summary>
[TestClass]
public class StatementPdfGeneratorTests
{
    [TestMethod]
    public void ParseLayoutConfig_ValidJson_ExtractsMetadataCorrectly()
    {
        // Arrange
        var template = new StatementPdfTemplateEntity
        {
            DisplayColumnsJson = "{\"columns\":[\"BT\",\"CD\",\"TD\",\"SG\",\"VC\",\"COSPHI\",\"REMARKS\",\"PMAX\",\"MULTIPLIER\"],\"documentCode\":\"M&E-1/01 PR04\",\"copyLabel\":\"(M&E Copy – Liên cho Phòng Cơ Điện)\",\"partyAName\":\"Party A (VSIP Hai Phong Co., Ltd)\\nBên A (Công ty TNHH VSIP Hải Phòng)\",\"partyBName\":\"Party B (Tenant)\\nBên B (Khách hàng)\"}"
        };

        // Act
        var layout = StatementPdfGenerator.ParseLayoutConfig(template);

        // Assert
        Assert.IsNotNull(layout);
        Assert.AreEqual("M&E-1/01 PR04", layout.DocumentCode);
        Assert.AreEqual("(M&E Copy – Liên cho Phòng Cơ Điện)", layout.CopyLabel);
        Assert.IsTrue(layout.PartyAName.Contains("Party A (VSIP Hai Phong Co., Ltd)"));
        Assert.IsTrue(layout.PartyBName.Contains("Party B (Tenant)"));
        Assert.IsTrue(layout.Columns.Contains("BT"));
        Assert.IsTrue(layout.Columns.Contains("REMARKS"));
        Assert.IsTrue(layout.Columns.Contains("MULTIPLIER"));
    }

    [TestMethod]
    public void ParseLogosConfig_NullOrEmpty_ReturnsDefaultLeftLogo()
    {
        // Arrange
        var template = new StatementPdfTemplateEntity
        {
            LogoBase64 = "base64-data-test",
            ExcelTemplateBase64 = null
        };

        // Act
        var logos = StatementPdfGenerator.ParseLogosConfig(template);

        // Assert
        Assert.IsNotNull(logos);
        Assert.AreEqual("base64-data-test", logos.HeaderLeftLogo);
    }
}

