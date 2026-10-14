# [DEV HANDOFF] QUẢN LÝ KHÁCH HÀNG: IMPORT CSV (SAFE UPSERT), PHÂN QUYỀN ADMIN (RBAC) & TỐI ƯU HÓA UI/UX

## 1. Tóm tắt Thay đổi (Summary of Changes)
Đã hoàn thành 100% các Task được giao trong [PLAN_SPEC_CUSTOMER_IMPORT.md](file:///e:/sources/MDMSv2/PLAN_SPEC_CUSTOMER_IMPORT.md):
- **Backend (.NET 8 Clean Architecture)**:
  * Thêm DTO [ImportCustomerResponse.cs](file:///e:/sources/MDMSv2/Backend/Application/DataTransferObjects/Responses/Customers/ImportCustomerResponse.cs) và [ImportCustomersCommand.cs](file:///e:/sources/MDMSv2/Backend/Application/Commands/Customers/ImportCustomersCommand.cs).
  * Khai báo interface `IImportCustomersCommandHandler` và method `ImportCustomersFromCsvAsync` trong [ICustomerReadWriteRepository.cs](file:///e:/sources/MDMSv2/Backend/Application/Repositories/ReadWrite/ICustomerReadWriteRepository.cs).
  * Hiện thực hóa `ImportCustomersFromCsvAsync` trong [CustomerReadWriteRepository.cs](file:///e:/sources/MDMSv2/Backend/Infrastructure/Repositories/ReadWrite/CustomerReadWriteRepository.cs):
    - Đọc stream CSV UTF-8 (hỗ trợ BOM), tự động nhận diện phân cách `,`, `;` hoặc `\t`.
    - Hỗ trợ tiêu đề linh hoạt Tiếng Việt và Tiếng Anh (`Mã KH`/`CustomerCode`, `Tên KH`/`FullName`, `Số điện thoại`/`PhoneNumber`, `Email`, `Địa chỉ`/`Address`, `Trạng thái`/`Status`).
    - **Safe Upsert**: Khách hàng trùng `CustomerCode` sẽ được cập nhật thông tin (`FullName`, `PhoneNumber`, `Email`, `Address`, `Status`, `ModifiedBy`, `ModifiedName`, `ModifiedTime`), giữ nguyên khóa chính `Id`, bảo toàn 100% các liên kết `measurementpoints` (điểm đo) và `usercustomers` (tài khoản portal). Khách hàng mới được cấp Snowflake ID mới.
    - Cập nhật `GenerateCustomerCsv` bổ sung cột `Email`.
  * Thêm [ImportCustomersCommandHandler.cs](file:///e:/sources/MDMSv2/Backend/Infrastructure/Handlers/Commands/Customers/ImportCustomersCommandHandler.cs).
  * Khai báo route `Customers/Import-Csv` trong [Url.cs](file:///e:/sources/MDMSv2/Backend/WebApi/Common/Url.cs).
  * Bổ sung endpoint `[HttpPost(Common.Url.Default.Customer.ImportCsv)]` với thuộc tính `[Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin])]` trong [CustomerController.cs](file:///e:/sources/MDMSv2/Backend/WebApi/Controllers/CustomerController.cs).
- **Frontend (React 18 + Vite + Ant Design 5)**:
  * Bổ sung thuộc tính `Email` vào `Constants.listHeaderExport.clients` trong [constants.ts](file:///e:/sources/MDMSv2/Frontend/src/core/application/common/constants.ts).
  * Bổ sung route `ImportCustomerCsv` vào `Endpoint.Customer` trong [endPoint.ts](file:///e:/sources/MDMSv2/Frontend/src/core/application/common/endPoint.ts).
  * Thêm `importCustomerCsv` vào [customer.interface.ts](file:///e:/sources/MDMSv2/Frontend/src/core/application/interfaces/customer.interface.ts) và [customer.service.ts](file:///e:/sources/MDMSv2/Frontend/src/infrastructure/repository/customer/services/customer.service.ts).
  * Tạo hook [useImportCustomerCsv.hook.tsx](file:///e:/sources/MDMSv2/Frontend/src/infrastructure/repository/customer/hooks/useImportCustomerCsv.hook.tsx).
  * Tạo component [dialog-import-customer.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/client-management/components/dialog-import-customer.tsx) hỗ trợ kéo thả tệp CSV, tải tệp mẫu `Template_KhachHang.csv`, hiển thị thống kê kết quả và danh sách dòng lỗi.
  * Cải tiến trang [client-management/index.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/client-management/index.tsx):
    - Lọc danh sách theo `Mã khách hàng` (`code`) và `Trạng thái` (`status`), loại bỏ ô lọc `Tên khách hàng`.
    - Menu hành động "Chi tiết": Mở trang chi tiết khách hàng tại tab trình duyệt mới (`window.open(..., '_blank')`).
    - Nút "Nhập từ CSV" bọc trong `<PrivateComponent roles={[Constants.Role.Admin.value, Constants.Role.SuperAdmin.value]}>`.
    - Bổ sung `email` vào `tableDataSource`.

## 2. Kết quả Tự kiểm tra (Self-Check Results)
- [x] **Backend .NET 8**: `dotnet build Backend/AMR.sln` -> PASS (0 errors, 18 warnings về third-party libraries).
- [x] **Frontend Linter**: `npm run check:lint` -> PASS (0 errors, 0 warnings).
- [x] **Frontend Production Build**: `npm run build:prod` (`tsc && vite build`) -> PASS (0 errors, built in 1m 60s).

## 3. Hướng dẫn Kiểm thử cho MDMS-Tester
1. Đăng nhập với tài khoản Admin/SuperAdmin -> Mở menu "Quản lý khách hàng".
2. Kiểm tra bộ lọc: Nhập mã khách hàng -> Bảng tự lọc; chọn trạng thái -> Bảng tự lọc; ô tìm tên khách hàng đã được loại bỏ.
3. Kiểm tra click menu `(...)` -> "Chi tiết": Trang chi tiết khách hàng mở ra ở tab mới.
4. Kiểm tra nút "Xuất file": File CSV tải về có thêm cột "Email".
5. Kiểm tra nút "Nhập từ CSV":
   - Nhấn "Tải tệp mẫu CSV" -> Nhận file `Template_KhachHang.csv` mẫu chuẩn UTF-8.
   - Thử sửa email của khách hàng đã có trong DB và thêm 1 khách hàng mới vào file CSV -> Upload -> Thông báo thành công và danh sách hiển thị dữ liệu mới cập nhật.
   - Kiểm tra DB: `measurementpoints` của khách hàng được cập nhật không hề bị gián đoạn hay mất liên kết.
6. Đăng nhập tài khoản không phải Admin (Staff/Customer) -> Nút "Nhập từ CSV" và "Thêm khách hàng" hoàn toàn bị ẩn trên giao diện; gọi API `POST /api/v1/Customers/Import-Csv` trả về HTTP 403 Forbidden.

---

## 4. Hotfix Refinement: Sắp xếp Xuất CSV (Cũ đến Mới) & Khắc phục Lỗi Import từ Excel
- **Yêu cầu 1: Sắp xếp File CSV xuất ra từ cũ tới mới (Khách hàng cũ xuất hiện đầu danh sách)**:
  * Thay đổi truy vấn trong [CustomerReadWriteRepository.cs](file:///e:/sources/MDMSv2/Backend/Infrastructure/Repositories/ReadWrite/CustomerReadWriteRepository.cs): `OrderBy(x => x.Id)` (thay vì `OrderByDescending(x => x.Id)`). Do ID Snowflake có thứ tự thời gian tăng dần, danh sách xuất ra sẽ luôn có khách hàng cũ nhất ở đầu.
  * Thêm UTF-8 BOM (`new UTF8Encoding(true)`) khi ghi file CSV để Microsoft Excel tự động nhận diện bảng mã UTF-8 chuẩn Tiếng Việt có dấu khi mở file.
  * Bổ sung đuôi mở rộng `.csv` trong tên file tải về tại [dialog-export.tsx](file:///e:/sources/MDMSv2/Frontend/src/infrastructure/common/components/dialog-export/dialog-export.tsx).
- **Yêu cầu 2: Khắc phục triệt để lỗi Import CSV sau khi chỉnh sửa bằng Microsoft Excel**:
  * **Nguyên nhân cốt lõi 1 (Thiếu Multipart Boundary trong Axios)**: Tại [request.service.ts](file:///e:/sources/MDMSv2/Frontend/src/infrastructure/services/request.service.ts), `makeUploadRequestAsync` gán thủ công `headers['Content-Type'] = 'multipart/form-data'` khiến Axios bỏ qua việc sinh tham số `boundary`. Kestrel phía Backend không phân tích được body multipart và trả về HTTP 400 Bad Request. Đã sửa: Xóa `headers['Content-Type']` để trình duyệt tự động đính kèm `boundary=----WebKitFormBoundary...`.
  * **Nguyên nhân cốt lõi 2 (Đa bảng mã từ Excel)**: Khi người dùng mở và lưu lại file bằng Excel trên Windows, file có thể được lưu ở dạng UTF-8 BOM, UTF-16, ANSI hoặc Windows-1258 khiến dấu Tiếng Việt ở tiêu đề bị biến dạng. Đã bổ sung hàm `DecodeCsvBytes` tự động phát hiện BOM và fallback bảng mã, kết hợp `NormalizeHeaderText` chuẩn hóa bỏ dấu Tiếng Việt (`Mã KH` -> `ma kh`) và cơ chế Positional Fallback cho vị trí các cột chuẩn.
  * **Nguyên nhân cốt lõi 3 (Binding an toàn phía Backend)**: Tại [CustomerController.cs](file:///e:/sources/MDMSv2/Backend/WebApi/Controllers/CustomerController.cs), thêm `[FromForm(Name = "file")]` và cơ chế fallback `Request.Form.Files["file"]`.
  * **Nguyên nhân cốt lõi 4 (Chống xung đột trùng khóa)**: Thay thế `ToDictionaryAsync` bằng Dictionary khởi tạo an toàn, loại trừ khả năng crash 500 nếu database có khách hàng trùng mã.
  * Đã publish gói cài đặt mới vào `Publish/MDMSv2_InstallPackage` (Backend Release + Frontend Dist).

---

## 5. Hotfix Refinement 2: Khắc phục Treo Trình duyệt khi Import & Cập nhật Icon Phân biệt Nút
- **Yêu cầu 1: Khắc phục hiện tượng Import làm treo/đơ trình duyệt**:
  * **Nguyên nhân cốt lõi**: Trong [request.service.ts](file:///e:/sources/MDMSv2/Frontend/src/infrastructure/services/request.service.ts), hàm `makeUploadRequestAsync` trước đó gọi `setRecoilStateAsync(LoadingState, { isLoading: true })` kích hoạt component `<FullPageLoading />` che phủ toàn màn hình, nhưng khối `finally` hủy loading bị comment. Toàn bộ viewport bị lớp phủ loading vô hình chặn toàn bộ sự kiện click/scroll của người dùng.
  * **Xử lý**: Loại bỏ `FullPageLoading` toàn cục tại hàm upload (nhất quán với các API khác trong dự án). Tiến trình tải được hiển thị trực tiếp và mượt mà ngay trên nút "Thực hiện nhập" của Dialog qua trạng thái `loading={loading}` của Ant Design Button.
  * Đồng thời sửa lỗi bóc tách `payload.data` trong [useImportCustomerCsv.hook.tsx](file:///e:/sources/MDMSv2/Frontend/src/infrastructure/repository/customer/hooks/useImportCustomerCsv.hook.tsx) để hook không bị nhảy nhầm vào `onError`.
  * Sửa lỗi thẻ tải về `const link = document.createElement('a'); link.href = url;` trong [dialog-export.tsx](file:///e:/sources/MDMSv2/Frontend/src/infrastructure/common/components/dialog-export/dialog-export.tsx).
- **Yêu cầu 2: Cập nhật Icon trực quan, phân biệt rõ ràng 2 nút Xuất/Nhập**:
  * Trước đó: Nút Xuất (`FiShare`) và Nút Nhập (`FiUpload`) đều có mũi tên hướng ra ngoài/lên trên gây nhầm lẫn thị giác.
  * Đã cập nhật:
    - Nút **Xuất file**: Giữ nguyên `FiShare` (mũi tên xuất ra ngoài - chuẩn thống nhất toàn hệ thống MDMSv2).
    - Nút **Nhập từ CSV**: Đổi sang `FaFileImport` từ `react-icons/fa6` (biểu tượng tệp tài liệu với mũi tên nạp dữ liệu vào trong `📄⮞`), phân biệt 100% rõ ràng và trực quan.
- **Đóng gói**: Đã build sạch toàn bộ, publish Backend Release và Frontend Dist mới nhất vào `Publish/MDMSv2_InstallPackage`.

