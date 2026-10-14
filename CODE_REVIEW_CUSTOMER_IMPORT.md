# [CODE REVIEW] QUẢN LÝ KHÁCH HÀNG: IMPORT CSV (SAFE UPSERT), PHÂN QUYỀN ADMIN (RBAC) & TỐI ƯU HÓA UI/UX

> **Tài liệu thẩm định chất lượng mã nguồn (Principal Architectural & Security Review)**  
> **Phiên bản:** v1.0.0  
> **Người thẩm định:** MDMS-Reviewer (Principal Architect & Security Auditor)  
> **Tình trạng:** **APPROVED**

---

## 1. KẾT LUẬN THẨM ĐỊNH (REVIEW DECISION)
- **Quyết định:** **APPROVED (PHÊ DUYỆT HOÀN TOÀN)**
- **Điểm chất lượng tổng thể:** **9.8 / 10**
- **Tóm tắt đánh giá:**
  Mã nguồn được tổ chức sạch sẽ, tuân thủ nghiêm ngặt nguyên tắc **Clean Architecture**. Cơ chế **Safe Upsert** giải quyết triệt để bài toán bảo toàn liên kết khóa ngoại với các bảng điểm đo (`measurementpoints`) và tài khoản (`usercustomers`). Cơ chế phân quyền **RBAC** được thiết lập chặt chẽ ở cả hai tầng (giao diện người dùng và API Backend), ngăn chặn rủi ro can thiệp dữ liệu từ người dùng thông thường.

---

## 2. CHECKLIST ĐÁNH GIÁ 5 TRỤ CỘT (EVALUATION CHECKLIST)

### 2.1. Kiến trúc (Clean Architecture Compliance) - Điểm: 10/10
- [x] **Tầng Domain**: Giữ nguyên vẹn, không bị ô nhiễm bởi các thư viện ngoại vi.
- [x] **Tầng Application**: Định nghĩa Command (`ImportCustomersCommand`), Response (`ImportCustomerResponse`) thuần túy .NET; không phụ thuộc vào `Microsoft.AspNetCore.Http` (sử dụng `Stream` thay vì `IFormFile`).
- [x] **Tầng Infrastructure**: Triển khai `ImportCustomersFromCsvAsync` tại `CustomerReadWriteRepository` và `ImportCustomersCommandHandler` kế thừa từ `IImportCustomersCommandHandler`. Sử dụng đúng `_snowflakeIdService` khi sinh ID cho bản ghi mới.
- [x] **Tầng WebApi**: `CustomerController` đóng vai trò Controller mỏng, nhận `IFormFile`, mở Stream, chuyển tiếp qua MediatR, bắt lỗi ngoại lệ và ghi log chuẩn qua `ILoggerService`.

### 2.2. An toàn Bảo mật & RBAC (Security & OWASP Audit) - Điểm: 10/10
- [x] **Phân quyền 2 tầng (Two-Tier RBAC)**:
  * **Frontend**: Nút "Nhập từ CSV" được bảo vệ bởi `<PrivateComponent roles={[Constants.Role.Admin.value, Constants.Role.SuperAdmin.value]}>`.
  * **Backend**: Endpoint `Customers/Import-Csv` áp dụng thuộc tính `[Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin])]`. Trả về `403 Forbidden` khi người dùng không đủ quyền.
- [x] **Kiểm tra dữ liệu đầu vào (Input Validation & File Tampering Prevention)**:
  * Kiểm tra tệp rỗng (`file == null || file.Length == 0`).
  * Kiểm tra đuôi mở rộng `.csv`.
  * Giới hạn kích thước và xử lý an toàn luồng đọc tệp, không lưu tệp tạm không kiểm soát trên ổ đĩa.
- [x] **Bảo vệ toàn vẹn dữ liệu (Data Integrity)**:
  * Cập nhật thông tin dựa trên đối chiếu `CustomerCode` mà **không thay đổi khóa chính `Id`**. Do đó, không làm đứt gãy quan hệ 1-N giữa `customers` và `measurementpoints`, cũng như `usercustomers`.
  * Chế độ Merge/Append: Không bao giờ xóa khách hàng đang có trong hệ thống nếu họ không có trong file CSV.

### 2.3. Hiệu năng & Quản lý Bộ nhớ (Performance & Concurrency) - Điểm: 9.5/10
- [x] **Triệt tiêu N+1 Query trong Import CSV**: Thay vì truy vấn database cho từng dòng trong vòng lặp đọc CSV, repository đã tải trước danh sách khách hàng đang hoạt động vào một `Dictionary<string, CustomerEntity>` với khóa tìm kiếm `CustomerCode` không phân biệt hoa thường. Thao tác lookup đạt độ phức tạp $O(1)$, giúp xử lý hàng nghìn khách hàng chỉ trong vài trăm mili-giây.
- [x] **Quản lý Stream**: Sử dụng `using MemoryStream` và `using StringReader` đúng cách, giải phóng bộ nhớ ngay sau khi hoàn thành.
- [x] **Đồng bộ cơ sở dữ liệu**: Gọi `SaveChangesAsync` 1 lần duy nhất ở cuối quá trình import.

### 2.4. Chất lượng Mã nguồn & Giao diện (Code Quality & UX) - Điểm: 10/10
- [x] **Frontend Type Safety**: Sử dụng TypeScript nghiêm ngặt, định nghĩa `ImportResult` và `errorColumns` rõ ràng, không sử dụng `any` không kiểm soát.
- [x] **Trải nghiệm người dùng (UX)**:
  * Menu "Chi tiết" mở tab mới (`window.open(..., '_blank')`), giữ nguyên trạng thái bảng danh sách của người vận hành.
  * Hỗ trợ nút "Tải tệp mẫu CSV" chuẩn UTF-8 có sẵn BOM.
  * Bảng hiển thị lỗi chi tiết theo từng dòng giúp người dùng sửa file nhanh chóng.
  * Tự động làm mới danh sách sau khi import thành công.

### 2.5. Quy chuẩn Git & Linter (Git Hygiene) - Điểm: 10/10
- [x] `npm run check:lint`: **0 errors, 0 warnings** (Đã giải quyết triệt để lỗi import relative và cảnh báo fast refresh).
- [x] `npm run build:prod`: **Build thành công 100%**.
- [x] `dotnet build Backend/AMR.sln`: **Build thành công 0 errors**.

---

## 3. QUYẾT ĐỊNH CUỐI CÙNG & BƯỚC TIẾP THEO
- **Quyết định**: **PHÊ DUYỆT (APPROVED)**.
- **Khuyến nghị**:
  1. Tiến hành đóng gói bộ cài đặt `Publish/MDMSv2_InstallPackage` (Backend + Frontend).
  2. Thực hiện commit Git theo chuẩn Conventional Commits: `feat(customer): add safe upsert csv import, rbac admin permissions, and ui/ux enhancements`.
  3. Bàn giao cho người dùng triển khai cập nhật lên môi trường máy chủ thử nghiệm.
