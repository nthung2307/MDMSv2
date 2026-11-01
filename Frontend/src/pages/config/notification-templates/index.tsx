import React, { useEffect, useState } from 'react';
import {
  Button,
  Card,
  Col,
  Drawer,
  Form,
  Input,
  Modal,
  Popconfirm,
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
  CheckCircleOutlined,
  CopyOutlined,
  DeleteOutlined,
  EditOutlined,
  EyeOutlined,
  MailOutlined,
  PlusOutlined,
  StarFilled,
  StarOutlined,
} from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';

import {
  NotificationTemplateDto,
  CreateNotificationTemplateRequest,
  UpdateNotificationTemplateRequest,
  PreviewNotificationTemplateResponse,
} from '@/core/application/interfaces/methods/templates/templateManagement.interface.ts';
import { TemplateManagementService } from '@/infrastructure/repository/templates/services/templateManagement.service.ts';

const { Title, Text, Paragraph } = Typography;
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
      { tag: '{{SerialNumber}}', desc: 'Số phiếu báo / Serial' },
      { tag: '{{Address}}', desc: 'Địa chỉ điểm đo' },
    ],
  },
  {
    group: 'Thời gian & Chu kỳ',
    tags: [
      { tag: '{{Period}}', desc: 'Tên chu kỳ chốt (vd: Kỳ 1 Tháng 10/2026)' },
      { tag: '{{FromDate}}', desc: 'Từ ngày (dd/MM/yyyy)' },
      { tag: '{{ToDate}}', desc: 'Đến ngày (dd/MM/yyyy)' },
      { tag: '{{Month}}', desc: 'Tháng' },
      { tag: '{{Year}}', desc: 'Năm' },
    ],
  },
  {
    group: 'Chỉ số & Sản lượng (kWh)',
    tags: [
      { tag: '{{PreviousBt}}', desc: 'Chỉ số cũ BT' },
      { tag: '{{Bt}}', desc: 'Chỉ số mới BT' },
      { tag: '{{ConsumtionBt}}', desc: 'Sản lượng BT' },
      { tag: '{{PreviousCd}}', desc: 'Chỉ số cũ CD' },
      { tag: '{{Cd}}', desc: 'Chỉ số mới CD' },
      { tag: '{{ConsumtionCd}}', desc: 'Sản lượng CD' },
      { tag: '{{PreviousTd}}', desc: 'Chỉ số cũ TD' },
      { tag: '{{Td}}', desc: 'Chỉ số mới TD' },
      { tag: '{{ConsumtionTd}}', desc: 'Sản lượng TD' },
      { tag: '{{ConsumtionTotal}}', desc: 'Tổng sản lượng hữu công' },
      { tag: '{{ConsumtionVc}}', desc: 'Sản lượng vô công (kVARh)' },
      { tag: '{{Pmax}}', desc: 'Công suất Pmax (kW)' },
      { tag: '{{PmaxTime}}', desc: 'Thời điểm ghi nhận Pmax' },
    ],
  },
  {
    group: 'Chăm sóc khách hàng & Tài khoản',
    tags: [
      { tag: '{{CareFullname}}', desc: 'Tên đơn vị CSKH' },
      { tag: '{{CareEmail}}', desc: 'Email CSKH' },
      { tag: '{{CarePhone}}', desc: 'Hotline CSKH' },
      { tag: '{{Username}}', desc: 'Tên đăng nhập' },
      { tag: '{{Password}}', desc: 'Mật khẩu khởi tạo' },
      { tag: '{{ResetLink}}', desc: 'Liên kết đặt lại mật khẩu' },
    ],
  },
];

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

  // Preview Modal
  const [previewOpen, setPreviewOpen] = useState<boolean>(false);
  const [previewData, setPreviewData] = useState<PreviewNotificationTemplateResponse | null>(null);
  const [previewLoading, setPreviewLoading] = useState<boolean>(false);

  const fetchTemplates = async (codeFilter?: string) => {
    setLoading(true);
    try {
      const code = codeFilter === 'ALL' ? undefined : codeFilter;
      const res: any = await service.getNotificationTemplates(code);
      if (res?.data) {
        setTemplates(res.data);
      } else {
        setTemplates([]);
      }
    } catch (err: any) {
      message.error(`Lỗi khi tải danh sách mẫu email: ${err?.message || err}`);
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
    form.setFieldsValue({
      code: activeTab !== 'ALL' ? activeTab : 'STATEMENT_NOTIFICATION',
      isDefault: false,
      isActive: true,
      subjectTemplate: 'Thông báo từ hệ thống MDMS - {{CustomerName}}',
      bodyHtmlTemplate: '<p>Kính gửi {{CustomerName}},</p>\n<p>Nội dung thông báo...</p>',
    });
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
    setDrawerOpen(true);
  };

  const handleSetDefault = async (record: NotificationTemplateDto) => {
    try {
      await service.setDefaultNotificationTemplate(record.id);
      message.success(`Đã thiết lập mẫu "${record.name}" làm mặc định cho ${record.code}`);
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
      if (res?.data) {
        setPreviewData(res.data);
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
      if (res?.data) {
        setPreviewData(res.data);
      }
    } catch (err: any) {
      message.error(`Không thể xem trước mẫu: ${err?.message || err}`);
    } finally {
      setPreviewLoading(false);
    }
  };

  const insertTagToBody = (tag: string) => {
    const currentBody = form.getFieldValue('bodyHtmlTemplate') || '';
    form.setFieldsValue({
      bodyHtmlTemplate: currentBody + tag,
    });
    navigator.clipboard?.writeText(tag);
    message.success(`Đã sao chép và chèn thẻ ${tag}`);
  };

  const columns: ColumnsType<NotificationTemplateDto> = [
    {
      title: 'Tên mẫu',
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
          {record.description && <Text type="secondary" style={{ fontSize: 12 }}>{record.description}</Text>}
        </Space>
      ),
    },
    {
      title: 'Nghiệp vụ (Code)',
      dataIndex: 'code',
      key: 'code',
      width: 230,
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
          <Space orientation="vertical" size={1}>
            <Tag color={color}>{label}</Tag>
            <Text code style={{ fontSize: 11 }}>{code}</Text>
          </Space>
        );
      },
    },
    {
      title: 'Tiêu đề email (Subject)',
      dataIndex: 'subjectTemplate',
      key: 'subjectTemplate',
      ellipsis: true,
      render: (text) => <Text italic>{text}</Text>,
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
          <Tooltip title="Xem trước nội dung rendered">
            <Button
              type="text"
              icon={<EyeOutlined style={{ color: '#1890ff' }} />}
              onClick={() => handlePreviewDirect(record)}
            />
          </Tooltip>
          <Tooltip title="Chỉnh sửa mẫu">
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
              title="Bạn có chắc chắn muốn xóa mẫu này không?"
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
            <MailOutlined style={{ color: '#b22222', fontSize: 20 }} />
            <Title level={4} style={{ margin: 0 }}>
              Quản lý Đa Mẫu Email Thông Báo
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
            Tạo Mẫu Mới
          </Button>
        }
      >
        <Paragraph type="secondary" style={{ marginBottom: 16 }}>
          Hệ thống cho phép định nghĩa nhiều profile mẫu email cho từng nghiệp vụ (Gửi phiếu báo điện năng hàng tháng, Cấp mật khẩu mới, Quên mật khẩu). Mẫu được đánh dấu <strong>Mặc định</strong> sẽ tự động được sử dụng trong các tiến trình gửi hàng loạt của hệ thống.
        </Paragraph>

        <Tabs
          activeKey={activeTab}
          onChange={(key) => setActiveTab(key)}
          items={[
            { key: 'ALL', label: 'Tất cả mẫu' },
            { key: 'STATEMENT_NOTIFICATION', label: 'Phiếu báo điện năng' },
            { key: 'FORGOT_PASSWORD', label: 'Đặt lại mật khẩu' },
            { key: 'USER_CREDENTIAL', label: 'Cấp tài khoản mới' },
          ]}
        />

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
        title={editingTemplate ? `Chỉnh sửa Mẫu Email: ${editingTemplate.name}` : 'Tạo Mới Mẫu Email Thông Báo'}
        width={850}
        open={drawerOpen}
        onClose={() => setDrawerOpen(false)}
        extra={
          <Space>
            <Button icon={<EyeOutlined />} onClick={handlePreviewFromForm}>
              Xem Trước (Preview)
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
                label="Tên mẫu email"
                rules={[{ required: true, message: 'Vui lòng nhập tên mẫu email' }]}
              >
                <Input placeholder="Ví dụ: Mẫu Phiếu Báo Tháng KCN VSIP Chuẩn" />
              </Form.Item>
            </Col>
            <Col span={10}>
              <Form.Item
                name="code"
                label="Mã nghiệp vụ"
                rules={[{ required: true, message: 'Vui lòng chọn loại nghiệp vụ' }]}
              >
                <Select disabled={!!editingTemplate} options={TEMPLATE_CODES} />
              </Form.Item>
            </Col>
          </Row>

          <Form.Item
            name="subjectTemplate"
            label="Tiêu đề Email (Subject)"
            rules={[{ required: true, message: 'Vui lòng nhập tiêu đề email' }]}
            tooltip="Hỗ trợ các thẻ biến như {{CustomerName}}, {{SerialNumber}}, {{Period}}..."
          >
            <Input placeholder="Ví dụ: Phiếu Báo Số {{SerialNumber}} - {{CustomerName}}" />
          </Form.Item>

          <Row gutter={16}>
            <Col span={12}>
              <Form.Item name="description" label="Ghi chú / Mô tả mẫu">
                <Input placeholder="Mô tả phạm vi áp dụng của mẫu..." />
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

          {/* Merge Tags Palette */}
          <Card
            size="small"
            style={{ marginBottom: 16, background: '#fafafa', border: '1px dashed #d9d9d9' }}
            title={
              <Space>
                <CopyOutlined style={{ color: '#b22222' }} />
                <Text strong style={{ fontSize: 13 }}>
                  Danh mục Thẻ Biến Nghiệp Vụ (Nhấp vào thẻ để tự động chèn vào nội dung)
                </Text>
              </Space>
            }
          >
            {MERGE_TAGS_BY_GROUP.map((grp) => (
              <div key={grp.group} style={{ marginBottom: 8 }}>
                <Text type="secondary" style={{ fontSize: 12, display: 'block', marginBottom: 4 }}>
                  {grp.group}:
                </Text>
                <Space wrap size={[4, 6]}>
                  {grp.tags.map((t) => (
                    <Tooltip key={t.tag} title={t.desc}>
                      <Tag
                        color="processing"
                        style={{ cursor: 'pointer', userSelect: 'none' }}
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

          <Form.Item
            name="bodyHtmlTemplate"
            label="Nội dung Email (HTML Template)"
            rules={[{ required: true, message: 'Vui lòng nhập nội dung HTML của email' }]}
          >
            <TextArea
              rows={14}
              style={{ fontFamily: 'Consolas, monospace', fontSize: 13 }}
              placeholder="Nhập mã HTML của email tại đây..."
            />
          </Form.Item>
        </Form>
      </Drawer>

      {/* Preview Modal */}
      <Modal
        title={
          <Space>
            <EyeOutlined style={{ color: '#b22222' }} />
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
              <Text style={{ color: '#b22222', fontWeight: 600 }}>{previewData.subject}</Text>
            </div>
            <div
              style={{
                border: '1px solid #e8e8e8',
                borderRadius: 8,
                padding: 16,
                maxHeight: 500,
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
    </div>
  );
};

export default NotificationTemplatesPage;
