# [DEV HANDOFF] NỀN TẢNG DỊCH VỤ EMAIL DOANH NGHIỆP (MICROSOFT GRAPH API OAUTH 2.0 & SMTP/GMAIL), QUẢN LÝ CẤU HÌNH ĐỘNG VÀ ĐẶC TẢ ĐO ĐẾM CÔNG TƠ EDMI MK6

> **Tài liệu bàn giao phát triển (Developer Handoff Changelog)**  
> **Phiên bản:** v2.1.0-ENTERPRISE  
> **Người thực hiện:** MDMS-Coder (Lead Full-Stack Developer)  
> **Người tiếp nhận:** MDMS-Tester (Lead QA Engineer) & MDMS-Reviewer (Principal Architect)  
> **Căn cứ thực hiện:** Bản thiết kế kỹ thuật [PLAN_SPEC.md](file:///e:/sources/MDMSv2/PLAN_SPEC.md).

---

## 1. Tóm tắt Thay đổi (Summary of Changes)
Đã hoàn thành 100% Phase 1 theo đúng kế hoạch:

### 1.1. Backend (.NET 8 Clean Architecture)
1. **Domain Layer (Entities & Models)**:
   - [SystemEmailConfigEntity.cs](file:///e:/sources/MDMSv2/Backend/Domain/Entities/SystemEmailConfigEntity.cs): Định nghĩa bảng cấu hình máy chủ gửi email đa nền tảng (`ProviderType`: `MICROSOFT_GRAPH`, `CUSTOM_SMTP`, `GMAIL`), lưu thông số Tenant ID, Client ID, Client Secret (OAuth 2.0) và thông số máy chủ SMTP (Host, Port, SecurityMode, RequireAuth, Username, Password).
   - [NotificationTemplateEntity.cs](file:///e:/sources/MDMSv2/Backend/Domain/Entities/NotificationTemplateEntity.cs): Định nghĩa bảng lưu trữ các mẫu email thông báo tùy biến (`USER_CREDENTIAL`, `FORGOT_PASSWORD`, `STATEMENT_NOTIFICATION`).
   - [StatementPdfTemplateEntity.cs](file:///e:/sources/MDMSv2/Backend/Domain/Entities/StatementPdfTemplateEntity.cs): Định nghĩa bảng cấu hình Profile mẫu Phiếu báo PDF QuestPDF đa dạng (Logo, Tiêu đề, Khổ giấy A4/A5, Cột hiển thị $P_{max}$, $Cos\varphi$, $AQ$).
   - [MeterReplacementEntity.cs](file:///e:/sources/MDMSv2/Backend/Domain/Entities/MeterReplacementEntity.cs): Định nghĩa bảng ghi nhận Biên bản Treo/Tháo công tơ phục vụ tính sản lượng phân đoạn.
   - [PeriodicEnergyStatementEntity.cs](file:///e:/sources/MDMSv2/Backend/Domain/Entities/PeriodicEnergyStatementEntity.cs): Bổ sung các trường $P_{max}$, $P_{max}Time$, $BillingTimestamp$, $HasMeterReplacement$, $MeterReplacementId$, $StatementPdfTemplateId$.
2. **Infrastructure Layer (EF Core & Email Providers)**:
   - Các EntityTypeConfigurations: [SystemEmailConfigEntityConfiguration.cs](file:///e:/sources/MDMSv2/Backend/Infrastructure/Databases/SystemEmailConfigEntityConfiguration.cs), [NotificationTemplateEntityConfiguration.cs](file:///e:/sources/MDMSv2/Backend/Infrastructure/Databases/NotificationTemplateEntityConfiguration.cs), [StatementPdfTemplateEntityConfiguration.cs](file:///e:/sources/MDMSv2/Backend/Infrastructure/Databases/StatementPdfTemplateEntityConfiguration.cs), [MeterReplacementEntityConfiguration.cs](file:///e:/sources/MDMSv2/Backend/Infrastructure/Databases/MeterReplacementEntityConfiguration.cs) và cập nhật quan hệ trong [PeriodicEnergyStatementEntityConfiguration.cs](file:///e:/sources/MDMSv2/Backend/Infrastructure/Databases/PeriodicEnergyStatementEntityConfiguration.cs).
   - Repositories: [SystemEmailConfigReadWriteRepository.cs](file:///e:/sources/MDMSv2/Backend/Infrastructure/Repositories/ReadWrite/SystemEmailConfigReadWriteRepository.cs) và [SystemEmailConfigReadOnlyRepository.cs](file:///e:/sources/MDMSv2/Backend/Infrastructure/Repositories/ReadOnly/SystemEmailConfigReadOnlyRepository.cs).
   - **Microsoft Graph API Email Provider**:
     * [MicrosoftGraphEmailProvider.cs](file:///e:/sources/MDMSv2/Backend/Infrastructure/Services/EmailProviders/MicrosoftGraphEmailProvider.cs): Tích hợp trực tiếp với Microsoft Entra ID (Azure AD) thông qua OAuth 2.0 Client Credentials Grant Flow (`POST https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token`). Tự động cache token trong bộ nhớ MemoryCache, gửi thư kèm file đính kèm Base64 qua Endpoint `POST https://graph.microsoft.com/v1.0/users/{senderEmail}/sendMail`. Cung cấp phương thức `TestConnectionAsync` gửi email kiểm tra kết nối thời gian thực.
   - **Custom SMTP & Gmail Provider**:
     * [SmtpEmailProvider.cs](file:///e:/sources/MDMSv2/Backend/Infrastructure/Services/EmailProviders/SmtpEmailProvider.cs): Chuẩn hóa trên thư viện `MailKit`, hỗ trợ các phương thức mã hóa (STARTTLS, SSL/TLS, None cho Internal Relay IP), hỗ trợ xác thực tài khoản hoặc gửi không xác thực.
   - **Email Provider Factory**:
     * [EmailProviderFactory.cs](file:///e:/sources/MDMSv2/Backend/Infrastructure/Services/EmailProviders/EmailProviderFactory.cs): Tự động đọc cấu hình kích hoạt trong cơ sở dữ liệu để khởi tạo Provider phù hợp, có cơ chế Fallback cấu hình mặc định trong `appsettings.json`.
   - **Cập nhật Dịch vụ Gửi Mail**:
     * [EmailService.cs](file:///e:/sources/MDMSv2/Backend/Infrastructure/Services/EmailService.cs): Chuyển đổi toàn bộ sang `IEmailProviderFactory`, giúp các luồng gửi email tài khoản khởi tạo và đặt lại mật khẩu tự động hỗ trợ ngay lập tức Microsoft 365 của VSIP Hải Phòng.
3. **Application Layer (DTOs & Contracts)**:
   - [Constants.cs](file:///e:/sources/MDMSv2/Backend/Application/Common/Constants.cs): Khai báo tên bảng và các hằng số `EmailProvider`, `NotificationTemplateCode`.
   - [IEmailProviderService.cs](file:///e:/sources/MDMSv2/Backend/Application/Services/IEmailProviderService.cs) & [IEmailProviderFactory.cs](file:///e:/sources/MDMSv2/Backend/Application/Services/IEmailProviderFactory.cs): Abstraction cho tầng dịch vụ email.
   - [EmailMessageDto.cs](file:///e:/sources/MDMSv2/Backend/Application/DataTransferObjects/Email/EmailMessageDto.cs) & [EmailAttachmentDto.cs](file:///e:/sources/MDMSv2/Backend/Application/DataTransferObjects/Email/EmailAttachmentDto.cs): Hợp đồng dữ liệu thư điện tử dùng chung.
   - Các DTO cấu hình: [UpdateSystemEmailConfigRequest.cs](file:///e:/sources/MDMSv2/Backend/Application/DataTransferObjects/Requests/EmailConfig/UpdateSystemEmailConfigRequest.cs), [TestEmailConfigRequest.cs](file:///e:/sources/MDMSv2/Backend/Application/DataTransferObjects/Requests/EmailConfig/TestEmailConfigRequest.cs), [SystemEmailConfigResponse.cs](file:///e:/sources/MDMSv2/Backend/Application/DataTransferObjects/Responses/EmailConfig/SystemEmailConfigResponse.cs).
4. **WebApi Layer (Controllers & Routes)**:
   - [Url.cs](file:///e:/sources/MDMSv2/Backend/WebApi/Common/Url.cs): Khai báo route `EmailConfig` (`Get-Active-Email-Config`, `Update-Email-Config`, `Test-Connection`).
   - [EmailConfigController.cs](file:///e:/sources/MDMSv2/Backend/WebApi/Controllers/EmailConfigController.cs): Cung cấp các RESTful API phục vụ quản trị cấu hình email với phân quyền `SuperAdmin` và `Admin`.
   - [Program.cs](file:///e:/sources/MDMSv2/Backend/WebApi/Program.cs): Đăng ký `AddMemoryCache()` và DI `IEmailProviderFactory`.

---

### 1.2. Frontend (React 18 + TypeScript + Ant Design 5)
1. **Routing & Menu**:
   - [appRouters.ts](file:///e:/sources/MDMSv2/Frontend/src/core/application/common/appRouters.ts): Khai báo route `ROUTE_PATH.EMAIL_CONFIG = '/email-config'`.
   - [endPoint.ts](file:///e:/sources/MDMSv2/Frontend/src/core/application/common/endPoint.ts): Khai báo các endpoint `GetActiveEmailConfig`, `UpdateEmailConfig`, `TestEmailConnection`.
   - [MenuKeys.ts](file:///e:/sources/MDMSv2/Frontend/src/core/domain/enums/MenuKeys.ts): Bổ sung `EmailConfig = 'email-config'`.
   - [constants.ts](file:///e:/sources/MDMSv2/Frontend/src/core/application/common/constants.ts): Thêm mục "Cấu hình Email" vào cụm menu "Cài đặt hệ thống".
   - [admin/index.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/admin/index.tsx): Đăng ký PrivateRoute cho trang Cấu hình Email.
2. **Services & Hooks**:
   - [configManagement.interface.ts](file:///e:/sources/MDMSv2/Frontend/src/core/application/interfaces/methods/tag-name/configManagement.interface.ts) & [configManagement.service.ts](file:///e:/sources/MDMSv2/Frontend/src/infrastructure/repository/config/services/configManagement.service.ts): Khai báo và hiện thực hóa các phương thức gọi API cấu hình email.
   - [useEmailConfigHook.tsx](file:///e:/sources/MDMSv2/Frontend/src/infrastructure/repository/config/hooks/useEmailConfigHook.tsx): Custom hook quản lý trạng thái tải, lưu và kiểm tra kết nối email.
3. **Màn hình Giao diện Quản trị**:
   - [email/index.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/config/email/index.tsx): Màn hình Quản trị Cấu hình Email Doanh nghiệp cao cấp:
     * Radio Card chọn 3 nhà cung cấp: **Microsoft 365 (Graph API OAuth 2.0)**, **Custom SMTP** và **Gmail**.
     * Form nhập linh hoạt theo loại dịch vụ đã chọn (với Microsoft 365: Tenant ID, Client ID, Client Secret, Email người gửi; với SMTP: Host, Port, Security Mode, Username, Password).
     * Card **"Kiểm tra kết nối (Test Connection)"**: Nhập email nhận bất kỳ và bấm gửi thử nghiệm, phản hồi trạng thái tức thời kèm thông báo lỗi rõ ràng.
     * Nút **"Lưu cấu hình"**: Lưu trực tiếp vào Database máy chủ.

---

## 2. Hướng dẫn Kiểm thử (Test Cases for MDMS-Tester)

| STT | Kịch bản kiểm thử | Dữ liệu đầu vào | Kết quả kỳ vọng |
|---|---|---|---|
| 1 | Truy cập trang Cấu hình Email | Đăng nhập tài khoản `SuperAdmin` $\to$ Cài đặt hệ thống $\to$ Cấu hình Email | Màn hình hiển thị đầy đủ các lựa chọn nhà cung cấp và thông tin cấu hình hiện hành |
| 2 | Kiểm tra kết nối Microsoft Graph API với VSIP HP | Chọn `Microsoft 365`, nhập thông số của VSIP HP (Tenant ID, Client ID, Secret, `noreply.hoadondientu.hp@vsip.com.vn`), nhập email nhận test | Gửi thư thành công, hộp thư nhận được email test từ `noreply.hoadondientu.hp@vsip.com.vn` |
| 3 | Lưu cấu hình Microsoft Graph API | Bấm nút "Lưu cấu hình" | Thông báo thành công, dữ liệu được lưu vào bảng `SystemEmailConfigs` |
| 4 | Kiểm tra gửi email mật khẩu / reset mật khẩu | Thực hiện tính năng cấp lại mật khẩu cho tài khoản | Email reset mật khẩu được gửi tự động qua Microsoft Graph API thay vì Gmail cũ |
| 5 | Kiểm tra kết nối SMTP / Gmail | Chọn `Custom SMTP` hoặc `Gmail`, nhập Host, Port, tài khoản $\to$ Bấm kiểm tra kết nối | Thư kiểm tra gửi thành công qua máy chủ SMTP |
| 6 | Kiểm tra phân quyền và cách ly dữ liệu Khách hàng | Đăng nhập tài khoản vai trò `Customer` (ví dụ `hungnt` liên kết `KYOCERA`) | Chỉ xem được các điểm đo, sự kiện, cảnh báo thuộc khách hàng đã liên kết; không xuất hiện điểm đo của khách hàng khác (`AUTEL ENERGY`) |
| 7 | Kiểm tra xóa liên kết tài khoản Khách hàng | Trên trang Chỉnh sửa khách hàng, bấm nút (x) xóa tài khoản liên kết | Thẻ tài khoản biến mất tức thì trên giao diện và đồng bộ form thành công |
| 8 | Kiểm tra hiển thị Trạng thái kết nối | Vào trang "Thông số tổng hợp" | Tiêu đề cột hiển thị "Trạng thái kết nối"; hiển thị "Kết nối" (0), "Mất kết nối" (1), hoặc "-" (chưa cấu hình hoặc REST API chưa cập nhật) |

---

## 3. Hoàn tất & Đóng gói Phase 1 (Phase 1 Sign-Off)
- **Tình trạng:** Hoàn thành 100% các tiêu chí nghiệm thu của Phase 1.
- **Biên dịch:** Backend và Frontend đều đạt 0 lỗi (`0 Error(s)`).
- **Cơ sở dữ liệu:** Tự động đồng bộ cấu trúc bảng `SystemEmailConfigs` và các bảng dữ liệu qua `Update_Hotfix.ps1`.
