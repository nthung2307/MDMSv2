# [DEV HANDOFF] QUẢN LÝ ĐIỂM ĐO: NÂNG CẤP BỘ LỌC, TIÊU ĐỀ BẢNG, MỞ TAB MỚI, IMPORT CSV AN TOÀN & XUẤT CSV KÈM EMAIL PHIẾU BÁO (TỪ CŨ ĐẾN MỚI)

> **Tài liệu bàn giao phát triển (Developer Handoff Changelog)**  
> **Người thực hiện:** MDMS-Coder (Lead Full-Stack Developer)  
> **Người tiếp nhận:** MDMS-Tester (Lead QA Engineer) & MDMS-Reviewer (Principal Architect)  
> **Căn cứ thực hiện:** Bản thiết kế kỹ thuật [PLAN_SPEC.md](file:///e:/sources/MDMSv2/PLAN_SPEC.md).

---

## 1. Tóm tắt Thay đổi (Summary of Changes)
Đã hoàn thành 100% các hạng mục theo yêu cầu kỹ thuật:

### 1.1. Backend (.NET 8 Clean Architecture)
1. **Cập nhật DTO & Mô hình dữ liệu**:
   - [ListMeasurementPointDto.cs](file:///e:/sources/MDMSv2/Backend/Application/DataTransferObjects/Responses/MeasurementPoints/ListMeasurementPointDto.cs): Bổ sung các thuộc tính `Email`, `Email2`, `Email3` để truyền dữ liệu người nhận phiếu báo lên Frontend phục vụ hiển thị và xuất file.
   - [ImportMeasurementPointResponse.cs](file:///e:/sources/MDMSv2/Backend/Application/DataTransferObjects/Responses/MeasurementPoints/ImportMeasurementPointResponse.cs): Khai báo DTO kết quả import gồm `TotalRows`, `InsertedCount`, `UpdatedCount`, `ErrorCount` và danh sách chi tiết `Errors` (`RowNumber`, `PointCode`, `ErrorMessage`).
   - [ExportMeasurementPointsCsvRequest.cs](file:///e:/sources/MDMSv2/Backend/Application/DataTransferObjects/Requests/MeasurementPoints/ExportMeasurementPointsCsvRequest.cs): DTO nhận điều kiện lọc (`Code`, `Status`, `SearchByFields`) khi xuất file CSV từ máy chủ.
2. **CQRS Command & Handler**:
   - [ImportMeasurementPointsCommand.cs](file:///e:/sources/MDMSv2/Backend/Application/Commands/MeasurementPoints/ImportMeasurementPointsCommand.cs): Command chứa luồng `Stream FileStream` của tệp CSV upload lên.
   - [IMeasurementPointsCommandHandlers.cs](file:///e:/sources/MDMSv2/Backend/Application/Handlers/Commands/MeasurementPoints/IMeasurementPointsCommandHandlers.cs): Khai báo interface `IImportMeasurementPointsCommandHandler`.
   - [ImportMeasurementPointsCommandHandler.cs](file:///e:/sources/MDMSv2/Backend/Infrastructure/Handlers/Commands/MeasurementPoints/ImportMeasurementPointsCommandHandler.cs): Hiện thực hóa handler gọi repository để xử lý nghiệp vụ, tự động inject qua Autofac + MediatR.
3. **Repository Layer**:
   - [IMeasurementPointsReadWriteRepository.cs](file:///e:/sources/MDMSv2/Backend/Application/Repositories/ReadWrite/IMeasurementPointsReadWriteRepository.cs): Khai báo phương thức `ImportMeasurementPointsFromCsvAsync` và cập nhật `GenerateCustomerCsv(ExportMeasurementPointsCsvRequest? request, CancellationToken cancellationToken)`.
   - [MeasurementPointReadWriteRepository.cs](file:///e:/sources/MDMSv2/Backend/Infrastructure/Repositories/ReadWrite/MeasurementPointReadWriteRepository.cs):
     * **Cập nhật `GenerateCustomerCsv`**:
       - Áp dụng bộ lọc linh hoạt theo `Code`, `Status` và `SearchByFields`.
       - Sắp xếp thứ tự từ cũ đến mới: `.OrderBy(x => x.Id)` (theo trình tự thời gian Snowflake ID, điểm đo tạo cũ nhất nằm ở dòng đầu tiên).
       - Thêm cột "Email nhận phiếu báo (tối đa 3 email)" (kết hợp `Email`, `Email2`, `Email3` phân cách bởi dấu chấm phẩy `; `).
       - Cập nhật tiêu đề cột thành "Tên khách hàng" (thay cho "Tên KH").
       - Mã hóa UTF-8 BOM (`new UTF8Encoding(true)`) giúp Excel mở file không bị lỗi font Tiếng Việt.
     * **Hiện thực hóa `ImportMeasurementPointsFromCsvAsync`**:
       - Bộ giải mã `DecodeCsvBytes` hỗ trợ UTF-8 có/không có BOM, Windows-1258 fallback.
       - Tự động nhận diện phân cách dấu phẩy `,`, chấm phẩy `;` hoặc tab `\t`.
       - Chuẩn hóa tiêu đề tiếng Việt không dấu `NormalizeHeaderText`.
       - Nhận diện cột linh hoạt (hỗ trợ cả cột đơn "Email nhận phiếu báo" và các cột riêng biệt `Email 2`, `Email 3`).
       - Phân tích tối đa 3 email cho mỗi điểm đo.
       - **Safe Upsert**: Điểm đo đã tồn tại (`Code`) chỉ cập nhật thông tin và email, giữ nguyên 100% `Id` (bảo toàn dữ liệu đo đếm lịch sử, báo cáo ngày/nửa giờ, cảnh báo, sự kiện). Điểm đo mới được gán ID Snowflake và liên kết với khách hàng tương ứng.
4. **WebApi Layer**:
   - [Url.cs](file:///e:/sources/MDMSv2/Backend/WebApi/Common/Url.cs): Khai báo hằng số route `public const string ImportCsv = "Measurement-Points/Import-Csv";`.
   - [MeasurementPointController.cs](file:///e:/sources/MDMSv2/Backend/WebApi/Controllers/MeasurementPointController.cs):
     * Cập nhật endpoint `[HttpPost(Common.Url.Default.MeasurementPoints.ExportCsv)]` nhận `[FromBody] ExportMeasurementPointsCsvRequest? request`.
     * Bổ sung endpoint `[HttpPost(Common.Url.Default.MeasurementPoints.ImportCsv)]` với phân quyền nghiêm ngặt `[Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin])]`.
     * Tích hợp đầy đủ using directives `Application.Common.ValueObjects` và `Microsoft.AspNetCore.Http` để giải quyết kiểu `ErrorItem` và `IFormFile`.

---

### 1.2. Frontend (React 18 + TypeScript + Vite + Ant Design 5)
1. **Quy chuẩn Constants & Enums**:
   - [constants.ts](file:///e:/sources/MDMSv2/Frontend/src/core/application/common/constants.ts): Cập nhật `Constants.listHeaderExport.PointManagement` đổi tiêu đề "Tên KH" thành "Tên khách hàng", thêm trường "Email nhận phiếu báo (tối đa 3 email)", chuẩn hóa trạng thái "Đang sử dụng" / "Ngừng sử dụng".
2. **TypeScript Interfaces & Routing**:
   - [measurementPoint.interface.ts](file:///e:/sources/MDMSv2/Frontend/src/core/application/interfaces/response/measurement-point/measurementPoint.interface.ts): Bổ sung `email2`, `email3`, `customer`, `status` vào interface `MeasurementPoint`.
   - [endPoint.ts](file:///e:/sources/MDMSv2/Frontend/src/core/application/common/endPoint.ts): Khai báo `static ImportMeasurementPointCsv: string = 'measurement-points/import-csv';`.
   - [measurementPoint.interface.ts (methods)](file:///e:/sources/MDMSv2/Frontend/src/core/application/interfaces/methods/measurement-point/measurementPoint.interface.ts): Khai báo phương thức `importMeasurementPointCsv`.
3. **Services & Custom Hooks**:
   - [measurementPoint.service.ts](file:///e:/sources/MDMSv2/Frontend/src/infrastructure/repository/measurement-point/services/measurementPoint.service.ts): Hiện thực hóa `importMeasurementPointCsv` sử dụng `makeUploadRequestAsync`.
   - [useImportMeasurementPointCsv.hook.tsx](file:///e:/sources/MDMSv2/Frontend/src/infrastructure/repository/measurement-point/hooks/useImportMeasurementPointCsv.hook.tsx): Tạo custom hook gọi API multipart upload kèm quản lý loading và error handling.
4. **Components & Giao diện Trang**:
   - [dialog-import-point.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/point-management/components/dialog-import-point.tsx):
     * Hướng dẫn quy tắc Safe Upsert qua thẻ `Alert`.
     * Nút tải file mẫu `Template_DiemDo.csv` chuẩn UTF-8 BOM.
     * Kéo thả tệp CSV qua `Upload.Dragger`.
     * Hiển thị bảng tóm tắt kết quả (Tổng số, Đã cập nhật, Thêm mới, Lỗi) và danh sách chi tiết các dòng bị lỗi.
   - [table-point.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/point-management/components/table-point.tsx):
     * Cập nhật tiêu đề cột thành **"Tên khách hàng"**.
     * Nút hành động "Chi tiết" gọi `window.open(url, '_blank')` để mở tab mới.
     * Dọn dẹp import thừa `useNavigate`.
   - [point-management/index.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/point-management/index.tsx):
     * **Hiệu chỉnh Bộ lọc**: Chỉ sử dụng 2 bộ lọc theo "Mã điểm đo" (`code`, debounce 300ms) và "Trạng thái" (`status`), loại bỏ hoàn toàn bộ lọc "Tên khách hàng".
     * **Nút Nhập từ CSV**: Thêm nút với icon `FiDownload` bên cạnh nút xuất file, phân quyền qua `<PrivateComponent roles={[Constants.Role.SuperAdmin.value, Constants.Role.Admin.value]}>`.
     * **Xuất File CSV**: Sắp xếp dữ liệu `sortedData` theo thứ tự từ cũ đến mới (cũ nhất ở dòng 1), bổ sung thông tin emails (`item.email`, `item.email2`, `item.email3`), và truyền `requestBody={getListMeasurementParamsRequest}` vào `DialogExportForm` để đồng bộ bộ lọc khi xuất từ máy chủ.

---

## 2. Danh sách Tệp thay đổi (List of Modified & Created Files)
### Backend
- `Backend/Application/DataTransferObjects/Responses/MeasurementPoints/ListMeasurementPointDto.cs` (Cập nhật)
- `Backend/Application/DataTransferObjects/Responses/MeasurementPoints/ImportMeasurementPointResponse.cs` (Tạo mới)
- `Backend/Application/DataTransferObjects/Requests/MeasurementPoints/ExportMeasurementPointsCsvRequest.cs` (Tạo mới)
- `Backend/Application/Commands/MeasurementPoints/ImportMeasurementPointsCommand.cs` (Tạo mới)
- `Backend/Application/Handlers/Commands/MeasurementPoints/IMeasurementPointsCommandHandlers.cs` (Cập nhật)
- `Backend/Application/Repositories/ReadWrite/IMeasurementPointsReadWriteRepository.cs` (Cập nhật)
- `Backend/Infrastructure/Handlers/Commands/MeasurementPoints/ImportMeasurementPointsCommandHandler.cs` (Tạo mới)
- `Backend/Infrastructure/Repositories/ReadWrite/MeasurementPointReadWriteRepository.cs` (Cập nhật)
- `Backend/WebApi/Common/Url.cs` (Cập nhật)
- `Backend/WebApi/Controllers/MeasurementPointController.cs` (Cập nhật)

### Frontend
- `Frontend/src/core/application/common/constants.ts` (Cập nhật)
- `Frontend/src/core/application/common/endPoint.ts` (Cập nhật)
- `Frontend/src/core/application/interfaces/response/measurement-point/measurementPoint.interface.ts` (Cập nhật)
- `Frontend/src/core/application/interfaces/methods/measurement-point/measurementPoint.interface.ts` (Cập nhật)
- `Frontend/src/infrastructure/repository/measurement-point/services/measurementPoint.service.ts` (Cập nhật)
- `Frontend/src/infrastructure/repository/measurement-point/hooks/useImportMeasurementPointCsv.hook.tsx` (Tạo mới)
- `Frontend/src/pages/point-management/components/dialog-import-point.tsx` (Tạo mới)
- `Frontend/src/pages/point-management/components/table-point.tsx` (Cập nhật)
- `Frontend/src/pages/point-management/index.tsx` (Cập nhật)

---

## 3. Hướng dẫn Kiểm thử cho MDMS-Tester (Testing Guide)
1. **Kiểm tra Bộ lọc**:
   - Truy cập trang "Quản lý điểm đo".
   - Kiểm tra chỉ còn đúng 2 ô lọc: "Nhập mã điểm đo" và "Trạng thái".
   - Nhập mã điểm đo -> Bảng dữ liệu tự động tải sau 300ms.
   - Chọn trạng thái ("Đang sử dụng" / "Ngừng sử dụng") -> Bảng dữ liệu tải lại tương ứng.
   - Bấm nút "Xóa tất cả" -> Bộ lọc reset về rỗng và bảng tải lại đầy đủ.
2. **Kiểm tra Header bảng**:
   - Tiêu đề cột khách hàng hiển thị đúng "Tên khách hàng" (không còn là "Tên KH").
3. **Kiểm tra Mở tab mới**:
   - Trong cột hành động `(...)` của mỗi dòng, click vào "Chi tiết" -> Trình duyệt mở trang chi tiết điểm đo tại một tab mới (`_blank`), tab danh sách giữ nguyên bộ lọc.
4. **Kiểm tra Xuất CSV**:
   - Khi đang áp dụng bộ lọc (ví dụ chọn "Đang sử dụng"), bấm nút "Xuất file" -> Chọn "CSV" -> Xuất file.
   - Kiểm tra file CSV tải về:
     * Cột "Tên khách hàng" và cột "Email nhận phiếu báo (tối đa 3 email)" hiển thị đầy đủ.
     * Điểm đo được sắp xếp từ cũ nhất đến mới nhất (bản ghi tạo đầu tiên nằm ở dòng 1).
     * Chỉ chứa các điểm đo thỏa mãn điều kiện lọc.
     * Mở trực tiếp trên Microsoft Excel không bị lỗi font tiếng Việt.
5. **Kiểm tra Nhập từ CSV**:
   - Bấm nút "Nhập từ CSV" -> Xuất hiện modal hướng dẫn Safe Upsert.
   - Bấm "Tải tệp mẫu CSV" -> Nhận file `Template_DiemDo.csv`.
   - Chỉnh sửa email nhận phiếu báo của một điểm đo đang có và thêm một điểm đo mới -> Tải lên -> Thông báo thành công và danh sách điểm đo tự động cập nhật.
   - Đăng nhập tài khoản không phải Admin/SuperAdmin -> Nút "Nhập từ CSV" bị ẩn; gọi API trực tiếp trả về HTTP 403 Forbidden.

---

## 4. Bổ sung Thay đổi Phase 2 (Phase 2 Bugfixes & Enhancements)

### 4.1. Sửa lỗi Nhận diện Trạng thái khi Import CSV
- **Tệp sửa đổi**: [MeasurementPointReadWriteRepository.cs](file:///e:/sources/MDMSv2/Backend/Infrastructure/Repositories/ReadWrite/MeasurementPointReadWriteRepository.cs)
- **Chi tiết**:
  * Tách biệt điều kiện nhận diện: Chuỗi "dang su dung" không còn bị kích hoạt bởi từ khóa "dung" (dành cho dừng/ngừng).
  * Quy tắc: Chứa "ngung", "tam dung", "deactivate", "inactive", hoặc "0" -> `EntityStatus.Deactivate`. Chứa "dang", "su dung", "hoat dong", "active", hoặc "1" -> `EntityStatus.Active`.
  * Safe Upsert: Nếu `rawStatus` để trống, giữ nguyên trạng thái `existingPoint.Status` hiện tại của điểm đo; đối với điểm đo mới, nếu để trống thì mặc định là `Active`.

### 4.2. Đồng nhất Hiển thị Trạng thái Điểm đo
- **Tệp sửa đổi**: [table-client-point.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/client-management/components/table-client-point.tsx)
- **Chi tiết**: Chuyển đổi trạng thái hiển thị của điểm đo trên trang Chi tiết khách hàng từ "Hoạt động / Ngưng hoạt động" thành `"Đang sử dụng"` / `"Ngừng sử dụng"` để thống nhất 100% với trang Quản lý điểm đo.

### 4.3. UI/UX Trang Quản lý Phiếu báo & Chuẩn hóa Logic Gửi Mail
- **Tệp sửa đổi**:
  * [table-notice.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/notice-management/components/table/table-notice.tsx):
    - Tách cột "Mã điểm đo": Chỉ hiển thị mã điểm đo, loại bỏ dòng text email bên dưới.
    - Bổ sung cột "Email nhận phiếu báo": Hiển thị danh sách đầy đủ tất cả email nhận phiếu báo (`email`, `email2`, `email3`, hoặc fallback từ `customer.email`).
    - Cập nhật hàm `isPointSendable`, cột trạng thái và thời gian gửi gần nhất để kiểm tra tính hợp lệ của toàn bộ các email trong danh sách.
  * [EmailSenderService.cs](file:///e:/sources/MDMSv2/Backend/Infrastructure/Services/EmailSenderService.cs):
    - Thu thập toàn bộ email được cấu hình (`Email`, `Email2`, `Email3` hoặc fallback từ `Customer.Email`).
    - Tiêu chí 1: Nếu không có email nào -> `NO_EMAIL_CONFIGURED`.
    - Tiêu chí 2: Khi phát lệnh qua SMTP: Xử lý theo kiến trúc Smart Selective Resend chi tiết từng email (xem mục 4.4).

### 4.4. Giải pháp Gửi Bù Thông Minh (Smart Selective Resend) & Hiển thị Chi tiết Thất bại từng Email
Nhằm khắc phục triệt để vấn đề: *"Khi có ít nhất 1 email thất bại, việc gửi lại toàn bộ sẽ gửi trùng lặp gây phiền hà cho người nhận đã thành công"*:
- **Mô hình DTO**:
  - Tạo [StatementEmailStatusDetail.cs](file:///e:/sources/MDMSv2/Backend/Application/DataTransferObjects/Responses/PeriodicEnergyStatement/StatementEmailStatusDetail.cs) lưu trữ `Overall` (`SEND_SUCCESS` | `PARTIAL_SUCCESS` | `SEND_FAILED`), `SentTime` và `Details` (`List<StatementEmailRecipientResult>` gồm `Email`, `Status`, `Error`).
- **Backend Service & Repository**:
  - [EmailSenderService.cs](file:///e:/sources/MDMSv2/Backend/Infrastructure/Services/EmailSenderService.cs): Phân tích lịch sử `item.Status` qua hàm `ParseStatusDetail`. Lập bản đồ `alreadySuccessfulMap`, chỉ đưa các email chưa gửi hoặc thất bại vào danh sách gửi tiếp `emailsToSend`. Nếu toàn bộ đã gửi thành công -> bỏ qua gửi lại (Zero Duplicate Email). Gửi độc lập từng email, bắt ngoại lệ riêng biệt và cập nhật tổng hợp kết quả.
  - [PeriodicEnergyStatementReadWriteRepository.cs](file:///e:/sources/MDMSv2/Backend/Infrastructure/Repositories/ReadWrite/PeriodicEnergyStatementReadWriteRepository.cs): Cập nhật query `GetPeriodicEnergyStatementByGcsIdAsync` bỏ qua các bản ghi có `overall:SEND_SUCCESS`.
  - [PeriodicEnergyStatementReadOnlyRepository.cs](file:///e:/sources/MDMSv2/Backend/Infrastructure/Repositories/ReadOnly/PeriodicEnergyStatementReadOnlyRepository.cs): Tương thích truy vấn bộ lọc cho cả định dạng chuỗi cũ và JSON mới (`SEND_SUCCESS`, `SEND_FAILED`, `PARTIAL_SUCCESS`, `UNSENT`).
- **Frontend UI/UX**:
  - [table-notice.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/notice-management/components/table/table-notice.tsx):
    * Phân tích trạng thái chi tiết qua hàm `parseStatusDetail`.
    * Cột **Email nhận phiếu báo**: Hiển thị từng email kèm huy hiệu trạng thái riêng biệt:
      - Xanh lá `Đã gửi` cho email thành công.
      - Đỏ `Thất bại` cho email lỗi, kèm **Tooltip** hiển thị rõ nguyên nhân thất bại (cú pháp sai, lỗi kết nối SMTP, hộp thư từ chối,...).
      - Xám `Chờ gửi` cho email chưa gửi.
    * Cột **Trạng thái gửi Email**: Hiển thị `Thành công` (100% email đã gửi), `Gửi một phần (X/Y)` (cam, có X trên tổng số Y email đã gửi thành công), `Thất bại` (tất cả email đều lỗi), `Chưa cấu hình Email`.
    * **Checkbox chọn gửi lại**: Tự động mở khóa (Enable) khi trạng thái là `Gửi một phần` hoặc `Thất bại` để người dùng thực hiện gửi lại. Tự động khóa (Disable) khi đã `Thành công` toàn bộ để tránh gửi nhầm.
  - [search-notice.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/notice-management/components/search/search-notice.tsx): Bổ sung tùy chọn lọc "Gửi một phần" (`PARTIAL_SUCCESS`).

### 4.5. Hiệu chỉnh UI/UX Trang Quản lý Sổ GCS & Quản lý Tài khoản (Phase 4)
- **Trang Quản lý Sổ GCS**:
  * [gcs-management/index.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/gcs-management/index.tsx):
    - Bỏ ô tìm kiếm theo "Tên sổ", tinh gọn bộ lọc thành 2 ô cân đối "Mã sổ" và "Trạng thái" (`basis-1/2` mỗi ô).
    - Cập nhật nút **"Chi tiết"** mở trang chi tiết sổ GCS trên Tab mới (`_blank`).
    - Cập nhật nút **"Tra soát"** mở trang tra soát kỳ chốt trên Tab mới (`_blank`) kèm URL Query params (`id`, `name`, `code`).
  * [gcs-management/check/index.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/gcs-management/check/index.tsx):
    - Bổ sung cơ chế fallback đọc tham số từ URL Query Params (`useLocation().search`), giúp trang tra soát hoạt động hoàn hảo khi người dùng mở tab mới qua `window.open`, loại bỏ triệt để lỗi crash destructuring `location.state`.
- **Trang Quản lý Tài khoản**:
  * [account-management/index.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/account-management/index.tsx):
    - Đổi Header cột "Số điện thoại" thành **"Điện thoại"**.
    - Cập nhật trạng thái hiển thị thành **"Đang hoạt động"** (`#41A026`) & **"Vô hiệu"** (`#B22222`), đồng nhất tuyệt đối với hệ thống.
    - Cập nhật danh sách chọn lọc trạng thái thành `Đang hoạt động` / `Vô hiệu`.
    - Cập nhật nút **"Chi tiết"** mở trang chi tiết tài khoản trên Tab mới (`_blank`).
  * [account-form.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/account-management/components/account-form.tsx): Đồng bộ nhãn form thành "Điện thoại".
  * [constants.ts](file:///e:/sources/MDMSv2/Frontend/src/core/application/common/constants.ts): Cập nhật tiêu đề cột xuất file thành "Điện thoại" và thông điệp trạng thái `activeMsg: 'Đang hoạt động'`, `inactiveMsg: 'Vô hiệu'`.

### 4.6. Hiệu chỉnh UI/UX Khách Hàng, Cài đặt Dữ liệu Điểm Đo & Cấu hình Đơn vị Liên kết Dữ liệu (Phase 5)
- **Đồng bộ Nhãn Form Khách hàng**:
  * [client-form.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/client-management/components/client-form.tsx): Đồng bộ nhãn từ "Số điện thoại" sang **"Điện thoại"** áp dụng nhất quán cho cả 3 trang Thêm mới (`ADD`), Sửa (`EDIT`) và Chi tiết khách hàng (`VIEW`).
- **Trang Cài đặt Hệ thống / Dữ liệu Điểm đo**:
  * [config/index.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/config/index.tsx):
    - Loại bỏ hoàn toàn bộ lọc "Tên khách hàng" (`fullName`).
    - Bố cục 2 bộ lọc còn lại: "Mã điểm đo" và "Trạng thái" dạng `basis-1/2` trên khung chứa `basis-3/5`.
  * [table-config.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/config/components/table-config.tsx):
    - Bỏ cột "Địa chỉ" trong bảng dữ liệu điểm đo.
    - Cập nhật nút **"Chi tiết"** trong Dropdown menu mở trang liên kết dữ liệu điểm đo trên Tab trình duyệt mới (`window.open(url, '_blank')`) kèm query parameters (`code`, `name`).
- **Trang Cấu hình "Liên kết Dữ liệu" Điểm đo**:
  * [constants.ts](file:///e:/sources/MDMSv2/Frontend/src/core/application/common/constants.ts):
    - Bổ sung nhóm đơn vị chuẩn `KW: [{ label: 'kW', value: 'kW' }]` và `KVAR: [{ label: 'kVAr', value: 'kVAr' }]` vào `Constants.dataMeasurementPoint.UNITS`.
    - Cập nhật đơn vị cố định cho Thông số vận hành: Dòng điện (`A`), Công suất tác dụng (`kW`), Công suất phản kháng (`kVAr`), Tần số (`Hz`), Công suất cực đại Pmax (`kW`). Giữ nguyên Điện áp có thể chọn `V` hoặc `kV`.
    - Cập nhật đơn vị cố định cho Thông số điện năng: Điện năng tác dụng (`kWh`), Điện năng phản kháng (`kVArh`).
  * [table-tag-name.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/config/components/table-tag-name.tsx):
    - Cập nhật thuộc tính Combobox đơn vị: `disabled={val.length <= 1}`. Khi đại lượng có đơn vị cố định (chỉ có 1 đơn vị), Combobox tự động mờ đi (grayed out) và không cho sửa đổi.
    - Tối ưu hóa hàm `getUnit(record)` trả về chính xác giá trị đơn vị mặc định khi dữ liệu chưa có trong danh sách.
  * [config/detail/index.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/config/detail/index.tsx):
    - Bổ sung fallback đọc `code` và `name` từ URL Query Parameters (`useLocation().search`) khi mở trang trong tab mới, ngăn chặn lỗi crash.
    - Ép buộc gán đơn vị cố định chuẩn xác trong `getTagNameListAsync` cho toàn bộ danh sách tham số.
    - **Tính năng Xuất CSV (`handleExportCsv`)**: Xuất toàn bộ cấu hình tham số hiện tại ra file CSV chuẩn UTF-8 BOM (`LienKetDuLieu_{code}.csv`), mở trực tiếp trên Excel tiếng Việt không bị lỗi font.
    - **Tính năng Nhập CSV (`handleImportCsv`)**: Phân tích file CSV, tự động ánh xạ Tag ID, Đơn vị và Tên cảnh báo theo mã tham số hoặc tên tham số, đồng thời tôn trọng và giữ nguyên quy chuẩn đơn vị cố định.
    - **Loại bỏ tính năng Copy**: Gỡ bỏ hoàn toàn nút "Copy" (`IoCopyOutline`), dialog nhân bản dữ liệu và phương thức gọi API sao chép điểm đo.

### 4.7. Nâng cấp Cơ chế Cập nhật & Tự động Refresh Dữ liệu Giám sát Điểm đo (Phase 6)
- **Backend - Tiến trình xử lý IoT & Phát sóng SignalR**:
  * [MeasurementPointMonitoringCommandHandler.cs](file:///e:/sources/MDMSv2/Backend/Infrastructure/Handlers/Commands/IoT/MeasurementPointMonitoringCommandHandler.cs):
    - Tăng `PageSize = 1000` (thay vì giới hạn 10 bản ghi trang 1) khi truy vấn `GetListMeasurementPointsMonitoringAsync` trong tiến trình nền Hangfire `ProcessMeasurementPointMonitoring`.
    - Phát sóng toàn diện dữ liệu điểm đo đã cập nhật qua `_listHubContext.Clients.All.SendAsync("ReceiveUpdate", ...)`.
- **Frontend - Trang "Giám sát điểm đo" / "Thông số tổng hợp"**:
  * [summary-parameter/index.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/point-monitoring/summary-parameter/index.tsx):
    - Bổ sung các `useRef` (`currentParamsRef`, `dataFormRef`, `sizeRef`) duy trì trạng thái phân trang và bộ lọc hiện tại, ngăn ngừa triệt để lỗi React stale closure.
    - Xây dựng hàm `refreshCurrentPageAsync()`: Tự động gửi truy vấn ngầm với đúng con trỏ (`before`, `after`), bộ lọc (`customerName`, `measurementPointCode`) và trạng thái kết nối (`connectionStatus`), không kích hoạt thanh loading che màn hình.
    - Cơ chế đồng bộ kép (Double-Shield Sync): Khi nhận `dataSocket` từ SignalR, vừa cập nhật tức thời in-memory cho các dòng hiển thị, vừa kích hoạt `refreshCurrentPageAsync()`.
    - **Bảo toàn vị trí trang tuyệt đối**: Không bao giờ bị nhảy/reset về trang 1 khi có dữ liệu mới từ API Client gửi về REST API.
    - Bổ sung cơ chế auto-refresh dự phòng ngầm mỗi 15 giây.
- **Frontend - Trang "Giám sát điểm đo" / "Thông số chi tiết"**:
  * [socket-io-detail.tsx](file:///e:/sources/MDMSv2/Frontend/src/infrastructure/common/components/websocket/socket-io-detail.tsx):
    - Khởi tạo và quản lý kết nối độc lập cho `DetailNotificationHub`, `DetailNotificationDateTimeHub` và `ListNotificationHub`.
    - Bổ sung callback `onreconnected` tự động gọi lại `SubscribeToDetail(id)` khi kết nối SignalR được phục hồi sau sự cố mạng.
    - Dọn dẹp an toàn các kết nối khi rời trang (`UnsubscribeFromDetail` và `stop()`).
  * [summary-parameter/detail/index.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/point-monitoring/summary-parameter/detail/index.tsx):
    - Khắc phục lỗi cập nhật state `dataUser` qua functional updater `setDataUser(prev => ({ ...prev, ...dataSocket }))`.
    - Tự động gọi `getMeasurementPointMonitoringNoLoadingAsync()` khi nhận `dataSocket` từ WebSocket để cập nhật toàn diện thông số vận hành, chỉ số chốt điện năng, cảnh báo và thời gian từ REST API.
    - Bổ sung bộ đếm thời gian tự động refresh ngầm mỗi 10 giây từ REST API, đảm bảo giao diện thông số chi tiết luôn được cập nhật liên tục và mượt mà.

### 4.8. Hiệu chỉnh UI/UX Giám sát Điểm đo: Thông số tổng hợp, Bảng sự kiện & Bảng cảnh báo (Phase 7)
- **Trang "Giám sát điểm đo" / "Thông số tổng hợp"**:
  * [summary-parameter/index.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/point-monitoring/summary-parameter/index.tsx):
    - Bỏ hoàn toàn ô tìm kiếm theo "Tên khách hàng" (`name`), loại bỏ `debounceSearch` và tham số `customerName`.
    - Tái cấu trúc khu vực bộ lọc: Chia đều 2 bộ lọc "Nhập mã điểm đo" (`measurementPointCode`) tích hợp biểu tượng `<IoSearch />` và `allowClear={true}` cùng "Trạng thái kết nối" (`connectionStatus`) tỉ lệ 50-50 (`basis-1/2`).
    - Nút "Xóa tất cả" cập nhật điều kiện hiển thị thông minh: chỉ kích hoạt khi có giá trị `dataForm.measurementPointCode || dataForm.connectionStatus`.
  * [table-point.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/point-monitoring/summary-parameter/components/table-point.tsx):
    - Loại bỏ hoàn toàn cột "Mô tả điểm đo" (`measurementPointDescription`), giúp các cột thông số đo lường hiển thị gọn gàng, thoáng đãng.
    - Hiệu chỉnh nút "Chi tiết" trong menu dòng (`...`) sử dụng `window.open(url, '_blank')`, mở trang chi tiết trên tab mới, giữ nguyên trang tổng hợp và bộ lọc hiện tại.
- **Trang "Giám sát điểm đo" / "Bảng sự kiện"**:
  * [point-event/index.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/point-monitoring/point-event/index.tsx):
    - Bỏ placeholder "Nguồn", chuyển thành "Mã điểm đo" (`_t('Mã điểm đo')`) với thuộc tính `measurementPointCode` và `allowClear={true}`.
    - Đồng bộ DTO yêu cầu gửi cả `measurementPointCode` và `measurementpointCode`.
    - Cập nhật `dataExport` tách thành 2 trường riêng biệt `measurementPointCode` và `customerName`.
    - **Cơ chế tự động refresh theo REST API & SignalR**: Bổ sung `currentParamsRef` và `sizeRef` lưu giữ trạng thái phân trang, bộ lọc; triển khai hàm `refreshCurrentPageAsync()` truy vấn ngầm từ máy chủ không giật màn hình (`() => {}` thay vì `setLoading`); tích hợp đồng hồ ngầm tự làm mới mỗi 15 giây.
  * [table-event.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/point-monitoring/point-event/components/table-event.tsx):
    - Thay thế cột đơn "Nguồn" bằng 2 cột riêng biệt: "Mã điểm đo" (`dataIndex: 'measurementPointCode'`, width 12%) và "Tên khách hàng" (`dataIndex: 'customerName'`, width 20%).
    - Làm sạch và bảo vệ sự kiện click dòng `onRow` an toàn với `record.measurementPointCode`.
- **Trang "Giám sát điểm đo" / "Bảng cảnh báo"**:
  * [point-warning/index.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/point-monitoring/point-warning/index.tsx):
    - Bỏ placeholder "Nguồn", chuyển thành "Mã điểm đo" (`_t('Mã điểm đo')`) với thuộc tính `measurementPointCode` và `allowClear={true}`.
    - Đồng bộ DTO yêu cầu gửi cả `measurementPointCode` và `measurementpointCode`.
    - Cập nhật `dataExport` tách thành 2 trường riêng biệt `measurementPointCode` và `customerName`.
    - **Cơ chế tự động refresh theo REST API & SignalR**: Bổ sung `currentParamsRef` và `sizeRef` duy trì con trỏ phân trang và bộ lọc; xây dựng hàm `refreshCurrentPageAsync()` làm mới dữ liệu ngầm từ REST API khi có tín hiệu `dataSocket` hoặc `countState`; bổ sung polling ngầm mỗi 15 giây.
  * [table-warning.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/point-monitoring/point-warning/components/table-warning.tsx):
    - Thay thế cột đơn "Nguồn" bằng 2 cột riêng biệt: "Mã điểm đo" (`dataIndex: 'measurementPointCode'`, width 12%) và "Tên khách hàng" (`dataIndex: 'customerName'`, width 20%).
    - Làm sạch và bảo vệ sự kiện click dòng `onRow` an toàn với `record.measurementPointCode`.
- **Thành phần WebSocket dùng chung**:
  * [socket-common.tsx](file:///e:/sources/MDMSv2/Frontend/src/infrastructure/common/components/websocket/socket-common.tsx):
    - Đưa `endPoint` vào mảng dependencies của `useMemo` tránh tạo kết nối nhầm hub.
    - Bổ sung handler `onreconnected` tự động tái đăng ký lắng nghe sự kiện khi SignalR phục hồi kết nối.
    - Dọn dẹp sạch sẽ listener (`hubConnection.off(status)`) khi unmount hoặc đổi endpoint.
- **Hằng số Cấu hình Chung**:
  * [constants.ts](file:///e:/sources/MDMSv2/Frontend/src/core/application/common/constants.ts):
    - Cập nhật danh sách tiêu đề cột xuất file CSV (`listHeaderExport.warningParams` và `listHeaderExport.eventParams`): Thay thế header `Nguồn` bằng `Mã điểm đo` (`measurementPointCode`) và `Tên khách hàng` (`customerName`).

---

## 4. Cập nhật UI/UX Bảng Sự Kiện, Bảng Cảnh Báo & Trang Liên Kết Dữ Liệu Điểm Đo

### 4.1. Bảng Sự Kiện & Bảng Cảnh Báo (Tối ưu độ rộng cột và căn chỉnh toàn màn hình)
- **Vấn đề trước khi sửa**:
  * Tổng phần trăm độ rộng các cột trong `table-warning.tsx` (82%) và `table-event.tsx` (77%) kết hợp với cấu hình `scroll={{ x: 'max-content' }}` khiến bảng bị co lại, để lại khoảng trống màu xám/trắng lớn ở bên phải. Cột "Mã điểm đo" bị chật hẹp và xuống dòng xấu.
  * Trong `table-event.tsx`, tiêu đề cột thứ 4 bị ghi nhầm là "Tên cảnh báo".
- **Giải pháp hiện thực**:
  * [table-warning.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/point-monitoring/point-warning/components/table-warning.tsx) & [table-event.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/point-monitoring/point-event/components/table-event.tsx):
    - Phân bổ lại tỷ lệ độ rộng các cột tối ưu chiếm trọn 100% bề rộng container:
      * **Thời gian**: `16%` (đủ hiển thị `dd/mm/yyyy hh:mi:ss` không bị cắt)
      * **Mã điểm đo**: `18%` (đủ chứa các mã điểm đo dài như `TRẠM NƯỚC THẢI STP4500 PHASE 2`)
      * **Tên khách hàng**: `30%` (rộng rãi cho tên doanh nghiệp dài)
      * **Tên cảnh báo / Tên sự kiện**: `20%` (hiển thị đầy đủ tên sự kiện / cảnh báo)
      * **Trạng thái**: `10%` (`align: 'center'`)
      * **Giá trị**: `6%` (`align: 'center'`)
    - Sửa tiêu đề cột trong `table-event.tsx` thành "Tên sự kiện" cho đúng ngữ nghĩa.
    - Cập nhật `scroll={{ x: 1000, y: window.innerHeight * 0.66 }}` và truyền `loading={loading}` đảm bảo bảng luôn phủ 100% chiều rộng card và hỗ trợ cuộn ngang an toàn trên màn hình nhỏ.

### 4.2. Trang "Liên kết dữ liệu" - Vị trí & Icon Nút Thao Tác Header
- **Giải pháp hiện thực**:
  * [Frontend/src/pages/config/detail/index.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/config/detail/index.tsx):
    - Điều chỉnh lại thứ tự 3 nút trên header theo đúng yêu cầu: **"Xuất CSV"** -> **"Nhập từ CSV"** -> **"Lưu"**.
    - Cập nhật icon cho nút "Nhập từ CSV" thành `<FiDownload size={20} />`, đồng bộ hoàn toàn với phong cách icon của các trang Quản lý khách hàng và Quản lý điểm đo.
    - Cập nhật hàm `getGroupName` trong chức năng Export CSV nhận diện cả `'TIME'` và `'DATETIME'` gán đúng tên nhóm "Thời gian".

### 4.3. Trang "Liên kết dữ liệu" / Tab "Thời gian" - Gom nhóm Khoa học, Dễ tiếp cận
- **Vấn đề trước khi sửa**:
  * Toàn bộ 24 tham số thời gian hiển thị dạng bảng phẳng không có tiêu đề nhóm; thứ tự các đại lượng trong nhóm thời gian hiện tại và chốt gần nhất bị lộn xộn (giờ, phút, giây trước năm, tháng, ngày).
- **Giải pháp hiện thực**:
  * [constants.ts](file:///e:/sources/MDMSv2/Frontend/src/core/application/common/constants.ts):
    - Cấu trúc lại danh mục `dateTimeParams` thành 4 nhóm rõ ràng, mỗi nhóm gồm đúng 6 thành phần theo trật tự thời gian logic: **Năm -> Tháng -> Ngày -> Giờ -> Phút -> Giây**:
      1. **Thời gian hiện tại**: `NAM_HIEN_TAI`, `THANG_HIEN_TAI`, `NGAY_HIEN_TAI`, `GIO_HIEN_TAI`, `PHUT_HIEN_TAI`, `GIAY_HIEN_TAI`.
      2. **Thời gian chốt gần nhất**: `NAM_CHOT_GAN_NHAT`, `THANG_CHOT_GAN_NHAT`, `NGAY_CHOT_GAN_NHAT`, `GIO_CHOT_GAN_NHAT`, `PHUT_CHOT_GAN_NHAT`, `GIAY_CHOT_GAN_NHAT`.
      3. **Thời gian công suất cực đại kỳ hiện tại**: `NAM_PMAX_KY_HIEN_TAI`, `THANG_PMAX_KY_HIEN_TAI`, `NGAY_PMAX_KY_HIEN_TAI`, `GIO_PMAX_KY_HIEN_TAI`, `PHUT_PMAX_KY_HIEN_TAI`, `GIAY_PMAX_KY_HIEN_TAI`.
      4. **Thời gian công suất cực đại kỳ trước**: `NAM_PMAX_KY_TRUOC`, `THANG_PMAX_KY_TRUOC`, `NGAY_PMAX_KY_TRUOC`, `GIO_PMAX_KY_TRUOC`, `PHUT_PMAX_KY_TRUOC`, `GIAY_PMAX_KY_TRUOC`.
    - Bảo toàn 100% thuộc tính `label`, `value`, `type` đảm bảo tính tương thích ngược hoàn hảo cho API Backend, Export CSV và Import CSV.
  * [table-tag-name.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/config/components/table-tag-name.tsx):
    - Đồng bộ hoàn toàn với cấu trúc bảng chuẩn `TableCommon` của hệ thống.
    - Bổ sung cột **Nhóm** (`width: 30%`, `rowSpan: 6`) gom nhóm 4 phân loại thời gian rõ ràng: "Thời gian hiện tại", "Thời gian chốt gần nhất", "Thời gian công suất cực đại kỳ hiện tại", "Thời gian công suất cực đại kỳ trước".
    - Cột **Dữ liệu** (`width: 25%`) hiển thị mạch lạc các đại lượng: Năm, Tháng, Ngày, Giờ, Phút, Giây.
    - Cột **Tag ID** (`width: 45%`) cho phép kỹ sư nhập liệu liền mạch, đồng nhất 100% phong cách với các tab "Thông số vận hành", "Thông số điện năng", "Cảnh báo".

---

## 5. Chuẩn Hóa Toàn Diện UI/UX & Đồng Bộ Hóa Quy Trình Import CSV

### 5.1. Đồng Bộ Giao Diện Bảng Tab "Thời Gian" Trong "Liên Kết Dữ Liệu"
- **Vấn đề**: Bố cục dạng 4 thẻ lưới (2x2) trước đó bị lệch chuẩn so với giao diện bảng phẳng Ant Design `TableCommon` của các tab "Thông số vận hành", "Thông số điện năng", "Cảnh báo".
- **Giải pháp hiện thực**:
  * [table-tag-name.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/config/components/table-tag-name.tsx):
    - Đưa toàn bộ tab "Thời gian" về chuẩn chung `<TableCommon>` với header nền xám `#d3dae5`, border đồng nhất và typography `text-14-20 font-bold`.
    - Sử dụng kỹ thuật `onCell: rowSpan = 6` gộp 6 dòng cho mỗi nhóm tại cột **Nhóm**, tạo sự phân tách rành mạch giữa 4 nhóm thời gian.
    - Cột **Dữ liệu** hiển thị: Năm, Tháng, Ngày, Giờ, Phút, Giây.
    - Cột **Tag ID** chiếm 45% chiều rộng, hỗ trợ thao tác nhập liệu rộng rãi, tiện lợi.

### 5.2. Chuẩn Hóa Hộp Thoại "Nhập Từ CSV" Cho Trang "Liên Kết Dữ Liệu"
- **Vấn đề**: Khi bấm "Nhập từ CSV", hệ thống mở ngay hộp thoại chọn file của Windows thay vì mở Modal giao diện thân thiện như trang "Quản lý khách hàng" và "Quản lý điểm đo".
- **Giải pháp hiện thực**:
  * [dialog-import-config.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/config/components/dialog-import-config.tsx):
    - Xây dựng modal `DialogImportConfig` độc lập, đồng bộ 100% phong cách với `DialogImportCustomer` và `DialogImportPoint`.
    - Tích hợp vùng kéo thả tệp `Dragger` (`.csv`, UTF-8).
    - Cung cấp nút **"Tải tệp mẫu CSV"** tự động kết xuất tệp `Template_LienKetDuLieu_{MãĐiểmĐo}.csv` với đầy đủ danh mục tham số và tag hiện có.
    - Hiển thị thống kê kết quả trực quan qua các thẻ Tag: "Tổng số dòng", "Đã cập nhật", "Không khớp/Bỏ qua" và Alert thông báo dữ liệu đã nạp lên bảng.
  * [Frontend/src/pages/config/detail/index.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/config/detail/index.tsx):
    - Tích hợp `DialogImportConfig`, thay thế hoàn toàn `<input type="file" />` ẩn trước đó.

### 5.3. Khắc Phục Triệt Để Popup Hiển Thị Tiếng Anh ("Success" / "Error")
- **Nguyên nhân cốt lõi**:
  * Hàm `notifySuccess` và `notifyError` trong [toastMessage.tsx](file:///e:/sources/MDMSv2/Frontend/src/infrastructure/common/components/toast/toastMessage.tsx) có chữ ký `(_translator: any, message?: ToastContent)`.
  * Các component import (`dialog-import-point.tsx`, `dialog-import-customer.tsx`) gọi hàm với 1 tham số `notifySuccess(msg)`. Khi đó `_translator` nhận chuỗi thông báo còn `message` bị `undefined`, dẫn đến fallback mặc định `message || 'Success'` in ra chữ **"Success"** (hoặc **"Error"**) bằng tiếng Anh.
- **Giải pháp hiện thực**:
  * [toastMessage.tsx](file:///e:/sources/MDMSv2/Frontend/src/infrastructure/common/components/toast/toastMessage.tsx):
    - Viết hàm trợ giúp `extractMessage` thông minh, tự động nhận diện và trích xuất đúng nội dung thông báo dù gọi với 1 tham số (`message`) hay 2 tham số (`_translator, message`).
    - Bản địa hóa toàn bộ giá trị fallback mặc định sang tiếng Việt: `'Thành công'`, `'Có lỗi xảy ra'`, `'Cảnh báo'`, `'Thông tin'`.
  * [dialog-import-point.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/point-management/components/dialog-import-point.tsx) & [dialog-import-customer.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/client-management/components/dialog-import-customer.tsx):
    - Chuẩn hóa toàn bộ các lệnh gọi thông báo thành `notifySuccess('', msg)` và `notifyError('', msg)`.

---

## 6. Tổ Chức Lại Tab "Thời Gian" - Bỏ Phân Nhóm & Đồng Bộ Cấu Trúc Bảng Phẳng (Flat Table)

### 6.1. Bối cảnh & Yêu cầu
- Bố cục trước đó của tab "Thời gian" sử dụng cơ chế gộp ô `rowSpan = 6` và bổ sung cột "Nhóm" làm cấu trúc hiển thị khác biệt so với các tab cài đặt khác ("Thông số điện năng", "Thông số vận hành", "Cảnh báo") vốn là bảng phẳng danh sách tham số.
- Người dùng yêu cầu tổ chức lại tab "Thời gian" tương tự các trang cài đặt khác và **bỏ hoàn toàn phân nhóm**.

### 6.2. Thay đổi Kỹ thuật
- **[table-tag-name.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/config/components/table-tag-name.tsx)**:
  * Loại bỏ hoàn toàn cột "Nhóm" (`dataIndex='group'`) và logic gộp dòng `rowSpan: 6`.
  * Cột **"Dữ liệu"**: Hiển thị trực tiếp tên tham số đầy đủ từ `record.label` (`<p>{val}</p>`), bảo toàn 24 thông số chuẩn (vd: *Thời gian hiện tại (năm)*, *Thời gian hiện tại (tháng)*, ..., *Thời gian chốt gần nhất (năm)*, ..., *Thời gian công suất cực đại kỳ hiện tại (năm)*, ..., *Thời gian công suất cực đại kỳ trước (năm)*...).
  * Cột **"Tag ID"**: Ô nhập `InputTextCommon` đồng nhất, hỗ trợ nhập liệu mượt mà.
  * Phân bổ tỷ lệ cột tối ưu 100%:
    - Tab "Thời gian": Dữ liệu `50%`, Tag ID `50%`.
    - Tab "Thông số vận hành", "Thông số điện năng", "Cảnh báo": Dữ liệu `45%`, Tag ID `30%`, Đơn vị / Tên cảnh báo `25%`.

