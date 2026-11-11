# [DEV UPDATE] PHASE 1 HOTFIX — KHẮC PHỤC XỬ LÝ TÍN HIỆU REST CLIENT 'Status' (CẢNH BÁO, SỰ KIỆN & KẾT NỐI)

> **Tài liệu cập nhật phát triển (Developer Changelog)**  
> **Phiên bản:** v3.2.1-PHASE1-STATUS-ALERT-INGESTION-HOTFIX  
> **Người thực hiện:** MDMS-Coder (Lead Full-Stack Developer)  
> **Thời gian:** 11/11/2026  

---

### Nguyên nhân gốc rễ & Các hạng mục khắc phục:
1. **Lỗi EF Core `DbUpdateConcurrencyException` (HTTP 500) khi một tag đổi trạng thái nhiều lần trong cùng gói Narrow Format (`false -> true -> false`)** ([`MeasurementPointEventCommandHandler.cs`](file:///e:/sources/MDMSv2/Backend/Infrastructure/Handlers/Commands/IoT/MeasurementPointEventCommandHandler.cs)):
   - Do trước đây thêm `MeasurementPointsAlertEntity` mới via `RestfulRepository.AddAsync` (`EntityState.Added`) rồi lại gọi `RemoveAsync` (`Attach` + `Remove`, đưa thực thể chưa lưu thành `EntityState.Deleted`) trong cùng một DbContext/request, khiến `SaveChangesAsync()` phát sinh `DELETE ... WHERE Id = @p0` 0 dòng và ném `DbUpdateConcurrencyException`.
   - Đã sửa bằng cách sắp xếp `matchedEntries` tăng dần theo `TimestampMs`, quản lý cảnh báo chờ tạo trong từ điển `pendingNewAlerts` trong suốt vòng lặp và chỉ gọi `AddAsync` sau vòng lặp nếu trạng thái cuối vẫn là `ALARM`.
   - Bổ sung dọn cả cảnh báo mồ côi (`TagNameId == null` khớp `MeasurementPointId` & `ParameterCode`) ở nhánh `NORMAL`, gom bản ghi Snapshot mới nhất theo `Description` trong `statusBatches` (`LatestSamples`), chỉ gọi `UpdateAsync` một lần cho mỗi `TagNameEntity` thay đổi (`dirtyTags`), và cô lập phát SignalR trong `TryBroadcastNotificationsAsync`.
2. **Lỗi HTTP 400 (`Bad Request`) khi gói Kepware chứa `"v": null` hoặc trường tag tùy chọn bị rỗng** ([`IoTDataRequest.cs`](file:///e:/sources/MDMSv2/Backend/Application/DataTransferObjects/Requests/IoT/IoTDataRequest.cs)):
   - Chuyển `IoTDataRequest.Values`, `IoTDataValue.Id`, `IoTDataValue.V`, `IoTDataValue.Q` sang nullable (`?`) để System.Text.Json / MVC model validation (`.NET 8 <Nullable>enable</Nullable>`) không từ chối toàn bộ gói Status trước khi vào controller.
3. **Chuẩn hóa Log Catch Blocks trong [`IoTController.cs`](file:///e:/sources/MDMSv2/Backend/WebApi/Controllers/IoTController.cs)**:
   - Sửa tên action trong log lỗi của `ReceiveDataByDateTimeAsync` và `ReceiveDataAlertAsync` từ `nameof(ReceiveDataAsync)` sang đúng tên hàm tương ứng.
4. **Bổ sung Unit Tests Hồi quy** ([`Phase1TelemetryAndPartitionTests.cs`](file:///e:/sources/MDMSv2/Backend/Tests/MDMS.UnitTests/Phase1TelemetryAndPartitionTests.cs)):
   - Thêm `IoTDataRequest_AllowsNullableTagFieldsWithoutModelBindingFailure` và `MeasurementPointEventCommandHandler_EvaluatesBufferedTransitionsSafely`.
5. **Hoàn thiện Bộ cài đặt máy mới ([`SetupMDMS.ps1`](file:///e:/sources/MDMSv2/Installers/Scripts/SetupMDMS.ps1) & SQL Clean-Install)**:
   - Sửa lỗi cú pháp PowerShell `$url:` tại [`1_Install_Prerequisites.ps1`](file:///e:/sources/MDMSv2/Installers/Scripts/1_Install_Prerequisites.ps1).
   - Bổ sung `EngineType`, `ExcelTemplateBase64`, `ExcelTemplateFileName` (và cột `gcstemporary`/`gcsmonthlydetail`/`periodicenergystatement`) vào [`amr_full.sql`](file:///e:/sources/MDMSv2/Installers/Database/amr_full.sql), gỡ bỏ block `notificationtemplates` trùng và các Event/Stored Procedure dọn bảng EAV cũ (`20261111_phase1_optimize_metering_schema.sql` cũng tự dọn triệt để khi nâng cấp).
   - Chuẩn hóa idempotent cho [`20261031_create_email_config_and_metering_tables.sql`](file:///e:/sources/MDMSv2/Installers/Database/20261031_create_email_config_and_metering_tables.sql) và [`20261109_standardize_decimal_metering_tables.sql`](file:///e:/sources/MDMSv2/Installers/Database/20261109_standardize_decimal_metering_tables.sql) qua `sp_phase3_ensure_column` (tự thêm hoặc chuyển đổi `NewPreviousReading`, `CosPhi`, `PmaxTime`, `ReadingTime`...).
   - Nâng cấp [`SetupMDMS.ps1`](file:///e:/sources/MDMSv2/Installers/Scripts/SetupMDMS.ps1), [`2_Setup_Backend_Service.ps1`](file:///e:/sources/MDMSv2/Installers/Scripts/2_Setup_Backend_Service.ps1), [`4_Verify_System.ps1`](file:///e:/sources/MDMSv2/Installers/Scripts/4_Verify_System.ps1), [`Import_MasterData.ps1`](file:///e:/sources/MDMSv2/Installers/Scripts/Import_MasterData.ps1): chuyển gọi `mysql.exe` sang `$env:MYSQL_PWD` + `-RedirectStandardInput` (tránh lỗi ký tự đặc biệt `.bat`), kiểm tra xác thực MySQL trước khi import, bảo vệ CSDL đã tồn tại trước khi reset, bắt buộc kiểm tra `ExitCode` từng migration `^\d{8}_*.sql`, xác minh đủ 10 bảng lõi / 0 cột `*Unit` dư / partition TimeSeries trước khi tiếp tục, upsert `users` và `systemconfig` không trùng key, và tăng timeout kiểm tra khởi động lần đầu Backend lên 60s (`30 x 2s`).

---

# [DEV UPDATE] PHASE 1 — TỐI ƯU HÓA CSDL ĐO ĐẾM, WIDE-TABLE PARTITION 12 THÁNG & PIPELINE THU THẬP IOT HIỆU NĂNG CAO

> **Tài liệu cập nhật phát triển (Developer Changelog)**  
> **Phiên bản:** v3.2.0-PHASE1-DB-AND-IOT-PIPELINE-OPTIMIZATION  
> **Người thực hiện:** MDMS-Coder (Lead Full-Stack Developer)  
> **Thời gian:** 11/11/2026  

---

### Tóm tắt các nội dung hoàn thành:

1. **Chuyển đổi Kiến trúc CSDL từ 5 Bảng EAV/Hourly sang Wide-Table Snapshot & TimeSeries Partitioned (Zero-Data-Loss)**:
   - **Loại bỏ 5 thực thể & bảng cũ** khỏi tầng Domain / Application / Infrastructure / EF Snapshot:
     `MeasurementPointMonitoring`, `ArchiveDataMeasurementPointMonitoring`, `MeasurementPointMonitoringByDateTime`, `MeasurementPointMonitoringByStatus`, `MeasurementPointMonitoringHourly` (cùng toàn bộ `I*ReadWriteRepository` & `BackgroundJobArchiveDataService`).
   - **Tạo bảng Snapshot trạng thái hiện hành (`MeasurementPointCurrentSnapshots`)** ([`MeasurementPointCurrentSnapshotEntity.cs`](file:///e:/sources/MDMSv2/Backend/Domain/Entities/MeasurementPointCurrentSnapshotEntity.cs), [`MeasurementPointCurrentSnapshotEntityConfiguration.cs`](file:///e:/sources/MDMSv2/Backend/Infrastructure/Databases/MeasurementPointCurrentSnapshotEntityConfiguration.cs)):
     * Mô hình **1 dòng / 1 Điểm đo** với `UNIQUE INDEX (MeasurementPointId)`.
     * Lưu trữ trọn vẹn 41 đại lượng `DECIMAL(18,4)` + `*Unit` + `*Quality` + `*UpdatedAt`, trạng thái kết nối `MatKetNoi`, 10 kênh cảnh báo `CanhBao01..10`, và 24 trường đồng hồ / thời điểm chốt / $P_{max}$ (`smallint`).
   - **Tạo bảng Chuỗi thời gian rộng phân vùng tháng (`MeasurementPointTimeSeries`)** ([`MeasurementPointTimeSeriesEntity.cs`](file:///e:/sources/MDMSv2/Backend/Domain/Entities/MeasurementPointTimeSeriesEntity.cs), [`MeasurementPointTimeSeriesEntityConfiguration.cs`](file:///e:/sources/MDMSv2/Backend/Infrastructure/Databases/MeasurementPointTimeSeriesEntityConfiguration.cs)):
     * Phân vùng `PARTITION BY RANGE (PartitionYm)` theo múi giờ Việt Nam `UTC+7` (`yyyyMM`), khóa chính tổ hợp `(MeasurementPointId, PartitionYm, RecordedAt, Id)`.
     * Thiết lập chính sách **lưu giữ 12 tháng** (`retentionMonths = 12`) và tự động khởi tạo trước **3 tháng tương lai** qua [`PartitionManagementService`](file:///e:/sources/MDMSv2/Backend/Infrastructure/Services/PartitionManagementService.cs) và Hangfire Job `"MaintainTimeSeriesPartitions"` (`0 1 * * *`), thay thế hoàn toàn `"GroupDataPerMeter"` & `"ArchiveDataMeasurementPointMonitoring"`.

2. **Chuẩn hóa Báo cáo Điện năng (`DECIMAL(18,4)` + `SuspectedMask`) & Giữ nguyên cấu trúc Vận hành 30 phút**:
   - **Tạo [`EnergySuspectedFlags`](file:///e:/sources/MDMSv2/Backend/Domain/Enums/EnergySuspectedFlags.cs) & [`MeteringValueHelper`](file:///e:/sources/MDMSv2/Backend/Domain/Helpers/MeteringValueHelper.cs)**:
     * Mã hóa cờ nghi ngờ (`*` / `(Not Topical)`) của 10 kênh điện năng tích lũy thành bitmask 10-bit `SuspectedMask` (bit `0..9`) và cờ tổng `IsSuspected` trên [`MeasurementPointHalfHourEnergyReportEntity`](file:///e:/sources/MDMSv2/Backend/Domain/Entities/MeasurementPointHalfHourEnergyReportEntity.cs) và [`MeasurementPointDailyEnergyReportEntity`](file:///e:/sources/MDMSv2/Backend/Domain/Entities/MeasurementPointDailyEnergyReportEntity.cs), kèm `UNIQUE INDEX (MeasurementPointId, CreatedTime)`.
     * Hỗ trợ parse chuẩn xác đa định dạng dấu thập phân (`.`, `,`, chuỗi `vi-VN`, `*`, `(Not Topical)`), không nhân nhầm dấu `,` đơn thành hàng nghìn.
     * AutoMapper Profiles ([`MeasurementPointHalfHourEnergyReportProfile.cs`](file:///e:/sources/MDMSv2/Backend/Infrastructure/MappingProfiles/MeasurementPointHalfHourEnergyReportProfile.cs), [`MeasurementPointDailyEnergyReportProfile.cs`](file:///e:/sources/MDMSv2/Backend/Infrastructure/MappingProfiles/MeasurementPointDailyEnergyReportProfile.cs)) cùng các trình xuất CSV/Excel/PDF bảo toàn 100% định dạng hiển thị `vi-VN` kèm dấu `*` và đơn vị `kWh`/`kVArh` qua `PaginateWithAscMappedAsync`.
   - **`MeasurementPointHalfHourOperatingReports`**: Giữ nguyên các cột giá trị số hiện hữu (không bổ sung 4 cột `P/Q Max/Min`), **xóa bỏ 9 cột `*Unit` dư thừa** (bổ sung migration idempotent [`20261111_remove_operating_report_unit_columns.sql`](file:///e:/sources/MDMSv2/Installers/Database/20261111_remove_operating_report_unit_columns.sql) và đồng bộ [`20261017_create_operating_parameter_report_table.sql`](file:///e:/sources/MDMSv2/Installers/Database/20261017_create_operating_parameter_report_table.sql), [`20261111_phase1_optimize_metering_schema.sql`](file:///e:/sources/MDMSv2/Installers/Database/20261111_phase1_optimize_metering_schema.sql), `Program.cs`). Đơn vị hiển thị cho API, Export CSV/Excel/PDF và Biểu đồ (Voltage/Load/Power) được resolve thống nhất theo cấu hình `"Liên kết dữ liệu"` (`TagName`) qua [`OperatingReportFallbackHelper.GetConfiguredUnitsAsync`](file:///e:/sources/MDMSv2/Backend/Infrastructure/Helpers/OperatingReportFallbackHelper.cs), bổ sung `UNIQUE INDEX (MeasurementPointId, CreatedTime)` và đồng bộ dữ liệu giờ còn thiếu từ `MeasurementPointMonitoringHourly`.
   - **Events & Alerts**: Bổ sung `ParameterCode VARCHAR(64)` và chuyển `TagNameId` thành `BIGINT NULL` (`ON DELETE SET NULL`) trên [`MeasurementPointsEventEntity`](file:///e:/sources/MDMSv2/Backend/Domain/Entities/MeasurementPointsEventEntity.cs) & [`MeasurementPointsAlertEntity`](file:///e:/sources/MDMSv2/Backend/Domain/Entities/MeasurementPointsAlertEntity.cs).

3. **Tối ưu Pipeline Tiếp nhận IoT & Tầng Truy vấn / Báo cáo / GCS**:
   - **Cache định tuyến $O(1)$ ([`TagRoutingCacheService`](file:///e:/sources/MDMSv2/Backend/Infrastructure/Services/TagRoutingCacheService.cs))**: Lưu ánh xạ `(TagName -> MeasurementPointId, Description, Unit, CustomName)` trong `IMemoryCache` (TTL 15 phút, semaphore lock + version race guard); tự động `Invalidate()` tại toàn bộ handlers Thêm/Sửa/Xóa/Đổi trạng thái/Import điểm đo, TagName và [`MeterReplacementService`](file:///e:/sources/MDMSv2/Backend/Infrastructure/Services/MeterReplacementService.cs).
   - **Micro-Batching & Debounced SignalR ([`BufferedTelemetryIngestionService`](file:///e:/sources/MDMSv2/Backend/Infrastructure/Services/BufferedTelemetryIngestionService.cs))**: Dùng `BoundedChannel` (20.000 items), gom lô 500 items / 500ms ghi SQL tham số hóa (`INSERT ... ON DUPLICATE KEY UPDATE` vào `MeasurementPointCurrentSnapshots`, bulk `INSERT` vào `MeasurementPointTimeSeries`), đồng bộ `_flushGate` cho bộ đệm phát SignalR delta mỗi 2 giây (`ReceiveUpdate`, `ReceiveDetailUpdate`, `ReceiveDetailDateTimeUpdate`), hoàn toàn loại bỏ query `PageSize = 1000` sau mỗi request IoT.
   - **Refactor toàn bộ Repositories & Charts**:
     * [`MeasurementPointMonitoringReadOnlyRepository`](file:///e:/sources/MDMSv2/Backend/Infrastructure/Repositories/ReadOnly/MeasurementPointMonitoringReadOnlyRepository.cs), [`MeasurementPointMonitoringHourlyReadOnlyRepository`](file:///e:/sources/MDMSv2/Backend/Infrastructure/Repositories/ReadOnly/MeasurementPointMonitoringHourlyReadOnlyRepository.cs) (`GetLoadChartAsync`, `GetVoltageChartAsync` trên OperatingReports; `GetPowerChartAsync` SQL `MAX`/`MIN` trên TimeSeries 12 tháng + fallback OperatingReports; `GetConsumptionOutputChartAsync` trên EnergyReports), [`EnergyConsumptionReportReadOnlyRepository`](file:///e:/sources/MDMSv2/Backend/Infrastructure/Repositories/ReadOnly/EnergyConsumptionReportReadOnlyRepository.cs), [`GcsCutOffCycleReadOnlyRepository`](file:///e:/sources/MDMSv2/Backend/Infrastructure/Repositories/ReadOnly/GcsCutOffCycleReadOnlyRepository.cs) (chốt GCS đọc trực tiếp Snapshot 1-query).

4. **Script Migration Zero-Data-Loss & Đóng gói Nâng cấp VM**:
   - Khởi tạo [`20261111_phase1_optimize_metering_schema.sql`](file:///e:/sources/MDMSv2/Installers/Database/20261111_phase1_optimize_metering_schema.sql) (idempotent, backfill toàn bộ bảng mới & báo cáo, kiểm định **Verification Gate** `COUNT`/`SUM` qua `SIGNAL SQLSTATE '45000'` trước khi xóa 5 bảng EAV/Hourly cũ và cột chuỗi cũ).
   - Nâng cấp [`Update_Hotfix.ps1`](file:///e:/sources/MDMSv2/Installers/Scripts/Update_Hotfix.ps1): tự động sao lưu `mysqldump` trước khi migrate, gỡ bỏ lệnh xóa `*` thủ công cũ, dừng nâng cấp ngay khi script SQL lỗi. Đồng bộ truy vấn kiểm tra sau import trong [`Import_AmrData.ps1`](file:///e:/sources/MDMSv2/Installers/Scripts/Import_AmrData.ps1) và [`Import_MasterData.ps1`](file:///e:/sources/MDMSv2/Installers/Scripts/Import_MasterData.ps1).

---

# [DEV UPDATE] KHẮC PHỤC TRIỆT ĐỂ LỖI IMPORT EXCEL ĐIỂM ĐO DO MERGED BANNER VÀ CHUẨN HÓA BREADCRUMB CHU KỲ PHIẾU BÁO

> **Tài liệu cập nhật phát triển (Developer Changelog)**  
> **Phiên bản:** v2.2.6-MEASUREMENT-POINT-EXCEL-IMPORT-AND-NOTICE-BREADCRUMB-CYCLE-FIX  
> **Người thực hiện:** MDMS-Coder (Lead Full-Stack Developer)  
> **Thời gian:** 06/11/2026  

---

### Tóm tắt các nội dung hoàn thành:
1. **Khắc phục triệt để lỗi Import Excel Điểm đo (Lệch cột Trạng thái thành Mã điểm đo, 163 dòng bị lỗi)**:
   - **Nguyên nhân gốc rễ**:
     * Khi người dùng xuất danh sách điểm đo ra file `MDMS_DanhSachDiemDo.xlsx`, dòng 2 là tiêu đề báo cáo `"QUẢN LÝ ĐIỂM ĐO"` được gộp ô qua 7 cột (`mergeCells(2, 1, 2, 7)`).
     * Khi đọc file Excel qua ExcelJS, cả 7 ô của dòng 2 đều mang giá trị `"QUẢN LÝ ĐIỂM ĐO"`.
     * Trong hàm `convertExcelFileToCsvFile` cũ, danh mục `headerKeywords` có chứa từ khóa lẻ `'diem do'`, khiến `"QUẢN LÝ ĐIỂM ĐO"` khớp trên cả 7 cột (`matchCount = 7`). Khi so sánh `>= maxMatchCount`, dòng 2 bị nhận diện nhầm làm dòng Header của bảng!
     * Tương tự ở Backend `MeasurementPointReadWriteRepository.cs`, điều kiện kiểm tra cột cũ có nhánh `(header.Contains("diem do") && !header.Contains("loai"))`, nên dòng banner bị nhận làm Header và cột cuối cùng (cột G - Trạng thái) bị gán làm `colCode`. Dòng tiêu đề thực sự và 162 dòng dữ liệu đều bị đọc cột G thành Mã điểm đo (`"Trạng thái"` và `"Đang sử dụng"`), gây ra đúng 163 dòng báo lỗi `Khách hàng không được để trống khi thêm mới điểm đo!`.
   - **Giải pháp xử lý hai tầng toàn diện (Full-Stack Smart Protection)**:
     * **Frontend ([`excelHelper.ts`](file:///e:/sources/MDMSv2/Frontend/src/infrastructure/utils/excelHelper.ts))**:
       - Thay từ khóa chung `'diem do'` bằng các từ khóa đặc trưng: `'ma diem do'`, `'madiemdo'`, `'pointcode'`, `'point_code'`, `'ten diem do'`, `'tendiemdo'`.
       - Thêm cơ chế phát hiện dòng Merged Banner: Nếu số lượng token độc nhất `uniqueTokens.size <= 2` hoặc chứa chuỗi tiêu đề (`he thong quan ly`, `quan ly diem do`, `quan ly khach hang`, `quan ly so gcs`, `thoi gian xuat`), tự động bỏ qua ngay lập tức, không bao giờ nhận diện làm Header bảng.
       - Chỉ công nhận dòng Header bảng thực sự khi `uniqueTokens.size >= 3` và có ít nhất 2 cột khớp từ khóa chuẩn.
     * **Backend ([`MeasurementPointReadWriteRepository.cs`](file:///e:/sources/MDMSv2/Backend/Infrastructure/Repositories/ReadWrite/MeasurementPointReadWriteRepository.cs))**:
       - Bổ sung bộ lọc loại trừ các dòng banner bị merge (`uniqueHeaders.Count <= 2` hoặc chứa tiêu đề hệ thống).
       - Tinh chỉnh điều kiện ánh xạ cột `colCode`: Bắt buộc phải là `"ma diem do"`, `"pointcode"`, `"point_code"`, `"madiemdo"`, `"code"` hoặc `(header.Contains("diem do") && !header.Contains("loai") && !header.Contains("quan ly") && !header.Contains("he thong") && !header.Contains("ten"))`.
       - Ràng buộc nhận diện Header hợp lệ: Cần ít nhất `colCode != -1` VÀ một cột thuộc tính khác (`colType`, `colCustomer`, `colDesc`, `colEmail` hoặc `colStatus`).
       - Thêm Guard trong vòng lặp đọc dữ liệu: Nếu `code` rơi vào các từ khóa tiêu đề hoặc trạng thái (`"trang thai"`, `"dang su dung"`, `"ngung su dung"`, `"stt"`, `"ma diem do"`,...), tự động bỏ qua ngay.
       - Tự động dọn dẹp các bản ghi rác nếu người dùng đã vô tình lưu trước đó (`"Trạng thái"`, `"Đang sử dụng"`, `"Ngừng sử dụng"`).

2. **Chuẩn hóa Breadcrumb và hiển thị chu kỳ Quản lý phiếu báo**:
   - **Yêu cầu**: Sổ GCS 1 kỳ phải hiển thị đúng chu kỳ tháng: **`"Từ 01/10/2026 đến 31/10/2026"`** (thay vì `"Từ 31/09/2026 đến 31/10/2026"`), và các chu kỳ tháng sau cũng phải hiển thị đúng từ ngày mùng 1 đến ngày cuối tháng.
   - **Nguyên nhân lỗi 31/09 cũ**: Tại `GcsCutOffCycleReadOnlyRepository.cs`, khi tính ngày chốt kỳ trước cho tháng 9, code lấy `DateTime.DaysInMonth(cycle.Year, cycle.Month)` (tháng 10 có 31 ngày) thay vì lấy số ngày của tháng trước (tháng 9 chỉ có 30 ngày), dẫn đến ngày 31/09 không tồn tại trong thực tế.
   - **Hiện thực hóa**:
     * **Backend ([`GcsCutOffCycleReadOnlyRepository.cs`](file:///e:/sources/MDMSv2/Backend/Infrastructure/Repositories/ReadOnly/GcsCutOffCycleReadOnlyRepository.cs))**: Tính chính xác `lastDayOfPrevMonth = DateTime.DaysInMonth(prevYear, prevMonth)`.
     * **Frontend ([`table-detail-gcs.tsx`](file:///e:/sources/MDMSv2/Frontend/src/pages/notice-management/components/table/table-detail-gcs.tsx) & [`notice-detail/index.tsx`](file:///e:/sources/MDMSv2/Frontend/src/pages/notice-management/notice-detail/index.tsx))**:
       - Với Sổ GCS 1 kỳ (hoặc các kỳ tháng): Chu kỳ tính sản lượng điện / phiếu báo luôn chuẩn từ **ngày 01 đầu tháng đến ngày cuối cùng của tháng** (`lastDayOfMonth = new Date(year, month, 0).getDate()`):
         + Tháng 10/2026: **`Từ 01/10/2026 đến 31/10/2026`**
         + Tháng 11/2026: **`Từ 01/11/2026 đến 30/11/2026`**
         + Tháng 02/2026: **`Từ 01/02/2026 đến 28/02/2026`**
       - Với Sổ nhiều kỳ: Kỳ 1 bắt đầu từ ngày 01 đến ngày chốt kỳ 1. Các kỳ sau tự động nối tiếp từ ngày chốt kỳ trước đến ngày chốt kỳ này.

3. **Biên dịch & Đóng gói xác thực**:
   - Frontend: `npm run build:prod` $\rightarrow$ **Thành công 100% (0 errors, 1m 5s)**.
   - Backend: `dotnet publish WebApi win-x64 Release` $\rightarrow$ **Thành công 100% (0 errors)**.

---

# [DEV UPDATE] KHẮC PHỤC LỖI IMPORT ĐIỂM ĐO DO KÝ TỰ TIẾNG VIỆT 'Đ' VÀ LOẠI BỎ THAO TÁC XÓA TRÊN DANH SÁCH KHÁCH HÀNG, ĐIỂM ĐO, SỔ GCS

> **Tài liệu cập nhật phát triển (Developer Changelog)**  
> **Phiên bản:** v2.2.5-MEASUREMENT-POINT-IMPORT-FIX-AND-REMOVE-DELETE-ACTIONS  
> **Người thực hiện:** MDMS-Coder (Lead Full-Stack Developer)  
> **Thời gian:** 06/11/2026  

---

### Tóm tắt các nội dung hoàn thành:
1. **Khắc phục triệt để lỗi "Không tìm thấy cột 'Mã điểm đo' hoặc 'PointCode' trong tiêu đề tệp"**:
   - **Nguyên nhân cốt lõi**: Trong bảng mã Unicode, ký tự `đ` (`U+0111`) là một ký tự độc lập, không phải là tổ hợp của chữ `d` kèm dấu thanh (NonSpacingMark). Do đó hàm chuẩn hóa `Normalize(NormalizationForm.FormD)` không chuyển đổi `đ` thành `d`. Khi chuẩn hóa tiêu đề `"Mã điểm đo"`, chuỗi trở thành `"ma điem đo"`. Chuỗi này không khớp với từ khóa so sánh `"ma diem do"`, khiến chỉ số cột mã điểm đo không được phát hiện.
   - **Giải pháp**:
     * Cập nhật hàm `NormalizeHeaderText` tại [`MeasurementPointReadWriteRepository.cs`](file:///e:/sources/MDMSv2/Backend/Infrastructure/Repositories/ReadWrite/MeasurementPointReadWriteRepository.cs) và [`CustomerReadWriteRepository.cs`](file:///e:/sources/MDMSv2/Backend/Infrastructure/Repositories/ReadWrite/CustomerReadWriteRepository.cs) bổ sung chuyển đổi triệt để ký tự `đ`/`Đ` thành `d`/`D`.
     * Nâng cấp cơ chế Positional Fallback: Khi tiêu đề có cột nhưng chưa gán được cột mã điểm đo, tự động dự phòng lấy cột thứ 2 (cột B sau STT) theo đúng định dạng xuất chuẩn của hệ thống.
2. **Loại bỏ thao tác "Xóa" không sử dụng trên cột Thao tác**:
   - **Quản lý khách hàng ([`client-management/index.tsx`](file:///e:/sources/MDMSv2/Frontend/src/pages/client-management/index.tsx))**: Loại bỏ nút Xóa và quyền PrivateComponent xóa, chỉ giữ lại liên kết Xem chi tiết, điều chỉnh độ rộng cột thành 90px.
   - **Quản lý điểm đo ([`point-management/components/table-point.tsx`](file:///e:/sources/MDMSv2/Frontend/src/pages/point-management/components/table-point.tsx))**: Loại bỏ nút Xóa và quyền PrivateComponent xóa, chỉ giữ lại liên kết Xem chi tiết, điều chỉnh độ rộng cột thành 90px.
   - **Quản lý sổ GCS ([`gcs-management/index.tsx`](file:///e:/sources/MDMSv2/Frontend/src/pages/gcs-management/index.tsx))**: Loại bỏ nút Xóa và quyền PrivateComponent xóa, chỉ giữ lại 2 thao tác nghiệp vụ là "Chi tiết" và "Tra soát", điều chỉnh độ rộng cột thành 170px.
3. **Biên dịch & Đóng gói xác thực**:
   - Frontend: `npm run build:prod` $\rightarrow$ **Thành công (0 errors, 1m 2s)**.
   - Backend: `dotnet publish WebApi win-x64 Release` $\rightarrow$ **Thành công (0 errors)**.

---

# [DEV UPDATE] KHẮC PHỤC LỖI BUILD DÓNG GÓI, CHUẨN HÓA BREADCRUMB CHU KỲ VÀ GIAO DIỆN DANH SÁCH PHIẾU BÁO

> **Tài liệu cập nhật phát triển (Developer Changelog)**  
> **Phiên bản:** v2.2.4-BUILD-FIX-AND-NOTICE-MANAGEMENT-IMPROVEMENTS  
> **Người thực hiện:** MDMS-Coder (Lead Full-Stack Developer)  
> **Thời gian:** 06/11/2026  

---

### Tóm tắt các nội dung hoàn thành:
1. **Khắc phục lỗi biên dịch `build_package.ps1` (Backend .NET 8 WebApi)**:
   - Sửa lỗi `CS0103: The name '_unitOfWork' does not exist in the current context` tại [`MeasurementPointReadWriteRepository.cs`](file:///e:/sources/MDMSv2/Backend/Infrastructure/Repositories/ReadWrite/MeasurementPointReadWriteRepository.cs#L489) bằng phương thức kế thừa `await SaveChangesAsync(cancellationToken)`.
   - Đã kiểm tra biên dịch và đóng gói độc lập `dotnet publish Backend/WebApi/WebApi.csproj -c Release -r win-x64 --self-contained true`: Thành công 100% (0 errors).
2. **Chuẩn hóa Breadcrumb hiển thị chu kỳ chốt sổ phiếu báo**:
   - Khắc phục lỗi hiển thị lệch ngày `"Từ 01/10/2026 đến 30/10/2026"` do hàm cũ `getPreviousDate` tự động trừ 1 ngày của ngày chốt tháng.
   - Nâng cấp hàm `formatCycleDate` tại [`table-detail-gcs.tsx`](file:///e:/sources/MDMSv2/Frontend/src/pages/notice-management/components/table/table-detail-gcs.tsx) và [`notice-detail/index.tsx`](file:///e:/sources/MDMSv2/Frontend/src/pages/notice-management/notice-detail/index.tsx):
     * Ngày bắt đầu: Lấy ngày chốt kỳ trước (nếu là kỳ đầu thì tự động lấy ngày cuối cùng của tháng trước: ngày **30/09/2026**).
     * Ngày kết thúc: Lấy ngày chốt kỳ này (ngày cuối cùng của tháng: ngày **31/10/2026**).
     * Breadcrumb hiển thị chuẩn xác: **`Từ 30/09/2026 đến 31/10/2026`**.
3. **Chuẩn hóa danh sách phiếu báo (`table-notice.tsx`)**:
   - Loại bỏ hoàn toàn cột **Mã khách hàng**.
   - Sắp xếp thứ tự các cột theo đúng yêu cầu:
     1. Checkbox chọn gửi
     2. Mã điểm đo
     3. Tên khách hàng
     4. Email nhận phiếu báo
     5. Trạng thái gửi Email
     6. Thời gian gửi gần nhất
   - Cột **Email nhận phiếu báo**: Toàn bộ email được nối thành chuỗi phân cách bởi dấu `; ` trên **1 dòng duy nhất**, không làm giãn chiều cao hàng của bảng. Bọc trong `<Tooltip>` hiển thị danh sách từng email kèm chi tiết trạng thái gửi riêng biệt (`[Đã gửi]`, `[Thất bại: lý do]`, `[Chờ gửi]`, `[Sai cú pháp]`).
   - Bổ sung Tooltip cho các cột văn bản dài và áp dụng `whitespace-nowrap`, `truncate` đảm bảo mỗi bản ghi chỉ hiển thị đúng trên **1 dòng duy nhất**.

---

# [DEV UPDATE] KHẮC PHỤC TRIỆT ĐỂ LỖI IMPORT EXCEL DANH SÁCH KHÁCH HÀNG & ĐIỂM ĐO VÀ CƠ CHẾ SMART HEADER DETECTION

> **Tài liệu cập nhật phát triển (Developer Changelog)**  
> **Phiên bản:** v2.2.3-SMART-EXCEL-IMPORT-HEADER-DETECTION  
> **Người thực hiện:** MDMS-Coder (Lead Full-Stack Developer)  
> **Thời gian:** 06/11/2026  

---

### Tóm tắt nguyên nhân và giải pháp:
1. **Nguyên nhân gốc rễ**:
   - Trước đây khi kiểm thử, người dùng sử dụng file mẫu chuẩn ("Tải file mẫu") hoặc file CSV thuần, trong đó dòng 1 chính là Header cột (`STT, Mã KH, Tên KH,...`) $\rightarrow$ Quá trình chuyển đổi và đọc CSV diễn ra thành công.
   - Khi người dùng xuất báo cáo trực tiếp từ phần mềm (`MDMS_DanhSachKhachHang.xlsx` / `MDMS_DanhSachDiemDo.xlsx`), mở ra hiệu chỉnh rồi dùng chính file xuất này để Import ngược lại vào hệ thống: File xuất luôn có 4 dòng tiêu đề phía trên (Dòng 1: Banner hệ thống MDMS/AMR, Dòng 2: Tên báo cáo, Dòng 3: Thời gian xuất báo cáo, Dòng 4: Dòng trống), từ Dòng 5 mới là bảng dữ liệu thực sự.
   - Hàm chuyển đổi tệp Excel sang CSV ở Frontend trước đây duyệt dòng từ Dòng 1 đưa vào CSV, khiến Backend CsvReader đọc dòng 1 làm Header (không khớp tên cột), rơi vào cơ chế Positional fallback và insert luôn các dòng Banner/Tiêu đề thành Khách hàng/Điểm đo.
2. **Giải pháp toàn diện đa tầng (Full-Stack Smart Protection)**:
   - **Tầng Frontend (`excelHelper.ts`)**: Nâng cấp hàm `convertExcelFileToCsvFile` với thuật toán **Smart Header Row Detection**. Tự động duyệt qua các dòng đầu tiên để tìm chính xác dòng chứa từ khóa tiêu đề cột chuẩn (`stt`, `ma kh`, `ten kh`, `ma diem do`, `loai diem do`, `email`, `dia chi`,...). Toàn bộ các dòng banner/tiêu đề báo cáo phía trên được tự động lược bỏ, chỉ chuyển đổi từ dòng Header thực sự trở đi. Bỏ qua các dòng trống hoặc dòng lặp lại tiêu đề.
   - **Tầng Backend CsvReader (`CustomerReadWriteRepository.cs` & `MeasurementPointReadWriteRepository.cs`)**:
     * Thêm vòng lặp quét tìm Header thực sự trong file CSV (bỏ qua mọi dòng metadata).
     * Bổ sung bộ lọc Guard trong vòng lặp đọc dữ liệu: Loại trừ ngay lập tức nếu mã bản ghi là các chuỗi tiêu đề (`Mã KH`, `QUẢN LÝ KHÁCH HÀNG`, `Mã điểm đo`, `QUẢN LÝ ĐIỂM ĐO`, `Thời gian xuất...`, `HỆ THỐNG QUẢN LÝ...`).
     * Cơ chế Self-Cleaning: Tự động phát hiện và xóa sạch các bản ghi rác từ bảng `Customer` và `MeasurementPoints` nếu đã lỡ bị insert trước đó khi có yêu cầu import mới.
   - **Tầng Backend Query ReadOnly (`CustomerReadOnlyRepository.cs` & `MeasurementPointReadOnlyRepository.cs`)**:
     * Thêm điều kiện lọc loại trừ mã rác trong tất cả các truy vấn danh sách (`GetListCustomersAsync`, `GetListMeasurementPointsAsync`) và xuất báo cáo, đảm bảo giao diện luôn hiển thị sạch sẽ 100%.
   - **Cung cấp Script SQL Dọn dẹp (`cleanup_garbage_import_records.sql`)**:
     * Cung cấp sẵn script SQL kèm `SET SQL_SAFE_UPDATES = 0;` để người quản trị có thể xóa dứt điểm các dòng rác ngay lập tức trong MySQL Workbench.

---

# [DEV UPDATE] CHUẨN HÓA ĐỊNH DẠNG TÊN TỆP EXCEL TRA SOÁT SỔ GCS VÀ THÊM ĐỒNG BỘ TIỀN TỐ MDMS CHO TẤT CẢ CÁC TỆP XUẤT TỪ PHẦN MỀM

> **Tài liệu cập nhật phát triển (Developer Changelog)**  
> **Phiên bản:** v2.2.2-MDMS-FILENAME-STANDARDIZATION  
> **Người thực hiện:** MDMS-Coder (Lead Full-Stack Developer)  
> **Thời gian:** 06/11/2026  

---

### Tóm tắt các nội dung hoàn thành:
1. **Chuẩn hóa tên tệp Excel xuất từ Tra soát sổ GCS (`check-detail`)**:
   - **Yêu cầu**: Đổi tên tệp thành `"TraSoat_{Sổ GCS}_{Kỳ GCS}"` kèm tiền tố nhận diện phần mềm `MDMS_`.
   - **Hiện thực hóa**:
     * Tại [`Frontend/src/pages/gcs-management/check-detail/index.tsx`](file:///e:/sources/MDMSv2/Frontend/src/pages/gcs-management/check-detail/index.tsx), loại bỏ việc đặt tên cũ bị khuyết kỳ (`trasoat_Số GCS 1 kỳ_`).
     * Tự động trích xuất Tên sổ GCS (`GCSName`), Số kỳ (`record.periodNumber`), Tháng và Năm (`record.month`, `record.year`) để tạo tên tệp chuẩn:  
       `MDMS_TraSoat_{Sổ GCS}_Ky{Số kỳ}_T{Tháng}_{Năm}.xlsx`  
       *(Ví dụ thực tế: `MDMS_TraSoat_Số GCS 1 kỳ_Ky1_T10_2026.xlsx`)*.
2. **Đồng bộ tiền tố `MDMS_` cho toàn bộ các tệp xuất ra từ phần mềm**:
   - **Tầng cốt lõi (`excelHelper.ts` & `dialog-export.tsx`)**:
     * Nâng cấp hàm xuất Excel [`exportToExcelWithChart`](file:///e:/sources/MDMSv2/Frontend/src/infrastructure/utils/excelHelper.ts) và tải tệp mẫu [`downloadExcelTemplate`](file:///e:/sources/MDMSv2/Frontend/src/infrastructure/utils/excelHelper.ts): Tự động phát hiện và bổ sung tiền tố `MDMS_` nếu tệp chưa có, đảm bảo cơ chế bảo vệ kép (dual-protection) áp dụng trên 100% các chức năng xuất Excel của toàn bộ hệ thống.
     * Cập nhật cơ chế tải tệp Native Charts trực tiếp từ máy chủ tại [`dialog-export.tsx`](file:///e:/sources/MDMSv2/Frontend/src/infrastructure/common/components/dialog-export/dialog-export.tsx) để tệp tải về luôn mang tiền tố `MDMS_`.
   - **Đồng bộ hóa trực tiếp trên toàn bộ các trang nghiệp vụ**:
     * Quản lý tài khoản: `MDMS_DanhSachTaiKhoan.xlsx`
     * Quản lý khách hàng: `MDMS_DanhSachKhachHang.xlsx`
     * Quản lý sổ GCS: `MDMS_DanhSachSoGCS.xlsx`
     * Quản lý điểm đo: `MDMS_DanhSachDiemDo.xlsx`
     * Cảnh báo điểm đo: `MDMS_DanhSachCanhBao.xlsx`
     * Sự kiện điểm đo: `MDMS_DanhSachSuKien.xlsx`
     * Thông số tổng hợp: `MDMS_ThongSoTongHop.xlsx`
     * Ghi chỉ số (Sổ GCS tạm): `MDMS_SoGCS_{Tên Sổ}_Ky{Kỳ}_T{Tháng}_{Năm}.xlsx`
     * Báo cáo sản lượng điện: `MDMS_BaoCao_SanLuongDien_{Điểm đo}_{Thời gian}.xlsx`
     * Báo cáo thông số vận hành: `MDMS_BaoCao_ThongSoVanHanh_{Điểm đo}_{Thời gian}.xlsx`
     * Báo cáo chỉ số điện các điểm đo: `MDMS_BaoCao_ChiSoDien_CacDiemDo_{Thời gian}.xlsx`
     * Báo cáo chỉ số điện chốt nửa giờ / ngày / tháng / năm: `MDMS_BaoCao_ChiSoDien_{Điểm đo}_{Thời gian}.xlsx`
     * Báo cáo biểu đồ phụ tải: `MDMS_BieuDoPhuTai_{Loại}_{Khách hàng}_{Điểm đo}.xlsx`
     * Báo cáo tổng sản lượng: `MDMS_BaoCao_TongSanLuong_CacDiemDo_{Thời gian}.xlsx`
   - **Đồng bộ hóa Backend API**:
     * [`GcsController.cs`](file:///e:/sources/MDMSv2/Backend/WebApi/Controllers/GcsController.cs): Cập nhật endpoint xuất file tạm thành `MDMS_SoGCS_Ky{periodNumber}_T{month:D2}_{year}.xlsx`.

---

# [DEV UPDATE] KHẮC PHỤC HIỂN THỊ MÀU ĐỎ/CAM CẢNH BÁO TRÊN BẢNG GHI CHỈ SỐ VÀ LOẠI BỎ CỘT COSPHI (PF) TRÊN DANH SÁCH HIỂN THỊ

> **Tài liệu cập nhật phát triển (Developer Changelog)**  
> **Phiên bản:** v2.2.1-GCS-CLEAN-READINGS-AND-REMOVE-COSPHI-COLUMN  
> **Người thực hiện:** MDMS-Coder (Lead Full-Stack Developer)  
> **Thời gian:** 06/11/2026  

---

### Tóm tắt các nội dung hoàn thành:
1. **Khắc phục triệt để tình trạng một số điểm dữ liệu hiển thị màu đỏ/cam sau khi Import từ Excel**:
   - **Nguyên nhân**:
     * Cột "Sản lượng" (`table-gcs.tsx`) trước đây cài đặt logic tô màu đỏ `text-red-600 font-bold` cho sản lượng âm và màu cam `text-amber-600 font-semibold` cho sản lượng bằng 0 (ở các biểu giá BT hoặc SG). Điều này khiến người dùng hiểu nhầm dữ liệu vừa import bị lỗi/hỏng mặc dù file Excel đã được kiểm tra và xác nhận đúng.
     * Cột "Chỉ số mới" (`table-gcs.tsx`) đối với các điểm đo chưa từng nhận chỉ số chốt (`hasNoReading`) trước đây hiển thị dấu `- *` màu cam `text-amber-600` với dấu hoa thị màu đỏ/cam nổi bật, gây cảm giác báo động.
     * Ở tầng Backend (`GcsTemporaryReadWriteRepository.cs`), khi import Excel nếu bản ghi có chỉ số hợp lệ nhưng người dùng không đổi thuộc tính màu nền ô sang Normal thì trước đó bản ghi vẫn bị kẹt ở trạng thái nghi ngờ (`IsSuspicious = true`).
   - **Giải pháp khắc phục**:
     * **Chuẩn hóa cột Sản lượng**: Loại bỏ hoàn toàn class `text-red-600` và `text-amber-600`. Dữ liệu sản lượng được hiển thị bằng màu chữ trung tính chuẩn mực `text-gray-800 font-medium`.
     * **Chuẩn hóa cột Chỉ số mới**: Với các điểm đo chưa có chỉ số, hiển thị dấu gạch ngang `-` màu xám trung tính `text-gray-400 font-medium`, loại bỏ dấu sao màu cam/đỏ.
     * **Cải tiến Backend Import Excel**: Trong `GcsTemporaryReadWriteRepository.ImportGcsTemporaryExcelAsync`, với các bản ghi vốn bị nghi ngờ, khi người dùng import file Excel có giá trị chỉ số hợp lệ (`parsedNew.HasValue`), hệ thống tự động giải phóng cờ nghi ngờ (`IsSuspicious = false`, `Quanlity = true`, `IsChange = false`), đảm bảo toàn bộ các điểm đo có số liệu trong file Excel đều trở về trạng thái hợp lệ bình thường.
2. **Loại bỏ cột Cosphi (PF) trên danh sách hiển thị**:
   - Xóa bỏ định nghĩa cột `{ title: 'Cos φ (PF)', dataIndex: 'cosPhi', ... }` trên bảng grid của trang **Ghi chỉ số** ([`Frontend/src/pages/gcs-sum/components/table-gcs.tsx`](file:///e:/sources/MDMSv2/Frontend/src/pages/gcs-sum/components/table-gcs.tsx)) và trang **Tra soát chi tiết** sau chốt sổ ([`Frontend/src/pages/gcs-management/components/table-check-detail.tsx`](file:///e:/sources/MDMSv2/Frontend/src/pages/gcs-management/components/table-check-detail.tsx)).
   - Điều chỉnh chiều rộng cuộn ngang của cả 2 bảng về `scroll={{ x: 1200 }}` cho bố cục gọn gàng, thoáng đãng và tối ưu trải nghiệm người dùng.
   - **Lưu ý**: Logic tính toán ngầm và lưu trữ hệ số Cosphi (PF) trung bình kỳ trong database (`GcsTemporary`, `GcsMonthlyDetail`, `PeriodicEnergyStatement`) và trên các mẫu in Thông báo tiền điện / Phiếu báo chỉ số định kỳ PDF vẫn được giữ nguyên vẹn 100%.

---

# [DEV UPDATE] TỐI ƯU GIAO DIỆN IMPORT EXCEL (LOẠI BỎ KHUNG NÉT ĐỨT LƠ LỬNG, ĐỔI TÊN MODAL) VÀ TRIỆT ĐỂ BỎ KÝ HIỆU 'M' KHI NHẬP CHỈ SỐ TỪ EXCEL

> **Tài liệu cập nhật phát triển (Developer Changelog)**  
> **Phiên bản:** v2.2.0-GCS-CLEAN-IMPORT-MODAL-AND-ZERO-MANUAL-MARK  
> **Người thực hiện:** MDMS-Coder (Lead Full-Stack Developer)  
> **Thời gian:** 06/11/2026  

---

### Tóm tắt các nội dung hoàn thành:
1. **Giao diện Nhập chỉ số & xác nhận từ Excel (`dialog-import-excel.tsx`)**:
   - **Cập nhật tiêu đề Modal**: Đổi tên thành chuẩn xác **"Nhập chỉ số & xác nhận từ Excel"**.
   - **Loại bỏ triệt để 2 khung (Frame) nét đứt lơ lửng**:
     * Cấu hình `showUploadList={false}` trên `Upload.Dragger` để chặn Ant Design tự sinh thumbnail item lỗi vỡ khung.
     * Bỏ `className` chứa `border-dashed border-2 border-gray-300` khỏi Dragger để sử dụng border nguyên bản chuẩn xác của Ant Design.
     * Tự xây dựng thẻ hiển thị tệp đã chọn (`fileList`) phẳng, sạch sẽ, gồm icon tệp 📊, tên tệp và nút "Xóa tệp" tiện lợi.
2. **Triệt để loại bỏ ký hiệu 'M' sau khi xác nhận / cập nhật từ file Excel**:
   - **Nguyên nhân**: Khi Import Excel, hệ thống trước đây gán `entity.IsChange = true` cho các dòng có giá trị mới (bao gồm cả điểm nghi ngờ được cập nhật và các điểm đo chưa có dữ liệu công tơ), dẫn đến sau khi chốt sổ tất cả đều bị hiển thị ký hiệu 'M'.
   - **Giải pháp khắc phục**:
     * Trong `GcsTemporaryReadWriteRepository.ImportGcsTemporaryExcelAsync`: Toàn bộ các dòng dữ liệu cập nhật từ file Excel (đã chuyển màu nền sang Normal) được coi là **số liệu chốt chuẩn xác đã được xác nhận**, hệ thống gán **`entity.IsChange = false`** (chỉ gán `true` khi người dùng cố tình nhập thêm dấu `!` hoặc chữ `M`).
     * Cả điểm "Số liệu nghi ngờ" sau khi cập nhật và các điểm "Số liệu đúng" đều mang `IsChange = false`.
     * Khi chốt sổ (`FinalMeterReadingCommandHandler.cs`): Lưu `IsChange = false` $\rightarrow$ Màn hình tra soát `check-detail` hiển thị số liệu chuẩn 100%, **hoàn toàn không còn ký hiệu 'M'**.
3. **Bảo toàn hiển thị số nguyên cho file Excel xuất từ sổ GCS**:
   - Duy trì format `#,#0` làm tròn đến hàng đơn vị và không ghép chuỗi 'M' vào ô số, đảm bảo file Excel xuất ra hiển thị số nguyên thuần túy.

---

# [DEV UPDATE] KHẮC PHỤC KÝ HIỆU 'M' TRÊN CÁC ĐIỂM ĐO KHÔNG SỬA, LOẠI BỎ SỐ THẬP PHÂN TRONG FILE EXCEL TRA SOÁT, TÙY CHỌN BỎ KÝ HIỆU 'M' KHI NHẬP LIỆU VÀ VALIDATION DỮ LIỆU EXCEL CHẶT CHẼ

> **Tài liệu cập nhật phát triển (Developer Changelog)**  
> **Phiên bản:** v2.1.9-GCS-MANUAL-MARK-ACCURACY-EXCEL-VALIDATION  
> **Người thực hiện:** MDMS-Coder (Lead Full-Stack Developer)  
> **Thời gian:** 06/11/2026  

---

### Tóm tắt các nội dung hoàn thành:
1. **Khắc phục triệt để lỗi hiển thị ký hiệu 'M' (Manual edit) với những số liệu không hề được chỉnh sửa (Kyocera, Zeon...)**:
   - **Nguyên nhân gốc rễ**: Khi người dùng xuất Excel rồi Import lại, vòng lặp gán `entity.ModifiedTime = DateTimeOffset.Now;` và `entity.NewCurrentReading = parsedNew.Value` cho tất cả các dòng, khiến `FinalMeterReadingCommandHandler` đánh dấu `IsChange = true` cho toàn bộ các điểm đo dù không hề sửa số liệu công tơ.
   - **Giải pháp**:
     * Trong `GcsTemporaryReadWriteRepository.ImportGcsTemporaryExcelAsync`: Phân biệt rõ điểm đo bình thường (`isNormalUnchanged` khi giá trị không đổi so với `CurrentReading`), giữ nguyên 100%, không gán `NewCurrentReading`, không gán `ModifiedTime`, `IsChange = false`.
     * Khi người dùng chuyển màu nền Normal để xác nhận số liệu chốt của công tơ: Chỉ xóa cờ nghi ngờ (`IsSuspicious = false`, `Quanlity = true`), không coi là hiệu chỉnh thủ công (`IsChange = false`).
     * Trong `FinalMeterReadingCommandHandler.cs`: Gán `IsChange = item.IsChange` (chỉ những bản ghi thực sự có cờ hiệu chỉnh thủ công mới mang `IsChange = true`). Các điểm đo chuẩn như Kyocera và Zeon bảo toàn `IsChange = false` $\rightarrow$ Tuyệt đối không hiển thị icon 'M'.
2. **Loại bỏ phần thập phân (.00) trong file Excel xuất từ sổ GCS (`trasoat_SoGCS_...xlsx`)**:
   - Trong `Backend/Infrastructure/Repositories/ReadOnly/GcsMonthlyDetailReadOnlyRepository.cs` (`ExportCsvGcsMonthlyDetailAsync`):
     * Format `Chỉ số cũ`, `Chỉ số mới`, `Hệ số nhân`, `Sản lượng` làm tròn số nguyên (`Math.Round(val, 0)`).
     * Loại bỏ việc ghép chuỗi `+ "M"` trực tiếp vào ô số của CSV để ô số trong Excel luôn là số nguyên thuần túy, tránh bị hiển thị lỗi `0M`.
   - Trong `Frontend/src/core/application/common/constants.ts` (`GcsCheckDetail` & `GcsSum`): Cấu hình định dạng số nguyên có phân cách hàng nghìn `numFmt: '#,##0'` cho các cột `previousReading`, `currentReading`, `multiplierFactor`, `consumption`.
   - Trong `Frontend/src/infrastructure/utils/excelHelper.ts`: Kiểm tra `isIntegerCol` và `colDef.numFmt === '#,##0'`, parse và làm tròn `Math.round(...)`, không rơi vào fallback `#,##0.00`.
   - Trong `Frontend/src/pages/gcs-management/check-detail/index.tsx` (`dataExport`): Làm tròn số nguyên `Math.round(Number(val))` cho tất cả các trường số.
3. **Cơ chế cho phép người dùng tùy chọn bật / bỏ ký hiệu 'M' khi nhập liệu thủ công**:
   - Trong `Backend`: Thêm trường `IsChange` vào `GcsTemporaryEntity`, `UpdateGcsTemporaryRequest` và bảng cơ sở dữ liệu MySQL (`GcsTemporary`).
   - Trong `UpdateGcsTemporaryCommandHandler.cs`: Nhận `requestItem.IsChange`. Nếu người dùng bỏ cờ M (`IsChange = false`), hệ thống xóa `ModifiedBy`, `ModifiedTime` và lưu `IsChange = false`.
   - Trong `Frontend/src/pages/gcs-sum/index.tsx`: Bổ sung Checkbox `[ ] Đánh dấu đã hiệu chỉnh thủ công (ký hiệu M)` trong modal sửa chỉ số kèm chú thích rõ ràng: *"Bỏ chọn nếu đây là số liệu chốt chuẩn từ công tơ hoặc không muốn gắn ký hiệu 'M' trên báo cáo."*
   - Trong `Frontend/src/pages/gcs-sum/components/table-gcs.tsx`: Chỉ hiển thị icon `EditedIc` khi `record.isChange === true`. Nếu người dùng bỏ chọn 'M', icon biến mất ngay lập tức.
4. **Bổ sung cơ chế kiểm tra (Validation) chặt chẽ khi Import từ file Excel**:
   - Trong `GcsTemporaryReadWriteRepository.cs`: Thêm hàm `ValidateAndParseCell` kiểm tra nghiêm ngặt kiểu dữ liệu:
     * Chặn các ký tự text, chữ cái, chuỗi rác không thể ép kiểu thành số thực/số nguyên hợp lệ (ví dụ `"abc"`, `"error"`, `"12a3"`).
     * Chặn chỉ số âm (`parsed < 0`) vì chỉ số công tơ điện tử không thể là số âm.
     * Khi phát hiện ô sai định dạng, ghi nhận chi tiết: `"Dòng {row}: Ô '{colName}' ({pointCode} - {readingSet}) chứa giá trị không hợp lệ '{str}'. Giá trị phải là số thực hoặc số nguyên."` và bổ sung vào `response.Errors`.
     * Bỏ qua dòng bị lỗi để tránh làm hỏng dữ liệu hệ thống, đồng thời hiển thị chi tiết danh sách lỗi trên modal kết quả Import để người dùng hiệu chỉnh lại file Excel.

---

# [DEV UPDATE] KHÓA CHẶT ĐIỀU KIỆN CHỐT SỔ (CHỈ CHO PHÉP CHỐT KHI 100% HẾT NGHI NGỜ), ĐỒNG BỘ GIAO DIỆN BẢNG TRA SOÁT SAU CHỐT VÀ BẢO TOÀN THỨ TỰ ĐIỂM ĐO

> **Tài liệu cập nhật phát triển (Developer Changelog)**  
> **Phiên bản:** v2.1.8-GCS-STRICT-FINALIZATION-AND-CHECK-DETAIL-ALIGNMENT  
> **Người thực hiện:** MDMS-Coder (Lead Full-Stack Developer)  
> **Thời gian:** 06/11/2026  

---

### Tóm tắt các nội dung hoàn thành:
1. **Khóa chặt điều kiện xác nhận "Chốt sổ" (Strict Finalization Gatekeeper)**:
   - **Frontend (`gcs-sum/index.tsx`)**:
     * Kiểm tra `suspiciousCount`: Nếu còn dù chỉ 1 bản ghi có số liệu nghi ngờ hoặc chưa có chỉ số chốt mới (`suspiciousCount > 0`), nút Chốt sổ hiển thị tooltip cảnh báo `"Còn X số liệu nghi ngờ chưa hiệu chỉnh (Không thể chốt sổ)"`.
     * Khi mở Dialog xác nhận chốt sổ: Vô hiệu hóa hoàn toàn nút "Xác nhận" (`disableSubmitButton={suspiciousCount > 0}`, `isButtonSubmit={suspiciousCount === 0}`), hiển thị thông báo Alert lỗi màu đỏ nghiêm ngặt:
       `"⛔ Không thể xác nhận chốt sổ! Hiện còn X bản ghi có số liệu nghi ngờ hoặc chưa cập nhật chỉ số mới. Người dùng chỉ được xác nhận chốt sổ nếu không còn số liệu nghi ngờ (sau khi đã nhập liệu thủ công hoặc Import file Excel có chuyển màu sang Normal)."`
   - **Backend (`FinalMeterReadingCommandHandler.cs` & `GcsTemporaryReadWriteRepository.cs`)**:
     * Trong `CheckExistGcsTemporaryAsync`: Bổ sung kiểm tra `x.CurrentReading == null && x.NewCurrentReading == null`. Chặn tuyệt đối từ tầng API nếu còn bất kỳ bản ghi nào chưa có chỉ số chốt mới hoặc còn nghi ngờ chưa xử lý.
2. **Đồng bộ hóa giao diện bảng Tra soát chi tiết sau chốt sổ (`table-check-detail.tsx`)**:
   - **Loại bỏ hoàn toàn cột "Mô tả"** rườm rà gây chật bảng.
   - **Đồng bộ giao diện theo chuẩn của trang Ghi chỉ số**:
     * Cột `Mã điểm đo`: `width: 200`, `truncate whitespace-nowrap`, có Tooltip đầy đủ khi rê chuột.
     * Cột `Tên khách hàng`: `width: 400`, `truncate whitespace-nowrap` (không bị xuống hàng 3 dòng), có Tooltip đầy đủ.
     * Cột `Bộ chỉ số`, `Chỉ số cũ`, `Chỉ số mới`, `Hệ số nhân`, `Sản lượng`, `Cos φ (PF)`: Đồng bộ width và định dạng `whitespace-nowrap`.
     * Thêm thanh cuộn ngang `scroll={{ x: 1320 }}` giúp bảng hiển thị rộng rãi, chuẩn xác, không bị co ép cột.
3. **Bảo toàn thứ tự hiển thị của danh sách điểm đo và bộ chỉ số sau khi chốt sổ**:
   - Trong `FinalMeterReadingCommandHandler.cs`: Sắp xếp danh sách `gcsTemporary` đồng nhất với màn hình Ghi chỉ số và sinh Snowflake ID tăng dần ngược từ cuối danh sách lên đầu, đảm bảo khi `GetListGcsMonthlyDetailWithPaginationAsync` truy vấn theo `Id DESC`, các điểm đo và bộ chỉ số xuất hiện theo đúng 100% thứ tự ban đầu.
   - Trong `check-detail/index.tsx`: Đổi mặc định phân trang thành **24 dòng/trang** (tương ứng 4 điểm đo nguyên vẹn, mỗi điểm đo 6 bộ chỉ số), hỗ trợ tùy chọn `24/48/96/192/page` và khắc phục lỗi gọi sai hàm state phân trang `setGetGCSCutOffCycleRequest` $\rightarrow$ `setGCSMonthlyDetailRequest`.

---

# [DEV UPDATE] KHẮC PHỤC LỖI GHI ĐÈ SAU IMPORT EXCEL GCS, TỰ ĐỘNG TÍNH TOÁN LẠI SẢN LƯỢNG & HỆ SỐ COSPHI (PF) TRUNG BÌNH VÀ LƯU ĐẦY ĐỦ THÔNG TIN CHỐT SỔ XUẤT PHIẾU BÁO

> **Tài liệu cập nhật phát triển (Developer Changelog)**  
> **Phiên bản:** v2.1.7-GCS-EXCEL-IMPORT-COSPHI-FINALIZE  
> **Người thực hiện:** MDMS-Coder (Lead Full-Stack Developer)  
> **Thời gian:** 06/11/2026  

---

### Tóm tắt các nội dung hoàn thành:
1. **Khắc phục triệt để lỗi mất dữ liệu vừa cập nhật sau khi Import file Excel (`SoGCS_...xlsx`)**:
   - **Nguyên nhân gốc rễ**:
     * Tại Frontend (`Frontend/src/pages/gcs-sum/index.tsx`), biến `refreshState` mặc định khởi tạo là `true` và trong callback `onSuccess` của `DialogImportExcel` lại gán `setRefreshState(true)`. Ngay sau khi API Import Excel lưu thành công 947 bản ghi vào bảng `GcsTemporary`, giao diện gọi lại `GetDataLatchResponseAsync` với `isRefresh: true`, khiến Backend đọc lại bảng IoT thô (nơi các điểm đo chưa có dữ liệu chốt) và **ghi đè xóa sạch toàn bộ dữ liệu vừa Import** (`NewCurrentReading = null`, `CurrentReading = null`, `IsSuspicious = true`, hiển thị lại `- *`).
   - **Giải pháp khắc phục**:
     * Tại `Frontend/src/pages/gcs-sum/index.tsx`: Đổi mặc định `refreshState = false`, tự động reset `setRefreshState(false)` ngay sau khi thực hiện thao tác "Làm mới" thủ công, và trong `DialogImportExcel.onSuccess` đặt `setRefreshState(false)` (đồng thời gỡ bộ lọc `"Số liệu nghi ngờ"` nếu đang bật để hiển thị ngay các bản ghi vừa được xác nhận).
     * Tại Backend (`GcsCutOffCycleReadOnlyRepository.cs`): Bảo vệ các bản ghi đã được người dùng nhập tay hoặc xác nhận qua Excel (`hasUserConfirmedEdit = NewCurrentReading != null || NewPreviousReading != null`) không bị ghi đè về `null` khi công tơ chưa gửi chỉ số chốt mới khác biệt.
     * Tại `GcsTemporaryReadWriteRepository.ImportGcsTemporaryExcelAsync`: Đọc trực tiếp kiểu số nguyên bản từ EPPlus (`double, int, long, decimal, float`) kết hợp chuỗi, gán `Quanlity = true`, `IsSuspicious = false`, `ModifiedTime = DateTimeOffset.Now`.
2. **Tự động tính toán lại Sản lượng và Hệ số $\cos\varphi$ (PF) trung bình ngay sau khi cập nhật số liệu**:
   - Bổ sung hàm tính toán chuẩn xác `CalculateAverageCosPhi` và `RecalculatePointConsumptionAndCosPhi` tại `GcsTemporaryReadWriteRepository.cs`:
     $$\cos\varphi = \frac{A_P}{\sqrt{A_P^2 + A_Q^2}}$$
     (trong đó $A_P$ là sản lượng hữu công tổng `SG` hoặc tổng `BT + CD + TD`, $A_Q$ là sản lượng vô công `VC`).
   - Tự động tính toán lại toàn bộ `Consumption` và `CosPhi` ở cả 3 luồng:
     1. Sau khi Import file Excel (`ImportGcsTemporaryExcelAsync`).
     2. Sau khi sửa trực tiếp Chỉ số cũ / Chỉ số mới trên giao diện Web (`UpdateGcsTemporaryCommandHandler.cs` & `gcs-sum/index.tsx`).
     3. Khi tổng hợp dữ liệu kỳ chốt (`GcsCutOffCycleReadOnlyRepository.cs`).
   - Bổ sung cột **`Cos φ (PF)`** ngay sau cột **`Sản lượng`** trên bảng Ghi chỉ số (`table-gcs.tsx`) và bảng Chi tiết sổ đã chốt (`table-check-detail.tsx`).
3. **Lưu trữ đầy đủ toàn bộ thông tin khi xác nhận "Chốt sổ" (`FinalMeterReadingCommandHandler.cs`) để xuất Phiếu báo chính thức**:
   - Bổ sung các trường dữ liệu vào Entity, DTO và tự động migration CSDL (`EnsureColumnsExistAsync` & `Program.cs`):
     * `GcsTemporaryEntity` & `GetDataLatchResponse`: `CosPhi`, `PmaxTime`.
     * `GcsMonthlyDetailEntity` & `GcsMonthlyDetailDto`: `CosPhi`, `ReadingTime`, `PmaxTime`.
     * `PeriodicEnergyStatementEntity` & `PeriodicEnergyStatementDto`: `CosPhi`, `PmaxTime`, `PreviousPmaxTime`, `BillingTimestamp`.
   - Khắc phục lỗi mất chỉ số sửa tay/Import khi bấm **"Chốt sổ"** trong `FinalMeterReadingCommandHandler.cs`: Luôn ưu tiên lấy `NewCurrentReading ?? CurrentReading ?? 0` và `NewPreviousReading ?? PreviousReading ?? 0` cho mọi bộ chỉ số (`BT, CD, TD, SG, VC, Pmax`), lưu đầy đủ `Consumption`, `CosPhi`, `BillingTimestamp` (thời điểm chốt), `Pmax`, `PmaxTime` (thời điểm $P_{max}$) vào `GcsMonthlyDetailEntity` và `PeriodicEnergyStatementEntity`.
   - Khắc phục lỗi ghi đè `Pmax = 0` trong `PeriodicEnergyStatementReadWriteRepository.GetListWithFormattedPMaxByIdsAsync`.
   - Cập nhật `StatementPdfGenerator.cs`, `EmailSenderService.cs` và `form-pdf/index.tsx` để hiển thị đầy đủ chỉ số vô công (`PreviousVc, Vc, DiffVc, ConsumtionVc`), `CosPhi`, `Pmax`, `PmaxTime`, `BillingTimestamp` trên Phiếu báo khách hàng (Excel, PDF và Email).

---

# [DEV UPDATE] TRIỂN KHAI CƠ CHẾ SMART UPSERT (STATE-AWARE DEDUPLICATION) CHO CẢ 3 ENDPOINT IOT CLIENT VÀ BẢO VỆ DỮ LIỆU CHỐT TRONG HANGFIRE JOB

> **Tài liệu cập nhật phát triển (Developer Changelog)**  
> **Phiên bản:** v2.1.6-IOT-SMART-UPSERT  
> **Người thực hiện:** MDMS-Coder (Lead Full-Stack Developer)  
> **Thời gian:** 04/11/2026  

---

### Tóm tắt các nội dung hoàn thành:
1. **Endpoint 1 — `MeasurementPointMonitoring` (Chu kỳ 5 phút - Thông số đo lường & Chỉ số)**:
   - Phân tách rõ 2 nhóm thông số tại `MeasurementPointMonitoringCommandHandler.cs`:
     * **Nhóm thông số biến thiên liên tục** ($U, I, P, Q, \cos\varphi, f$, Chỉ số tích lũy chiều giao/nhận): Giữ nguyên cơ chế `INSERT` bản ghi lịch sử mới mỗi chu kỳ 5 phút để phục vụ biểu đồ phụ tải và báo cáo thông số vận hành.
     * **Nhóm 12 thông số đóng băng theo kỳ (`FrozenMonitoringTags`)**: Bao gồm Bộ chỉ số chốt chiều giao (`BT, CD, TD, AP_TONG, AQ_TONG`), Bộ chỉ số chốt chiều nhận (`BT, CD, TD, AP_TONG, AQ_TONG`), `PMAX_KY_TRUOC` và `PMAX_KY_HIEN_TAI`.
   - **Cơ chế Smart Upsert**:
     * Truy vấn bản ghi mới nhất của nhóm Frozen Tags theo danh sách điểm đo (`GetLatestFrozenMonitoringRecordsAsync`).
     * Khi nhận gói tin mới: So sánh giá trị (`AreValuesEqual`, sai số $< 10^{-6}$). Nếu giá trị không thay đổi so với bản ghi hiện hành trong CSDL $\rightarrow$ **Chỉ `UPDATE` tại chỗ** `GroupId = newGroupId`, `Quanlity` và `TagNameId` trên bản ghi hiện có, **không tạo dòng mới**.
     * Chỉ thực hiện `INSERT` dòng mới khi chưa có dữ liệu hoặc khi giá trị thay đổi (bước sang kỳ chốt mới hoặc $P_{max}$ kỳ hiện tại lập đỉnh mới).
     * Đồng bộ `GroupId = newGroupId` cho các Frozen Tags thuộc cùng điểm đo có dữ liệu trong gói tin, đảm bảo màn hình Chi tiết điểm đo (`Where(x => x.GroupId == latestGroupId)`) luôn lấy đầy đủ 100% thông số mà chỉ tốn 1 dòng duy nhất/tag/kỳ.
     * Loại bỏ lỗi cũ chèn dòng rỗng (`Value = null, TagNameDescription = null`) khi điểm đo không xuất hiện trong gói tin.
2. **Endpoint 2 — `MeasurementPointMonitoringByDateTime` (Chu kỳ 5 phút - 24 Tag thời gian)**:
   - Triển khai **Cluster-aware Smart Upsert** tại `MeasurementPointMonitoringByDateTimeCommandHandler.cs` cho 4 cụm thời gian (mỗi cụm 6 tag `Nam, Thang, Ngay, Gio, Phut, Giay`):
     * **Cụm 1 — Đồng hồ hiện tại công tơ (`CurrentClockTags` - `...HienTai`)**: Luôn `UPDATE` tại chỗ trên bản ghi hiện hành của từng tag (chỉ giữ 1 bản ghi/tag/điểm đo, không lưu lịch sử đồng hồ đếm giây).
     * **Cụm 2, 3, 4 — Thời điểm chốt gần nhất (`CutoffDateTimeTags` - `...ChotGanNhat`), Thời điểm $P_{max}$ kỳ hiện tại (`PmaxCurrDateTimeTags` - `...PmaxKyHienTai`), Thời điểm $P_{max}$ kỳ trước (`PmaxPrevDateTimeTags` - `...PmaxKyTruoc`)**: Kiểm tra sự thay đổi theo **từng cụm 6 tag**. Nếu cả 6 thành phần (`Năm-Tháng-Ngày Giờ:Phút:Giây`) trong cụm không đổi $\rightarrow$ `UPDATE` tại chỗ `GroupId` và `Quanlity`. Nếu bất kỳ thành phần nào trong cụm thay đổi $\rightarrow$ `INSERT` bộ 6 bản ghi mới của cụm đó cho kỳ mới.
   - Bổ sung đầy đủ 12 thuộc tính thời điểm $P_{max}$ kỳ hiện tại và kỳ trước vào `MeasurementPointMonitoringByDateTimeDto.cs` để đẩy realtime qua SignalR.
   - **Khắc phục lỗi Hangfire Job `GroupDataPerMeter`** trong `MeasurementPointMonitoringByDateTimeReadWriteRepository.cs`: Trước đây xóa toàn bộ bản ghi `CreatedTime <= DateTime.UtcNow.AddHours(-1)`, gây nguy cơ mất sạch dữ liệu thời điểm chốt nếu Kepware tạm ngưng gửi dữ liệu. Nay chỉ dọn các bản ghi rỗng hoặc bản ghi đồng hồ hiện tại cũ dư thừa, bảo toàn tuyệt đối bản ghi mới nhất và lịch sử thời điểm chốt / $P_{max}$.
3. **Endpoint 3 — `MeasurementPointAlert` / Status (Chu kỳ 1 phút - Trạng thái & Cảnh báo)**:
   - Áp dụng **Smart Upsert** cho bảng trạng thái hiện hành `MeasurementPointMonitoringByStatusEntity` tại `MeasurementPointEventCommandHandler.cs`:
     * Truy vấn bản ghi trạng thái mới nhất của từng tag theo điểm đo (`GetLatestStatusRecordsAsync`).
     * Nếu đã có bản ghi $\rightarrow$ `UPDATE` tại chỗ `Status`, `Quanlity`, `GroupId`, `TagNameId` (giảm từ 43,200 dòng/tháng/tag xuống còn duy nhất **1 dòng/tag**).
     * Bảng lịch sử sự kiện (`MeasurementPointsEventEntity`) và cảnh báo (`MeasurementPointsAlertEntity`) giữ nguyên cơ chế chỉ `INSERT` khi phát hiện đảo chiều trạng thái (`NORMAL` $\leftrightarrow$ `ALARM`).
     * Chuẩn hóa bộ đọc JSON (`TryGetBooleanFromJsonElement`, `ParseQuality`) hỗ trợ đồng thời kiểu `bool`, số (`0`/`1`) và chuỗi (`"0"`, `"1"`, `"NORMAL"`, `"ALARM"`).

---

# [DEV UPDATE] LOẠI BỎ HOÀN TOÀN REST POLLING KEPWARE SERVER, CHỈ DÙNG DỮ LIỆU TỪ IOT REST CLIENT VÀ CHUẨN HÓA TIÊU CHÍ ĐÁNH GIÁ SỐ LIỆU ĐÚNG GCS

> **Tài liệu cập nhật phát triển (Developer Changelog)**  
> **Phiên bản:** v2.1.5-GCS-REST-REMOVAL-AND-CLIENT-ALIGNMENT  
> **Người thực hiện:** MDMS-Coder (Lead Full-Stack Developer)  
> **Thời gian:** 03/11/2026  

---

### Tóm tắt các nội dung hoàn thành:
1. **Loại bỏ 100% cơ chế đọc/polling từ Kepware IoT REST API Server**:
   - Loại bỏ hoàn toàn logic gọi REST API chủ động (`BaseUrlGetDataFromServerIoT`, `CallApiAsync`, vòng lặp batch polling, `ReadResultWrapper`, `ReadResult`, `ValueConverter`) trong `GcsCutOffCycleReadOnlyRepository.cs`.
   - Đúng theo thống nhất kiến trúc: Hệ thống không chủ động gửi HTTP GET request sang Kepware IoT Gateway Server.
2. **Chỉ sử dụng số liệu từ Kepware IoT REST API Client gửi đến**:
   - Nguồn dữ liệu duy nhất phục vụ ghi chỉ số là dữ liệu do Kepware IoT Client tự động gửi đẩy vào hệ thống qua API endpoint và lưu trữ trong CSDL:
     * **Chỉ số chốt chiều giao**: Truy vấn trực tiếp từ bảng `MeasurementPointMonitoringEntity` cho 6 bộ chỉ số: `CHI_SO_CHOT_DIEN_NANG_AP_GIO_BT` (BT), `CHI_SO_CHOT_DIEN_NANG_AP_GIO_CD` (CD), `CHI_SO_CHOT_DIEN_NANG_AP_GIO_TD` (TD), `CHI_SO_CHOT_DIEN_NANG_AP_TONG` (SG), `CHI_SO_CHOT_DIEN_NANG_AQ_TONG` (VC), `PMAX_KY_TRUOC` (Pmax).
     * **Thời điểm chốt công tơ**: Truy vấn từ bảng `MeasurementPointMonitoringByDateTimeEntity` cho 6 tag: `NamChotGanNhat`, `ThangChotGanNhat`, `NgayChotGanNhat`, `GioChotGanNhat`, `PhutChotGanNhat`, `GiayChotGanNhat`.
3. **Quy tắc khi dữ liệu chỉ số chốt không có (chưa bao giờ được cập nhật)**:
   - Nếu điểm đo chưa từng nhận được dữ liệu chốt (hoặc giá trị rỗng/null):
     * Cột "Chỉ số mới": Hiển thị ký hiệu `"-"` (kèm cờ cảnh báo nghi ngờ `*` màu cam trên Web và nền hồng nhạt `#FFC7CE` trong Excel).
     * Cột "Sản lượng": Hiển thị `"-"`.
     * Gắn cờ nghi ngờ: `IsSuspicious = true` với lý do: *"Chưa có chỉ số chốt từ công tơ (chưa từng được cập nhật)"*.
     * File Excel xuất ra: Ô chỉ số mới hiển thị `"-"`, tô nền hồng nhạt `#FFC7CE`, có comment hướng dẫn để người dùng kiểm tra hoặc sửa tay.
4. **Quy tắc kiểm tra các điều kiện đảm bảo số liệu đúng khi có dữ liệu**:
   - Khi có dữ liệu chỉ số chốt, hệ thống kiểm tra toàn diện các điều kiện đã thống nhất:
     * **Tín hiệu đo đếm**: `Quanlity` không bị `false` (không mất kết nối, tín hiệu chuẩn).
     * **Kỳ chốt hợp lệ**: Thời điểm chốt thực tế của công tơ nằm trong khoảng giữa kỳ chốt ($T_{curr} - 2\text{d} \le T_{chot} \le T_{next}$).
     * **Cập nhật kỳ mới**: Thời điểm chốt phải khác thời điểm chốt của kỳ trước (tránh trường hợp công tơ chưa chốt chu kỳ mới).
     * **Sản lượng tiêu thụ**:
       - Sản lượng không âm: $Consumption \ge 0$ (chỉ số mới $\ge$ chỉ số cũ).
       - Sản lượng bằng 0 không bất thường: Bộ chỉ số BT/SG không bằng 0 nếu kỳ trước tiêu thụ $> 100\text{ kWh}$.
       - Sản lượng không biến động đột biến: Không giảm $> 80\%$ ($< 0.2$ lần) hoặc tăng $> 300\%$ ($> 3.0$ lần) so với kỳ trước nếu kỳ trước tiêu thụ $\ge 100\text{ kWh}$.
   - Thỏa mãn toàn bộ các điều kiện trên $\rightarrow$ Đạt chuẩn **Số liệu đúng** (Nền trắng, Style Normal, không có cờ cảnh báo).
5. **Tối ưu hóa Auto-Heal**:
   - Hiệu chỉnh phương thức tự động chữa cháy trong `GcsTemporaryReadWriteRepository` chỉ gỡ cờ cho các bản ghi mang lý do cũ `lệch so với kỳ chốt (> 1 ngày)`, không xóa nhầm các cảnh báo hợp lệ theo tiêu chí mới.

---

# [DEV UPDATE] KHẮC PHỤC TRUY VẤN DỮ LIỆU GHI CHỈ SỐ: LẤY CHÍNH XÁC TỪ BỘ CHỈ SỐ CHỐT CHIỀU GIAO VÀ THỜI ĐIỂM CHỐT CÔNG TƠ

> **Tài liệu cập nhật phát triển (Developer Changelog)**  
> **Phiên bản:** v2.1.4-GCS-CUTOFF-DATA-FIX  
> **Người thực hiện:** MDMS-Coder (Lead Full-Stack Developer)  
> **Thời gian:** 03/11/2026  

---

### Tóm tắt các nội dung hoàn thành:
1. **Khắc phục lỗi lấy nhầm bộ chỉ số tích lũy sang bộ chỉ số chốt chiều giao**:
   - **Nguyên nhân gốc rễ**: Tại `GcsCutOffCycleReadOnlyRepository.cs`, câu truy vấn CSDL trước đó lọc `relevantDescriptions` (`CHI_SO_CHOT_...`) nhầm trên bảng `MeasurementPointMonitoringByDateTimeEntity` (bảng này chỉ lưu trữ các tag ngày giờ của công tơ). Do đó, kết quả truy vấn CSDL luôn bị rỗng (empty), dẫn đến hệ thống buộc phải fallback qua cơ chế polling IoT Gateway với nguy cơ bị lệch tag hoặc lấy nhầm số liệu tích lũy thời gian thực.
   - **Giải pháp xử lý**:
     * Chuyển truy vấn chỉ số đo đếm sang đúng bảng **`MeasurementPointMonitoringEntity`** (khóa ngoại `MeasurementPointsId`), lọc chính xác theo danh sách tag chốt chiều giao: `CHI_SO_CHOT_DIEN_NANG_AP_GIO_BT`, `CHI_SO_CHOT_DIEN_NANG_AP_GIO_CD`, `CHI_SO_CHOT_DIEN_NANG_AP_GIO_TD`, `CHI_SO_CHOT_DIEN_NANG_AP_TONG`, `CHI_SO_CHOT_DIEN_NANG_AQ_TONG`, và `PMAX_KY_TRUOC`.
     * Đảm bảo giá trị hiển thị và tính toán sản lượng trong quá trình GCS (ví dụ với điểm đo `KYOCERA 1`) lấy chính xác giá trị chốt: `14,540,265` (BT), `14,540,266` (CD), `14,540,267` (TD), `14,540,270` (SG), `14,540,264` (VC) thay vì giá trị tích lũy tức thời `14,540,275`, `14,540,276`, ...
2. **Chuẩn hóa thời điểm chốt (`ReadingTime`) theo đúng chu kỳ công tơ**:
   - Truy vấn bổ sung 6 tag thời gian chốt từ `MeasurementPointMonitoringByDateTimeEntity`: `NamChotGanNhat`, `ThangChotGanNhat`, `NgayChotGanNhat`, `GioChotGanNhat`, `PhutChotGanNhat`, `GiayChotGanNhat`.
   - Ghép thành thời điểm chốt chuẩn mực của công tơ (ví dụ `01/11/2026 00:01:05`) để gán cho trường `ReadingTime`, thay vì sử dụng `CreatedTime` của bản ghi CSDL.
3. **Thứ tự ưu tiên dữ liệu**:
   - Ưu tiên 1: Lấy trực tiếp từ CSDL nội bộ (`MeasurementPointMonitoringEntity`), đảm bảo tốc độ tức thì và dữ liệu đã được đồng bộ chuẩn xác.
   - Ưu tiên 2: Fallback sang REST API IoT Gateway polling chỉ khi CSDL nội bộ chưa có dữ liệu.

---

# [DEV UPDATE] HIỆU CHỈNH TOÀN DIỆN UI/UX TRANG GHI CHỈ SỐ GCS VÀ TỐI ƯU HÓA MẪU EXCEL 5 CỘT TINH GỌN (LỌC THEO MÀU SẮC BAD / NORMAL)

> **Tài liệu cập nhật phát triển (Developer Changelog)**  
> **Phiên bản:** v2.1.3-GCS-UX-REFINEMENT  
> **Người thực hiện:** MDMS-Coder (Lead Full-Stack Developer)  
> **Thời gian:** 03/11/2026  

---

### Tóm tắt các nội dung hoàn thành:
1. **Cân chỉnh độ rộng cột bảng ghi chỉ số (Mục 1 mới)**:
   - Tăng độ rộng cột **Tên khách hàng** lên 400px (tối đa 390px nội dung), font medium, giúp tên doanh nghiệp dài hiển thị trọn vẹn, không bị cắt ngắn sớm.
   - Cân chỉnh tỷ lệ các cột còn lại: Mã điểm đo (200px), Bộ chỉ số (90px), Chỉ số cũ (130px), Chỉ số mới (140px), Hệ số nhân (100px), Sản lượng (150px).
   - Thiết lập cuộn ngang `scroll={{ x: 1210 }}` và phân nhóm viền mờ sau mỗi 6 bộ chỉ số (`(index + 1) % 6 === 0`) tương ứng với mỗi điểm đo.
2. **Nâng cấp phân trang thành "24/48/96/192/page" (Mục 2 mới)**:
   - Chuyển kích thước trang mặc định thành **24/page** (tương ứng chính xác 4 điểm đo $\times$ 6 bộ chỉ số).
   - Thiết lập bộ chọn `pageSizeOptions = ['24', '48', '96', '192']` đồng nhất với báo cáo *Thông số vận hành*.
3. **Chuẩn hóa File xuất & nhập Excel 5 cột (Mục 3 mới)**:
   - **Tên file xuất chuẩn**: Đổi thành `SoGCS_{Kỳ GCS}.xlsx` (ví dụ `SoGCS_Ky1_T10_2026.xlsx`), áp dụng đồng bộ ở cả Backend API và Frontend hook.
   - **Thứ tự sắp xếp**: Sắp xếp danh sách theo thời gian khởi tạo điểm đo (`MeasurementPoint.CreatedTime`), gom đủ 6 bộ chỉ số của từng điểm đo liên tục.
   - **Định dạng số liệu nghi ngờ & Quy tắc màu sắc**:
     * **Không dùng dấu nháy đơn**: Số liệu nghi ngờ xuất ra là giá trị số chuẩn (`#,##0`), người dùng có thể thực hiện công thức tính toán trực tiếp trong Excel.
     * **Font chữ & Màu nền**: Chữ giữ nguyên format bình thường (màu đen / tự động như số liệu đúng), chỉ tô **màu nền hồng nhạt** (`#FFC7CE`).
     * Số liệu đúng / đã xác nhận: Nền trắng (`#FFFFFF` / style Normal).
     * Số liệu chưa có từ REST API: Hiển thị `-` với nền hồng nhạt.
   - **Xác nhận số liệu sau khi sửa**: Sau khi kiểm tra hoặc sửa tay chỉ số, người dùng chỉ cần **chuyển màu nền ô về không màu / màu trắng (Normal)** là hệ thống tự động xác nhận số liệu khi import, tự động tính toán lại sản lượng và gỡ cờ nghi ngờ.
4. **Các tối ưu đã hoàn thiện trước đó**:
   - Breadcrumb chuẩn: `Ghi chỉ số > [Tên sổ] - Kỳ [Kỳ chốt] Tháng [Tháng/Năm]`.
   - Loại bỏ các nút lọc nhanh trùng lặp và cột "Mô tả".
   - Ký hiệu chỉ số chốt chưa cập nhật từ REST API hiển thị rõ `-*` (dấu `*` màu cam cảnh báo).
   - Bổ sung bộ tiêu chí đánh giá số liệu đúng (kiểm tra cập nhật thời điểm chốt mới so với chu kỳ trước, khoảng dao động sản lượng hợp lý).

---

# [DEV UPDATE] NÂNG CẤP QUY TRÌNH GHI CHỈ SỐ GCS: ĐIỀU CHỈNH KHOẢNG THỜI GIAN HỢP LỆ, ĐỒNG NHẤT ICON THANH CÔNG CỤ VÀ BẢNG KÊ EXCEL HIỆU CHỈNH 2 CHIỀU

> **Tài liệu cập nhật phát triển (Developer Changelog)**  
> **Phiên bản:** v2.1.2-GCS-LATCH  
> **Người thực hiện:** MDMS-Coder (Lead Full-Stack Developer)  
> **Thời gian:** 03/11/2026  

---

### Tóm tắt các nội dung hoàn thành:
1. **Khắc phục triệt để cảnh báo sai thời điểm chốt (Requirement 1)**:
   - Quy tắc nghiệp vụ mới: Thời điểm chốt công tơ nằm trong khoảng giữa lần chốt hiện tại ($T_{curr} - 2\text{d}$) và lần chốt kế tiếp ($T_{next}$) được coi là **Số liệu đúng**.
   - Bổ sung cơ chế tự động khắc phục (*Auto-Heal*) trong `GcsTemporaryReadWriteRepository`: tự động gỡ bỏ cờ nghi ngờ cho toàn bộ 972 bản ghi bị đánh dấu sai trước đó mà người dùng không cần thao tác tải lại thủ công.
2. **Đồng nhất giao diện và Icon thanh công cụ (Requirement 2)**:
   - Thay thế các nút màu xanh / đỏ rời rạc và Dropdown phức tạp bằng bộ nút chuẩn dùng chung `classButton='text-button-variant'`:
     * Nút **"Xuất file"**: Icon `<FiShare size={20} />`, tooltip `'Xuất file'` (tải trực tiếp file Excel toàn bộ số liệu).
     * Nút **"Nhập file"**: Icon `<FiDownload size={20} />`, tooltip `'Nhập file'` (mở hộp thoại nhập file Excel).
   - Hoàn toàn đồng nhất với các trang Quản lý điểm đo (`point-management`) và Khách hàng (`client-management`).
3. **Xuất toàn bộ dữ liệu & Phân biệt trạng thái trực quan trong Excel (Requirement 3)**:
   - File Excel luôn xuất 100% dữ liệu kỳ chốt để người dùng dễ theo dõi và kiểm soát.
   - Định dạng màu sắc và trạng thái trực quan:
     * Cột Trạng thái & Giá trị nghi ngờ: Nổi bật với màu Cam / Hổ phách (`#D97706`) và nền vàng nhạt.
     * Cột Trạng thái đã xác nhận / hiệu chỉnh: Nổi bật với màu Xanh lục (`#16A34A`) và nền xanh nhạt.
     * Số liệu đúng: Hiển thị chuẩn mực, rõ ràng.
4. **Hỗ trợ sửa & xác nhận đồng thời cả Chỉ số cũ và Chỉ số mới, Tự động tính sản lượng (Requirement 4)**:
   - Cấu trúc file Excel 15 cột tiêu chuẩn:
     * Cột I: **Chỉ số cũ hiệu chỉnh (Nhập để sửa)** [Nền xanh lá, header nổi bật].
     * Cột K: **Chỉ số mới hiệu chỉnh (Nhập để sửa)** [Nền xanh lá, header nổi bật].
     * Cột L: **Sản lượng (kWh) (Tự động tính)**.
     * Cột O: **Ghi chú xác nhận (Nhập 'Xác nhận')**.
   - Khi Import file Excel:
     * Hệ thống tự động nhận diện và cập nhật `NewPreviousReading` (nếu sửa chỉ số cũ) và `NewCurrentReading` (nếu sửa chỉ số mới).
     * Hệ thống tự động tính toán lại sản lượng $Consumption = (Current - Previous) \times MultiplierFactor$.
     * Đánh dấu bản ghi sang trạng thái **"Đã xác nhận & hiệu chỉnh từ file Excel"** và đưa vào danh sách **Số liệu đúng**.

---

# [DEV HANDOFF] NỀN TẢNG DỊCH VỤ EMAIL DOANH NGHIỆP (MICROSOFT GRAPH API OAUTH 2.0 & SMTP/GMAIL), QUẢN LÝ CẤU HÌNH ĐỘNG VÀ ĐẶC TẢ ĐO ĐẾM CÔNG TƠ EDMI MK6

> **Tài liệu bàn giao phát triển (Developer Handoff Changelog)**  
> **Phiên bản:** v2.1.0-ENTERPRISE  
> **Người thực hiện:** MDMS-Coder (Lead Full-Stack Developer)  
> **Người tiếp nhận:** MDMS-Tester (Lead QA Engineer) & MDMS-Reviewer (Principal Architect)  
> **Căn cứ thực hiện:** Bản thiết kế kỹ thuật [PLAN_SPEC.md](file:///e:/sources/MDMSv2/PLAN_SPEC.md).

---

## 1. Tóm tắt Thay đổi (Summary of Changes)
Đã hoàn thành 100% Phase 1 theo đúng kế hoạch:

### 1.1. Backend (.NET 8 Clean Architecture)
1. **Domain Layer (Entities & Models)**:
   - [SystemEmailConfigEntity.cs](file:///e:/sources/MDMSv2/Backend/Domain/Entities/SystemEmailConfigEntity.cs): Định nghĩa bảng cấu hình máy chủ gửi email đa nền tảng (`ProviderType`: `MICROSOFT_GRAPH`, `CUSTOM_SMTP`, `GMAIL`), lưu thông số Tenant ID, Client ID, Client Secret (OAuth 2.0) và thông số máy chủ SMTP (Host, Port, SecurityMode, RequireAuth, Username, Password).
   - [NotificationTemplateEntity.cs](file:///e:/sources/MDMSv2/Backend/Domain/Entities/NotificationTemplateEntity.cs): Định nghĩa bảng lưu trữ các mẫu email thông báo tùy biến (`USER_CREDENTIAL`, `FORGOT_PASSWORD`, `STATEMENT_NOTIFICATION`).
   - [StatementPdfTemplateEntity.cs](file:///e:/sources/MDMSv2/Backend/Domain/Entities/StatementPdfTemplateEntity.cs): Định nghĩa bảng cấu hình Profile mẫu Phiếu báo PDF QuestPDF đa dạng (Logo, Tiêu đề, Khổ giấy A4/A5, Cột hiển thị $P_{max}$, $Cos\varphi$, $AQ$).
   - [MeterReplacementEntity.cs](file:///e:/sources/MDMSv2/Backend/Domain/Entities/MeterReplacementEntity.cs): Định nghĩa bảng ghi nhận Biên bản Treo/Tháo công tơ phục vụ tính sản lượng phân đoạn.
   - [PeriodicEnergyStatementEntity.cs](file:///e:/sources/MDMSv2/Backend/Domain/Entities/PeriodicEnergyStatementEntity.cs): Bổ sung các trường $P_{max}$, $P_{max}Time$, $BillingTimestamp$, $HasMeterReplacement$, $MeterReplacementId$, $StatementPdfTemplateId$.
2. **Infrastructure Layer (EF Core & Email Providers)**:
   - Các EntityTypeConfigurations: [SystemEmailConfigEntityConfiguration.cs](file:///e:/sources/MDMSv2/Backend/Infrastructure/Databases/SystemEmailConfigEntityConfiguration.cs), [NotificationTemplateEntityConfiguration.cs](file:///e:/sources/MDMSv2/Backend/Infrastructure/Databases/NotificationTemplateEntityConfiguration.cs), [StatementPdfTemplateEntityConfiguration.cs](file:///e:/sources/MDMSv2/Backend/Infrastructure/Databases/StatementPdfTemplateEntityConfiguration.cs), [MeterReplacementEntityConfiguration.cs](file:///e:/sources/MDMSv2/Backend/Infrastructure/Databases/MeterReplacementEntityConfiguration.cs) và cập nhật quan hệ trong [PeriodicEnergyStatementEntityConfiguration.cs](file:///e:/sources/MDMSv2/Backend/Infrastructure/Databases/PeriodicEnergyStatementEntityConfiguration.cs).
   - Repositories: [SystemEmailConfigReadWriteRepository.cs](file:///e:/sources/MDMSv2/Backend/Infrastructure/Repositories/ReadWrite/SystemEmailConfigReadWriteRepository.cs) và [SystemEmailConfigReadOnlyRepository.cs](file:///e:/sources/MDMSv2/Backend/Infrastructure/Repositories/ReadOnly/SystemEmailConfigReadOnlyRepository.cs).
   - **Microsoft Graph API Email Provider**:
     * [MicrosoftGraphEmailProvider.cs](file:///e:/sources/MDMSv2/Backend/Infrastructure/Services/EmailProviders/MicrosoftGraphEmailProvider.cs): Tích hợp trực tiếp với Microsoft Entra ID (Azure AD) thông qua OAuth 2.0 Client Credentials Grant Flow (`POST https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token`). Tự động cache token trong bộ nhớ MemoryCache, gửi thư kèm file đính kèm Base64 qua Endpoint `POST https://graph.microsoft.com/v1.0/users/{senderEmail}/sendMail`. Cung cấp phương thức `TestConnectionAsync` gửi email kiểm tra kết nối thời gian thực.
   - **Custom SMTP & Gmail Provider**:
     * [SmtpEmailProvider.cs](file:///e:/sources/MDMSv2/Backend/Infrastructure/Services/EmailProviders/SmtpEmailProvider.cs): Chuẩn hóa trên thư viện `MailKit`, hỗ trợ các phương thức mã hóa (STARTTLS, SSL/TLS, None cho Internal Relay IP), hỗ trợ xác thực tài khoản hoặc gửi không xác thực.
   - **Email Provider Factory**:
     * [EmailProviderFactory.cs](file:///e:/sources/MDMSv2/Backend/Infrastructure/Services/EmailProviders/EmailProviderFactory.cs): Tự động đọc cấu hình kích hoạt trong cơ sở dữ liệu để khởi tạo Provider phù hợp, có cơ chế Fallback cấu hình mặc định trong `appsettings.json`.
   - **Cập nhật Dịch vụ Gửi Mail**:
     * [EmailService.cs](file:///e:/sources/MDMSv2/Backend/Infrastructure/Services/EmailService.cs): Chuyển đổi toàn bộ sang `IEmailProviderFactory`, giúp các luồng gửi email tài khoản khởi tạo và đặt lại mật khẩu tự động hỗ trợ ngay lập tức Microsoft 365 của VSIP Hải Phòng.
3. **Application Layer (DTOs & Contracts)**:
   - [Constants.cs](file:///e:/sources/MDMSv2/Backend/Application/Common/Constants.cs): Khai báo tên bảng và các hằng số `EmailProvider`, `NotificationTemplateCode`.
   - [IEmailProviderService.cs](file:///e:/sources/MDMSv2/Backend/Application/Services/IEmailProviderService.cs) & [IEmailProviderFactory.cs](file:///e:/sources/MDMSv2/Backend/Application/Services/IEmailProviderFactory.cs): Abstraction cho tầng dịch vụ email.
   - [EmailMessageDto.cs](file:///e:/sources/MDMSv2/Backend/Application/DataTransferObjects/Email/EmailMessageDto.cs) & [EmailAttachmentDto.cs](file:///e:/sources/MDMSv2/Backend/Application/DataTransferObjects/Email/EmailAttachmentDto.cs): Hợp đồng dữ liệu thư điện tử dùng chung.
   - Các DTO cấu hình: [UpdateSystemEmailConfigRequest.cs](file:///e:/sources/MDMSv2/Backend/Application/DataTransferObjects/Requests/EmailConfig/UpdateSystemEmailConfigRequest.cs), [TestEmailConfigRequest.cs](file:///e:/sources/MDMSv2/Backend/Application/DataTransferObjects/Requests/EmailConfig/TestEmailConfigRequest.cs), [SystemEmailConfigResponse.cs](file:///e:/sources/MDMSv2/Backend/Application/DataTransferObjects/Responses/EmailConfig/SystemEmailConfigResponse.cs).
4. **WebApi Layer (Controllers & Routes)**:
   - [Url.cs](file:///e:/sources/MDMSv2/Backend/WebApi/Common/Url.cs): Khai báo route `EmailConfig` (`Get-Active-Email-Config`, `Update-Email-Config`, `Test-Connection`).
   - [EmailConfigController.cs](file:///e:/sources/MDMSv2/Backend/WebApi/Controllers/EmailConfigController.cs): Cung cấp các RESTful API phục vụ quản trị cấu hình email với phân quyền `SuperAdmin` và `Admin`.
   - [Program.cs](file:///e:/sources/MDMSv2/Backend/WebApi/Program.cs): Đăng ký `AddMemoryCache()` và DI `IEmailProviderFactory`.

---

### 1.2. Frontend (React 18 + TypeScript + Ant Design 5)
1. **Routing & Menu**:
   - [appRouters.ts](file:///e:/sources/MDMSv2/Frontend/src/core/application/common/appRouters.ts): Khai báo route `ROUTE_PATH.EMAIL_CONFIG = '/email-config'`.
   - [endPoint.ts](file:///e:/sources/MDMSv2/Frontend/src/core/application/common/endPoint.ts): Khai báo các endpoint `GetActiveEmailConfig`, `UpdateEmailConfig`, `TestEmailConnection`.
   - [MenuKeys.ts](file:///e:/sources/MDMSv2/Frontend/src/core/domain/enums/MenuKeys.ts): Bổ sung `EmailConfig = 'email-config'`.
   - [constants.ts](file:///e:/sources/MDMSv2/Frontend/src/core/application/common/constants.ts): Thêm mục "Cấu hình Email" vào cụm menu "Cài đặt hệ thống".
   - [admin/index.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/admin/index.tsx): Đăng ký PrivateRoute cho trang Cấu hình Email.
2. **Services & Hooks**:
   - [configManagement.interface.ts](file:///e:/sources/MDMSv2/Frontend/src/core/application/interfaces/methods/tag-name/configManagement.interface.ts) & [configManagement.service.ts](file:///e:/sources/MDMSv2/Frontend/src/infrastructure/repository/config/services/configManagement.service.ts): Khai báo và hiện thực hóa các phương thức gọi API cấu hình email.
   - [useEmailConfigHook.tsx](file:///e:/sources/MDMSv2/Frontend/src/infrastructure/repository/config/hooks/useEmailConfigHook.tsx): Custom hook quản lý trạng thái tải, lưu và kiểm tra kết nối email.
3. **Màn hình Giao diện Quản trị**:
   - [email/index.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/config/email/index.tsx): Màn hình Quản trị Cấu hình Email Doanh nghiệp cao cấp:
     * Radio Card chọn 3 nhà cung cấp: **Microsoft 365 (Graph API OAuth 2.0)**, **Custom SMTP** và **Gmail**.
     * Form nhập linh hoạt theo loại dịch vụ đã chọn (với Microsoft 365: Tenant ID, Client ID, Client Secret, Email người gửi; với SMTP: Host, Port, Security Mode, Username, Password).
     * Card **"Kiểm tra kết nối (Test Connection)"**: Nhập email nhận bất kỳ và bấm gửi thử nghiệm, phản hồi trạng thái tức thời kèm thông báo lỗi rõ ràng.
     * Nút **"Lưu cấu hình"**: Lưu trực tiếp vào Database máy chủ.

---

## 2. Hoàn thành Phase 2: Đa Mẫu Email Thông Báo & Phiếu Báo PDF QuestPDF

### 2.1. Backend (.NET 8 Clean Architecture)
1. **Quản lý Mẫu Email Thông báo (`NotificationTemplates`)**:
   - [NotificationTemplateDto.cs](file:///e:/sources/MDMSv2/Backend/Application/DataTransferObjects/Templates/NotificationTemplateDto.cs): Khai báo hợp đồng CRUD, Preview request/response cho các loại mẫu: `STATEMENT_NOTIFICATION`, `FORGOT_PASSWORD`, `USER_CREDENTIAL`.
   - [INotificationTemplateService.cs](file:///e:/sources/MDMSv2/Backend/Application/Services/INotificationTemplateService.cs) & [NotificationTemplateService.cs](file:///e:/sources/MDMSv2/Backend/Infrastructure/Services/Templates/NotificationTemplateService.cs): Bộ parser linh hoạt Regex biên dịch `{{Key}}` (không phân biệt chữ hoa/thường, tự động loại bỏ khoảng trắng dư thừa, hỗ trợ format specifier, tích hợp bộ fallback templates mặc định sẵn có).
   - [NotificationTemplatesController.cs](file:///e:/sources/MDMSv2/Backend/WebApi/Controllers/NotificationTemplatesController.cs): Cung cấp RESTful API đầy đủ CRUD, thiết lập mẫu mặc định (`/set-default`), và xem trước email động (`/preview`).
2. **Quản lý Mẫu Phiếu Báo PDF Modular (`StatementPdfTemplates`)**:
   - [StatementPdfTemplateDto.cs](file:///e:/sources/MDMSv2/Backend/Application/DataTransferObjects/Templates/StatementPdfTemplateDto.cs): Định nghĩa profile cấu hình phiếu báo PDF: khổ giấy (`A5_LANDSCAPE` 595x420 pt chuẩn VSIP hoặc `A4_PORTRAIT` 595x842 pt chuẩn kế toán), tùy biến tiêu đề, tên/địa chỉ/SĐT đơn vị phát hành (Bên A), upload logo Base64, danh sách cột hiển thị linh hoạt dạng JSON (`BT`, `CD`, `TD`, `SG`, `VC`, `PMAX`, `COSPHI`), tùy chọn 1 chữ ký (Bên A) hoặc 2 chữ ký song song (Bên A & Bên B), ghi chú chân trang.
   - [IStatementPdfGenerator.cs](file:///e:/sources/MDMSv2/Backend/Application/Services/IStatementPdfGenerator.cs) & [StatementPdfGenerator.cs](file:///e:/sources/MDMSv2/Backend/Infrastructure/Services/Templates/StatementPdfGenerator.cs): Hiện thực hóa render PDF bằng thư viện QuestPDF chất lượng cao, chia lưới bảng thông số chính xác, tự động tính toán tỷ lệ theo khổ giấy và render tức thời.
   - [StatementPdfTemplatesController.cs](file:///e:/sources/MDMSv2/Backend/WebApi/Controllers/StatementPdfTemplatesController.cs): Cung cấp các API CRUD, `/set-default`, và `/preview-pdf` trả về luồng nhị phân `application/pdf` phục vụ xem trước trực tiếp trên trình duyệt.
3. **Tích hợp Tự động hóa Gửi Phiếu báo**:
   - [EmailSenderService.cs](file:///e:/sources/MDMSv2/Backend/Infrastructure/Services/EmailSenderService.cs): Được tái cấu trúc để nạp mẫu PDF và mẫu Email mặc định từ cơ sở dữ liệu. Tạo file PDF động qua `IStatementPdfGenerator` và biên dịch tiêu đề/nội dung qua `INotificationTemplateService`, sau đó gửi đính kèm file PDF qua `IEmailProviderFactory`.
   - [EmailService.cs](file:///e:/sources/MDMSv2/Backend/Infrastructure/Services/EmailService.cs): Kết nối với `NotificationTemplateService` cho các luồng gửi email tài khoản và quên mật khẩu.
   - [Program.cs](file:///e:/sources/MDMSv2/Backend/WebApi/Program.cs): Đăng ký bản quyền QuestPDF Community License và đăng ký DI cho `INotificationTemplateService` và `IStatementPdfGenerator`.

---

### 2.2. Frontend (React 18 + TypeScript + Ant Design 5)
1. **Routing, Menu & Navigation**:
   - [MenuKeys.ts](file:///e:/sources/MDMSv2/Frontend/src/core/domain/enums/MenuKeys.ts): Khai báo menu keys `NotificationTemplates` và `StatementPdfTemplates`.
   - [appRouters.ts](file:///e:/sources/MDMSv2/Frontend/src/core/application/common/appRouters.ts): Thêm routes `/notification-templates` và `/statement-pdf-templates`.
   - [constants.ts](file:///e:/sources/MDMSv2/Frontend/src/core/application/common/constants.ts): Bổ sung 2 mục menu "Mẫu Email thông báo" và "Mẫu Phiếu báo PDF" dưới nhóm "Cài đặt hệ thống".
   - [admin/index.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/admin/index.tsx): Đăng ký Route bảo vệ cho cả 2 màn hình.
2. **Services & Repositories**:
   - [templateManagement.interface.ts](file:///e:/sources/MDMSv2/Frontend/src/core/application/interfaces/methods/templates/templateManagement.interface.ts): Định nghĩa kiểu dữ liệu và request contract đầy đủ.
   - [templateManagement.service.ts](file:///e:/sources/MDMSv2/Frontend/src/infrastructure/repository/templates/services/templateManagement.service.ts): Tầng dịch vụ gọi API, xử lý nhận file PDF dưới dạng `Blob` cho việc xem trước trực tiếp.
3. **Màn hình Quản trị Mẫu Email Thông Báo**:
   - [notification-templates/index.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/config/notification-templates/index.tsx):
     * Giao diện chia Tabs theo loại mẫu: Thông báo phiếu báo, Quên mật khẩu, Cấp tài khoản.
     * Bảng danh sách mẫu kèm trạng thái Mặc định (Default Tag với ngôi sao vàng), thao tác Đặt làm mặc định, Sửa, Xóa.
     * Drawer thêm mới/chỉnh sửa mẫu với bảng thẻ Merge Tags thông minh (bấm vào thẻ là tự động chèn vào vị trí con trỏ).
     * Modal Live Preview xem trước email thực tế sau khi bind biến dữ liệu.
4. **Màn hình Quản trị Mẫu Phiếu Báo PDF**:
   - [statement-pdf-templates/index.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/config/statement-pdf-templates/index.tsx):
     * Bảng danh sách Profile mẫu phiếu báo kèm Tag khổ giấy A5/A4, số lượng cột hiển thị, trạng thái mặc định.
     * Drawer cấu hình toàn diện: Radio button chọn khổ giấy A5 Ngang / A4 Đứng, Checkbox danh sách cột hiển thị (BT, CD, TD, SG, VC, PMAX, COSPHI), Switch chuyển đổi chế độ ký xác nhận (Chỉ bên A vs Hai bên A & B), Upload Logo kèm xem trước.
     * Modal Live Preview xem file PDF trực tiếp thông qua thẻ iframe với dữ liệu render từ backend trong thời gian thực, có nút tải file PDF mẫu về máy.

---

### 2.3. Cơ sở dữ liệu (Database Scripts)
- Đã cập nhật [amr_full.sql](file:///e:/sources/MDMSv2/Installers/Database/amr_full.sql) và [20261031_create_email_config_and_metering_tables.sql](file:///e:/sources/MDMSv2/Installers/Database/20261031_create_email_config_and_metering_tables.sql) (đồng bộ cả thư mục `Publish/MDMSv2_InstallPackage/Database`):
  * DDL bảng `NotificationTemplates` & `StatementPdfTemplates`.
  * DDL bảng `MeterReplacements` (sẵn sàng cho Phase 3).
  * Khởi tạo sẵn các mẫu mặc định: Mẫu email gửi phiếu báo VSIP Hải Phòng, mẫu email quên mật khẩu, mẫu email tạo tài khoản, mẫu phiếu báo PDF A5 Ngang (VSIP Hải Phòng) và mẫu A4 Đứng.

---

## 3. Hướng dẫn Kiểm thử (Test Cases for MDMS-Tester)

| STT | Kịch bản kiểm thử | Dữ liệu đầu vào | Kết quả kỳ vọng |
|---|---|---|---|
| 1 | Truy cập trang Cấu hình Email | Đăng nhập tài khoản `SuperAdmin` $\to$ Cài đặt hệ thống $\to$ Cấu hình Email | Màn hình hiển thị đầy đủ các lựa chọn nhà cung cấp và thông tin cấu hình hiện hành |
| 2 | Kiểm tra kết nối Microsoft Graph API với VSIP HP | Chọn `Microsoft 365`, nhập thông số của VSIP HP (Tenant ID, Client ID, Secret, `noreply.hoadondientu.hp@vsip.com.vn`), nhập email nhận test | Gửi thư thành công, hộp thư nhận được email test từ `noreply.hoadondientu.hp@vsip.com.vn` |
| 3 | Lưu cấu hình Microsoft Graph API | Bấm nút "Lưu cấu hình" | Thông báo thành công, dữ liệu được lưu vào bảng `SystemEmailConfigs` |
| 4 | Quản lý Mẫu Email Thông Báo | Cài đặt hệ thống $\to$ Mẫu Email thông báo | Hiển thị các tab mẫu `STATEMENT_NOTIFICATION`, `FORGOT_PASSWORD`, `USER_CREDENTIAL`. Chức năng sửa mẫu, bấm chèn merge tag hoạt động chuẩn xác |
| 5 | Live Preview Mẫu Email | Tại form sửa mẫu email, bấm "Xem trước" | Modal mở ra hiển thị nội dung HTML đã được bind các biến dữ liệu giả lập thực tế |
| 6 | Đặt Mẫu Email Mặc Định | Bấm nút "Đặt làm mặc định" trên bảng mẫu | Icon ngôi sao vàng sáng lên, mẫu trở thành mặc định và các mẫu cùng loại bị hủy mặc định |
| 7 | Quản lý Mẫu Phiếu Báo PDF | Cài đặt hệ thống $\to$ Mẫu Phiếu báo PDF | Danh sách mẫu A5/A4 hiển thị rõ ràng các cột được cấu hình bật/tắt |
| 8 | Live Preview Phiếu Báo PDF | Bấm "Xem trước PDF" trên danh sách mẫu hoặc trong Drawer chỉnh sửa | Iframe hiển thị file PDF render thời gian thực từ QuestPDF đúng chuẩn kích thước A5/A4 và các cột đã chọn |
| 9 | Kiểm tra gửi phiếu báo tự động | Chạy tiến trình gửi phiếu báo hàng tháng | Email được gửi qua Microsoft Graph API đính kèm file PDF định dạng theo template mặc định |

---

## 4. Cập nhật Kế hoạch Triển khai & Mẫu Thông báo Chuẩn Doanh nghiệp (v2.2.0)
- **Email Templates (HTML Modular Blocks)**:
  * Đã chuẩn hóa toàn bộ 3 mẫu: `USER_CREDENTIAL` (Cấp tài khoản mới), `FORGOT_PASSWORD` (Đặt lại mật khẩu) và `STATEMENT_NOTIFICATION` (Phiếu báo điện năng).
  * Mã nguồn HTML được chia thành các khối phân tách tường minh bằng HTML comments (`<!-- [KHỐI 1] ... -->`, `<!-- [KHỐI 2] ... -->`), giúp quản trị viên không biết lập trình vẫn có thể định vị và chỉnh sửa nội dung văn bản một cách an toàn và nhanh chóng.
  * Mẫu `STATEMENT_NOTIFICATION` đã bao gồm đầy đủ bộ thông số: 3 biểu giá (BT, CD, TD), tổng hữu công (SG), điện phản kháng vô công (VC), công suất cực đại $P_{max}$, thời điểm xuất hiện $T_{Pmax}$, hệ số công suất $\text{Cos}\ \varphi$, thông tin điểm đo, số công tơ và hệ số nhân (HSN).
- **Mẫu Phiếu Báo PDF (Excel Template qua LibreOffice Headless CLI)**:
  * Đã hoàn thiện kiến trúc: Cung cấp tính năng "Tải Mẫu Excel Chuẩn (.xlsx)" trực tiếp trên giao diện. Quản trị viên sử dụng Microsoft Excel để tạo/sửa file `.xlsx` (khổ A5 ngang hoặc A4 dọc, font, viền, logo, chèn thẻ giữ chỗ `{{...}}`).
  * Backend hỗ trợ 2 Engine (`EXCEL_LIBREOFFICE` và `QUEST_PDF`). Backend sử dụng `EPPlus` thay thế token số liệu đo đếm và gọi LibreOffice Headless CLI (`soffice.exe --headless --convert-to pdf`) chuyển đổi sang PDF chất lượng cao, tự động Fallback sang QuestPDF nếu môi trường chưa cài LibreOffice.
  * Hỗ trợ lưu trữ file Excel mẫu dưới dạng `ExcelTemplateBase64` và `ExcelTemplateFileName` trong bảng `StatementPdfTemplates`.
- **Nghiệp vụ Chốt Chỉ Số GCS (Mô hình Pure PUSH - REST Server & Nhận diện Số liệu Nghi ngờ)**:
  * Loại bỏ hoàn toàn lỗi chặn khi thiếu `BaseUrlGetDataFromServerIoT`, loại bỏ yêu cầu Polling REST Client bắt buộc qua Port 39320 của Kepware IoT Gateway.
  * Hệ thống hoạt động theo mô hình REST Server: Kepware IoT Client Gateway đẩy dữ liệu trực tiếp vào bảng `MeasurementPointMonitoringByDateTimeEntity`.
  * Đọc các thanh ghi chốt kỳ đông kết (Frozen Cutoff Registers) bất biến từ CSDL để tính toán sản lượng `(Chỉ số mới - Chỉ số cũ) * HSN`.
  * Thuật toán phát hiện dị thường & số liệu nghi ngờ (`IsSuspicious = true` kèm `SuspiciousReason` cụ thể):
    - Tín hiệu đo đếm chất lượng kém (`Quanlity = false`) hoặc mất kết nối.
    - Chưa nhận được chỉ số chốt từ công tơ (`CurrentReading == null`).
    - Thời điểm chốt trên công tơ bị lệch so với ngày chốt quy định (> 2 ngày).
    - Sản lượng âm (`Consumption < 0`) do chỉ số mới nhỏ hơn chỉ số cũ.
    - Sản lượng bằng 0 đối với điện năng biểu giờ bình thường hoặc tổng (`BT`, `SG`).
    - Sản lượng tăng/giảm đột biến $\pm 30\%$ so với sản lượng của kỳ trước đó.
  * Giao diện GCS Management (`gcs-sum`):
    - Thêm bộ nút Tab lọc nhanh: `[📊 Tất cả điểm đo]`, `[⚠️ Số liệu bị nghi ngờ]`, `[❌ Chỉ số lỗi / mất kết nối]`.
    - Dòng số liệu nghi ngờ được tô viền/nền vàng hổ phách (`suspicious-row`), gắn huy hiệu `⚠️ Nghi ngờ` kèm Tooltip hiển thị lý do chi tiết.
    - Cho phép click sửa trực tiếp chỉ số mới hoặc chỉ số cũ với tính năng tính nhẩm sản lượng thời gian thực. Khi lưu thành công, hệ thống tự động giải phóng cờ nghi ngờ.
    - Popup "Xác nhận chốt sổ" tự động thống kê và cảnh báo cán bộ nếu còn tồn tại điểm đo nghi ngờ hoặc lỗi trước khi ghi nhận chốt kỳ.
    - Chặn chốt sổ ở Backend trong `FinalMeterReadingCommandHandler` nếu chưa được xử lý/hiệu chỉnh.

---

## 5. Cập nhật Phiếu báo chỉ số & Nghiệp vụ Chốt kỳ Đầu tiên (Statement Branding & First-Time Cutoff)
1. **Nhận diện Thương hiệu VSIP trên Mẫu Phiếu Báo (Statement Notice Branding)**:
   - **Logo VSIP (Góc trên bên trái)**: Tích hợp trực tiếp logo VSIP độ nét cao (Base64 PNG) chuẩn nhận diện thương hiệu vào các mẫu Phiếu báo.
   - **Biểu tượng Ba sọc trang trí (Góc trên bên phải)**: Hiện thực hóa biểu tượng 3 dải màu uốn cong đặc trưng của VSIP gồm: Xanh lá (`#9FC153`), Xanh lơ (`#0093ED`), và Xanh thẫm (`#004C87`) trên nền trong suốt (Anti-aliased PNG).
   - Đồng bộ hiển thị trên:
     * Excel Template chuẩn (`StatementPdfGenerator.GetStandardExcelTemplate`) chèn drawing logo và họa tiết vào ô góc phải trên cùng.
     * Chuyển đổi Excel $\to$ PDF tự động bằng LibreOffice Headless giữ nguyên bố cục hình ảnh sắc nét.
     * QuestPDF Fallback Renderer (`BuildQuestPdfDocument`) hỗ trợ căn lề đối xứng header (Logo trái - Họa tiết phải).
     * Giao diện Cấu hình Mẫu Phiếu báo (`Frontend/src/pages/config/statement-pdf-templates/index.tsx`): Tự động nạp sẵn Logo VSIP cho các Profile mẫu A5 / A4.

2. **Tối ưu hóa Điều kiện Chốt sổ Lần đầu (First-Time Cutoff Cycle Initialization)**:
   - Khắc phục lỗi `"Vui lòng chốt kỳ theo đúng trình tự"` khi cán bộ chốt chỉ số Tháng 10/2026 trên sổ mới triển khai tại đường dẫn `/gcs-latch`.
   - [GcsCutOffCycleReadOnlyRepository.cs](file:///e:/sources/MDMSv2/Backend/Infrastructure/Repositories/ReadOnly/GcsCutOffCycleReadOnlyRepository.cs):
     * Hiệu chỉnh cơ chế xác thực khi chưa tồn tại chu kỳ chốt nào trong cơ sở dữ liệu (`lastGcsCutOffCycle == null`), chấp nhận Tháng/Năm được chọn (ví dụ: Tháng 10/2026) làm mốc chốt chỉ số đầu tiên mà không bị chặn bởi ngày giờ tạo bản ghi sổ (`CreatedTime`) hay điều kiện thời gian tương lai (`datetime > DateTime.Now`), chỉ kiểm tra tính hợp lệ của số thứ tự kỳ (`request.PeriodNumber >= 1 && request.PeriodNumber <= existGcs.PeriodNumber`).
     * Xử lý an toàn mảng `DayPeriod` và chuyển sang `new DateTime(year, month, day)` an toàn, loại bỏ triệt để lỗi ép kiểu hoặc phân tích chuỗi ngày giờ.
     * Bọc try/catch chống lỗi cho truy vấn `dbMonitoringRecords` và xử lý exception tại [IoTController.cs](file:///e:/sources/MDMSv2/Backend/WebApi/Controllers/IoTController.cs) trả về `RequestResult.Fail` có thông điệp rõ ràng, triệt tiêu lỗi HTTP 500.
     * An toàn hóa Promise trong [dialog-gcs.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/gcs-sum/components/dialog-gcs.tsx) đảm bảo quá trình chốt sổ mượt mà.
   - Các kỳ chốt tiếp theo tiếp tục tuân thủ quy tắc tịnh tiến chu kỳ nghiêm ngặt (`IsValidCycleSequence`).

3. **Khắc phục Lỗi Xem trước PDF & Hiệu chỉnh UI/UX Nút Engine Mẫu Phiếu Báo**:
   - **Xem trước PDF**: Khắc phục lỗi chuỗi Base64 Logo VSIP bị ngắt dòng; chuẩn hóa trọn vẹn 11.980 ký tự (giải mã đúng 8.983 bytes PNG chuẩn); bổ sung cơ chế bọc an toàn tránh lỗi crash nếu dữ liệu ảnh trống.
   - **Căn chỉnh Nút Engine & Khổ giấy**: Tái cấu trúc Radio Button chọn Công nghệ kết xuất (`Mẫu Excel qua LibreOffice` và `Mẫu C# tĩnh QuestPDF`) sang Flex layout với chiều cao đồng nhất 44px, gắn nhãn Tag "Khuyên dùng" tinh tế, đảm bảo 2 nút luôn thẳng hàng, không bị tràn dòng hay lệch độ cao.

---

---

## 7. Khắc phục Lỗi Ghi chỉ số (GCS Latch) & Tối ưu hóa Toàn diện Giao diện Mẫu Phiếu Báo
1. **Khắc phục Triệt để Lỗi Ghi chỉ số Chốt sổ (`Unknown column 'g.NewPreviousReading' in 'field list'`)**:
   - **Nguyên nhân cốt lõi**: Thuộc tính `NewPreviousReading` (và `IsSuspicious`, `SuspiciousReason`) mới được bổ sung vào entity [GcsTemporaryEntity.cs](file:///e:/sources/MDMSv2/Backend/Domain/Entities/GcsTemporaryEntity.cs) phục vụ nghiệp vụ xử lý bất thường số liệu, nhưng bảng vật lý `GcsTemporary` (hoặc `gcstemporary`) trên máy chủ CSDL MySQL chưa được bổ sung các cột này, dẫn đến EF Core phát sinh câu truy vấn `SELECT g.NewPreviousReading...` và bị MySQL từ chối.
   - **Giải pháp xử lý nhiều lớp (Multi-layer Self-Healing Migration)**:
     * [GcsTemporaryReadWriteRepository.cs](file:///e:/sources/MDMSv2/Backend/Infrastructure/Repositories/ReadWrite/GcsTemporaryReadWriteRepository.cs): Bổ sung cơ chế `EnsureColumnsExistAsync()` tự phục hồi thông minh. Tự động chạy `ALTER TABLE` bổ sung `NewPreviousReading`, `IsSuspicious`, `SuspiciousReason` (cho cả `GcsTemporary` và `gcstemporary`) ngay tại thời điểm gọi dữ liệu trước khi truy vấn EF Core được thực thi.
     * [GcsCutOffCycleReadOnlyRepository.cs](file:///e:/sources/MDMSv2/Backend/Infrastructure/Repositories/ReadOnly/GcsCutOffCycleReadOnlyRepository.cs): Bọc gọi an toàn `EnsureColumnsExistAsync` ngay đầu hàm `GetDataLatchResponseAsync`, ngăn chặn mọi phát sinh lỗi `MySqlException`.
     * [Program.cs](file:///e:/sources/MDMSv2/Backend/WebApi/Program.cs): Bổ sung `ALTER TABLE` các cột vào khối kiểm tra CSDL khởi động hệ thống.
     * [Update_Hotfix.ps1](file:///e:/sources/MDMSv2/Installers/Scripts/Update_Hotfix.ps1): Tích hợp tự động câu lệnh cập nhật cấu trúc schema CSDL khi chạy gói Hotfix trên máy chủ môi trường thật.

2. **Chuẩn hóa Giao diện "Chỉnh sửa Mẫu Phiếu Báo" (Ngắn gọn, Trực quan, Phổ quát, Không gán cứng cụ thể)**:
   - [Frontend/src/pages/config/statement-pdf-templates/index.tsx](file:///e:/sources/MDMSv2/Frontend/src/pages/config/statement-pdf-templates/index.tsx):
     * **Bộ nút Mẫu cấu hình sẵn**:
       - `+ Mẫu Phiếu Báo Chuẩn A5 Ngang (Khuyên dùng)` $\to$ `+ Mẫu A5 Tiêu chuẩn`
       - `+ Mẫu Phiếu Báo Toàn Diện A4 Dọc` $\to$ `+ Mẫu A4 Đầy đủ`
       - `+ Mẫu Phiếu Báo Tinh Gọn A5 Ngang` $\to$ `+ Mẫu A5 Tinh gọn`
     * **Khổ giấy (Paper Size)**: Loại bỏ các nhãn gán tên riêng doanh nghiệp/phòng ban (`Chuẩn VSIP`, `Chuẩn Kế toán`), tinh gọn thành hai tùy chọn chuẩn: `A5 Ngang` và `A4 Dọc`.
     * **Công nghệ kết xuất (Engine)**: Tinh gọn nhãn nút thành `Excel (LibreOffice)` (với Tag "Khuyên dùng") và `Mã C# (QuestPDF)`, giữ thẳng hàng, cân đối 44px.
     * **Tùy biến mẫu Excel**:
       - Tiêu đề card: `Tùy biến mẫu Excel (.xlsx)`
       - Nút thao tác: `Tải file mẫu (.xlsx)`, `Đổi file Excel (.xlsx)`, `Dùng mẫu mặc định`.
       - Nhãn tham số: `Các tham số trong file Excel:`.
     * **Nhãn các trường Form**: Rút gọn, phổ quát, bỏ các chỉ dẫn rườm rà:
       - `Tên mẫu` (placeholder: "Ví dụ: Mẫu A5 Tiêu chuẩn")
       - `Mã mẫu` (placeholder: "Ví dụ: A5_STANDARD")
       - `Mặc định`, `Kích hoạt`
       - `Tiêu đề chính` (placeholder: "Ví dụ: PHIẾU BÁO CHỈ SỐ ĐIỆN NĂNG")
       - `Tiêu đề phụ` (placeholder: "Ví dụ: (MONTHLY POWER METER READING)")
       - `Tên đơn vị phát hành` (placeholder: "Ví dụ: TÊN ĐƠN VỊ PHÁT HÀNH")
       - `Thông tin liên hệ` (placeholder: "Ví dụ: Điện thoại: (024) 3xxx xxxx - Fax: (024) 3xxx xxxx")
       - `Địa chỉ đơn vị` (placeholder: "Ví dụ: Địa chỉ trụ sở đơn vị phát hành")
       - `Cột hiển thị`, `Chữ ký xác nhận` (`2 Chữ ký (Giao & Nhận)` / `1 Chữ ký`)
       - `Ghi chú chân trang`, `Logo phiếu báo` (Nút: `Tải ảnh logo`, `Xóa logo`).
     * **Nút điều hướng & Bảng danh sách**:
       - Nút thao tác Drawer: `[Chọn mẫu sẵn]`, `[Xem trước]`, `[Hủy]`, `[Lưu]`.
       - Thanh nút trang chủ: `Tải file mẫu (.xlsx)`, `Thư viện mẫu`, `Khôi phục mặc định`, `Thêm mới`.
       - Cột bảng: `Tên mẫu`, `Mã mẫu`, `Khổ giấy`, `Công nghệ`, `Chữ ký`, `Trạng thái`, `Thao tác`.

---

## 9. Nâng cấp UI/UX Ghi chỉ số & Quy trình Hiệu chỉnh Hàng loạt qua Excel (GCS Latch)
1. **Đồng bộ UI/UX hiển thị "Số liệu nghi ngờ" theo đúng chuẩn trang Báo cáo (Screenshot 1)**:
   - Loại bỏ class `suspicious-row` màu hồng cam phủ toàn dòng gây rối mắt; bảng dữ liệu giữ nguyên phông nền sạch, phân dòng xen kẽ tiêu chuẩn.
   - Bỏ huy hiệu `⚠️ Nghi ngờ` rườm rà ở cột *Mã điểm đo*.
   - Bỏ dòng phụ ghi lý do nghi ngờ ở cột *Bộ chỉ số*.
   - Cột *Chỉ số mới* và *Sản lượng*: Nếu là số liệu nghi ngờ chưa xử lý, hiển thị số màu cam hổ phách (`text-amber-600 font-medium`) kèm dấu sao (`*` `text-amber-500 font-bold`) và Tooltip giải thích chi tiết lý do nghi ngờ khi hover chuột (đồng nhất 100% với `EnergyConsumptionTable`).
   - Nếu bản ghi đã được hiệu chỉnh/xác nhận (`newCurrentReading != null`), hiển thị biểu tượng `[Đã hiệu chỉnh]` và giữ màu chữ bình thường.

2. **Khắc phục số lượng "Tất cả các điểm đo"**:
   - Backend tính toán chính xác số điểm đo phân biệt `TotalMeasurementPoints = await baseScope.Select(x => x.MeasurementPointId).Distinct().CountAsync()` và trả về trong `CursorPaginationResponse`.
   - Frontend hiển thị nhãn tab: `Tất cả điểm đo ({TotalMeasurementPoints})` (ví dụ: 162 điểm đo thay vì hiển thị 972 bản ghi).

3. **Khắc phục lỗi đếm "Số liệu nghi ngờ"**:
   - Backend tính toán tổng số bản ghi nghi ngờ chưa xử lý trên toàn bộ kỳ chốt `TotalSuspiciousCount = await baseScope.CountAsync(x => x.IsSuspicious && x.NewCurrentReading == null && x.NewPreviousReading == null)` và trả về trong `CursorPaginationResponse`.
   - Frontend hiển thị chính xác tổng số liệu nghi ngờ của cả kỳ: `Số liệu nghi ngờ ({TotalSuspiciousCount})` thay vì chỉ đếm 10 dòng trên trang hiện tại.

4. **Loại bỏ hiển thị "Chỉ số lỗi / mất kết nối"**:
   - Loại bỏ nút tab `❌ Chỉ số lỗi / mất kết nối` trên giao diện ghi chỉ số.
   - Loại bỏ tùy chọn `bad_quality` trong dropdown bộ lọc.

5. **Bộ lọc trạng thái chỉ số ("Số liệu đúng" / "Số liệu nghi ngờ")**:
   - Dropdown bộ lọc hỗ trợ 2 trạng thái: `Số liệu đúng` (`valid`) và `Số liệu nghi ngờ` (`suspicious`).
   - Ba tab truy vấn nhanh:
     * `Tất cả điểm đo ({TotalMeasurementPoints})`
     * `Số liệu nghi ngờ ({TotalSuspiciousCount})`
     * `Số liệu đúng ({TotalCount - TotalSuspiciousCount})`

6. **Giải pháp & Quy trình Hiệu chỉnh Số liệu Nghi ngờ Hàng loạt qua Excel**:
   - **Đánh giá nghiệp vụ**: Với hàng trăm điểm đo/chỉ số nghi ngờ trong một kỳ chốt sổ, việc click từng modal popup để sửa thủ công trên Web rất tốn thời gian. Giải pháp xuất Excel $\to$ hiệu chỉnh $\to$ nhập lại $\to$ chốt sổ là giải pháp tối ưu, chuẩn hóa theo quy trình nghiệp vụ điện lực.
   - **Backend API**:
     * `GET /api/v1/gcs/export-excel-gcs-temporary?gcsId=...&month=...&year=...&periodNumber=...&onlySuspicious=...`: Sử dụng EPPlus xuất bảng kê Excel định dạng chuẩn (Header màu xanh đậm, cột J "Chỉ số mới hiệu chỉnh" được tô viền xanh lá để nhập liệu, highlight dòng nghi ngờ màu cam).
     * `POST /api/v1/gcs/import-excel-gcs-temporary`: Nhận file `.xlsx`, tự động đối soát theo ID bản ghi hoặc Mã điểm đo & Bộ chỉ số. Cập nhật `NewCurrentReading`, tính lại sản lượng `Consumption`, đánh dấu `IsSuspicious = false` và lý do "Đã xác nhận & hiệu chỉnh từ file Excel", trả về thống kê kết quả chi tiết.
   - **Frontend UI/UX**:
     * Nút **Xuất file Excel** (`Dropdown`): Cho phép chọn "Xuất danh sách nghi ngờ" hoặc "Xuất toàn bộ bảng kê".
     * Nút **Nhập file Excel & Xác nhận**: Mở modal `DialogImportExcel` với vùng kéo thả file, hướng dẫn 4 bước, nút tải mẫu trực tiếp và bảng thông báo kết quả chi tiết (tổng dòng, số dòng cập nhật, số dòng giữ nguyên, cảnh báo nếu có).
     * Tự động làm mới danh sách dữ liệu và cập nhật số lượng nghi ngờ về 0 sau khi nhập file.
     * Cảnh báo chốt sổ: Chỉ hiển thị khi vẫn còn số liệu nghi ngờ chưa xử lý.

---

## 10. Hoàn tất & Trạng thái Hệ thống (Sign-Off Status)
- **Phase 1 (Nền tảng Email Đa Dịch vụ):** Hoàn thành 100%.
- **Phase 2 (Mẫu Email Modular & Excel $\to$ PDF LibreOffice Headless):** Hoàn thành 100%.
- **Phase 4 (GCS Pure PUSH REST Server & Giám sát Số liệu Nghi ngờ):** Hoàn thành 100%.
- **Khắc phục Lỗi GCS Latch (Unknown column `NewPreviousReading`):** Hoàn thành 100%.
- **Hiệu chỉnh UI/UX Mẫu Phiếu Báo Ngắn gọn, Phổ quát:** Hoàn thành 100%.
- **Nâng cấp UI/UX & Quy trình Hiệu chỉnh Excel cho GCS Latch:** Hoàn thành 100%.
- **Biên dịch & Kiểm thử Hệ thống:**
  * Backend `.NET 8 Release`: **0 Error(s)**, build thành công.
  * Frontend `Vite + TypeScript (build:uat)`: **0 Error(s)**, build thành công.
- **Tuân thủ quy chuẩn Git:** Giữ nguyên toàn bộ mã nguồn trên local working tree, **tuyệt đối không thực hiện commit hay push lên Git** theo đúng chỉ đạo của Quản trị viên.




