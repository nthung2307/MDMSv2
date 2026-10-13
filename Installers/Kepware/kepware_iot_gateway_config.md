# HƯỚNG DẪN CẤU HÌNH KEPWARE V6.6 CHO HỆ THỐNG MDMSv2

Tài liệu này hướng dẫn chi tiết cách thiết lập **Kepware V6.6.350.00 (KEPServerEX v6)** trên cùng máy ảo Windows Server 2019 để thu thập dữ liệu công tơ Modbus và giao tiếp với Backend MDMSv2.

---

## 1. Cấu hình Kênh Truyền Thông (Channel) & Thiết Bị (Device)

### 1.1. Cấu hình Modbus RTU (Serial RS-485 qua cổng COM hoặc USB-to-RS485)
1. Mở **KEPServerEX Configuration**.
2. Nhấp chuột phải vào `Project` $\to$ **New Channel**:
   * **Channel type**: Chọn `Modbus RTU Serial`.
   * **Name**: Đặt tên (ví dụ: `Channel_ModbusRTU`).
   * **COM Port**: Chọn cổng COM vật lý hoặc COM ảo của bộ chuyển đổi (ví dụ: `COM1`, `COM3`).
   * **Baud Rate**: `9600` hoặc `19200` (khớp với thông số cài đặt trên công tơ).
   * **Data Bits**: `8`, **Parity**: `Even` (hoặc `None`), **Stop Bits**: `1`.
3. Tạo **Device** trong Channel:
   * **Device Model**: `Modbus`.
   * **Device ID (Slave ID)**: Nhập địa chỉ Modbus của công tơ (ví dụ: `1`, `2`, `3`).
   * **Timing**: Request Timeout = `1000ms`, Retry Attempts = `2`.
   * **Scan Mode**: `Respect client-specified scan rate`.

### 1.2. Cấu hình Modbus TCP (Ethernet / Bộ chuyển đổi NPort)
1. Nhấp chuột phải vào `Project` $\to$ **New Channel**:
   * **Channel type**: Chọn `Modbus TCP/IP Ethernet`.
   * **Name**: Đặt tên (ví dụ: `Channel_ModbusTCP`).
   * **Network Adapter**: Chọn card mạng nội bộ của máy ảo.
2. Tạo **Device** trong Channel:
   * **Device Model**: `Modbus`.
   * **Device ID**: Định dạng `<IP_Công_tơ_hoặc_Gateway>.1` (ví dụ: `192.168.1.100.1`).
   * **Port TCP**: Mặc định `502`.

---

## 2. Cấu hình Danh Sách Tags Đo Đếm (Tag Database)

Tạo nhóm Tag cho từng công tơ theo quy chuẩn thanh ghi (Holding Registers `4xxxxx`):

| Tag Name | Địa chỉ Modbus | Kiểu dữ liệu | Mô tả |
|---|---|---|---|
| `Voltage_Phase_A` | `40001` | Float | Điện áp pha A ($V$) |
| `Voltage_Phase_B` | `40003` | Float | Điện áp pha B ($V$) |
| `Voltage_Phase_C` | `40005` | Float | Điện áp pha C ($V$) |
| `Current_Phase_A` | `40007` | Float | Dòng điện pha A ($A$) |
| `Current_Phase_B` | `40009` | Float | Dòng điện pha B ($A$) |
| `Current_Phase_C` | `40011` | Float | Dòng điện pha C ($A$) |
| `Active_Power_Total` | `40013` | Float | Công suất tác dụng tổng ($kW$) |
| `Reactive_Power_Total` | `40015` | Float | Công suất phản kháng tổng ($kVAR$) |
| `Power_Factor` | `40017` | Float | Hệ số công suất ($Cos\varphi$) |
| `Total_Active_Energy` | `40021` | Double / DWord | Chỉ số tích lũy điện năng ($kWh$) |

---

## 3. Cấu hình Plug-in IoT Gateway của Kepware

### 3.1. Thiết lập REST Server Agent (Phục vụ PULL chốt chỉ số GCS)
* Trong cây thư mục dự án Kepware, mở mục **IoT Gateway**.
* Nhấp chuột phải $\to$ **New Agent** $\to$ Chọn loại **REST Server**.
* **Name**: `MDMS_REST_Server`.
* **Port**: `39320` (hoặc cổng cấu hình trong `appsettings.Production.json`).
* **Endpoint URL**: `http://127.0.0.1:39320/iotgateway/read`.
* **Allow Cross-Origin Requests (CORS)**: Bật `Enable`.

### 3.2. Thiết lập REST Client Agent (Tự động PUSH dữ liệu tức thời sang WebApi)
* Nhấp chuột phải $\to$ **New Agent** $\to$ Chọn loại **REST Client**.
* **Name**: `MDMS_REST_Client`.
* **URL**: `http://127.0.0.1:19089/api/v1/iot/measurement-point-monitoring`.
* **Method**: `POST`.
* **Message Format**: `JSON`.
* **Publish Rate / Scan Rate**: `5000ms` (5 giây gửi dữ liệu 1 lần) hoặc tùy chỉnh theo nhu cầu.
* **Gán Tags (Add Tag Items)**: Chọn các Tag công tơ cần gửi (U, I, P, Q, $Cos\varphi$).
