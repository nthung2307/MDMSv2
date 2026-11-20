# KẾ HOẠCH PHÁT TRIỂN HỆ THỐNG THU THẬP DỮ LIỆU ĐO ĐẾM MDMS-HES (HEAD-END SYSTEM)

> **Mã dự án:** MDMS.HES  
> **Phiên bản:** 1.1.0  
> **Ngày cập nhật:** 20/11/2026  
> **Trạng thái:** Phase 1 Hoàn thành — Chuẩn bị triển khai Phase 1.5 (Cấu hình, Mapping & Web Console)  
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
* **Tương thích 100% với REST Telemetry hiện hữu:** Đẩy dữ liệu trực tiếp về `POST /IoT/telemetry` của MDMSv2 theo chuẩn Narrow Format định dạng JSON, không yêu cầu thay đổi bất kỳ dòng code nào ở Backend MDMSv2.
* **Chống chịu rớt mạng tuyệt đối (Resilience by Design):** Tích hợp **Circuit Breaker** và **Hybrid Smart Buffer** (Latest Snapshot cho thông số tức thời + Transition Queue cho sự kiện/cảnh báo), không bao giờ tràn RAM hay treo service dù mất mạng hàng tháng.
* **Cấu hình & Mapping Dữ liệu trực quan (Zero-Code Provisioning):** Tích hợp sẵn Web Management Console siêu nhẹ ngay trong HES Host để cài đặt Driver (Simulation / OPC-DA / Modbus), Mapping Tag nguồn $\leftrightarrow$ OBIS, quản lý REST Client, Serial Number & Private API Key theo từng công tơ, hỗ trợ Hot-Reload không cần restart service.
* **Đa giao thức thu thập:** Hỗ trợ OPC-DA (kết nối Kepware hiện hữu), Modbus RTU/TCP Master thích ứng (Adaptive PDU), và sẵn sàng mở rộng DLMS/COSEM (IEC 62056).

---

## 2. KIẾN TRÚC TỔNG THỂ (SYSTEM ARCHITECTURE)

```
+-----------------------------------------------------------------------------------+
|                                MDMS.HES ENGINE                                    |
|                                                                                   |
|  +-----------------------------------------------------------------------------+  |
|  |         Embedded Web Management Console & Config Store (Port 19090)         |  |
|  |  [Driver Config] [OBIS Mapping Profiles] [REST Uplink] [Meters/Serial/Key]  |  |
|  |  [Kepware JSON / CSV Importer] [Live Tag Diagnostics] [Hot-Reload Engine]   |  |
|  +-----------------------------------------------------------------------------+  |
|                                        |                                          |
|  +------------------------+  +------------------------+  +---------------------+  |
|  |  Data Collection Layer |  |    Processing Engine   |  | Smart Uplink Layer  |  |
|  |                        |  |                        |  |                     |  |
|  | [OPC-DA COM Client]    |  | [Tag Mapping Engine]   |  | [Single Probe]      |  |
|  | (SDGv2 Interop Wrapper)|  | (Source -> OBIS/Scale) |  | [Circuit Breaker]   |  |
|  |                        |  |                        |  |                     |  |
|  | [Modbus RTU / TCP]     |  | [OBIS DateTime Builder]|  | [Hybrid Smart Buff] |  |
|  | (Adaptive Sizer 100->2)|  | (24 Short -> 8 OBIS)   |  | - Latest Snapshot   |  |
|  |                        |  |                        |  | - Event FIFO Queue  |  |
|  | [Built-in Simulator]   |  | [Register Extractor]   |  |                     |  |
|  | (162 Virtual + Alarms) |  | & [Expression Engine]  |  | [REST JSON Poster]  |  |
|  +------------------------+  +------------------------+  +---------------------+  |
+-----------------------------------------------------------------------------------+
                                          |
                                          | POST /IoT/telemetry (JSON Narrow)
                                          v
+-----------------------------------------------------------------------------------+
|                             MDMSv2 BACKEND API                                    |
|             (IoTController -> RawIngestionWorker -> DB Snapshot / TimeSeries)      |
+-----------------------------------------------------------------------------------+
```

### 2.1. Phân tầng cấu trúc Solution (.NET 8)
* `MDMS.HES.Core`: Chứa Domain Model, Tag Definition, OBIS Catalog, Mapping Profile Model, Unified Config Schema, Buffer & Driver Interfaces.
* `MDMS.HES.Application`: Quản lý chu trình thu thập, `TagMappingEngine`, `ObisDateTimeStringTagBuilder`, `HesConfigurationStore` (Atomic JSON persistence + Hot-Reload), `KepwareConfigImporter`.
* `MDMS.HES.Infrastructure.OpcDa`: Kế thừa COM Automation wrapper từ `SDG.OPC-DA` (chạy trên Windows x86/x64, hỗ trợ Dynamic Tag Subscription & OPC Tag Browsing).
* `MDMS.HES.Infrastructure.Modbus`: Kế thừa & nâng cấp từ `SDG.Modbus` (RTU/SerialPort, TCP Client, Adaptive PDU Sizer) *(Phase 2)*.
* `MDMS.HES.Infrastructure.Uplink`: HTTP REST Client hiệu năng cao (`SocketsHttpHandler`), Circuit Breaker, Hybrid Smart Buffer.
* `MDMS.HES.Infrastructure.Simulation`: Engine mô phỏng tải 162 công tơ nội bộ (tùy chỉnh dải thông số & tiêm sự kiện cảnh báo giả lập).
* `MDMS.HES.Host`: Ứng dụng BackgroundService kết hợp **ASP.NET Core Minimal API + Embedded Web UI** siêu nhẹ (không cần Node.js runtime), chạy tốt trên Windows Service, Linux systemd daemon, Docker và IoT Gateway ARM.

---

## 3. CÁC TÍNH NĂNG KỸ THUẬT CỐT LÕI (TECHNICAL HIGHLIGHTS)

### 3.1. ObisDateTimeStringTagBuilder (Chuẩn hóa Date & Time OBIS)
* **Vấn đề:** Các Driver đo đếm (hoặc Kepware/Modbus) thường đọc ngày giờ dưới dạng 24 thanh ghi riêng lẻ kiểu `short`/`int`: `Year`, `Month`, `Day`, `Hour`, `Minute`, `Second`.
* **Giải pháp HES:**
  Bộ xử lý `ObisDateTimeStringTagBuilder` tự động thu thập và ghép 24 biến `short` thành đúng 8 Tag OBIS chuẩn định dạng chuỗi (khớp 100% `ObisTelemetryCatalog` của MDMSv2 Backend):
  * `0_9_1` (Current Time: `HH:mm:ss`) & `0_9_2` (Current Date: `yyyy-MM-dd`) — từ `DT.Current.*`
  * `0_9_3` (Closing/Reset Date: `yyyy-MM-dd`) & `0_9_4` (Closing/Reset Time: `HH:mm:ss`) — từ `DT.Reset.*`
  * `0_9_5` (Pmax Current / MDC Date: `yyyy-MM-dd`) & `0_9_6` (Pmax Current / MDC Time: `HH:mm:ss`) — từ `DT.MDC.*`
  * `0_9_7` (Pmax Prev / MDP Date: `yyyy-MM-dd`) & `0_9_8` (Pmax Prev / MDP Time: `HH:mm:ss`) — từ `DT.MDP.*`

### 3.2. Single-Probe Circuit Breaker & Smart Uplink Publisher
* **Cơ chế chống bão Request (Anti-Storming):**
  * Bình thường (State = **Closed**): Dữ liệu gửi chu kỳ 60s/lần cho từng công tơ về endpoint `/IoT/telemetry`.
  * Khi Server sập / Mất mạng (State = **Open**): Ngay khi gói tin đầu tiên bị lỗi (Timeout hoặc HTTP 5xx/Connection Refused), Circuit Breaker **ngắt lập tức toàn bộ 162 kênh publish**.
  * Chế độ Thăm dò (State = **Half-Open**): Duy nhất 1 luồng ping có xác thực (`Serial` + `Private-key`, payload rỗng) được gửi mỗi 5 giây. 161 luồng còn lại ở trạng thái ngủ (Sleep/Paused), không tiêu tốn CPU, không tạo nghẽn Socket, không sinh cảnh báo `UNAUTHORIZED` giả.
  * Phục hồi (Auto-Recovery): Ngay khi probe thành công 2 lần liên tiếp, Circuit Breaker đóng lại (Closed) và xả dữ liệu đệm về server tuần tự.

### 3.3. Hybrid Smart Buffer (Bộ nhớ đệm thông minh 2 lớp)
1. **Lớp 1 - Latest Snapshot In-Place (Thông số đo đếm & Sản lượng):**
   * Chỉ giữ bản ghi **Mới nhất (Latest Value)** cho mỗi công tơ ($O(N)$ RAM cố định $\approx 100$ KB cho 162 điểm đo). Đóng dấu nhãn thời gian chu kỳ quét hiện tại (`scanTimestampMs`) khi publish để các tag tĩnh vẫn cập nhật `LastTelemetryAt`.
2. **Lớp 2 - Alarm & Event Transition FIFO Queue (Sự kiện & Cảnh báo):**
   * Các tag trạng thái `96_5_0`, `97_97_1..10` chỉ đẩy vào hàng đợi FIFO khi có **chuyển biến trạng thái** (`0 <-> 1`), giữ nguyên nhãn thời gian chính xác tại thời điểm xảy ra sự cố.

### 3.4. Hệ thống Cấu hình Thống nhất & Tag Mapping Engine (Phase 1.5 — Mới bổ sung)
Giải quyết bài toán vận hành thực tế mà không cần chỉnh sửa code hay file JSON thủ công:

1. **Unified Configuration Store (`hes-config.json` + Atomic Save + Hot-Reload):**
   * Tập trung toàn bộ cấu hình vào 1 nguồn chân lý duy nhất:
     * `Uplink`: `TargetUrl`, `GlobalApiKey`, `TimeoutSeconds`, `MaxEventsPerPublish`, `DefaultScanRateMs`, `CircuitBreaker` (`FailureThreshold`, `SuccessThreshold`, `ProbeIntervalSeconds`).
     * `Driver`: `ActiveDriver` (`Simulation` | `OPC-DA` | `Modbus`), cấu hình riêng cho **Simulation** (dải `U/I/PF/Hz`, chu kỳ tick, danh sách cảnh báo đang tiêm giả lập) và **OPC-DA** (`ProgId`, `Host`, `GroupName`, `UpdateRateMs`, chế độ `SharedTemplate` hoặc `PerMeter`).
     * `MappingProfiles`: Danh sách các bộ mẫu ánh xạ tái sử dụng (Template) ánh xạ **Source Tag** $\rightarrow$ **Target OBIS Code**, hỗ trợ biến thay thế `{MeterId}` / `{TagPrefix}` (ví dụ `{TagPrefix}.Meas.32_7_0` hoặc `_AdvancedTags.Template.Meas.32_7_0`), `ScaleFactor`, `Offset`, và `DateTimeGroup` (`Current/Reset/MDC/MDP`).
     * `Meters`: Danh sách công tơ đầy đủ: `MeterId`, `Name`, `Serial`, `ApiKey` (Private-key), `Enabled`, `ScanRateMs`, `MappingProfileId`, `TagPrefix`, `OverrideMappings` (tùy biến riêng cho từng công tơ nếu cần).
   * Lưu file theo cơ chế **Atomic Write** (ghi file tạm `.tmp` $\rightarrow$ backup `.bak` $\rightarrow$ rename) để chống hỏng cấu hình khi mất điện đột ngột trên thiết bị IoT.
   * **Hot-Reload Engine:** Khi người dùng nhấn **"Lưu & Áp dụng"** trên Web UI, `HesWorker` tự động áp dụng cấu hình mới (đổi URL, đổi API Key, thêm/xóa công tơ, đổi mapping tag, chuyển đổi giữa Simulation $\leftrightarrow$ OPC-DA) trong < 1 giây mà không cần tắt tiến trình.

2. **Giao diện Web Quản trị Nhúng (Embedded Management Console — Port `19090`):**
   * Nhúng ASP.NET Core Minimal API + Giao diện Web SPA tĩnh gọn nhẹ trực tiếp trong `MDMS.HES.Host` (không phụ thuộc Node.js, chạy tốt trên Windows / Linux / ARM IoT).
   * **5 Phân hệ màn hình trực quan:**
     1. **Dashboard Tổng quan:** Trạng thái Engine, Driver đang chạy, Circuit Breaker (Closed/Open/Half-Open + nút Reset/Probe ngay), tốc độ đẩy dữ liệu (ms/chu kỳ), số lượng công tơ Online/Lỗi, Log real-time.
     2. **Cài đặt REST Client (Uplink):** Sửa Target URL, Global API Key, Timeout, Chu kỳ đẩy, ngưỡng Circuit Breaker + nút **Test Connection** hiển thị mã HTTP & phản hồi JSON thực tế từ MDMSv2.
     3. **Cài đặt Nguồn Dữ liệu (Simulation & OPC-DA):**
        * Tab **Simulation:** Bật/Tắt chế độ mô phỏng, chỉnh tốc độ tick, dải U/I/PF, và bảng **Tiêm Cảnh báo giả lập (Alarm Injector)** (bật/tắt `96_5_0`, `97_97_1..7` cho từng công tơ để test ngay luồng Event/Alert của MDMSv2).
        * Tab **OPC-DA:** Cấu hình `ProgId` (nút **Scan OPC Servers** trên máy Windows), `GroupName`, `UpdateRateMs`, nút **Test & Browse OPC Tags** để duyệt cây tag trực tiếp từ Kepware và kiểm tra chất lượng `rawQ` (Good/Bad) trước khi mapping.
     4. **Quản lý Mapping Dữ liệu (Mapping Profiles):**
        * Danh sách Profile mẫu (tích hợp sẵn `Kepware_Template_60OBIS`, `Direct_Prefix_60OBIS`, `Simulation_Default`).
        * Bảng Mapping 2 cột trực quan: Chọn **Target OBIS** từ danh mục chuẩn 60-tag OBIS (có sẵn tên tiếng Việt, đơn vị, phân nhóm Điện áp/Dòng điện/Công suất/Điện năng/Ngày giờ/Cảnh báo) $\leftrightarrow$ nhập hoặc chọn **Source Tag** (hỗ trợ `{TagPrefix}`), `ScaleFactor`, `Offset`.
        * Hỗ trợ cấu hình nhóm **Date & Time Synthesis** (24 biến short $\rightarrow$ 8 OBIS `0_9_1..0_9_8`).
     5. **Quản lý Công tơ (Serial Number & Private API Key):**
        * Bảng danh sách 162+ công tơ: Tìm kiếm, Lọc, Thêm mới, Sửa, Xóa, Bật/Tắt hàng loạt.
        * Quản lý `Serial Number`, `Private API Key` (có nút ẩn/hiện & copy), gán `Mapping Profile`, `TagPrefix`, `ScanRateMs`.
        * **Import / Export 1-Click:** Upload file `Meter Simulation.json` của Kepware để tự động nhập toàn bộ danh sách công tơ + Serial + Private-key + Mapping; hoặc Import/Export CSV/JSON.
        * **Live Inspector & Test Push:** Xem bảng giá trị tag trực tiếp của từng công tơ (Source Tag $\rightarrow$ Target OBIS $\rightarrow$ Giá trị $\rightarrow$ Quality $\rightarrow$ Thời gian) và nút **Đẩy thử ngay (Push Now)** để kiểm tra phản hồi từ MDMSv2 Backend.

### 3.5. Adaptive PDU Planner cho Modbus (Phase 2)
* Tự động thích ứng độ dài đọc thanh ghi từ $L = 100 \rightarrow 32 \rightarrow 8 \rightarrow 2$ registers khi công tơ timeout/lỗi CRC, và tự nâng bậc trở lại khi đường truyền ổn định.

### 3.6. Flexible Register Extractor & Expression Builder (Phase 2 — Kế thừa SDGv2)
* Hỗ trợ mọi kiểu mã hóa byte công nghiệp: Big-Endian, Little-Endian, Mid-Endian (ABCD, CDAB, BADC, DCBA), IEEE 754 Float32, Double, Int32, Int64, BCD.
* Tích hợp bộ phân tích biểu thức Shunting-Yard AST (`SDG.ExpressionBuilder`) tính toán hệ số TU/TI ngay tại Edge.

---

## 4. KẾ HOẠCH TRIỂN KHAI THEO TỪNG GIAI ĐOẠN (ROADMAP)

### Giai đoạn 1 (Phase 1): HES Core + OPC-DA Client + Simulator & Smart Uplink
*Trạng thái: **ĐÃ HOÀN THÀNH (DONE)** | Unit Tests: 12/12 PASS | Đã kiểm thử thực tế 162 công tơ với Kepware OPC-DA (< 1s/chu kỳ)*
* **Các hạng mục đã hoàn thành:**
  1. [x] Khởi tạo cấu trúc Solution `MDMS.HES` (.NET 8 Clean Architecture).
  2. [x] Tích hợp `SDG.OPC-DA` (Interop COM Automation) đọc 60/60 template tags từ `Kepware.KEPServerEX.V6`, xử lý chuẩn hóa UTC Timestamp và Quality `192 -> true`.
  3. [x] Xây dựng `ObisDateTimeStringTagBuilder` tổng hợp 24 biến `short` thành 8 chuỗi OBIS (`0_9_1` .. `0_9_8`).
  4. [x] Xây dựng `RestTelemetryPublisher`, `CircuitBreaker` (Single-Probe có xác thực) và `SmartBufferManager` (2 lớp Snapshot + Event FIFO).
  5. [x] Xây dựng `SimulationDriver` giả lập đầy đủ 60 mã OBIS cho 162 công tơ.
  6. [x] Trích xuất 162 công tơ thực tế với `Serial` và `Private-key` sang `meters.json`.
  7. [x] Unit Tests 12/12 PASS.

---

### Giai đoạn 1.5 (Phase 1.5): Cài đặt Hệ thống, Mapping Dữ liệu & Embedded Web Console *(Ưu tiên thực hiện ngay trước Phase 2)*
*Trạng thái: **SẴN SÀNG TRIỂN KHAI (NEXT)** | Thời gian dự kiến: 2 - 3 ngày*
* **Mục tiêu:** Cung cấp đầy đủ năng lực cấu hình, ánh xạ dữ liệu linh hoạt (Simulation, OPC-DA, REST Client, Serial Number & Private API Key) qua giao diện Web nhúng trực quan + Hot-Reload, làm nền tảng chuẩn cho cả Phase 2 (Modbus) và Phase 4 (DLMS).
* **Các hạng mục thực hiện chi tiết:**
  1. [ ] **Task 1.5.1 — Unified Config & Mapping Domain Models (`MDMS.HES.Core`):**
     * Định nghĩa `HesSystemConfig`, `UplinkConfig`, `SimulationConfig`, `OpcDaConfig`, `TagMappingProfile`, `TagMappingRule`, `DateTimeMappingGroup`, `MeterConfig` (bổ sung `MappingProfileId`, `TagPrefix`).
     * Tích hợp `ObisCatalog` đầy đủ 60 mã OBIS chuẩn MDMSv2 (Mã OBIS, Tên tiếng Việt, Đơn vị, Phân loại) phục vụ dropdown Mapping UI.
  2. [ ] **Task 1.5.2 — Configuration Store, Hot-Reload & Tag Mapping Engine (`MDMS.HES.Application`):**
     * Xây dựng `HesConfigurationStore`: Đọc/ghi `hes-config.json` theo cơ chế Atomic Write, tự động migrate từ `appsettings.json` + `meters.json` ở lần chạy đầu tiên.
     * Xây dựng `TagMappingEngine`: Ánh xạ động `SourceTag -> TargetObisCode`, hỗ trợ `{MeterId}` / `{TagPrefix}`, `ScaleFactor`, `Offset`, và tách luồng `DateTimePart -> ObisDateTimeStringTagBuilder`.
     * Xây dựng `KepwareJsonImporter` & `MeterCsvService`: Import trực tiếp từ `Meter Simulation.json` hoặc file CSV/JSON danh sách công tơ (`MeterId, Serial, Private-key, Profile, Prefix`).
  3. [ ] **Task 1.5.3 — Nâng cấp Drivers hỗ trợ Dynamic Config & Diagnostics (`Simulation` & `OPC-DA`):**
     * Nâng cấp `OpcDaDriver`: Đọc danh sách tag đăng ký động từ `MappingProfiles` + `Meters` (hỗ trợ cả Shared Template lẫn Per-Meter Prefix), bổ sung hàm `BrowseOpcServers()` và `BrowseOpcTags(prefix)` phục vụ Web UI.
     * Nâng cấp `SimulationDriver`: Nhận tham số mô phỏng động từ `SimulationConfig`, hỗ trợ API tiêm/xóa cảnh báo giả lập (`96_5_0`, `97_97_1..10`) cho từng công tơ theo thời gian thực.
     * Nâng cấp `HesWorker`: Hỗ trợ `ReloadAsync()` (Hot-Reload không cần restart tiến trình), lưu lịch sử chẩn đoán Uplink gần nhất, và hỗ trợ `TestPushMeterAsync(meterId)`.
  4. [ ] **Task 1.5.4 — Embedded Management API, Web Auth Security & Lightweight Web Console (`MDMS.HES.Host`):**
     * Chuyển `MDMS.HES.Host` sang `Microsoft.NET.Sdk.Web` (vẫn hỗ trợ chạy Windows Service & Linux systemd), lắng nghe cổng quản trị cấu hình được trong `appsettings.json` (mặc định `http://0.0.0.0:19090`).
     * **Bảo mật truy cập Web Console:** Màn hình đăng nhập (Username/Password lưu dạng PBKDF2-SHA256 hash trong cấu hình, mặc định `admin` / cấu hình khởi tạo trong `appsettings.json`, cho phép đổi mật khẩu ngay trên UI), bảo vệ toàn bộ `/api/*` bằng Bearer Token / HMAC Session Token, tự động hết hạn phiên và chống brute-force cơ bản.
     * Xây dựng REST APIs nội bộ:
       * `POST /api/auth/login`, `POST /api/auth/logout`, `POST /api/auth/change-password`, `GET /api/auth/me`
       * `GET /api/status`, `POST /api/control/reload`, `POST /api/control/reset-circuit`
       * `GET/PUT /api/config/uplink`, `POST /api/config/uplink/test`
       * `GET/PUT /api/config/drivers/simulation`, `POST /api/simulation/alarm`
       * `GET/PUT /api/config/drivers/opcda`, `GET /api/opcda/servers`, `GET /api/opcda/browse`
       * `GET /api/obis-catalog`, `GET/POST/PUT/DELETE /api/mappings/profiles`
       * `GET/POST/PUT/DELETE /api/meters`, `POST /api/meters/import-kepware`, `POST /api/meters/import-csv`, `GET /api/meters/export-csv`
       * `GET /api/meters/{id}/live-tags`, `POST /api/meters/{id}/push-now`
     * Xây dựng giao diện Web Console hiện đại (Responsive, không cần build Node.js, nhúng trực tiếp trong `wwwroot`) bao gồm màn hình Đăng nhập và đủ 5 tab: **Tổng quan**, **REST Client**, **Simulation / OPC-DA**, **Mapping Dữ liệu**, **Công tơ & API Key**.
  5. [ ] **Task 1.5.5 — Unit Tests & Integration Verification:**
     * Viết thêm Unit Tests cho `TagMappingEngine`, `HesConfigurationStore`, `KepwareJsonImporter`, `MeterCsvService`.
     * Kiểm thử thực tế toàn trình: Chỉnh sửa URL / Serial / Private-key / Mapping trên Web UI `http://localhost:19090` $\rightarrow$ Hot-Reload $\rightarrow$ Xác nhận cập nhật tức thì trên MDMSv2.

---

### Giai đoạn 2 (Phase 2): Modbus RTU/TCP Driver + Adaptive PDU Engine
*Thời gian dự kiến: 5 - 7 ngày*
* **Mục tiêu:** Thu thập dữ liệu trực tiếp từ công tơ thực tế qua đường truyền RS485 / Serial / Ethernet Modbus, không cần Kepware làm trung gian, tái sử dụng ngay Mapping Engine & Web Console của Phase 1.5.
* **Các hạng mục thực hiện:**
  1. [ ] Porting và refactor `SDG.Modbus` (`RTU/PhysicalLayer`, `TCP/Connection`, `PDU`).
  2. [ ] Hiện thực hóa `AdaptivePduPlanner` (cơ chế phân mảnh động từ 100 xuống 32 $\rightarrow$ 8 $\rightarrow$ 2 registers).
  3. [ ] Hiện thực hóa `FlexibleRegisterExtractor` hỗ trợ swap byte (ABCD/CDAB/BADC/DCBA) và giải mã số liệu.
  4. [ ] Tích hợp `SDG.ExpressionBuilder` (Shunting-Yard AST) để tính toán hệ số TU/TI & công thức tùy biến.
  5. [ ] Tích hợp tab cấu hình **Modbus Channels & Register Map** vào Embedded Web Console.

---

### Giai đoạn 3 (Phase 3): Đóng gói Đa Nền tảng & Giám sát Nâng cao (Packaging & Production Hardening)
*Thời gian dự kiến: 2 - 3 ngày*
* **Mục tiêu:** Đóng gói tự động cho Windows Service Installer, Linux Docker/systemd, và gói `.deb` cho IoT Gateway ARM (Artila Matrix-752), bổ sung xác thực đăng nhập cho Web Console và log xoay vòng (Rolling File Logs).

---

### Giai đoạn 4 (Phase 4): Mở rộng Giao thức Đo đếm DLMS/COSEM
*Thời gian dự kiến: Tùy chọn nâng cao (Backlog)*
* **Mục tiêu:** Hỗ trợ đọc trực tiếp dữ liệu công tơ cao cấp (Elster A1700/A1800, Landis+Gyr, Gelex) qua chuẩn IEC 62056-21 / IEC 62056-47 (HDLC / TCP Wrapper).

---

## 5. KẾ HOẠCH ĐÓNG GÓI VÀ TRIỂN KHAI ĐA NỀN TẢNG (PACKAGING & RUNTIME)

| Nền tảng đích | Hình thức triển khai | Kỹ thuật biên dịch | Ghi chú |
| :--- | :--- | :--- | :--- |
| **Windows Server / 10 / 11** | Windows Service / Console Daemon | .NET 8 Standalone / Framework-dependent | Hỗ trợ đầy đủ OPC-DA COM Client, Modbus, Simulation & Web UI |
| **Linux Server (x64)** | systemd service / Docker Container | `linux-x64` Native single-file | Thu thập Modbus TCP / RTU (USB-to-RS485), Simulation, Smart Uplink & Web UI |
| **Industrial IoT (ARM32/ARM64)** *(Artila Matrix-752)* | Linux Daemon / Package `.deb` | `linux-arm` / `linux-arm64` Single-File | Tiết kiệm RAM (< 40MB), CPU < 5%, khởi động < 1s, cấu hình qua Web UI nội bộ |

---

## 6. TIÊU CHÍ NGHIỆM THU (ACCEPTANCE CRITERIA)
1. **Khả năng tự phục hồi (Self-Healing):** Ngắt mạng liên tục trong 8–24 giờ; khi có mạng trở lại, HES phải tự động khôi phục kết nối và đẩy dữ liệu về MDMSv2 trong vòng 30 giây mà không cần can thiệp thủ công.
2. **Tiêu thụ tài nguyên:** RAM < 90 MB trên Windows; RAM < 45 MB trên IoT Gateway ARM; CPU bình quân < 3%.
3. **Độ chính xác dữ liệu:** 100% trường dữ liệu OBIS (U, I, P, Q, PF, F, Điện năng tích lũy, Trạng thái cảnh báo, Ngày giờ chốt) trùng khớp chuẩn REST API của MDMSv2 với `Quality = true`.
4. **Cấu hình linh hoạt (Zero-Code):** Kỹ sư vận hành có thể cài đặt REST Client, chuyển đổi Driver (Simulation/OPC-DA), cấu hình Mapping Tag $\leftrightarrow$ OBIS, và quản lý danh sách Serial Number & Private API Key ngay trên Web UI của HES mà không cần sửa code hay khởi động lại service.
5. **Không rò rỉ Socket & Thread:** Số lượng socket TCP duy trì ổn định, không bị hiện tượng TIME_WAIT dồn ứ hoặc Thread pool bùng phát khi mạng chập chờn.

---
*Kế hoạch này là tài liệu cơ sở kỹ thuật để triển khai mã nguồn và giám sát tiến độ thực hiện.*
