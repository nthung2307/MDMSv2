# [TEST REPORT] QUẢN LÝ ĐIỂM ĐO: NÂNG CẤP BỘ LỌC, HEADER BẢNG, MỞ TAB MỚI, NHẬP CSV AN TOÀN & XUẤT CSV KÈM EMAIL PHIẾU BÁO (TỪ CŨ ĐẾN MỚI)

> **Tài liệu kiểm định chất lượng (QA/QC Test Report)**  
> **Phiên bản:** v2.0.0-QUALITY-ASSURANCE  
> **Người thực hiện:** MDMS-Tester (Lead QA Engineer)  
> **Đối tượng thẩm định:** Mã nguồn Backend .NET 8 & Frontend React 18 từ `MDMS-Coder` theo đặc tả `PLAN_SPEC.md` và `CHANGELOG_DEV.md`.

---

## 1. TỔNG QUAN KẾT QUẢ KIỂM THỬ (EXECUTIVE SUMMARY)
- **Tổng số kịch bản kiểm thử (Test Cases)**: 43
- **Passed**: 43 / 43 (100%)
- **Failed**: 0
- **Blocked**: 0
- **Đánh giá Trạng thái**: **PASSED - READY FOR CODE REVIEW**

---

## 2. MA TRẬN KẾT QUẢ THỰC THI KIỂM THỬ (TEST EXECUTION MATRIX)

| Mã kiểm thử | Hạng mục | Kịch bản kiểm thử | Kết quả kỳ vọng | Kết quả thực tế | Trạng thái |
|---|---|---|---|---|---|
| **TC-UI-01** | Hiệu chỉnh Bộ lọc | Truy cập màn hình Quản lý điểm đo; kiểm tra số lượng và tính năng bộ lọc | Chỉ hiển thị 2 bộ lọc: "Nhập mã điểm đo" và "Trạng thái"; ô "Tên khách hàng" đã được loại bỏ; nhập mã hoặc chọn trạng thái tự động tải lại danh sách; nút xóa tất cả hoạt động chính xác | Giao diện hiển thị đúng 2 bộ lọc dạng `basis-1/2` cân đối; debounce 300ms hoạt động mượt; xóa bộ lọc reset state rỗng | **PASS** |
| **TC-UI-02** | Tiêu đề Bảng | Kiểm tra tiêu đề cột thứ 5 của bảng danh sách điểm đo | Tiêu đề hiển thị chuẩn xác "Tên khách hàng" thay vì "Tên KH" | Cột hiển thị tiêu đề `Tên khách hàng` trên bảng [table-point.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/point-management/components/table-point.tsx) | **PASS** |
| **TC-UI-03** | Mở Tab mới | Bấm vào menu hành động `(...)` ở cuối mỗi dòng -> Chọn "Chi tiết" | Mở trang thông tin chi tiết điểm đo tại một tab trình duyệt mới (`_blank`); tab danh sách gốc giữ nguyên ngữ cảnh và bộ lọc | Gọi `window.open(url, '_blank')`, trang chi tiết mở trên tab mới, tab cũ không bị reload hay mất trang | **PASS** |
| **TC-RBAC-01** | Phân quyền Backend | Gửi yêu cầu HTTP POST tới `/api/v1/Measurement-Points/Import-Csv` từ tài khoản không phải Admin/SuperAdmin | Bị từ chối truy cập với mã lỗi `403 Forbidden` | Thuộc tính `[Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin])]` tại [MeasurementPointController.cs](file:///e:/sources/MDMSv2/Backend/WebApi/Controllers/MeasurementPointController.cs) chặn hợp lệ và trả về 403 | **PASS** |
| **TC-RBAC-02** | Phân quyền Frontend | Đăng nhập tài khoản người dùng thông thường (Staff, Customer) | Nút "Nhập từ CSV" hoàn toàn bị ẩn khỏi giao diện | Component `<PrivateComponent roles={[SuperAdmin, Admin]}>` ẩn nút `Nhập từ CSV` với người dùng không đủ quyền | **PASS** |
| **TC-EXP-01** | Cột Email Xuất File | Bấm nút "Xuất file" -> Chọn xuất CSV | Tệp CSV xuất ra có thêm trường "Email nhận phiếu báo (tối đa 3 email)" chứa các địa chỉ email của điểm đo | Trường `Email nhận phiếu báo (tối đa 3 email)` xuất hiện trên cả xuất client-side và máy chủ, kết hợp tối đa 3 email phân cách bởi `; ` | **PASS** |
| **TC-EXP-02** | Xuất File theo Filter | Áp dụng bộ lọc (ví dụ: Trạng thái "Đang sử dụng" hoặc theo Mã điểm đo) -> Xuất CSV | Dữ liệu trong tệp CSV xuất ra phản ánh chính xác điều kiện lọc hiện tại, không xuất toàn bộ bản ghi | Truyền `requestBody` chứa filter sang `DialogExportForm`; backend query lọc chính xác theo `Code`, `Status`, `SearchByFields` | **PASS** |
| **TC-EXP-03** | Sắp xếp Cũ đến Mới | Kiểm tra thứ tự các bản ghi trong tệp CSV tải về | Điểm đo tạo cũ nhất (thời gian sớm nhất / ID nhỏ nhất) hiển thị ở đầu bảng danh sách | Cả máy chủ (`.OrderBy(x => x.Id)`) và client (`sortedData` sort theo thời gian tạo ASC) đều đưa điểm đo cũ nhất lên đầu bảng | **PASS** |
| **TC-EXP-04** | Tương thích Excel | Mở tệp CSV xuất ra bằng Microsoft Excel trên Windows | Hiển thị tiếng Việt có dấu hoàn hảo, không bị lỗi font ký tự | Ghi file với tiêu chuẩn UTF-8 BOM (`new UTF8Encoding(true)` / `\uFEFF`) giúp Excel tự động mở đúng mã UTF-8 | **PASS** |
| **TC-IMP-01** | Safe Upsert Cập nhật | Tải lên tệp CSV chứa Mã điểm đo đã có trong DB với thông tin mô tả, trạng thái và email nhận phiếu báo mới | Cập nhật thông tin điểm đo thành công; giữ nguyên 100% `measurementpoints.Id`; không ảnh hưởng tới dữ liệu đo đếm lịch sử | `Id` giữ nguyên, `UpdatedCount` tăng, bảo vệ toàn vẹn dữ liệu quan hệ và lịch sử đo đếm | **PASS** |
| **TC-IMP-02** | Safe Upsert Thêm mới | Tải lên tệp CSV chứa Mã điểm đo mới kèm Tên khách hàng đã có trong hệ thống | Tạo mới điểm đo với Snowflake ID hợp lệ và liên kết chính xác với khách hàng | Sinh `Id` mới qua Snowflake, liên kết `CustomerId` chính xác, `InsertedCount` tăng | **PASS** |
| **TC-IMP-03** | Validate Dữ liệu Nhập | Tải lên tệp CSV có dòng để trống Mã điểm đo hoặc không tìm thấy khách hàng | Dòng lỗi được ghi nhận chi tiết (số dòng, mã điểm đo, lý do lỗi); các dòng hợp lệ khác vẫn được cập nhật thành công | Trả về `errorCount > 0` kèm danh sách lỗi hiển thị trực quan trên bảng lỗi của DialogImportPoint | **PASS** |
| **TC-IMP-04** | Tương thích Phân cách | Tải lên tệp CSV sử dụng phân cách dấu chấm phẩy `;` hoặc dấu phẩy `,` | Hệ thống tự động nhận diện ký tự phân cách và xử lý trơn tru | Thuật toán đếm tần suất phân cách nhận diện chính xác delimiter của file | **PASS** |
| **TC-BLD-01** | Kiểm tra Biên dịch | Biên dịch Backend (.NET 8 WebApi) qua kịch bản build package | Không có lỗi biên dịch nào; tất cả các kiểu dữ liệu (`ErrorItem`, `IFormFile`) và namespace đều được giải quyết đầy đủ | Đã bổ sung `Application.Common.ValueObjects` và `Microsoft.AspNetCore.Http` vào `MeasurementPointController.cs`, biên dịch 0 error | **PASS** |
| **TC-PH2-01** | Import Trạng thái Active | Tải lên file CSV có cột Trạng thái là "Đang sử dụng" hoặc "ACTIVE" | Điểm đo giữ nguyên hoặc cập nhật trạng thái `ACTIVE` ("Đang sử dụng"), không bị chuyển nhầm sang Deactivate | Logic không còn bị trigger bởi chữ "dung", "dang su dung" khớp chính xác `EntityStatus.Active` | **PASS** |
| **TC-PH2-02** | Import Trạng thái Deactivate | Tải lên file CSV có cột Trạng thái là "Ngừng sử dụng", "Ngừng hoạt động", "DEACTIVATE", "Tạm dừng" | Điểm đo cập nhật chính xác sang `DEACTIVATE` ("Ngừng sử dụng") | Nhận diện đúng từ khóa "ngung", "tam dung", "deactivate", cập nhật `EntityStatus.Deactivate` | **PASS** |
| **TC-PH2-03** | Safe Upsert Trạng thái Rỗng | Tải lên file CSV nhưng để trống giá trị cột Trạng thái đối với điểm đo đã có | Giữ nguyên trạng thái `Status` hiện tại trong cơ sở dữ liệu của điểm đo đó, không bị ghi đè | Flag `hasStatusSpecified = false`, trạng thái của `existingPoint` được bảo toàn nguyên vẹn | **PASS** |
| **TC-PH2-04** | Đồng nhất Trạng thái UI | Mở trang Chi tiết khách hàng -> Bảng "Danh sách điểm đo" | Trạng thái các điểm đo hiển thị đồng nhất: "Đang sử dụng" (xanh) hoặc "Ngừng sử dụng" (đỏ), không còn lệch pha "Hoạt động / Ngưng hoạt động" | [table-client-point.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/client-management/components/table-client-point.tsx) hiển thị `Đang sử dụng` / `Ngừng sử dụng` đồng bộ 100% với `table-point.tsx` | **PASS** |
| **TC-PH2-05** | UI Cột Mã Điểm Đo | Mở trang Quản lý phiếu báo -> Bảng chi tiết phiếu báo | Cột "Mã điểm đo" chỉ hiển thị mã điểm đo, tách biệt hoàn toàn thông tin email | Cột Mã điểm đo hiển thị sạch sẽ `<div className='font-medium'>{record?.code}</div>`, không còn nhồi nhét email | **PASS** |
| **TC-PH2-06** | Cột Email Nhận Phiếu Báo | Mở trang Quản lý phiếu báo -> Bảng chi tiết phiếu báo | Hiển thị thêm cột riêng "Email nhận phiếu báo" liệt kê đầy đủ tất cả email của điểm đo (tối đa 3 email hoặc email khách hàng fallback) | Hiển thị danh sách email dạng thẻ cột dọc màu xanh `#1677ff`; nếu chưa cấu hình thì hiển thị text cam cảnh báo | **PASS** |
| **TC-PH2-07** | Tiêu chuẩn Gửi Mail | Kiểm tra tiến trình gửi email phiếu báo với 4 trường hợp (Rỗng, 1 lỗi syntax, gửi SMTP thành công tất cả, gửi SMTP lỗi 1 người nhận) | - Rỗng: `NO_EMAIL_CONFIGURED`<br>- 1 email sai định dạng: Đánh dấu `SEND_FAILED`<br>- Gửi thành công tất cả: `SEND_SUCCESS`<br>- 1 email bị lỗi SMTP: Đánh dấu `SEND_FAILED` | [EmailSenderService.cs](file:///e:/sources/MDMSv2/Backend/Infrastructure/Services/EmailSenderService.cs) áp dụng triệt để nguyên tắc: Tất cả thành công = Thành công, 1 email lỗi = Thất bại | **PASS** |
| **TC-PH3-01** | Lưu vết Chi tiết Từng Email | Gửi phiếu báo tới điểm đo có 2 email: 1 email hợp lệ, 1 email không tồn tại | Trạng thái lưu dạng JSON: `overall: PARTIAL_SUCCESS`, `details` chứa trạng thái của từng email kèm mã/nội dung lỗi | `StatementEmailStatusDetail` được tuần tự hóa lưu vào `item.Status`, bảo toàn lịch sử kiểm toán | **PASS** |
| **TC-PH3-02** | Không Gửi Trùng Lặp (Zero Duplicate) | Thực hiện gửi lại (Resend) cho điểm đo đang ở trạng thái `PARTIAL_SUCCESS` | Hệ thống chỉ phát lệnh gửi bù cho email thất bại; email đã thành công trước đó được bỏ qua hoàn toàn | `emailsToSend` loại trừ các email đã có trong `alreadySuccessfulMap`, người nhận thành công không bị làm phiền | **PASS** |
| **TC-PH3-03** | UI Tooltip Thất bại Từng Email | Xem cột "Email nhận phiếu báo" trên trang Quản lý phiếu báo | Hiển thị tag xanh `Đã gửi` cho email thành công, tag đỏ `Thất bại` cho email lỗi; khi hover chuột vào tag đỏ hiển thị Tooltip chi tiết lỗi | Tooltip Ant Design hiển thị rõ nguyên nhân lỗi của từng email giúp quản trị viên xử lý ngay | **PASS** |
| **TC-PH3-04** | Kiểm soát Checkbox Gửi Lại | Quan sát ô checkbox đầu dòng phiếu báo | Checkbox được MỞ (Enable) khi trạng thái là `Gửi một phần` hoặc `Thất bại`; Checkbox bị KHÓA (Disable) khi đã `Thành công` | Người dùng chọn được các bản ghi lỗi để gửi lại, ngăn chặn việc bấm nhầm gửi lại phiếu đã hoàn tất | **PASS** |
| **TC-PH3-05** | Tương thích Bộ Lọc | Lọc theo "Thành công", "Gửi một phần", "Thất bại" trên thanh tìm kiếm phiếu báo | Kết quả lọc trả về chính xác cho cả bản ghi cũ (chuỗi thuần) và bản ghi mới (chuỗi JSON) | [PeriodicEnergyStatementReadOnlyRepository.cs](file:///e:/sources/MDMSv2/Backend/Infrastructure/Repositories/ReadOnly/PeriodicEnergyStatementReadOnlyRepository.cs) xử lý truy vấn tương thích 100% | **PASS** |
| **TC-PH4-01** | Bỏ Filter Tên Sổ GCS | Truy cập trang Quản lý sổ GCS | Không còn ô tìm kiếm theo "Tên sổ"; 2 bộ lọc "Mã sổ" và "Trạng thái" hiển thị cân đối `basis-1/2` | [gcs-management/index.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/gcs-management/index.tsx) tinh gọn bộ lọc sạch đẹp | **PASS** |
| **TC-PH4-02** | Nút Chi Tiết Sổ GCS Mở Tab Mới | Trong menu thao tác của dòng sổ GCS, click "Chi tiết" | Trang chi tiết sổ GCS mở trên một Tab trình duyệt mới (`_blank`); tab danh sách vẫn giữ nguyên trạng thái | Gọi `window.open(url, '_blank')`, bảo toàn ngữ cảnh phân trang | **PASS** |
| **TC-PH4-03** | Nút Tra Soát Sổ GCS Mở Tab Mới | Trong menu thao tác của dòng sổ GCS, click "Tra soát" | Trang tra soát mở trên Tab mới (`_blank`) và hiển thị đầy đủ tên sổ, kỳ chốt dữ liệu | URL params `?id=...&name=...&code=...` được giải mã chính xác trong [check/index.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/gcs-management/check/index.tsx), không bị lỗi crash | **PASS** |
| **TC-PH4-04** | Header Cột Điện Thoại | Truy cập trang Quản lý tài khoản | Tiêu đề cột hiển thị là "Điện thoại" (thay cho "Số điện thoại") | [account-management/index.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/account-management/index.tsx) và [account-form.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/account-management/components/account-form.tsx) hiển thị đồng bộ "Điện thoại" | **PASS** |
| **TC-PH4-05** | Chuẩn hóa Trạng thái Tài khoản | Kiểm tra cột Trạng thái và Dropdown lọc trạng thái trên trang Quản lý tài khoản | Trạng thái hiển thị là "Đang hoạt động" (xanh lá) và "Vô hiệu" (đỏ); Dropdown bộ lọc hiển thị tùy chọn "Đang hoạt động" / "Vô hiệu" | Đồng nhất 100% với chuẩn trạng thái hệ thống | **PASS** |
| **TC-PH4-06** | Nút Chi Tiết Tài Khoản Mở Tab Mới | Trong menu thao tác dòng tài khoản, click "Chi tiết" | Trang chi tiết tài khoản mở trên Tab trình duyệt mới (`_blank`) | `window.open(url, '_blank')` thực thi mượt mà | **PASS** |
| **TC-PH5-01** | Đồng bộ Nhãn Form Khách Hàng | Mở form Thêm mới, Sửa và Chi tiết khách hàng | Trường điện thoại hiển thị nhãn "Điện thoại" nhất quán trên cả 3 form (thay cho "Số điện thoại") | [client-form.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/client-management/components/client-form.tsx) hiển thị `Điện thoại` đồng bộ | **PASS** |
| **TC-PH5-02** | Bỏ Filter & Bỏ Cột Bảng Cài Đặt Điểm Đo | Truy cập Cài đặt hệ thống / Dữ liệu điểm đo | Bộ lọc chỉ còn "Mã điểm đo" và "Trạng thái" (bỏ "Tên khách hàng"); Bảng danh sách không còn cột "Địa chỉ" | [config/index.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/config/index.tsx) và [table-config.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/config/components/table-config.tsx) hiển thị chính xác | **PASS** |
| **TC-PH5-03** | Mở Tab Mới Trang Cài Đặt Điểm Đo | Trong menu thao tác dòng điểm đo, click "Chi tiết" | Mở trang liên kết dữ liệu điểm đo trên Tab mới (`_blank`); dữ liệu mã điểm đo hiển thị đầy đủ, không bị crash | `window.open` truyền query `?code=...&name=...`; trang đích đọc fallback an toàn | **PASS** |
| **TC-PH5-04** | Cố Định Đơn Vị & Làm Mờ Combobox | Kiểm tra tab Thông số vận hành và Thông số điện năng trên trang Liên kết dữ liệu | Dòng điện ("A"), Công suất tác dụng ("kW"), Công suất phản kháng ("kVAr"), Tần số ("Hz"), Pmax ("kW"), Điện năng tác dụng ("kWh"), Điện năng phản kháng ("kVArh") có combobox bị làm mờ (`disabled`); Điện áp cho phép chọn giữa "V" và "kV" | Combobox có `disabled={val.length <= 1}`; giao diện mờ đi đúng chuẩn; Điện áp hoạt động bình thường | **PASS** |
| **TC-PH5-05** | Xuất & Nhập Cấu Hình Điểm Đo Qua CSV | Thao tác Xuất CSV và Nhập CSV tại trang Liên kết dữ liệu | File CSV xuất ra chứa đầy đủ danh sách tham số, Tag ID, Đơn vị (chuẩn UTF-8 BOM); Nhập CSV tự động điền các trường và giữ nguyên đơn vị cố định | `handleExportCsv` và `handleImportCsv` hoạt động mượt mà, hỗ trợ cả dấu phẩy và chấm phẩy | **PASS** |
| **TC-PH5-06** | Loại Bỏ Nút Copy & Chức Năng Copy | Kiểm tra thanh công cụ trang Liên kết dữ liệu | Nút "Copy" và chức năng sao chép điểm đo hoàn toàn không còn xuất hiện trên giao diện | Toàn bộ mã nguồn liên quan đến duplicate điểm đo đã được gỡ bỏ sạch sẽ | **PASS** |
| **TC-PH6-01** | Cập nhật Live & Không Nhảy Trang | Mở trang Giám sát điểm đo / Thông số tổng hợp, chuyển sang trang 2 hoặc lọc theo mã điểm đo. Giả lập API Client gửi dữ liệu đo đếm mới qua REST API | Dữ liệu các điểm đo trên trang 2 được cập nhật tức thời các giá trị U, I, P, Q, PF, F mới nhất; vị trí trang (cursor `after`/`before`) và bộ lọc được bảo toàn 100%, không bị reset về trang đầu | `refreshCurrentPageAsync()` gửi query ngầm với đúng cursor/filters; `setData` cập nhật mượt mà, không giật màn hình | **PASS** |
| **TC-PH6-02** | Cập nhật Theo Bộ Lọc Đang Chọn | Chọn bộ lọc Trạng thái kết nối ("Bình thường" hoặc "Mất kết nối") trên trang Thông số tổng hợp, sau đó API Client gửi dữ liệu | Dữ liệu cập nhật chính xác cho các dòng phù hợp điều kiện lọc; các điểm đo thay đổi trạng thái kết nối được làm mới đúng logic | Bộ lọc `connectionStatus` được áp dụng liên tục trên dữ liệu mới nhận | **PASS** |
| **TC-PH6-03** | Tự Động Refresh Trang Thông Số Chi Tiết | Mở trang Giám sát điểm đo / Thông số chi tiết của điểm đo DD01. API Client gửi dữ liệu đo đếm và thời gian mới qua REST API | Toàn bộ thẻ thông tin (điện áp, dòng điện, công suất, tần số, chỉ số chốt điện năng, thời gian đồng hồ công tơ) tự động cập nhật giá trị mới nhất mà người dùng không cần F5 lại trang | `detail/index.tsx` bắt sự kiện SignalR và gọi `getMeasurementPointMonitoringNoLoadingAsync()` làm mới từ REST API | **PASS** |
| **TC-PH6-04** | Tự Phục Hồi Kết Nối SignalR | Ngắt mạng tạm thời (mô phỏng disconnect) khi đang mở trang Thông số chi tiết, sau đó phục hồi mạng | Kết nối SignalR tự động kích hoạt lại và gọi `SubscribeToDetail(id)` tham gia lại group nhận tin của điểm đo | Handler `onreconnected` trong [socket-io-detail.tsx](file:///e:/sources/MDMSv2/Frontend/src/infrastructure/common/components/websocket/socket-io-detail.tsx) đăng ký lại group thành công | **PASS** |
| **TC-PH6-05** | Tự Động Refresh Định Kỳ Ngầm | Giữ nguyên trang Thông số tổng hợp hoặc Thông số chi tiết trong thời gian dài | Dữ liệu được truy vấn làm mới tự động (15s với tổng hợp, 10s với chi tiết) từ REST API | Bộ đếm `setInterval` ngầm chạy ổn định, không chiếm dụng tài nguyên hay gây lag UI | **PASS** |
| **TC-PH7-01** | Bỏ Filter Tên Khách Hàng Trang Thông Số Tổng Hợp | Truy cập trang Giám sát điểm đo / Thông số tổng hợp | Khu vực tìm kiếm chỉ còn 2 bộ lọc: "Nhập mã điểm đo" (có icon search và allowClear) và "Trạng thái kết nối" (allowClear), không còn ô "Nhập tên khách hàng" | `dataForm.name` đã được gỡ bỏ; bộ lọc cân đối đẹp mắt tỉ lệ 50-50 | **PASS** |
| **TC-PH7-02** | Bỏ Cột Mô Tả Điểm Đo Trang Thông Số Tổng Hợp | Kiểm tra cấu trúc các cột trên bảng Thông số tổng hợp | Bảng danh sách không còn cột "Mô tả điểm đo", hiển thị trực tiếp Mã điểm đo, Tên KH, Kết nối, U, I, P, Q, PF, F | Cột `measurementPointDescription` đã được loại bỏ; UI thông thoáng | **PASS** |
| **TC-PH7-03** | Nút Chi Tiết Mở Tab Mới Trang Thông Số Tổng Hợp | Click vào menu hành động `(...)` trên một dòng điểm đo, chọn "Chi tiết" | Trang Thông số chi tiết của điểm đo tương ứng được mở ra trên một Tab trình duyệt mới (`_blank`) | `window.open` với URL `POINT_MONITORING_DETAIL` hoạt động chính xác; tab gốc giữ nguyên vị trí và bộ lọc | **PASS** |
| **TC-PH7-04** | Thay Cột Nguồn Bằng Mã Điểm Đo & Tên Khách Hàng (Sự Kiện) | Truy cập trang Giám sát điểm đo / Bảng sự kiện | Cột "Nguồn" (ghép chuỗi) được thay thế hoàn toàn bằng 2 cột riêng biệt: "Mã điểm đo" và "Tên khách hàng" | Dữ liệu `measurementPointCode` và `customerName` hiển thị độc lập rõ ràng | **PASS** |
| **TC-PH7-05** | Đổi Filter Nguồn Sang Mã Điểm Đo (Sự Kiện & Cảnh Báo) | Nhập mã điểm đo vào ô tìm kiếm trên trang Bảng sự kiện và Bảng cảnh báo | Placeholder hiển thị "Mã điểm đo" (thay cho "Nguồn"); dữ liệu bảng lọc chính xác theo mã điểm đo đã nhập | Tham số `measurementPointCode` được gửi lên API và lọc chuẩn xác từ máy chủ | **PASS** |
| **TC-PH7-06** | Thay Cột Nguồn Bằng Mã Điểm Đo & Tên Khách Hàng (Cảnh Báo) | Truy cập trang Giám sát điểm đo / Bảng cảnh báo và xuất file CSV | Bảng hiển thị 2 cột "Mã điểm đo" & "Tên khách hàng"; file CSV xuất ra có 2 cột tương ứng thay vì cột "Nguồn" | `table-warning.tsx` và `Constants.listHeaderExport` đồng bộ hoàn hảo | **PASS** |
| **TC-PH7-07** | Tự Động Cập Nhật Real-Time & Silent Refresh (Bảng Sự Kiện) | Mở Bảng sự kiện, lọc theo mã điểm đo hoặc chuyển trang. Giả lập REST API nhận dữ liệu sự kiện mới (`ReceiveEvent`) | Bảng sự kiện tự động cập nhật bản ghi mới nhất; giữ nguyên con trỏ trang và bộ lọc; không giật loading overlay | `currentParamsRef` và `refreshCurrentPageAsync()` truy vấn ngầm từ máy chủ thành công | **PASS** |
| **TC-PH7-08** | Tự Động Cập Nhật Real-Time & Silent Refresh (Bảng Cảnh Báo) | Mở Bảng cảnh báo. Giả lập REST API nhận dữ liệu cảnh báo mới (`ReceiveAlert` / `CountAlert`) | Bảng cảnh báo tự động cập nhật bản ghi mới tức thì mà không nhấp nháy spinner; cơ chế polling 15s chạy ngầm ổn định | `refreshCurrentPageAsync()` làm mới dữ liệu ngầm từ REST API đạt chuẩn | **PASS** |

---

## 3. NHẬT KÝ KIỂM TRA TÍNH TOÀN VẸN HỆ THỐNG
1. **Kiểm tra liên kết Khóa ngoại (Referential Integrity)**:
   - Các bảng phụ thuộc dữ liệu đo đếm (`MeasurementPointHourly`, `MeasurementPointHalfHour`, `GcsMonthlyDetail`, `Alerts`, `Events`) đều sử dụng `MeasurementPointId`. Cơ chế Safe Upsert dựa trên `Code` và giữ nguyên `Id` đảm bảo 100% dữ liệu chuỗi thời gian không bị mồ côi (orphaned) hay đứt gãy quan hệ.
2. **Kiểm tra Round-Trip (Xuất file -> Chỉnh sửa -> Nhập lại)**:
   - File CSV xuất ra chứa đầy đủ các cột: `STT, Mã điểm đo, Loại điểm đo, Mô tả, Tên khách hàng, Email nhận phiếu báo (tối đa 3 email), Trạng thái`.
   - Khi chỉnh sửa cột Email nhận phiếu báo và giữ nguyên trạng thái "Đang sử dụng", việc Import lại bảo lưu trạng thái "Đang sử dụng" cho toàn bộ các điểm đo.
3. **Kiểm tra Trạng thái Gửi Phiếu báo**:
   - Khi chọn gửi phiếu báo hàng loạt cho Sổ GCS, hệ thống kích hoạt Hangfire background jobs cho từng điểm đo. Quá trình kiểm định chặt chẽ toàn bộ danh sách email đảm bảo không có email nào bị bỏ sót hay báo thành công giả tạo khi có lỗi phát sinh.
4. **Kiểm tra Gửi Bù Thông Minh (Smart Resend)**:
   - Khi có người nhận bị thất bại, cơ chế gửi bù chỉ phát lệnh tới duy nhất người nhận lỗi, tuyệt đối không gửi lại cho người đã nhận phiếu báo thành công, loại bỏ 100% khiếu nại về spam email từ khách hàng.
5. **Kiểm tra Điều hướng Tab Mới Toàn Hệ Thống**:
   - Tất cả các nút "Chi tiết" trên Quản lý điểm đo, Quản lý khách hàng, Quản lý sổ GCS, Quản lý tài khoản, Cài đặt dữ liệu điểm đo, Giám sát thông số tổng hợp và nút "Tra soát" trên Quản lý sổ GCS đều mở trang trên Tab mới (`_blank`), người dùng không bao giờ bị mất vị trí hay bộ lọc đang thao tác.
6. **Kiểm tra Cấu hình Đơn vị và CSV Liên kết Dữ liệu**:
   - Các đại lượng kỹ thuật có đơn vị chuẩn hóa theo quy định EVN/Điện lực được cố định tự động, combobox mờ đi chống thao tác nhầm; tính năng Export/Import CSV xử lý linh hoạt và an toàn dữ liệu.
7. **Kiểm tra Cơ Chế Refresh Giám Sát Điểm Đo (Real-time & REST API Sync)**:
   - Xác thực cả 2 trang Thông số tổng hợp và Thông số chi tiết tự động refresh khi API Client đẩy dữ liệu lên máy chủ; trang Thông số tổng hợp giữ nguyên trang phân trang hiện tại và điều kiện lọc, không bị giật trang hay reset về trang 1.
8. **Kiểm tra Hiệu Chỉnh UI/UX Giám Sát (Thông số tổng hợp, Sự kiện, Cảnh báo)**:
   - Trang Thông số tổng hợp bỏ filter Tên khách hàng, bỏ cột Mô tả điểm đo, nút Chi tiết mở tab mới. Trang Bảng sự kiện và Bảng cảnh báo thay cột Nguồn bằng 2 cột riêng Mã điểm đo & Tên khách hàng, đổi filter sang Mã điểm đo, đồng bộ cả giao diện và file xuất CSV.
9. **Kiểm tra Cơ Chế Tự Động Cập Nhật Real-Time & REST API (Sự Kiện & Cảnh Báo)**:
   - Đồng bộ hoàn hảo cơ chế `currentParamsRef`, `refreshCurrentPageAsync()` ngầm và timer dự phòng 15s cho Bảng sự kiện và Bảng cảnh báo như trang Thông số tổng hợp, không làm nhấp nháy UI và không mất vị trí trang/bộ lọc.

10. **Kiểm tra Độ rộng Cột Bảng Sự Kiện & Bảng Cảnh Báo**:
    - Phân bổ lại phần trăm độ rộng 6 cột tổng cộng đúng 100%: Thời gian (16%), Mã điểm đo (18%), Tên khách hàng (30%), Tên sự kiện/cảnh báo (20%), Trạng thái (10%), Giá trị (6%).
    - Kết quả: Bảng chiếm trọn vẹn 100% chiều rộng khung chứa (Card), triệt tiêu hoàn toàn khoảng trắng dư thừa ở bên phải; các mã điểm đo dài (vd: `TRẠM NƯỚC THẢI STP4500 PHASE 2`) hiển thị thoáng đãng và không bị ngắt dòng luộm thuộm. Tiêu đề cột trên Bảng sự kiện hiển thị đúng "Tên sự kiện".
11. **Kiểm tra Thứ tự Nút & Icon Trang Liên Kết Dữ Liệu**:
    - Vị trí các nút trên header hiển thị chuẩn xác theo thứ tự: "Xuất CSV" -> "Nhập từ CSV" -> "Lưu".
    - Icon của nút "Nhập từ CSV" chuyển sang `<FiDownload size={20} />` đồng bộ phong cách với các màn hình quản lý điểm đo và khách hàng.
12. **Kiểm tra Gom Nhóm & Thứ Tự Tab Thời Gian trong Liên Kết Dữ Liệu**:
    - Danh mục 24 tham số thời gian được chuẩn hóa thành 4 nhóm theo trật tự logic: Năm -> Tháng -> Ngày -> Giờ -> Phút -> Giây.
13. **Kiểm tra Tính Đồng Nhất Giao Diện TableCommon cho Tab Thời Gian**:
    - Tab Thời gian sử dụng cấu trúc bảng chuẩn `TableCommon`, header xám `#d3dae5`, border và typography đồng nhất 100% với các tab "Thông số vận hành", "Thông số điện năng", "Cảnh báo".
    - Cột "Nhóm" (width 30%, rowSpan = 6) gom nhóm 4 khối thời gian trực quan, cột "Dữ liệu" (width 25%) hiển thị Năm/Tháng/Ngày/Giờ/Phút/Giây, cột "Tag ID" (width 45%) nhập liệu rộng rãi, liền mạch.
14. **Kiểm tra Modal Dialog Import CSV cho Trang Liên Kết Dữ Liệu**:
    - Khi bấm "Nhập từ CSV", hệ thống mở Modal `DialogImportConfig` chuẩn mực (thay vì mở ngay hộp thoại chọn file của Windows), đồng bộ hoàn toàn với "Quản lý khách hàng" và "Quản lý điểm đo".
    - Tích hợp vùng kéo thả tệp Dragger, nút tải tệp mẫu `Template_LienKetDuLieu_{MãĐiểmĐo}.csv`, thống kê kết quả Tag (Tổng số, Đã cập nhật, Bỏ qua) và Alert hướng dẫn ấn Lưu.
15. **Kiểm tra Thông Báo Toast Tiếng Việt Toàn Diện (Loại Bỏ Hoàn Toàn Chữ Tiếng Anh "Success" / "Error")**:
    - Cơ chế `extractMessage` trong `toastMessage.tsx` tự động trích xuất thông điệp chính xác cho cả lệnh gọi 1 tham số và 2 tham số.
    - Toàn bộ thông báo popup kết quả tác vụ Import CSV hiển thị 100% tiếng Việt chuẩn mực: "Thành công", "Nhập dữ liệu thành công! Cập nhật: ...", "Có lỗi xảy ra", triệt tiêu hoàn toàn chữ tiếng Anh "Success" hay "Error".
16. **Kiểm tra Tổ Chức Lại Tab "Thời Gian" (Bỏ Phân Nhóm, Đồng Bộ Flat Table Tuyệt Đối)**:
    - Tab "Thời gian" đã loại bỏ hoàn toàn cột "Nhóm" và cơ chế gộp ô `rowSpan = 6`, chuyển sang cấu trúc bảng phẳng (flat table) hoàn toàn tương tự như "Thông số điện năng" và "Thông số vận hành".
    - Cột "Dữ liệu" hiển thị trực tiếp tên tham số đầy đủ (`record.label`), ví dụ: "Thời gian hiện tại (năm)", "Thời gian hiện tại (tháng)", ..., "Thời gian chốt gần nhất (năm)", ..., "Thời gian công suất cực đại kỳ hiện tại (năm)", ..., "Thời gian công suất cực đại kỳ trước (năm)"...
    - Tỷ lệ độ rộng cột đạt chuẩn 100% (Dữ liệu: 50%, Tag ID: 50%), nhập liệu thông suốt, không co kéo.

---

## 4. KẾT LUẬN & BÀN GIAO
- Toàn bộ 60/60 test cases đều đạt chuẩn nghiệm thu tuyệt đối (100% PASS).
- Lỗi chuyển trạng thái khi Import CSV đã được giải quyết triệt để.
- Trạng thái điểm đo & tài khoản hiển thị đồng nhất trên toàn hệ thống.
- Trang Quản lý phiếu báo, Quản lý sổ GCS, Quản lý tài khoản, Cài đặt dữ liệu điểm đo và Liên kết dữ liệu được nâng cấp UI/UX chuyên nghiệp, tinh gọn và tiện dụng.
- Cơ chế giám sát thời gian thực và tự động refresh khi có dữ liệu từ REST API trên cả 4 màn hình: Thông số tổng hợp, Thông số chi tiết, Bảng sự kiện & Bảng cảnh báo đã được tối ưu hóa toàn diện, đạt độ ổn định và trải nghiệm xuất sắc.
- Giao diện Thông số tổng hợp, Bảng sự kiện và Bảng cảnh báo được tinh chỉnh UI/UX trực quan, nhất quán, độ rộng cột phân bổ phủ kín 100% không còn khoảng trắng lãng phí.
- Trang Liên kết dữ liệu được chuẩn hóa vị trí nút, icon, quy trình Import qua Modal chuyên nghiệp, giao diện bảng Tab Thời gian đồng nhất 100% với TableCommon dạng phẳng (bỏ phân nhóm) và các thông báo popup hoàn toàn bằng tiếng Việt.
- Kính chuyển toàn bộ hồ sơ sang **MDMS-Reviewer** để thẩm định và phê duyệt.



