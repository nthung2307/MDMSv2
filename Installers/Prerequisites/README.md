# DANH MỤC THÀNH PHẦN PHỤ TRỢ (PREREQUISITES) OFFLINE

Thư mục này chứa các bộ cài đặt phụ trợ cần thiết khi triển khai hệ thống MDMSv2 trên Windows Server 2019:

| Tên File | Vai Trò / Dịch Vụ | Tự Động Cài Đặt Bởi | Bắt Buộc? |
|---|---|---|---|
| `rewrite_amd64_en-US.msi` | IIS URL Rewrite Module 2.1 (hỗ trợ Reverse Proxy API & React SPA) | `1_Install_Prerequisites.ps1` | Có |
| `requestRouter_amd64.msi` | IIS Application Request Routing (ARR) 3.0 (Proxy reverse) | `1_Install_Prerequisites.ps1` | Có |
| `jre-8u451-windows-i586-iftw.exe` | Java Runtime Environment 8 (Yêu cầu bởi Kepware IoT Gateway) | `1_Install_Prerequisites.ps1` | Có (cho Kepware) |
| `MicrosoftEdgeSetup.exe` | Trình duyệt Microsoft Edge (thay thế IE11 trên Windows Server 2019) | `1_Install_Prerequisites.ps1` | Khuyến nghị |
| `LibreOffice_24.8.3_Win_x86-64.msi` (hoặc bản bất kỳ) | LibreOffice Headless (chuyển đổi mẫu phiếu báo Excel `.xlsx` sang PDF) | `1_Install_Prerequisites.ps1` / `Update_Hotfix.ps1` | Tùy chọn (Hệ thống tự động Fallback sang QuestPDF nếu thiếu) |

### Hướng Dẫn Tự Động Cài Đặt LibreOffice:
1. Tải bản LibreOffice Windows x86-64 MSI chính thức từ:  
   `https://www.libreoffice.org/download/download-libreoffice/` hoặc `https://download.documentfoundation.org/libreoffice/stable/`
2. Đặt file tệp dạng `LibreOffice_*_Win_x86-64.msi` vào thư mục `Prerequisites/` này.
3. Khi chạy `SetupMDMS.ps1` (máy mới) hoặc `Update_Hotfix.ps1` (máy cũ), script sẽ tự động nhận diện và cài đặt ngầm hoàn toàn (`/qn /norestart ALLUSERS=1`) mà không cần can thiệp thủ công.
