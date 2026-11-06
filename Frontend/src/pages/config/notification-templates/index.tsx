import React, { useEffect, useState } from 'react';
import {
  Alert,
  Badge,
  Button,
  Card,
  Col,
  Divider,
  Drawer,
  Dropdown,
  Form,
  Input,
  Modal,
  Popconfirm,
  Radio,
  Row,
  Select,
  Space,
  Switch,
  Table,
  Tabs,
  Tag,
  Tooltip,
  Typography,
  message,
} from 'antd';
import {
  AppstoreOutlined,
  BlockOutlined,
  CheckCircleOutlined,
  CopyOutlined,
  DeleteOutlined,
  DownOutlined,
  EditOutlined,
  EyeOutlined,
  KeyOutlined,
  MailOutlined,
  PhoneOutlined,
  PlusOutlined,
  ReloadOutlined,
  SendOutlined,
  StarFilled,
  StarOutlined,
  TableOutlined,
  WarningOutlined,
} from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';

import {
  NotificationTemplateDto,
  CreateNotificationTemplateRequest,
  UpdateNotificationTemplateRequest,
  PreviewNotificationTemplateResponse,
} from '@/core/application/interfaces/methods/templates/templateManagement.interface';
import { ButtonCommonCustomizable } from '@/infrastructure/common/components/button/button-common-customizable';
import { BreadcrumbUI } from '@/infrastructure/common/components/controls/BreadcrumbUI';
import { FullPageLoading } from '@/infrastructure/common/components/controls/Loading';
import PageTitle from '@/infrastructure/common/components/title/page-title';
import { TemplateManagementService } from '@/infrastructure/repository/templates/services/templateManagement.service';
import { _t } from '@/infrastructure/utils/translation';

const { Text, Paragraph, Title } = Typography;
const { TextArea } = Input;

const TEMPLATE_CODES = [
  { value: 'STATEMENT_NOTIFICATION', label: 'Phiếu báo điện năng (STATEMENT_NOTIFICATION)' },
  { value: 'FORGOT_PASSWORD', label: 'Quên mật khẩu (FORGOT_PASSWORD)' },
  { value: 'USER_CREDENTIAL', label: 'Cấp tài khoản mới (USER_CREDENTIAL)' },
];

const MERGE_TAGS_BY_GROUP = [
  {
    group: 'Khách hàng & Điểm đo',
    tags: [
      { tag: '{{CustomerName}}', desc: 'Tên khách hàng' },
      { tag: '{{PointCode}}', desc: 'Mã điểm đo' },
      { tag: '{{PointName}}', desc: 'Tên điểm đo' },
      { tag: '{{CustomerCode}}', desc: 'Mã khách hàng' },
      { tag: '{{MeterSerial}}', desc: 'Số chế tạo công tơ' },
      { tag: '{{Multiplier}}', desc: 'Hệ số nhân (HSN)' },
      { tag: '{{CompanyName}}', desc: 'Tên đơn vị phát hành' },
    ],
  },
  {
    group: 'Thời gian & Chu kỳ',
    tags: [
      { tag: '{{CycleName}}', desc: 'Tên chu kỳ chốt (vd: Kỳ 1 Tháng 10/2026)' },
      { tag: '{{Period}}', desc: 'Chu kỳ đo đếm' },
      { tag: '{{FromDate}}', desc: 'Từ ngày (dd/MM/yyyy)' },
      { tag: '{{ToDate}}', desc: 'Đến ngày (dd/MM/yyyy)' },
      { tag: '{{Month}}', desc: 'Tháng' },
      { tag: '{{Year}}', desc: 'Năm' },
    ],
  },
  {
    group: 'Chỉ số & Sản lượng',
    tags: [
      { tag: '{{ConsumtionBt}}', desc: 'Sản lượng Bình thường (kWh)' },
      { tag: '{{ConsumtionCd}}', desc: 'Sản lượng Cao điểm (kWh)' },
      { tag: '{{ConsumtionTd}}', desc: 'Sản lượng Thấp điểm (kWh)' },
      { tag: '{{ConsumtionTotal}}', desc: 'Tổng điện năng hữu công (kWh)' },
      { tag: '{{ConsumtionVc}}', desc: 'Điện năng vô công (kVARh)' },
      { tag: '{{Pmax}}', desc: 'Công suất cực đại Pmax (kW)' },
      { tag: '{{PmaxTime}}', desc: 'Thời điểm xuất hiện Pmax' },
      { tag: '{{CosPhi}}', desc: 'Hệ số công suất Cos φ' },
      { tag: '{{SerialNumber}}', desc: 'Số thứ tự phiếu báo' },
    ],
  },
  {
    group: 'Tài khoản & Xác thực',
    tags: [
      { tag: '{{FullName}}', desc: 'Họ và tên người dùng' },
      { tag: '{{Username}}', desc: 'Tên đăng nhập hệ thống' },
      { tag: '{{Password}}', desc: 'Mật khẩu khởi tạo' },
      { tag: '{{LoginUrl}}', desc: 'Đường dẫn đăng nhập' },
      { tag: '{{ResetLink}}', desc: 'Đường dẫn đặt lại mật khẩu' },
      { tag: '{{SupportPhone}}', desc: 'Số điện thoại hỗ trợ CSKH' },
      { tag: '{{SystemEmail}}', desc: 'Email quản trị hệ thống' },
    ],
  },
];

// Visual Pre-built Templates Library
interface PrebuiltTemplateItem {
  id: string;
  code: string;
  name: string;
  tag: string;
  description: string;
  subjectTemplate: string;
  bodyHtmlTemplate: string;
  badgeColor: string;
}

const PREBUILT_TEMPLATES_LIBRARY: PrebuiltTemplateItem[] = [
  {
    id: 'stmt-standard',
    code: 'STATEMENT_NOTIFICATION',
    name: 'Phiếu Báo Điện Năng - Tiêu Chuẩn Doanh Nghiệp (Phân Khối Khoa Học)',
    tag: 'Khuyên dùng cho VSIP',
    badgeColor: '#b22222',
    description: 'Mẫu thông báo chỉ số và sản lượng điện năng hàng tháng, cấu trúc phân khối comment rõ ràng, đầy đủ bảng 3 giá, Pmax, Cos phi và tệp đính kèm PDF.',
    subjectTemplate: 'Phiếu Báo Chỉ Số Điện Số {{SerialNumber}} - {{CustomerName}} (Kỳ {{Period}})',
    bodyHtmlTemplate: `<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Thông Báo Chỉ Số & Sản Lượng Điện Năng</title>
</head>
<body style='margin:0;padding:0;background-color:#f5f7fc;font-family:-apple-system,BlinkMacSystemFont,"Segoe UI",Roboto,Helvetica,Arial,sans-serif;color:#334155;'>
    <table width='100%' cellpadding='0' cellspacing='0' border='0' style='background-color:#f5f7fc;padding:30px 15px;'>
        <tr>
            <td align='center'>
                <table width='100%' cellpadding='0' cellspacing='0' border='0' style='max-width:640px;background-color:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 4px 16px rgba(0,0,0,0.06);border:1px solid #e2e8f0;'>
                    
                    <!-- ======================================================== -->
                    <!-- [KHỐI 1] HEADER: TÊN CÔNG TY & HỆ THỐNG ĐO ĐẾM (MDMS)   -->
                    <!-- Quản trị viên có thể thay đổi tên đơn vị tại đây         -->
                    <!-- ======================================================== -->
                    <tr>
                        <td style='background:linear-gradient(135deg,#b22222 0%,#8b1a1a 100%);padding:24px 30px;text-align:center;'>
                            <h2 style='margin:0;color:#ffffff;font-size:20px;font-weight:700;'>{{CompanyName}}</h2>
                            <p style='margin:6px 0 0 0;color:#fecaca;font-size:13px;letter-spacing:0.5px;'>HỆ THỐNG QUẢN LÝ DỮ LIỆU ĐO ĐẾM ĐIỆN TỬ (MDMS)</p>
                            <p style='margin:4px 0 0 0;color:#ffffff;font-size:14px;font-weight:600;'>THÔNG BÁO CHỈ SỐ & SẢN LƯỢNG ĐIỆN NĂNG</p>
                        </td>
                    </tr>

                    <!-- ======================================================== -->
                    <!-- [KHỐI 2] THÔNG TIN KHÁCH HÀNG & ĐIỂM ĐO ĐIỆN             -->
                    <!-- ======================================================== -->
                    <tr>
                        <td style='padding:24px 30px 10px 30px;'>
                            <p style='margin:0 0 14px 0;font-size:15px;color:#0f172a;'>Kính gửi Quý khách hàng: <strong>{{CustomerName}}</strong>,</p>
                            <p style='margin:0 0 16px 0;font-size:13.5px;line-height:1.6;color:#475569;'>
                                Chúng tôi xin trân trọng gửi tới Quý khách hàng bảng kê chỉ số công tơ và sản lượng điện năng tiêu thụ trong kỳ ghi chỉ số: <strong>{{Period}}</strong> (từ ngày {{FromDate}} đến ngày {{ToDate}}).
                            </p>
                            <table width='100%' cellpadding='8' cellspacing='0' border='0' style='background-color:#f8fafc;border-radius:8px;border:1px solid #e2e8f0;margin-bottom:18px;font-size:13px;'>
                                <tr>
                                    <td width='25%' style='color:#64748b;'>Mã khách hàng:</td>
                                    <td width='25%' style='font-weight:600;color:#0f172a;'>{{CustomerCode}}</td>
                                    <td width='25%' style='color:#64748b;'>Tên điểm đo:</td>
                                    <td width='25%' style='font-weight:600;color:#0f172a;'>{{PointName}}</td>
                                </tr>
                                <tr>
                                    <td style='color:#64748b;'>Mã điểm đo:</td>
                                    <td style='font-weight:600;color:#0f172a;'>{{PointCode}}</td>
                                    <td style='color:#64748b;'>Số công tơ:</td>
                                    <td style='font-weight:600;color:#0f172a;'>{{MeterSerial}}</td>
                                </tr>
                                <tr>
                                    <td style='color:#64748b;'>Hệ số nhân (HSN):</td>
                                    <td style='font-weight:700;color:#b22222;'>{{Multiplier}}</td>
                                    <td style='color:#64748b;'>Địa chỉ:</td>
                                    <td style='color:#334155;'>{{Address}}</td>
                                </tr>
                            </table>
                        </td>
                    </tr>

                    <!-- ======================================================== -->
                    <!-- [KHỐI 3] BẢNG BỘ CHỈ SỐ ĐO ĐẾM & SẢN LƯỢNG 3 BIỂU GIÁ    -->
                    <!-- ======================================================== -->
                    <tr>
                        <td style='padding:0 30px 15px 30px;'>
                            <table width='100%' cellpadding='8' cellspacing='0' border='1' style='border-collapse:collapse;border-color:#e2e8f0;font-size:13px;'>
                                <tr style='background-color:#f1f5f9;color:#0f172a;font-weight:600;text-align:center;'>
                                    <td width='32%'>Biểu giá</td>
                                    <td width='22%'>Chỉ số cũ</td>
                                    <td width='22%'>Chỉ số mới</td>
                                    <td width='24%'>Sản lượng (kWh)</td>
                                </tr>
                                <tr>
                                    <td style='padding-left:12px;'>Bình thường (BT)</td>
                                    <td align='right' style='color:#475569;'>{{PreviousBt}}</td>
                                    <td align='right' style='color:#475569;'>{{Bt}}</td>
                                    <td align='right' style='font-weight:700;color:#0f172a;'>{{ConsumtionBt}}</td>
                                </tr>
                                <tr>
                                    <td style='padding-left:12px;'>Cao điểm (CD)</td>
                                    <td align='right' style='color:#475569;'>{{PreviousCd}}</td>
                                    <td align='right' style='color:#475569;'>{{Cd}}</td>
                                    <td align='right' style='font-weight:700;color:#0f172a;'>{{ConsumtionCd}}</td>
                                </tr>
                                <tr>
                                    <td style='padding-left:12px;'>Thấp điểm (TD)</td>
                                    <td align='right' style='color:#475569;'>{{PreviousTd}}</td>
                                    <td align='right' style='color:#475569;'>{{Td}}</td>
                                    <td align='right' style='font-weight:700;color:#0f172a;'>{{ConsumtionTd}}</td>
                                </tr>
                                <tr style='background-color:#fef2f2;font-weight:700;'>
                                    <td style='padding-left:12px;color:#991b1b;'>Tổng điện hữu công (SG)</td>
                                    <td align='right' style='color:#991b1b;'>{{PreviousSg}}</td>
                                    <td align='right' style='color:#991b1b;'>{{Sg}}</td>
                                    <td align='right' style='color:#b22222;font-size:14px;'>{{ConsumtionTotal}}</td>
                                </tr>
                                <tr>
                                    <td style='padding-left:12px;color:#64748b;'>Điện phản kháng (VC - kVARh)</td>
                                    <td align='right' style='color:#64748b;'>{{PreviousVc}}</td>
                                    <td align='right' style='color:#64748b;'>{{Vc}}</td>
                                    <td align='right' style='font-weight:600;color:#64748b;'>{{ConsumtionVc}}</td>
                                </tr>
                            </table>
                        </td>
                    </tr>

                    <!-- ======================================================== -->
                    <!-- [KHỐI 4] THÔNG SỐ VẬN HÀNH PHỤ TẢI (PMAX & COS PHI)     -->
                    <!-- ======================================================== -->
                    <tr>
                        <td style='padding:0 30px 15px 30px;'>
                            <table width='100%' cellpadding='8' cellspacing='0' border='0' style='background-color:#eff6ff;border:1px solid #bfdbfe;border-radius:6px;font-size:13px;color:#1e40af;'>
                                <tr>
                                    <td width='50%'>
                                        • Công suất cực đại <strong>Pmax</strong>: <strong>{{Pmax}}</strong><br>
                                        • Thời điểm xuất hiện Pmax: <strong>{{PmaxTime}}</strong>
                                    </td>
                                    <td width='50%' style='vertical-align:top;'>
                                        • Hệ số công suất <strong>Cos &phi;</strong>: <strong>{{CosPhi}}</strong><br>
                                        • Tình trạng chốt: <span style='color:#16a34a;font-weight:600;'>Đã xác nhận dữ liệu</span>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>

                    <!-- ======================================================== -->
                    <!-- [KHỐI 5] THÔNG BÁO TỆP ĐÍNH KÈM PHIẾU BÁO (PDF)          -->
                    <!-- ======================================================== -->
                    <tr>
                        <td style='padding:0 30px 15px 30px;'>
                            <div style='background-color:#f8fafc;border-left:4px solid #b22222;padding:12px 16px;border-radius:4px;font-size:13px;color:#334155;line-height:1.6;'>
                                📎 <strong>Tệp đính kèm:</strong> Chi tiết phiếu báo chỉ số điện năng định dạng PDF đã được đính kèm theo thư này: <strong>PhieuBaoSo_{{SerialNumber}}.pdf</strong>. Quý khách có thể tải về và lưu trữ để phục vụ đối soát và thanh toán.
                            </div>
                        </td>
                    </tr>

                    <!-- ======================================================== -->
                    <!-- [KHỐI 6] THÔNG TIN LIÊN HỆ ĐỐI SOÁT & CHĂM SÓC KHÁCH HÀNG -->
                    <!-- Quản trị viên sửa hotline, email hỗ trợ tại đây           -->
                    <!-- ======================================================== -->
                    <tr>
                        <td style='padding:0 30px 24px 30px;'>
                            <div style='background-color:#fafaf9;border:1px dashed #d6d3d1;padding:12px 16px;border-radius:6px;font-size:12.5px;color:#57534e;line-height:1.6;'>
                                Mọi thắc mắc hoặc yêu cầu đối soát số liệu đo đếm, Quý khách vui lòng liên hệ: <strong>{{CareFullname}}</strong><br>
                                📞 Hotline CSKH: <strong>{{CarePhone}}</strong> | ✉️ Email tiếp nhận: <strong>{{CareEmail}}</strong>
                            </div>
                            <p style='margin:16px 0 0 0;font-size:13.5px;color:#334155;'>Trân trọng cảm ơn sự hợp tác của Quý khách hàng!</p>
                        </td>
                    </tr>

                    <!-- ======================================================== -->
                    <!-- [KHỐI 7] CHÂN TRANG HỆ THỐNG                             -->
                    <!-- ======================================================== -->
                    <tr>
                        <td style='background-color:#f8fafc;border-top:1px solid #e2e8f0;padding:16px;text-align:center;font-size:12px;color:#94a3b8;'>
                            Email tự động phát hành từ Hệ thống Quản trị Đo đếm Điện tử MDMS. Vui lòng không phản hồi trực tiếp vào địa chỉ thư này.
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>`,
  },
  {
    id: 'stmt-modern',
    code: 'STATEMENT_NOTIFICATION',
    name: 'Phiếu Báo Điện Năng - Hiện Đại Tinh Gọn',
    tag: 'Giao diện Card',
    badgeColor: '#1890ff',
    description: 'Mẫu thiết kế phong cách thẻ card hiện đại, làm nổi bật tổng điện năng tiêu thụ và bảng biểu giá 3 pha trực quan.',
    subjectTemplate: 'Thông báo sản lượng điện năng tiêu thụ {{Period}} - {{CustomerName}}',
    bodyHtmlTemplate: `<html>
<head><meta charset='utf-8'></head>
<body style='margin:0;padding:0;background-color:#f1f5f9;font-family:-apple-system,BlinkMacSystemFont,"Segoe UI",Roboto,sans-serif;color:#1e293b;'>
    <table width='100%' cellpadding='0' cellspacing='0' border='0' style='padding:32px 16px;'>
        <tr>
            <td align='center'>
                <table width='100%' cellpadding='0' cellspacing='0' border='0' style='max-width:600px;background:#ffffff;border-radius:16px;overflow:hidden;box-shadow:0 10px 25px -5px rgba(0,0,0,0.05);border:1px solid #e2e8f0;'>
                    <tr>
                        <td style='background:#0f172a;padding:28px 32px;'>
                            <span style='background:#b22222;color:#ffffff;font-size:11px;font-weight:700;padding:4px 10px;border-radius:20px;text-transform:uppercase;'>BÁO CÁO ĐIỆN NĂNG</span>
                            <h2 style='margin:12px 0 4px 0;color:#ffffff;font-size:20px;'>{{CustomerName}}</h2>
                            <p style='margin:0;color:#94a3b8;font-size:13px;'>Kỳ ghi chỉ số: {{Period}} (Điểm đo: {{PointCode}})</p>
                        </td>
                    </tr>
                    <tr>
                        <td style='padding:32px;'>
                            <div style='background:#f8fafc;border-radius:12px;padding:20px;text-align:center;margin-bottom:24px;border:1px solid #e2e8f0;'>
                                <div style='color:#64748b;font-size:13px;font-weight:600;text-transform:uppercase;'>Tổng Điện Năng Hữu Công Tiêu Thụ</div>
                                <div style='color:#b22222;font-size:36px;font-weight:800;margin:8px 0;'>{{ConsumtionTotal}} <span style='font-size:18px;font-weight:500;color:#64748b;'>kWh</span></div>
                                <div style='font-size:12.5px;color:#64748b;'>Từ ngày {{FromDate}} đến {{ToDate}}</div>
                            </div>
                            <table width='100%' cellpadding='10' cellspacing='0' border='0' style='margin-bottom:24px;font-size:13px;'>
                                <tr style='border-bottom:1px solid #e2e8f0;'>
                                    <td style='color:#475569;'>• Bình thường (BT):</td>
                                    <td align='right' style='font-weight:700;color:#0f172a;'>{{ConsumtionBt}} kWh</td>
                                </tr>
                                <tr style='border-bottom:1px solid #e2e8f0;'>
                                    <td style='color:#475569;'>• Cao điểm (CD):</td>
                                    <td align='right' style='font-weight:700;color:#0f172a;'>{{ConsumtionCd}} kWh</td>
                                </tr>
                                <tr style='border-bottom:1px solid #e2e8f0;'>
                                    <td style='color:#475569;'>• Thấp điểm (TD):</td>
                                    <td align='right' style='font-weight:700;color:#0f172a;'>{{ConsumtionTd}} kWh</td>
                                </tr>
                                <tr>
                                    <td style='color:#475569;'>• Công suất cực đại Pmax:</td>
                                    <td align='right' style='font-weight:700;color:#0f172a;'>{{Pmax}}</td>
                                </tr>
                            </table>
                            <div style='background:#f1f5f9;border-radius:8px;padding:14px;font-size:12.5px;color:#475569;line-height:1.6;'>
                                Chi tiết bảng kê chỉ số công tơ được đính kèm trong tệp <strong>PhieuBaoSo_{{SerialNumber}}.pdf</strong>. Hotline giải đáp: <strong>{{SupportPhone}}</strong>.
                            </div>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>`,
  },
  {
    id: 'user-cred',
    code: 'USER_CREDENTIAL',
    name: 'Cấp Tài Khoản & Mật Khẩu Khởi Tạo (Phân Khối Khoa Học)',
    tag: 'Bảo mật cao',
    badgeColor: '#52c41a',
    description: 'Mẫu thông báo cấp tài khoản người dùng mới với cấu trúc phân khối rõ ràng, nút đăng nhập nhanh và khuyến cáo an toàn.',
    subjectTemplate: '[MDMS] Thông tin cấp tài khoản người dùng hệ thống - {{FullName}}',
    bodyHtmlTemplate: `<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Thông Tin Cấp Tài Khoản</title>
</head>
<body style='margin:0;padding:0;background-color:#f5f7fc;font-family:-apple-system,BlinkMacSystemFont,"Segoe UI",Roboto,Helvetica,Arial,sans-serif;color:#334155;'>
    <table width='100%' cellpadding='0' cellspacing='0' border='0' style='padding:40px 15px;'>
        <tr>
            <td align='center'>
                <table width='100%' cellpadding='0' cellspacing='0' border='0' style='max-width:600px;background:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 4px 16px rgba(0,0,0,0.06);border:1px solid #e2e8f0;'>
                    
                    <!-- ======================================================== -->
                    <!-- [KHỐI 1] HEADER: TÊN CÔNG TY & HỆ THỐNG ĐO ĐẾM (MDMS)   -->
                    <!-- Quản trị viên sửa tên công ty / hệ thống tại đây         -->
                    <!-- ======================================================== -->
                    <tr>
                        <td style='background:linear-gradient(135deg,#b22222 0%,#8b1a1a 100%);padding:28px 32px;text-align:center;'>
                            <h2 style='margin:0;color:#ffffff;font-size:20px;font-weight:700;'>{{CompanyName}}</h2>
                            <p style='margin:6px 0 0 0;color:#fecaca;font-size:13px;letter-spacing:0.5px;'>HỆ THỐNG QUẢN LÝ DỮ LIỆU ĐO ĐẾM ĐIỆN TỬ (MDMS)</p>
                            <p style='margin:4px 0 0 0;color:#ffffff;font-size:14px;font-weight:600;'>THÔNG TIN CẤP TÀI KHOẢN NGƯỜI DÙNG</p>
                        </td>
                    </tr>

                    <!-- ======================================================== -->
                    <!-- [KHỐI 2] THÔNG TIN TÀI KHOẢN ĐĂNG NHẬP                   -->
                    <!-- ======================================================== -->
                    <tr>
                        <td style='padding:32px 32px 20px 32px;'>
                            <p style='margin:0 0 14px 0;font-size:15px;color:#0f172a;'>Kính gửi Quý người dùng: <strong>{{FullName}}</strong>,</p>
                            <p style='margin:0 0 20px 0;font-size:14px;line-height:1.7;color:#475569;'>
                                Tài khoản truy cập Hệ thống Quản trị Dữ liệu Đo đếm Điện tử (MDMS) của Quý vị đã được cấp thành công. Thông tin tài khoản đăng nhập chi tiết như sau:
                            </p>
                            <table width='100%' cellpadding='12' cellspacing='0' border='0' style='background-color:#f8fafc;border-radius:8px;border:1px solid #e2e8f0;margin-bottom:24px;font-size:14px;'>
                                <tr>
                                    <td width='35%' style='color:#64748b;font-weight:500;'>Tên đăng nhập:</td>
                                    <td width='65%' style='color:#0f172a;font-weight:700;font-family:monospace;font-size:15px;'>{{Username}}</td>
                                </tr>
                                <tr>
                                    <td style='color:#64748b;font-weight:500;'>Mật khẩu khởi tạo:</td>
                                    <td style='color:#b22222;font-weight:700;font-family:monospace;font-size:15px;'>{{Password}}</td>
                                </tr>
                                <tr>
                                    <td style='color:#64748b;font-weight:500;'>Đơn vị liên kết:</td>
                                    <td style='color:#0f172a;font-weight:600;'>{{CustomerName}}</td>
                                </tr>
                            </table>

                            <!-- ==================================================== -->
                            <!-- [KHỐI 3] ĐƯỜNG DẪN TRUY CẬP HỆ THỐNG (NÚT BẤM CTA)  -->
                            <!-- ==================================================== -->
                            <table width='100%' cellpadding='0' cellspacing='0' border='0' style='margin:24px 0;'>
                                <tr>
                                    <td align='center'>
                                        <a href='{{LoginUrl}}' target='_blank' style='display:inline-block;background-color:#b22222;color:#ffffff;font-size:14px;font-weight:600;text-decoration:none;padding:13px 36px;border-radius:6px;box-shadow:0 2px 8px rgba(178,34,34,0.3);'>
                                            Đăng Nhập Vào Hệ Thống
                                        </a>
                                    </td>
                                </tr>
                            </table>

                            <!-- ==================================================== -->
                            <!-- [KHỐI 4] KHUYẾN CÁO AN TOÀN & ĐỔI MẬT KHẨU LẦN ĐẦU   -->
                            <!-- ==================================================== -->
                            <div style='background-color:#fffbeb;border-left:4px solid #f59e0b;padding:12px 16px;margin-bottom:20px;border-radius:4px;'>
                                <p style='margin:0;font-size:13px;color:#92400e;line-height:1.5;'>
                                    ⚠️ <strong>Khuyến cáo an toàn:</strong> Vì lý do bảo mật thông tin, Quý vị vui lòng thực hiện đổi mật khẩu ngay trong lần đăng nhập đầu tiên và tuyệt đối không chia sẻ mật khẩu này cho bất kỳ ai.
                                </p>
                            </div>

                            <!-- ==================================================== -->
                            <!-- [KHỐI 5] THÔNG TIN HỖ TRỢ KỸ THUẬT                  -->
                            <!-- Quản trị viên sửa hotline, email hỗ trợ tại đây      -->
                            <!-- ==================================================== -->
                            <div style='background-color:#fafaf9;border:1px dashed #d6d3d1;padding:12px 16px;border-radius:6px;font-size:12.5px;color:#57534e;line-height:1.6;'>
                                Nếu Quý vị cần hỗ trợ kỹ thuật hoặc cấp lại quyền truy cập, vui lòng liên hệ: <strong>{{CareFullname}}</strong><br>
                                📞 Hotline hỗ trợ: <strong>{{SupportPhone}}</strong> | ✉️ Email kỹ thuật: <strong>{{SystemEmail}}</strong>
                            </div>
                        </td>
                    </tr>

                    <!-- ======================================================== -->
                    <!-- [KHỐI 6] CHÂN TRANG HỆ THỐNG                             -->
                    <!-- ======================================================== -->
                    <tr>
                        <td style='background-color:#f8fafc;border-top:1px solid #e2e8f0;padding:16px;text-align:center;font-size:12px;color:#94a3b8;'>
                            Email tự động phát hành từ Hệ thống Quản trị Đo đếm Điện tử MDMS.
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>`,
  },
  {
    id: 'forgot-pass',
    code: 'FORGOT_PASSWORD',
    name: 'Yêu Cầu Thiết Lập Lại Mật Khẩu (Phân Khối Khoa Học)',
    tag: 'Bảo mật OTP',
    badgeColor: '#faad14',
    description: 'Mẫu gửi liên kết bảo mật để người dùng tự đặt lại mật khẩu với giới hạn thời gian 10 phút và lưu ý an toàn.',
    subjectTemplate: 'Yêu cầu đặt lại mật khẩu - Hệ thống MDMS',
    bodyHtmlTemplate: `<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Yêu Cầu Đặt Lại Mật Khẩu</title>
</head>
<body style='margin:0;padding:0;background-color:#f5f7fc;font-family:-apple-system,BlinkMacSystemFont,"Segoe UI",Roboto,Helvetica,Arial,sans-serif;color:#334155;'>
    <table width='100%' cellpadding='0' cellspacing='0' border='0' style='padding:40px 15px;'>
        <tr>
            <td align='center'>
                <table width='100%' cellpadding='0' cellspacing='0' border='0' style='max-width:580px;background:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 4px 16px rgba(0,0,0,0.06);border:1px solid #e2e8f0;'>
                    
                    <!-- ======================================================== -->
                    <!-- [KHỐI 1] HEADER: TÊN CÔNG TY & HỆ THỐNG ĐO ĐẾM (MDMS)   -->
                    <!-- Quản trị viên sửa tên công ty / hệ thống tại đây         -->
                    <!-- ======================================================== -->
                    <tr>
                        <td style='background:linear-gradient(135deg,#b22222 0%,#8b1a1a 100%);padding:28px 32px;text-align:center;'>
                            <h2 style='margin:0;color:#ffffff;font-size:20px;font-weight:700;'>{{CompanyName}}</h2>
                            <p style='margin:6px 0 0 0;color:#fecaca;font-size:13px;letter-spacing:0.5px;'>HỆ THỐNG QUẢN LÝ DỮ LIỆU ĐO ĐẾM ĐIỆN TỬ (MDMS)</p>
                            <p style='margin:4px 0 0 0;color:#ffffff;font-size:14px;font-weight:600;'>YÊU CẦU ĐẶT LẠI MẬT KHẨU</p>
                        </td>
                    </tr>

                    <!-- ======================================================== -->
                    <!-- [KHỐI 2] THÔNG TIN YÊU CẦU ĐẶT LẠI MẬT KHẨU             -->
                    <!-- ======================================================== -->
                    <tr>
                        <td style='padding:32px 32px 20px 32px;'>
                            <p style='margin:0 0 14px 0;font-size:15px;color:#0f172a;'>Kính gửi <strong>{{Username}}</strong>,</p>
                            <p style='margin:0 0 20px 0;font-size:14px;line-height:1.7;color:#475569;'>
                                Chúng tôi nhận được yêu cầu thiết lập lại mật khẩu đăng nhập cho tài khoản liên kết với địa chỉ email này trên hệ thống MDMS. Để hoàn tất quy trình, Quý vị vui lòng nhấp vào nút thao tác bên dưới:
                            </p>

                            <!-- ==================================================== -->
                            <!-- [KHỐI 3] LIÊN KẾT ĐẶT LẠI MẬT KHẨU (NÚT BẤM CTA)     -->
                            <!-- ==================================================== -->
                            <table width='100%' cellpadding='0' cellspacing='0' border='0' style='margin:24px 0;'>
                                <tr>
                                    <td align='center'>
                                        <a href='{{ResetLink}}' target='_blank' style='display:inline-block;background-color:#b22222;color:#ffffff;font-size:14px;font-weight:600;text-decoration:none;padding:13px 36px;border-radius:6px;box-shadow:0 2px 8px rgba(178,34,34,0.3);'>
                                            Thiết Lập Lại Mật Khẩu
                                        </a>
                                    </td>
                                </tr>
                            </table>

                            <!-- ==================================================== -->
                            <!-- [KHỐI 4] CẢNH BÁO THỜI HẠN VÀ KHUYẾN CÁO AN TOÀN    -->
                            <!-- ==================================================== -->
                            <div style='background-color:#fffbeb;border-left:4px solid #f59e0b;padding:12px 16px;margin-bottom:20px;border-radius:4px;'>
                                <p style='margin:0;font-size:12.5px;color:#92400e;line-height:1.5;'>
                                    ⏱️ <strong>Lưu ý bảo mật:</strong> Liên kết đặt lại mật khẩu có hiệu lực trong vòng <strong>10 phút</strong> kể từ khi phát hành. Nếu Quý vị không gửi yêu cầu này, vui lòng bỏ qua thư hoặc thông báo ngay cho quản trị viên.
                                </p>
                            </div>

                            <!-- ==================================================== -->
                            <!-- [KHỐI 5] THÔNG TIN HỖ TRỢ KỸ THUẬT                  -->
                            <!-- Quản trị viên sửa hotline, email hỗ trợ tại đây      -->
                            <!-- ==================================================== -->
                            <div style='background-color:#fafaf9;border:1px dashed #d6d3d1;padding:12px 16px;border-radius:6px;font-size:12.5px;color:#57534e;line-height:1.6;'>
                                Hotline hỗ trợ: <strong>{{SupportPhone}}</strong> | Email tiếp nhận: <strong>{{SystemEmail}}</strong>
                            </div>
                        </td>
                    </tr>

                    <!-- ======================================================== -->
                    <!-- [KHỐI 6] CHÂN TRANG HỆ THỐNG                             -->
                    <!-- ======================================================== -->
                    <tr>
                        <td style='background-color:#f8fafc;border-top:1px solid #e2e8f0;padding:16px;text-align:center;font-size:12px;color:#94a3b8;'>
                            Email tự động phát hành từ Hệ thống Quản trị Đo đếm Điện tử MDMS.
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>`,
  },
  {
    id: 'alert-anomaly',
    code: 'STATEMENT_NOTIFICATION',
    name: 'Cảnh Báo Phụ Tải Bất Thường / Sự Cố Điểm Đo',
    tag: 'Cảnh báo phụ tải',
    badgeColor: '#cf1322',
    description: 'Mẫu thông báo khẩn cấp khi điểm đo có hiện tượng quá tải Pmax, mất tín hiệu AMR hoặc tiêu thụ vượt định mức.',
    subjectTemplate: '[Cảnh Báo] Bất thường phụ tải tại điểm đo {{PointName}} - {{CustomerName}}',
    bodyHtmlTemplate: `<html>
<head><meta charset='utf-8'></head>
<body style='margin:0;padding:0;background-color:#fef2f2;font-family:-apple-system,BlinkMacSystemFont,"Segoe UI",Roboto,sans-serif;color:#1e293b;'>
    <table width='100%' cellpadding='0' cellspacing='0' border='0' style='padding:32px 16px;'>
        <tr>
            <td align='center'>
                <table width='100%' cellpadding='0' cellspacing='0' border='0' style='max-width:580px;background:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 4px 16px rgba(185,28,28,0.1);border:2px solid #ef4444;'>
                    <tr>
                        <td style='background:#b91c1c;padding:24px 30px;color:#ffffff;'>
                            <h2 style='margin:0;font-size:18px;font-weight:700;'>⚠️ CẢNH BÁO BẤT THƯỜNG DỮ LIỆU ĐO ĐẾM</h2>
                            <p style='margin:6px 0 0 0;font-size:13px;color:#fee2e2;'>Hệ thống giám sát tự động phát hiện thông số vận hành vượt ngưỡng</p>
                        </td>
                    </tr>
                    <tr>
                        <td style='padding:28px 30px;'>
                            <p style='margin:0 0 16px 0;font-size:15px;'>Kính gửi Bộ phận Kỹ thuật <strong>{{CustomerName}}</strong>,</p>
                            <p style='margin:0 0 20px 0;font-size:14px;line-height:1.6;color:#475569;'>
                                Hệ thống đo đếm từ xa AMR/MDMS ghi nhận điểm đo <strong>{{PointCode}} ({{PointName}})</strong> có biến động phụ tải đáng chú ý trong kỳ theo dõi:
                            </p>
                            <div style='background:#fef2f2;border:1px solid #fecaca;border-radius:8px;padding:16px;margin-bottom:20px;'>
                                <p style='margin:0 0 8px 0;font-size:13px;color:#991b1b;'>• <strong>Công suất Pmax ghi nhận:</strong> {{Pmax}} (vào lúc {{PmaxTime}})</p>
                                <p style='margin:0 0 8px 0;font-size:13px;color:#991b1b;'>• <strong>Hệ số công suất Cos φ:</strong> {{CosPhi}}</p>
                                <p style='margin:0;font-size:13px;color:#991b1b;'>• <strong>Tổng sản lượng tạm tính:</strong> {{ConsumtionTotal}} kWh</p>
                            </div>
                            <p style='font-size:13px;color:#475569;line-height:1.6;'>
                                Đề nghị Quý đơn vị kiểm tra hệ thống tụ bù và thiết bị điện tại trạm. Mọi yêu cầu hỗ trợ kỹ thuật xin liên hệ Trung tâm Điều hành: <strong>{{SupportPhone}}</strong>.
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>`,
  },
];

// Helper to render HTML with realistic mock data for live visual preview
const renderWithMockData = (template: string, subject: string) => {
  const mock: Record<string, string> = {
    CustomerName: 'CÔNG TY TNHH KYOCERA VIỆT NAM',
    PointCode: '101.402.001',
    PointName: 'Điểm đo TBA 1 - Kyocera',
    CustomerCode: 'KH_KYOCERA_01',
    MeterSerial: '23098172',
    Multiplier: '120',
    CompanyName: 'CÔNG TY TNHH VSIP HẢI PHÒNG',
    Period: 'Kỳ 1 Tháng 10/2026',
    CycleName: 'Kỳ 1 Tháng 10/2026',
    FromDate: '01/10/2026',
    ToDate: '31/10/2026',
    Month: '10',
    Year: '2026',
    PreviousBt: '1,245,670',
    Bt: '1,295,430',
    ConsumtionBt: '49,760',
    PreviousCd: '345,120',
    Cd: '362,450',
    ConsumtionCd: '17,330',
    PreviousTd: '560,890',
    Td: '581,200',
    ConsumtionTd: '20,310',
    PreviousSg: '2,151,680',
    Sg: '2,239,080',
    ConsumtionTotal: '87,400',
    ConsumtionSg: '87,400',
    Pmax: '1,450.85 kW',
    PmaxTime: '15/10/2026 14:30:00',
    CosPhi: '0.98',
    SerialNumber: 'PB-202610-001',
    FullName: 'Nguyễn Văn An',
    Username: 'an.nguyen',
    Password: 'Vsip@2026#',
    LoginUrl: 'https://amr.nobisoft.vn/login',
    ResetLink: 'https://amr.nobisoft.vn/reset-password?token=sample_demo_token',
    CareFullname: 'Phòng Vận Hành Cơ Điện VSIP',
    CareEmail: 'noreply.mdms@vsip.com.vn',
    CarePhone: '0225 3959868',
    SystemEmail: 'noreply.mdms@vsip.com.vn',
    SupportPhone: '0225 3959868',
  };

  let renderedSubj = subject || '';
  let renderedHtml = template || '';

  Object.keys(mock).forEach((k) => {
    const reg = new RegExp(`\\{\\{\\s*${k}\\s*\\}\\}`, 'gi');
    renderedSubj = renderedSubj.replace(reg, mock[k]);
    renderedHtml = renderedHtml.replace(reg, mock[k]);
  });

  return { renderedSubj, renderedHtml };
};

export const NotificationTemplatesPage: React.FC = () => {
  const service = new TemplateManagementService();
  const [loading, setLoading] = useState<boolean>(false);
  const [templates, setTemplates] = useState<NotificationTemplateDto[]>([]);
  const [activeTab, setActiveTab] = useState<string>('ALL');

  // Drawer Edit/Create
  const [drawerOpen, setDrawerOpen] = useState<boolean>(false);
  const [editingTemplate, setEditingTemplate] = useState<NotificationTemplateDto | null>(null);
  const [form] = Form.useForm();
  const [submitting, setSubmitting] = useState<boolean>(false);
  const [editorMode, setEditorMode] = useState<'split' | 'code'>('split');
  const [liveHtml, setLiveHtml] = useState<string>('');
  const [liveSubject, setLiveSubject] = useState<string>('');

  // Pre-built Library Modal
  const [libraryModalOpen, setLibraryModalOpen] = useState<boolean>(false);
  const [libraryTab, setLibraryTab] = useState<string>('ALL');

  // Preview Modal
  const [previewOpen, setPreviewOpen] = useState<boolean>(false);
  const [previewData, setPreviewData] = useState<PreviewNotificationTemplateResponse | null>(null);
  const [previewLoading, setPreviewLoading] = useState<boolean>(false);

  // Test Email Sending State
  const [testSendOpen, setTestSendOpen] = useState<boolean>(false);
  const [testEmailTarget, setTestEmailTarget] = useState<string>('');
  const [testSending, setTestSending] = useState<boolean>(false);
  const [testSendPayload, setTestSendPayload] = useState<{
    code?: string;
    subjectTemplate?: string;
    bodyHtmlTemplate?: string;
  } | null>(null);

  const fetchTemplates = async (codeFilter?: string) => {
    setLoading(true);
    try {
      const code = codeFilter === 'ALL' ? undefined : codeFilter;
      const res: any = await service.getNotificationTemplates(code);
      const rawData = res?.data?.data !== undefined ? res.data.data : res?.data;
      setTemplates(Array.isArray(rawData) ? rawData : []);
    } catch (err: any) {
      message.error(`Không thể tải danh sách mẫu email: ${err?.message || err}`);
      setTemplates([]);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchTemplates(activeTab);
  }, [activeTab]);

  const handleOpenCreate = () => {
    setEditingTemplate(null);
    form.resetFields();
    const defaultLib = PREBUILT_TEMPLATES_LIBRARY[0];
    form.setFieldsValue({
      code: activeTab !== 'ALL' ? activeTab : 'STATEMENT_NOTIFICATION',
      name: defaultLib.name,
      subjectTemplate: defaultLib.subjectTemplate,
      bodyHtmlTemplate: defaultLib.bodyHtmlTemplate,
      description: defaultLib.description,
      isDefault: false,
      isActive: true,
    });
    setLiveHtml(defaultLib.bodyHtmlTemplate);
    setLiveSubject(defaultLib.subjectTemplate);
    setDrawerOpen(true);
  };

  const handleOpenEdit = (record: NotificationTemplateDto) => {
    setEditingTemplate(record);
    form.resetFields();
    form.setFieldsValue({
      code: record.code,
      name: record.name,
      subjectTemplate: record.subjectTemplate,
      bodyHtmlTemplate: record.bodyHtmlTemplate,
      description: record.description,
      isDefault: record.isDefault,
      isActive: record.isActive,
    });
    setLiveHtml(record.bodyHtmlTemplate || '');
    setLiveSubject(record.subjectTemplate || '');
    setDrawerOpen(true);
  };

  const handleApplyPrebuilt = (tpl: PrebuiltTemplateItem) => {
    form.setFieldsValue({
      code: tpl.code,
      name: tpl.name,
      subjectTemplate: tpl.subjectTemplate,
      bodyHtmlTemplate: tpl.bodyHtmlTemplate,
      description: tpl.description,
    });
    setLiveHtml(tpl.bodyHtmlTemplate);
    setLiveSubject(tpl.subjectTemplate);
    message.success(`Đã áp dụng mẫu "${tpl.name}" vào biểu mẫu soạn thảo`);
    setLibraryModalOpen(false);
  };

  const handleResetSystemDefaults = async () => {
    setLoading(true);
    try {
      await service.resetDefaultNotificationTemplates();
      message.success('Đã khôi phục thành công các mẫu email tiêu chuẩn của hệ thống');
      fetchTemplates(activeTab);
    } catch (err: any) {
      message.error(`Không thể khôi phục mẫu chuẩn: ${err?.message || err}`);
    } finally {
      setLoading(false);
    }
  };

  // Quick Block Inserters
  const insertContentBlock = (blockHtml: string, blockName: string) => {
    const currentBody = form.getFieldValue('bodyHtmlTemplate') || '';
    const newBody = currentBody + '\n\n' + blockHtml;
    form.setFieldsValue({ bodyHtmlTemplate: newBody });
    setLiveHtml(newBody);
    message.success(`Đã chèn "${blockName}" vào nội dung email`);
  };

  const insertTagToBody = (tag: string) => {
    const currentBody = form.getFieldValue('bodyHtmlTemplate') || '';
    const newBody = currentBody + tag;
    form.setFieldsValue({ bodyHtmlTemplate: newBody });
    setLiveHtml(newBody);
    navigator.clipboard?.writeText(tag);
    message.success(`Đã chèn thẻ biến ${tag}`);
  };

  const handleSetDefault = async (record: NotificationTemplateDto) => {
    try {
      await service.setDefaultNotificationTemplate(record.id);
      message.success(`Đã thiết lập mẫu "${record.name}" làm mặc định cho nghiệp vụ ${record.code}`);
      fetchTemplates(activeTab);
    } catch (err: any) {
      message.error(`Không thể đặt mặc định: ${err?.message || err}`);
    }
  };

  const handleDelete = async (record: NotificationTemplateDto) => {
    try {
      await service.deleteNotificationTemplate(record.id);
      message.success(`Đã xóa mẫu email "${record.name}"`);
      fetchTemplates(activeTab);
    } catch (err: any) {
      message.error(`Không thể xóa mẫu email: ${err?.message || err}`);
    }
  };

  const handleSubmit = async () => {
    try {
      const values = await form.validateFields();
      setSubmitting(true);

      if (editingTemplate) {
        const updateData: UpdateNotificationTemplateRequest = {
          name: values.name,
          subjectTemplate: values.subjectTemplate,
          bodyHtmlTemplate: values.bodyHtmlTemplate,
          description: values.description,
          isDefault: values.isDefault,
          isActive: values.isActive,
        };
        await service.updateNotificationTemplate(editingTemplate.id, updateData);
        message.success('Cập nhật mẫu email thành công');
      } else {
        const createData: CreateNotificationTemplateRequest = {
          code: values.code,
          name: values.name,
          subjectTemplate: values.subjectTemplate,
          bodyHtmlTemplate: values.bodyHtmlTemplate,
          description: values.description,
          isDefault: values.isDefault,
          isActive: values.isActive,
        };
        await service.createNotificationTemplate(createData);
        message.success('Tạo mới mẫu email thành công');
      }

      setDrawerOpen(false);
      fetchTemplates(activeTab);
    } catch (err: any) {
      message.error(`Lỗi khi lưu mẫu email: ${err?.message || err}`);
    } finally {
      setSubmitting(false);
    }
  };

  const handlePreviewDirect = async (record: NotificationTemplateDto) => {
    setPreviewLoading(true);
    setPreviewOpen(true);
    try {
      const res: any = await service.previewNotificationTemplate({
        code: record.code,
        subjectTemplate: record.subjectTemplate,
        bodyHtmlTemplate: record.bodyHtmlTemplate,
      });
      const data = res?.data?.data !== undefined ? res.data.data : res?.data;
      if (data) {
        setPreviewData({
          subject: data.subject || data.Subject || '',
          bodyHtml: data.bodyHtml || data.BodyHtml || '',
        });
      }
    } catch (err: any) {
      message.error(`Không thể xem trước mẫu: ${err?.message || err}`);
    } finally {
      setPreviewLoading(false);
    }
  };

  const handlePreviewFromForm = async () => {
    try {
      const values = form.getFieldsValue();
      setPreviewLoading(true);
      setPreviewOpen(true);
      const res: any = await service.previewNotificationTemplate({
        code: values.code,
        subjectTemplate: values.subjectTemplate || '',
        bodyHtmlTemplate: values.bodyHtmlTemplate || '',
      });
      const data = res?.data?.data !== undefined ? res.data.data : res?.data;
      if (data) {
        setPreviewData({
          subject: data.subject || data.Subject || '',
          bodyHtml: data.bodyHtml || data.BodyHtml || '',
        });
      }
    } catch (err: any) {
      message.error(`Không thể xem trước mẫu: ${err?.message || err}`);
    } finally {
      setPreviewLoading(false);
    }
  };

  const handleOpenTestSendFromRecord = (record: NotificationTemplateDto) => {
    setTestSendPayload({
      code: record.code,
      subjectTemplate: record.subjectTemplate,
      bodyHtmlTemplate: record.bodyHtmlTemplate,
    });
    setTestSendOpen(true);
  };

  const handleOpenTestSendFromForm = () => {
    const values = form.getFieldsValue();
    setTestSendPayload({
      code: values.code,
      subjectTemplate: values.subjectTemplate || '',
      bodyHtmlTemplate: values.bodyHtmlTemplate || '',
    });
    setTestSendOpen(true);
  };

  const handleSendTest = async () => {
    if (!testEmailTarget || !testEmailTarget.includes('@')) {
      message.warning('Vui lòng nhập địa chỉ email hợp lệ');
      return;
    }
    if (!testSendPayload) return;
    setTestSending(true);
    try {
      const res: any = await service.testSendNotificationTemplate({
        code: testSendPayload.code,
        toEmail: testEmailTarget.trim(),
        subjectTemplate: testSendPayload.subjectTemplate,
        bodyHtmlTemplate: testSendPayload.bodyHtmlTemplate,
      });
      const data = res?.data?.data !== undefined ? res.data.data : res?.data;
      if (res?.status === 200 || data === true || res?.success) {
        message.success(`Đã gửi email thử nghiệm thành công tới ${testEmailTarget}`);
        setTestSendOpen(false);
      } else {
        message.error('Không thể gửi email thử nghiệm, vui lòng kiểm tra cấu hình Email');
      }
    } catch (err: any) {
      message.error(`Lỗi khi gửi email thử nghiệm: ${err?.message || err}`);
    } finally {
      setTestSending(false);
    }
  };

  // Render mock data for live split preview
  const livePreview = renderWithMockData(liveHtml, liveSubject);

  const columns: ColumnsType<NotificationTemplateDto> = [
    {
      title: 'Tên mẫu',
      dataIndex: 'name',
      key: 'name',
      render: (name, record) => (
        <Space direction="vertical" size={2}>
          <Space>
            <Text strong className='text-gray-800'>{name}</Text>
            {record.isDefault && (
              <Tag color="gold" icon={<StarFilled />}>
                Mặc định
              </Tag>
            )}
          </Space>
          {record.description && <Text type="secondary" style={{ fontSize: 12 }}>{record.description}</Text>}
        </Space>
      ),
    },
    {
      title: 'Nghiệp vụ (Code)',
      dataIndex: 'code',
      key: 'code',
      width: 220,
      render: (code) => {
        let color = 'blue';
        let label = code;
        if (code === 'STATEMENT_NOTIFICATION') {
          color = 'magenta';
          label = 'Phiếu báo điện năng';
        } else if (code === 'FORGOT_PASSWORD') {
          color = 'orange';
          label = 'Đặt lại mật khẩu';
        } else if (code === 'USER_CREDENTIAL') {
          color = 'green';
          label = 'Cấp tài khoản mới';
        }
        return (
          <Space direction="vertical" size={1}>
            <Tag color={color}>{label}</Tag>
            <Text code style={{ fontSize: 11 }}>{code}</Text>
          </Space>
        );
      },
    },
    {
      title: 'Tiêu đề Email (Subject)',
      dataIndex: 'subjectTemplate',
      key: 'subjectTemplate',
      ellipsis: true,
      render: (text) => <Text italic className='text-gray-600'>{text}</Text>,
    },
    {
      title: 'Trạng thái',
      dataIndex: 'isActive',
      key: 'isActive',
      width: 110,
      align: 'center',
      render: (active) =>
        active ? <Tag color="success">Kích hoạt</Tag> : <Tag color="default">Vô hiệu</Tag>,
    },
    {
      title: 'Thao tác',
      key: 'action',
      width: 220,
      align: 'center',
      render: (_, record) => (
        <Space size="small">
          <Tooltip title="Xem trước nội dung đã bind dữ liệu">
            <Button
              type="text"
              icon={<EyeOutlined style={{ color: '#1890ff', fontSize: 16 }} />}
              onClick={() => handlePreviewDirect(record)}
            />
          </Tooltip>
          <Tooltip title="Gửi email thử nghiệm">
            <Button
              type="text"
              icon={<SendOutlined style={{ color: '#722ed1', fontSize: 16 }} />}
              onClick={() => handleOpenTestSendFromRecord(record)}
            />
          </Tooltip>
          <Tooltip title="Chỉnh sửa mẫu">
            <Button
              type="text"
              icon={<EditOutlined style={{ color: '#52c41a', fontSize: 16 }} />}
              onClick={() => handleOpenEdit(record)}
            />
          </Tooltip>
          {!record.isDefault && (
            <Tooltip title="Đặt làm mẫu mặc định">
              <Button
                type="text"
                icon={<StarOutlined style={{ color: '#faad14', fontSize: 16 }} />}
                onClick={() => handleSetDefault(record)}
              />
            </Tooltip>
          )}
          {!record.isDefault && (
            <Popconfirm
              title="Xóa mẫu email"
              description={`Bạn có chắc chắn muốn xóa mẫu "${record.name}" không?`}
              onConfirm={() => handleDelete(record)}
              okText="Xóa"
              cancelText="Hủy"
              okButtonProps={{ danger: true }}
            >
              <Tooltip title="Xóa mẫu">
                <Button type="text" danger icon={<DeleteOutlined style={{ fontSize: 16 }} />} />
              </Tooltip>
            </Popconfirm>
          )}
        </Space>
      ),
    },
  ];

  return (
    <div>
      <PageTitle title={'Cài đặt hệ thống'} />
      <div className='flex justify-between items-center'>
        <BreadcrumbUI items={[{ name: 'Cài đặt hệ thống' }, { name: 'Mẫu Email thông báo' }]} />
        <div className='flex items-center gap-2'>
          <Button
            icon={<AppstoreOutlined />}
            onClick={() => setLibraryModalOpen(true)}
            style={{ borderColor: '#003366', color: '#003366', fontWeight: 500 }}
          >
            Thư Viện Mẫu Tạo Sẵn
          </Button>

          <Popconfirm
            title="Khôi phục mẫu chuẩn hệ thống"
            description="Thao tác này sẽ đảm bảo các mẫu mặc định tiêu chuẩn (Phiếu báo, Cấp tài khoản, Đặt lại mật khẩu) được nạp đầy đủ vào hệ thống. Tiếp tục?"
            onConfirm={handleResetSystemDefaults}
            okText="Khôi phục"
            cancelText="Hủy"
          >
            <Button icon={<ReloadOutlined />} title="Nạp lại các mẫu email chuẩn mặc định">
              Khôi phục mẫu chuẩn
            </Button>
          </Popconfirm>

          <ButtonCommonCustomizable
            title={_t('Thêm mới')}
            tooltipText={'Tạo mẫu email mới'}
            classButton='text-button-variant'
            onClick={handleOpenCreate}
            icon={<PlusOutlined />}
          />
        </div>
      </div>

      <div className='mt-6'>
        <Card className={'shadow-custom'}>
          <div>
            <div className={'flex justify-between items-center mb-4'}>
              <div>
                <h1 className='font-bold text-20-20'>{'Mẫu Email thông báo'}</h1>
                <Text type="secondary" style={{ fontSize: 13 }}>
                  Quản lý và tùy biến giao diện email tự động gửi tới khách hàng. Hỗ trợ biên soạn trực quan, chèn khối nội dung sẵn và xem trước hiển thị thời gian thực.
                </Text>
              </div>
            </div>

            <Tabs
              activeKey={activeTab}
              onChange={(key) => setActiveTab(key)}
              items={[
                { key: 'ALL', label: 'Tất cả mẫu' },
                { key: 'STATEMENT_NOTIFICATION', label: 'Phiếu báo điện năng' },
                { key: 'USER_CREDENTIAL', label: 'Cấp tài khoản mới' },
                { key: 'FORGOT_PASSWORD', label: 'Đặt lại mật khẩu' },
              ]}
              className='mb-4'
            />

            <Table
              columns={columns}
              dataSource={templates}
              rowKey="id"
              loading={loading}
              pagination={{ pageSize: 10, showSizeChanger: true }}
            />
          </div>
        </Card>
      </div>

      <FullPageLoading isLoading={loading} />

      {/* Drawer Create / Edit with Dual-Mode & Split View */}
      <Drawer
        title={
          <div className="flex justify-between items-center pr-6">
            <Space>
              <MailOutlined style={{ color: '#003366', fontSize: 18 }} />
              <Text strong style={{ fontSize: 16 }}>
                {editingTemplate ? `Chỉnh sửa Mẫu Email: ${editingTemplate.name}` : 'Tạo Mới Mẫu Email Thông Báo'}
              </Text>
            </Space>
            <Radio.Group
              size="small"
              value={editorMode}
              onChange={(e) => setEditorMode(e.target.value)}
              buttonStyle="solid"
            >
              <Radio.Button value="split">
                <Space size={4}>
                  <EyeOutlined />
                  <span>Trực quan & Xem trước (Live Preview)</span>
                </Space>
              </Radio.Button>
              <Radio.Button value="code">
                <Space size={4}>
                  <BlockOutlined />
                  <span>Mã nguồn HTML</span>
                </Space>
              </Radio.Button>
            </Radio.Group>
          </div>
        }
        width={'92vw'}
        open={drawerOpen}
        onClose={() => setDrawerOpen(false)}
        extra={
          <Space>
            <Button
              icon={<AppstoreOutlined />}
              onClick={() => setLibraryModalOpen(true)}
              style={{ color: '#003366', borderColor: '#003366' }}
            >
              Chọn từ Thư Viện Mẫu
            </Button>
            <Button icon={<SendOutlined />} onClick={handleOpenTestSendFromForm}>
              Gửi Thử Nghiệm
            </Button>
            <Button onClick={() => setDrawerOpen(false)}>Hủy</Button>
            <Button
              type="primary"
              onClick={handleSubmit}
              loading={submitting}
              style={{ backgroundColor: '#003366', borderColor: '#003366' }}
            >
              Lưu Mẫu
            </Button>
          </Space>
        }
      >
        <Form form={form} layout="vertical">
          <Row gutter={16}>
            <Col span={10}>
              <Form.Item
                name="name"
                label="Tên mẫu email"
                rules={[{ required: true, message: 'Vui lòng nhập tên mẫu email' }]}
              >
                <Input placeholder="Ví dụ: Mẫu Thông Báo Phiếu Báo Tháng VSIP Hải Phòng" />
              </Form.Item>
            </Col>
            <Col span={6}>
              <Form.Item
                name="code"
                label="Mã nghiệp vụ"
                rules={[{ required: true, message: 'Vui lòng chọn loại nghiệp vụ' }]}
              >
                <Select disabled={!!editingTemplate} options={TEMPLATE_CODES} />
              </Form.Item>
            </Col>
            <Col span={4}>
              <Form.Item name="isDefault" label="Đặt làm mặc định" valuePropName="checked">
                <Switch />
              </Form.Item>
            </Col>
            <Col span={4}>
              <Form.Item name="isActive" label="Kích hoạt" valuePropName="checked">
                <Switch defaultChecked />
              </Form.Item>
            </Col>
          </Row>

          <Row gutter={16}>
            <Col span={16}>
              <Form.Item
                name="subjectTemplate"
                label="Tiêu đề Email (Subject)"
                rules={[{ required: true, message: 'Vui lòng nhập tiêu đề email' }]}
                tooltip="Hỗ trợ các thẻ biến như {{CustomerName}}, {{PointName}}, {{CycleName}}..."
              >
                <Input
                  placeholder="Ví dụ: [VSIP Hải Phòng] Phiếu báo chỉ số điện kỳ {{CycleName}} - Điểm đo {{PointName}}"
                  onChange={(e) => setLiveSubject(e.target.value)}
                />
              </Form.Item>
            </Col>
            <Col span={8}>
              <Form.Item name="description" label="Ghi chú / Mô tả phạm vi áp dụng">
                <Input placeholder="Mô tả tóm tắt đối tượng áp dụng mẫu..." />
              </Form.Item>
            </Col>
          </Row>

          {/* Visual Toolbar: Quick Block Inserters for Non-Technical Admins */}
          <Card
            size="small"
            style={{ marginBottom: 14, background: '#f8fafc', border: '1px solid #cbd5e1', borderRadius: 8 }}
            title={
              <div className="flex justify-between items-center">
                <Space>
                  <BlockOutlined style={{ color: '#003366', fontSize: 15 }} />
                  <Text strong style={{ fontSize: 13, color: '#0f172a' }}>
                    Thanh Công Cụ Trực Quan: Chèn Nhanh Khối Nội Dung (Không cần viết mã HTML)
                  </Text>
                </Space>
                <Text type="secondary" style={{ fontSize: 12 }}>
                  Nhấp vào khối để chèn trực tiếp vào nội dung email
                </Text>
              </div>
            }
          >
            <Space wrap size={[8, 8]}>
              <Button
                size="small"
                icon={<MailOutlined />}
                onClick={() =>
                  insertContentBlock(
                    `<p style='margin:0 0 16px 0;font-size:15px;'>Kính gửi Quý khách hàng <strong>{{CustomerName}}</strong>,</p>`,
                    'Đoạn Chào Mừng Khách Hàng'
                  )
                }
              >
                + Đoạn chào mừng
              </Button>

              <Button
                size="small"
                icon={<TableOutlined style={{ color: '#003366' }} />}
                style={{ borderColor: '#003366', color: '#003366' }}
                onClick={() =>
                  insertContentBlock(
                    `<table width='100%' cellpadding='8' cellspacing='0' border='1' style='border-collapse:collapse;border-color:#e2e8f0;margin-bottom:20px;font-size:13px;'>
    <tr style='background-color:#f8fafc;font-weight:600;text-align:center;'>
        <td>Biểu giá</td>
        <td>Chỉ số cũ</td>
        <td>Chỉ số mới</td>
        <td>Sản lượng (kWh)</td>
    </tr>
    <tr>
        <td>Bình thường (BT)</td>
        <td align='right'>{{PreviousBt}}</td>
        <td align='right'>{{Bt}}</td>
        <td align='right'><strong>{{ConsumtionBt}}</strong></td>
    </tr>
    <tr>
        <td>Cao điểm (CD)</td>
        <td align='right'>{{PreviousCd}}</td>
        <td align='right'>{{Cd}}</td>
        <td align='right'><strong>{{ConsumtionCd}}</strong></td>
    </tr>
    <tr>
        <td>Thấp điểm (TD)</td>
        <td align='right'>{{PreviousTd}}</td>
        <td align='right'>{{Td}}</td>
        <td align='right'><strong>{{ConsumtionTd}}</strong></td>
    </tr>
    <tr style='background-color:#fef2f2;font-weight:700;'>
        <td>Tổng cộng (SG)</td>
        <td align='right'>{{PreviousSg}}</td>
        <td align='right'>{{Sg}}</td>
        <td align='right' style='color:#b22222;'>{{ConsumtionTotal}}</td>
    </tr>
</table>`,
                    'Bảng Chỉ Số 3 Biểu Giá'
                  )
                }
              >
                + Bảng chỉ số 3 giá (BT, CD, TD, SG)
              </Button>

              <Button
                size="small"
                icon={<KeyOutlined style={{ color: '#52c41a' }} />}
                onClick={() =>
                  insertContentBlock(
                    `<table width='100%' cellpadding='12' cellspacing='0' border='0' style='background-color:#f8fafc;border-radius:8px;border:1px solid #e2e8f0;margin-bottom:24px;font-size:14px;'>
    <tr>
        <td width='35%' style='color:#64748b;font-weight:500;'>Tên đăng nhập:</td>
        <td width='65%' style='color:#0f172a;font-weight:700;font-family:monospace;font-size:15px;'>{{Username}}</td>
    </tr>
    <tr>
        <td style='color:#64748b;font-weight:500;'>Mật khẩu khởi tạo:</td>
        <td style='color:#b22222;font-weight:700;font-family:monospace;font-size:15px;'>{{Password}}</td>
    </tr>
    <tr>
        <td style='color:#64748b;font-weight:500;'>Đơn vị liên kết:</td>
        <td style='color:#0f172a;font-weight:600;'>{{CustomerName}}</td>
    </tr>
</table>`,
                    'Khung Tài Khoản & Mật Khẩu'
                  )
                }
              >
                + Khung tài khoản & mật khẩu
              </Button>

              <Button
                size="small"
                icon={<CheckCircleOutlined style={{ color: '#b22222' }} />}
                onClick={() =>
                  insertContentBlock(
                    `<table width='100%' cellpadding='0' cellspacing='0' border='0' style='margin:24px 0;'>
    <tr>
        <td align='center'>
            <a href='{{LoginUrl}}' target='_blank' style='display:inline-block;background-color:#b22222;color:#ffffff;font-size:14px;font-weight:600;text-decoration:none;padding:12px 32px;border-radius:6px;box-shadow:0 2px 8px rgba(178,34,34,0.3);'>
                Đăng Nhập Vào Hệ Thống
            </a>
        </td>
    </tr>
</table>`,
                    'Nút Bấm Hành Động (CTA)'
                  )
                }
              >
                + Nút bấm hành động (CTA)
              </Button>

              <Button
                size="small"
                icon={<WarningOutlined style={{ color: '#faad14' }} />}
                onClick={() =>
                  insertContentBlock(
                    `<div style='background-color:#fffbeb;border-left:4px solid #f59e0b;padding:12px 16px;margin-bottom:20px;border-radius:4px;'>
    <p style='margin:0;font-size:13px;color:#92400e;line-height:1.5;'>
        <strong>Lưu ý quan trọng:</strong> Vì lý do an toàn bảo mật, Quý vị vui lòng đổi mật khẩu ngay trong lần đăng nhập đầu tiên và không chia sẻ thông tin cho bên thứ ba.
    </p>
</div>`,
                    'Khung Lưu Ý / Cảnh Báo'
                  )
                }
              >
                + Khung lưu ý / cảnh báo
              </Button>

              <Button
                size="small"
                icon={<PhoneOutlined style={{ color: '#13c2c2' }} />}
                onClick={() =>
                  insertContentBlock(
                    `<div style='background-color:#f8fafc;border-left:4px solid #003366;padding:12px 16px;border-radius:4px;margin-bottom:20px;font-size:13px;color:#475569;'>
    Mọi thắc mắc hoặc yêu cầu đối soát số liệu, xin vui lòng liên hệ <strong>{{CareFullname}}</strong> qua email <strong>{{CareEmail}}</strong> hoặc hotline <strong>{{CarePhone}}</strong>.
</div>`,
                    'Khối Liên Hệ CSKH'
                  )
                }
              >
                + Khối hỗ trợ CSKH
              </Button>
            </Space>
          </Card>

          {/* Merge Tags Palette */}
          <Card
            size="small"
            style={{ marginBottom: 16, background: '#fafafa', border: '1px dashed #d9d9d9', borderRadius: 8 }}
            title={
              <Space>
                <CopyOutlined style={{ color: '#003366' }} />
                <Text strong style={{ fontSize: 13 }}>
                  Danh mục Thẻ Biến Nghiệp Vụ (Nhấp vào thẻ để tự động chèn vào nội dung và sao chép vào bộ nhớ đệm)
                </Text>
              </Space>
            }
          >
            {MERGE_TAGS_BY_GROUP.map((grp) => (
              <div key={grp.group} style={{ marginBottom: 6 }}>
                <Text type="secondary" style={{ fontSize: 12, display: 'inline-block', width: 170, fontWeight: 500 }}>
                  {grp.group}:
                </Text>
                <Space wrap size={[4, 6]}>
                  {grp.tags.map((t) => (
                    <Tooltip key={t.tag} title={t.desc}>
                      <Tag
                        color="processing"
                        style={{ cursor: 'pointer', userSelect: 'none', borderRadius: 4 }}
                        onClick={() => insertTagToBody(t.tag)}
                      >
                        {t.tag}
                      </Tag>
                    </Tooltip>
                  ))}
                </Space>
              </div>
            ))}
          </Card>

          {/* Main Editor Section: Split View vs Full HTML */}
          {editorMode === 'split' ? (
            <Row gutter={16}>
              <Col span={12}>
                <div style={{ display: 'flex', justifyContent: 'between', alignItems: 'center', marginBottom: 6 }}>
                  <Text strong style={{ fontSize: 13, color: '#0f172a' }}>
                    Nội dung Email (HTML & Thẻ Biến)
                  </Text>
                  <Text type="secondary" style={{ fontSize: 11 }}>
                    Biên soạn và chèn khối nội dung bên dưới
                  </Text>
                </div>
                <Form.Item
                  name="bodyHtmlTemplate"
                  rules={[{ required: true, message: 'Vui lòng nhập nội dung mẫu email' }]}
                  style={{ marginBottom: 0 }}
                >
                  <TextArea
                    rows={19}
                    style={{ fontFamily: 'Consolas, monospace', fontSize: 12.5, lineHeight: 1.5 }}
                    placeholder="Nhập mã HTML của mẫu email..."
                    onChange={(e) => setLiveHtml(e.target.value)}
                  />
                </Form.Item>
              </Col>

              <Col span={12}>
                <div style={{ display: 'flex', justifyContent: 'between', alignItems: 'center', marginBottom: 6 }}>
                  <Space>
                    <EyeOutlined style={{ color: '#52c41a' }} />
                    <Text strong style={{ fontSize: 13, color: '#0f172a' }}>
                      Xem trước trực tiếp (Live Interactive Preview)
                    </Text>
                  </Space>
                  <Tag color="cyan">Dữ liệu mẫu thực tế</Tag>
                </div>

                <div
                  style={{
                    border: '1px solid #cbd5e1',
                    borderRadius: 8,
                    height: 480,
                    overflowY: 'auto',
                    background: '#f8fafc',
                    boxShadow: 'inset 0 2px 4px rgba(0,0,0,0.02)',
                  }}
                >
                  {/* Subject Bar in Preview */}
                  <div
                    style={{
                      padding: '10px 14px',
                      background: '#e2e8f0',
                      borderBottom: '1px solid #cbd5e1',
                      fontSize: 12.5,
                    }}
                  >
                    <Text strong style={{ color: '#475569' }}>Tiêu đề thư: </Text>
                    <Text strong style={{ color: '#0f172a' }}>{livePreview.renderedSubj || '(Chưa có tiêu đề)'}</Text>
                  </div>

                  {/* Rendered Body */}
                  <div
                    style={{ padding: 12, background: '#ffffff', minHeight: 430 }}
                    dangerouslySetInnerHTML={{ __html: livePreview.renderedHtml }}
                  />
                </div>
              </Col>
            </Row>
          ) : (
            <div>
              <Form.Item
                name="bodyHtmlTemplate"
                label="Nội dung Email (HTML Template)"
                rules={[{ required: true, message: 'Vui lòng nhập nội dung mẫu email' }]}
                tooltip="Hỗ trợ mã HTML hoàn chỉnh và các thẻ biến {{Key}}"
              >
                <TextArea
                  rows={20}
                  style={{ fontFamily: 'Consolas, monospace', fontSize: 13 }}
                  placeholder="Nhập mã HTML của mẫu email..."
                  onChange={(e) => setLiveHtml(e.target.value)}
                />
              </Form.Item>
            </div>
          )}
        </Form>
      </Drawer>

      {/* Modal Pre-built Templates Library */}
      <Modal
        title={
          <Space>
            <AppstoreOutlined style={{ color: '#003366', fontSize: 20 }} />
            <Text strong style={{ fontSize: 17, color: '#003366' }}>
              Thư Viện Mẫu Email Tạo Sẵn (Pre-built Templates Library)
            </Text>
          </Space>
        }
        open={libraryModalOpen}
        onCancel={() => setLibraryModalOpen(false)}
        footer={[
          <Button key="close" onClick={() => setLibraryModalOpen(false)}>
            Đóng
          </Button>,
        ]}
        width={960}
      >
        <Paragraph type="secondary" style={{ marginTop: 4, marginBottom: 16 }}>
          Hệ thống cung cấp sẵn các biểu mẫu email nghiệp vụ được thiết kế chuẩn doanh nghiệp theo nhận diện VSIP. Quản trị viên chỉ cần chọn mẫu phù hợp để nạp nhanh vào biểu mẫu soạn thảo và hiệu chỉnh thông tin theo nhu cầu mà không cần lập trình HTML.
        </Paragraph>

        <Tabs
          activeKey={libraryTab}
          onChange={(key) => setLibraryTab(key)}
          items={[
            { key: 'ALL', label: 'Tất cả biểu mẫu' },
            { key: 'STATEMENT_NOTIFICATION', label: 'Phiếu báo điện năng' },
            { key: 'USER_CREDENTIAL', label: 'Cấp tài khoản mới' },
            { key: 'FORGOT_PASSWORD', label: 'Đặt lại mật khẩu' },
          ]}
          style={{ marginBottom: 16 }}
        />

        <Row gutter={[16, 16]}>
          {PREBUILT_TEMPLATES_LIBRARY.filter(
            (t) => libraryTab === 'ALL' || t.code === libraryTab
          ).map((item) => (
            <Col span={12} key={item.id}>
              <Card
                hoverable
                style={{ height: '100%', borderColor: '#e2e8f0', borderRadius: 8 }}
                bodyStyle={{ display: 'flex', flexDirection: 'column', height: '100%', padding: 18 }}
              >
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: 8 }}>
                  <Text strong style={{ fontSize: 15, color: '#0f172a' }}>
                    {item.name}
                  </Text>
                  <Tag color={item.badgeColor} style={{ marginRight: 0 }}>
                    {item.tag}
                  </Tag>
                </div>

                <div style={{ marginBottom: 10 }}>
                  <Tag color="geekblue">{item.code}</Tag>
                </div>

                <Paragraph type="secondary" style={{ fontSize: 13, flex: 1, marginBottom: 14 }}>
                  {item.description}
                </Paragraph>

                <div style={{ background: '#f8fafc', padding: '8px 12px', borderRadius: 6, marginBottom: 16, fontSize: 12 }}>
                  <Text type="secondary">Tiêu đề mẫu: </Text>
                  <Text italic style={{ color: '#334155' }}>{item.subjectTemplate}</Text>
                </div>

                <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 8, marginTop: 'auto' }}>
                  <Button
                    size="small"
                    icon={<EyeOutlined />}
                    onClick={() => {
                      const rendered = renderWithMockData(item.bodyHtmlTemplate, item.subjectTemplate);
                      setPreviewData({
                        subject: rendered.renderedSubj,
                        bodyHtml: rendered.renderedHtml,
                      });
                      setPreviewOpen(true);
                    }}
                  >
                    Xem Thử Mẫu
                  </Button>
                  <Button
                    size="small"
                    type="primary"
                    icon={<CheckCircleOutlined />}
                    style={{ backgroundColor: '#003366', borderColor: '#003366' }}
                    onClick={() => {
                      if (!drawerOpen) {
                        setEditingTemplate(null);
                        form.resetFields();
                        setDrawerOpen(true);
                      }
                      handleApplyPrebuilt(item);
                    }}
                  >
                    Sử Dụng Mẫu Này
                  </Button>
                </div>
              </Card>
            </Col>
          ))}
        </Row>
      </Modal>

      {/* Modal Live Preview */}
      <Modal
        title={
          <Space>
            <EyeOutlined style={{ color: '#003366' }} />
            <Text strong>Xem trước Email (Live Preview)</Text>
          </Space>
        }
        open={previewOpen}
        onCancel={() => setPreviewOpen(false)}
        footer={[
          <Button key="close" type="primary" onClick={() => setPreviewOpen(false)}>
            Đóng
          </Button>,
        ]}
        width={750}
      >
        {previewLoading ? (
          <div style={{ textAlign: 'center', padding: 40 }}>Đang biên dịch mẫu email với dữ liệu mẫu...</div>
        ) : previewData ? (
          <div>
            <div style={{ padding: '8px 12px', background: '#f5f5f5', borderRadius: 6, marginBottom: 12 }}>
              <Text strong>Tiêu đề: </Text>
              <Text style={{ color: '#003366', fontWeight: 600 }}>{previewData.subject}</Text>
            </div>
            <div
              style={{
                border: '1px solid #e8e8e8',
                borderRadius: 8,
                padding: 16,
                maxHeight: 520,
                overflowY: 'auto',
                background: '#ffffff',
              }}
              dangerouslySetInnerHTML={{ __html: previewData.bodyHtml }}
            />
          </div>
        ) : (
          <div>Không có dữ liệu xem trước</div>
        )}
      </Modal>

      {/* Modal Test Send Email */}
      <Modal
        title={
          <Space>
            <SendOutlined style={{ color: '#003366' }} />
            <Text strong>Gửi Email Thử Nghiệm (Live Test Mail)</Text>
          </Space>
        }
        open={testSendOpen}
        onCancel={() => setTestSendOpen(false)}
        onOk={handleSendTest}
        confirmLoading={testSending}
        okText="Gửi Ngay"
        cancelText="Hủy"
        okButtonProps={{ style: { backgroundColor: '#003366', borderColor: '#003366' } }}
        width={520}
      >
        <Paragraph type="secondary" style={{ marginTop: 8, marginBottom: 16 }}>
          Hệ thống sẽ biên dịch mẫu email này cùng bộ dữ liệu mẫu thực tế và gửi trực tiếp qua hạ tầng Email hiện tại tới hòm thư cá nhân của bạn để kiểm tra hiển thị.
        </Paragraph>
        <Form layout="vertical">
          <Form.Item
            label="Địa chỉ Email người nhận thử nghiệm"
            required
            tooltip="Hệ thống sẽ gửi email thật tới địa chỉ này"
          >
            <Input
              placeholder="ví dụ: admin@company.com"
              value={testEmailTarget}
              onChange={(e) => setTestEmailTarget(e.target.value)}
              prefix={<SendOutlined style={{ color: '#8c8c8c' }} />}
            />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
};

export default NotificationTemplatesPage;
