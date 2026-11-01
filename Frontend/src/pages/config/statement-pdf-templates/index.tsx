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
  DeleteOutlined,
  DownloadOutlined,
  EditOutlined,
  EyeOutlined,
  PlusOutlined,
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

  // PDF Preview Modal
  const [previewOpen, setPreviewOpen] = useState<boolean>(false);
  const [previewPdfUrl, setPreviewPdfUrl] = useState<string>('');
  const [previewLoading, setPreviewLoading] = useState<boolean>(false);

  const fetchTemplates = async () => {
    setLoading(true);
    try {
      const res: any = await service.getStatementPdfTemplates();
      if (res?.data) {
        setTemplates(Array.isArray(res.data) ? res.data : []);
      } else {
        setTemplates([]);
      }
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

  const handleOpenCreate = () => {
    setEditingTemplate(null);
    setLogoBase64('');
    form.resetFields();
    form.setFieldsValue({
      code: `PDF_PROFILE_${Date.now().toString().slice(-4)}`,
      name: 'Mẫu Phiếu Báo Chuẩn Doanh Nghiệp',
      headerTitle: 'PHIẾU BÁO CHỈ SỐ ĐIỆN NĂNG',
      subHeaderTitle: 'DÀNH CHO KHÁCH HÀNG SỬ DỤNG ĐIỆN',
      companyName: 'CÔNG TY TNHH VSIP HẢI PHÒNG',
      companyAddress: 'Khu Đô thị, Công nghiệp và Dịch vụ VSIP Hải Phòng, Thủy Nguyên, Hải Phòng',
      contactPhone: '(0225) 3959 888',
      paperSize: 'A5_LANDSCAPE',
      displayColumns: ['BT', 'CD', 'TD', 'SG', 'VC', 'PMAX', 'COSPHI'],
      requireDualSignatures: true,
      footerNotes: 'Ghi chú: Phiếu báo chỉ số điện năng này là cơ sở để phát hành hóa đơn tiền điện hàng tháng. Mọi thắc mắc xin vui lòng liên hệ Phòng Cơ Điện trong vòng 24 giờ kể từ khi nhận được phiếu báo.',
      isDefault: false,
      isActive: true,
    });
    setDrawerOpen(true);
  };

  const handleOpenEdit = (record: StatementPdfTemplateDto) => {
    setEditingTemplate(record);
    setLogoBase64(record.logoBase64 || '');
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
      message.success(`Đã xóa mẫu "${record.name}"`);
      fetchTemplates();
    } catch (err: any) {
      message.error(`Không thể xóa mẫu: ${err?.message || err}`);
    }
  };

  const handleSubmit = async () => {
    try {
      const values = await form.validateFields();
      setSubmitting(true);

      const displayColumnsJson = JSON.stringify(values.displayColumns || []);

      if (editingTemplate) {
        const updateData: UpdateStatementPdfTemplateRequest = {
          name: values.name,
          headerTitle: values.headerTitle,
          subHeaderTitle: values.subHeaderTitle,
          companyName: values.companyName,
          companyAddress: values.companyAddress,
          contactPhone: values.contactPhone,
          logoBase64: logoBase64 || null,
          paperSize: values.paperSize,
          displayColumnsJson,
          footerNotes: values.footerNotes,
          requireDualSignatures: values.requireDualSignatures,
          isDefault: values.isDefault,
          isActive: values.isActive,
        };
        await service.updateStatementPdfTemplate(editingTemplate.id, updateData);
        message.success('Cập nhật mẫu phiếu báo thành công');
      } else {
        const createData: CreateStatementPdfTemplateRequest = {
          code: values.code,
          name: values.name,
          headerTitle: values.headerTitle,
          subHeaderTitle: values.subHeaderTitle,
          companyName: values.companyName,
          companyAddress: values.companyAddress,
          contactPhone: values.contactPhone,
          logoBase64: logoBase64 || null,
          paperSize: values.paperSize,
          displayColumnsJson,
          footerNotes: values.footerNotes,
          requireDualSignatures: values.requireDualSignatures,
          isDefault: values.isDefault,
          isActive: values.isActive,
        };
        await service.createStatementPdfTemplate(createData);
        message.success('Tạo mới mẫu phiếu báo thành công');
      }

      setDrawerOpen(false);
      fetchTemplates();
    } catch (err: any) {
      message.error(`Lỗi khi lưu mẫu phiếu báo: ${err?.message || err}`);
    } finally {
      setSubmitting(false);
    }
  };

  const handlePreviewPdf = async (reqData: PreviewStatementPdfTemplateRequest) => {
    setPreviewLoading(true);
    setPreviewOpen(true);
    try {
      const blob = await service.previewStatementPdfTemplate(reqData);
      const url = URL.createObjectURL(blob);
      setPreviewPdfUrl(url);
    } catch (err: any) {
      message.error(`Không thể render xem trước PDF: ${err?.message || err}`);
    } finally {
      setPreviewLoading(false);
    }
  };

  const handlePreviewFromTable = (record: StatementPdfTemplateDto) => {
    handlePreviewPdf({
      headerTitle: record.headerTitle,
      subHeaderTitle: record.subHeaderTitle,
      companyName: record.companyName,
      companyAddress: record.companyAddress,
      contactPhone: record.contactPhone,
      logoBase64: record.logoBase64,
      paperSize: record.paperSize,
      displayColumnsJson: record.displayColumnsJson,
      footerNotes: record.footerNotes,
      requireDualSignatures: record.requireDualSignatures,
    });
  };

  const handlePreviewFromForm = () => {
    const values = form.getFieldsValue();
    handlePreviewPdf({
      headerTitle: values.headerTitle || 'PHIẾU BÁO CHỈ SỐ ĐIỆN NĂNG',
      subHeaderTitle: values.subHeaderTitle || 'DÀNH CHO KHÁCH HÀNG SỬ DỤNG ĐIỆN',
      companyName: values.companyName || 'CÔNG TY TNHH VSIP HẢI PHÒNG',
      companyAddress: values.companyAddress || '',
      contactPhone: values.contactPhone || '',
      logoBase64: logoBase64 || undefined,
      paperSize: values.paperSize || 'A5_LANDSCAPE',
      displayColumnsJson: JSON.stringify(values.displayColumns || []),
      footerNotes: values.footerNotes || '',
      requireDualSignatures: values.requireDualSignatures !== false,
    });
  };

  const columns: ColumnsType<StatementPdfTemplateDto> = [
    {
      title: 'Tên mẫu phiếu báo',
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
      width: 160,
      render: (code) => <Text code>{code}</Text>,
    },
    {
      title: 'Khổ giấy',
      dataIndex: 'paperSize',
      key: 'paperSize',
      width: 170,
      align: 'center',
      render: (size) =>
        size === 'A4_PORTRAIT' ? (
          <Tag color="cyan">A4 Đứng (595x842 pt)</Tag>
        ) : (
          <Tag color="blue">A5 Ngang (595x420 pt)</Tag>
        ),
    },
    {
      title: 'Chữ ký xác nhận',
      dataIndex: 'requireDualSignatures',
      key: 'requireDualSignatures',
      width: 170,
      align: 'center',
      render: (dual) =>
        dual ? (
          <Tag color="purple">Bên A & Bên B cùng ký</Tag>
        ) : (
          <Tag color="default">Chỉ Bên A ký</Tag>
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
      width: 200,
      align: 'center',
      render: (_, record) => (
        <Space size="middle">
          <Tooltip title="Xem trước PDF trong thời gian thực">
            <Button
              type="text"
              icon={<EyeOutlined style={{ color: '#1890ff', fontSize: 16 }} />}
              onClick={() => handlePreviewFromTable(record)}
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
        <div className='flex'>
          <ButtonCommonCustomizable
            title={_t('Thêm mới')}
            tooltipText={'Tạo mẫu phiếu báo mới'}
            classButton='text-button-variant'
            onClick={handleOpenCreate}
            icon={<PlusOutlined />}
          />
        </div>
      </div>

      <div className='mt-10'>
        <Card className={'shadow-custom'}>
          <div>
            <div className={'flex justify-between items-center mb-4'}>
              <h1 className='font-bold text-20-20'>{'Mẫu phiếu báo chỉ số điện'}</h1>
            </div>

            <Paragraph type="secondary" className="mb-4">
              Cho phép tùy biến linh hoạt giao diện Phiếu ghi số điện đính kèm khi gửi khách hàng: Khổ giấy (A5 Landscape truyền thống hoặc A4 Portrait chuẩn kế toán), Logo doanh nghiệp, tiêu đề, các cột dữ liệu hiển thị (BT, CD, TD, SG, VC, Pmax, Cos φ) và số lượng bên ký xác nhận. Mẫu <strong>Mặc định</strong> sẽ tự động được sử dụng khi gửi phiếu báo hàng tháng.
            </Paragraph>

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
        title={editingTemplate ? `Chỉnh sửa Mẫu Phiếu Báo: ${editingTemplate.name}` : 'Tạo Mới Mẫu Phiếu Báo Chỉ Số Điện'}
        width={850}
        open={drawerOpen}
        onClose={() => setDrawerOpen(false)}
        extra={
          <Space>
            <Button icon={<EyeOutlined />} onClick={handlePreviewFromForm}>
              Xem Trước PDF
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
            <Col span={14}>
              <Form.Item
                name="name"
                label="Tên mẫu phiếu báo"
                rules={[{ required: true, message: 'Vui lòng nhập tên mẫu phiếu báo' }]}
              >
                <Input placeholder="Ví dụ: Mẫu Phiếu Báo A5 Chuẩn VSIP Hải Phòng" />
              </Form.Item>
            </Col>
            <Col span={10}>
              <Form.Item
                name="code"
                label="Mã mẫu (Code)"
                rules={[{ required: true, message: 'Vui lòng nhập mã định danh mẫu' }]}
              >
                <Input disabled={!!editingTemplate} placeholder="VD: A5_VSIP_STANDARD" />
              </Form.Item>
            </Col>
          </Row>

          <Row gutter={16}>
            <Col span={12}>
              <Form.Item
                name="paperSize"
                label="Khổ giấy (Paper Size)"
                rules={[{ required: true }]}
              >
                <Radio.Group className="w-full">
                  <Radio.Button value="A5_LANDSCAPE" style={{ width: '50%', textAlign: 'center' }}>
                    A5 Ngang (Chuẩn VSIP)
                  </Radio.Button>
                  <Radio.Button value="A4_PORTRAIT" style={{ width: '50%', textAlign: 'center' }}>
                    A4 Đứng (Chuẩn Kế toán)
                  </Radio.Button>
                </Radio.Group>
              </Form.Item>
            </Col>
            <Col span={6}>
              <Form.Item name="isDefault" label="Đặt làm mặc định" valuePropName="checked">
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
            <Col span={12}>
              <Form.Item
                name="headerTitle"
                label="Tiêu đề chính (Header Title)"
                rules={[{ required: true, message: 'Vui lòng nhập tiêu đề chính' }]}
              >
                <Input placeholder="Ví dụ: PHIẾU BÁO CHỈ SỐ ĐIỆN NĂNG" />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item name="subHeaderTitle" label="Tiêu đề phụ (Sub-header Title)">
                <Input placeholder="Ví dụ: DÀNH CHO KHÁCH HÀNG SỬ DỤNG ĐIỆN" />
              </Form.Item>
            </Col>
          </Row>

          <Row gutter={16}>
            <Col span={12}>
              <Form.Item
                name="companyName"
                label="Tên đơn vị phát hành (Bên A)"
                rules={[{ required: true, message: 'Vui lòng nhập tên công ty/đơn vị' }]}
              >
                <Input placeholder="Ví dụ: CÔNG TY TNHH VSIP HẢI PHÒNG" />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item name="contactPhone" label="Thông tin liên hệ / Hotline">
                <Input placeholder="Ví dụ: (0225) 3959 888 - Hotline: 19001006" />
              </Form.Item>
            </Col>
          </Row>

          <Form.Item name="companyAddress" label="Địa chỉ đơn vị phát hành">
            <Input placeholder="Khu Đô thị, Công nghiệp và Dịch vụ VSIP Hải Phòng, Thủy Nguyên, Hải Phòng" />
          </Form.Item>

          {/* Logo Upload Section */}
          <Form.Item label="Logo doanh nghiệp (Tùy chọn)">
            <Space align="start" size="large">
              <Upload
                accept="image/*"
                showUploadList={false}
                beforeUpload={(file) => {
                  const reader = new FileReader();
                  reader.onload = (e) => {
                    setLogoBase64(e.target?.result as string);
                  };
                  reader.readAsDataURL(file);
                  return false;
                }}
              >
                <Button icon={<UploadOutlined />}>Tải ảnh Logo lên</Button>
              </Upload>
              {logoBase64 ? (
                <Space>
                  <Image src={logoBase64} height={40} alt="Logo Preview" />
                  <Button type="link" danger onClick={() => setLogoBase64('')}>
                    Xóa Logo
                  </Button>
                </Space>
              ) : (
                <Text type="secondary" style={{ fontSize: 12 }}>
                  Chưa cài logo (sẽ hiển thị logo mặc định hệ thống)
                </Text>
              )}
            </Space>
          </Form.Item>

          {/* Display Columns Selection */}
          <Form.Item
            name="displayColumns"
            label="Danh sách các cột thông số hiển thị trên Phiếu báo"
            rules={[{ required: true, message: 'Vui lòng chọn ít nhất một cột hiển thị' }]}
          >
            <Checkbox.Group className="w-full">
              <Row gutter={[16, 12]}>
                {AVAILABLE_COLUMNS.map((col) => (
                  <Col span={8} key={col.value}>
                    <Checkbox value={col.value}>{col.label}</Checkbox>
                  </Col>
                ))}
              </Row>
            </Checkbox.Group>
          </Form.Item>

          <Form.Item
            name="requireDualSignatures"
            label="Chế độ ký xác nhận cuối phiếu báo"
            valuePropName="checked"
          >
            <Switch
              checkedChildren="Hai bên cùng ký (Bên A & Bên B)"
              unCheckedChildren="Chỉ Bên A ký xác nhận"
            />
          </Form.Item>

          <Form.Item name="footerNotes" label="Ghi chú chân trang phiếu báo (Footer Notes)">
            <TextArea
              rows={3}
              placeholder="Nhập nội dung ghi chú ở cuối trang phiếu báo..."
            />
          </Form.Item>
        </Form>
      </Drawer>

      {/* PDF Live Preview Modal */}
      <Modal
        title={
          <Space>
            <EyeOutlined style={{ color: '#003366' }} />
            <Text strong>Xem trước Phiếu Báo PDF (Live QuestPDF Engine)</Text>
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
