import React, { useEffect, useMemo, useState } from 'react';
import {
  Card,
  Checkbox,
  Col,
  ColorPicker,
  Divider,
  Form,
  Input,
  Modal,
  Radio,
  Row,
  Select,
  Space,
  Tabs,
  Tag,
  Tooltip,
  Typography,
  message,
} from 'antd';
import {
  AppstoreOutlined,
  ControlOutlined,
  EyeOutlined,
  FileTextOutlined,
  MailOutlined,
  PictureOutlined,
  SaveOutlined,
  SendOutlined,
} from '@ant-design/icons';

import Constants from '@/core/application/common/constants';
import { Endpoint } from '@/core/application/common/endPoint';
import {
  NotificationTemplateDto,
  CreateNotificationTemplateRequest,
  UpdateNotificationTemplateRequest,
} from '@/core/application/interfaces/methods/templates/templateManagement.interface';
import { ButtonCommonCustomizable } from '@/infrastructure/common/components/button/button-common-customizable';
import { BreadcrumbUI } from '@/infrastructure/common/components/controls/BreadcrumbUI';
import { FullPageLoading } from '@/infrastructure/common/components/controls/Loading';
import PageTitle from '@/infrastructure/common/components/title/page-title';
import { ConfigManagementService } from '@/infrastructure/repository/config/services/configManagement.service';
import { TemplateManagementService } from '@/infrastructure/repository/templates/services/templateManagement.service';
import {
  getCachedStatementTemplate,
  parseStatementLayoutConfig,
  setCachedStatementTemplate,
} from '@/pages/notice-management/components/form-pdf';

const { Text } = Typography;
const { TextArea } = Input;

const TEMPLATE_CODES = [
  { value: 'STATEMENT_NOTIFICATION', label: 'Phiếu báo điện năng (STATEMENT_NOTIFICATION)' },
  { value: 'USER_CREDENTIAL', label: 'Cấp tài khoản mới (USER_CREDENTIAL)' },
  { value: 'FORGOT_PASSWORD', label: 'Đặt lại mật khẩu (FORGOT_PASSWORD)' },
];

const DEFAULT_INTRO_TEXT =
  'Chúng tôi xin trân trọng gửi tới Quý khách hàng bảng kê chỉ số công tơ và sản lượng điện năng tiêu thụ trong kỳ ghi chỉ số: {{Period}}.';

const DEFAULT_CONTACT_NOTE =
  'Mọi thắc mắc hoặc yêu cầu đối soát số liệu, Quý khách vui lòng liên hệ: {{CareFullname}}\n📞 Điện thoại: {{ContactPhone}}\n✉️ Email: {{CareEmail}}';

interface VisualEmailConfig {
  themeColor: string;
  headerAlign: 'left' | 'center' | 'right';
  showCompanyMeta: boolean;
  systemName: string;
  headerTitle: string;
  columns: string[];
  introText: string;
  contactNote: string;
}

const EMAIL_THEME_PRESETS = [
  { label: 'Đỏ VSIP (#b22222)', value: '#b22222', gradient: 'linear-gradient(135deg,#b22222 0%,#8b1a1a 100%)' },
  { label: 'Xanh Điện Lực (#003366)', value: '#003366', gradient: 'linear-gradient(135deg,#003366 0%,#001d3d 100%)' },
  { label: 'Xanh Dương Hiện Đại (#0284c7)', value: '#0284c7', gradient: 'linear-gradient(135deg,#0284c7 0%,#0369a1 100%)' },
  { label: 'Xanh Lá Bền Vững (#15803d)', value: '#15803d', gradient: 'linear-gradient(135deg,#15803d 0%,#166534 100%)' },
];

const EMAIL_INDICATOR_SWITCHES = [
  { key: 'BT', label: 'Bình thường (BT)' },
  { key: 'CD', label: 'Cao điểm (CD)' },
  { key: 'TD', label: 'Thấp điểm (TD)' },
  { key: 'SG', label: 'Tổng điện năng hữu công (SG)' },
  { key: 'VC', label: 'Tổng điện năng phản kháng (VC)' },
  { key: 'PMAX', label: 'Công suất Pmax & Thời điểm ghi nhận' },
  { key: 'COSPHI', label: 'Hệ số Cos φ' },
];

/**
 * Xóa bỏ hoàn toàn các trường cũ (Mã khách hàng, Mã điểm đo, HSN) nếu template lưu trong CSDL vẫn còn chứa
 */
const cleanLegacyBodyHtml = (html: string): string => {
  if (!html) return html;
  let cleaned = html;

  // 1. Nếu chứa bảng thông tin cũ có Mã khách hàng, Mã điểm đo hoặc HSN -> Thay thế bằng bảng chuẩn chỉ có Tên điểm đo
  if (
    cleaned.includes('Mã khách hàng') ||
    cleaned.includes('CustomerCode') ||
    cleaned.includes('Mã điểm đo') ||
    cleaned.includes('Hệ số nhân') ||
    cleaned.includes('HSN') ||
    cleaned.includes('Số công tơ')
  ) {
    const cleanInfoTable = `<table width='100%' cellpadding='10' cellspacing='0' border='0' style='background-color:#f8fafc;border-radius:8px;border:1px solid #e2e8f0;margin-bottom:18px;font-size:13.5px;'>
                                <tr>
                                    <td width='22%' style='color:#64748b;font-weight:500;'>Tên điểm đo:</td>
                                    <td style='font-weight:700;color:#0f172a;'>{{PointName}}</td>
                                </tr>
                            </table>`;

    cleaned = cleaned.replace(
      /<table[^>]*>[\s\S]*?(?:Mã khách hàng|CustomerCode|Mã điểm đo)[\s\S]*?<\/table>/gi,
      cleanInfoTable
    );
  }

  // 2. Đồng bộ thông báo tệp đính kèm theo chuẩn yêu cầu
  cleaned = cleaned.replace(
    /(?:📎\s*(?:<strong>)?Tệp đính kèm:?(?:<\/strong>)?\s*)?Chi tiết phiếu báo chỉ số điện năng định dạng PDF[^<]*<strong>[^<]*<\/strong>[^<]*/gi,
    '📎 Chi tiết Phiếu chỉ số điện hàng tháng được đính kèm <strong>PhieuBaoSo_{{SerialNumber}}.pdf</strong>'
  );

  // 3. Hiệu chỉnh Hotline & Email trên 2 dòng riêng biệt nếu còn ngăn cách bởi dấu gạch đứng |
  cleaned = cleaned.replace(
    /\|\s*(📞\s*Hotline CSKH:[^<|]*)\|\s*(✉️\s*Email tiếp nhận:[^<]*)/gi,
    '<br>$1<br>$2'
  );

  // 4. Căn lề trên (vertical-align: top) cho cột Pmax và CosPhi trong Khối thông số vận hành để thẳng hàng
  cleaned = cleaned.replace(/style=['"]vertical-align:\s*middle;?['"]/gi, "style='vertical-align:top;'");
  cleaned = cleaned.replace(
    /(<table[^>]*style=['"][^'"]*color:#1e40af;[^'"]*['"][^>]*>[\s\S]*?<td\s+width=['"](?:55%|100%)['"])(?!\s+style=)/gi,
    "$1 style='vertical-align:top;'"
  );

  // 5. Loại bỏ dòng "Đơn vị liên kết" trong mẫu email cấp tài khoản
  cleaned = cleaned.replace(
    /<tr>\s*<td[^>]*>[^<]*Đơn vị liên kết:?<\/td>[\s\S]*?<\/tr>/gi,
    ''
  );

  // 6. Đổi nút "Đăng Nhập Vào Hệ Thống" thành "Đăng Nhập"
  cleaned = cleaned.replace(/Đăng\s+Nhập\s+Vào\s+Hệ\s+Thống/gi, 'Đăng Nhập');

  // 7. Đồng bộ tiêu đề hệ thống dạng (MDMS)
  cleaned = cleaned.replace(/HỆ THỐNG THU THẬP DỮ LIỆU ĐO ĐẾM - MDMS/gi, 'HỆ THỐNG THU THẬP DỮ LIỆU ĐO ĐẾM (MDMS)');

  // 8. Chuẩn hóa tiêu đề Header cho mẫu tài khoản & mật khẩu
  cleaned = cleaned.replace(/THÔNG TIN CẤP TÀI KHOẢN NGƯỜI DÙNG/gi, 'THÔNG TIN TÀI KHOẢN');

  // 9. Bỏ đoạn "(từ ngày {{FromDate}} đến ngày {{ToDate}})" trong lời mở đầu để khớp 100% hình ảnh
  cleaned = cleaned.replace(/\s*\(từ ngày \{\{FromDate\}\} đến ngày \{\{ToDate\}\}\)/gi, '');

  return cleaned;
};

const buildVisualStatementEmailHtml = (cfg: VisualEmailConfig): string => {
  const preset = EMAIL_THEME_PRESETS.find(
    (p) => p.value.toLowerCase() === (cfg.themeColor || '').toLowerCase()
  );
  const headerBg = preset ? preset.gradient : (cfg.themeColor || '#b22222');
  const colSet = new Set(cfg.columns.map((c) => c.toUpperCase()));

  const rowsHtml: string[] = [];
  if (colSet.has('BT')) {
    rowsHtml.push(`                                <tr>
                                    <td style='padding-left:12px;'>Bình thường (BT)</td>
                                    <td align='right' style='color:#475569;'>{{PreviousBt}}</td>
                                    <td align='right' style='color:#475569;'>{{Bt}}</td>
                                    <td align='right' style='font-weight:700;color:#0f172a;'>{{ConsumtionBt}}</td>
                                </tr>`);
  }
  if (colSet.has('CD')) {
    rowsHtml.push(`                                <tr>
                                    <td style='padding-left:12px;'>Cao điểm (CD)</td>
                                    <td align='right' style='color:#475569;'>{{PreviousCd}}</td>
                                    <td align='right' style='color:#475569;'>{{Cd}}</td>
                                    <td align='right' style='font-weight:700;color:#0f172a;'>{{ConsumtionCd}}</td>
                                </tr>`);
  }
  if (colSet.has('TD')) {
    rowsHtml.push(`                                <tr>
                                    <td style='padding-left:12px;'>Thấp điểm (TD)</td>
                                    <td align='right' style='color:#475569;'>{{PreviousTd}}</td>
                                    <td align='right' style='color:#475569;'>{{Td}}</td>
                                    <td align='right' style='font-weight:700;color:#0f172a;'>{{ConsumtionTd}}</td>
                                </tr>`);
  }
  if (colSet.has('SG')) {
    rowsHtml.push(`                                <tr style='background-color:#fef2f2;font-weight:700;'>
                                    <td style='padding-left:12px;color:${cfg.themeColor};'>Tổng điện năng hữu công (SG)</td>
                                    <td align='right' style='color:${cfg.themeColor};'>{{PreviousSg}}</td>
                                    <td align='right' style='color:${cfg.themeColor};'>{{Sg}}</td>
                                    <td align='right' style='color:${cfg.themeColor};font-size:14px;'>{{ConsumtionTotal}}</td>
                                </tr>`);
  }
  if (colSet.has('VC')) {
    rowsHtml.push(`                                <tr>
                                    <td style='padding-left:12px;color:#64748b;'>Tổng điện năng phản kháng (VC)</td>
                                    <td align='right' style='color:#64748b;'>{{PreviousVc}}</td>
                                    <td align='right' style='color:#64748b;'>{{Vc}}</td>
                                    <td align='right' style='font-weight:600;color:#64748b;'>{{ConsumtionVc}}</td>
                                </tr>`);
  }

  const showPmax = colSet.has('PMAX');
  const showCosPhi = colSet.has('COSPHI');

  let pmaxCosBlock = '';
  if (showPmax || showCosPhi) {
    pmaxCosBlock = `
                    <!-- [KHỐI 4] THÔNG SỐ VẬN HÀNH PHỤ TẢI (PMAX & COS PHI) -->
                    <tr>
                        <td style='padding:0 30px 15px 30px;'>
                            <table width='100%' cellpadding='8' cellspacing='0' border='0' style='background-color:#eff6ff;border:1px solid #bfdbfe;border-radius:6px;font-size:13px;color:#1e40af;'>
                                <tr>
                                    ${
                                      showPmax
                                        ? `<td width='${showCosPhi ? '55%' : '100%'}' style='vertical-align:top;'>
                                        • Công suất cực đại <strong>Pmax</strong>: <strong>{{Pmax}}</strong><br>
                                        • Thời điểm ghi nhận hồi: <strong>{{PmaxTime}}</strong>
                                    </td>`
                                        : ''
                                    }
                                    ${
                                      showCosPhi
                                        ? `<td width='${showPmax ? '45%' : '100%'}' style='vertical-align:top;'>
                                        • Hệ số công suất <strong>Cos &phi;</strong>: <strong>{{CosPhi}}</strong>
                                    </td>`
                                        : ''
                                    }
                                </tr>
                            </table>
                        </td>
                    </tr>`;
  }

  const introParagraph = cfg.introText || DEFAULT_INTRO_TEXT;
  const contactText = (cfg.contactNote || DEFAULT_CONTACT_NOTE).replace(/\n/g, '<br>');

  return `<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Thông Báo Chỉ Số & Sản Lượng Điện Năng</title>
</head>
<body style='margin:0;padding:0;background-color:#f5f7fc;font-family:-apple-system,BlinkMacSystemFont,"Segoe UI",Roboto,Helvetica,Arial,sans-serif;color:#334155;'>
    <table width='100%' cellpadding='0' cellspacing='0' border='0' style='background-color:#f5f7fc;padding:24px 10px;width:100%;max-width:100%;'>
        <tr>
            <td align='center'>
                <table width='100%' cellpadding='0' cellspacing='0' border='0' style='max-width:640px;width:100%;background-color:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 4px 16px rgba(0,0,0,0.06);border:1px solid #e2e8f0;'>
                    <!-- [KHỐI 1] HEADER ĐƠN VỊ PHÁT HÀNH -->
                    <tr>
                        <td style='background:${headerBg};padding:24px 30px;text-align:${cfg.headerAlign};'>
                            <h2 style='margin:0;color:#ffffff;font-size:20px;font-weight:700;'>{{CompanyName}}</h2>
                            <p style='margin:6px 0 0 0;color:#fecaca;font-size:13px;letter-spacing:0.5px;'>${cfg.systemName || 'HỆ THỐNG THU THẬP DỮ LIỆU ĐO ĐẾM (MDMS)'}</p>
                            <p style='margin:4px 0 0 0;color:#ffffff;font-size:14px;font-weight:600;letter-spacing:0.4px;'>${cfg.headerTitle || 'THÔNG BÁO CHỈ SỐ & SẢN LƯỢNG ĐIỆN NĂNG'}</p>
                        </td>
                    </tr>
                    <!-- [KHỐI 2] THÔNG TIN ĐIỂM ĐO -->
                    <tr>
                        <td style='padding:24px 30px 10px 30px;'>
                            <p style='margin:0 0 14px 0;font-size:15px;color:#0f172a;'>Kính gửi Quý khách hàng: <strong>{{CustomerName}}</strong>,</p>
                            <p style='margin:0 0 16px 0;font-size:13.5px;line-height:1.6;color:#475569;'>
                                ${introParagraph}
                            </p>
                            <table width='100%' cellpadding='10' cellspacing='0' border='0' style='background-color:#f8fafc;border-radius:8px;border:1px solid #e2e8f0;margin-bottom:18px;font-size:13.5px;'>
                                <tr>
                                    <td width='22%' style='color:#64748b;font-weight:500;'>Tên điểm đo:</td>
                                    <td style='font-weight:700;color:#0f172a;'>{{PointName}}</td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <!-- [KHỐI 3] BẢNG BỘ CHỈ SỐ ĐO ĐẾM & SẢN LƯỢNG -->
                    <tr>
                        <td style='padding:0 30px 15px 30px;'>
                            <table width='100%' cellpadding='8' cellspacing='0' border='1' style='border-collapse:collapse;border-color:#e2e8f0;font-size:13px;'>
                                <tr style='background-color:#f1f5f9;color:#0f172a;font-weight:600;text-align:center;'>
                                    <td width='34%'>Biểu giá</td>
                                    <td width='22%'>Chỉ số cũ</td>
                                    <td width='22%'>Chỉ số mới</td>
                                    <td width='22%'>Sản lượng</td>
                                </tr>
${rowsHtml.join('\n')}
                            </table>
                        </td>
                    </tr>${pmaxCosBlock}
                    <!-- [KHỐI 5] THÔNG BÁO TỆP ĐÍNH KÈM PHIẾU BÁO PDF -->
                    <tr>
                        <td style='padding:0 30px 15px 30px;'>
                            <div style='background-color:#f8fafc;border-left:4px solid ${cfg.themeColor};padding:12px 16px;border-radius:4px;font-size:13px;color:#334155;line-height:1.6;'>
                                📎 Chi tiết Phiếu chỉ số điện hàng tháng được đính kèm <strong>PhieuBaoSo_{{SerialNumber}}.pdf</strong>
                            </div>
                        </td>
                    </tr>
                    <!-- [KHỐI 6] THÔNG TIN LIÊN HỆ ĐỐI SOÁT -->
                    <tr>
                        <td style='padding:0 30px 24px 30px;'>
                            <div style='background-color:#fafaf9;border:1px dashed #d6d3d1;padding:12px 16px;border-radius:6px;font-size:12.5px;color:#57534e;line-height:1.6;'>
                                ${contactText}
                            </div>
                            <p style='margin:16px 0 0 0;font-size:13.5px;color:#334155;'>Trân trọng cảm ơn sự hợp tác của Quý khách hàng!</p>
                        </td>
                    </tr>
                    <!-- [KHỐI 7] CHÂN TRANG HỆ THỐNG -->
                    <tr>
                        <td style='background-color:#f8fafc;border-top:1px solid #e2e8f0;padding:16px;text-align:center;font-size:12px;color:#94a3b8;'>
                            Email tự động phát hành từ ${cfg.systemName || 'HỆ THỐNG THU THẬP DỮ LIỆU ĐO ĐẾM (MDMS)'} - {{CompanyName}}. Vui lòng không phản hồi trực tiếp vào địa chỉ thư này.
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>`;
};

// Helper: Trích xuất các trường từ HTML string
const extractHeaderInfo = (html: string, code: string) => {
  const sysMatch = html?.match(/letter-spacing:0\.5px;'>([^<]+)<\/p>/i);
  const titleMatch = html?.match(/font-weight:600;[^']*'>([^<]+)<\/p>/i);
  const defaultSys =
    code === 'STATEMENT_NOTIFICATION'
      ? 'HỆ THỐNG THU THẬP DỮ LIỆU ĐO ĐẾM (MDMS)'
      : 'HỆ THỐNG QUẢN LÝ DỮ LIỆU ĐO ĐẾM ĐIỆN TỬ (MDMS)';
  const defaultTitle =
    code === 'STATEMENT_NOTIFICATION'
      ? 'THÔNG BÁO CHỈ SỐ & SẢN LƯỢNG ĐIỆN NĂNG'
      : code === 'USER_CREDENTIAL'
      ? 'THÔNG TIN TÀI KHOẢN'
      : 'YÊU CẦU ĐẶT LẠI MẬT KHẨU';
  return {
    systemName: sysMatch ? sysMatch[1].trim() : defaultSys,
    headerTitle: titleMatch ? titleMatch[1].trim() : defaultTitle,
  };
};

const extractIntroText = (html: string): string => {
  const match = html?.match(/<p style='margin:0 0 16px 0;font-size:13\.5px;line-height:1\.6;color:#475569;'>([\s\S]*?)<\/p>/i);
  if (match && match[1]) {
    return match[1].replace(/<[^>]+>/g, '').trim();
  }
  return DEFAULT_INTRO_TEXT;
};

const extractContactNote = (html: string, code?: string): string => {
  const match = html?.match(/<div style='background-color:#fafaf9;border:1px dashed #d6d3d1;padding:12px 16px;border-radius:6px;font-size:12\.5px;color:#57534e;line-height:1\.6;'>([\s\S]*?)<\/div>/i);
  if (match && match[1]) {
    return match[1]
      .replace(/\|\s*(📞\s*Hotline CSKH:)/gi, '\n$1')
      .replace(/\|\s*(✉️\s*Email tiếp nhận:)/gi, '\n$1')
      .replace(/\|\s*(✉️\s*Email:)/gi, '\n$1')
      .replace(/<br\s*\/?>/gi, '\n')
      .replace(/<[^>]+>/g, '')
      .trim();
  }
  return code === 'USER_CREDENTIAL' || code === 'FORGOT_PASSWORD'
    ? 'Nếu Quý vị cần hỗ trợ kỹ thuật, vui lòng liên hệ: {{CareFullname}}\n📞 Hotline: {{SupportPhone}}\n✉️ Email: {{SystemEmail}}'
    : DEFAULT_CONTACT_NOTE;
};

// Realistic mock data for 100% live preview (synced with DB Contact & Company info)
const renderWithMockData = (
  template: string,
  subject: string,
  companyName: string,
  contactInfo?: { careFullname?: string; careEmail?: string; carePhone?: string },
  companyAddress?: string,
  companyPhone?: string
) => {
  const activeStmtTpl = getCachedStatementTemplate();
  const stmtLayout = parseStatementLayoutConfig(activeStmtTpl?.displayColumnsJson);

  const mock: Record<string, string> = {
    CustomerName: 'CÔNG TY TNHH CÔNG NGHỆ MÁY VĂN PHÒNG KYOCERA VIỆT NAM',
    PointCode: 'KYOCERA 1',
    PointName: 'KYOCERA 1',
    CustomerCode: 'KYOCERA',
    MeterSerial: '23098172',
    Multiplier: '1',
    Address: 'Khu Đô thị, Công nghiệp và Dịch vụ VSIP Hải Phòng',
    CompanyName: companyName || activeStmtTpl?.companyName || 'VSIP HAI PHONG CO., LTD',
    CompanyAddress: (
      companyAddress ||
      activeStmtTpl?.companyAddress ||
      'VSIP Hai Phong Administration Office, VSIP Hai Phong Township, Industrial & Service Park, Hoa Binh Ward, Hai Phong City, Vietnam'
    ).replace(/\n/g, ', '),
    ContactPhone: companyPhone || activeStmtTpl?.contactPhone || 'Tel: 84-225-3959868 Fax: 84-225-3959886',
    TaxCode: stmtLayout.taxCode || '0200856991',
    SystemName: 'Hệ thống thu thập dữ liệu đo đếm - MDMS',
    Period: '01/10/2026 - 31/10/2026',
    CycleName: 'Tháng 10/2026',
    FromDate: '01/10/2026',
    ToDate: '31/10/2026',
    Month: '10',
    Year: '2026',
    PreviousBt: '0.00',
    Bt: '67.00',
    ConsumtionBt: '67.00',
    PreviousCd: '0.00',
    Cd: '0.00',
    ConsumtionCd: '0.00',
    PreviousTd: '0.00',
    Td: '0.00',
    ConsumtionTd: '0.00',
    PreviousSg: '0.00',
    Sg: '67.00',
    ConsumtionTotal: '67.00',
    ConsumtionSg: '67.00',
    PreviousVc: '0.00',
    Vc: '28.00',
    ConsumtionVc: '28.00',
    Pmax: '66.00 kW',
    PmaxTime: '06/11/2026 03:16:00',
    CosPhi: '0.92',
    SerialNumber: '00000162',
    FullName: 'Nguyễn Văn An',
    AccountFullName: 'Nguyễn Văn An',
    Username: 'an.nguyen',
    Password: 'Vsip@2026#',
    Email: 'an.nguyen@vsip.com.vn',
    AccountEmail: 'an.nguyen@vsip.com.vn',
    LoginUrl: 'https://amr.nobisoft.vn/login',
    ResetLink: 'https://amr.nobisoft.vn/reset-password?token=sample_demo_token',
    CareFullname: contactInfo?.careFullname || 'Bộ phận Chăm sóc khách hàng',
    CareEmail: contactInfo?.careEmail || 'cskh@mdms.vn',
    CarePhone: contactInfo?.carePhone || '19001006',
    SystemEmail: contactInfo?.careEmail || 'cskh@mdms.vn',
    SupportPhone: contactInfo?.carePhone || '19001006',
  };

  let renderedSubj = subject || '';
  let renderedHtml = cleanLegacyBodyHtml(template || '');

  Object.keys(mock).forEach((k) => {
    const reg = new RegExp(`\\{\\{\\s*${k}\\s*\\}\\}`, 'gi');
    renderedSubj = renderedSubj.replace(reg, mock[k]);
    renderedHtml = renderedHtml.replace(reg, mock[k]);
  });

  return { renderedSubj, renderedHtml };
};

export const NotificationTemplatesPage: React.FC = () => {
  const service = useMemo(() => new TemplateManagementService(), []);
  const [loading, setLoading] = useState<boolean>(false);
  const [saving, setSaving] = useState<boolean>(false);
  const [templates, setTemplates] = useState<NotificationTemplateDto[]>([]);
  const [selectedTemplateId, setSelectedTemplateId] = useState<number | null>(null);

  // Form State
  const [templateName, setTemplateName] = useState<string>('');
  const [templateCode, setTemplateCode] = useState<string>('STATEMENT_NOTIFICATION');
  const [subjectTemplate, setSubjectTemplate] = useState<string>('');
  const [bodyHtmlTemplate, setBodyHtmlTemplate] = useState<string>('');
  const [introText, setIntroText] = useState<string>(DEFAULT_INTRO_TEXT);
  const [contactNote, setContactNote] = useState<string>(DEFAULT_CONTACT_NOTE);
  const [activeTabKey, setActiveTabKey] = useState<string>('general');

  // Recipient format for email preview
  const recipientDisplay = useMemo(() => {
    if (templateCode === 'USER_CREDENTIAL' || templateCode === 'FORGOT_PASSWORD') {
      return '{AccountFullName} <{AccountEmail}>';
    }
    return 'CÔNG TY TNHH KYOCERA VIỆT NAM <hungpetit@gmail.com>';
  }, [templateCode]);

  // When switching to a template that doesn't have metrics, switch tab back to general
  useEffect(() => {
    if (templateCode !== 'STATEMENT_NOTIFICATION' && activeTabKey === 'metrics') {
      setActiveTabKey('general');
    }
  }, [templateCode, activeTabKey]);

  // Company & Contact info from DB (Cài đặt hệ thống -> Thông tin liên hệ)
  const [companyName, setCompanyName] = useState<string>('VSIP HAI PHONG CO., LTD');
  const [companyAddress, setCompanyAddress] = useState<string>('');
  const [companyPhone, setCompanyPhone] = useState<string>('');

  // Contact Config State (synced from DB mục "Thông tin liên hệ")
  const [contactConfig, setContactConfig] = useState<{
    careFullname: string;
    careEmail: string;
    carePhone: string;
  }>({
    careFullname: 'Bộ phận Chăm sóc khách hàng',
    careEmail: 'cskh@mdms.vn',
    carePhone: '19001006',
  });

  // Visual Configurator State
  const [visualEmailCfg, setVisualEmailCfg] = useState<VisualEmailConfig>({
    themeColor: '#b22222',
    headerAlign: 'center',
    showCompanyMeta: true,
    systemName: 'HỆ THỐNG THU THẬP DỮ LIỆU ĐO ĐẾM - MDMS',
    headerTitle: 'THÔNG BÁO CHỈ SỐ & SẢN LƯỢNG ĐIỆN NĂNG',
    columns: ['BT', 'CD', 'TD', 'SG', 'VC', 'PMAX', 'COSPHI'],
    introText: DEFAULT_INTRO_TEXT,
    contactNote: DEFAULT_CONTACT_NOTE,
  });

  // Test Send Modal State
  const [testSendOpen, setTestSendOpen] = useState<boolean>(false);
  const [testEmailTarget, setTestEmailTarget] = useState<string>('');
  const [testSending, setTestSending] = useState<boolean>(false);

  const selectedTemplate = useMemo(() => {
    return templates.find((t) => t.id === selectedTemplateId) || null;
  }, [templates, selectedTemplateId]);

  const loadTemplates = async (preferredId?: number) => {
    setLoading(true);
    try {
      // 1. Load notification templates
      const res: any = await service.getNotificationTemplates();
      const rawData = res?.data?.data !== undefined ? res.data.data : res?.data;
      const list: NotificationTemplateDto[] = Array.isArray(rawData) ? rawData : [];
      setTemplates(list);

      // 2. Load contact info from DB (Cài đặt hệ thống -> Thông tin liên hệ)
      try {
        const cfgRes: any = await new ConfigManagementService().getSystemListAsync(
          Endpoint.Config.GetSystemList,
          {},
          null as any
        );
        const cfgData = cfgRes?.data?.data || cfgRes?.data || [];
        if (Array.isArray(cfgData) && cfgData.length > 0) {
          const findVal = (key: string, def: string = '') =>
            cfgData.find((x: any) => x.key === key)?.value ?? def;
          const careName =
            findVal(Constants.LockAuth.CUSTOMERCAREFULLNAME) ||
            findVal(Constants.LockAuth.SYSTEMADMINISTRATIONFULLNAME, 'Bộ phận Chăm sóc khách hàng');
          const careMail =
            findVal(Constants.LockAuth.CUSTOMERCAREEMAIL) ||
            findVal(Constants.LockAuth.SYSTEMADMINISTRATIONEMAIL, 'cskh@mdms.vn');
          const carePh =
            findVal(Constants.LockAuth.CUSTOMERCAREPHONE) ||
            findVal(Constants.LockAuth.SYSTEMADMINISTRATIONPHONE, '19001006');
          setContactConfig({
            careFullname: careName,
            careEmail: careMail,
            carePhone: carePh,
          });

          const sysCompName = findVal(Constants.LockAuth.COMPANY_NAME);
          if (sysCompName) {
            setCompanyName(sysCompName);
          }
          const sysCompAddress = findVal(Constants.LockAuth.COMPANY_ADDRESS);
          if (sysCompAddress) {
            setCompanyAddress(sysCompAddress);
          }
          const sysCompPhone = findVal(Constants.LockAuth.COMPANY_PHONE);
          if (sysCompPhone) {
            setCompanyPhone(sysCompPhone);
          }
        }
      } catch {
        // Fallback default
      }

      // 3. Cache statement template & Company Name from DB
      try {
        const stmtRes: any = await service.getStatementPdfTemplates();
        const stmtList = stmtRes?.data?.data !== undefined ? stmtRes.data.data : stmtRes?.data;
        if (Array.isArray(stmtList) && stmtList.length > 0) {
          const activeStmt =
            stmtList.find((x: any) => x.isDefault && x.isActive) ||
            stmtList.find((x: any) => x.isDefault) ||
            stmtList.find((x: any) => x.isActive) ||
            stmtList[0];
          if (activeStmt) {
            setCachedStatementTemplate(activeStmt);
            if (activeStmt.companyName) {
              setCompanyName(activeStmt.companyName);
            }
          }
        }
      } catch {
        // Fallback
      }

      if (list.length > 0) {
        const nextSelected = preferredId
          ? list.find((t) => t.id === preferredId) || list[0]
          : list.find((t) => t.code === 'STATEMENT_NOTIFICATION' && t.isDefault) ||
            list.find((t) => t.isDefault) ||
            list[0];
        applyTemplateToForm(nextSelected);
      }
    } catch (err: any) {
      message.error(`Không thể tải danh sách mẫu email: ${err?.message || err}`);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    void loadTemplates();
  }, []);

  const applyTemplateToForm = (tpl: NotificationTemplateDto) => {
    setSelectedTemplateId(tpl.id);
    setTemplateName(tpl.name || '');
    setTemplateCode(tpl.code || 'STATEMENT_NOTIFICATION');
    setSubjectTemplate(tpl.subjectTemplate || '');

    const cleanedBody = cleanLegacyBodyHtml(tpl.bodyHtmlTemplate || '');
    setBodyHtmlTemplate(cleanedBody);

    const { systemName, headerTitle } = extractHeaderInfo(cleanedBody, tpl.code || '');
    const extractedIntro = extractIntroText(cleanedBody);
    const extractedContact = extractContactNote(cleanedBody, tpl.code || '');

    setIntroText(extractedIntro);
    setContactNote(extractedContact);

    setVisualEmailCfg((prev) => ({
      ...prev,
      systemName,
      headerTitle,
      introText: extractedIntro,
      contactNote: extractedContact,
    }));
  };

  const handleSelectTemplate = (id: number) => {
    const tpl = templates.find((t) => t.id === id);
    if (tpl) {
      applyTemplateToForm(tpl);
    }
  };

  const applyVisualEmailConfig = (nextCfg: VisualEmailConfig) => {
    setVisualEmailCfg(nextCfg);
    const html = buildVisualStatementEmailHtml(nextCfg);
    setBodyHtmlTemplate(html);
  };

  const handleUpdateHeader = (field: 'systemName' | 'headerTitle', val: string) => {
    const nextCfg = { ...visualEmailCfg, [field]: val };
    setVisualEmailCfg(nextCfg);
    if (templateCode === 'STATEMENT_NOTIFICATION') {
      applyVisualEmailConfig(nextCfg);
    } else {
      let updated = bodyHtmlTemplate;
      if (field === 'systemName') {
        updated = updated.replace(
          /<p style='margin:6px 0 0 0;color:#fecaca;font-size:13px;letter-spacing:0.5px;'>[^<]*<\/p>/i,
          `<p style='margin:6px 0 0 0;color:#fecaca;font-size:13px;letter-spacing:0.5px;'>${val}</p>`
        );
      } else {
        updated = updated.replace(
          /<p style='margin:4px 0 0 0;color:#ffffff;font-size:14px;font-weight:600;'>[^<]*<\/p>/i,
          `<p style='margin:4px 0 0 0;color:#ffffff;font-size:14px;font-weight:600;'>${val}</p>`
        );
      }
      setBodyHtmlTemplate(updated);
    }
  };

  const handleUpdateIntroText = (val: string) => {
    setIntroText(val);
    const nextCfg = { ...visualEmailCfg, introText: val };
    setVisualEmailCfg(nextCfg);
    if (templateCode === 'STATEMENT_NOTIFICATION') {
      applyVisualEmailConfig(nextCfg);
    }
  };

  const handleUpdateContactNote = (val: string) => {
    setContactNote(val);
    const nextCfg = { ...visualEmailCfg, contactNote: val };
    setVisualEmailCfg(nextCfg);
    if (templateCode === 'STATEMENT_NOTIFICATION') {
      applyVisualEmailConfig(nextCfg);
    } else {
      const formattedVal = val.replace(/\n/g, '<br>');
      const updated = bodyHtmlTemplate.replace(
        /<div style='background-color:#fafaf9;border:1px dashed #d6d3d1;padding:12px 16px;border-radius:6px;font-size:12\.5px;color:#57534e;line-height:1\.6;'>[\s\S]*?<\/div>/i,
        `<div style='background-color:#fafaf9;border:1px dashed #d6d3d1;padding:12px 16px;border-radius:6px;font-size:12.5px;color:#57534e;line-height:1.6;'>${formattedVal}</div>`
      );
      setBodyHtmlTemplate(updated);
    }
  };

  const syncVisualEmailFromStatementPdf = () => {
    const activeStmt = getCachedStatementTemplate();
    const stmtLayout = parseStatementLayoutConfig(activeStmt?.displayColumnsJson);
    const stmtSet = new Set(stmtLayout.columns.map((c) => c.toUpperCase()));
    const syncedCols = EMAIL_INDICATOR_SWITCHES.map((s) => s.key).filter((k) => stmtSet.has(k));
    const nextCfg: VisualEmailConfig = {
      ...visualEmailCfg,
      columns: syncedCols.length > 0 ? syncedCols : ['BT', 'CD', 'TD', 'SG', 'VC', 'PMAX', 'COSPHI'],
    };
    applyVisualEmailConfig(nextCfg);
    message.success('Đã đồng bộ danh sách chỉ số từ Mẫu phiếu sang Email!');
  };

  const handleSave = async () => {
    if (!templateName.trim()) {
      message.warning('Vui lòng nhập tên mẫu');
      return;
    }
    if (!subjectTemplate.trim()) {
      message.warning('Vui lòng nhập tiêu đề email');
      return;
    }

    setSaving(true);
    try {
      const cleanedHtml = cleanLegacyBodyHtml(bodyHtmlTemplate);
      if (selectedTemplateId) {
        const updateData: UpdateNotificationTemplateRequest = {
          name: templateName.trim(),
          subjectTemplate: subjectTemplate.trim(),
          bodyHtmlTemplate: cleanedHtml,
          description: selectedTemplate?.description || '',
          isDefault: selectedTemplate?.isDefault ?? true,
          isActive: true,
        };
        await service.updateNotificationTemplate(selectedTemplateId, updateData);
        message.success('Đã lưu và cập nhật mẫu email thành công!');
        await loadTemplates(selectedTemplateId);
      } else {
        const createData: CreateNotificationTemplateRequest = {
          code: templateCode,
          name: templateName.trim(),
          subjectTemplate: subjectTemplate.trim(),
          bodyHtmlTemplate: cleanedHtml,
          description: '',
          isDefault: true,
          isActive: true,
        };
        const res: any = await service.createNotificationTemplate(createData);
        const created = res?.data?.data !== undefined ? res.data.data : res?.data;
        message.success('Đã tạo mới và lưu mẫu email thành công!');
        await loadTemplates(created?.id);
      }
    } catch (err: any) {
      message.error(`Lỗi khi lưu mẫu email: ${err?.message || err}`);
    } finally {
      setSaving(false);
    }
  };

  const handleSendTest = async () => {
    if (!testEmailTarget || !testEmailTarget.includes('@')) {
      message.warning('Vui lòng nhập địa chỉ email người nhận hợp lệ');
      return;
    }
    setTestSending(true);
    try {
      const res: any = await service.testSendNotificationTemplate({
        code: templateCode,
        toEmail: testEmailTarget.trim(),
        subjectTemplate: subjectTemplate,
        bodyHtmlTemplate: cleanLegacyBodyHtml(bodyHtmlTemplate),
      });
      const data = res?.data?.data !== undefined ? res.data.data : res?.data;
      if (res?.status === 200 || data === true || res?.success) {
        message.success(`Đã gửi email thử nghiệm thành công tới ${testEmailTarget}`);
        setTestSendOpen(false);
      } else {
        message.error('Không thể gửi email thử nghiệm, vui lòng kiểm tra cấu hình Mail Server');
      }
    } catch (err: any) {
      message.error(`Lỗi gửi email: ${err?.message || err}`);
    } finally {
      setTestSending(false);
    }
  };

  // Render mock data live preview (synced with DB CompanyName, CompanyAddress, ContactPhone, contactConfig)
  const livePreview = useMemo(() => {
    return renderWithMockData(
      bodyHtmlTemplate,
      subjectTemplate,
      companyName,
      contactConfig,
      companyAddress,
      companyPhone
    );
  }, [bodyHtmlTemplate, subjectTemplate, companyName, contactConfig, companyAddress, companyPhone]);

  return (
    <div className="w-full max-w-full overflow-x-hidden">
      <PageTitle title="Mẫu Email" />
      <div className="flex justify-between items-center mb-3">
        <BreadcrumbUI items={[{ name: 'Cài đặt hệ thống' }, { name: 'Mẫu Email' }]} />

        {/* Top Action Bar */}
        <div className="flex items-center gap-2">
          {/* Template Selector */}
          <Select
            style={{ width: 280 }}
            placeholder="Chọn mẫu email..."
            value={selectedTemplateId || undefined}
            onChange={handleSelectTemplate}
            options={templates.map((t) => ({
              value: t.id,
              label: (
                <div className="flex items-center justify-between">
                  <span className="truncate" style={{ maxWidth: 170 }}>
                    {t.name}
                  </span>
                  <Space size={4}>
                    {t.isDefault && (
                      <Tag color="gold" style={{ margin: 0, padding: '0 4px', fontSize: 10 }}>
                        Mặc định
                      </Tag>
                    )}
                    <Tag
                      color={
                        t.code === 'STATEMENT_NOTIFICATION'
                          ? 'magenta'
                          : t.code === 'USER_CREDENTIAL'
                          ? 'green'
                          : 'orange'
                      }
                      style={{ margin: 0, padding: '0 4px', fontSize: 10 }}
                    >
                      {t.code === 'STATEMENT_NOTIFICATION'
                        ? 'Phiếu báo'
                        : t.code === 'USER_CREDENTIAL'
                        ? 'Tài khoản'
                        : 'Mật khẩu'}
                    </Tag>
                  </Space>
                </div>
              ),
            }))}
          />

          {/* Gửi thử nghiệm */}
          <ButtonCommonCustomizable
            title={null}
            classButton="text-button-variant"
            tooltipText="Gửi email thử nghiệm"
            onClick={() => setTestSendOpen(true)}
            icon={<SendOutlined style={{ fontSize: 18 }} />}
          />

          {/* Lưu thay đổi */}
          <ButtonCommonCustomizable
            title={null}
            classButton="text-button-variant"
            tooltipText="Lưu thay đổi"
            disabled={saving}
            onClick={handleSave}
            icon={<SaveOutlined style={{ fontSize: 18 }} />}
          />
        </div>
      </div>

      {/* Main Split-Screen Container - Không cuộn ngang */}
      <Row gutter={[16, 16]} style={{ margin: 0, width: '100%' }}>
        {/* Left Column: Form Configuration */}
        <Col span={11}>
          <Card
            className="shadow-custom"
            style={{ borderRadius: 8 }}
            bodyStyle={{ padding: 16 }}
          >
            <div className="flex justify-between items-center mb-3 pb-2 border-b border-gray-100">
              <Space>
                <MailOutlined style={{ color: '#003366', fontSize: 16 }} />
                <Text strong style={{ fontSize: 15, color: '#003366' }}>
                  {selectedTemplateId ? `Cấu hình: ${templateName}` : 'Cấu hình mẫu Email'}
                </Text>
              </Space>
            </div>

            <div>
              <Tabs
                activeKey={activeTabKey}
                onChange={setActiveTabKey}
                items={[
                  {
                    key: 'general',
                    label: (
                      <Space size={6}>
                        <FileTextOutlined />
                        <span className="font-semibold">1. Thông tin chung</span>
                      </Space>
                    ),
                    children: (
                      <Form
                        layout="horizontal"
                        labelAlign="left"
                        labelCol={{ flex: '135px' }}
                        wrapperCol={{ flex: 'auto' }}
                        className="pt-2"
                      >
                        {/* Thuộc tính 1: Tên mẫu */}
                        <Form.Item label="Tên mẫu" required style={{ marginBottom: 14 }}>
                          <Input
                            value={templateName}
                            onChange={(e) => setTemplateName(e.target.value)}
                            placeholder="Ví dụ: Phiếu Báo Điện Năng - VSIP Hải Phòng"
                          />
                        </Form.Item>

                        {/* Thuộc tính 2: Nghiệp vụ */}
                        <Form.Item label="Nghiệp vụ" required style={{ marginBottom: 14 }}>
                          <Select
                            value={templateCode}
                            onChange={setTemplateCode}
                            options={TEMPLATE_CODES}
                            disabled={Boolean(selectedTemplateId)}
                          />
                        </Form.Item>

                        {/* Thuộc tính 3: Tiêu đề (Subject) */}
                        <Form.Item
                          label="Tiêu đề (Subject)"
                          required
                          tooltip="Hỗ trợ các biến tự động {{CustomerName}}, {{Period}}, {{SerialNumber}}..."
                          style={{ marginBottom: 14 }}
                        >
                          <Input
                            value={subjectTemplate}
                            onChange={(e) => setSubjectTemplate(e.target.value)}
                            placeholder="Ví dụ: Phiếu Báo Chỉ Số Điện Số {{SerialNumber}} - {{CustomerName}} (Kỳ {{Period}})"
                          />
                        </Form.Item>

                        {/* Thuộc tính 4: Nội dung Email */}
                        {templateCode === 'STATEMENT_NOTIFICATION' && (
                          <Form.Item
                            label="Nội dung Email"
                            tooltip="Đoạn văn mở đầu gửi tới khách hàng, hỗ trợ biến {{Period}}, {{FromDate}}, {{ToDate}}..."
                            style={{ marginBottom: 14 }}
                          >
                            <TextArea
                              autoSize={{ minRows: 4, maxRows: 6 }}
                              value={introText}
                              onChange={(e) => handleUpdateIntroText(e.target.value)}
                              placeholder="Chúng tôi xin trân trọng gửi tới Quý khách hàng bảng kê chỉ số công tơ..."
                            />
                          </Form.Item>
                        )}
                      </Form>
                    ),
                  },
                  {
                    key: 'headerFooter',
                    label: (
                      <Space size={6}>
                        <PictureOutlined />
                        <span className="font-semibold">2. Header & Footer</span>
                      </Space>
                    ),
                    children: (
                      <div className="pt-1 space-y-3">
                        <Card
                          size="small"
                          title={
                            <Text strong style={{ fontSize: 13, color: '#003366' }}>
                              Header
                            </Text>
                          }
                          style={{ background: '#f8fafc', borderColor: '#e2e8f0', borderRadius: 6 }}
                        >
                          <Form
                            layout="horizontal"
                            labelAlign="left"
                            labelCol={{ flex: '135px' }}
                            wrapperCol={{ flex: 'auto' }}
                          >
                            <Form.Item
                              label="Hệ thống"
                              required
                              tooltip="Dòng tên phần mềm/hệ thống hiển thị trên đầu thư"
                              style={{ marginBottom: 12 }}
                            >
                              <Input
                                value={visualEmailCfg.systemName}
                                onChange={(e) => handleUpdateHeader('systemName', e.target.value)}
                                placeholder="HỆ THỐNG THU THẬP DỮ LIỆU ĐO ĐẾM - MDMS"
                              />
                            </Form.Item>

                            <Form.Item
                              label="Tiêu đề"
                              required
                              tooltip="Dòng tiêu đề chính nổi bật trên nền màu của thư"
                              style={{ marginBottom: 12 }}
                            >
                              <Input
                                value={visualEmailCfg.headerTitle}
                                onChange={(e) => handleUpdateHeader('headerTitle', e.target.value)}
                                placeholder="THÔNG BÁO CHỈ SỐ & SẢN LƯỢNG ĐIỆN NĂNG"
                              />
                            </Form.Item>

                            <Form.Item label="Màu sắc" style={{ marginBottom: 12 }}>
                              <div className="flex items-center gap-2 flex-wrap">
                                <ColorPicker
                                  showText
                                  value={visualEmailCfg.themeColor}
                                  onChange={(color, hex) => {
                                    const chosenHex =
                                      hex || (typeof color === 'string' ? color : color?.toHexString?.() || '#b22222');
                                    applyVisualEmailConfig({ ...visualEmailCfg, themeColor: chosenHex });
                                  }}
                                />
                                <div className="flex items-center gap-1.5">
                                  {EMAIL_THEME_PRESETS.map((p) => {
                                    const isSelected =
                                      visualEmailCfg.themeColor.toLowerCase() === p.value.toLowerCase();
                                    return (
                                      <Tooltip title={p.label} key={p.value}>
                                        <button
                                          type="button"
                                          onClick={() => applyVisualEmailConfig({ ...visualEmailCfg, themeColor: p.value })}
                                          style={{
                                            backgroundColor: p.value,
                                            width: 28,
                                            height: 28,
                                            borderRadius: 6,
                                            cursor: 'pointer',
                                            border: isSelected ? '2px solid #003366' : '1px solid #cbd5e1',
                                            boxShadow: isSelected ? '0 0 0 2px #fff inset, 0 1px 3px rgba(0,0,0,0.25)' : 'none',
                                            transform: isSelected ? 'scale(1.08)' : 'scale(1)',
                                            transition: 'all 0.15s ease',
                                          }}
                                        />
                                      </Tooltip>
                                    );
                                  })}
                                </div>
                              </div>
                            </Form.Item>

                            <Form.Item label="Căn lề" style={{ marginBottom: 4 }}>
                              <Radio.Group
                                size="middle"
                                value={visualEmailCfg.headerAlign}
                                onChange={(e) => applyVisualEmailConfig({ ...visualEmailCfg, headerAlign: e.target.value })}
                                buttonStyle="solid"
                              >
                                <Radio.Button value="left">Trái</Radio.Button>
                                <Radio.Button value="center">Giữa</Radio.Button>
                                <Radio.Button value="right">Phải</Radio.Button>
                              </Radio.Group>
                            </Form.Item>
                          </Form>
                        </Card>

                        <Card
                          size="small"
                          title={
                            <Text strong style={{ fontSize: 13, color: '#003366' }}>
                              Footer
                            </Text>
                          }
                          style={{ background: '#f8fafc', borderColor: '#e2e8f0', borderRadius: 6 }}
                        >
                          <Form
                            layout="horizontal"
                            labelAlign="left"
                            labelCol={{ flex: '135px' }}
                            wrapperCol={{ flex: 'auto' }}
                          >
                            <Form.Item
                              label="Thông tin liên hệ"
                              tooltip="Hiển thị ở chân thư: Lời dẫn tiếp nhận, Hotline và Email đối soát"
                              style={{ marginBottom: 4 }}
                            >
                              <TextArea
                                autoSize={{ minRows: 4, maxRows: 8 }}
                                value={contactNote}
                                onChange={(e) => handleUpdateContactNote(e.target.value)}
                                placeholder="Mọi thắc mắc hoặc yêu cầu đối soát số liệu đo đếm, Quý khách vui lòng liên hệ..."
                              />
                            </Form.Item>
                          </Form>
                        </Card>
                      </div>
                    ),
                  },
                  ...(templateCode === 'STATEMENT_NOTIFICATION'
                    ? [
                        {
                          key: 'metrics',
                          label: (
                            <Space size={6}>
                              <ControlOutlined />
                              <span className="font-semibold">3. Chỉ số</span>
                            </Space>
                          ),
                          children: (
                            <div className="pt-1">
                              <Card
                                size="small"
                                style={{
                                  background: '#f8fafc',
                                  border: '1px solid #cbd5e1',
                                  borderRadius: 6,
                                }}
                                title={
                                  <Text strong style={{ fontSize: 13, color: '#003366' }}>
                                    Chỉ số điện
                                  </Text>
                                }
                              >
                                <p style={{ fontSize: 12, color: '#64748b', marginBottom: 12 }}>
                                  Bật hoặc tắt các chỉ số điện năng hiển thị trong Email gửi tới khách hàng.
                                </p>

                                <div className="space-y-2">
                                  {EMAIL_INDICATOR_SWITCHES.map((sw) => {
                                    const isChecked = visualEmailCfg.columns.includes(sw.key);
                                    return (
                                      <div
                                        key={sw.key}
                                        onClick={() => {
                                          const nextVal = !isChecked;
                                          const nextCols = nextVal
                                            ? Array.from(new Set([...visualEmailCfg.columns, sw.key]))
                                            : visualEmailCfg.columns.filter((x) => x !== sw.key);
                                          applyVisualEmailConfig({ ...visualEmailCfg, columns: nextCols });
                                        }}
                                        className="flex justify-between items-center px-3 py-2.5 rounded-md border cursor-pointer transition-colors hover:border-blue-400"
                                        style={{
                                          backgroundColor: isChecked ? '#ffffff' : '#f8fafc',
                                          borderColor: isChecked ? '#cbd5e1' : '#e2e8f0',
                                        }}
                                      >
                                        <span className="font-semibold text-[13px] text-gray-800 select-none">
                                          {sw.label}
                                        </span>
                                        <Checkbox
                                          checked={isChecked}
                                          onChange={(e) => {
                                            e.stopPropagation();
                                            const nextCols = e.target.checked
                                              ? Array.from(new Set([...visualEmailCfg.columns, sw.key]))
                                              : visualEmailCfg.columns.filter((x) => x !== sw.key);
                                            applyVisualEmailConfig({ ...visualEmailCfg, columns: nextCols });
                                          }}
                                        />
                                      </div>
                                    );
                                  })}
                                </div>
                              </Card>
                            </div>
                          ),
                        },
                      ]
                    : []),
                ]}
              />
            </div>
          </Card>
        </Col>

        {/* Right Column: 100% Live Interactive Email Preview */}
        <Col span={13}>
          <Card
            className="shadow-custom"
            style={{ borderRadius: 8 }}
            bodyStyle={{ padding: 14 }}
          >
            {/* Live Preview Header Bar - Xem trước Email */}
            <div className="flex items-center mb-2 pb-2 border-b border-gray-200">
              <Space>
                <EyeOutlined style={{ color: '#003366', fontSize: 17 }} />
                <Text strong style={{ fontSize: 14, color: '#003366' }}>
                  Xem trước Email
                </Text>
              </Space>
            </div>

            {/* Email Client Simulated Container */}
            <div
              style={{
                background: '#f8fafc',
                border: '1px solid #e2e8f0',
                borderRadius: 8,
                overflow: 'hidden',
                boxShadow: 'inset 0 1px 3px rgba(0,0,0,0.03)',
                maxWidth: '100%',
              }}
            >
              {/* Fake Email Client Metadata Header */}
              <div
                style={{
                  background: '#f1f5f9',
                  padding: '10px 16px',
                  borderBottom: '1px solid #e2e8f0',
                  fontSize: 12,
                }}
              >
                <div style={{ display: 'flex', alignItems: 'baseline', gap: 16, marginBottom: 8, width: '100%' }}>
                  <Text type="secondary" style={{ width: 140, flexShrink: 0, whiteSpace: 'nowrap' }}>
                    Người gửi:
                  </Text>
                  <Text strong style={{ color: '#334155', flex: 1, wordBreak: 'break-word' }}>
                    {companyName} &lt;{contactConfig.careEmail}&gt;
                  </Text>
                </div>
                <div style={{ display: 'flex', alignItems: 'baseline', gap: 16, marginBottom: 8, width: '100%' }}>
                  <Text type="secondary" style={{ width: 140, flexShrink: 0, whiteSpace: 'nowrap' }}>
                    Người nhận:
                  </Text>
                  <Text strong style={{ color: '#334155', flex: 1, wordBreak: 'break-word' }}>
                    {recipientDisplay}
                  </Text>
                </div>
                <div style={{ display: 'flex', alignItems: 'baseline', gap: 16, width: '100%' }}>
                  <Text type="secondary" style={{ width: 140, flexShrink: 0, whiteSpace: 'nowrap' }}>
                    Tiêu đề (Subject):
                  </Text>
                  <Text strong style={{ color: '#003366', fontSize: 13, flex: 1, wordBreak: 'break-word' }}>
                    {livePreview.renderedSubj || '(Chưa có tiêu đề)'}
                  </Text>
                </div>
              </div>

              {/* Rendered HTML Email Content - Không cuộn ngang */}
              <div
                style={{
                  background: '#ffffff',
                  maxHeight: 'calc(100vh - 280px)',
                  minHeight: 460,
                  overflowY: 'auto',
                  overflowX: 'hidden',
                  padding: 8,
                  width: '100%',
                }}
                dangerouslySetInnerHTML={{
                  __html: (livePreview.renderedHtml || '')
                    .replace(/<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>/gi, '')
                    .replace(/\son\w+\s*=\s*(?:"[^"]*"|'[^']*'|[^\s>]+)/gi, '')
                    .replace(/javascript\s*:/gi, ''),
                }}
              />
            </div>
          </Card>
        </Col>
      </Row>

      <FullPageLoading isLoading={loading} />

      {/* Modal Test Send Email */}
      <Modal
        title={
          <Space>
            <SendOutlined style={{ color: '#003366' }} />
            <Text strong>Gửi email thử nghiệm</Text>
          </Space>
        }
        open={testSendOpen}
        onCancel={() => setTestSendOpen(false)}
        onOk={handleSendTest}
        confirmLoading={testSending}
        okText="Gửi ngay"
        cancelText="Hủy"
        okButtonProps={{ style: { backgroundColor: '#003366', borderColor: '#003366' } }}
        width={500}
      >
        <p style={{ marginTop: 8, marginBottom: 14, color: '#64748b', fontSize: 13 }}>
          Hệ thống sẽ biên dịch mẫu email này cùng thông tin liên hệ và gửi trực tiếp qua Mail Server tới hòm thư của bạn.
        </p>
        <Form layout="vertical">
          <Form.Item label="Địa chỉ Email người nhận thử nghiệm" required>
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
