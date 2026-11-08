import React, { useEffect, useMemo, useState } from 'react';
import {
  Badge,
  Button,
  Card,
  Checkbox,
  Col,
  Divider,
  Form,
  Input,
  Modal,
  Row,
  Space,
  Spin,
  Tabs,
  Tag,
  Tooltip,
  Typography,
  Upload,
  message,
} from 'antd';
import {
  BankOutlined,
  CheckCircleOutlined,
  ControlOutlined,
  DeleteOutlined,
  DownloadOutlined,
  EyeOutlined,
  FilePdfOutlined,
  FileTextOutlined,
  PictureOutlined,
  SaveOutlined,
  UploadOutlined,
} from '@ant-design/icons';

import Constants from '@/core/application/common/constants';
import { Endpoint } from '@/core/application/common/endPoint';
import {
  StatementPdfTemplateDto,
  CreateStatementPdfTemplateRequest,
  UpdateStatementPdfTemplateRequest,
} from '@/core/application/interfaces/methods/templates/templateManagement.interface';
import { ButtonCommonCustomizable } from '@/infrastructure/common/components/button/button-common-customizable';
import { BreadcrumbUI } from '@/infrastructure/common/components/controls/BreadcrumbUI';
import { FullPageLoading } from '@/infrastructure/common/components/controls/Loading';
import PageTitle from '@/infrastructure/common/components/title/page-title';
import { ConfigManagementService } from '@/infrastructure/repository/config/services/configManagement.service';
import { TemplateManagementService } from '@/infrastructure/repository/templates/services/templateManagement.service';
import TemplateNotice, {
  DEFAULT_LAYOUT_CONFIG,
  StatementLayoutConfig,
  StatementLogosConfig,
  formatBase64Src,
  parseStatementLayoutConfig,
  parseStatementLogosConfig,
  setCachedStatementTemplate,
} from '@/pages/notice-management/components/form-pdf';

const { Text, Paragraph } = Typography;
const { TextArea } = Input;

const VSIP_LOGO_BASE64 =
  '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';

const METRIC_TOGGLE_ITEMS = [
  {
    code: 'BT',
    title: 'Bình thường (BT)',
  },
  {
    code: 'CD',
    title: 'Cao điểm (CD)',
  },
  {
    code: 'TD',
    title: 'Thấp điểm (TD)',
  },
  {
    code: 'SG',
    title: 'Tổng điện năng hữu công (SG)',
  },
  {
    code: 'VC',
    title: 'Tổng điện năng phản kháng (VC)',
  },
  {
    code: 'COSPHI',
    title: 'Hệ số Cos phi',
  },
  {
    code: 'REMARKS',
    title: 'Ghi chú (Remarks)',
  },
  {
    code: 'MULTIPLIER',
    title: 'Hệ số nhân (Multiplier)',
  },
  {
    code: 'PMAX',
    title: 'Công suất Pmax & Thời điểm ghi nhận',
  },
];

const MOCK_PREVIEW_STATEMENT = {
  serialNumber: '00000162',
  day: 31,
  month: 10,
  year: 2026,
  previousDay: 1,
  previousMonth: 10,
  previousYear: 2026,
  billingTimestamp: '2026-10-31T00:00:00+07:00',
  previousBt: 0,
  bt: 67,
  consumtionBt: 67,
  previousCd: 0,
  cd: 0,
  consumtionCd: 0,
  previousTd: 0,
  td: 0,
  consumtionTd: 0,
  previousSg: 0,
  sg: 67,
  consumtionSg: 67,
  previousVc: 0,
  vc: 28,
  consumtionVc: 28,
  pmax: 66,
  pmaxTime: '2026-11-06T03:16:00+07:00',
  cosPhi: 0.92,
  multiplier: 1,
  measurementPoint: {
    code: 'KYOCERA 1',
    multiplierFactor: 1,
    customer: {
      fullName: 'CÔNG TY TNHH CÔNG NGHỆ MÁY VĂN PHÒNG KYOCERA VIỆT NAM',
      customerCode: 'KYOCERA',
    },
  },
};

export const StatementPdfTemplatesPage: React.FC = () => {
  const service = new TemplateManagementService();
  const [form] = Form.useForm();
  const [loading, setLoading] = useState<boolean>(false);
  const [saving, setSaving] = useState<boolean>(false);
  const [activeRecord, setActiveRecord] = useState<StatementPdfTemplateDto | null>(null);

  // Live state for immediate WYSIWYG preview
  const [companyName, setCompanyName] = useState<string>('VSIP HAI PHONG CO.,LTD');
  const [companyAddress, setCompanyAddress] = useState<string>(
    'VSIP Hai Phong Administration Office, VSIP Hai Phong Township, Industrial & Service Park, Hoa Bình Ward, Hai Phong City, Vietnam'
  );
  const [contactPhone, setContactPhone] = useState<string>(
    'Tel: 84-225-3959868   Fax: 84-225-3959886'
  );
  const [headerTitle, setHeaderTitle] = useState<string>('MONTHLY POWER METER READING');
  const [subHeaderTitle, setSubHeaderTitle] = useState<string>(
    '(PHIẾU CHỈ SỐ ĐIỆN HÀNG THÁNG)'
  );
  const [paperSize, setPaperSize] = useState<string>('A5_LANDSCAPE');
  const [requireDualSignatures, setRequireDualSignatures] = useState<boolean>(true);
  const [footerNotes, setFooterNotes] = useState<string>(
    'Quý khách vui lòng kiểm tra và phản hồi số liệu trong vòng 03 ngày làm việc kể từ ngày nhận phiếu báo.'
  );

  const [layoutConfig, setLayoutConfig] = useState<StatementLayoutConfig>({
    ...DEFAULT_LAYOUT_CONFIG,
  });
  const [logosConfig, setLogosConfig] = useState<StatementLogosConfig>({
    headerLeftLogo: VSIP_LOGO_BASE64,
  });

  // PDF Preview Modal (QuestPDF Backend Engine)
  const [previewOpen, setPreviewOpen] = useState<boolean>(false);
  const [previewPdfUrl, setPreviewPdfUrl] = useState<string>('');
  const [previewLoading, setPreviewLoading] = useState<boolean>(false);

  const applyTemplateToState = (tpl: StatementPdfTemplateDto) => {
    setActiveRecord(tpl);
    setCachedStatementTemplate(tpl);
    const parsedLayout = parseStatementLayoutConfig(tpl.displayColumnsJson);
    const parsedLogos = parseStatementLogosConfig(tpl.excelTemplateBase64, tpl.logoBase64);

    setCompanyName(tpl.companyName || 'VSIP HAI PHONG CO.,LTD');
    setCompanyAddress(tpl.companyAddress || '');
    setContactPhone(tpl.contactPhone || 'Tel: 84-225-3959868   Fax: 84-225-3959886');
    setHeaderTitle(tpl.headerTitle || 'MONTHLY POWER METER READING');
    setSubHeaderTitle(tpl.subHeaderTitle || '(PHIẾU CHỈ SỐ ĐIỆN HÀNG THÁNG)');
    setPaperSize(tpl.paperSize || 'A5_LANDSCAPE');
    setRequireDualSignatures(true);
    setFooterNotes(tpl.footerNotes || '');
    setLayoutConfig(parsedLayout);
    setLogosConfig(parsedLogos);

    form.setFieldsValue({
      companyName: tpl.companyName || 'VSIP HAI PHONG CO.,LTD',
      companyAddress: tpl.companyAddress || '',
      contactPhone: tpl.contactPhone || 'Tel: 84-225-3959868   Fax: 84-225-3959886',
      headerTitle: tpl.headerTitle || 'MONTHLY POWER METER READING',
      subHeaderTitle: tpl.subHeaderTitle || '(PHIẾU CHỈ SỐ ĐIỆN HÀNG THÁNG)',
      paperSize: tpl.paperSize || 'A5_LANDSCAPE',
      requireDualSignatures: true,
      footerNotes: tpl.footerNotes || '',
    });
  };

  const loadActiveTemplate = async () => {
    setLoading(true);
    try {
      const res: any = await service.getStatementPdfTemplates();
      const rawData = res?.data?.data !== undefined ? res.data.data : res?.data;
      const list: StatementPdfTemplateDto[] = Array.isArray(rawData) ? rawData : [];
      if (list.length > 0) {
        const chosen =
          list.find((x) => x.isDefault && x.isActive) ||
          list.find((x) => x.isActive) ||
          list.find((x) => x.isDefault) ||
          list[0];
        if (chosen) {
          applyTemplateToState(chosen);
        }
      }

      // Đồng bộ thông tin đơn vị tập trung từ SystemConfig (Cài đặt hệ thống -> Thông tin liên hệ)
      try {
        const cfgRes: any = await new ConfigManagementService().getSystemListAsync(
          Endpoint.Config.GetSystemList,
          {},
          null as any
        );
        const cfgData = cfgRes?.data?.data || cfgRes?.data || [];
        if (Array.isArray(cfgData) && cfgData.length > 0) {
          const findVal = (key: string, defVal: string = '') =>
            cfgData.find((x: any) => x.key === key)?.value ?? defVal;
          const sysCompName = findVal(Constants.LockAuth.COMPANY_NAME);
          const sysCompAddress = findVal(Constants.LockAuth.COMPANY_ADDRESS);
          const sysCompPhone = findVal(Constants.LockAuth.COMPANY_PHONE);
          const sysCompFax = findVal(Constants.LockAuth.COMPANY_FAX);
          const sysTaxCode = findVal(Constants.LockAuth.COMPANY_TAX_CODE);

          if (sysCompName) {
            setCompanyName(sysCompName);
            form.setFieldValue('companyName', sysCompName);
          }
          if (sysCompAddress) {
            setCompanyAddress(sysCompAddress);
            form.setFieldValue('companyAddress', sysCompAddress);
          }
          let formattedPhone = '';
          if (sysCompPhone && sysCompFax) {
            const p = sysCompPhone.replace(/^(tel|điện thoại|phone):\s*/i, '').trim();
            const f = sysCompFax.replace(/^(fax):\s*/i, '').trim();
            formattedPhone = `Tel: ${p}   Fax: ${f}`;
          } else if (sysCompPhone) {
            const p = sysCompPhone.replace(/^(tel|điện thoại|phone):\s*/i, '').trim();
            formattedPhone = `Tel: ${p}`;
          } else if (sysCompFax) {
            const f = sysCompFax.replace(/^(fax):\s*/i, '').trim();
            formattedPhone = `Fax: ${f}`;
          }
          if (formattedPhone) {
            setContactPhone(formattedPhone);
            form.setFieldValue('contactPhone', formattedPhone);
          }
          if (sysTaxCode) {
            setLayoutConfig((prev) => ({ ...prev, taxCode: sysTaxCode }));
          }
        }
      } catch {
        // Fallback
      }
    } catch (err: any) {
      message.error(`Không thể tải cấu hình phiếu báo: ${err?.message || err}`);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    void loadActiveTemplate();
  }, []);

  const liveTemplateDto = useMemo<Partial<StatementPdfTemplateDto>>(() => {
    return {
      id: activeRecord?.id || 0,
      code: activeRecord?.code || 'STATEMENT_PDF_A5_STANDARD',
      name: activeRecord?.name || 'Mẫu Phiếu Báo Chỉ Số Điện Chuẩn',
      companyName,
      companyAddress,
      contactPhone,
      headerTitle,
      subHeaderTitle,
      paperSize,
      engineType: 'QUEST_PDF',
      requireDualSignatures,
      footerNotes,
      logoBase64: logosConfig.headerLeftLogo || VSIP_LOGO_BASE64,
      excelTemplateBase64: JSON.stringify(logosConfig),
      displayColumnsJson: JSON.stringify(layoutConfig),
      isDefault: true,
      isActive: true,
    };
  }, [
    activeRecord,
    companyName,
    companyAddress,
    contactPhone,
    headerTitle,
    subHeaderTitle,
    paperSize,
    requireDualSignatures,
    footerNotes,
    layoutConfig,
    logosConfig,
  ]);

  const handleToggleColumn = (code: string, checked: boolean) => {
    setLayoutConfig((prev) => {
      const currentSet = new Set((prev.columns || []).map((c) => c.toUpperCase()));
      if (checked) {
        currentSet.add(code.toUpperCase());
      } else {
        currentSet.delete(code.toUpperCase());
      }
      return {
        ...prev,
        columns: Array.from(currentSet),
      };
    });
  };

  const handleUploadLogoSlot = (slot: keyof StatementLogosConfig, file: File) => {
    if (!file.type.startsWith('image/')) {
      message.error('Vui lòng chỉ tải lên tệp hình ảnh (PNG, JPG, SVG)!');
      return false;
    }
    if (file.size / 1024 / 1024 > 2) {
      message.error('Kích thước ảnh logo phải nhỏ hơn 2MB!');
      return false;
    }

    const reader = new FileReader();
    reader.onload = (e) => {
      const base64Str = e.target?.result as string;
      const cleanBase64 = base64Str.includes(',') ? base64Str.split(',')[1] : base64Str;
      setLogosConfig((prev) => ({
        ...prev,
        [slot]: cleanBase64,
      }));
      message.success('Đã cập nhật ảnh Logo trên bản xem trước!');
    };
    reader.readAsDataURL(file);
    return false;
  };

  const handleSaveConfiguration = async () => {
    if (!companyName.trim()) {
      message.warning('Vui lòng nhập Tên Công ty / Đơn vị phát hành');
      return;
    }
    setSaving(true);
    try {
      const displayColumnsJson = JSON.stringify(layoutConfig);
      const logosJson = JSON.stringify(logosConfig);
      const primaryLogo = logosConfig.headerLeftLogo || VSIP_LOGO_BASE64;

      if (activeRecord && activeRecord.id) {
        const updatePayload: UpdateStatementPdfTemplateRequest = {
          name: activeRecord.name || 'Mẫu Phiếu Báo Chỉ Số Điện Chuẩn',
          headerTitle: headerTitle.trim() || 'MONTHLY POWER METER READING',
          subHeaderTitle: subHeaderTitle.trim(),
          companyName: companyName.trim(),
          companyAddress: companyAddress.trim(),
          contactPhone: contactPhone.trim(),
          logoBase64: primaryLogo,
          paperSize: paperSize || 'A5_LANDSCAPE',
          engineType: 'QUEST_PDF',
          excelTemplateBase64: logosJson,
          excelTemplateFileName: '',
          displayColumnsJson,
          requireDualSignatures,
          footerNotes: footerNotes.trim(),
          isDefault: true,
          isActive: true,
        };
        const res: any = await service.updateStatementPdfTemplate(activeRecord.id, updatePayload);
        const saved = res?.data?.data !== undefined ? res.data.data : res?.data;
        if (saved && saved.id) {
          setCachedStatementTemplate(saved);
          setActiveRecord(saved);
        } else {
          setCachedStatementTemplate(liveTemplateDto as StatementPdfTemplateDto);
        }
      } else {
        const createPayload: CreateStatementPdfTemplateRequest = {
          code: 'STATEMENT_PDF_A5_STANDARD',
          name: 'Mẫu Phiếu Báo Chỉ Số Điện Chuẩn',
          headerTitle: headerTitle.trim() || 'MONTHLY POWER METER READING',
          subHeaderTitle: subHeaderTitle.trim(),
          companyName: companyName.trim(),
          companyAddress: companyAddress.trim(),
          contactPhone: contactPhone.trim(),
          logoBase64: primaryLogo,
          paperSize: paperSize || 'A5_LANDSCAPE',
          engineType: 'QUEST_PDF',
          excelTemplateBase64: logosJson,
          excelTemplateFileName: '',
          displayColumnsJson,
          requireDualSignatures,
          footerNotes: footerNotes.trim(),
          isDefault: true,
          isActive: true,
        };
        const res: any = await service.createStatementPdfTemplate(createPayload);
        const saved = res?.data?.data !== undefined ? res.data.data : res?.data;
        if (saved && saved.id) {
          setCachedStatementTemplate(saved);
          setActiveRecord(saved);
        }
      }

      message.success(
        'Đã lưu và đồng bộ cấu hình Phiếu báo & Thông tin đơn vị trên toàn hệ thống (Web, PDF & Email)!'
      );
    } catch (err: any) {
      message.error(`Lỗi khi lưu cấu hình phiếu báo: ${err?.message || err}`);
    } finally {
      setSaving(false);
    }
  };

  const handlePreviewPdfBackend = async () => {
    setPreviewLoading(true);
    setPreviewOpen(true);
    setPreviewPdfUrl('');
    try {
      const blob = await service.previewStatementPdfTemplate({
        headerTitle: headerTitle.trim() || 'MONTHLY POWER METER READING',
        subHeaderTitle: subHeaderTitle.trim(),
        companyName: companyName.trim(),
        companyAddress: companyAddress.trim(),
        contactPhone: contactPhone.trim(),
        logoBase64: logosConfig.headerLeftLogo || VSIP_LOGO_BASE64,
        paperSize: paperSize || 'A5_LANDSCAPE',
        engineType: 'QUEST_PDF',
        excelTemplateBase64: JSON.stringify(logosConfig),
        excelTemplateFileName: '',
        displayColumnsJson: JSON.stringify(layoutConfig),
        requireDualSignatures,
        footerNotes: footerNotes.trim(),
      });
      const url = URL.createObjectURL(new Blob([blob], { type: 'application/pdf' }));
      setPreviewPdfUrl(url);
    } catch (err: any) {
      message.error(`Không thể kết xuất tệp PDF xem thử: ${err?.message || err}`);
    } finally {
      setPreviewLoading(false);
    }
  };

  const enabledColsSet = new Set((layoutConfig.columns || []).map((c) => c.toUpperCase()));

  return (
    <div>
      <PageTitle title={'Cài đặt hệ thống'} />
      <div className='flex flex-wrap justify-between items-center gap-3'>
        <BreadcrumbUI
          items={[{ name: 'Cài đặt hệ thống' }, { name: 'Mẫu Phiếu chỉ số điện' }]}
        />
        <div className='flex items-center gap-2'>
          <ButtonCommonCustomizable
            title={null}
            classButton='text-button-variant'
            tooltipText='Xem thử file PDF đính kèm Email'
            onClick={handlePreviewPdfBackend}
            icon={<FilePdfOutlined style={{ fontSize: 18, color: '#b22222' }} />}
          />

          <ButtonCommonCustomizable
            title={null}
            classButton='text-button-variant'
            tooltipText='Lưu & Áp dụng toàn hệ thống'
            disabled={saving}
            onClick={handleSaveConfiguration}
            icon={<SaveOutlined style={{ fontSize: 18 }} />}
          />
        </div>
      </div>

      <Row gutter={20} className='mt-4'>
        {/* CỘT TRÁI: BẢNG ĐIỀU KHIỂN CẤU HÌNH TRỰC QUAN (NO-CODE) */}
        <Col xs={24} xl={10}>
          <Card
            className='shadow-custom'
            bodyStyle={{ padding: '12px 18px 18px 18px' }}
          >
            <Tabs
              defaultActiveKey='unit'
              items={[
                {
                  key: 'unit',
                  label: (
                    <Space size={6}>
                      <FileTextOutlined />
                      <span className='font-semibold'>1. Thông tin chung</span>
                    </Space>
                  ),
                  children: (
                    <Form
                      layout='horizontal'
                      labelAlign='left'
                      labelCol={{ flex: '135px' }}
                      wrapperCol={{ flex: 'auto' }}
                      className='pt-2'
                    >
                      <Form.Item label='Tiêu đề chính' style={{ marginBottom: 14 }}>
                        <Input
                          value={headerTitle}
                          onChange={(e) => setHeaderTitle(e.target.value)}
                          placeholder='MONTHLY POWER METER READING'
                        />
                      </Form.Item>

                      <Form.Item label='Tiêu đề phụ' style={{ marginBottom: 14 }}>
                        <Input
                          value={subHeaderTitle}
                          onChange={(e) => setSubHeaderTitle(e.target.value)}
                          placeholder='(PHIẾU CHỈ SỐ ĐIỆN HÀNG THÁNG)'
                        />
                      </Form.Item>

                      <Form.Item label='Ghi chú' style={{ marginBottom: 14 }}>
                        <Input
                          value={layoutConfig.copyLabel}
                          onChange={(e) =>
                            setLayoutConfig((prev) => ({
                              ...prev,
                              copyLabel: e.target.value,
                            }))
                          }
                          placeholder='(M&E Copy – Liên cho Phòng Cơ Điện)'
                        />
                      </Form.Item>

                      <Form.Item label='Mã biểu mẫu' style={{ marginBottom: 14 }}>
                        <Input
                          value={layoutConfig.documentCode}
                          onChange={(e) =>
                            setLayoutConfig((prev) => ({
                              ...prev,
                              documentCode: e.target.value,
                            }))
                          }
                          placeholder='M&E-1/01 PR04'
                        />
                      </Form.Item>
                    </Form>
                  ),
                },
                  {
                    key: 'logos',
                    label: (
                      <Space size={6}>
                        <PictureOutlined />
                        <span className='font-semibold'>2. Header & Footer</span>
                      </Space>
                    ),
                    children: (
                      <div className='pt-1 space-y-3'>
                        {/* Khu vực Header */}
                        <Card
                          size='small'
                          title={
                            <Text strong style={{ fontSize: 13, color: '#003366' }}>
                              Header
                            </Text>
                          }
                          style={{ background: '#f8fafc', borderColor: '#e2e8f0' }}
                        >
                          <div className='space-y-3'>
                            {/* Header Left */}
                            <div className='pb-2.5 border-b border-gray-200'>
                              <div className='flex justify-between items-center mb-1.5'>
                                <span className='font-semibold text-[13px] text-gray-800 select-none'>
                                  Header Left
                                </span>
                                <Checkbox
                                  checked={layoutConfig.headerLeftMode !== 'NONE'}
                                  onChange={(e) =>
                                    setLayoutConfig((prev) => ({
                                      ...prev,
                                      headerLeftMode: e.target.checked ? 'LOGO' : 'NONE',
                                    }))
                                  }
                                />
                              </div>
                              <div className='flex gap-2 items-center'>
                                <Upload
                                  beforeUpload={(file) =>
                                    handleUploadLogoSlot('headerLeftLogo', file)
                                  }
                                  showUploadList={false}
                                  accept='image/*'
                                >
                                  <Button size='small' icon={<UploadOutlined />}>
                                    Logo
                                  </Button>
                                </Upload>
                                {logosConfig.headerLeftLogo && (
                                  <>
                                    <img
                                      src={formatBase64Src(logosConfig.headerLeftLogo)}
                                      alt='leftLogo'
                                      style={{
                                        height: 24,
                                        maxWidth: 60,
                                        objectFit: 'contain',
                                        background: '#fff',
                                        padding: 2,
                                        border: '1px solid #cbd5e1',
                                        borderRadius: 4,
                                      }}
                                    />
                                    <Button
                                      size='small'
                                      danger
                                      icon={<DeleteOutlined />}
                                      onClick={() =>
                                        setLogosConfig((prev) => ({
                                          ...prev,
                                          headerLeftLogo: '',
                                        }))
                                      }
                                    />
                                  </>
                                )}
                                <Input
                                  size='small'
                                  placeholder='Nội dung chữ Header Left (tùy chọn)...'
                                  value={layoutConfig.headerLeftText || ''}
                                  onChange={(e) =>
                                    setLayoutConfig((prev) => ({
                                      ...prev,
                                      headerLeftText: e.target.value,
                                    }))
                                  }
                                />
                              </div>
                            </div>

                            {/* Header Center */}
                            <div className='pb-2.5 border-b border-gray-200'>
                              <div className='flex justify-between items-center mb-1.5'>
                                <span className='font-semibold text-[13px] text-gray-800 select-none'>
                                  Header Center
                                </span>
                                <Checkbox
                                  checked={layoutConfig.headerCenterMode !== 'NONE'}
                                  onChange={(e) =>
                                    setLayoutConfig((prev) => ({
                                      ...prev,
                                      headerCenterMode: e.target.checked ? 'LOGO' : 'NONE',
                                    }))
                                  }
                                />
                              </div>
                              <div className='flex gap-2 items-center'>
                                <Upload
                                  beforeUpload={(file) =>
                                    handleUploadLogoSlot('headerCenterLogo', file)
                                  }
                                  showUploadList={false}
                                  accept='image/*'
                                >
                                  <Button size='small' icon={<UploadOutlined />}>
                                    Logo
                                  </Button>
                                </Upload>
                                {logosConfig.headerCenterLogo && (
                                  <>
                                    <img
                                      src={formatBase64Src(logosConfig.headerCenterLogo)}
                                      alt='centerLogo'
                                      style={{
                                        height: 24,
                                        maxWidth: 60,
                                        objectFit: 'contain',
                                        background: '#fff',
                                        padding: 2,
                                        border: '1px solid #cbd5e1',
                                        borderRadius: 4,
                                      }}
                                    />
                                    <Button
                                      size='small'
                                      danger
                                      icon={<DeleteOutlined />}
                                      onClick={() =>
                                        setLogosConfig((prev) => ({
                                          ...prev,
                                          headerCenterLogo: '',
                                        }))
                                      }
                                    />
                                  </>
                                )}
                                <Input
                                  size='small'
                                  placeholder='Nội dung chữ Header Center (tùy chọn)...'
                                  value={layoutConfig.headerCenterText || ''}
                                  onChange={(e) =>
                                    setLayoutConfig((prev) => ({
                                      ...prev,
                                      headerCenterText: e.target.value,
                                    }))
                                  }
                                />
                              </div>
                            </div>

                            {/* Header Right */}
                            <div>
                              <div className='flex justify-between items-center mb-1.5'>
                                <span className='font-semibold text-[13px] text-gray-800 select-none'>
                                  Header Right
                                </span>
                                <Checkbox
                                  checked={layoutConfig.headerRightMode !== 'NONE'}
                                  onChange={(e) =>
                                    setLayoutConfig((prev) => ({
                                      ...prev,
                                      headerRightMode: e.target.checked
                                        ? (logosConfig.headerRightLogo ? 'LOGO' : 'DECOR')
                                        : 'NONE',
                                    }))
                                  }
                                />
                              </div>
                              <div className='flex gap-2 items-center'>
                                <Upload
                                  beforeUpload={(file) => {
                                    handleUploadLogoSlot('headerRightLogo', file);
                                    setLayoutConfig((prev) => ({
                                      ...prev,
                                      headerRightMode: 'LOGO',
                                    }));
                                    return false;
                                  }}
                                  showUploadList={false}
                                  accept='image/*'
                                >
                                  <Button size='small' icon={<UploadOutlined />}>
                                    Logo
                                  </Button>
                                </Upload>
                                {logosConfig.headerRightLogo && (
                                  <>
                                    <img
                                      src={formatBase64Src(logosConfig.headerRightLogo)}
                                      alt='rightLogo'
                                      style={{
                                        height: 24,
                                        maxWidth: 60,
                                        objectFit: 'contain',
                                        background: '#fff',
                                        padding: 2,
                                        border: '1px solid #cbd5e1',
                                        borderRadius: 4,
                                      }}
                                    />
                                    <Button
                                      size='small'
                                      danger
                                      icon={<DeleteOutlined />}
                                      onClick={() => {
                                        setLogosConfig((prev) => ({
                                          ...prev,
                                          headerRightLogo: '',
                                        }));
                                        setLayoutConfig((prev) => ({
                                          ...prev,
                                          headerRightMode: 'DECOR',
                                        }));
                                      }}
                                    />
                                  </>
                                )}
                                <Input
                                  size='small'
                                  placeholder='Nội dung chữ Header Right (tùy chọn)...'
                                  value={layoutConfig.headerRightText || ''}
                                  onChange={(e) =>
                                    setLayoutConfig((prev) => ({
                                      ...prev,
                                      headerRightText: e.target.value,
                                    }))
                                  }
                                />
                              </div>
                            </div>
                          </div>
                        </Card>

                        {/* Khu vực Footer */}
                        <Card
                          size='small'
                          title={
                            <Text strong style={{ fontSize: 13, color: '#003366' }}>
                              Footer
                            </Text>
                          }
                          style={{ background: '#f8fafc', borderColor: '#e2e8f0' }}
                        >
                          <div className='space-y-3'>
                            {/* Footer Left */}
                            <div className='pb-2.5 border-b border-gray-200'>
                              <div className='flex justify-between items-center mb-1.5'>
                                <span className='font-semibold text-[13px] text-gray-800 select-none'>
                                  Footer Left
                                </span>
                                <Checkbox
                                  checked={layoutConfig.footerLeftMode !== 'NONE'}
                                  onChange={(e) =>
                                    setLayoutConfig((prev) => ({
                                      ...prev,
                                      footerLeftMode: e.target.checked ? 'LOGO' : 'NONE',
                                    }))
                                  }
                                />
                              </div>
                              <div className='flex gap-2 items-center'>
                                <Upload
                                  beforeUpload={(file) =>
                                    handleUploadLogoSlot('footerLeftLogo', file)
                                  }
                                  showUploadList={false}
                                  accept='image/*'
                                >
                                  <Button size='small' icon={<UploadOutlined />}>
                                    Logo
                                  </Button>
                                </Upload>
                                {logosConfig.footerLeftLogo && (
                                  <>
                                    <img
                                      src={formatBase64Src(logosConfig.footerLeftLogo)}
                                      alt='footerLeft'
                                      style={{
                                        height: 24,
                                        maxWidth: 60,
                                        objectFit: 'contain',
                                        background: '#fff',
                                        padding: 2,
                                        border: '1px solid #cbd5e1',
                                        borderRadius: 4,
                                      }}
                                    />
                                    <Button
                                      size='small'
                                      danger
                                      icon={<DeleteOutlined />}
                                      onClick={() =>
                                        setLogosConfig((prev) => ({
                                          ...prev,
                                          footerLeftLogo: '',
                                        }))
                                      }
                                    />
                                  </>
                                )}
                                <Input
                                  size='small'
                                  placeholder='Nội dung chữ Footer Left (tùy chọn)...'
                                  value={layoutConfig.footerLeftText || ''}
                                  onChange={(e) =>
                                    setLayoutConfig((prev) => ({
                                      ...prev,
                                      footerLeftText: e.target.value,
                                    }))
                                  }
                                />
                              </div>
                            </div>

                            {/* Footer Center */}
                            <div className='pb-2.5 border-b border-gray-200'>
                              <div className='flex justify-between items-center mb-1.5'>
                                <span className='font-semibold text-[13px] text-gray-800 select-none'>
                                  Footer Center
                                </span>
                                <Checkbox
                                  checked={layoutConfig.footerCenterMode !== 'NONE'}
                                  onChange={(e) =>
                                    setLayoutConfig((prev) => ({
                                      ...prev,
                                      footerCenterMode: e.target.checked ? 'LOGO' : 'NONE',
                                    }))
                                  }
                                />
                              </div>
                              <div className='flex gap-2 items-center'>
                                <Upload
                                  beforeUpload={(file) =>
                                    handleUploadLogoSlot('footerCenterLogo', file)
                                  }
                                  showUploadList={false}
                                  accept='image/*'
                                >
                                  <Button size='small' icon={<UploadOutlined />}>
                                    Logo
                                  </Button>
                                </Upload>
                                {logosConfig.footerCenterLogo && (
                                  <>
                                    <img
                                      src={formatBase64Src(logosConfig.footerCenterLogo)}
                                      alt='footerCenter'
                                      style={{
                                        height: 24,
                                        maxWidth: 60,
                                        objectFit: 'contain',
                                        background: '#fff',
                                        padding: 2,
                                        border: '1px solid #cbd5e1',
                                        borderRadius: 4,
                                      }}
                                    />
                                    <Button
                                      size='small'
                                      danger
                                      icon={<DeleteOutlined />}
                                      onClick={() =>
                                        setLogosConfig((prev) => ({
                                          ...prev,
                                          footerCenterLogo: '',
                                        }))
                                      }
                                    />
                                  </>
                                )}
                                <Input
                                  size='small'
                                  placeholder='Nội dung chữ Footer Center (tùy chọn)...'
                                  value={layoutConfig.footerCenterText || ''}
                                  onChange={(e) =>
                                    setLayoutConfig((prev) => ({
                                      ...prev,
                                      footerCenterText: e.target.value,
                                    }))
                                  }
                                />
                              </div>
                            </div>

                            {/* Footer Right */}
                            <div>
                              <div className='flex justify-between items-center mb-1.5'>
                                <span className='font-semibold text-[13px] text-gray-800 select-none'>
                                  Footer Right
                                </span>
                                <Checkbox
                                  checked={layoutConfig.footerRightMode !== 'NONE'}
                                  onChange={(e) =>
                                    setLayoutConfig((prev) => ({
                                      ...prev,
                                      footerRightMode: e.target.checked ? 'LOGO' : 'NONE',
                                    }))
                                  }
                                />
                              </div>
                              <div className='flex gap-2 items-center'>
                                <Upload
                                  beforeUpload={(file) =>
                                    handleUploadLogoSlot('footerRightLogo', file)
                                  }
                                  showUploadList={false}
                                  accept='image/*'
                                >
                                  <Button size='small' icon={<UploadOutlined />}>
                                    Logo
                                  </Button>
                                </Upload>
                                {logosConfig.footerRightLogo && (
                                  <>
                                    <img
                                      src={formatBase64Src(logosConfig.footerRightLogo)}
                                      alt='footerRight'
                                      style={{
                                        height: 24,
                                        maxWidth: 60,
                                        objectFit: 'contain',
                                        background: '#fff',
                                        padding: 2,
                                        border: '1px solid #cbd5e1',
                                        borderRadius: 4,
                                      }}
                                    />
                                    <Button
                                      size='small'
                                      danger
                                      icon={<DeleteOutlined />}
                                      onClick={() =>
                                        setLogosConfig((prev) => ({
                                          ...prev,
                                          footerRightLogo: '',
                                        }))
                                      }
                                    />
                                  </>
                                )}
                                <Input
                                  size='small'
                                  placeholder='Nội dung chữ Footer Right (tùy chọn)...'
                                  value={layoutConfig.footerRightText || ''}
                                  onChange={(e) =>
                                    setLayoutConfig((prev) => ({
                                      ...prev,
                                      footerRightText: e.target.value,
                                    }))
                                  }
                                />
                              </div>
                            </div>
                          </div>
                        </Card>
                      </div>
                    ),
                  },
                  {
                    key: 'metrics',
                    label: (
                      <Space size={6}>
                        <ControlOutlined />
                        <span className='font-semibold'>3. Chỉ số & Chữ ký</span>
                      </Space>
                    ),
                    children: (
                      <div className='pt-1'>
                        <Paragraph type='secondary' style={{ fontSize: 12, marginBottom: 10 }}>
                          Bật hoặc tắt các chỉ số điện năng hiển thị trên Phiếu chỉ số điện.
                        </Paragraph>

                        <div className='space-y-2 mb-4'>
                          {METRIC_TOGGLE_ITEMS.map((item) => {
                            const isChecked = enabledColsSet.has(item.code);
                            return (
                              <div
                                key={item.code}
                                className='flex justify-between items-center px-3 py-2 rounded-md border cursor-pointer transition-colors hover:border-blue-400'
                                style={{
                                  backgroundColor: isChecked ? '#ffffff' : '#f8fafc',
                                  borderColor: isChecked ? '#cbd5e1' : '#e2e8f0',
                                }}
                                onClick={() => handleToggleColumn(item.code, !isChecked)}
                              >
                                <div>
                                  <Text strong style={{ fontSize: 13, color: '#0f172a' }}>
                                    {item.title}
                                  </Text>
                                </div>
                                <Checkbox
                                  checked={isChecked}
                                  onChange={(e) => {
                                    e.stopPropagation();
                                    handleToggleColumn(item.code, e.target.checked);
                                  }}
                                />
                              </div>
                            );
                          })}
                        </div>

                        <Divider style={{ margin: '12px 0' }} />

                        <Form layout='vertical'>
                          <Row gutter={12}>
                            <Col span={12}>
                              <Form.Item
                                label='Khung ký Bên A (Đơn vị cung cấp điện)'
                                style={{ marginBottom: 8 }}
                              >
                                <TextArea
                                  rows={2}
                                  value={layoutConfig.partyAName}
                                  onChange={(e) =>
                                    setLayoutConfig((prev) => ({
                                      ...prev,
                                      partyAName: e.target.value,
                                    }))
                                  }
                                />
                              </Form.Item>
                            </Col>
                            <Col span={12}>
                              <Form.Item
                                label='Khung ký Bên B (Khách hàng)'
                                style={{ marginBottom: 8 }}
                              >
                                <TextArea
                                  rows={2}
                                  value={layoutConfig.partyBName}
                                  onChange={(e) =>
                                    setLayoutConfig((prev) => ({
                                      ...prev,
                                      partyBName: e.target.value,
                                    }))
                                  }
                                />
                              </Form.Item>
                            </Col>
                          </Row>
                        </Form>
                      </div>
                    ),
                  },
                ]}
              />
          </Card>
        </Col>

        {/* CỘT PHẢI: XEM TRƯỚC PHIẾU BÁO TRỰC TIẾP (LIVE WYSIWYG PREVIEW) */}
        <Col xs={24} xl={14}>
          <Card
            className='shadow-custom sticky top-4'
            title={
              <div className='flex justify-between items-center'>
                <Space>
                  <EyeOutlined style={{ color: '#16a34a', fontSize: 18 }} />
                  <Text strong style={{ fontSize: 15, color: '#0f172a' }}>
                    Xem trước Phiếu chỉ số điện
                  </Text>
                </Space>
                <Tag color='blue'>Khổ A5 Ngang</Tag>
              </div>
            }
            bodyStyle={{
              background: '#e2e8f0',
              padding: '20px 12px',
              display: 'flex',
              justifyContent: 'center',
              overflowX: 'auto',
            }}
          >
            <div
              style={{
                width: 794,
                minHeight: 545,
                background: '#ffffff',
                borderRadius: 4,
                boxShadow: '0 10px 25px rgba(0, 0, 0, 0.12)',
                paddingBottom: 12,
              }}
            >
              <TemplateNotice record={MOCK_PREVIEW_STATEMENT} template={liveTemplateDto} />
            </div>
          </Card>
        </Col>
      </Row>

      <FullPageLoading isLoading={loading} />

      {/* Modal Xem trước File PDF kết xuất từ Backend QuestPDF */}
      <Modal
        title={
          <Space>
            <FilePdfOutlined style={{ color: '#b22222' }} />
            <Text strong>Xem trước Tệp PDF Đính kèm Email (QuestPDF Engine)</Text>
          </Space>
        }
        open={previewOpen}
        onCancel={() => setPreviewOpen(false)}
        footer={[
          previewPdfUrl && (
            <Button
              key='download'
              icon={<DownloadOutlined />}
              onClick={() => {
                const a = document.createElement('a');
                a.href = previewPdfUrl;
                a.download = 'MDMS_PhieuBao_Preview.pdf';
                a.click();
              }}
            >
              Tải tệp PDF
            </Button>
          ),
          <Button key='close' type='primary' onClick={() => setPreviewOpen(false)}>
            Đóng
          </Button>,
        ]}
        width={950}
        style={{ top: 20 }}
      >
        {previewLoading ? (
          <div style={{ textAlign: 'center', padding: '60px 0' }}>
            <Spin size='large' />
            <div style={{ marginTop: 16 }}>
              Đang kết xuất tệp PDF chuẩn thông qua QuestPDF Engine...
            </div>
          </div>
        ) : previewPdfUrl ? (
          <iframe
            src={previewPdfUrl}
            style={{ width: '100%', height: '650px', border: 'none', borderRadius: 6 }}
            title='PDF Preview'
          />
        ) : (
          <div style={{ textAlign: 'center', padding: 40 }}>Không thể tải bản xem trước PDF</div>
        )}
      </Modal>
    </div>
  );
};

export default StatementPdfTemplatesPage;
