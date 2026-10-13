# THƯ MỤC CHỨA BỘ CÀI ĐẶT KEPWARE SERVER

Vui lòng sao chép tệp cài đặt Kepware KEPServerEX vào thư mục này:

### Các tệp cài đặt được hỗ trợ:
1. **KEPServerEX Installer**:
   - Tên tệp: `KEPServerEX-*.exe` (ví dụ: `KEPServerEX-6.6.350.0.exe`, `KEPServerEX-6.14.*.exe`, hoặc `setup.exe`)
   - Phiên bản khuyến nghị: **KEPServerEX V6.6+**

---
### Ghi chú khi cài đặt:
- Chọn các thành phần bắt buộc:
  - **Modbus Suite** (Hỗ trợ Modbus RTU Serial và Modbus TCP Ethernet).
  - **IoT Gateway** (REST Server Agent mở cổng 39320 và REST Client Agent đẩy dữ liệu sang WebApi cổng 19089).
- Sau khi cài đặt, tham khảo hướng dẫn cấu hình chi tiết tại: `kepware_iot_gateway_config.md`.
