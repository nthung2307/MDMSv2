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
  CopyOutlined,
  DeleteOutlined,
  EditOutlined,
  EyeOutlined,
  PlusOutlined,
  SendOutlined,
  StarFilled,
  StarOutlined,
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

const { Text, Paragraph } = Typography;
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
      { tag: '{{FromDate}}', desc: 'Từ ngày (dd/MM/yyyy)' },
      { tag: '{{ToDate}}', desc: 'Đến ngày (dd/MM/yyyy)' },
      { tag: '{{Month}}', desc: 'Tháng' },
      { tag: '{{Year}}', desc: 'Năm' },
    ],
  },
  {
    group: 'Chỉ số & Sản lượng',
    tags: [
      { tag: '{{OutputBt}}', desc: 'Sản lượng BT (kWh)' },
      { tag: '{{OutputCd}}', desc: 'Sản lượng CD (kWh)' },
      { tag: '{{OutputTd}}', desc: 'Sản lượng TD (kWh)' },
      { tag: '{{OutputSg}}', desc: 'Tổng điện năng hữu công (kWh)' },
      { tag: '{{OutputVc}}', desc: 'Điện năng vô công (kVARh)' },
      { tag: '{{Pmax}}', desc: 'Công suất cực đại Pmax (kW)' },
      { tag: '{{CosPhi}}', desc: 'Hệ số công suất Cos φ' },
    ],
  },
  {
    group: 'Tài khoản & Xác thực',
    tags: [
      { tag: '{{FullName}}', desc: 'Họ và tên người dùng' },
      { tag: '{{Username}}', desc: 'Tên đăng nhập hệ thống' },
      { tag: '{{Password}}', desc: 'Mật khẩu khởi tạo / OTP' },
      { tag: '{{SystemEmail}}', desc: 'Email quản trị hệ thống' },
      { tag: '{{SupportPhone}}', desc: 'Số điện thoại hỗ trợ CSKH' },
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
    form.setFieldsValue({
      code: activeTab !== 'ALL' ? activeTab : 'STATEMENT_NOTIFICATION',
      isDefault: false,
      isActive: true,
      subjectTemplate: '[VSIP Hải Phòng] Thông báo chỉ số & sản lượng điện kỳ {{CycleName}} - Điểm đo {{PointName}}',
      bodyHtmlTemplate: '<p>Kính gửi Quý khách hàng: <b>{{CustomerName}}</b>,</p>\n<p>Phòng Cơ Điện trân trọng gửi tới Quý khách thông tin chỉ số điện năng...</p>',
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

  // Test Email Sending State & Handlers
  const [testSendOpen, setTestSendOpen] = useState<boolean>(false);
  const [testEmailTarget, setTestEmailTarget] = useState<string>('');
  const [testSending, setTestSending] = useState<boolean>(false);
  const [testSendPayload, setTestSendPayload] = useState<{
    code?: string;
    subjectTemplate?: string;
    bodyHtmlTemplate?: string;
  } | null>(null);

  const handleOpenTestSendFromForm = () => {
    const values = form.getFieldsValue();
    setTestSendPayload({
      code: values.code,
      subjectTemplate: values.subjectTemplate || '',
      bodyHtmlTemplate: values.bodyHtmlTemplate || '',
    });
    setTestSendOpen(true);
  };

  const handleOpenTestSendFromRecord = (record: NotificationTemplateDto) => {
    setTestSendPayload({
      code: record.code,
      subjectTemplate: record.subjectTemplate,
      bodyHtmlTemplate: record.bodyHtmlTemplate,
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

  const insertTagToBody = (tag: string) => {
    const currentBody = form.getFieldValue('bodyHtmlTemplate') || '';
    form.setFieldsValue({
      bodyHtmlTemplate: currentBody + tag,
    });
    navigator.clipboard?.writeText(tag);
    message.success(`Đã chèn thẻ biến ${tag}`);
  };

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
        <div className='flex'>
          <ButtonCommonCustomizable
            title={_t('Thêm mới')}
            tooltipText={'Tạo mẫu email mới'}
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
              <h1 className='font-bold text-20-20'>{'Mẫu Email thông báo'}</h1>
            </div>

            <Paragraph type="secondary" className="mb-4">
              Hệ thống cho phép tùy biến nhiều profile mẫu email cho từng nghiệp vụ (Gửi thông báo chỉ số & phiếu báo điện năng hàng tháng, Cấp mật khẩu mới, Quên mật khẩu). Mẫu được đánh dấu <strong>Mặc định</strong> sẽ tự động được sử dụng trong các tiến trình gửi email của hệ thống.
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

      {/* Drawer Create / Edit */}
      <Drawer
        title={editingTemplate ? `Chỉnh sửa Mẫu Email: ${editingTemplate.name}` : 'Tạo Mới Mẫu Email Thông Báo'}
        width={850}
        open={drawerOpen}
        onClose={() => setDrawerOpen(false)}
        extra={
          <Space>
            <Button icon={<EyeOutlined />} onClick={handlePreviewFromForm}>
              Xem Trước
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
            <Col span={14}>
              <Form.Item
                name="name"
                label="Tên mẫu email"
                rules={[{ required: true, message: 'Vui lòng nhập tên mẫu email' }]}
              >
                <Input placeholder="Ví dụ: Mẫu Thông Báo Phiếu Báo Tháng VSIP Hải Phòng" />
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
            tooltip="Hỗ trợ các thẻ biến như {{CustomerName}}, {{PointName}}, {{CycleName}}..."
          >
            <Input placeholder="Ví dụ: [VSIP Hải Phòng] Phiếu báo chỉ số điện kỳ {{CycleName}} - Điểm đo {{PointName}}" />
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
                <CopyOutlined style={{ color: '#003366' }} />
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
            rules={[{ required: true, message: 'Vui lòng nhập nội dung mẫu email' }]}
            tooltip="Hỗ trợ mã HTML hoàn chỉnh và các thẻ biến {{Key}}"
          >
            <TextArea
              rows={12}
              style={{ fontFamily: 'Consolas, monospace', fontSize: 13 }}
              placeholder="Nhập mã HTML của mẫu email..."
            />
          </Form.Item>
        </Form>
      </Drawer>

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
