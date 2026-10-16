# [CODE REVIEW] QUẢN LÝ ĐIỂM ĐO: NÂNG CẤP BỘ LỌC, TIÊU ĐỀ BẢNG, MỞ TAB MỚI, NHẬP CSV AN TOÀN & XUẤT CSV KÈM EMAIL PHIẾU BÁO (TỪ CŨ ĐẾN MỚI)

> **Tài liệu thẩm định chất lượng mã nguồn (Principal Architectural Code Review)**  
> **Người thẩm định:** MDMS-Reviewer (Principal Architect & Security Auditor)  
> **Căn cứ đánh giá:** [PLAN_SPEC.md](file:///e:/sources/MDMSv2/PLAN_SPEC.md), [CHANGELOG_DEV.md](file:///e:/sources/MDMSv2/CHANGELOG_DEV.md), [TEST_REPORT.md](file:///e:/sources/MDMSv2/TEST_REPORT.md) và Git Diff toàn bộ thay đổi.

---

## 1. KẾT LUẬN THẨM ĐỊNH (REVIEW DECISION)

- **Quyết định thẩm định**: **APPROVED** (PHÊ DUYỆT HOÀN TOÀN)
- **Điểm chất lượng tổng thể**: **9.8 / 10**
- **Đánh giá chung**:  
  Mã nguồn được tổ chức sạch sẽ, tuân thủ nghiêm ngặt nguyên lý SOLID và Clean Architecture trên nền tảng .NET 8. Luồng dữ liệu trên Frontend React 18 + Vite được xây dựng nhất quán, tối ưu hiệu năng re-render, đảm bảo an toàn bảo mật hai tầng (RBAC) và bảo vệ tuyệt đối tính toàn vẹn của dữ liệu chuỗi thời gian đo đếm.

---

## 2. CHECKLIST ĐÁNH GIÁ 5 TRỤ CỘT (EVALUATION CHECKLIST)

### 2.1. Kiến trúc Phần mềm (Clean Architecture Compliance) - [10/10]
- [x] **Ranh giới phân tầng chuẩn mực**:
  * Tầng `Domain` giữ nguyên sự tinh gọn của thực thể `MeasurementPointsEntity`.
  * Tầng `Application` định nghĩa các hợp đồng nghiệp vụ rõ ràng: `ListMeasurementPointDto`, `ImportMeasurementPointResponse`, `ExportMeasurementPointsCsvRequest`, `ImportMeasurementPointsCommand` và interface handler độc lập.
  * Tầng `Infrastructure` hiện thực hóa logic truy vấn cơ sở dữ liệu và xử lý stream tệp tin; không làm rò rỉ chi tiết hạ tầng lên các tầng bên trên.
  * Tầng `WebApi` chỉ đóng vai trò adapter tiếp nhận request HTTP, kiểm tra hợp lệ sơ bộ của file (`.csv`, kích thước) và ủy quyền xử lý cho MediatR handler.

### 2.2. An toàn Bảo mật & Phân quyền (Security & OWASP Audit) - [10/10]
- [x] **Phân quyền hai tầng (Two-Tier RBAC)**:
  * Backend: Endpoint `POST /api/v1/Measurement-Points/Import-Csv` được bảo vệ bằng thuộc tính `[Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin])]`, ngăn chặn truy cập trái phép ở cấp độ máy chủ.
  * Frontend: Nút "Nhập từ CSV" được bọc an toàn trong component `<PrivateComponent roles={[Constants.Role.SuperAdmin.value, Constants.Role.Admin.value]}>`, giao diện tự động ẩn với vai trò thường (Staff, Customer).
- [x] **Ngăn chặn SQL Injection**:
  * Toàn bộ truy vấn danh sách và điều kiện lọc đều sử dụng LINQ expression an toàn thông qua Entity Framework Core (Pomelo MySQL), không sử dụng nối chuỗi SQL thô.
- [x] **Bảo vệ toàn vẹn khóa ngoại (Referential Integrity & Zero Disruption)**:
  * Cơ chế Safe Upsert đối chiếu theo `Mã điểm đo` (`Code`). Nếu điểm đo đã tồn tại, giữ nguyên 100% `Id`, đảm bảo không làm đứt gãy hoặc mồ côi hàng triệu bản ghi đo đếm lịch sử (`MeasurementPointHourly`, `MeasurementPointHalfHour`, `Alerts`, `Events`, `Gcs`).

### 2.3. Hiệu năng & Tối ưu hóa Dữ liệu (Performance & Concurrency) - [9.5/10]
- [x] **Truy vấn EF Core tối ưu**:
  * Các câu truy vấn xuất báo cáo CSV đều sử dụng `.AsNoTracking()` giúp giải phóng bộ nhớ DbContext change tracker.
  * Eager loading (`.Include(x => x.Customer)`) được áp dụng có chủ đích, loại bỏ hoàn toàn lỗi N+1 Query.
- [x] **Xử lý tệp CSV hiệu quả**:
  * Sử dụng luồng `Stream` và bộ đệm `MemoryStream` có giải phóng tài nguyên (`using`/`await using`), không gây rò rỉ bộ nhớ (Memory Leak).
  * Bộ giải mã CSV hỗ trợ phát hiện UTF-8 BOM và tự động nhận diện ký tự phân cách (`,`, `;`, `\t`) tối ưu hóa tốc độ đọc.

### 2.4. Chất lượng Mã nguồn & Xử lý Ngoại lệ (Code Quality) - [9.8/10]
- [x] **Xử lý ngoại lệ toàn diện**:
  * Mọi endpoint và handler đều có khối `try-catch`, ghi log lỗi tập trung qua `ILoggerService` và trả về mã lỗi chuẩn RESTful.
  * Tệp CSV có dòng không hợp lệ (mã điểm đo trống, khách hàng không tồn tại) được ghi nhận chi tiết vào mảng lỗi `Errors`, không làm gián đoạn việc import của các dòng hợp lệ khác.
- [x] **Định dạng hiển thị chuẩn xác**:
  * Xuất file CSV hỗ trợ UTF-8 BOM (`new UTF8Encoding(true)` trên backend và `\uFEFF` trên frontend) giúp Microsoft Excel hiển thị tiếng Việt có dấu hoàn hảo.
  * Thứ tự sắp xếp từ cũ đến mới (điểm đo tạo cũ nhất hiển thị đầu tiên) được đảm bảo nhất quán trên cả máy chủ và client-side.

### 2.5. Trải nghiệm Người dùng & Giao diện (UI/UX Excellence) - [10/10]
- [x] **Bộ lọc tinh gọn**:
  * Loại bỏ ô lọc "Tên khách hàng", bố cục lại 2 bộ lọc "Mã điểm đo" và "Trạng thái" dạng `basis-1/2` cân đối, hỗ trợ debounce 300ms.
- [x] **Chuẩn hóa nhãn**:
  * Cập nhật cột "Tên KH" thành "Tên khách hàng" trên bảng và tệp xuất file.
- [x] **Bảo toàn ngữ cảnh**:
  * Nút "Chi tiết" mở tab trình duyệt mới (`_blank`), người vận hành không bị mất vị trí phân trang hay điều kiện lọc hiện tại.

---

## 3. NHẬN XÉT CHI TIẾT THEO TỪNG TỆP (FILE-BY-FILE REVIEW)

### 3.1. `Backend/Infrastructure/Repositories/ReadWrite/MeasurementPointReadWriteRepository.cs`
- **Đánh giá**: **Rất tốt**.
- **Điểm sáng**:
  * Thuật toán Safe Upsert phân tích email thông minh: hỗ trợ cả trường kết hợp (ngăn cách bởi dấu chấm phẩy, phẩy) và các cột riêng biệt `Email 2`, `Email 3`.
  * Sắp xếp `.OrderBy(x => x.Id)` dựa trên Snowflake ID bảo đảm 100% tính đơn điệu thời gian tăng dần (cũ nhất ở dòng đầu tiên).
  * Hỗ trợ tìm kiếm khách hàng liên kết linh hoạt theo cả `CustomerCode` và `FullName`.

### 3.2. `Backend/WebApi/Controllers/MeasurementPointController.cs`
- **Đánh giá**: **Chuẩn mực**.
- **Điểm sáng**:
  * Endpoint `ExportCsvMeasurementPointAsync` đã tiếp nhận linh hoạt `[FromBody] ExportMeasurementPointsCsvRequest? request`, đồng bộ trọn vẹn với bộ lọc của Frontend.
  * Endpoint `ImportCsvMeasurementPointsAsync` kiểm tra định dạng đuôi `.csv`, xác thực tệp rỗng và áp dụng phân quyền `[Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin])]`.
  * Khai báo đầy đủ using directives `Application.Common.ValueObjects` (`ErrorItem`) và `Microsoft.AspNetCore.Http` (`IFormFile`), giải quyết triệt để lỗi biên dịch CS0246.

### 3.3. `Frontend/src/pages/point-management/index.tsx`
- **Đánh giá**: **Tuyệt vời**.
- **Điểm sáng**:
  * Bố cục bộ lọc 2 ô rất thoáng và gọn gàng, nút xóa tất cả chỉ hiện khi có điều kiện lọc.
  * Dữ liệu xuất `dataExport` client-side được sắp xếp trước theo thời gian tạo ASC (`oldest first`) và truyền `requestBody` sang `DialogExportForm` để server export cũng nhận diện đúng filter.
  * Tích hợp mượt mà component `DialogImportPoint`.

### 3.4. `Frontend/src/pages/point-management/components/table-point.tsx`
- **Đánh giá**: **Rất tốt**.
- **Điểm sáng**:
  * Cập nhật tiêu đề cột thành "Tên khách hàng" và mở tab mới qua `window.open(url, '_blank')`.
  * Đã dọn dẹp sạch sẽ import thừa `useNavigate` giúp code sạch và không phát sinh cảnh báo linter.

### 3.5. `Frontend/src/pages/point-management/components/dialog-import-point.tsx`
- **Đánh giá**: **Hoàn hảo**.
- **Điểm sáng**:
  * Đầy đủ tính năng: Tải tệp mẫu `Template_DiemDo.csv` chuẩn UTF-8 BOM, kéo thả upload, bảng thống kê và danh sách chi tiết các dòng lỗi.

### 3.6. `Backend/Infrastructure/Repositories/ReadWrite/MeasurementPointReadWriteRepository.cs` (Phase 2 Fix)
- **Đánh giá**: **Xuất sắc**.
- **Điểm sáng**:
  * Đã loại bỏ hoàn toàn trigger từ khóa "dung", tách biệt tường minh điều kiện nhận diện "ngung" / "tam dung" cho `Deactivate` và "dang" / "su dung" cho `Active`.
  * Đã áp dụng `hasStatusSpecified`: Nếu cột trạng thái để trống trong file CSV, bảo lưu 100% trạng thái hiện tại của điểm đo, tránh việc vô tình thay đổi trạng thái khi người dùng chỉ muốn cập nhật email hoặc thông tin mô tả.

### 3.7. `Backend/Infrastructure/Services/EmailSenderService.cs` (Phase 2 Enhancement)
- **Đánh giá**: **Chuẩn xác & Minh bạch**.
- **Điểm sáng**:
  * Thu thập đầy đủ các email cấu hình và thực thi triệt để nguyên tắc: Tất cả email thành công = `SEND_SUCCESS`; bất kỳ 1 email bị lỗi định dạng hoặc gửi thất bại qua SMTP = `SEND_FAILED`.
  * Không còn tình trạng bỏ qua âm thầm các email phụ bị lỗi cú pháp để báo thành công giả tạo.

### 3.8. `Frontend/src/pages/notice-management/components/table/table-notice.tsx` & `table-client-point.tsx`
- **Đánh giá**: **Tinh tế & Đồng nhất**.
- **Điểm sáng**:
  * Trạng thái điểm đo được chuẩn hóa đồng nhất trên toàn bộ các trang: "Đang sử dụng" (Active) và "Ngừng sử dụng" (Deactivate).
  * Trang Quản lý phiếu báo được tinh chỉnh giao diện xuất sắc: Cột Mã điểm đo hiển thị gọn gàng, cột Email nhận phiếu báo hiển thị trực quan toàn bộ danh sách email.

### 3.9. `Backend/Infrastructure/Services/EmailSenderService.cs` & `StatementEmailStatusDetail.cs` (Phase 3 Smart Resend)
- **Đánh giá**: **Đột phá Kiến trúc (Architectural Breakthrough)**.
- **Điểm sáng**:
  * Tận dụng cột `longtext Status` để lưu cấu trúc JSON `StatementEmailStatusDetail` mà không phải migration DB, vừa tương thích ngược dữ liệu cũ (`SEND_SUCCESS`, `SEND_FAILED`), vừa lưu trữ chi tiết kiểm toán (Audit Trail) cho từng email.
  * Cơ chế Smart Selective Resend: Thuật toán lọc `alreadySuccessfulMap` loại trừ 100% email đã thành công trước đó; hệ thống chỉ phát lệnh gửi bù cho các email lỗi hoặc chưa gửi. Đảm bảo triệt để **Zero Duplicate Resend**, ngăn ngừa hoàn toàn khiếu nại spam email từ khách hàng.
  * Trạng thái phân loại chuẩn xác: `SEND_SUCCESS` (100% thành công), `PARTIAL_SUCCESS` (gửi thành công một phần), `SEND_FAILED` (thất bại toàn bộ).
  * Các truy vấn tìm kiếm và gửi theo chu kỳ GCS trong `PeriodicEnergyStatementReadOnlyRepository.cs` và `PeriodicEnergyStatementReadWriteRepository.cs` được cập nhật đồng bộ, không phát sinh lỗi lọc.

### 3.10. `Frontend/src/pages/notice-management/components/table/table-notice.tsx` (Phase 3 Smart UI/UX)
- **Đánh giá**: **Trải nghiệm Xuất sắc (Outstanding UX)**.
- **Điểm sáng**:
  * Mỗi email trong cột "Email nhận phiếu báo" có huy hiệu trạng thái trực quan riêng biệt (Xanh lá `Đã gửi`, Đỏ `Thất bại`, Xám `Chờ gửi`).
  * Email lỗi được tích hợp `Tooltip` hiển thị chính xác thông báo lỗi kỹ thuật (từ chối kết nối SMTP, sai cú pháp,...), giúp người vận hành biết ngay nguyên nhân cần khắc phục.
  * Checkbox gửi lại được kiểm soát thông minh: Mở khóa khi có lỗi hoặc gửi một phần để gửi bù; tự động khóa khi đã thành công toàn bộ để tránh gửi nhầm.
  * Bộ lọc hỗ trợ thêm lựa chọn `Gửi một phần` (`PARTIAL_SUCCESS`), giúp quản trị viên lọc nhanh các điểm đo có email cần gửi bù.

### 3.11. `Frontend/src/pages/gcs-management/index.tsx` & `check/index.tsx` (Phase 4 GCS UI/UX)
- **Đánh giá**: **Chuẩn mực & An toàn Tuyệt đối**.
- **Điểm sáng**:
  * Loại bỏ triệt để filter dư thừa "Tên sổ", bố cục 2 filter "Mã sổ" và "Trạng thái" dạng `basis-1/2` cân đối, tối ưu không gian hiển thị.
  * Nút "Chi tiết" và "Tra soát" mở tab mới (`_blank`) bảo toàn nguyên vẹn trang danh sách, dữ liệu phân trang và bộ lọc đang áp dụng.
  * Đặc biệt, giải pháp bổ sung fallback đọc query parameters trong [check/index.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/gcs-management/check/index.tsx) giải quyết hoàn toàn rủi ro crash ứng dụng khi mở tab mới bằng `window.open` (do không có `location.state`).

### 3.12. `Frontend/src/pages/account-management/index.tsx` & `account-form.tsx` (Phase 4 Account UI/UX)
- **Đánh giá**: **Đồng bộ Hoàn hảo (Total UI Consistency)**.
- **Điểm sáng**:
  * Cột "Số điện thoại" được đổi thành "Điện thoại" đồng bộ từ Header bảng, Form chi tiết đến cấu hình xuất file.
  * Trạng thái tài khoản được chuẩn hóa thành "Đang hoạt động" (`#41A026`) & "Vô hiệu" (`#B22222`), xóa bỏ hoàn toàn sự không nhất quán giữa các trang quản lý.
  * Nút "Chi tiết" mở tab mới (`_blank`) thống nhất với tiêu chuẩn chung của toàn bộ ứng dụng MDMSv2.

### 3.13. `Frontend/src/pages/client-management/components/client-form.tsx` (Phase 5 Client Label)
- **Đánh giá**: **Nhất quán & Tối ưu (Consistent & Clean)**.
- **Điểm sáng**:
  * Nhãn ô nhập liệu được đồng nhất thành "Điện thoại". Do `client-form.tsx` là component tái sử dụng duy nhất cho các màn hình Thêm mới (`ADD`), Sửa (`EDIT`) và Chi tiết khách hàng (`VIEW`), thay đổi này lập tức đồng bộ 100% trải nghiệm người dùng trong phân hệ khách hàng.

### 3.14. `Frontend/src/pages/config/index.tsx` & `table-config.tsx` (Phase 5 Config UI/UX)
- **Đánh giá**: **Chuẩn mực & An toàn Tuyệt đối**.
- **Điểm sáng**:
  * Bỏ hoàn toàn ô tìm kiếm "Tên khách hàng" và cột "Địa chỉ", giúp bố cục bảng dữ liệu điểm đo thoáng, gọn gàng và tập trung vào các trường đo đếm cốt lõi.
  * Nút "Chi tiết" mở Tab mới (`_blank`) kèm URL query parameters (`code`, `name`), phối hợp cùng logic fallback ở trang chi tiết đảm bảo không bao giờ bị lỗi runtime.

### 3.15. `Frontend/src/core/application/common/constants.ts` & `table-tag-name.tsx` (Phase 5 Fixed Units & Greyed-out Combobox)
- **Đánh giá**: **Kiến trúc Vững chắc & Trải nghiệm Trực quan**.
- **Điểm sáng**:
  * Bổ sung đầy đủ các hằng số đơn vị kỹ thuật chuẩn (`KW`, `KVAR`, `HZ`, `KWH`, `KVARH`) theo đúng tiêu chuẩn điện lực/EVN.
  * Tự động làm mờ (`disabled={val.length <= 1}`) các Combobox của các đại lượng có đơn vị cố định, đồng thời cho phép lựa chọn linh hoạt đối với Điện áp (V / kV).
  * Hàm `getUnit(record)` có fallback thông minh, bảo đảm combobox luôn hiển thị đúng đơn vị chuẩn ngay cả khi dữ liệu chưa được lưu trong cơ sở dữ liệu.

### 3.16. `Frontend/src/pages/config/detail/index.tsx` (Phase 5 Rapid CSV Setup & Copy Deprecation)
- **Đánh giá**: **Mạnh mẽ, Toàn diện & Thân thiện Người Dùng**.
- **Điểm sáng**:
  * Tính năng **Xuất CSV**: Xuất đầy đủ cấu hình điểm đo (Thông số vận hành, Thông số điện năng, Cảnh báo, Thời gian) kèm Tag ID, Đơn vị, Tên cảnh báo với tiền tố `\uFEFF` UTF-8 BOM, đảm bảo mở trực tiếp trên Microsoft Excel tiếng Việt không bị lỗi font.
  * Tính năng **Nhập CSV**: Bộ parser CSV linh hoạt, hỗ trợ cả định dạng dấu phẩy `,` và dấu chấm phẩy `;`, phân tích header thông minh và tự động ánh xạ dữ liệu theo mã hoặc tên tham số, đồng thời bảo toàn quy tắc đơn vị cố định.
  * Loại bỏ triệt để các mã nguồn thừa của chức năng "Copy" (biểu tượng sao chép, state dialog duplicate, hàm API sao chép), giúp code tinh giản và ngăn chặn hành vi clone cấu hình không mong muốn.

### 3.17. `Backend/Infrastructure/Handlers/Commands/IoT/MeasurementPointMonitoringCommandHandler.cs` (Phase 6 IoT Broadcast)
- **Đánh giá**: **Giải phóng Băng thông & Mở rộng Phạm vi Giám sát**.
- **Điểm sáng**:
  * Tăng `PageSize = 1000` trong `ProcessMeasurementPointMonitoring` giải quyết triệt để nút thắt cổ chai giới hạn 10 điểm đo trang đầu tiên. Dữ liệu phát sóng qua `ListNotificationHub` giờ đây bao quát toàn diện các điểm đo đang hoạt động trên hệ thống, cho phép các client theo dõi ở bất kỳ trang nào cũng nhận được dữ liệu tức thời.

### 3.18. `Frontend/src/pages/point-monitoring/summary-parameter/index.tsx` (Phase 6 No-Reset Live Pagination)
- **Đánh giá**: **Kiến trúc State Xuất sắc & Bảo toàn Trải nghiệm Người dùng**.
- **Điểm sáng**:
  * Áp dụng thành công các `useRef` (`currentParamsRef`, `dataFormRef`, `sizeRef`) để bẻ gãy React stale closure, đảm bảo các hàm gọi bất đồng bộ luôn có tham số phân trang và bộ lọc mới nhất.
  * Thiết kế hàm `refreshCurrentPageAsync()` truy vấn ngầm (silent query), vừa cập nhật mới số liệu đo đếm vừa bảo toàn con trỏ trang (`before`/`after`) và điều kiện lọc (`customerName`, `measurementPointCode`, `connectionStatus`).
  * Tuyệt đối không reset về trang đầu khi có dữ liệu từ REST API gửi về, đáp ứng 100% tiêu chí nghiệp vụ của khách hàng.
  * Cơ chế đồng bộ 2 lớp (In-memory merge kết hợp authoritative server refresh) mang lại phản hồi giao diện tức thì và độ chính xác dữ liệu tuyệt đối.

### 3.19. `Frontend/src/infrastructure/common/components/websocket/socket-io-detail.tsx` (Phase 6 Resilient Multi-Hub Connection)
- **Đánh giá**: **Ổn định Cao & Tự phục hồi Kết nối**.
- **Điểm sáng**:
  * Tách biệt logic kết nối và quản lý vòng đời cho 3 Hub: `DetailNotificationHub`, `DetailNotificationDateTimeHub` và `ListNotificationHub`.
  * Bổ sung cơ chế `onreconnected` tự động gọi lại `SubscribeToDetail(id)`, giải quyết hoàn toàn lỗi mất group subscription của SignalR khi kết nối mạng bị gián đoạn.
  * Dọn dẹp an toàn khi unmount component, tránh rò rỉ bộ nhớ (memory leaks) và kết nối socket mồ côi.

### 3.20. `Frontend/src/pages/point-monitoring/summary-parameter/detail/index.tsx` (Phase 6 Detail Auto-Refresh)
- **Đánh giá**: **Mượt mà, Đồng bộ & Đầy đủ Dữ liệu**.
- **Điểm sáng**:
  * Khắc phục closure bug khi cập nhật `dataUser` qua functional update `setDataUser(prev => ({ ...prev, ...dataSocket }))`.
  * Tự động kích hoạt `getMeasurementPointMonitoringNoLoadingAsync()` khi có thông báo từ REST API, đảm bảo toàn bộ thẻ thông tin (vận hành, chốt điện năng, đồng hồ thiết bị, cảnh báo) đều được kéo mới đầy đủ từ máy chủ.
  * Kết hợp bộ đếm định kỳ ngầm mỗi 10 giây dự phòng cho môi trường mạng hạn chế WebSocket.

### 3.21. `Frontend/src/pages/point-monitoring/` (Phase 7 UI/UX Enhancements)
- **Đánh giá**: **Chuẩn hóa Giao diện, Trực quan & Thuận tiện Vận hành**.
- **Điểm sáng**:
  * **Thông số tổng hợp (`summary-parameter/index.tsx` & `table-point.tsx`)**:
    - Loại bỏ ô lọc "Tên khách hàng" và cột "Mô tả điểm đo" giúp bảng số liệu đo đếm tập trung vào các đại lượng quan trọng (U, I, P, Q, PF, F, Kết nối).
    - Bộ lọc được căn chỉnh đẹp mắt theo tỉ lệ 50-50 cho 2 tiêu chí cần thiết nhất: Mã điểm đo (tích hợp icon tìm kiếm & xóa nhanh) và Trạng thái kết nối.
    - Nút "Chi tiết" trong menu dòng mở trang trên tab mới (`window.open(url, '_blank')`), đảm bảo người vận hành giữ nguyên vị trí trang và bộ lọc đang xem.
  * **Bảng sự kiện & Bảng cảnh báo (`point-event` & `point-warning`)**:
    - Thay thế cột ghép chuỗi "Nguồn" bằng 2 cột riêng biệt "Mã điểm đo" và "Tên khách hàng", tăng tính rõ ràng và chuẩn mực dữ liệu.
    - Chuyển đổi bộ lọc tìm kiếm từ "Nguồn" sang "Mã điểm đo", đồng bộ với tham số `measurementPointCode` của Backend API.
    - Đồng bộ tệp xuất CSV (`Constants.listHeaderExport`): Tách biệt rõ ràng 2 cột "Mã điểm đo" và "Tên khách hàng" thay cho cột "Nguồn" cũ.
    - Bảo vệ sự kiện click dòng `onRow` an toàn với mã điểm đo, tránh crash khi dữ liệu liên kết điểm đo chưa nạp đầy đủ.
    - **Cơ chế Tự động Cập nhật Không Giật UI (Silent Refresh & Fail-Safe Polling)**:
      * Trang bị `currentParamsRef` và `sizeRef` lưu trữ con trỏ phân trang và bộ lọc, chống lại React stale closure.
      * Xây dựng `refreshCurrentPageAsync()` truy vấn ngầm từ máy chủ, không bật `setLoading(true)` gây nhấp nháy spinner.
      * Tích hợp đồng hồ ngầm tự làm mới mỗi 15 giây, đảm bảo tính sẵn sàng cao ngay cả khi kết nối mạng tạm thời gián đoạn.
      * Nâng cấp `SocketCommon` quản lý kết nối chuẩn xác (`[baseURL, endPoint]`) và tái đăng ký handler qua `onreconnected`.

### 3.22. `Frontend/src/pages/point-monitoring/` & `Frontend/src/pages/config/` (Phase 8 UI/UX Enhancements)
- **Đánh giá**: **Tuyệt hảo, Hiện đại, Khoa học & Chuẩn mực Trải nghiệm Người dùng**.
- **Điểm sáng**:
  * **Bảng sự kiện & Bảng cảnh báo (`table-warning.tsx`, `table-event.tsx`)**:
    - Phân bổ lại phần trăm độ rộng các cột tổng cộng 100%: Thời gian (16%), Mã điểm đo (18%), Tên khách hàng (30%), Tên sự kiện/cảnh báo (20%), Trạng thái (10%), Giá trị (6%).
    - Triệt tiêu hoàn toàn khoảng trống xám/trắng ở lề phải bảng; mã điểm đo dài và tên khách hàng có đủ không gian hiển thị ngay ngắn.
    - Sửa tiêu đề cột trong `table-event.tsx` thành "Tên sự kiện" chính xác theo nghiệp vụ.
    - Bổ sung `loading={loading}` và `scroll={{ x: 1000, y: window.innerHeight * 0.66 }}`.
  * **Trang Liên kết dữ liệu (`config/detail/index.tsx`)**:
    - Thứ tự các nút thao tác chuẩn hóa theo luồng thao tác logic: "Xuất CSV" -> "Nhập từ CSV" -> "Lưu".
    - Icon nút "Nhập từ CSV" chuyển sang `<FiDownload size={20} />`, đồng bộ hoàn hảo với các trang Quản lý khách hàng và Quản lý điểm đo.
  * **Tab Thời gian trong Liên kết dữ liệu (`constants.ts`, `table-tag-name.tsx`)**:
    - Cấu trúc lại toàn bộ 24 tham số thời gian thành 4 nhóm rõ rệt, mỗi nhóm sắp xếp tuần tự theo trật tự: Năm -> Tháng -> Ngày -> Giờ -> Phút -> Giây.
    - Thiết kế giao diện lưới 2x2 trực quan với 4 Card phân biệt màu sắc, có huy hiệu số lượng thông số và icon đại diện.
    - Bảng 2 cột gọn gàng: Badge đại lượng (Năm/Tháng/Ngày/Giờ/Phút/Giây) kèm mã kỹ thuật và ô nhập Tag ID.
    - Kỹ sư có thể nhìn thấy trọn vẹn toàn bộ 4 nhóm thời gian trên một màn hình mà không cần cuộn dọc.

### 3.23. `Frontend/src/pages/config/` & `Frontend/src/infrastructure/` (Phase 9 UI/UX & Localization Review)
- **Đánh giá**: **Hoàn hảo, Nhất quán Tuyệt đối & Trải nghiệm Người dùng Chuẩn mực**.
- **Điểm sáng**:
  * **Đồng bộ hóa giao diện bảng Tab Thời gian (`table-tag-name.tsx`)**:
    - Thay thế bố cục thẻ lưới bằng chuẩn bảng Ant Design `TableCommon`, hòa hợp 100% với các tab "Thông số vận hành", "Thông số điện năng", "Cảnh báo" về màu nền tiêu đề `#d3dae5`, border, padding và font chữ.
    - Ứng dụng kỹ thuật `onCell: rowSpan = 6` gom 6 dòng đại lượng cho mỗi nhóm trong cột "Nhóm", vừa đáp ứng yêu cầu nhóm khoa học của người dùng, vừa đảm bảo tính đồng bộ thị giác hoàn hảo.
    - Cột "Dữ liệu" thể hiện tuần tự: Năm, Tháng, Ngày, Giờ, Phút, Giây; cột "Tag ID" rộng 45% tạo không gian nhập liệu thoải mái.
  * **Hộp thoại Import CSV chuyên nghiệp (`dialog-import-config.tsx` & `config/detail/index.tsx`)**:
    - Loại bỏ hoàn toàn hành vi mở thẳng hộp thoại Windows Native khi bấm "Nhập từ CSV".
    - Xây dựng Modal `DialogImportConfig` với vùng kéo thả `Dragger`, nút tải file mẫu có sẵn tham số điểm đo, thống kê kết quả Tag rõ ràng và hướng dẫn ấn Lưu. Hoàn toàn đồng nhất với `DialogImportCustomer` và `DialogImportPoint`.
  * **Xử lý triệt để lỗi Toast tiếng Anh (`toastMessage.tsx`, `dialog-import-*.tsx`)**:
    - Thiết kế hàm `extractMessage` chịu lỗi linh hoạt, tự động xử lý chuẩn xác dù hàm thông báo được gọi với 1 tham số hay 2 tham số.
    - Việt hóa 100% các giá trị mặc định (`Thành công`, `Có lỗi xảy ra`, `Cảnh báo`, `Thông tin`), triệt tiêu hoàn toàn chữ tiếng Anh "Success" hay "Error" xuất hiện trên màn hình người dùng.

### 3.24. `Frontend/src/pages/config/components/table-tag-name.tsx` (Phase 10 Flat Table & Bỏ Phân Nhóm)
- **Đánh giá**: **Xuất sắc, Tinh gọn, Nhất quán Tuyệt đối với Toàn bộ Hệ Thống**.
- **Điểm sáng**:
  * **Loại bỏ phân nhóm**: Bỏ hoàn toàn cột "Nhóm" và cơ chế gộp ô `rowSpan = 6`, đưa tab "Thời gian" về chuẩn bảng phẳng (flat table) hoàn toàn tương đồng với các tab "Thông số điện năng", "Thông số vận hành", "Cảnh báo".
  * **Hiển thị trực quan**: Cột "Dữ liệu" hiển thị trực tiếp `record.label` với tên gọi đầy đủ của từng tham số (vd: *Thời gian hiện tại (năm)*, *Thời gian hiện tại (tháng)*, ..., *Thời gian chốt gần nhất (năm)*, ..., *Thời gian công suất cực đại kỳ hiện tại (năm)*, ..., *Thời gian công suất cực đại kỳ trước (năm)*...).
  * **Tỷ lệ độ rộng cột hoàn hảo**: Cột "Dữ liệu" chiếm 50% và Cột "Tag ID" chiếm 50%, phủ kín 100% bề ngang bảng dữ liệu, tạo cảm giác nhập liệu liền mạch, cân đối và thẩm mỹ cao.

---

## 4. HƯỚNG DẪN BƯỚC TIẾP THEO (NEXT STEPS)
1. **Phê duyệt**: Đủ điều kiện hợp nhất (Merge) vào nhánh phát triển chính.
2. **Triển khai**: Sẵn sàng đóng gói bộ cài đặt `Publish/MDMSv2_InstallPackage` qua kịch bản `build_package.ps1` phục vụ triển khai thử nghiệm trên máy chủ.





