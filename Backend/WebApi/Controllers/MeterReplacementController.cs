using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Application.Common.Responses.Common.Responses;
using Application.Common.ValueObjects;
using Application.Common.ValueObjects.OffsetBased;
using Application.DataTransferObjects.Requests.MeterReplacement;
using Application.DataTransferObjects.Responses.MeterReplacement;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nobisoft.Core.Authorization;
using Nobisoft.Core.Authorization.Attributes;
using Swashbuckle.AspNetCore.Annotations;

namespace WebApi.Controllers;

[ApiController]
[Area(Common.Url.Areas.MeterReplacements)]
[ApiVersion(Constants.ApiVersion.V1_0)]
[SwaggerTag(Common.Url.Areas.MeterReplacements)]
public class MeterReplacementController : ControllerBase
{
    private readonly IMeterReplacementService _meterReplacementService;
    private readonly ILogger<MeterReplacementController> _logger;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    // HttpClient chuyên biệt cho Gemini API: ép kết nối IPv4 để tránh lỗi treo 60s do IPv6 blackhole trên VMware/Windows Server
    private static readonly HttpClient GeminiHttpClient = CreateIpv4GeminiHttpClient();

    private static HttpClient CreateIpv4GeminiHttpClient()
    {
        var handler = new SocketsHttpHandler
        {
            UseProxy = false,
            ConnectTimeout = TimeSpan.FromSeconds(8),
            PooledConnectionLifetime = TimeSpan.FromMinutes(2),
            ConnectCallback = async (context, cancellationToken) =>
            {
                var ipv4Addresses = await Dns.GetHostAddressesAsync(
                    context.DnsEndPoint.Host,
                    AddressFamily.InterNetwork,
                    cancellationToken);

                if (ipv4Addresses == null || ipv4Addresses.Length == 0)
                {
                    throw new SocketException((int)SocketError.HostNotFound);
                }

                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    NoDelay = true
                };

                try
                {
                    await socket.ConnectAsync(ipv4Addresses, context.DnsEndPoint.Port, cancellationToken);
                    return new NetworkStream(socket, ownsSocket: true);
                }
                catch
                {
                    socket.Dispose();
                    throw;
                }
            }
        };

        return new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(90)
        };
    }

    public MeterReplacementController(
        IMeterReplacementService meterReplacementService,
        ILogger<MeterReplacementController> logger,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory)
    {
        _meterReplacementService = meterReplacementService;
        _logger = logger;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }

    [HttpPost]
    [Route(Common.Url.Default.MeterReplacements.List)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff])]
    [SwaggerOperation(Summary = "Danh sách biên bản treo tháo công tơ (phân trang, tìm kiếm, lọc)")]
    public async Task<RequestResult<OffsetPaginationResponse<MeterReplacementDto>>> GetListAsync(
        [FromBody] GetListMeterReplacementRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _meterReplacementService.GetListAsync(request, cancellationToken);
            return RequestResult<OffsetPaginationResponse<MeterReplacementDto>>.Succeed(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách biên bản treo tháo công tơ: {Message}", ex.Message);
            return RequestResult<OffsetPaginationResponse<MeterReplacementDto>>.Fail(new[]
            {
                new ErrorItem
                {
                    FieldName = "MeterReplacement",
                    Error = "Lỗi khi lấy danh sách biên bản: " + ex.Message
                }
            });
        }
    }

    [HttpGet]
    [Route(Common.Url.Default.MeterReplacements.View)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff])]
    [SwaggerOperation(Summary = "Chi tiết biên bản treo tháo theo ID")]
    public async Task<RequestResult<MeterReplacementDto>> GetDetailAsync(
        [FromRoute] long id,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _meterReplacementService.GetByIdAsync(id, cancellationToken);
            if (result == null)
            {
                return RequestResult<MeterReplacementDto>.Fail(new[]
                {
                    new ErrorItem
                    {
                        FieldName = "Id",
                        Error = $"Không tìm thấy biên bản treo tháo ID {id}"
                    }
                });
            }

            return RequestResult<MeterReplacementDto>.Succeed(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy chi tiết biên bản {Id}: {Message}", id, ex.Message);
            return RequestResult<MeterReplacementDto>.Fail(new[]
            {
                new ErrorItem
                {
                    FieldName = "MeterReplacement",
                    Error = ex.Message
                }
            });
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.MeterReplacements.Create)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff])]
    [SwaggerOperation(Summary = "Tạo mới biên bản treo tháo công tơ (DRAFT hoặc APPROVED)")]
    public async Task<RequestResult<MeterReplacementDto>> CreateAsync(
        [FromBody] CreateMeterReplacementRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _meterReplacementService.CreateAsync(request, cancellationToken);
            return RequestResult<MeterReplacementDto>.Succeed(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo biên bản treo tháo: {Message}", ex.Message);
            return RequestResult<MeterReplacementDto>.Fail(new[]
            {
                new ErrorItem
                {
                    FieldName = "MeterReplacement",
                    Error = ex.Message
                }
            });
        }
    }

    [HttpPut]
    [Route(Common.Url.Default.MeterReplacements.Update)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff])]
    [SwaggerOperation(Summary = "Cập nhật biên bản treo tháo (chỉ khi ở trạng thái DRAFT)")]
    public async Task<RequestResult<MeterReplacementDto>> UpdateAsync(
        [FromRoute] long id,
        [FromBody] UpdateMeterReplacementRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _meterReplacementService.UpdateAsync(id, request, cancellationToken);
            return RequestResult<MeterReplacementDto>.Succeed(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật biên bản treo tháo {Id}: {Message}", id, ex.Message);
            return RequestResult<MeterReplacementDto>.Fail(new[]
            {
                new ErrorItem
                {
                    FieldName = "MeterReplacement",
                    Error = ex.Message
                }
            });
        }
    }

    [HttpDelete]
    [Route(Common.Url.Default.MeterReplacements.Delete)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin])]
    [SwaggerOperation(Summary = "Xóa biên bản treo tháo (chỉ khi ở trạng thái DRAFT)")]
    public async Task<RequestResult<bool>> DeleteAsync(
        [FromRoute] long id,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _meterReplacementService.DeleteAsync(id, cancellationToken);
            return RequestResult<bool>.Succeed(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xóa biên bản treo tháo {Id}: {Message}", id, ex.Message);
            return RequestResult<bool>.Fail(new[]
            {
                new ErrorItem
                {
                    FieldName = "MeterReplacement",
                    Error = ex.Message
                }
            });
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.MeterReplacements.Approve)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin])]
    [SwaggerOperation(Summary = "Phê duyệt biên bản treo tháo (chuyển sang APPROVED & cập nhật HSN/Điểm đo)")]
    public async Task<RequestResult<MeterReplacementDto>> ApproveAsync(
        [FromRoute] long id,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _meterReplacementService.ApproveAsync(id, cancellationToken);
            return RequestResult<MeterReplacementDto>.Succeed(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi phê duyệt biên bản treo tháo {Id}: {Message}", id, ex.Message);
            return RequestResult<MeterReplacementDto>.Fail(new[]
            {
                new ErrorItem
                {
                    FieldName = "MeterReplacement",
                    Error = ex.Message
                }
            });
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.MeterReplacements.Cancel)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin])]
    [SwaggerOperation(Summary = "Hủy biên bản treo tháo (chuyển sang CANCELLED)")]
    public async Task<RequestResult<MeterReplacementDto>> CancelAsync(
        [FromRoute] long id,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _meterReplacementService.CancelAsync(id, cancellationToken);
            return RequestResult<MeterReplacementDto>.Succeed(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi hủy biên bản treo tháo {Id}: {Message}", id, ex.Message);
            return RequestResult<MeterReplacementDto>.Fail(new[]
            {
                new ErrorItem
                {
                    FieldName = "MeterReplacement",
                    Error = ex.Message
                }
            });
        }
    }

    [HttpGet]
    [Route(Common.Url.Default.MeterReplacements.PointMeterInfo)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff])]
    [SwaggerOperation(Summary = "Lấy thông tin công tơ & chỉ số hiện tại của Điểm đo để tự động điền form lập biên bản")]
    public async Task<RequestResult<PointMeterInfoDto>> GetPointMeterInfoAsync(
        [FromRoute] long pointId,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _meterReplacementService.GetPointMeterInfoAsync(pointId, cancellationToken);
            if (result == null)
            {
                return RequestResult<PointMeterInfoDto>.Fail(new[]
                {
                    new ErrorItem
                    {
                        FieldName = "PointId",
                        Error = $"Không tìm thấy thông tin điểm đo ID {pointId}"
                    }
                });
            }

            return RequestResult<PointMeterInfoDto>.Succeed(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy thông tin công tơ điểm đo {PointId}: {Message}", pointId, ex.Message);
            return RequestResult<PointMeterInfoDto>.Fail(new[]
            {
                new ErrorItem
                {
                    FieldName = "MeterReplacement",
                    Error = ex.Message
                }
            });
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.MeterReplacements.UploadDocument)]
    [Route("api/v1/meter-replacements/upload-document")]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff])]
    [SwaggerOperation(Summary = "Tải lên tệp đính kèm biên bản hiện trường (PDF, JPG, PNG)")]
    public async Task<RequestResult<string>> UploadDocumentAsync(
        [FromForm(Name = "file")] IFormFile? file,
        CancellationToken cancellationToken)
    {
        try
        {
            var uploadFile = file ?? (Request.HasFormContentType ? Request.Form?.Files?["file"] ?? Request.Form?.Files?.FirstOrDefault() : null);
            if (uploadFile == null || uploadFile.Length == 0)
            {
                return RequestResult<string>.Fail(new[]
                {
                    new ErrorItem { FieldName = "File", Error = "Vui lòng chọn tệp tin cần tải lên." }
                });
            }

            var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
            var ext = Path.GetExtension(uploadFile.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(ext))
            {
                return RequestResult<string>.Fail(new[]
                {
                    new ErrorItem { FieldName = "File", Error = "Chỉ chấp nhận các tệp định dạng PDF hoặc hình ảnh (JPG, PNG)." }
                });
            }

            if (uploadFile.Length > 20 * 1024 * 1024)
            {
                return RequestResult<string>.Fail(new[]
                {
                    new ErrorItem { FieldName = "File", Error = "Dung lượng tệp không được vượt quá 20MB." }
                });
            }

            string uploadDir;
            try
            {
                uploadDir = Path.Combine(AppContext.BaseDirectory, "Uploads", "MeterReplacements");
                if (!Directory.Exists(uploadDir))
                {
                    Directory.CreateDirectory(uploadDir);
                }
            }
            catch
            {
                uploadDir = Path.Combine(Path.GetTempPath(), "MDMSv2", "MeterReplacements");
                if (!Directory.Exists(uploadDir))
                {
                    Directory.CreateDirectory(uploadDir);
                }
            }

            var uniqueFileName = $"{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}{ext}";
            var filePath = Path.Combine(uploadDir, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await uploadFile.CopyToAsync(stream, cancellationToken);
            }

            var fileUrl = $"/Meter-Replacements/Documents/{uniqueFileName}";
            return RequestResult<string>.Succeed(fileUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tải lên biên bản hiện trường: {Message}", ex.Message);
            return RequestResult<string>.Fail(new[]
            {
                new ErrorItem { FieldName = "File", Error = ex.Message }
            });
        }
    }

    [HttpGet]
    [Route(Common.Url.Default.MeterReplacements.ViewDocument)]
    [Route("api/v1/meter-replacements/documents/{fileName}")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Xem/tải về tệp biên bản hiện trường")]
    public IActionResult GetDocument([FromRoute] string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName) || fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            return BadRequest("Tên tệp không hợp lệ.");
        }

        var safeFileName = Path.GetFileName(fileName);
        var primaryPath = Path.Combine(AppContext.BaseDirectory, "Uploads", "MeterReplacements", safeFileName);
        var fallbackPath = Path.Combine(Path.GetTempPath(), "MDMSv2", "MeterReplacements", safeFileName);

        var filePath = System.IO.File.Exists(primaryPath) ? primaryPath : (System.IO.File.Exists(fallbackPath) ? fallbackPath : null);
        if (filePath == null)
        {
            return NotFound("Không tìm thấy tệp biên bản hiện trường.");
        }

        var ext = Path.GetExtension(safeFileName).ToLowerInvariant();
        var contentType = ext switch
        {
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            _ => "application/octet-stream"
        };

        return PhysicalFile(filePath, contentType, enableRangeProcessing: true);
    }

    [HttpPost]
    [Route(Common.Url.Default.MeterReplacements.ExtractOcr)]
    [Route("api/v1/meter-replacements/extract-ocr")]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff])]
    [SwaggerOperation(Summary = "Trích xuất chỉ số công tơ từ biên bản hiện trường bằng Gemini 1.5 Flash")]
    public async Task<RequestResult<ExtractedMeterDataDto>> ExtractOcrAsync(
        [FromBody] ExtractOcrRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var apiKey = !string.IsNullOrWhiteSpace(request?.ApiKey)
                ? request.ApiKey.Trim()
                : (_configuration["Gemini:ApiKey"] ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY"));

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return RequestResult<ExtractedMeterDataDto>.Fail(new[]
                {
                    new ErrorItem { FieldName = "ApiKey", Error = "Chưa cấu hình API Key Gemini. Vui lòng nhập khóa API trên giao diện hoặc cấu hình trong hệ thống." }
                });
            }

            string base64Data = string.Empty;
            string mimeType = "image/jpeg";

            if (!string.IsNullOrWhiteSpace(request?.ImageBase64))
            {
                var rawBase64 = request.ImageBase64;
                if (rawBase64.Contains(","))
                {
                    var parts = rawBase64.Split(new[] { ',' }, 2);
                    var header = parts[0];
                    base64Data = parts[1];

                    if (header.Contains("image/png")) mimeType = "image/png";
                    else if (header.Contains("image/webp")) mimeType = "image/webp";
                    else if (header.Contains("application/pdf")) mimeType = "application/pdf";
                    else mimeType = "image/jpeg";
                }
                else
                {
                    base64Data = rawBase64;
                }
            }
            else if (!string.IsNullOrWhiteSpace(request?.DocumentUrl))
            {
                var safeFileName = Path.GetFileName(request.DocumentUrl);
                var primaryPath = Path.Combine(AppContext.BaseDirectory, "Uploads", "MeterReplacements", safeFileName);
                var fallbackPath = Path.Combine(Path.GetTempPath(), "MDMSv2", "MeterReplacements", safeFileName);
                var filePath = System.IO.File.Exists(primaryPath) ? primaryPath : (System.IO.File.Exists(fallbackPath) ? fallbackPath : null);

                if (filePath == null)
                {
                    return RequestResult<ExtractedMeterDataDto>.Fail(new[]
                    {
                        new ErrorItem { FieldName = "DocumentUrl", Error = "Không tìm thấy tệp tài liệu để quét AI." }
                    });
                }

                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath, cancellationToken);
                base64Data = Convert.ToBase64String(fileBytes);

                var ext = Path.GetExtension(safeFileName).ToLowerInvariant();
                mimeType = ext switch
                {
                    ".pdf" => "application/pdf",
                    ".png" => "image/png",
                    _ => "image/jpeg"
                };
            }
            else
            {
                return RequestResult<ExtractedMeterDataDto>.Fail(new[]
                {
                    new ErrorItem { FieldName = "File", Error = "Vui lòng cung cấp ảnh cắt từ Canvas hoặc đường dẫn tài liệu." }
                });
            }

            var prompt = @"Bạn là trợ lý AI chuyên gia bóc tách số liệu biên bản treo tháo và bảng ghi chỉ số công tơ điện lực Việt Nam (AMR/MDMS).
Hãy đọc kỹ hình ảnh hoặc tài liệu đính kèm và trích xuất chính xác các thông tin đo đếm sang định dạng JSON sau:
{
  ""oldMeterSerial"": ""Số chế tạo công tơ tháo (hoặc Serial No nếu là bảng ghi 1 công tơ)"",
  ""oldMeterMultiplier"": 1,
  ""oldBt"": 0,
  ""oldCd"": 0,
  ""oldTd"": 0,
  ""oldSg"": 0,
  ""oldVc"": 0,
  ""oldPmax"": 0.00,
  ""newMeterSerial"": ""Số chế tạo công tơ treo (nếu có)"",
  ""newMeterMultiplier"": 1,
  ""newBt"": 0,
  ""newCd"": 0,
  ""newTd"": 0,
  ""newSg"": 0,
  ""newVc"": 0,
  ""newPmax"": 0.00,
  ""reason"": ""Lý do treo tháo hoặc ghi chú nếu có trong biên bản""
}
Quy tắc chuẩn hóa:
- Nếu là biên bản treo tháo có 2 công tơ (Công tơ tháo & Công tơ treo): điền đầy đủ vào cả nhóm old* và new*.
- Nếu tài liệu chỉ ghi nhận 1 công tơ (ví dụ Phiếu ghi chỉ số / Biên bản xác nhận chỉ số có cột Giờ bình thường / Normal hours kWh meter 1, Giờ cao điểm / Peak hours, Giờ thấp điểm / Off-peak hours, Tổng tác dụng AP, Tổng phản kháng AQ, Hệ số / Multiplier, Pmax): điền Serial No và bộ chỉ số mới nhất (Current meter reading) vào nhóm old* (Công tơ tháo), để newMeterSerial = """".
- oldBt / newBt = Bình thường (Normal hours / BT).
- oldCd / newCd = Cao điểm (Peak hours / CD).
- oldTd / newTd = Thấp điểm (Off-peak hours / TD).
- oldSg / newSg = Tổng hữu công / Tổng công suất tác dụng (SG / AP).
- oldVc / newVc = Tổng phản kháng (VC / AQ).
- oldMeterMultiplier / newMeterMultiplier: Hệ số nhân HSN (Multiplier, mặc định 1 nếu không ghi).
- Các chỉ số điện năng BT, CD, TD, SG, VC làm tròn đến hàng đơn vị (số nguyên).
- Pmax: Công suất cực đại lấy số thực 2 chữ số thập phân (VD: 7.52 hoặc 18.23).
- Chỉ trả về duy nhất chuỗi JSON hợp lệ, tuyệt đối không chèn thêm văn bản hay markdown.";

            var geminiRequest = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new object[]
                        {
                            new { text = prompt },
                            new
                            {
                                inline_data = new
                                {
                                    mime_type = mimeType,
                                    data = base64Data
                                }
                            }
                        }
                    }
                },
                generationConfig = new
                {
                    response_mime_type = "application/json",
                    temperature = 0.1
                }
            };

            var configuredModel = _configuration["Gemini:Model"];
            var modelsToTry = new System.Collections.Generic.List<string>();
            if (!string.IsNullOrWhiteSpace(configuredModel))
            {
                modelsToTry.Add(configuredModel.Trim());
            }
            foreach (var m in new[] { "gemini-2.5-flash", "gemini-3.8-flash", "gemini-3.5-flash-lite", "gemini-2.0-flash" })
            {
                if (!modelsToTry.Contains(m, StringComparer.OrdinalIgnoreCase))
                {
                    modelsToTry.Add(m);
                }
            }

            var jsonPayload = JsonSerializer.Serialize(geminiRequest);
            HttpResponseMessage? response = null;
            string responseString = string.Empty;
            bool allNotFound = true;

            foreach (var modelName in modelsToTry)
            {
                var geminiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{modelName}:generateContent";
                using var reqMsg = new HttpRequestMessage(HttpMethod.Post, geminiUrl);
                reqMsg.Headers.Add("x-goog-api-key", apiKey);
                reqMsg.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                response = await GeminiHttpClient.SendAsync(reqMsg, cancellationToken);
                responseString = await response.Content.ReadAsStringAsync(cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    allNotFound = false;
                    break;
                }

                if ((int)response.StatusCode == 404)
                {
                    _logger.LogWarning("Gemini model {Model} trả về 404, đang thử model kế tiếp...", modelName);
                    continue;
                }

                allNotFound = false;
                break;
            }

            // Nếu tất cả tên model mặc định đều 404, tự động truy vấn /v1beta/models để lấy tên model Flash khả dụng cho API Key này
            if (allNotFound)
            {
                using var listReq = new HttpRequestMessage(HttpMethod.Get, "https://generativelanguage.googleapis.com/v1beta/models");
                listReq.Headers.Add("x-goog-api-key", apiKey);
                var listRes = await GeminiHttpClient.SendAsync(listReq, cancellationToken);
                if (listRes.IsSuccessStatusCode)
                {
                    var listBody = await listRes.Content.ReadAsStringAsync(cancellationToken);
                    using var listDoc = JsonDocument.Parse(listBody);
                    if (listDoc.RootElement.TryGetProperty("models", out var modelsArray))
                    {
                        foreach (var item in modelsArray.EnumerateArray())
                        {
                            var name = item.TryGetProperty("name", out var n) ? n.GetString() : null;
                            var methods = item.TryGetProperty("supportedGenerationMethods", out var m)
                                ? m.EnumerateArray().Select(x => x.GetString()).ToList()
                                : new System.Collections.Generic.List<string?>();

                            if (!string.IsNullOrWhiteSpace(name) &&
                                name.Contains("flash", StringComparison.OrdinalIgnoreCase) &&
                                methods.Contains("generateContent"))
                            {
                                var dynamicModel = name.Replace("models/", "", StringComparison.OrdinalIgnoreCase);
                                var geminiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{dynamicModel}:generateContent";
                                using var reqMsg = new HttpRequestMessage(HttpMethod.Post, geminiUrl);
                                reqMsg.Headers.Add("x-goog-api-key", apiKey);
                                reqMsg.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                                response = await GeminiHttpClient.SendAsync(reqMsg, cancellationToken);
                                responseString = await response.Content.ReadAsStringAsync(cancellationToken);
                                if (response.IsSuccessStatusCode) break;
                            }
                        }
                    }
                }
            }

            if (response == null || !response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Gemini API error ({StatusCode}): {Body}", response?.StatusCode, responseString);
                var errorMsg = "Gọi API Gemini Flash thất bại.";
                try
                {
                    using var doc = JsonDocument.Parse(responseString);
                    if (doc.RootElement.TryGetProperty("error", out var errorProp) &&
                        errorProp.TryGetProperty("message", out var msgProp))
                    {
                        errorMsg = $"Lỗi từ Gemini AI: {msgProp.GetString()}";
                    }
                }
                catch {}

                return RequestResult<ExtractedMeterDataDto>.Fail(new[]
                {
                    new ErrorItem { FieldName = "Gemini", Error = errorMsg }
                });
            }

            using var resDoc = JsonDocument.Parse(responseString);
            var candidates = resDoc.RootElement.GetProperty("candidates");
            if (candidates.GetArrayLength() == 0)
            {
                return RequestResult<ExtractedMeterDataDto>.Fail(new[]
                {
                    new ErrorItem { FieldName = "Gemini", Error = "Gemini không nhận diện được dữ liệu từ ảnh." }
                });
            }

            var textContent = candidates[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            if (string.IsNullOrWhiteSpace(textContent))
            {
                return RequestResult<ExtractedMeterDataDto>.Fail(new[]
                {
                    new ErrorItem { FieldName = "Gemini", Error = "Nội dung phản hồi từ Gemini rỗng." }
                });
            }

            var cleanJson = textContent.Trim();
            if (cleanJson.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
            {
                cleanJson = cleanJson.Substring(7);
            }
            if (cleanJson.StartsWith("```"))
            {
                cleanJson = cleanJson.Substring(3);
            }
            if (cleanJson.EndsWith("```"))
            {
                cleanJson = cleanJson.Substring(0, cleanJson.Length - 3);
            }
            cleanJson = cleanJson.Trim();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
            };

            var extractedData = JsonSerializer.Deserialize<ExtractedMeterDataDto>(cleanJson, options) ?? new ExtractedMeterDataDto();
            extractedData.RawAiResponse = cleanJson;

            return RequestResult<ExtractedMeterDataDto>.Succeed(extractedData);
        }
        catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException || ex is SocketException || ex.InnerException is SocketException || ex.InnerException is TimeoutException)
        {
            _logger.LogWarning(ex, "Máy chủ không kết nối được tới Google Gemini API: {Message}", ex.Message);
            return RequestResult<ExtractedMeterDataDto>.Fail(new[]
            {
                new ErrorItem { FieldName = "Gemini", Error = $"[NET_CONNECT_FAILED] Máy chủ không thể kết nối ra Internet (Google Gemini API): {ex.Message}" }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi trích xuất dữ liệu bằng Gemini AI: {Message}", ex.Message);
            return RequestResult<ExtractedMeterDataDto>.Fail(new[]
            {
                new ErrorItem { FieldName = "Gemini", Error = $"Lỗi xử lý AI: {ex.Message}" }
            });
        }
    }
}
