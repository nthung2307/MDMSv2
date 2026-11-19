# KẾ HOẠCH PHÁT TRIỂN HỆ THỐNG THU THẬP DỮ LIỆU ĐO ĐẾM MDMS-HES (HEAD-END SYSTEM)

> **Mã dự án:** MDMS.HES  
> **Phiên bản:** 1.0.0-DRAFT  
> **Ngày lập:** 19/11/2026  
> **Trạng thái:** Sẵn sàng triển khai (Approved for Implementation)  
> **Tham chiếu kiến trúc:** Clean Architecture, .NET 8 LTS, Cross-Platform (Windows / Linux / Industrial IoT Gateway ARM)  

---

## 1. BỐI CẢNH VÀ MỤC TIÊU DỰ ÁN

### 1.1. Hiện trạng và Vấn đề với Kepware IoT Gateway
Trong quá trình kiểm thử tải và mô phỏng thực tế với 162 công tơ:
1. **Sự cố rớt mạng kéo dài (Network Outage > 8h):** Khi mất kết nối mạng và phục hồi lại, module Kepware IoT Gateway chuyển sang trạng thái `"Stopping"` và treo hoàn toàn (Deadlock). Khởi động lại service vẫn không thể tự phục hồi nếu không kill tiến trình hoặc cấu hình lại.
2. **Tràn bộ nhớ và Deadlock luồng:** Mô hình Narrow Format của Kepware sử dụng hàng đợi `RecordFifo` trong bộ nhớ 32-bit. Khi có 162 REST Clients chạy đồng thời với hàng chục ngàn sự kiện tích lũy trong nhiều giờ, tiến trình Java/C++ của IoT Gateway bị cạn kiệt tài nguyên (Thread exhaustion & Out-of-Memory).
3. **Thiếu cơ chế Circuit Breaker:** Khi rớt mạng, 162 REST Client độc lập liên tục retry kết nối dồn dập (thời gian timeout 3–5s), gây nghẽn socket và treo cổng HTTP stack.
4. **Hạn chế phần cứng & Bản quyền:** Kepware phụ thuộc môi trường Windows, tốn kém chi phí license công nghiệp lớn khi scale lên hàng nghìn trạm, và không thể chạy trực tiếp trên các thiết bị IoT Edge Gateway nhúng (ARM Linux như Artila Matrix-752, Raspberry Pi CM4).

### 1.2. Mục tiêu của MDMS.HES
Xây dựng một hệ thống **Head-End System (MDMS.HES)** chuyên dụng, tự chủ công nghệ 100%, độc lập, tối ưu hóa cho ngành đo đếm điện năng:
* **Native Cross-Platform (.NET 8):** Chạy mượt mà trên **Windows Server / Workstation**, **Linux Server (Docker/systemd)** và **Industrial IoT Gateway (ARM32/ARM64 Linux)**.
* **Tương thích 100% với REST Telemetry hiện hữu:** Đẩy dữ liệu trực tiếp về `POST /api/iot/telemetry` của MDMSv2 theo chuẩn Narrow Format định dạng JSON, không yêu cầu thay đổi bất kỳ dòng code nào ở Backend MDMSv2.
* **Chống chịu rớt mạng tuyệt đối (Resilience by Design):** Tích hợp **Circuit Breaker** và **Hybrid Smart Buffer** (Latest Snapshot cho thông số tức thời + Transition Queue cho sự kiện/cảnh báo), không bao giờ tràn RAM hay treo service dù mất mạng hàng tháng.
* **Đa giao thức thu thập:** Hỗ trợ OPC-DA (kết nối Kepware hiện hữu), Modbus RTU/TCP Master thích ứng (Adaptive PDU), và sẵn sàng mở rộng DLMS/COSEM (IEC 62056).

---

## 2. KIẾN TRÚC TỔNG THỂ (SYSTEM ARCHITECTURE)

```
+-----------------------------------------------------------------------------------+
|                                MDMS.HES ENGINE                                    |
|                                                                                   |
|  +------------------------+  +------------------------+  +---------------------+  |
|  |  Data Collection Layer |  |    Processing Engine   |  | Smart Uplink Layer  |  |
|  |                        |  |                        |  |                     |  |
|  | [OPC-DA COM Client]    |  | [OBIS DateTime Builder]|  | [Single Probe]      |  |
|  | (SDGv2 Interop Wrapper)|  | (Short -> String Tags) |  | [Circuit Breaker]   |  |
|  |                        |  |                        |  |                     |  |
|  | [Modbus RTU / TCP]     |  | [Adaptive PDU Planner] |  | [Hybrid Smart Buff] |  |
|  | (Adaptive Sizer 100->2)|  | (Dynamic Length Sizer) |  | - Latest Snapshot   |  |
|  |                        |  |                        |  | - Event FIFO Queue  |  |
|  | [Built-in Simulator]   |  | [Register Extractor]   |  |                     |  |
|  | (162 Virtual Meters)   |  | & [Expression Engine]  |  | [REST JSON Poster]  |  |
|  +------------------------+  +------------------------+  +---------------------+  |
+-----------------------------------------------------------------------------------+
                                          |
                                          | POST /api/iot/telemetry (JSON Narrow)
                                          v
+-----------------------------------------------------------------------------------+
|                             MDMSv2 BACKEND API                                    |
|             (IoTController -> RawIngestionWorker -> DB Snapshot / TimeSeries)      |
+-----------------------------------------------------------------------------------+
```

### 2.1. Phân tầng cấu trúc Solution (.NET 8)
* `MDMS.HES.Core`: Chứa Domain Model, Tag Definition, Channel/Device Entity, Buffer Data Contracts.
* `MDMS.HES.Application`: Quản lý chu trình thu thập (Polling Scheduler), điều phối Uplink, quản lý trạng thái kết nối và log chẩn đoán.
* `MDMS.HES.Infrastructure.OpcDa`: Kế thừa COM Automation wrapper từ `SDG.OPC-DA` (chạy trên Windows x86/x64).
* `MDMS.HES.Infrastructure.Modbus`: Kế thừa & nâng cấp từ `SDG.Modbus` (RTU/SerialPort, TCP Client, Adaptive PDU Sizer).
* `MDMS.HES.Infrastructure.Uplink`: HTTP REST Client hiệu năng cao (`SocketsHttpHandler`), Circuit Breaker, Hybrid Smart Buffer.
* `MDMS.HES.Infrastructure.Simulation`: Engine mô phỏng tải 162 công tơ nội bộ, tự phát sinh dữ liệu chỉ số, công suất, điện áp theo thời gian thực.
* `MDMS.HES.Host`: Ứng dụng Console / BackgroundService, hỗ trợ chạy như Windows Service, Linux systemd daemon hoặc Docker container.

---

## 3. CÁC TÍNH NĂNG KỸ THUẬT CỐT LÕI (TECHNICAL HIGHLIGHTS)

### 3.1. ObisDateTimeStringTagBuilder (Chuẩn hóa Date & Time OBIS)
* **Vấn đề:** Các Driver đo đếm (hoặc mô phỏng) thường đọc ngày giờ dưới dạng các thanh ghi riêng lẻ:
  `DT.Current.Year`, `DT.Current.Month`, `DT.Current.Day`, `DT.Current.Hour`, `DT.Current.Minute`, `DT.Current.Second` (kiểu `short`/`int`).
* **Giải pháp HES:**
  Bộ xử lý `ObisDateTimeStringTagBuilder` tự động thu thập và ghép 24 biến `short` thành đúng 8 Tag OBIS chuẩn định dạng chuỗi:
  * `0_9_1` (Current Time: `HH:mm:ss`) & `0_9_2` (Current Date: `yyyy-MM-dd`)
  * `0_9_3` (MDC Time: `HH:mm:ss`) & `0_9_4` (MDC Date: `yyyy-MM-dd`)
  * `0_9_5` (MDP Time: `HH:mm:ss`) & `0_9_6` (MDP Date: `yyyy-MM-dd`)
  * `0_9_7` (Reset Time: `HH:mm:ss`) & `0_9_8` (Reset Date: `yyyy-MM-dd`)
* **Khả năng tương thích:** Trùng khớp 100% với hàm `TryDecomposeCompositeDateTime` tại Backend MDMSv2, đảm bảo dữ liệu ghi nhận tức thì vào cơ sở dữ liệu mà không cần xử lý bù.

### 3.2. Single-Probe Circuit Breaker & Smart Uplink Publisher
* **Cơ chế chống bão Request (Anti-Storming):**
  * Bình thường (State = **Closed**): Dữ liệu gửi chu kỳ 60s/lần cho từng công tơ về endpoint `/api/iot/telemetry`.
  * Khi Server sập / Mất mạng (State = **Open**): Ngay khi gói tin đầu tiên bị lỗi (Timeout hoặc HTTP 5xx/Connection Refused), Circuit Breaker **ngắt lập tức toàn bộ 162 kênh publish**.
  * Chế độ Thăm dò (State = **Half-Open**): Duy nhất 1 luồng ping nhẹ (Healthcheck probe) được gửi mỗi 5 giây. 161 luồng còn lại ở trạng thái ngủ (Sleep/Paused), không tiêu tốn CPU, không tạo nghẽn Socket, không sinh log rác.
  * Phục hồi (Auto-Recovery): Ngay khi ping probe thành công 2 lần liên tiếp, Circuit Breaker đóng lại (Closed) và xả dữ liệu đệm về server tuần tự.

### 3.3. Hybrid Smart Buffer (Bộ nhớ đệm thông minh 2 lớp)
Khắc phục triệt để lỗi Out-Of-Memory của Kepware Narrow Format:
1. **Lớp 1 - Latest Snapshot In-Place (Thông số đo đếm & Sản lượng):**
   * Điện áp, Dòng điện, Công suất, Tần số, Chỉ số tích lũy $1.8.0 \dots 3.8.0$ chỉ giữ bản ghi **Mới nhất (Latest Value)** cho mỗi công tơ.
   * Độ phức tạp bộ nhớ: $O(N)$ với $N = 162$ điểm đo $\approx 100$ KB RAM cố định. Dù mất mạng 1 ngày, 1 tháng hay 1 năm, bộ nhớ RAM tiêu thụ không bao giờ tăng.
2. **Lớp 2 - Alarm & Event Transition FIFO Queue (Sự kiện & Cảnh báo):**
   * Các tag trạng thái $96.5.0 \dots 97.97.7$ chỉ được đẩy vào hàng đợi FIFO khi có **chuyển biến trạng thái** (State Transition: $0 \rightarrow 1$ hoặc $1 \rightarrow 0$).
   * Kèm theo nhãn thời gian chính xác (Timestamp). Đảm bảo không bỏ sót bất kỳ sự kiện mất nguồn, quá áp hay cảnh báo kỹ thuật nào khi mạng phục hồi.

### 3.4. Adaptive PDU Planner cho Modbus (Tự động thích ứng độ dài PDU)
* **Thực tế hiện trường:** Nhiều dòng công tơ điện tử (hoặc RTU trung gian) trong điều kiện bình thường cho phép đọc Block lớn tới 100 registers/message (để tăng tốc độ quét), nhưng khi đường truyền suy hao hoặc công tơ tải cao sẽ trả về lỗi `Time-Out` hoặc `Illegal Data Address`.
* **Thuật toán thích ứng (Adaptive Fallback):**
  * Khởi tạo mức đọc tối ưu: $L = 100$ registers.
  * Nếu gặp timeout hoặc CRC error $k$ lần liên tiếp: Tự động hạ bậc xuống $L = 32 \rightarrow L = 8 \rightarrow L = 2$ registers.
  * Khi truyền thông ổn định trở lại qua $M$ chu kỳ: Tự động thử nghiệm nâng bậc dần lên để khôi phục tốc độ tối đa.

### 3.5. Flexible Register Extractor & Expression Builder (Kế thừa SDGv2)
* Hỗ trợ mọi kiểu mã hóa byte công nghiệp: Big-Endian, Little-Endian, Mid-Endian (ABCD, CDAB, BADC, DCBA), IEEE 754 Float32, Double, Int32, Int64, BCD.
* Tích hợp bộ phân tích biểu thức Shunting-Yard AST (`SDG.ExpressionBuilder`) cho phép cấu hình tính toán nhân hệ số TU/TI, bù công suất hoặc chuẩn hóa đơn vị trực tiếp tại tầng thu thập.

---

## 4. KẾ HOẠCH TRIỂN KHAI THEO TỪNG GIAI ĐOẠN (ROADMAP)

### Giai đoạn 1 (Phase 1): HES Core + OPC-DA Client + Simulator & Smart Uplink
*Trạng thái: **ĐÃ HOÀN THÀNH (DONE)** | Unit Tests: 12/12 PASS*
* **Mục tiêu:** Thay thế ngay lập tức Kepware IoT Gateway bằng HES chạy trên Windows, kết nối dữ liệu từ Kepware Runtime (hoặc Built-in Simulator) qua OPC-DA, giải quyết dứt điểm lỗi treo service.
* **Các hạng mục thực hiện:**
  1. [x] Khởi tạo cấu trúc dự án `MDMS.HES` (.NET 8): `MDMS.HES.Core`, `MDMS.HES.Application`, `MDMS.HES.Infrastructure.OpcDa`, `MDMS.HES.Infrastructure.Simulation`, `MDMS.HES.Infrastructure.Uplink`, `MDMS.HES.Host`.
  2. [x] Tích hợp `SDG.OPC-DA` (Interop COM Automation) để đọc dữ liệu từ `Kepware.KEPServerEX.V6`.
  3. [x] Xây dựng module `ObisDateTimeStringTagBuilder` tổng hợp 24 biến short thành 8 chuỗi OBIS (`0_9_1` .. `0_9_8`).
  4. [x] Xây dựng `SmartRestUplinkPublisher` với `SingleProbeCircuitBreaker` (Closed/Open/HalfOpen) và `HybridSmartBuffer` (Latest Snapshot O(1) + Event FIFO).
  5. [x] Xây dựng `SimulationDriver` giả lập 162 công tơ chạy độc lập không cần Kepware để kiểm thử.
  6. [x] Trích xuất đầy đủ 162 công tơ thực tế với Serial và Private-key từ Kepware sang `meters.json`.
  7. [x] Viết bộ Unit Tests toàn diện (`MDMS.HES.UnitTests`), 12/12 bài test vượt qua thành công 100%.

### Giai đoạn 2 (Phase 2): Modbus RTU/TCP Driver + Adaptive PDU Engine
*Thời gian dự kiến: 5 - 7 ngày*
* **Mục tiêu:** Thu thập dữ liệu trực tiếp từ công tơ thực tế qua đường truyền RS485 / Serial / Ethernet Modbus, không cần Kepware làm trung gian.
* **Các hạng mục thực hiện:**
  1. Porting và refactor `SDG.Modbus` (`RTU/PhysicalLayer`, `TCP/Connection`, `PDU`).
  2. Hiện thực hóa `AdaptivePduPlanner` (cơ chế phân mảnh từ 100 xuống 2 registers).
  3. Hiện thực hóa `FlexibleRegisterExtractor` hỗ trợ swap byte và giải mã số liệu.
  4. Tích hợp `SDG.ExpressionBuilder` để biên dịch các biểu thức quy đổi hệ số tỷ số đo.
  5. Thiết kế định dạng cấu hình `meters_config.json` và `modbus_profiles.json` chuẩn JSON gọn nhẹ.

### Giai đoạn 3 (Phase 3): Giao diện Giám sát & Quản trị Nhẹ (Web Diagnostic UI)
*Thời gian dự kiến: 3 - 4 ngày*
* **Mục tiêu:** Cung cấp Dashboard trực quan ngay trên port HTTP nội bộ của HES để kỹ sư vận hành dễ dàng theo dõi trên cả PC và thiết bị di động.
* **Các hạng mục thực hiện:**
  1. Nhúng ASP.NET Core Minimal API vào HES Host.
  2. Giao diện Web SPA tĩnh siêu nhẹ (Single HTML/JS bundle nhúng vào Assembly).
  3. Bảng điều khiển: Tình trạng kết nối các kênh (Online/Offline/Retry Count), Tốc độ Uplink, Live Data Grid, và nút Restart/Reconnect từng kênh đo.

### Giai đoạn 4 (Phase 4): Mở rộng Giao thức Đo đếm DLMS/COSEM
*Thời gian dự kiến: Tùy chọn nâng cao (Backlog)*
* **Mục tiêu:** Hỗ trợ đọc trực tiếp dữ liệu công tơ cao cấp (Elster A1700/A1800, Landis+Gyr, Gelex) qua chuẩn IEC 62056-21 / IEC 62056-47 (HDLC / TCP Wrapper).

---

## 5. KẾ HOẠCH ĐÓNG GÓI VÀ TRIỂN KHAI ĐA NỀN TẢNG (PACKAGING & RUNTIME)

| Nền tảng đích | Hình thức triển khai | Kỹ thuật biên dịch | Ghi chú |
| :--- | :--- | :--- | :--- |
| **Windows Server / 10 / 11** | Windows Service / Console Daemon | .NET 8 Standalone / Framework-dependent | Hỗ trợ đầy đủ OPC-DA COM Client và Modbus |
| **Linux Server (x64)** | systemd service / Docker Container | `linux-x64` Native single-file | Thu thập Modbus TCP / RTU (USB-to-RS485), Smart Uplink |
| **Industrial IoT (ARM32/ARM64)** *(Artila Matrix-752)* | Linux Daemon / Package `.deb` | `linux-arm` / `linux-arm64` Single-File NativeAOT | Tiết kiệm RAM (< 30MB), CPU < 5%, khởi động < 1s |

---

## 6. TIÊU CHÍ NGHIỆM THU (ACCEPTANCE CRITERIA)
1. **Khả năng tự phục hồi (Self-Healing):** Ngắt mạng liên tục trong 8–24 giờ; khi có mạng trở lại, HES phải tự động khôi phục kết nối và đẩy dữ liệu về MDMSv2 trong vòng 30 giây mà không cần can thiệp thủ công.
2. **Tiêu thụ tài nguyên:** RAM < 80 MB trên Windows; RAM < 35 MB trên IoT Gateway ARM; CPU bình quân < 3%.
3. **Độ chính xác dữ liệu:** 100% trường dữ liệu OBIS (U, I, P, Q, PF, F, Điện năng tích lũy, Trạng thái cảnh báo, Ngày giờ chốt) trùng khớp chuẩn REST API của MDMSv2.
4. **Không rò rỉ Socket & Thread:** Số lượng socket TCP duy trì ổn định, không bị hiện tượng TIME_WAIT dồn ứ hoặc Thread pool bùng phát khi mạng chập chờn.

---
*Kế hoạch này là tài liệu cơ sở kỹ thuật để triển khai mã nguồn và giám sát tiến độ thực hiện.*

