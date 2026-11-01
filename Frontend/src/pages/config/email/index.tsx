import { Card, Radio, Form, Input, InputNumber, Switch, Button, Alert, Space, Divider, Typography, Row, Col, Modal } from 'antd';
import { useEffect, useState } from 'react';
import { IoMailOutline, IoSaveOutline, IoSendOutline, IoCheckmarkCircleOutline, IoShieldCheckmarkOutline, IoServerOutline } from 'react-icons/io5';

import { BreadcrumbUI } from '@/infrastructure/common/components/controls/BreadcrumbUI';
import { FullPageLoading } from '@/infrastructure/common/components/controls/Loading';
import PageTitle from '@/infrastructure/common/components/title/page-title';
import { notifySuccess, notifyError } from '@/infrastructure/common/components/toast/toastMessage';
import useEmailConfigHook from '@/infrastructure/repository/config/hooks/useEmailConfigHook';

const MicrosoftIcon = ({ className = 'w-6 h-6' }: { className?: string }) => (
  <svg className={className} viewBox='0 0 24 24'>
    <path fill='#f25022' d='M1 1h10v10H1z' />
    <path fill='#00a4ef' d='M1 13h10v10H1z' />
    <path fill='#7fba00' d='M13 1h10v10H13z' />
    <path fill='#ffb900' d='M13 13h10v10H13z' />
  </svg>
);

const GoogleIcon = ({ className = 'w-6 h-6' }: { className?: string }) => (
  <svg className={className} viewBox='0 0 24 24'>
    <path fill='#4285F4' d='M23.745 12.27c0-.7-.06-1.4-.19-2.07H12v4.51h6.6c-.29 1.52-1.14 2.82-2.4 3.68v3.05h3.88c2.27-2.09 3.66-5.17 3.66-9.17z' />
    <path fill='#34A853' d='M12 24c3.24 0 5.95-1.08 7.93-2.91l-3.88-3.05c-1.08.72-2.45 1.16-4.05 1.16-3.12 0-5.77-2.1-6.72-4.93H1.25v3.15C3.26 21.36 7.33 24 12 24z' />
    <path fill='#FBBC05' d='M5.28 14.27c-.25-.72-.38-1.49-.38-2.27s.13-1.55.38-2.27V6.58H1.25C.45 8.18 0 9.99 0 12s.45 3.82 1.25 5.42l4.03-3.15z' />
    <path fill='#EA4335' d='M12 4.75c1.77 0 3.35.61 4.6 1.8l3.42-3.42C17.95 1.19 15.24 0 12 0 7.33 0 3.26 2.64 1.25 6.58l4.03 3.15c.95-2.83 3.6-4.98 6.72-4.98z' />
  </svg>
);

const { Title, Text, Paragraph } = Typography;

const EmailConfigPage = () => {
  const [form] = Form.useForm();
  const [loading, setLoading] = useState<boolean>(false);
  const [testing, setTesting] = useState<boolean>(false);
  const [hasClientSecret, setHasClientSecret] = useState<boolean>(false);
  const [hasSmtpPassword, setHasSmtpPassword] = useState<boolean>(false);
  const [providerType, setProviderType] = useState<string>('MICROSOFT_GRAPH');
  const [requireAuth, setRequireAuth] = useState<boolean>(true);
  const [testEmail, setTestEmail] = useState<string>('');
  const [testResult, setTestResult] = useState<{ success: boolean; message: string } | null>(null);

  const { getActiveEmailConfig, updateEmailConfig, testEmailConnection } = useEmailConfigHook();

  const loadData = async () => {
    await getActiveEmailConfig(
      (res: any) => {
        const data = res?.data;
        if (data) {
          const type = data.providerType || 'MICROSOFT_GRAPH';
          setProviderType(type);
          setHasClientSecret(!!data.hasClientSecret);
          setHasSmtpPassword(!!data.hasSmtpPassword);
          setRequireAuth(data.requireAuth !== false);

          form.setFieldsValue({
            providerType: type,
            senderEmail: data.senderEmail,
            senderDisplayName: data.senderDisplayName,
            tenantId: data.tenantId,
            clientId: data.clientId,
            clientSecret: '',
            smtpHost: data.smtpHost,
            smtpPort: data.smtpPort || 587,
            securityMode: data.securityMode || 'StartTls',
            requireAuth: data.requireAuth !== false,
            smtpUsername: data.smtpUsername,
            smtpPassword: '',
            isActive: data.isActive !== false,
          });
        }
      },
      () => {
        notifyError('Không thể tải cấu hình email từ máy chủ.');
      },
      setLoading
    );
  };

  useEffect(() => {
    loadData();
  }, []);

  const handleProviderChange = (e: any) => {
    const newType = e.target.value;
    setProviderType(newType);
    if (newType === 'GMAIL') {
      form.setFieldsValue({
        smtpHost: 'smtp.gmail.com',
        smtpPort: 587,
        securityMode: 'StartTls',
        requireAuth: true,
      });
      setRequireAuth(true);
    }
  };

  const onFinish = async (values: any) => {
    await updateEmailConfig(
      {
        providerType: values.providerType,
        senderEmail: values.senderEmail,
        senderDisplayName: values.senderDisplayName,
        tenantId: values.tenantId,
        clientId: values.clientId,
        clientSecret: values.clientSecret || null,
        smtpHost: values.smtpHost,
        smtpPort: values.smtpPort,
        securityMode: values.securityMode,
        requireAuth: values.requireAuth,
        smtpUsername: values.smtpUsername,
        smtpPassword: values.smtpPassword || null,
        isActive: values.isActive ?? true,
      },
      (res: any) => {
        notifySuccess('Lưu cấu hình dịch vụ email thành công!');
        setTestResult(null);
        loadData();
      },
      () => {
        notifyError('Lưu cấu hình thất bại, vui lòng kiểm tra lại thông tin.');
      },
      setLoading
    );
  };

  const handleTestConnection = async () => {
    if (!testEmail || !testEmail.includes('@')) {
      notifyError('Vui lòng nhập địa chỉ email nhận kiểm tra hợp lệ!');
      return;
    }

    const currentValues = form.getFieldsValue();
    setTesting(true);
    setTestResult(null);

    await testEmailConnection(
      {
        testRecipientEmail: testEmail,
        providerType: currentValues.providerType,
        senderEmail: currentValues.senderEmail,
        senderDisplayName: currentValues.senderDisplayName,
        tenantId: currentValues.tenantId,
        clientId: currentValues.clientId,
        clientSecret: currentValues.clientSecret || null,
        smtpHost: currentValues.smtpHost,
        smtpPort: currentValues.smtpPort,
        securityMode: currentValues.securityMode,
        requireAuth: currentValues.requireAuth,
        smtpUsername: currentValues.smtpUsername,
        smtpPassword: currentValues.smtpPassword || null,
      },
      (res: any) => {
        setTesting(false);
        setTestResult({
          success: true,
          message: `Gửi email kiểm tra thành công tới ${testEmail}! Hãy kiểm tra hòm thư đến hoặc mục Junk/Spam.`,
        });
        notifySuccess('Kiểm tra kết nối email thành công!');
      },
      () => {
        setTesting(false);
        setTestResult({
          success: false,
          message: 'Kết nối hoặc gửi email thử nghiệm thất bại. Vui lòng kiểm tra lại thông số cấu hình và quyền hạn tài khoản.',
        });
        notifyError('Kiểm tra kết nối thất bại!');
      }
    );
  };

  return (
    <div className='p-4 max-w-6xl mx-auto'>
      <BreadcrumbUI
        items={[
          { title: 'Cài đặt hệ thống' },
          { title: 'Cấu hình Email' },
        ]}
      />

      <div className='flex justify-between items-center my-4'>
        <PageTitle title='Cấu Hình Dịch Vụ Gửi Email Doanh Nghiệp' />
      </div>

      <FullPageLoading isLoading={loading} />

      <Row gutter={[24, 24]}>
        <Col xs={24} lg={16}>
          <Card className='shadow-sm rounded-lg border-gray-200'>
            <Form
              form={form}
              layout='vertical'
              onFinish={onFinish}
              initialValues={{
                providerType: 'MICROSOFT_GRAPH',
                smtpPort: 587,
                securityMode: 'StartTls',
                requireAuth: true,
                isActive: true,
              }}
            >
              <div className='mb-6'>
                <Text strong className='text-base text-gray-700 block mb-2'>
                  1. Chọn Nhà Cung Cấp Dịch Vụ Email
                </Text>
                <Form.Item name='providerType' noStyle>
                  <Radio.Group onChange={handleProviderChange} className='w-full grid grid-cols-1 md:grid-cols-3 gap-3'>
                    <Radio.Button
                      value='MICROSOFT_GRAPH'
                      className='h-auto p-4 text-center rounded-lg border flex flex-col items-center justify-center'
                    >
                      <MicrosoftIcon className='w-7 h-7 mb-2 mx-auto' />
                      <div className='font-semibold text-gray-800'>Microsoft 365</div>
                      <div className='text-xs text-gray-500'>Graph API OAuth 2.0 (VSIP HP)</div>
                    </Radio.Button>

                    <Radio.Button
                      value='CUSTOM_SMTP'
                      className='h-auto p-4 text-center rounded-lg border flex flex-col items-center justify-center'
                    >
                      <IoServerOutline className='text-2xl text-indigo-600 mb-2 mx-auto' />
                      <div className='font-semibold text-gray-800'>Custom SMTP</div>
                      <div className='text-xs text-gray-500'>Máy chủ Exchange / Nội bộ</div>
                    </Radio.Button>

                    <Radio.Button
                      value='GMAIL'
                      className='h-auto p-4 text-center rounded-lg border flex flex-col items-center justify-center'
                    >
                      <GoogleIcon className='w-7 h-7 mb-2 mx-auto' />
                      <div className='font-semibold text-gray-800'>Gmail</div>
                      <div className='text-xs text-gray-500'>Tài khoản Google App Password</div>
                    </Radio.Button>
                  </Radio.Group>
                </Form.Item>
              </div>

              <Divider />

              <div className='mb-6'>
                <Text strong className='text-base text-gray-700 block mb-3'>
                  2. Thông Tin Địa Chỉ Gửi Thư (Sender Info)
                </Text>
                <Row gutter={16}>
                  <Col xs={24} md={12}>
                    <Form.Item
                      label='Email người gửi (From Address)'
                      name='senderEmail'
                      rules={[
                        { required: true, message: 'Vui lòng nhập email người gửi' },
                        { type: 'email', message: 'Địa chỉ email không đúng định dạng' },
                      ]}
                      tooltip='Địa chỉ email được ủy quyền gửi (ví dụ: noreply.hoadondientu.hp@vsip.com.vn)'
                    >
                      <Input placeholder='noreply.hoadondientu.hp@vsip.com.vn' size='large' />
                    </Form.Item>
                  </Col>
                  <Col xs={24} md={12}>
                    <Form.Item
                      label='Tên hiển thị người gửi (Display Name)'
                      name='senderDisplayName'
                      rules={[{ required: true, message: 'Vui lòng nhập tên hiển thị' }]}
                    >
                      <Input placeholder='VSIP Hải Phòng - Phòng Cơ Điện' size='large' />
                    </Form.Item>
                  </Col>
                </Row>
              </div>

              {providerType === 'MICROSOFT_GRAPH' && (
                <>
                  <Divider />
                  <div className='mb-6'>
                    <div className='flex items-center gap-2 mb-3'>
                      <MicrosoftIcon className='w-5 h-5 inline-block' />
                      <Text strong className='text-base text-gray-700'>
                        3. Cấu Hình Microsoft Graph API (Azure AD / Entra ID)
                      </Text>
                    </div>

                    <Form.Item
                      label='Directory (Tenant) ID'
                      name='tenantId'
                      rules={[{ required: true, message: 'Vui lòng nhập Tenant ID' }]}
                      tooltip='Mã định danh Tenant trong Azure Active Directory'
                    >
                      <Input placeholder='5e5bd7f8-2adb-4595-956a-a03d68ea32a6' size='large' />
                    </Form.Item>

                    <Form.Item
                      label='Application (Client) ID'
                      name='clientId'
                      rules={[{ required: true, message: 'Vui lòng nhập Client ID' }]}
                      tooltip='Mã định danh App Registration trong Azure'
                    >
                      <Input placeholder='6a86904e-3153-496f-8c1b-986ae398747a' size='large' />
                    </Form.Item>

                    <Form.Item
                      label='Client Secret (Value)'
                      name='clientSecret'
                      tooltip='Khóa bí mật Client Secret của App Registration'
                    >
                      <Input.Password
                        placeholder={
                          hasClientSecret
                            ? '•••••••••••••••••••••••••• (Đã có khóa bí mật, nhập mới nếu muốn thay đổi)'
                            : 'Nhập Client Secret Value từ Azure AD'
                        }
                        size='large'
                      />
                    </Form.Item>

                    <div className='p-3 bg-blue-50 border border-blue-200 rounded-md text-xs text-blue-800 space-y-1'>
                      <p className='font-semibold'>Ghi chú bảo mật cho Microsoft Graph API:</p>
                      <p>• Cơ chế xác thực: <b>OAuth 2.0 Client Credentials Grant Flow</b>.</p>
                      <p>• Quyền ứng dụng cần cấp: <code>Mail.Send</code> (Application permission).</p>
                      <p>• Chính sách hạn chế: Đảm bảo đã gán <code>ApplicationAccessPolicy</code> cho phép ứng dụng gửi từ địa chỉ email người gửi ở trên.</p>
                    </div>
                  </div>
                </>
              )}

              {(providerType === 'CUSTOM_SMTP' || providerType === 'GMAIL') && (
                <>
                  <Divider />
                  <div className='mb-6'>
                    <div className='flex items-center gap-2 mb-3'>
                      <IoServerOutline className='text-xl text-indigo-600' />
                      <Text strong className='text-base text-gray-700'>
                        3. Thông Số Máy Chủ SMTP
                      </Text>
                    </div>

                    <Row gutter={16}>
                      <Col xs={24} md={16}>
                        <Form.Item
                          label='Máy chủ SMTP (Host)'
                          name='smtpHost'
                          rules={[{ required: true, message: 'Vui lòng nhập SMTP Host' }]}
                        >
                          <Input placeholder='smtp.gmail.com hoặc mail.company.vn' size='large' />
                        </Form.Item>
                      </Col>
                      <Col xs={24} md={8}>
                        <Form.Item
                          label='Cổng (Port)'
                          name='smtpPort'
                          rules={[{ required: true, message: 'Vui lòng nhập Port' }]}
                        >
                          <InputNumber className='w-full' size='large' min={1} max={65535} />
                        </Form.Item>
                      </Col>
                    </Row>

                    <Row gutter={16}>
                      <Col xs={24} md={12}>
                        <Form.Item label='Phương thức bảo mật (Security Mode)' name='securityMode'>
                          <Radio.Group buttonStyle='solid' className='w-full grid grid-cols-2 gap-2'>
                            <Radio.Button value='StartTls' className='text-center'>STARTTLS (587)</Radio.Button>
                            <Radio.Button value='SslOnConnect' className='text-center'>SSL/TLS (465)</Radio.Button>
                            <Radio.Button value='None' className='text-center'>None (25)</Radio.Button>
                            <Radio.Button value='Auto' className='text-center'>Tự động (Auto)</Radio.Button>
                          </Radio.Group>
                        </Form.Item>
                      </Col>
                      <Col xs={24} md={12}>
                        <Form.Item
                          label='Yêu cầu xác thực tài khoản'
                          name='requireAuth'
                          valuePropName='checked'
                          tooltip='Bật nếu máy chủ yêu cầu Username/Password; Tắt nếu dùng Internal IP Relay Whitelist'
                        >
                          <Switch
                            checked={requireAuth}
                            onChange={(checked) => setRequireAuth(checked)}
                            checkedChildren='Có'
                            unCheckedChildren='Không'
                          />
                        </Form.Item>
                      </Col>
                    </Row>

                    {requireAuth && (
                      <Row gutter={16}>
                        <Col xs={24} md={12}>
                          <Form.Item
                            label='Tên đăng nhập SMTP (Username)'
                            name='smtpUsername'
                            rules={[{ required: requireAuth, message: 'Vui lòng nhập Username' }]}
                          >
                            <Input placeholder='tentaikhoan@company.vn' size='large' />
                          </Form.Item>
                        </Col>
                        <Col xs={24} md={12}>
                          <Form.Item
                            label='Mật khẩu SMTP (Password)'
                            name='smtpPassword'
                          >
                            <Input.Password
                              placeholder={
                                hasSmtpPassword
                                  ? '•••••••••••••••••••••••••• (Đã có mật khẩu, nhập mới nếu muốn đổi)'
                                  : 'Nhập mật khẩu hoặc App Password'
                              }
                              size='large'
                            />
                          </Form.Item>
                        </Col>
                      </Row>
                    )}
                  </div>
                </>
              )}

              <Divider />

              <div className='flex justify-end gap-3'>
                <Button
                  type='primary'
                  htmlType='submit'
                  size='large'
                  icon={<IoSaveOutline />}
                  className='bg-blue-600 hover:bg-blue-700 min-w-[150px]'
                >
                  Lưu Cấu Hình
                </Button>
              </div>
            </Form>
          </Card>
        </Col>

        <Col xs={24} lg={8}>
          <Card title='Kiểm Tra Kết Nối (Test Connection)' className='shadow-sm rounded-lg border-gray-200'>
            <Paragraph className='text-sm text-gray-600'>
              Gửi một email thử nghiệm đến địa chỉ bất kỳ để kiểm tra xem cấu hình máy chủ và xác thực tài khoản có hoạt động bình thường hay không.
            </Paragraph>

            <div className='my-4'>
              <label className='block text-xs font-semibold text-gray-700 mb-1'>Email nhận thử nghiệm:</label>
              <Input
                placeholder='admin.test@vsip.com.vn'
                value={testEmail}
                onChange={(e) => setTestEmail(e.target.value)}
                size='large'
                prefix={<IoMailOutline className='text-gray-400' />}
              />
            </div>

            <Button
              type='default'
              size='large'
              block
              loading={testing}
              onClick={handleTestConnection}
              icon={<IoSendOutline />}
              className='border-blue-500 text-blue-600 hover:bg-blue-50'
            >
              Kiểm Tra Kết Nối & Gửi Thử
            </Button>

            {testResult && (
              <Alert
                className='mt-4'
                type={testResult.success ? 'success' : 'error'}
                showIcon
                message={testResult.success ? 'Thành công' : 'Thất bại'}
                description={testResult.message}
              />
            )}

            <Divider />

            <div className='text-xs text-gray-500 space-y-2'>
              <div className='flex items-center gap-1 font-semibold text-gray-700'>
                <IoShieldCheckmarkOutline className='text-green-600 text-base' />
                Bảo vệ an toàn thông tin:
              </div>
              <p>Mật khẩu SMTP và Azure Client Secret được bảo mật mã hóa AES và chỉ lưu trữ trên máy chủ cơ sở dữ liệu nội bộ.</p>
              <p>Hệ thống tự động sử dụng cấu hình này cho mọi tác vụ gửi email thông báo mật khẩu, reset mật khẩu và phát hành phiếu báo tiền điện.</p>
            </div>
          </Card>
        </Col>
      </Row>
    </div>
  );
};

export default EmailConfigPage;
