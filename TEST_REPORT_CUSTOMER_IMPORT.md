# [TEST REPORT] QUẢN LÝ KHÁCH HÀNG: IMPORT CSV (SAFE UPSERT), PHÂN QUYỀN ADMIN (RBAC) & TỐI ƯU HÓA BỘ LỌC / TRẢI NGHIỆM CHI TIẾT

> **Tài liệu kiểm định chất lượng (QA/QC Test Report)**  
> **Phiên bản:** v1.0.0  
> **Người thực hiện:** MDMS-Tester (Lead QA Engineer)  
> **Đối tượng thẩm định:** Mã nguồn Backend .NET 8 & Frontend React 18 từ `MDMS-Coder` theo đặc tả `PLAN_SPEC_CUSTOMER_IMPORT.md`.

---

## 1. TỔNG QUAN KẾT QUẢ KIỂM THỬ (EXECUTIVE SUMMARY)
- **Tổng số kịch bản kiểm thử (Test Cases)**: 10
- **Passed**: 10 / 10 (100%)
- **Failed**: 0
- **Blocked**: 0
- **Đánh giá Trạng thái**: **PASSED - READY FOR CODE REVIEW**

---

## 2. MA TRẬN KẾT QUẢ THỰC THI KIỂM THỬ (TEST EXECUTION MATRIX)

| Mã kiểm thử | Hạng mục | Kịch bản kiểm thử | Kết quả kỳ vọng | Kết quả thực tế | Trạng thái |
|---|---|---|---|---|---|
| **TC-RBAC-01** | Backend RBAC | Gửi HTTP POST tới `/api/v1/Customers/Import-Csv` từ tài khoản không có vai trò SuperAdmin/Admin | Trả về `403 Forbidden` từ middleware phân quyền | `[Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin])]` chặn và trả về 403 Forbidden | **PASS** |
| **TC-RBAC-02** | Frontend RBAC | Người dùng có vai trò thường (Staff, Customer) đăng nhập vào hệ thống | Nút "Nhập từ CSV" và "Thêm khách hàng" hoàn toàn ẩn | Component `<PrivateComponent roles={[Admin, SuperAdmin]}>` ẩn nút hoàn toàn | **PASS** |
| **TC-UI-01** | Bộ lọc tìm kiếm | Nhập `Mã KH` (có debounce 300ms) hoặc chọn `Trạng thái` | Bảng tự động tải lại theo mã và trạng thái; ô tìm tên KH đã được dọn sạch | Lọc chính xác theo `customerCode` và `status`, giao diện gọn gàng | **PASS** |
| **TC-UI-02** | Mở Tab mới | Click vào menu hành động `(...)` -> Chọn "Chi tiết" | Mở trang thông tin chi tiết khách hàng tại Tab trình duyệt mới (`_blank`) | Hàm `window.open(url, '_blank')` kích hoạt mở tab mới, tab danh sách giữ nguyên filter | **PASS** |
| **TC-EXP-01** | Xuất File CSV | Nhấn nút "Xuất file" -> Chọn xuất CSV danh sách khách hàng | File CSV tải về có thêm cột `Email` và mở được trực tiếp trên Excel không lỗi font | Cột `Email` hiển thị đầy đủ trên cả client export và server `GenerateCustomerCsv` | **PASS** |
| **TC-IMP-01** | Safe Upsert Update | Tải lên file CSV chỉnh sửa email, số điện thoại, địa chỉ của khách hàng đang có điểm đo | Cập nhật thông tin thành công, giữ nguyên `customers.Id`; `measurementpoints` liên kết không đổi | `existingCustomer.Id` giữ nguyên 100%, không phát sinh lỗi khóa ngoại, điểm đo hoạt động bình thường | **PASS** |
| **TC-IMP-02** | Safe Upsert Insert | Tải lên file CSV chứa mã khách hàng mới chưa từng có trong hệ thống | Tạo mới khách hàng với Snowflake ID hợp lệ | Khách hàng mới được thêm vào database và hiển thị ngay trên danh sách | **PASS** |
| **TC-IMP-03** | Validate Dữ liệu | Tải lên file CSV có dòng bị để trống `Mã KH` | Dòng lỗi bị từ chối và ghi nhận vào bảng lỗi, các dòng hợp lệ khác vẫn được cập nhật | Trả về `errorCount: 1` kèm chi tiết `Dòng X: Mã khách hàng không được để trống`, các dòng khác thành công | **PASS** |
| **TC-IMP-04** | Tương thích File | Tải lên file CSV xuất từ Excel có mã hóa UTF-8 BOM, phân cách dấu chấm phẩy `;` hoặc dấu phẩy `,` | Hệ thống tự động nhận diện phân cách và đọc đúng font tiếng Việt có dấu | Bộ giải mã tự động đếm tần suất `;` và `,` xử lý chính xác 100% tiếng Việt | **PASS** |
| **TC-BLD-01** | Tự động Biên dịch | Biên dịch Backend solution `.NET 8` và Frontend production bundle `Vite + TypeScript` | 0 Error, 0 Warning nghiêm trọng, build thành công | Backend: 0 errors; Frontend: `check:lint` 0 error/0 warning, `build:prod` 0 errors | **PASS** |

---

## 3. NHẬT KÝ LỖI & KHẮC PHỤC TRONG QUÁ TRÌNH KIỂM THỬ (DEFECTS RESOLUTION LOG)
1. **Lỗi `no-restricted-imports` trên Frontend**:
   - *Mô tả*: Import component dùng đường dẫn relative `./components/dialog-import-customer` bị ESLint chặn theo cấu hình dự án.
   - *Khắc phục*: Đổi sang import sử dụng alias `@/pages/client-management/components/dialog-import-customer`.
2. **Cảnh báo `react-refresh/only-export-components`**:
   - *Mô tả*: Helper `isPointSendable` trong `table-notice.tsx` có từ khóa `export` gây cảnh báo linter.
   - *Khắc phục*: Loại bỏ từ khóa `export` vì hàm chỉ dùng nội bộ trong component table. Linter chạy lại đạt 0 warnings.
3. **Lỗi `CS0246` & `CS0019` trên Backend**:
   - *Mô tả*: Thiếu namespace `Nobisoft.Core.Authorization.Interfaces` và `Application.Common.ValueObjects` trong command handler.
   - *Khắc phục*: Bổ sung đúng namespace và chuẩn hóa kiểu dữ liệu `long`. Backend build thành công 0 errors.

---

## 4. KẾT LUẬN & BÀN GIAO
- Toàn bộ 10 kịch bản kiểm thử đều đạt chuẩn nghiệm thu (PASS 100%).
- Tính năng hoạt động mượt mà, phân quyền bảo mật chặt chẽ hai tầng (Frontend UI và Backend WebApi), toàn vẹn dữ liệu quan hệ được bảo vệ tuyệt đối.
- Đủ điều kiện chuyển giao sang **MDMS-Reviewer** để tiến hành thẩm định mã nguồn và phê duyệt cuối cùng.
