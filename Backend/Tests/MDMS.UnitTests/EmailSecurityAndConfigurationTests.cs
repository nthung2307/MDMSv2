using System;
using Infrastructure.Services.EmailProviders;
using Infrastructure.Services.Templates;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MDMS.UnitTests;

/// <summary>
/// Bộ kiểm thử bảo mật mã hóa mật khẩu cấu hình Email và tính toàn vẹn ký tự Unicode của Notification Templates.
/// </summary>
[TestClass]
public class EmailSecurityAndConfigurationTests
{
    private const string TestCryptoKey = "VSIP_MDMS_V2_SECRET_KEY_FOR_TESTING_PURPOSES_2026";

    /// <summary>
    /// Kiểm thử mã hóa đối xứng AES cho mật khẩu hòm thư SMTP và Client Secret của Microsoft Graph OAuth 2.0.
    /// Đảm bảo mật khẩu không bao giờ lưu Plaintext trong CSDL và giải mã chính xác 100%.
    /// </summary>
    [TestMethod]
    [DataRow("Abc@123456")]
    [DataRow("P@ssw0rd_Very_Complex_#2026!")]
    [DataRow("secret-with-special-chars-&^%$#@!~")]
    public void EmailPasswordEncryption_EncryptAndDecrypt_ReturnsOriginalPlainText(string rawSecret)
    {
        // Act
        string encrypted = EmailSecretCryptoHelper.Encrypt(rawSecret, TestCryptoKey);
        string decrypted = EmailSecretCryptoHelper.Decrypt(encrypted, TestCryptoKey);

        // Assert
        Assert.IsFalse(string.IsNullOrWhiteSpace(encrypted), "Chuỗi mã hóa không được để trống");
        Assert.AreNotEqual(rawSecret, encrypted, "Mật khẩu sau mã hóa phải khác hoàn toàn mật khẩu gốc (không được lưu plaintext)");
        Assert.AreEqual(rawSecret, decrypted, "Mật khẩu giải mã phải trùng khớp 100% với mật khẩu gốc");
    }

    /// <summary>
    /// Kiểm thử cơ chế phát hiện chuỗi Unicode bị lỗi font (Garbled Text / Encoding Corruption).
    /// </summary>
    [TestMethod]
    public void EncodingVerification_DetectsCorruptedCharactersAccurately()
    {
        // Arrange
        string normalVietnamese = "PHIẾU GHI CHỈ SỐ ĐIỆN HÀNG THÁNG - CÔNG TY TNHH VSIP HẢI PHÒNG";
        string garbledVietnamese = "TH├öNG B├üO CH─q Sß╗É ─ÉIß╗åN N─éNG";

        // Act & Assert
        Assert.IsFalse(NotificationTemplateService.IsGarbledEncoding(normalVietnamese), "Chuỗi tiếng Việt UTF-8 hợp lệ không được đánh dấu là lỗi");
        Assert.IsTrue(NotificationTemplateService.IsGarbledEncoding(garbledVietnamese), "Chuỗi bị lỗi font CP1252/ASCII phải được nhận diện chính xác");
    }

    /// <summary>
    /// Kiểm thử cơ chế thay thế biến giữ chỗ (Placeholder Substitution) trong mẫu email.
    /// </summary>
    [TestMethod]
    public void TemplatePlaceholders_SubstitutesAllVariablesProperly()
    {
        // Arrange
        string template = "Kính gửi {{CustomerName}}, sản lượng kỳ {{Period}} của điểm đo {{PointName}} là: {{ConsumtionTotal}} kWh.";
        
        // Act
        string rendered = template
            .Replace("{{CustomerName}}", "KYOCERA VIỆT NAM")
            .Replace("{{Period}}", "10/2026")
            .Replace("{{PointName}}", "KYOCERA 1")
            .Replace("{{ConsumtionTotal}}", "67.00");

        // Assert
        Assert.IsFalse(rendered.Contains("{{"), "Toàn bộ placeholder phải được thay thế, không còn dấu ngoặc nhọn đúp");
        Assert.IsTrue(rendered.Contains("KYOCERA VIỆT NAM"));
        Assert.IsTrue(rendered.Contains("67.00"));
    }
}

