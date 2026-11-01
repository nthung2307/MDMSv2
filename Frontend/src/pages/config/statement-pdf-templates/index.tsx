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
  CheckCircleOutlined,
  DeleteOutlined,
  DownloadOutlined,
  EditOutlined,
  EyeOutlined,
  FilePdfOutlined,
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
} from '@/core/application/interfaces/methods/templates/templateManagement.interface.ts';
import { TemplateManagementService } from '@/infrastructure/repository/templates/services/templateManagement.service.ts';

const { Title, Text, Paragraph } = Typography;
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
        setTemplates(res.data);
      } else {
        setTemplates([]);
      }
    } catch (err: any) {
      message.error(`Lỗi khi tải danh sách mẫu phiếu báo PDF: ${err?.message || err}`);
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
      headerTitle: 'MONTHLY POWER METER READING',
      subHeaderTitle: '(PHIẾU GHI SỐ ĐIỆN HÀNG THÁNG)',
      companyName: 'CÔNG TY TNHH VSIP HẢI PHÒNG',
      companyAddress: 'Văn phòng Điều Hành VSIP Hải Phòng, KCN VSIP Hải Phòng, Xã An Lư, Huyện Thủy Nguyên, TP. Hải Phòng',
      contactPhone: 'Tel: 84-225-3959868   Fax: 84-255-3959886',
      paperSize: 'A5_LANDSCAPE',
      displayColumns: ['BT', 'CD', 'TD', 'SG', 'VC', 'PMAX', 'COSPHI'],
      requireDualSignatures: true,
      footerNotes: 'Quý khách vui lòng kiểm tra và phản hồi số liệu trong vòng 03 ngày làm việc kể từ ngày nhận phiếu báo.',
      isDefault: false,
      isActive: true,
    });
    setDrawerOpen(true);
  };

  const handleOpenEdit = (record: StatementPdfTemplateDto) => {
    setEditingTemplate(record);
    setLogoBase64(record.logoBase64 || '');
    form.resetFields();

    let cols: string[] = ['BT', 'CD', 'TD', 'SG', 'VC', 'PMAX', 'COSPHI'];
    try {
      if (record.displayColumnsJson) {
        cols = JSON.parse(record.displayColumnsJson);
      }
    } catch {
      // fallback
    }

    form.setFieldsValue({
      code: record.code,
      name: record.name,
      headerTitle: record.headerTitle,
      subHeaderTitle: record.subHeaderTitle,
      companyName: record.companyName,
      companyAddress: record.companyAddress,
      contactPhone: record.contactPhone,
      paperSize: record.paperSize || 'A5_LANDSCAPE',
      displayColumns: cols,
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
      message.success(`Đã thiết lập mẫu "${record.name}" làm mẫu phiếu báo mặc định`);
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

  const handleLogoUpload = (file: File) => {
    const isImage = file.type.startsWith('image/');
    if (!isImage) {
      message.error('Vui lòng tải lên tệp hình ảnh (PNG, JPG, SVG)!');
      return false;
    }
    const isLt2M = file.size / 1024 / 1024 < 2;
    if (!isLt2M) {
      message.error('Kích thước logo không được vượt quá 2MB!');
      return false;
    }

    const reader = new FileReader();
    reader.onload = (e) => {
      const b64 = e.target?.result as string;
      setLogoBase64(b64);
      message.success('Đã tải lên và mã hóa logo thành công');
    };
    reader.readAsDataURL(file);
    return false; // Chặn upload tự động
  };

  const handleSubmit = async () => {
    try {
      const values = await form.validateFields();
      setSubmitting(true);

      const columnsJson = JSON.stringify(values.displayColumns || []);

      if (editingTemplate) {
        const updateData: UpdateStatementPdfTemplateRequest = {
          name: values.name,
          headerTitle: values.headerTitle,
          subHeaderTitle: values.subHeaderTitle,
          companyName: values.companyName,
          companyAddress: values.companyAddress,
          contactPhone: values.contactPhone,
          logoBase64: logoBase64 || undefined,
          paperSize: values.paperSize,
          displayColumnsJson: columnsJson,
          footerNotes: values.footerNotes,
          requireDualSignatures: values.requireDualSignatures,
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
          logoBase64: logoBase64 || undefined,
          paperSize: values.paperSize,
          displayColumnsJson: columnsJson,
          footerNotes: values.footerNotes,
          requireDualSignatures: values.requireDualSignatures,
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
      headerTitle: values.headerTitle || 'MONTHLY POWER METER READING',
      subHeaderTitle: values.subHeaderTitle || '(PHIẾU GHI SỐ ĐIỆN HÀNG THÁNG)',
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
        <Space orientation="vertical" size={2}>
          <Space>
            <Text strong>{name}</Text>
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
      width: 160,
      render: (size) =>
        size === 'A4_PORTRAIT' ? (
          <Tag color="cyan">A4 Đứng (Portrait)</Tag>
        ) : (
          <Tag color="blue">A5 Ngang (Landscape)</Tag>
        ),
    },
    {
      title: 'Chữ ký xác nhận',
      dataIndex: 'requireDualSignatures',
      key: 'requireDualSignatures',
      width: 170,
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
      render: (active) =>
        active ? <Tag color="success">Kích hoạt</Tag> : <Tag color="default">Vô hiệu</Tag>,
    },
    {
      title: 'Thao tác',
      key: 'action',
      width: 220,
      align: 'center',
      render: (_, record) => (
        <Space size="middle">
          <Tooltip title="Xem trước file PDF">
            <Button
              type="text"
              icon={<FilePdfOutlined style={{ color: '#b22222', fontSize: 16 }} />}
              onClick={() => handlePreviewFromTable(record)}
            />
          </Tooltip>
          <Tooltip title="Chỉnh sửa cấu hình mẫu">
            <Button
              type="text"
              icon={<EditOutlined style={{ color: '#fa8c16' }} />}
              onClick={() => handleOpenEdit(record)}
            />
          </Tooltip>
          {!record.isDefault && (
            <Tooltip title="Đặt làm mẫu mặc định">
              <Button
                type="text"
                icon={<StarOutlined style={{ color: '#faad14' }} />}
                onClick={() => handleSetDefault(record)}
              />
            </Tooltip>
          )}
          {!record.isDefault && (
            <Popconfirm
              title="Bạn có chắc chắn muốn xóa mẫu phiếu báo này?"
              okText="Xóa"
              cancelText="Hủy"
              okButtonProps={{ danger: true }}
              onConfirm={() => handleDelete(record)}
            >
              <Tooltip title="Xóa mẫu">
                <Button type="text" danger icon={<DeleteOutlined />} />
              </Tooltip>
            </Popconfirm>
          )}
        </Space>
      ),
    },
  ];

  return (
    <div style={{ padding: '24px', background: '#f0f2f5', minHeight: '100vh' }}>
      <Card
        style={{ borderRadius: 8, boxShadow: '0 2px 8px rgba(0,0,0,0.06)' }}
        title={
          <Space>
            <FilePdfOutlined style={{ color: '#b22222', fontSize: 20 }} />
            <Title level={4} style={{ margin: 0 }}>
              Quản lý Đa Mẫu "Phiếu Báo" PDF (QuestPDF Modular)
            </Title>
          </Space>
        }
        extra={
          <Button
            type="primary"
            icon={<PlusOutlined />}
            onClick={handleOpenCreate}
            style={{ backgroundColor: '#b22222', borderColor: '#b22222' }}
          >
            Tạo Mẫu Phiếu Báo
          </Button>
        }
      >
        <Paragraph type="secondary" style={{ marginBottom: 16 }}>
          Cho phép tùy biến linh hoạt giao diện Phiếu ghi số điện đính kèm: Khổ giấy (A5 Landscape truyền thống hoặc A4 Portrait chuẩn kế toán), Logo doanh nghiệp, tiêu đề, các cột dữ liệu hiển thị ($P_{max}$, $Cos\varphi$, $VC$) và số lượng bên ký xác nhận.
        </Paragraph>

        <Table
          columns={columns}
          dataSource={templates}
          rowKey="id"
          loading={loading}
          pagination={{ pageSize: 10, showSizeChanger: true }}
        />
      </Card>

      {/* Drawer Create / Edit */}
      <Drawer
        title={editingTemplate ? `Chỉnh sửa Mẫu Phiếu Báo: ${editingTemplate.name}` : 'Tạo Mới Mẫu Phiếu Báo PDF'}
        width={850}
        open={drawerOpen}
        onClose={() => setDrawerOpen(false)}
        extra={
          <Space>
            <Button icon={<EyeOutlined />} onClick={handlePreviewFromForm}>
              Xem Trước PDF
            </Button>
            <Button onClick={() => setDrawerOpen(false)}>Hủy</Button>
            <Button type="primary" onClick={handleSubmit} loading={submitting} style={{ backgroundColor: '#b22222', borderColor: '#b22222' }}>
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
                <Input placeholder="Ví dụ: Phiếu Báo Chuẩn A5 VSIP Hải Phòng" />
              </Form.Item>
            </Col>
            <Col span={10}>
              <Form.Item
                name="code"
                label="Mã mẫu (Code)"
                rules={[{ required: true, message: 'Vui lòng nhập mã mẫu' }]}
              >
                <Input disabled={!!editingTemplate} placeholder="Ví dụ: DEFAULT_A5_VSIP" />
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
                <Radio.Group buttonStyle="solid">
                  <Radio.Button value="A5_LANDSCAPE">A5 Ngang (595x420) - VSIP</Radio.Button>
                  <Radio.Button value="A4_PORTRAIT">A4 Đứng (595x842) - Chuẩn</Radio.Button>
                </Radio.Group>
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item
                name="requireDualSignatures"
                label="Chế độ chữ ký chân trang (Signatures)"
              >
                <Radio.Group>
                  <Radio value={true}>Cả Bên A và Bên B ký</Radio>
                  <Radio value={false}>Chỉ Bên A ký (Đơn phương)</Radio>
                </Radio.Group>
              </Form.Item>
            </Col>
          </Row>

          <Card size="small" title="Thông tin Đơn vị phát hành (Bên A)" style={{ marginBottom: 16, background: '#fafafa' }}>
            <Row gutter={16}>
              <Col span={12}>
                <Form.Item name="headerTitle" label="Tiêu đề chính (Tiếng Anh/Việt)">
                  <Input placeholder="MONTHLY POWER METER READING" />
                </Form.Item>
              </Col>
              <Col span={12}>
                <Form.Item name="subHeaderTitle" label="Tiêu đề phụ">
                  <Input placeholder="(PHIẾU GHI SỐ ĐIỆN HÀNG THÁNG)" />
                </Form.Item>
              </Col>
            </Row>

            <Form.Item name="companyName" label="Tên doanh nghiệp / Đơn vị bán điện">
              <Input placeholder="CÔNG TY TNHH VSIP HẢI PHÒNG" />
            </Form.Item>

            <Row gutter={16}>
              <Col span={16}>
                <Form.Item name="companyAddress" label="Địa chỉ đơn vị">
                  <Input placeholder="KCN VSIP Hải Phòng, Huyện Thủy Nguyên, TP. Hải Phòng" />
                </Form.Item>
              </Col>
              <Col span={8}>
                <Form.Item name="contactPhone" label="Hotline / Tel / Fax">
                  <Input placeholder="Tel: 84-225-3959868" />
                </Form.Item>
              </Col>
            </Row>

            {/* Logo Upload */}
            <Row gutter={16} align="middle">
              <Col span={12}>
                <Text strong style={{ display: 'block', marginBottom: 8 }}>Logo Doanh Nghiệp (Góc trên trái PDF):</Text>
                <Upload
                  accept="image/*"
                  showUploadList={false}
                  beforeUpload={handleLogoUpload}
                >
                  <Button icon={<UploadOutlined />}>Tải Logo Mới (PNG/JPG)</Button>
                </Upload>
              </Col>
              <Col span={12}>
                {logoBase64 ? (
                  <Space>
                    <Image
                      src={logoBase64}
                      width={120}
                      height={50}
                      style={{ objectFit: 'contain', background: '#fff', padding: 4, border: '1px solid #d9d9d9', borderRadius: 4 }}
                    />
                    <Button type="link" danger onClick={() => setLogoBase64('')}>
                      Xóa logo
                    </Button>
                  </Space>
                ) : (
                  <Text type="secondary" italic>Đang sử dụng logo mặc định của VSIP Hải Phòng</Text>
                )}
              </Col>
            </Row>
          </Card>

          <Form.Item
            name="displayColumns"
            label="Các cột dữ liệu hiển thị trong bảng sản lượng"
            tooltip="Chọn các cột được phép hiển thị trên phiếu báo PDF"
          >
            <Checkbox.Group options={AVAILABLE_COLUMNS} />
          </Form.Item>

          <Form.Item name="footerNotes" label="Ghi chú chân trang (Footer Notes)">
            <TextArea rows={2} placeholder="Nhập hướng dẫn thanh toán hoặc thời hạn phản hồi số liệu..." />
          </Form.Item>

          <Row gutter={16}>
            <Col span={12}>
              <Form.Item name="isDefault" label="Đặt làm mẫu mặc định" valuePropName="checked">
                <Switch />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item name="isActive" label="Kích hoạt" valuePropName="checked">
                <Switch defaultChecked />
              </Form.Item>
            </Col>
          </Row>
        </Form>
      </Drawer>

      {/* PDF Live Preview Modal */}
      <Modal
        title={
          <Space>
            <FilePdfOutlined style={{ color: '#b22222' }} />
            <Text strong>Xem trước Phiếu Báo PDF (QuestPDF Live Render)</Text>
          </Space>
        }
        open={previewOpen}
        onCancel={() => {
          setPreviewOpen(false);
          if (previewPdfUrl) {
            URL.revokeObjectURL(previewPdfUrl);
            setPreviewPdfUrl('');
          }
        }}
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
