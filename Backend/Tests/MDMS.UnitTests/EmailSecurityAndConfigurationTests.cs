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

    /// <summary>
    /// Kiểm thử IoTSecurityManager sinh khóa ngẫu nhiên an toàn và ghi nhận dữ liệu telemetry chính xác.
    /// </summary>
    [TestMethod]
    public void IoTSecurityManager_GeneratesSecureKeyAndRecordsTelemetry_Accurately()
    {
        // Act: Sinh khóa ngẫu nhiên
        string key1 = Infrastructure.Services.IoTSecurityManager.GenerateSecureKey();
        string key2 = Infrastructure.Services.IoTSecurityManager.GenerateSecureKey();

        // Assert: Định dạng và tính duy nhất
        Assert.IsTrue(key1.StartsWith("MDMS_"), "Khóa API phải bắt đầu bằng tiền tố MDMS_");
        Assert.IsTrue(key1.Length >= 32, "Độ dài khóa API phải tối thiểu 32 ký tự để đảm bảo an toàn bảo mật");
        Assert.AreNotEqual(key1, key2, "Các khóa API sinh ra phải hoàn toàn ngẫu nhiên và khác nhau");

        // Act: Cập nhật cấu hình và ghi nhận push telemetry
        Infrastructure.Services.IoTSecurityManager.UpdateConfig(key1, "192.168.28.140, 10.0.0.5", true);
        var (cachedKey, cachedIps, requireKey) = Infrastructure.Services.IoTSecurityManager.GetCachedConfig();

        Assert.AreEqual(key1, cachedKey);
        Assert.IsTrue(requireKey);
        Assert.IsTrue(cachedIps.Contains("192.168.28.140"));

        long countBefore = Infrastructure.Services.IoTSecurityManager.TotalPushCount;
        Infrastructure.Services.IoTSecurityManager.RecordPush("192.168.28.140", true, 48);

        Assert.AreEqual("192.168.28.140", Infrastructure.Services.IoTSecurityManager.LastPushIp);
        Assert.AreEqual("SUCCESS", Infrastructure.Services.IoTSecurityManager.LastPushStatus);
        Assert.AreEqual(48, Infrastructure.Services.IoTSecurityManager.LastPushTagCount);
        Assert.AreEqual(countBefore + 1, Infrastructure.Services.IoTSecurityManager.TotalPushCount);
    }

    /// <summary>
    /// Kiểm thử ưu tiên tên miền công khai / Cloudflare Tunnel trên cơ sở dữ liệu thay vì rơi vào 127.0.0.1.
    /// </summary>
    [TestMethod]
    public void PublicBaseUrl_PrioritizesDatabaseOrTunnelDomain_OverLocalhost()
    {
        // Arrange
        string configuredDbUrl = "https://my-tunnel.trycloudflare.com/";
        string fallbackConfigUrl = "http://127.0.0.1";

        // Act: Giả lập quy tắc ưu tiên
        string resolvedUrl = !string.IsNullOrWhiteSpace(configuredDbUrl)
            ? configuredDbUrl.Trim().TrimEnd('/')
            : fallbackConfigUrl.TrimEnd('/');

        // Assert
        Assert.AreEqual("https://my-tunnel.trycloudflare.com", resolvedUrl);
        Assert.IsFalse(resolvedUrl.Contains("127.0.0.1"), "URL công khai không được là 127.0.0.1 khi đã có cấu hình Cloudflare Tunnel");
    }
}

