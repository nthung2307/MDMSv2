# [DEV HANDOFF] HOTFIX: KHẮC PHỤC TRIỆT ĐỂ LỖI PHÂN TRANG NEXT/PREVIOUS VÀ GIAO DIỆN TỰ ĐỘNG RESET VỀ TRANG ĐẦU TIÊN

## 1. Nguyên nhân Gốc rễ (Root Causes)
1. **Lỗi Backend EF Core Translator với Cursor Pagination (Previous Navigation)**:
   - Trong `QueryableWithAndExtensions.cs`, `QueryableExtensions.cs`, và `QueryableAndFilterExtensions.cs`: Khi người dùng click "Previous", logic cũ gọi `finalQuery.Where(i => i.Id > Before).Reverse()` (đối với DESC) hoặc `finalQuery.Where(i => i.Id < Before).Reverse()` (đối với ASC).
   - Tuy nhiên, EF Core LINQ to Entities **hoàn toàn bỏ qua phương thức `.Reverse()`** trên `IQueryable` khi dịch sang SQL.
   - Do đó, câu lệnh SQL thực thi vẫn giữ nguyên chiều `ORDER BY Id DESC` (hoặc ASC) và `LIMIT PageSize + 1`. Kết quả trả về từ database luôn là 10 bản ghi đầu tiên của toàn bảng (Trang 1) thay vì 10 bản ghi ngay trước vị trí con trỏ `Before`! Sau đó ở bộ nhớ RAM, danh sách Trang 1 này bị đảo ngược thứ tự, khiến giao diện nhảy phắt về Trang 1 với danh sách bị đảo ngược, làm hỏng hoàn toàn luồng chuyển trang lùi.
2. **Lỗi Background Polling gọi đè dữ liệu Trang 1 (Notice Detail)**:
   - Trong `notice-detail/index.tsx`, tiến trình định kỳ `getStatusSendMailAsync` (30 giây/lần) có câu lệnh `if (!res.data)` gọi lại `getListNoticeAsync` mang theo tham số khởi tạo ban đầu (`getListNoticeRequest` chưa có `before`/`after`). Khi người dùng đang ở Trang 2, 3... thì đúng 30 giây sau, hàm này tự kích hoạt và kéo dữ liệu Trang 1 đè lên, khiến giao diện bất ngờ nhảy về Trang 1.
3. **Lỗi Ghi đè Telemetry IoT qua WebSocket (Summary Parameter)**:
   - Trong `summary-parameter/index.tsx`, mỗi khi có dữ liệu Socket IoT cập nhật, `useEffect` thực hiện `setData(dataSocket['data'])` ghi đè toàn bộ danh sách hiện tại bằng danh sách đo đếm mặc định (Trang 1) nhận từ Socket.
4. **Lỗi Lưu giữ Con trỏ Cũ khi Thay đổi Bộ lọc / Thay đổi PageSize**:
   - Ở hầu hết các trang danh sách (`client-management`, `account-management`, `point-management`, `point-event`, `point-warning`, `gcs-management`, `notice-detail`, `reports`), khi người dùng gõ tìm kiếm hoặc thay đổi bộ lọc, state request chỉ gộp thêm trường tìm kiếm `{ ...prev, searchByFields }` mà không xóa bỏ `before` / `after` của trang cũ. Dẫn đến việc tìm kiếm bị giới hạn sau ID của trang hiện tại, làm kết quả bộ lọc bị thiếu hoặc trống.

---

## 2. Chi tiết Giải pháp & Hiện thực hóa (Implementation Details)
### Backend (.NET 8 Clean Architecture)
- **Đảo chiều sắp xếp SQL khi phân trang lùi (`Before != null`)**:
  - Tại [QueryableWithAndExtensions.cs](file:///e:/sources/MDMSv2/Backend/Application/Extensions/QueryableWithAndExtensions.cs):
    * Hàm `PaginateWithAndAsync` (mặc định DESC): Khi `Before != null`, query SQL chuyển sang sắp xếp tăng dần `OrderBy(x => x.Id)` (hoặc đảo ngược chiều `SortByFields`), lấy `PageSize + 1` bản ghi có `Id > Before`. Sau khi lấy dữ liệu từ DB về bộ nhớ, đảo ngược danh sách (`result.Reverse()`) để hiển thị đúng thứ tự DESC tự nhiên.
    * Đặt lại cờ chuẩn xác: `hasNext = true`, `hasPrevious = result.Count == PageSize + 1`.
    * Hàm `PaginateWithAscAndAsync` (mặc định ASC): Khi `Before != null`, query SQL sắp xếp giảm dần `OrderByDescending(x => x.Id)`, lấy `PageSize + 1` bản ghi có `Id < Before`, sau đó `result.Reverse()` về thứ tự ASC.
  - Đồng bộ logic tương tự cho [QueryableExtensions.cs](file:///e:/sources/MDMSv2/Backend/Application/Extensions/QueryableExtensions.cs) (`PaginateAsync`) và [QueryableAndFilterExtensions.cs](file:///e:/sources/MDMSv2/Backend/Application/Extensions/QueryableAndFilterExtensions.cs) (`PaginateWithFilterAsync`).

### Frontend (React 18 + Vite)
- **Chuẩn hóa reset con trỏ phân trang trên toàn bộ các trang**:
  - [client-management/index.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/client-management/index.tsx): `addListCustomerParams` và `updateParamsValueFromListCustomerParams` tự động gọi `deleteParams(updatedParams, ['before', 'after'])`.
  - [account-management/index.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/account-management/index.tsx): Khi thay đổi tên/nhóm/trạng thái người dùng, tự động xóa `before` và `after`.
  - [point-management/index.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/point-management/index.tsx): Khi thay đổi bộ lọc điểm đo, tự động xóa `before` và `after`.
  - [notice-detail/index.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/notice-management/notice-detail/index.tsx):
    * Bổ sung `isSendingMailRef`: Chỉ kiểm tra và refresh dữ liệu khi tiến trình gửi mail đang chạy thực sự. Loại bỏ việc tự động reload Trang 1 mỗi 30 giây khi không gửi mail.
    * Khi thay đổi bộ lọc tìm kiếm hoặc kích thước trang (`setSize`), tự động xóa `before` và `after`.
  - [summary-parameter/index.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/point-monitoring/summary-parameter/index.tsx):
    * Cơ chế **In-place Telemetry Merge**: Khi nhận Socket dữ liệu IoT, chỉ cập nhật các chỉ số đo đếm (`dienAp`, `dongDien`, `congSuat`...) của các điểm đo đang hiển thị trên trang hiện tại theo `measurementPointsId`, tuyệt đối không ghi đè toàn bộ danh sách hoặc kéo về Trang 1.
    * Tách biệt bộ lọc tìm kiếm và tự động xóa `before`/`after` khi gõ tìm kiếm.
  - [point-event/index.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/point-monitoring/point-event/index.tsx) & [point-warning/index.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/point-monitoring/point-warning/index.tsx): `updateParamsValueFromListCustomerParams` tự động xóa `before` và `after`.
  - [gcs-management/index.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/gcs-management/index.tsx), [check/index.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/gcs-management/check/index.tsx), [check-detail/index.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/gcs-management/check-detail/index.tsx), [gcs-sum/index.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/gcs-sum/index.tsx): Cập nhật `setSize` và bộ lọc xóa sạch con trỏ cũ.
  - [daily-finalized-energy-report.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/reports/daily-finalized-energy-report/daily-finalized-energy-report.tsx) & [half-hour-finalized-energy-report.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/reports/half-hour-finalized-energy-report/half-hour-finalized-energy-report.tsx): Tách riêng `useEffect` reset con trỏ phân trang khi đổi khách hàng/điểm đo/thời gian.

---

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
    - Cập nhật icon nút Import thành `FiDownload` (mũi tên hướng xuống), đồng bộ kiểu dáng với nút Export `FiShare` (mũi tên hướng lên).
    - Cập nhật thứ tự các cột danh sách khách hàng: `Mã khách hàng`, `Tên khách hàng`, `Điện thoại`, `Địa chỉ`, `Trạng thái`. Cập nhật tiêu đề cột tương ứng (`Mã khách hàng` thay vì `Mã KH`, `Tên khách hàng` thay vì `Tên KH`, `Điện thoại` thay vì `Số điện thoại`).
    - Bổ sung `email` vào `tableDataSource`.
  * Cải tiến trang Quản lý danh sách phiếu báo [table-detail-gcs.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/notice-management/components/table/table-detail-gcs.tsx), [search-detail-gcs.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/notice-management/components/search/search-detail-gcs.tsx) và [notice-detail/index.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/notice-management/notice-detail/index.tsx):
    - Nút "Chi tiết" trong menu thao tác (`...`) chuyển sang mở trang Chi tiết phiếu báo trong **tab mới** (`window.open(url, '_blank')`).
    - Đồng bộ lưu trữ và phục hồi ngữ cảnh chu kỳ chốt sổ qua `localStorage` và `sessionStorage`, chống crash khi mở tab mới hoặc F5.
    - Sửa placeholder của bộ lọc Năm từ `"Tất cả năm"` thành `"Tất cả"`.
    - Khắc phục triệt để lỗi trang Chi tiết phiếu báo re-fetch dữ liệu liên tục không ngừng: Memoize `noticeState` (`useMemo`) và thay đổi dependency array của `useEffect` từ object reference `record` sang primitive ID `record?.id`, triệt tiêu hoàn toàn vòng lặp re-render vô hạn.

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

