# THƯ MỤC CHỨA BỘ CÀI ĐẶT MYSQL SERVER 8.0 (WINDOWS SERVER 2019)

Vui lòng sao chép tệp cài đặt MySQL Community Server 8.0 (64-bit) vào thư mục này:

### Các tệp cài đặt được hỗ trợ:
1. **MySQL Community Installer (Khuyến nghị)**:
   - Tên tệp: `mysql-installer-community-8.0.*.msi` hoặc `mysql-installer-web-community-8.0.*.msi`
   - Link tải chính thức từ Oracle: https://dev.mysql.com/downloads/installer/
2. **MySQL Server MSI**:
   - Tên tệp: `mysql-server-8.0.*-winx64.msi` hoặc `mysql-8.0.*-winx64.msi`

---
### Ghi chú khi cài đặt:
- Chọn gói cài đặt: **Server only** (hoặc Custom -> MySQL Server 8.0).
- Cổng mặc định: **3306**.
- Mật khẩu root: Do bạn đặt trong quá trình cài đặt (sau đó sẽ nhập vào Wizard `SetupMDMS.ps1` để tự động đồng bộ vào cấu hình hệ thống).
