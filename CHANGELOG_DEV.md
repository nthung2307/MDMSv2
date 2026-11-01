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

## 2. Hoàn thành Phase 2: Đa Mẫu Email Thông Báo & Phiếu Báo PDF QuestPDF

### 2.1. Backend (.NET 8 Clean Architecture)
1. **Quản lý Mẫu Email Thông báo (`NotificationTemplates`)**:
   - [NotificationTemplateDto.cs](file:///e:/sources/MDMSv2/Backend/Application/DataTransferObjects/Templates/NotificationTemplateDto.cs): Khai báo hợp đồng CRUD, Preview request/response cho các loại mẫu: `STATEMENT_NOTIFICATION`, `FORGOT_PASSWORD`, `USER_CREDENTIAL`.
   - [INotificationTemplateService.cs](file:///e:/sources/MDMSv2/Backend/Application/Services/INotificationTemplateService.cs) & [NotificationTemplateService.cs](file:///e:/sources/MDMSv2/Backend/Infrastructure/Services/Templates/NotificationTemplateService.cs): Bộ parser linh hoạt Regex biên dịch `{{Key}}` (không phân biệt chữ hoa/thường, tự động loại bỏ khoảng trắng dư thừa, hỗ trợ format specifier, tích hợp bộ fallback templates mặc định sẵn có).
   - [NotificationTemplatesController.cs](file:///e:/sources/MDMSv2/Backend/WebApi/Controllers/NotificationTemplatesController.cs): Cung cấp RESTful API đầy đủ CRUD, thiết lập mẫu mặc định (`/set-default`), và xem trước email động (`/preview`).
2. **Quản lý Mẫu Phiếu Báo PDF Modular (`StatementPdfTemplates`)**:
   - [StatementPdfTemplateDto.cs](file:///e:/sources/MDMSv2/Backend/Application/DataTransferObjects/Templates/StatementPdfTemplateDto.cs): Định nghĩa profile cấu hình phiếu báo PDF: khổ giấy (`A5_LANDSCAPE` 595x420 pt chuẩn VSIP hoặc `A4_PORTRAIT` 595x842 pt chuẩn kế toán), tùy biến tiêu đề, tên/địa chỉ/SĐT đơn vị phát hành (Bên A), upload logo Base64, danh sách cột hiển thị linh hoạt dạng JSON (`BT`, `CD`, `TD`, `SG`, `VC`, `PMAX`, `COSPHI`), tùy chọn 1 chữ ký (Bên A) hoặc 2 chữ ký song song (Bên A & Bên B), ghi chú chân trang.
   - [IStatementPdfGenerator.cs](file:///e:/sources/MDMSv2/Backend/Application/Services/IStatementPdfGenerator.cs) & [StatementPdfGenerator.cs](file:///e:/sources/MDMSv2/Backend/Infrastructure/Services/Templates/StatementPdfGenerator.cs): Hiện thực hóa render PDF bằng thư viện QuestPDF chất lượng cao, chia lưới bảng thông số chính xác, tự động tính toán tỷ lệ theo khổ giấy và render tức thời.
   - [StatementPdfTemplatesController.cs](file:///e:/sources/MDMSv2/Backend/WebApi/Controllers/StatementPdfTemplatesController.cs): Cung cấp các API CRUD, `/set-default`, và `/preview-pdf` trả về luồng nhị phân `application/pdf` phục vụ xem trước trực tiếp trên trình duyệt.
3. **Tích hợp Tự động hóa Gửi Phiếu báo**:
   - [EmailSenderService.cs](file:///e:/sources/MDMSv2/Backend/Infrastructure/Services/EmailSenderService.cs): Được tái cấu trúc để nạp mẫu PDF và mẫu Email mặc định từ cơ sở dữ liệu. Tạo file PDF động qua `IStatementPdfGenerator` và biên dịch tiêu đề/nội dung qua `INotificationTemplateService`, sau đó gửi đính kèm file PDF qua `IEmailProviderFactory`.
   - [EmailService.cs](file:///e:/sources/MDMSv2/Backend/Infrastructure/Services/EmailService.cs): Kết nối với `NotificationTemplateService` cho các luồng gửi email tài khoản và quên mật khẩu.
   - [Program.cs](file:///e:/sources/MDMSv2/Backend/WebApi/Program.cs): Đăng ký bản quyền QuestPDF Community License và đăng ký DI cho `INotificationTemplateService` và `IStatementPdfGenerator`.

---

### 2.2. Frontend (React 18 + TypeScript + Ant Design 5)
1. **Routing, Menu & Navigation**:
   - [MenuKeys.ts](file:///e:/sources/MDMSv2/Frontend/src/core/domain/enums/MenuKeys.ts): Khai báo menu keys `NotificationTemplates` và `StatementPdfTemplates`.
   - [appRouters.ts](file:///e:/sources/MDMSv2/Frontend/src/core/application/common/appRouters.ts): Thêm routes `/notification-templates` và `/statement-pdf-templates`.
   - [constants.ts](file:///e:/sources/MDMSv2/Frontend/src/core/application/common/constants.ts): Bổ sung 2 mục menu "Mẫu Email thông báo" và "Mẫu Phiếu báo PDF" dưới nhóm "Cài đặt hệ thống".
   - [admin/index.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/admin/index.tsx): Đăng ký Route bảo vệ cho cả 2 màn hình.
2. **Services & Repositories**:
   - [templateManagement.interface.ts](file:///e:/sources/MDMSv2/Frontend/src/core/application/interfaces/methods/templates/templateManagement.interface.ts): Định nghĩa kiểu dữ liệu và request contract đầy đủ.
   - [templateManagement.service.ts](file:///e:/sources/MDMSv2/Frontend/src/infrastructure/repository/templates/services/templateManagement.service.ts): Tầng dịch vụ gọi API, xử lý nhận file PDF dưới dạng `Blob` cho việc xem trước trực tiếp.
3. **Màn hình Quản trị Mẫu Email Thông Báo**:
   - [notification-templates/index.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/config/notification-templates/index.tsx):
     * Giao diện chia Tabs theo loại mẫu: Thông báo phiếu báo, Quên mật khẩu, Cấp tài khoản.
     * Bảng danh sách mẫu kèm trạng thái Mặc định (Default Tag với ngôi sao vàng), thao tác Đặt làm mặc định, Sửa, Xóa.
     * Drawer thêm mới/chỉnh sửa mẫu với bảng thẻ Merge Tags thông minh (bấm vào thẻ là tự động chèn vào vị trí con trỏ).
     * Modal Live Preview xem trước email thực tế sau khi bind biến dữ liệu.
4. **Màn hình Quản trị Mẫu Phiếu Báo PDF**:
   - [statement-pdf-templates/index.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/config/statement-pdf-templates/index.tsx):
     * Bảng danh sách Profile mẫu phiếu báo kèm Tag khổ giấy A5/A4, số lượng cột hiển thị, trạng thái mặc định.
     * Drawer cấu hình toàn diện: Radio button chọn khổ giấy A5 Ngang / A4 Đứng, Checkbox danh sách cột hiển thị (BT, CD, TD, SG, VC, PMAX, COSPHI), Switch chuyển đổi chế độ ký xác nhận (Chỉ bên A vs Hai bên A & B), Upload Logo kèm xem trước.
     * Modal Live Preview xem file PDF trực tiếp thông qua thẻ iframe với dữ liệu render từ backend trong thời gian thực, có nút tải file PDF mẫu về máy.

---

### 2.3. Cơ sở dữ liệu (Database Scripts)
- Đã cập nhật [amr_full.sql](file:///e:/sources/MDMSv2/Installers/Database/amr_full.sql) và [20261031_create_email_config_and_metering_tables.sql](file:///e:/sources/MDMSv2/Installers/Database/20261031_create_email_config_and_metering_tables.sql) (đồng bộ cả thư mục `Publish/MDMSv2_InstallPackage/Database`):
  * DDL bảng `NotificationTemplates` & `StatementPdfTemplates`.
  * DDL bảng `MeterReplacements` (sẵn sàng cho Phase 3).
  * Khởi tạo sẵn các mẫu mặc định: Mẫu email gửi phiếu báo VSIP Hải Phòng, mẫu email quên mật khẩu, mẫu email tạo tài khoản, mẫu phiếu báo PDF A5 Ngang (VSIP Hải Phòng) và mẫu A4 Đứng.

---

## 3. Hướng dẫn Kiểm thử (Test Cases for MDMS-Tester)

| STT | Kịch bản kiểm thử | Dữ liệu đầu vào | Kết quả kỳ vọng |
|---|---|---|---|
| 1 | Truy cập trang Cấu hình Email | Đăng nhập tài khoản `SuperAdmin` $\to$ Cài đặt hệ thống $\to$ Cấu hình Email | Màn hình hiển thị đầy đủ các lựa chọn nhà cung cấp và thông tin cấu hình hiện hành |
| 2 | Kiểm tra kết nối Microsoft Graph API với VSIP HP | Chọn `Microsoft 365`, nhập thông số của VSIP HP (Tenant ID, Client ID, Secret, `noreply.hoadondientu.hp@vsip.com.vn`), nhập email nhận test | Gửi thư thành công, hộp thư nhận được email test từ `noreply.hoadondientu.hp@vsip.com.vn` |
| 3 | Lưu cấu hình Microsoft Graph API | Bấm nút "Lưu cấu hình" | Thông báo thành công, dữ liệu được lưu vào bảng `SystemEmailConfigs` |
| 4 | Quản lý Mẫu Email Thông Báo | Cài đặt hệ thống $\to$ Mẫu Email thông báo | Hiển thị các tab mẫu `STATEMENT_NOTIFICATION`, `FORGOT_PASSWORD`, `USER_CREDENTIAL`. Chức năng sửa mẫu, bấm chèn merge tag hoạt động chuẩn xác |
| 5 | Live Preview Mẫu Email | Tại form sửa mẫu email, bấm "Xem trước" | Modal mở ra hiển thị nội dung HTML đã được bind các biến dữ liệu giả lập thực tế |
| 6 | Đặt Mẫu Email Mặc Định | Bấm nút "Đặt làm mặc định" trên bảng mẫu | Icon ngôi sao vàng sáng lên, mẫu trở thành mặc định và các mẫu cùng loại bị hủy mặc định |
| 7 | Quản lý Mẫu Phiếu Báo PDF | Cài đặt hệ thống $\to$ Mẫu Phiếu báo PDF | Danh sách mẫu A5/A4 hiển thị rõ ràng các cột được cấu hình bật/tắt |
| 8 | Live Preview Phiếu Báo PDF | Bấm "Xem trước PDF" trên danh sách mẫu hoặc trong Drawer chỉnh sửa | Iframe hiển thị file PDF render thời gian thực từ QuestPDF đúng chuẩn kích thước A5/A4 và các cột đã chọn |
| 9 | Kiểm tra gửi phiếu báo tự động | Chạy tiến trình gửi phiếu báo hàng tháng | Email được gửi qua Microsoft Graph API đính kèm file PDF định dạng theo template mặc định |

---

## 4. Hoàn tất & Đóng gói (Sign-Off Status)
- **Phase 1 (Nền tảng Email Đa Dịch vụ):** Hoàn thành 100%.
- **Phase 2 (Đa Mẫu Email & Phiếu Báo PDF Modular QuestPDF):** Hoàn thành 100%.
- **Biên dịch:** Backend và Frontend đều đạt 0 lỗi (`0 Error(s)`).
- **Cơ sở dữ liệu:** Sẵn sàng cho triển khai mới hoặc cập nhật thông qua script migrations.
- **Tiếp theo:** Sẵn sàng bước vào **Phase 3 (Nghiệp vụ Treo / Tháo Công Tơ và Tính Sản Lượng Phân Đoạn Kỳ Chốt)**.

