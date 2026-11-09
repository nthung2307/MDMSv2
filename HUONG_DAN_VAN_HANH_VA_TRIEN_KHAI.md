# SỔ TAY HƯỚNG DẪN TỪNG BƯỚC VẬN HÀNH, TRIỂN KHAI & KIỂM THỬ HỆ THỐNG MDMSv2
> **Dự án**: Hệ thống Thu thập & Quản lý Dữ liệu Đo đếm Công tơ Điện tử (MDMSv2)  
> **Khách hàng / Đơn vị áp dụng**: VSIP Hải Phòng (Khu Đô thị, Công nghiệp & Dịch vụ VSIP Hải Phòng)  
> **Phiên bản hoàn thiện**: Version 2.0 (Bao gồm trọn vẹn nâng cấp từ Phase 1 đến Phase 4)  
> **Thời điểm cập nhật**: Tháng 11/2026

---

## MỤC LỤC
1. [TỔNG QUAN HỆ THỐNG VÀ CÁC NÂNG CẤP ĐÃ HOÀN THIỆN](#1-tổng-quan-hệ-thống-và-các-nâng-cấp-đã-hoàn-thiện)
2. [HƯỚNG DẪN CẬP NHẬT TRÊN MÁY ẢO ĐANG CHẠY (HOTFIX UPDATE)](#2-hướng-dẫn-cập-nhật-trên-máy-ảo-đang-chạy-hotfix-update)
3. [HƯỚNG DẪN CÀI ĐẶT MỚI 100% TRÊN MÁY CHỦ WINDOWS SERVER](#3-hướng-dẫn-cài-đặt-mới-100-trên-máy-chủ-windows-server)
4. [HƯỚNG DẪN TRIỂN KHAI BẰNG DOCKER CONTAINER](#4-hướng-dẫn-triển-khai-bằng-docker-container)
5. [HƯỚNG DẪN CHẠY BỘ KIỂM THỬ TỰ ĐỘNG (AUTOMATED TESTS)](#5-hướng-dẫn-chạy-bộ-kiểm-thử-tự-động-automated-tests)
6. [HƯỚNG DẪN KIỂM THỬ NGHIỆM THU NGHIỆP VỤ (STEP-BY-STEP UAT)](#6-hướng-dẫn-kiểm-thử-nghiệm-thu-nghiệp-vụ-step-by-step-uat)
7. [HƯỚNG DẪN XÓA SỔ GCS ĐỂ THỰC HIỆN LẠI](#7-hướng-dẫn-xóa-sổ-gcs-để-thực-hiện-lại)
8. [XỬ LÝ SỰ CỐ THƯỜNG GẶP (TROUBLESHOOTING & FAQ)](#8-xử-lý-sự-cố-thường-gặp-troubleshooting--faq)

---

## 1. TỔNG QUAN HỆ THỐNG VÀ CÁC NÂNG CẤP ĐÃ HOÀN THIỆN

Hệ thống MDMSv2 đã được nâng cấp toàn diện qua 4 giai đoạn kỹ thuật:

- **Giai đoạn 1 (Bảo mật & Ổn định)**:
  - Mã hóa AES-256 đối xứng cho mật khẩu hòm thư SMTP và Client Secret của Microsoft Graph OAuth 2.0.
  - Khắc phục lỗi rò rỉ bộ nhớ (RxJS / Recoil Memory Leak) khi挂 máy theo dõi giám sát điểm đo.
  - Khắc phục lỗi 401 hàng loạt trong interceptor Refresh Token của Frontend.
- **Giai đoạn 2 (Tối ưu CSDL & Hiệu năng)**:
  - Khắc phục tình trạng nạp bảng lịch sử triệu dòng vào RAM (`ToListAsync`).
  - Đánh Composite Indexes cho các bảng đo đếm (`MeasurementPointMonitoring`, `PeriodicEnergyStatement`).
  - Hợp nhất kết nối WebSocket SignalR, loại bỏ zombie sockets và bão refetch polling.
- **Giai đoạn 3 (Chuẩn hóa Đo đếm & Giao diện)**:
  - Chuẩn hóa toàn bộ kiểu số liệu điện năng sang `decimal(18,4)` tránh sai số dấu phẩy động.
  - Cấp chính xác công suất cực đại $P_{max}$ bảo toàn 2 chữ số thập phân (`N2`).
  - Mẫu Phiếu chỉ số điện A5 Ngang tiêu chuẩn VSIP Hải Phòng chuẩn xác 100% theo mẫu thực tế.
- **Giai đoạn 4 (Kiểm thử Tự động & Containerization)**:
  - Bộ 22 kịch bản Unit Tests Backend tự động (.NET 8 MSTest).
  - Bộ 4 kịch bản Unit Tests Frontend tự động (Node.js Native Test Runner).
  - Docker Multi-stage build và Docker Compose hỗ trợ triển khai 1 lệnh.
  - CI/CD Pipeline GitHub Actions tự động hóa kiểm thử và kiểm tra build.

---

## 2. HƯỚNG DẪN CẬP NHẬT TRÊN MÁY ẢO ĐANG CHẠY (HOTFIX UPDATE)

Dành cho máy ảo Windows Server hiện tại đang có dữ liệu và đang kiểm thử:

### Bước 2.1: Đóng gói bản cập nhật mới nhất (Thực hiện trên máy lập trình)
Chạy script đóng gói:
```powershell
powershell -ExecutionPolicy Bypass -File .\build_package.ps1
```
> Kết quả tạo ra thư mục: `E:\sources\MDMSv2\Publish\MDMSv2_InstallPackage`

### Bước 2.2: Sao chép các tệp sang máy ảo
Tạo một thư mục trên máy ảo (ví dụ: `C:\Hotfix`), sau đó copy từ `Publish\MDMSv2_InstallPackage` sang máy ảo:
1. Thư mục **`Backend`** (toàn bộ tệp `.dll`, `.exe` và web.config)
2. Thư mục **`Frontend`** (toàn bộ mã nguồn static bundle `dist`)
3. Tệp script **`Update_Hotfix.ps1`**
4. Thư mục **`Database`** (chứa các script SQL migration như `20261109_fix_vietnamese_font_encoding.sql`, `20261031_create_email_config_and_metering_tables.sql`, `20261109_standardize_decimal_metering_tables.sql`)

### Bước 2.3: Thực thi cập nhật tự động
1. Trên máy ảo, mở **Windows PowerShell** với quyền **Administrator** (Run as Administrator).
2. Chuyển vào thư mục chứa hotfix và chạy lệnh:
```powershell
cd C:\Hotfix
powershell -ExecutionPolicy Bypass -File .\Update_Hotfix.ps1
```

> Script `Update_Hotfix.ps1` sẽ tự động thực hiện:
> - Dừng dịch vụ `MDMS_Backend` và ứng dụng IIS an toàn.
> - Cập nhật mã nguồn Backend vào `C:\MDMSv2\Backend` (tự động giữ nguyên mật khẩu DB trong `appsettings.Production.json`).
> - Tự động chạy tất cả các script SQL migration với bảng mã UTF-8 (`SET NAMES utf8mb4;`) để cập nhật CSDL không bị lỗi font tiếng Việt.
> - Cập nhật mã nguồn Frontend vào `C:\MDMSv2\Frontend`.
> - Kiểm tra và cài đặt LibreOffice Headless (nếu chưa có).
> - Khởi động lại dịch vụ Backend và IIS, kiểm tra trạng thái hoạt động HTTP 200 OK.

---

## 3. HƯỚNG DẪN CÀI ĐẶT MỚI 100% TRÊN MÁY CHỦ WINDOWS SERVER

Dành cho máy chủ mới bàn giao hoặc cài đặt độc lập tại phòng máy chủ:

### Bước 3.1: Chuẩn bị gói cài đặt
Sao chép toàn bộ thư mục `Publish\MDMSv2_InstallPackage` sang máy chủ mới (ví dụ tại ổ đĩa `D:\MDMSv2_Setup`).

### Bước 3.2: Chạy kịch bản cài đặt tự động
1. Mở PowerShell với quyền **Administrator**.
2. Thực thi script cài đặt tổng thể:
```powershell
cd D:\MDMSv2_Setup
powershell -ExecutionPolicy Bypass -File .\SetupMDMS.ps1
```
3. Script sẽ tự động:
   - Cài đặt IIS, URL Rewrite 2.1, ASP.NET Core Hosting Bundle 8.0, Node.js, MySQL Server 8.0.
   - Khởi tạo cơ sở dữ liệu `amr` với cấu hình font chuẩn `utf8mb4` và múi giờ GMT+7.
   - Khởi tạo tài khoản quản trị mặc định:
     - **Tài khoản**: `admin`
     - **Mật khẩu**: `Abc@123456`
   - Cấu hình sẵn Mẫu Phiếu chỉ số điện A5 Ngang tiêu chuẩn VSIP Hải Phòng.
   - Mở cổng tường lửa Windows Firewall an toàn (Port 80/443).

---

## 4. HƯỚNG DẪN TRIỂN KHAI BẰNG DOCKER CONTAINER

Hệ thống hỗ trợ chạy hoàn chỉnh trên Docker (Linux/Windows Server có cài Docker):

### Bước 4.1: Khởi động toàn bộ cụm dịch vụ
Tại thư mục gốc dự án, thực thi:
```bash
docker compose up -d
```

### Bước 4.2: Các cổng truy cập
- **Frontend (Web App)**: `http://localhost/` hoặc `http://localhost:8866/`
- **Backend API**: `http://localhost:8080/api/`
- **MySQL Database**: `localhost:3306` (User: `root`, Password: `Abc@123456`)

---

## 5. HƯỚNG DẪN CHẠY BỘ KIỂM THỬ TỰ ĐỘNG (AUTOMATED TESTS)

Hệ thống trang bị 2 bộ kiểm thử tự động giúp kiểm tra tức thời tính toàn vẹn hệ thống:

### 5.1. Kiểm thử Backend (.NET 8 Unit Tests)
Chạy trực tiếp Autonomous Test Runner (chạy được trong mọi môi trường mà không cần cài đặt thêm công cụ):
```powershell
Backend/Tests/MDMS.UnitTests/bin/Debug/net8.0/MDMS.UnitTests.exe
```
Hoặc chạy qua lệnh chuẩn dotnet:
```powershell
dotnet test Backend/Tests/MDMS.UnitTests/MDMS.UnitTests.csproj
```
> **Bao gồm 22 kịch bản test**:
> - Tính sản lượng đa biểu giá (BT, CD, TD, SG, VC) bằng Decimal.
> - Xử lý tràn vòng công tơ (Rollover).
> - Công thức tính và làm tròn $\cos\varphi$ chuẩn EVN.
> - Tách sản lượng khi thay công tơ giữa kỳ (`MeterReplacement`).
> - Độ chính xác 2 chữ số thập phân của công suất cực đại $P_{max}$.
> - Mã hóa bảo mật mật khẩu AES-256.
> - Phát hiện chuỗi tiếng Việt Unicode lỗi font.
> - Cấu hình layout PDF động.

### 5.2. Kiểm thử Frontend (Node Native Test Runner)
Tại thư mục Frontend, thực thi:
```powershell
npm --prefix Frontend test
```
> **Bao gồm 4 test suites**: Kiểm tra hàm `calculateCosTB`, `formatNumberWithCommas`, `isGarbledEncoding`, `parseStatementLayoutConfig`.

---

## 6. HƯỚNG DẪN KIỂM THỬ NGHIỆM THU NGHIỆP VỤ (STEP-BY-STEP UAT)

### Bước 6.1: Kiểm tra Cấu hình Email và Khóa bảo mật
1. Đăng nhập hệ thống bằng tài khoản `admin`.
2. Vào menu: **Cài đặt hệ thống > Cấu hình Email**.
3. **Nền tảng Gmail**:
   - Chọn nền tảng: **Gmail**.
   - Email gửi: `mmds.vsiphaiphong@gmail.com`.
   - Host: `smtp.gmail.com`, Port: `587`, Mode: `STARTTLS (587)`.
   - Mật khẩu: Nhập App Password 16 ký tự của Google.
   - Bấm **Lưu cấu hình**.
4. **Kiểm tra mã hóa trong CSDL**: Mở MySQL Workbench chạy:
   ```sql
   SELECT Id, ProviderType, SenderEmail, EncryptedSmtpPassword FROM amr.systememailconfigs;
   ```
   *Kết quả*: Mật khẩu hiển thị dưới dạng chuỗi mã hóa ký tự ngẫu nhiên AES (không còn plain text).
5. Bấm nút **Gửi email thử nghiệm**: Kiểm tra hòm thư nhận nhận được email test từ hệ thống.

---

### Bước 6.2: Kiểm tra Mẫu Phiếu chỉ số điện A5 Ngang
1. Vào menu: **Cài đặt hệ thống > Mẫu Phiếu chỉ số điện** (`/statement-pdf-templates`).
2. Quan sát bản xem trước bên phải (Khổ A5 Ngang):
   - **Logo**: Logo VSIP góc trái phía trên.
   - **Tên công ty**: `VSIP HAI PHONG CO., LTD`.
   - **Địa chỉ**: `VSIP Hai Phong Administration Office, VSIP Hai Phong Township, Industrial & Service Park\nHoa Binh Ward, Hai Phong City, Vietnam`.
   - **Số điện thoại**: `Tel: 84-225-3959868   Fax: 84-225-3959886`.
   - **Tiêu đề chính**: `MONTHLY POWER METER READING`.
   - **Tiêu đề phụ**: `PHIẾU GHI CHỈ SỐ ĐIỆN HÀNG THÁNG`.
   - **Ghi chú**: `(M&E Copy – Liên cho Phòng Cơ Điện)`.
   - **Mã biểu mẫu**: `M&E-1/01 PR04`.
   - **Bảng số liệu 8 cột**: Mô tả, BT, CD, TD, AP, AQ, $\cos\varphi$ TB, Remarks.
   - **Dòng Pmax**: `* Công suất cực đại Pmax: 66.00 kW, ghi nhận hồi: ...` (có đủ 2 chữ số thập phân).
   - **Hai khung chữ ký**:
     - *Bên A*: `Party A (VSIP Hai Phong Co., Ltd) - Bên A (Công ty TNHH VSIP Hải Phòng)`.
     - *Bên B*: `Party B (Tenant) - Bên B (Khách hàng)`.
     - Dòng ký: `Name and Signature\n(Ký và ghi rõ họ tên)`.
3. Bấm biểu tượng **Xem thử PDF (icon PDF đỏ)** ở góc trên để xác nhận bản PDF xuất ra từ Backend QuestPDF trùng khớp 100% với giao diện xem trước.

---

### Bước 6.3: Thực hiện quy trình Ghi chỉ số (GCS) và Chốt số liệu
1. Vào menu: **Ghi chỉ số > Quản lý sổ GCS**.
2. Chọn kỳ chốt tháng (ví dụ: Tháng 10/2026), chọn ngày chốt (ví dụ: ngày 31).
3. Bấm **Tạo sổ ghi chỉ số** (nếu chưa có).
4. Vào **Chi tiết sổ GCS**:
   - Hệ thống tự động nạp chỉ số tức thời từ công tơ Kyocera về.
   - Kiểm tra các đại lượng:
     - `Chỉ số cũ` và `Chỉ số mới`.
     - `Sản lượng tiêu thụ (kWh)` = $(Mới - Cũ) \times Hệ số$.
     - `Pmax` hiển thị đúng cấp chính xác 2 chữ số thập phân (ví dụ: `7.52` hoặc `66.00`).
     - `Cos phi` tính toán tự động dựa trên $AP$ và $AQ$.
5. Bấm **Chốt sổ GCS**:
   - Hệ thống kích hoạt Transaction chốt dữ liệu vào bảng `periodicenergystatement`.
   - Cấp số Serial theo định dạng chuẩn: `00000162`.

---

### Bước 6.4: Xuất Phiếu báo & Gửi Email tự động kèm tệp PDF
1. Vào menu: **Quản lý phiếu báo**.
2. Tìm phiếu báo vừa chốt của khách hàng Kyocera.
3. Bấm **Xem phiếu báo**: Giao diện hiển thị phiếu báo chuẩn A5 Ngang.
4. Bấm **Xuất PDF**: Tệp PDF tải về máy kiểm tra:
   - Font tiếng Việt hiển thị sắc nét, không bị ô vuông hoặc ký tự rác.
   - Bố cục căn lề chuẩn trang in khổ giấy A5 Ngang.
5. Bấm **Gửi Email**:
   - Khách hàng nhận được email thông báo sản lượng điện với tiêu đề tiếng Việt chuẩn `THÔNG BÁO CHỈ SỐ ĐIỆN HÀNG THÁNG`.
   - File PDF đính kèm: `PhieuBaoSo_00000162.pdf`.

---

## 7. HƯỚNG DẪN XÓA SỔ GCS ĐỂ THỰC HIỆN LẠI

Khi bạn muốn xóa sạch kỳ chốt tháng 10/2026 hoặc kỳ bất kỳ để thực hiện lại từ đầu quy trình ghi chỉ số, hãy sử dụng các script SQL sau:

### Cách 1: Xóa toàn bộ dữ liệu chốt và mở lại sổ GCS kỳ Tháng 10/2026
Mở MySQL Workbench và thực thi script `Installers/Database/reset_gcs_data_T10_2026.sql`:
```sql
USE `amr`;
SET SQL_SAFE_UPDATES = 0;

-- 1. Xóa các phiếu báo chỉ số định kỳ đã phát hành trong tháng 10/2026
DELETE FROM `periodicenergystatement` 
WHERE `Year` = 2026 AND `Month` = 10;

-- 2. Xóa chi tiết chốt chỉ số tháng
DELETE FROM `gcsmonthlydetail` 
WHERE `GcsId` IN (SELECT `Id` FROM `gcscutoffcycles` WHERE `Year` = 2026 AND `Month` = 10);

-- 3. Xóa dữ liệu tạm tính
DELETE FROM `gcstemporary` 
WHERE `GcsId` IN (SELECT `Id` FROM `gcscutoffcycles` WHERE `Year` = 2026 AND `Month` = 10);

-- 4. Đưa trạng thái Sổ GCS về Chưa chốt (Draft)
UPDATE `gcscutoffcycles` 
SET `Status` = 0, `IsCheck` = 0, `IsSendingMail` = 0
WHERE `Year` = 2026 AND `Month` = 10;
```

### Cách 2: Xóa hoàn toàn sổ GCS để tạo lại sổ mới từ đầu
```sql
USE `amr`;
SET SQL_SAFE_UPDATES = 0;

DELETE FROM `periodicenergystatement` WHERE `Year` = 2026 AND `Month` = 10;
DELETE FROM `gcsmonthlydetail` WHERE `GcsId` IN (SELECT `Id` FROM `gcscutoffcycles` WHERE `Year` = 2026 AND `Month` = 10);
DELETE FROM `gcstemporary` WHERE `GcsId` IN (SELECT `Id` FROM `gcscutoffcycles` WHERE `Year` = 2026 AND `Month` = 10);
DELETE FROM `gcscutoffcycles` WHERE `Year` = 2026 AND `Month` = 10;
```
> Sau khi chạy lệnh trên, bạn F5 lại màn hình **Ghi chỉ số**, kỳ chốt sẽ hoàn toàn trống và có thể bấm nút **Tạo sổ ghi chỉ số** để kiểm thử lại từ đầu.

---

## 8. XỬ LÝ SỰ CỐ THƯỜNG GẶP (TROUBLESHOOTING & FAQ)

| Hiện tượng | Nguyên nhân | Cách khắc phục |
| :--- | :--- | :--- |
| **Lỗi 502 Bad Gateway khi mở Web** | Dịch vụ `MDMS_Backend` bị tắt hoặc sai mật khẩu MySQL trong `appsettings.Production.json`. | Mở file `C:\MDMSv2\Backend\appsettings.Production.json`, kiểm tra `AppDb.Password`. Khởi động lại dịch vụ trong `services.msc` hoặc chạy lại `Update_Hotfix.ps1`. |
| **Email gửi đi bị lỗi font tiếng Việt** | MySQL lưu dữ liệu bảng `NotificationTemplates` không phải utf8mb4. | Chạy script migration `Installers/Database/20261109_fix_vietnamese_font_encoding.sql` bằng MySQL Workbench hoặc PowerShell với cờ `--default-character-set=utf8mb4`. |
| **Nút "Gửi email" báo lỗi xác thực SMTP** | Gmail bật bảo mật 2 lớp nhưng chưa tạo App Password, hoặc đổi mật khẩu tài khoản Google. | Vào tài khoản Google > Security > 2-Step Verification > App Passwords, tạo mật khẩu 16 ký tự mới và cập nhật vào màn hình Cấu hình Email. |
| **Không xuất được tệp PDF / Nút PDF quay vô tận** | Máy chủ thiếu font hiển thị hoặc chưa cài LibreOffice. | Chạy `Update_Hotfix.ps1`, script sẽ tự động kiểm tra và cài đặt bổ sung gói LibreOffice Headless và font tiếng Việt. |
| **Số liệu Pmax hiển thị làm tròn số nguyên** | Phiên bản Backend cũ chưa nạp định dạng `N2`. | Sao chép bản build Backend mới nhất từ `Publish\MDMSv2_InstallPackage\Backend` sang máy ảo và khởi động lại dịch vụ. |

