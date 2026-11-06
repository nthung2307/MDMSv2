import React, { useEffect, useState } from 'react';
import {
  Button,
  Card,
  Checkbox,
  Col,
  Drawer,
  Form,
  Image,
  Input,
  Modal,
  Popconfirm,
  Radio,
  Row,
  Space,
  Spin,
  Switch,
  Table,
  Tag,
  Tooltip,
  Typography,
  Upload,
  message,
} from 'antd';
import {
  AppstoreOutlined,
  CheckCircleOutlined,
  DeleteOutlined,
  DownloadOutlined,
  EditOutlined,
  EyeOutlined,
  FileExcelOutlined,
  FilePdfOutlined,
  PlusOutlined,
  ReloadOutlined,
  StarFilled,
  StarOutlined,
  UploadOutlined,
} from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import type { UploadFile } from 'antd/es/upload/interface';

import {
  StatementPdfTemplateDto,
  CreateStatementPdfTemplateRequest,
  UpdateStatementPdfTemplateRequest,
  PreviewStatementPdfTemplateRequest,
} from '@/core/application/interfaces/methods/templates/templateManagement.interface';
import { ButtonCommonCustomizable } from '@/infrastructure/common/components/button/button-common-customizable';
import { BreadcrumbUI } from '@/infrastructure/common/components/controls/BreadcrumbUI';
import { FullPageLoading } from '@/infrastructure/common/components/controls/Loading';
import PageTitle from '@/infrastructure/common/components/title/page-title';
import { TemplateManagementService } from '@/infrastructure/repository/templates/services/templateManagement.service';
import { _t } from '@/infrastructure/utils/translation';

const { Text, Paragraph } = Typography;
const { TextArea } = Input;

const AVAILABLE_COLUMNS = [
  { label: 'Bình thường (BT)', value: 'BT' },
  { label: 'Cao điểm (CD)', value: 'CD' },
  { label: 'Thấp điểm (TD)', value: 'TD' },
  { label: 'Tổng hữu công (SG)', value: 'SG' },
  { label: 'Vô công (VC - kVARh)', value: 'VC' },
  { label: 'Công suất Pmax (kW)', value: 'PMAX' },
  { label: 'Hệ số Cos φ', value: 'COSPHI' },
];

interface PrebuiltPdfProfile {
  id: string;
  name: string;
  tag: string;
  badgeColor: string;
  description: string;
  code: string;
  headerTitle: string;
  subHeaderTitle: string;
  companyName: string;
  companyAddress: string;
  contactPhone: string;
  logoBase64?: string;
  paperSize: string;
  displayColumns: string[];
  requireDualSignatures: boolean;
  footerNotes: string;
}

const VSIP_LOGO_BASE64 = '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';

const PREBUILT_PDF_PROFILES: PrebuiltPdfProfile[] = [
  {
    id: 'a5-standard',
    name: 'Mẫu A5 Tiêu chuẩn',
    tag: 'Tiêu chuẩn',
    badgeColor: '#b22222',
    description: 'Định dạng A5 ngang chuẩn đo đếm điện lực, đầy đủ bảng kê 3 giá, công suất Pmax và chữ ký biên bản giao nhận.',
    code: 'STATEMENT_PDF_A5_STANDARD',
    headerTitle: 'PHIẾU BÁO CHỈ SỐ ĐIỆN NĂNG',
    subHeaderTitle: '(MONTHLY POWER METER READING)',
    companyName: 'ĐƠN VỊ CUNG CẤP ĐIỆN NĂNG',
    companyAddress: 'Khu công nghiệp / Đô thị tập trung',
    contactPhone: 'Điện thoại: (024) 3xxx xxxx - Fax: (024) 3xxx xxxx',
    logoBase64: VSIP_LOGO_BASE64,
    paperSize: 'A5_LANDSCAPE',
    displayColumns: ['BT', 'CD', 'TD', 'SG', 'VC', 'PMAX', 'COSPHI'],
    requireDualSignatures: true,
    footerNotes: 'Quý khách vui lòng kiểm tra và phản hồi số liệu trong vòng 03 ngày làm việc kể từ ngày nhận phiếu báo.',
  },
  {
    id: 'a4-full',
    name: 'Mẫu A4 Đầy đủ',
    tag: 'Đầy đủ',
    badgeColor: '#1890ff',
    description: 'Định dạng A4 dọc chi tiết, phù hợp hồ sơ quyết toán và lưu trữ kế toán, đầy đủ biểu giá, Pmax, Cos phi và điều khoản pháp lý.',
    code: 'STATEMENT_PDF_A4_FULL',
    headerTitle: 'PHIẾU BÁO CHỈ SỐ VÀ SẢN LƯỢNG ĐIỆN NĂNG',
    subHeaderTitle: 'BÁO CÁO ĐO ĐẾM CHỈ SỐ ĐIỆN KỲ HÀNG THÁNG',
    companyName: 'ĐƠN VỊ CUNG CẤP ĐIỆN NĂNG',
    companyAddress: 'Khu công nghiệp / Đô thị tập trung',
    contactPhone: 'Điện thoại: (024) 3xxx xxxx - Bộ phận Vận hành Đo đếm',
    logoBase64: VSIP_LOGO_BASE64,
    paperSize: 'A4_PORTRAIT',
    displayColumns: ['BT', 'CD', 'TD', 'SG', 'VC', 'PMAX', 'COSPHI'],
    requireDualSignatures: true,
    footerNotes: 'Phiếu báo này được kết xuất tự động từ hệ thống đo đếm từ xa AMR/MDMS. Mọi yêu cầu đối soát số liệu kỹ thuật xin liên hệ Bộ phận Vận hành Đo đếm.',
  },
  {
    id: 'a5-compact',
    name: 'Mẫu A5 Tinh gọn',
    tag: 'Tinh gọn',
    badgeColor: '#52c41a',
    description: 'Định dạng A5 ngang tinh gọn, tập trung vào các chỉ số sản lượng cốt lõi (BT, CD, TD, SG), phù hợp bàn giao nhanh.',
    code: 'STATEMENT_PDF_A5_COMPACT',
    headerTitle: 'PHIẾU GHI CHỈ SỐ ĐIỆN HÀNG THÁNG',
    subHeaderTitle: 'BẢNG KÊ CHỈ SỐ VÀ TIÊU THỤ ĐIỆN NĂNG',
    companyName: 'ĐƠN VỊ CUNG CẤP ĐIỆN NĂNG',
    companyAddress: 'Khu công nghiệp / Đô thị tập trung',
    contactPhone: 'Điện thoại: (024) 3xxx xxxx',
    paperSize: 'A5_LANDSCAPE',
    displayColumns: ['BT', 'CD', 'TD', 'SG'],
    requireDualSignatures: false,
    footerNotes: 'Số liệu được ghi nhận tự động từ hệ thống công tơ điện tử đo xa.',
  },
];

export const StatementPdfTemplatesPage: React.FC = () => {
  const service = new TemplateManagementService();
  const [loading, setLoading] = useState<boolean>(false);
  const [templates, setTemplates] = useState<StatementPdfTemplateDto[]>([]);

  // Drawer Create/Edit
  const [drawerOpen, setDrawerOpen] = useState<boolean>(false);
  const [editingTemplate, setEditingTemplate] = useState<StatementPdfTemplateDto | null>(null);
  const [form] = Form.useForm();
  const [submitting, setSubmitting] = useState<boolean>(false);
  const [logoBase64, setLogoBase64] = useState<string>('');
  const [excelTemplateBase64, setExcelTemplateBase64] = useState<string>('');
  const [excelTemplateFileName, setExcelTemplateFileName] = useState<string>('');
  const [selectedEngine, setSelectedEngine] = useState<string>('EXCEL_LIBREOFFICE');

  // Library Modal
  const [libraryModalOpen, setLibraryModalOpen] = useState<boolean>(false);

  // PDF Preview Modal
  const [previewOpen, setPreviewOpen] = useState<boolean>(false);
  const [previewPdfUrl, setPreviewPdfUrl] = useState<string>('');
  const [previewLoading, setPreviewLoading] = useState<boolean>(false);

  const fetchTemplates = async () => {
    setLoading(true);
    try {
      const res: any = await service.getStatementPdfTemplates();
      const rawData = res?.data?.data !== undefined ? res.data.data : res?.data;
      setTemplates(Array.isArray(rawData) ? rawData : []);
    } catch (err: any) {
      message.error(`Không thể tải danh sách mẫu phiếu báo: ${err?.message || err}`);
      setTemplates([]);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchTemplates();
  }, []);

  const handleDownloadDefaultExcel = async (paperSize: string = 'A5_LANDSCAPE') => {
    try {
      const blob = await service.downloadDefaultExcelTemplate(paperSize);
      const url = window.URL.createObjectURL(new Blob([blob]));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', `Mau_Phieu_Bao_${paperSize}_${new Date().toISOString().slice(0, 10)}.xlsx`);
      document.body.appendChild(link);
      link.click();
      link.parentNode?.removeChild(link);
      message.success('Tải mẫu Excel chuẩn thành công');
    } catch (err: any) {
      message.error(`Không thể tải tệp mẫu Excel: ${err?.message || err}`);
    }
  };

  const handleExcelUpload = (file: File) => {
    const isXlsx = file.name.endsWith('.xlsx') || file.name.endsWith('.xls') || file.type.includes('spreadsheet') || file.type.includes('excel');
    if (!isXlsx) {
      message.error('Vui lòng chỉ tải lên tệp định dạng Excel (.xlsx)!');
      return false;
    }
    const isLt10M = file.size / 1024 / 1024 < 10;
    if (!isLt10M) {
      message.error('Kích thước tệp Excel phải nhỏ hơn 10MB!');
      return false;
    }

    const reader = new FileReader();
    reader.onload = (e) => {
      const base64Str = e.target?.result as string;
      const cleanBase64 = base64Str.includes(',') ? base64Str.split(',')[1] : base64Str;
      setExcelTemplateBase64(cleanBase64);
      setExcelTemplateFileName(file.name);
      message.success(`Đã nạp tệp mẫu Excel "${file.name}" thành công!`);
    };
    reader.readAsDataURL(file);
    return false;
  };

  const handleOpenCreate = () => {
    setEditingTemplate(null);
    const defaultProfile = PREBUILT_PDF_PROFILES[0];
    setLogoBase64(defaultProfile.logoBase64 || VSIP_LOGO_BASE64);
    setExcelTemplateBase64('');
    setExcelTemplateFileName('');
    setSelectedEngine('EXCEL_LIBREOFFICE');
    form.resetFields();
    form.setFieldsValue({
      code: `PDF_${Date.now().toString().slice(-4)}`,
      name: defaultProfile.name,
      engineType: 'EXCEL_LIBREOFFICE',
      headerTitle: defaultProfile.headerTitle,
      subHeaderTitle: defaultProfile.subHeaderTitle,
      companyName: defaultProfile.companyName,
      companyAddress: defaultProfile.companyAddress,
      contactPhone: defaultProfile.contactPhone,
      paperSize: defaultProfile.paperSize,
      displayColumns: defaultProfile.displayColumns,
      requireDualSignatures: defaultProfile.requireDualSignatures,
      footerNotes: defaultProfile.footerNotes,
      isDefault: false,
      isActive: true,
    });
    setDrawerOpen(true);
  };

  const handleOpenEdit = (record: StatementPdfTemplateDto) => {
    setEditingTemplate(record);
    setLogoBase64(record.logoBase64 || '');
    setExcelTemplateBase64(record.excelTemplateBase64 || '');
    setExcelTemplateFileName(record.excelTemplateFileName || '');
    setSelectedEngine(record.engineType || 'EXCEL_LIBREOFFICE');
    let displayCols = ['BT', 'CD', 'TD', 'SG', 'VC', 'PMAX', 'COSPHI'];
    if (record.displayColumnsJson) {
      try {
        displayCols = JSON.parse(record.displayColumnsJson);
      } catch {}
    }
    form.resetFields();
    form.setFieldsValue({
      code: record.code,
      name: record.name,
      engineType: record.engineType || 'EXCEL_LIBREOFFICE',
      headerTitle: record.headerTitle,
      subHeaderTitle: record.subHeaderTitle,
      companyName: record.companyName,
      companyAddress: record.companyAddress,
      contactPhone: record.contactPhone,
      paperSize: record.paperSize || 'A5_LANDSCAPE',
      displayColumns: displayCols,
      requireDualSignatures: record.requireDualSignatures,
      footerNotes: record.footerNotes,
      isDefault: record.isDefault,
      isActive: record.isActive,
    });
    setDrawerOpen(true);
  };

  const handleApplyProfile = (profile: PrebuiltPdfProfile) => {
    if (profile.logoBase64) {
      setLogoBase64(profile.logoBase64);
    }
    form.setFieldsValue({
      name: profile.name,
      code: editingTemplate ? editingTemplate.code : profile.code,
      headerTitle: profile.headerTitle,
      subHeaderTitle: profile.subHeaderTitle,
      companyName: profile.companyName,
      companyAddress: profile.companyAddress,
      contactPhone: profile.contactPhone,
      paperSize: profile.paperSize,
      displayColumns: profile.displayColumns,
      requireDualSignatures: profile.requireDualSignatures,
      footerNotes: profile.footerNotes,
    });
    message.success(`Đã áp dụng cấu hình "${profile.name}"`);
    setLibraryModalOpen(false);
  };

  const handleResetSystemDefaults = async () => {
    setLoading(true);
    try {
      await service.resetDefaultStatementPdfTemplates();
      message.success('Đã khôi phục thành công các mẫu phiếu báo PDF tiêu chuẩn của hệ thống');
      fetchTemplates();
    } catch (err: any) {
      message.error(`Không thể khôi phục mẫu chuẩn: ${err?.message || err}`);
    } finally {
      setLoading(false);
    }
  };

  const handleSetDefault = async (record: StatementPdfTemplateDto) => {
    try {
      await service.setDefaultStatementPdfTemplate(record.id);
      message.success(`Đã thiết lập mẫu "${record.name}" làm mặc định cho hệ thống`);
      fetchTemplates();
    } catch (err: any) {
      message.error(`Không thể đặt mặc định: ${err?.message || err}`);
    }
  };

  const handleDelete = async (record: StatementPdfTemplateDto) => {
    try {
      await service.deleteStatementPdfTemplate(record.id);
      message.success(`Đã xóa mẫu phiếu báo "${record.name}"`);
      fetchTemplates();
    } catch (err: any) {
      message.error(`Không thể xóa mẫu phiếu báo: ${err?.message || err}`);
    }
  };

  const handleSubmit = async () => {
    try {
      const values = await form.validateFields();
      setSubmitting(true);

      const displayColsJson = JSON.stringify(values.displayColumns || []);

      if (editingTemplate) {
        const updateData: UpdateStatementPdfTemplateRequest = {
          name: values.name,
          headerTitle: values.headerTitle,
          subHeaderTitle: values.subHeaderTitle,
          companyName: values.companyName,
          companyAddress: values.companyAddress,
          contactPhone: values.contactPhone,
          logoBase64: logoBase64,
          paperSize: values.paperSize,
          engineType: values.engineType || 'EXCEL_LIBREOFFICE',
          excelTemplateBase64: excelTemplateBase64,
          excelTemplateFileName: excelTemplateFileName,
          displayColumnsJson: displayColsJson,
          requireDualSignatures: values.requireDualSignatures,
          footerNotes: values.footerNotes,
          isDefault: values.isDefault,
          isActive: values.isActive,
        };
        await service.updateStatementPdfTemplate(editingTemplate.id, updateData);
        message.success('Cập nhật mẫu phiếu báo PDF thành công');
      } else {
        const createData: CreateStatementPdfTemplateRequest = {
          code: values.code,
          name: values.name,
          headerTitle: values.headerTitle,
          subHeaderTitle: values.subHeaderTitle,
          companyName: values.companyName,
          companyAddress: values.companyAddress,
          contactPhone: values.contactPhone,
          logoBase64: logoBase64,
          paperSize: values.paperSize,
          engineType: values.engineType || 'EXCEL_LIBREOFFICE',
          excelTemplateBase64: excelTemplateBase64,
          excelTemplateFileName: excelTemplateFileName,
          displayColumnsJson: displayColsJson,
          requireDualSignatures: values.requireDualSignatures,
          footerNotes: values.footerNotes,
          isDefault: values.isDefault,
          isActive: values.isActive,
        };
        await service.createStatementPdfTemplate(createData);
        message.success('Tạo mới mẫu phiếu báo PDF thành công');
      }

      setDrawerOpen(false);
      fetchTemplates();
    } catch (err: any) {
      message.error(`Lỗi khi lưu mẫu phiếu báo: ${err?.message || err}`);
    } finally {
      setSubmitting(false);
    }
  };

  const handlePreviewDirect = async (record: StatementPdfTemplateDto) => {
    setPreviewLoading(true);
    setPreviewOpen(true);
    setPreviewPdfUrl('');
    try {
      const blob = await service.previewStatementPdfTemplate({
        headerTitle: record.headerTitle,
        subHeaderTitle: record.subHeaderTitle,
        companyName: record.companyName,
        companyAddress: record.companyAddress,
        contactPhone: record.contactPhone,
        logoBase64: record.logoBase64,
        paperSize: record.paperSize,
        engineType: record.engineType || 'EXCEL_LIBREOFFICE',
        excelTemplateBase64: record.excelTemplateBase64,
        excelTemplateFileName: record.excelTemplateFileName,
        displayColumnsJson: record.displayColumnsJson,
        requireDualSignatures: record.requireDualSignatures,
        footerNotes: record.footerNotes,
      });

      const url = URL.createObjectURL(new Blob([blob], { type: 'application/pdf' }));
      setPreviewPdfUrl(url);
    } catch (err: any) {
      message.error(`Không thể xem trước tệp PDF: ${err?.message || err}`);
    } finally {
      setPreviewLoading(false);
    }
  };

  const handlePreviewFromForm = async () => {
    try {
      const values = form.getFieldsValue();
      setPreviewLoading(true);
      setPreviewOpen(true);
      setPreviewPdfUrl('');

      const blob = await service.previewStatementPdfTemplate({
        headerTitle: values.headerTitle,
        subHeaderTitle: values.subHeaderTitle,
        companyName: values.companyName,
        companyAddress: values.companyAddress,
        contactPhone: values.contactPhone,
        logoBase64: logoBase64,
        paperSize: values.paperSize,
        engineType: values.engineType || 'EXCEL_LIBREOFFICE',
        excelTemplateBase64: excelTemplateBase64,
        excelTemplateFileName: excelTemplateFileName,
        displayColumnsJson: JSON.stringify(values.displayColumns || []),
        requireDualSignatures: values.requireDualSignatures,
        footerNotes: values.footerNotes,
      });

      const url = URL.createObjectURL(new Blob([blob], { type: 'application/pdf' }));
      setPreviewPdfUrl(url);
    } catch (err: any) {
      message.error(`Không thể xem trước tệp PDF: ${err?.message || err}`);
    } finally {
      setPreviewLoading(false);
    }
  };

  const handleLogoUpload = (file: File) => {
    const isImage = file.type.startsWith('image/');
    if (!isImage) {
      message.error('Vui lòng chỉ tải lên tệp hình ảnh (PNG, JPG, SVG)!');
      return false;
    }
    const isLt2M = file.size / 1024 / 1024 < 2;
    if (!isLt2M) {
      message.error('Kích thước ảnh logo phải nhỏ hơn 2MB!');
      return false;
    }

    const reader = new FileReader();
    reader.onload = (e) => {
      const base64Str = e.target?.result as string;
      const cleanBase64 = base64Str.includes(',') ? base64Str.split(',')[1] : base64Str;
      setLogoBase64(cleanBase64);
      message.success('Tải ảnh logo lên thành công');
    };
    reader.readAsDataURL(file);
    return false;
  };

  const columns: ColumnsType<StatementPdfTemplateDto> = [
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
          <Text type="secondary" style={{ fontSize: 12 }}>{record.companyName}</Text>
        </Space>
      ),
    },
    {
      title: 'Mã mẫu',
      dataIndex: 'code',
      key: 'code',
      width: 170,
      render: (code) => <Text code style={{ fontSize: 12 }}>{code}</Text>,
    },
    {
      title: 'Khổ giấy',
      dataIndex: 'paperSize',
      key: 'paperSize',
      width: 130,
      render: (paper) => (
        <Tag color={paper === 'A5_LANDSCAPE' ? 'purple' : 'geekblue'}>
          {paper === 'A5_LANDSCAPE' ? 'A5 Ngang' : 'A4 Dọc'}
        </Tag>
      ),
    },
    {
      title: 'Công nghệ',
      dataIndex: 'engineType',
      key: 'engineType',
      width: 175,
      render: (engine, record) => {
        const isExcel = (engine || 'EXCEL_LIBREOFFICE') === 'EXCEL_LIBREOFFICE';
        return (
          <Space direction="vertical" size={1}>
            <Tag color={isExcel ? 'green' : 'blue'} icon={isExcel ? <FileExcelOutlined /> : <FilePdfOutlined />}>
              {isExcel ? 'Excel (LibreOffice)' : 'C# (QuestPDF)'}
            </Tag>
            {isExcel && record.excelTemplateFileName && (
              <Text type="secondary" ellipsis style={{ fontSize: 11, maxWidth: 140 }}>
                {record.excelTemplateFileName}
              </Text>
            )}
          </Space>
        );
      },
    },
    {
      title: 'Chữ ký',
      dataIndex: 'requireDualSignatures',
      key: 'requireDualSignatures',
      width: 130,
      align: 'center',
      render: (dual) => (
        <Tag color={dual ? 'blue' : 'default'}>
          {dual ? '2 Chữ ký' : '1 Chữ ký'}
        </Tag>
      ),
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
      width: 180,
      align: 'center',
      render: (_, record) => (
        <Space size="small">
          <Tooltip title="Xem trước tệp PDF kết xuất mẫu">
            <Button
              type="text"
              icon={<EyeOutlined style={{ color: '#1890ff', fontSize: 16 }} />}
              onClick={() => handlePreviewDirect(record)}
            />
          </Tooltip>
          <Tooltip title="Chỉnh sửa cấu hình">
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
              title="Xóa mẫu phiếu báo"
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
        <BreadcrumbUI items={[{ name: 'Cài đặt hệ thống' }, { name: 'Mẫu phiếu báo chỉ số điện' }]} />
        <div className='flex items-center gap-2'>
          <Button
            icon={<FileExcelOutlined style={{ color: '#107c41' }} />}
            onClick={() => handleDownloadDefaultExcel('A5_LANDSCAPE')}
            style={{ borderColor: '#107c41', color: '#107c41', fontWeight: 500 }}
          >
            Tải file mẫu (.xlsx)
          </Button>

          <Button
            icon={<AppstoreOutlined />}
            onClick={() => setLibraryModalOpen(true)}
            style={{ borderColor: '#003366', color: '#003366', fontWeight: 500 }}
          >
            Thư viện mẫu
          </Button>

          <Popconfirm
            title="Khôi phục mẫu mặc định"
            description="Thao tác này sẽ đảm bảo các mẫu phiếu báo PDF mặc định (A5 Ngang, A4 Dọc) được nạp lại đầy đủ vào hệ thống. Tiếp tục?"
            onConfirm={handleResetSystemDefaults}
            okText="Khôi phục"
            cancelText="Hủy"
          >
            <Button icon={<ReloadOutlined />} title="Nạp lại các mẫu phiếu báo mặc định">
              Khôi phục mặc định
            </Button>
          </Popconfirm>

          <ButtonCommonCustomizable
            title={_t('Thêm mới')}
            tooltipText={'Tạo mẫu phiếu báo mới'}
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
                <h1 className='font-bold text-20-20'>{'Mẫu phiếu báo chỉ số điện'}</h1>
                <Text type="secondary" style={{ fontSize: 13 }}>
                  Tùy biến cấu hình phiếu báo điện năng (PDF đính kèm email và kết xuất in ấn). Dễ dàng chọn khổ giấy A5/A4, cấu hình thông tin đơn vị, ẩn hiện các cột sản lượng và chữ ký xác nhận.
                </Text>
              </div>
            </div>

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

      {/* Drawer Create / Edit */}
      <Drawer
        title={editingTemplate ? `Chỉnh sửa mẫu phiếu báo: ${editingTemplate.name}` : 'Tạo mới mẫu phiếu báo'}
        width={900}
        open={drawerOpen}
        onClose={() => setDrawerOpen(false)}
        extra={
          <Space>
            <Button
              icon={<AppstoreOutlined />}
              onClick={() => setLibraryModalOpen(true)}
              style={{ color: '#003366', borderColor: '#003366' }}
            >
              Chọn mẫu sẵn
            </Button>
            <Button icon={<EyeOutlined />} onClick={handlePreviewFromForm}>
              Xem trước
            </Button>
            <Button onClick={() => setDrawerOpen(false)}>Hủy</Button>
            <Button
              type="primary"
              onClick={handleSubmit}
              loading={submitting}
              style={{ backgroundColor: '#003366', borderColor: '#003366' }}
            >
              Lưu
            </Button>
          </Space>
        }
      >
        {/* Quick Presets Bar */}
        <Card
          size="small"
          style={{ marginBottom: 16, background: '#f8fafc', border: '1px solid #cbd5e1', borderRadius: 8 }}
          title={
            <Space>
              <FilePdfOutlined style={{ color: '#003366' }} />
              <Text strong style={{ fontSize: 13, color: '#0f172a' }}>
                Mẫu cấu hình sẵn
              </Text>
            </Space>
          }
        >
          <Space wrap size={[8, 8]}>
            {PREBUILT_PDF_PROFILES.map((p) => (
              <Button
                key={p.id}
                size="small"
                onClick={() => handleApplyProfile(p)}
                style={{ borderColor: p.badgeColor, color: p.badgeColor }}
              >
                + {p.name}
              </Button>
            ))}
          </Space>
        </Card>

        <Form form={form} layout="vertical">
          <Row gutter={16}>
            <Col span={14}>
              <Form.Item
                name="name"
                label="Tên mẫu"
                rules={[{ required: true, message: 'Vui lòng nhập tên mẫu phiếu báo' }]}
              >
                <Input placeholder="Ví dụ: Mẫu A5 Tiêu chuẩn" />
              </Form.Item>
            </Col>
            <Col span={10}>
              <Form.Item
                name="code"
                label="Mã mẫu"
                rules={[{ required: true, message: 'Vui lòng nhập mã định danh mẫu' }]}
              >
                <Input disabled={!!editingTemplate} placeholder="Ví dụ: A5_STANDARD" />
              </Form.Item>
            </Col>
          </Row>

          <Row gutter={16}>
            <Col span={12}>
              <Form.Item
                name="paperSize"
                label="Khổ giấy"
                rules={[{ required: true }]}
              >
                <Radio.Group className="w-full" style={{ display: 'flex', width: '100%', gap: 12 }}>
                  <Radio.Button
                    value="A5_LANDSCAPE"
                    style={{
                      flex: 1,
                      height: 40,
                      display: 'inline-flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      borderRadius: 6,
                      borderLeftWidth: 1,
                      fontWeight: 500,
                    }}
                  >
                    A5 Ngang
                  </Radio.Button>
                  <Radio.Button
                    value="A4_PORTRAIT"
                    style={{
                      flex: 1,
                      height: 40,
                      display: 'inline-flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      borderRadius: 6,
                      borderLeftWidth: 1,
                      fontWeight: 500,
                    }}
                  >
                    A4 Dọc
                  </Radio.Button>
                </Radio.Group>
              </Form.Item>
            </Col>
            <Col span={6}>
              <Form.Item name="isDefault" label="Mặc định" valuePropName="checked">
                <Switch />
              </Form.Item>
            </Col>
            <Col span={6}>
              <Form.Item name="isActive" label="Kích hoạt" valuePropName="checked">
                <Switch defaultChecked />
              </Form.Item>
            </Col>
          </Row>

          <Row gutter={16}>
            <Col span={24}>
              <Form.Item
                name="engineType"
                label="Công nghệ kết xuất"
                tooltip="Chọn phương thức tạo tệp PDF: Kết xuất trực tiếp từ file Excel qua LibreOffice hoặc mã C# QuestPDF"
                rules={[{ required: true }]}
              >
                <Radio.Group
                  className="w-full"
                  onChange={(e) => setSelectedEngine(e.target.value)}
                  style={{ display: 'flex', width: '100%', gap: 12 }}
                >
                  <Radio.Button
                    value="EXCEL_LIBREOFFICE"
                    style={{
                      flex: 1,
                      height: 44,
                      display: 'inline-flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      borderRadius: 6,
                      borderLeftWidth: 1,
                      padding: '0 16px',
                    }}
                  >
                    <Space align="center" size={8}>
                      <FileExcelOutlined style={{ color: '#107c41', fontSize: 16 }} />
                      <span style={{ fontWeight: 500 }}>Excel (LibreOffice)</span>
                      <Tag color="success" style={{ margin: 0, fontSize: 11, lineHeight: '18px' }}>Khuyên dùng</Tag>
                    </Space>
                  </Radio.Button>
                  <Radio.Button
                    value="QUEST_PDF"
                    style={{
                      flex: 1,
                      height: 44,
                      display: 'inline-flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      borderRadius: 6,
                      borderLeftWidth: 1,
                      padding: '0 16px',
                    }}
                  >
                    <Space align="center" size={8}>
                      <FilePdfOutlined style={{ color: '#1890ff', fontSize: 16 }} />
                      <span style={{ fontWeight: 500 }}>Mã C# (QuestPDF)</span>
                    </Space>
                  </Radio.Button>
                </Radio.Group>
              </Form.Item>
            </Col>
          </Row>

          {selectedEngine === 'EXCEL_LIBREOFFICE' && (
            <Card
              size="small"
              style={{
                marginBottom: 16,
                background: '#f0fdf4',
                borderColor: '#bbf7d0',
                borderRadius: 8,
              }}
              title={
                <Space>
                  <FileExcelOutlined style={{ color: '#107c41' }} />
                  <Text strong style={{ color: '#14532d', fontSize: 13 }}>
                    Tùy biến mẫu Excel (.xlsx)
                  </Text>
                </Space>
              }
              extra={
                <Button
                  size="small"
                  icon={<DownloadOutlined />}
                  onClick={() => handleDownloadDefaultExcel(form.getFieldValue('paperSize') || 'A5_LANDSCAPE')}
                  style={{ borderColor: '#107c41', color: '#107c41' }}
                >
                  Tải file mẫu (.xlsx)
                </Button>
              }
            >
              <Paragraph style={{ fontSize: 12, marginBottom: 8, color: '#166534' }}>
                Tải file Excel mẫu về máy để tùy biến font chữ, kích cỡ ô, viền khung, màu sắc và logo; sau đó tải lên lại. Hệ thống sẽ tự động điền dữ liệu và kết xuất sang PDF theo đúng định dạng Excel.
              </Paragraph>

              <Space direction="vertical" style={{ width: '100%' }}>
                <Upload
                  beforeUpload={handleExcelUpload}
                  showUploadList={false}
                  accept=".xlsx,.xls"
                >
                  <Button icon={<UploadOutlined />} style={{ borderColor: '#107c41', color: '#107c41' }}>
                    {excelTemplateFileName ? 'Đổi file Excel (.xlsx)' : 'Tải lên file Excel (.xlsx)'}
                  </Button>
                </Upload>

                {excelTemplateFileName && (
                  <div style={{ background: '#ffffff', padding: '6px 12px', borderRadius: 4, border: '1px solid #dcfce7', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                    <Space>
                      <CheckCircleOutlined style={{ color: '#16a34a' }} />
                      <Text strong style={{ fontSize: 13 }}>{excelTemplateFileName}</Text>
                      <Tag color="green">Đã nạp</Tag>
                    </Space>
                    <Button
                      type="link"
                      danger
                      size="small"
                      onClick={() => {
                        setExcelTemplateBase64('');
                        setExcelTemplateFileName('');
                        message.info('Đã xóa file Excel tùy chỉnh. Hệ thống sẽ sử dụng mẫu mặc định.');
                      }}
                    >
                      Dùng mẫu mặc định
                    </Button>
                  </div>
                )}

                <div style={{ marginTop: 6, fontSize: 11, color: '#4b5563' }}>
                  <Text strong style={{ fontSize: 11, color: '#374151' }}>Các tham số trong file Excel: </Text>
                  <Text code>{'{{CustomerName}}'}</Text>, <Text code>{'{{PointCode}}'}</Text>, <Text code>{'{{MeterSerial}}'}</Text>, <Text code>{'{{Period}}'}</Text>, <Text code>{'{{Multiplier}}'}</Text>, <Text code>{'{{PreviousBt}}'}</Text>, <Text code>{'{{Bt}}'}</Text>, <Text code>{'{{ConsumtionBt}}'}</Text>, <Text code>{'{{PreviousCd}}'}</Text>, <Text code>{'{{Cd}}'}</Text>, <Text code>{'{{ConsumtionCd}}'}</Text>, <Text code>{'{{PreviousTd}}'}</Text>, <Text code>{'{{Td}}'}</Text>, <Text code>{'{{ConsumtionTd}}'}</Text>, <Text code>{'{{PreviousSg}}'}</Text>, <Text code>{'{{Sg}}'}</Text>, <Text code>{'{{ConsumtionSg}}'}</Text>, <Text code>{'{{ConsumtionVc}}'}</Text>, <Text code>{'{{Pmax}}'}</Text>, <Text code>{'{{PmaxTime}}'}</Text>, <Text code>{'{{CosPhi}}'}</Text>
                </div>
              </Space>
            </Card>
          )}

          <Row gutter={16}>
            <Col span={12}>
              <Form.Item
                name="headerTitle"
                label="Tiêu đề chính"
                rules={[{ required: true, message: 'Vui lòng nhập tiêu đề chính' }]}
              >
                <Input placeholder="Ví dụ: PHIẾU BÁO CHỈ SỐ ĐIỆN NĂNG" />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item name="subHeaderTitle" label="Tiêu đề phụ">
                <Input placeholder="Ví dụ: (MONTHLY POWER METER READING)" />
              </Form.Item>
            </Col>
          </Row>

          <Row gutter={16}>
            <Col span={12}>
              <Form.Item
                name="companyName"
                label="Tên đơn vị phát hành"
                rules={[{ required: true, message: 'Vui lòng nhập tên đơn vị phát hành' }]}
              >
                <Input placeholder="Ví dụ: TÊN ĐƠN VỊ PHÁT HÀNH" />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item name="contactPhone" label="Thông tin liên hệ">
                <Input placeholder="Ví dụ: Điện thoại: (024) 3xxx xxxx - Fax: (024) 3xxx xxxx" />
              </Form.Item>
            </Col>
          </Row>

          <Form.Item name="companyAddress" label="Địa chỉ đơn vị">
            <Input placeholder="Ví dụ: Địa chỉ trụ sở đơn vị phát hành" />
          </Form.Item>

          <Row gutter={16}>
            <Col span={14}>
              <Form.Item
                name="displayColumns"
                label="Cột hiển thị"
                tooltip="Chọn các thông số điện năng cần hiển thị trên bảng kê"
              >
                <Checkbox.Group options={AVAILABLE_COLUMNS} />
              </Form.Item>
            </Col>
            <Col span={10}>
              <Form.Item
                name="requireDualSignatures"
                label="Chữ ký xác nhận"
                valuePropName="checked"
                tooltip="Bật: 2 chữ ký (Bên giao & Bên nhận). Tắt: 1 chữ ký (Bên phát hành)"
              >
                <Switch
                  checkedChildren="2 Chữ ký (Giao & Nhận)"
                  unCheckedChildren="1 Chữ ký"
                />
              </Form.Item>
            </Col>
          </Row>

          <Form.Item name="footerNotes" label="Ghi chú chân trang">
            <TextArea
              rows={3}
              placeholder="Ví dụ: Quý khách vui lòng kiểm tra và phản hồi số liệu trong vòng 03 ngày làm việc kể từ ngày nhận phiếu báo..."
            />
          </Form.Item>

          <Form.Item label="Logo phiếu báo">
            <Space direction="vertical" style={{ width: '100%' }}>
              <Upload
                beforeUpload={handleLogoUpload}
                showUploadList={false}
                accept="image/*"
              >
                <Button icon={<UploadOutlined />}>Tải ảnh logo</Button>
              </Upload>
              {logoBase64 && (
                <div style={{ marginTop: 8 }}>
                  <Text type="secondary" style={{ fontSize: 12, display: 'block', marginBottom: 4 }}>
                    Xem trước ảnh logo:
                  </Text>
                  <img
                    src={`data:image/png;base64,${logoBase64}`}
                    alt="Logo Preview"
                    style={{ maxHeight: 60, maxWidth: 200, objectFit: 'contain', border: '1px solid #d9d9d9', padding: 4, borderRadius: 4 }}
                  />
                  <Button
                    type="link"
                    danger
                    size="small"
                    onClick={() => setLogoBase64('')}
                    style={{ paddingLeft: 8 }}
                  >
                    Xóa logo
                  </Button>
                </div>
              )}
            </Space>
          </Form.Item>
        </Form>
      </Drawer>

      {/* Modal Pre-built PDF Profiles Library */}
      <Modal
        title={
          <Space>
            <AppstoreOutlined style={{ color: '#003366', fontSize: 20 }} />
            <Text strong style={{ fontSize: 17, color: '#003366' }}>
              Thư viện mẫu phiếu báo
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
        width={850}
      >
        <Paragraph type="secondary" style={{ marginTop: 4, marginBottom: 16 }}>
          Các mẫu phiếu báo được thiết lập sẵn theo chuẩn đo đếm điện năng. Bạn có thể chọn mẫu phù hợp để áp dụng nhanh cấu hình vào hệ thống.
        </Paragraph>

        <Row gutter={[16, 16]}>
          {PREBUILT_PDF_PROFILES.map((profile) => (
            <Col span={24} key={profile.id}>
              <Card
                hoverable
                style={{ borderColor: '#e2e8f0', borderRadius: 8 }}
                bodyStyle={{ padding: 18 }}
              >
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: 8 }}>
                  <Space>
                    <FilePdfOutlined style={{ color: profile.badgeColor, fontSize: 20 }} />
                    <Text strong style={{ fontSize: 16, color: '#0f172a' }}>
                      {profile.name}
                    </Text>
                  </Space>
                  <Tag color={profile.badgeColor}>{profile.tag}</Tag>
                </div>

                <Paragraph type="secondary" style={{ fontSize: 13, marginBottom: 12 }}>
                  {profile.description}
                </Paragraph>

                <div style={{ background: '#f8fafc', padding: '10px 14px', borderRadius: 6, marginBottom: 14, fontSize: 12 }}>
                  <Row gutter={16}>
                    <Col span={8}>
                      <Text type="secondary">Khổ giấy: </Text>
                      <Text strong>{profile.paperSize === 'A5_LANDSCAPE' ? 'A5 Ngang' : 'A4 Dọc'}</Text>
                    </Col>
                    <Col span={8}>
                      <Text type="secondary">Chữ ký: </Text>
                      <Text strong>{profile.requireDualSignatures ? '2 Chữ ký (Giao & Nhận)' : '1 Chữ ký'}</Text>
                    </Col>
                    <Col span={8}>
                      <Text type="secondary">Cột hiển thị: </Text>
                      <Text strong>{profile.displayColumns.join(', ')}</Text>
                    </Col>
                  </Row>
                </div>

                <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 8 }}>
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
                      handleApplyProfile(profile);
                    }}
                  >
                    Áp dụng mẫu này
                  </Button>
                </div>
              </Card>
            </Col>
          ))}
        </Row>
      </Modal>

      {/* PDF Live Preview Modal */}
      <Modal
        title={
          <Space>
            <EyeOutlined style={{ color: '#003366' }} />
            <Text strong>Xem trước mẫu phiếu báo PDF</Text>
          </Space>
        }
        open={previewOpen}
        onCancel={() => setPreviewOpen(false)}
        footer={[
          previewPdfUrl && (
            <Button
              key="download"
              icon={<DownloadOutlined />}
              onClick={() => {
                const a = document.createElement('a');
                a.href = previewPdfUrl;
                a.download = 'PhieuBao_Preview.pdf';
                a.click();
              }}
            >
              Tải tệp PDF
            </Button>
          ),
          <Button key="close" type="primary" onClick={() => setPreviewOpen(false)}>
            Đóng
          </Button>,
        ]}
        width={950}
        style={{ top: 20 }}
      >
        {previewLoading ? (
          <div style={{ textAlign: 'center', padding: '60px 0' }}>
            <Spin size="large" />
            <div style={{ marginTop: 16 }}>Đang render tệp PDF thông qua QuestPDF Engine...</div>
          </div>
        ) : previewPdfUrl ? (
          <iframe
            src={previewPdfUrl}
            style={{ width: '100%', height: '650px', border: 'none', borderRadius: 6 }}
            title="PDF Preview"
          />
        ) : (
          <div style={{ textAlign: 'center', padding: 40 }}>Không thể tải bản xem trước PDF</div>
        )}
      </Modal>
    </div>
  );
};

export default StatementPdfTemplatesPage;
