import { Card, Radio, Form, Input, InputNumber, Switch, Button, Alert, Space, Divider, Typography, Row, Col } from 'antd';
import { useEffect, useState } from 'react';
import { IoMailOutline, IoSendOutline, IoCheckmarkCircleOutline, IoServerOutline } from 'react-icons/io5';
import { TbBook2 } from 'react-icons/tb';

import { ButtonCommonCustomizable } from '@/infrastructure/common/components/button/button-common-customizable';
import { BreadcrumbUI } from '@/infrastructure/common/components/controls/BreadcrumbUI';
import { FullPageLoading } from '@/infrastructure/common/components/controls/Loading';
import DialogCommon from '@/infrastructure/common/components/popup/dialogCommon';
import PageTitle from '@/infrastructure/common/components/title/page-title';
import { notifySuccess, notifyError } from '@/infrastructure/common/components/toast/toastMessage';
import useEmailConfigHook from '@/infrastructure/repository/config/hooks/useEmailConfigHook';
import { _t } from '@/infrastructure/utils/translation';

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

const { Text } = Typography;

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
  const [isShowDialog, setIsShowDialog] = useState<boolean>(false);

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
            senderDisplayName: data.senderDisplayName || '',
            tenantId: data.tenantId,
            clientId: data.clientId,
            clientSecret: '',
            smtpHost: data.smtpHost,
            smtpPort: data.smtpPort || 587,
            securityMode: data.securityMode || 'StartTls',
            requireAuth: data.requireAuth !== false,
            smtpUsername: data.smtpUsername,
            smtpPassword: '',
            isActive: true,
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

  const executeSave = async () => {
    try {
      const values = await form.validateFields();
      setIsShowDialog(false);
      await updateEmailConfig(
        {
          providerType: values.providerType,
          senderEmail: values.senderEmail,
          senderDisplayName: values.senderDisplayName || values.senderEmail || '',
          tenantId: values.tenantId,
          clientId: values.clientId,
          clientSecret: values.clientSecret || null,
          smtpHost: values.smtpHost,
          smtpPort: values.smtpPort,
          securityMode: values.securityMode,
          requireAuth: values.requireAuth,
          smtpUsername: values.smtpUsername,
          smtpPassword: values.smtpPassword || null,
          isActive: true,
        },
        () => {
          notifySuccess('', 'Lưu cấu hình email thành công');
          setTestResult(null);
          loadData();
        },
        () => {
          notifyError('Lưu cấu hình thất bại, vui lòng kiểm tra lại thông tin.');
        },
        setLoading
      );
    } catch {
      notifyError('Vui lòng kiểm tra lại các trường thông tin bắt buộc.');
    }
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
        senderDisplayName: currentValues.senderDisplayName || currentValues.senderEmail || '',
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
      () => {
        setTesting(false);
        setTestResult({
          success: true,
          message: `Gửi email kiểm tra thành công tới ${testEmail}! Hãy kiểm tra hòm thư đến hoặc Junk/Spam.`,
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
    <div>
      <PageTitle title={'Cài đặt hệ thống'} />
      <div className='flex justify-between items-center'>
        <BreadcrumbUI items={[{ name: 'Cài đặt hệ thống' }, { name: 'Cấu hình Email' }]} />
        <div className='flex'>
          <ButtonCommonCustomizable
            title={''}
            tooltipText={'Lưu'}
            classButton='text-button-variant'
            disabled={loading || testing}
            onClick={() => {
              setIsShowDialog(true);
            }}
            icon={<TbBook2 size={30} />}
          />
        </div>
      </div>

      <div className='mt-10'>
        <Card className={'shadow-custom'}>
          <div>
            <div className={'flex justify-between items-center mb-6'}>
              <h1 className='font-bold text-20-20'>{'Cấu hình dịch vụ Email'}</h1>
            </div>

            <Form
              form={form}
              layout='vertical'
              initialValues={{
                providerType: 'MICROSOFT_GRAPH',
                smtpPort: 587,
                securityMode: 'StartTls',
                requireAuth: true,
                isActive: true,
              }}
            >
              {/* 1. Chọn Nhà Cung Cấp */}
              <div className='mb-6 p-4 bg-gray-50 rounded-lg border border-gray-100'>
                <Text strong className='text-16-20 text-gray-800 block mb-3'>
                  1. Nền tảng dịch vụ Email
                </Text>
                <Form.Item name='providerType' noStyle>
                  <Radio.Group onChange={handleProviderChange} className='w-full grid grid-cols-1 md:grid-cols-3 gap-4'>
                    <Radio.Button
                      value='MICROSOFT_GRAPH'
                      className='h-auto p-4 text-center rounded-lg border flex flex-col items-center justify-center hover:border-red-500 transition-colors'
                      style={{ minHeight: 96 }}
                    >
                      <MicrosoftIcon className='w-8 h-8 mb-2 mx-auto' />
                      <div className='font-semibold text-gray-800'>Microsoft 365</div>
                      <div className='text-xs text-gray-500 mt-1'>Graph API OAuth 2.0</div>
                    </Radio.Button>

                    <Radio.Button
                      value='CUSTOM_SMTP'
                      className='h-auto p-4 text-center rounded-lg border flex flex-col items-center justify-center hover:border-red-500 transition-colors'
                      style={{ minHeight: 96 }}
                    >
                      <IoServerOutline className='text-3xl text-indigo-600 mb-2 mx-auto' />
                      <div className='font-semibold text-gray-800'>Custom SMTP</div>
                    </Radio.Button>

                    <Radio.Button
                      value='GMAIL'
                      className='h-auto p-4 text-center rounded-lg border flex flex-col items-center justify-center hover:border-red-500 transition-colors'
                      style={{ minHeight: 96 }}
                    >
                      <GoogleIcon className='w-8 h-8 mb-2 mx-auto' />
                      <div className='font-semibold text-gray-800'>Gmail</div>
                    </Radio.Button>
                  </Radio.Group>
                </Form.Item>
              </div>

              {/* 2. Thông Tin Người Gửi */}
              <div className='mb-6'>
                <Text strong className='text-16-20 text-gray-800 block mb-4'>
                  2. Thông tin hộp thư gửi (Sender Information)
                </Text>
                <Row gutter={24}>
                  <Col xs={24} md={12}>
                    <Form.Item
                      label='Email'
                      name='senderEmail'
                      rules={[
                        { required: true, message: 'Vui lòng nhập địa chỉ email người gửi' },
                        { type: 'email', message: 'Địa chỉ email không đúng định dạng' },
                      ]}
                      tooltip='Địa chỉ email được ủy quyền gửi trên hệ thống (Ví dụ: noreply.hoadondientu.hp@vsip.com.vn)'
                    >
                      <Input
                        size='large'
                        prefix={<IoMailOutline className='text-gray-400 mr-1' />}
                        placeholder='Ví dụ: noreply.hoadondientu.hp@vsip.com.vn'
                      />
                    </Form.Item>
                  </Col>
                  <Col xs={24} md={12}>
                    <Form.Item
                      label='Tên hiển thị người gửi (Sender Display Name)'
                      name='senderDisplayName'
                      tooltip='Tên hiển thị trên hộp thư người nhận (Ví dụ: VSIP Hải Phòng - AMR/MDMS)'
                    >
                      <Input
                        size='large'
                        placeholder='Ví dụ: VSIP Hải Phòng - AMR System'
                      />
                    </Form.Item>
                  </Col>
                </Row>
              </div>

              <Divider className='my-6' />

              {/* 3. Chi tiết Cấu hình theo Loại */}
              {providerType === 'MICROSOFT_GRAPH' ? (
                <div className='mb-6'>
                  <Text strong className='text-16-20 text-gray-800 block mb-4'>
                    3. Thông số xác thực Microsoft Azure Entra ID (OAuth 2.0)
                  </Text>
                  <Row gutter={24}>
                    <Col xs={24} md={12}>
                      <Form.Item
                        label='Directory (Tenant) ID'
                        name='tenantId'
                        rules={[{ required: true, message: 'Vui lòng nhập Tenant ID' }]}
                      >
                        <Input size='large' placeholder='5e5bd7f8-2adb-4595-956a-a03d68ea32a6' />
                      </Form.Item>
                    </Col>
                    <Col xs={24} md={12}>
                      <Form.Item
                        label='Application (Client) ID'
                        name='clientId'
                        rules={[{ required: true, message: 'Vui lòng nhập Client ID' }]}
                      >
                        <Input size='large' placeholder='6a86904e-3153-496f-8c1b-986ae398747a' />
                      </Form.Item>
                    </Col>
                    <Col xs={24}>
                      <Form.Item
                        label='Client Secret Value'
                        name='clientSecret'
                        required={!hasClientSecret}
                        rules={
                          !hasClientSecret
                            ? [{ required: true, message: 'Vui lòng nhập Client Secret Value' }]
                            : []
                        }
                        tooltip={
                          hasClientSecret
                            ? 'Mật khẩu ứng dụng đã được lưu trên hệ thống. Để trống nếu giữ nguyên.'
                            : 'Khóa bảo mật ứng dụng Client Secret từ Azure Portal'
                        }
                      >
                        <Input.Password
                          size='large'
                          placeholder={
                            hasClientSecret
                              ? '•••••••••••••••••••• (Đã có secret được lưu an toàn - Để trống nếu giữ nguyên)'
                              : 'Nhập Client Secret Value từ Azure Portal'
                          }
                        />
                      </Form.Item>
                    </Col>
                  </Row>
                </div>
              ) : (
                <div className='mb-6'>
                  <Text strong className='text-16-20 text-gray-800 block mb-4'>
                    3. Thông số máy chủ SMTP
                  </Text>
                  <Row gutter={24}>
                    <Col xs={24} md={14}>
                      <Form.Item
                        label='Địa chỉ máy chủ SMTP (Host)'
                        name='smtpHost'
                        rules={[{ required: true, message: 'Vui lòng nhập máy chủ SMTP' }]}
                      >
                        <Input size='large' placeholder='smtp.gmail.com hoặc mail.company.com.vn' />
                      </Form.Item>
                    </Col>
                    <Col xs={24} md={10}>
                      <Form.Item
                        label='Cổng (Port)'
                        name='smtpPort'
                        rules={[{ required: true, message: 'Vui lòng nhập cổng SMTP' }]}
                      >
                        <InputNumber size='large' className='w-full' min={1} max={65535} />
                      </Form.Item>
                    </Col>
                    <Col xs={24} md={12}>
                      <Form.Item label='Phương thức mã hóa (Security Mode)' name='securityMode'>
                        <Radio.Group size='large' className='w-full flex gap-2'>
                          <Radio.Button value='StartTls' className='flex-1 text-center'>STARTTLS (587)</Radio.Button>
                          <Radio.Button value='SslOnConnect' className='flex-1 text-center'>SSL/TLS (465)</Radio.Button>
                          <Radio.Button value='None' className='flex-1 text-center'>None (25)</Radio.Button>
                        </Radio.Group>
                      </Form.Item>
                    </Col>
                    <Col xs={24} md={12}>
                      <Form.Item label='Yêu cầu xác thực tài khoản (Require Auth)' name='requireAuth' valuePropName='checked'>
                        <Switch
                          checked={requireAuth}
                          onChange={(checked) => setRequireAuth(checked)}
                          checkedChildren='Bắt buộc đăng nhập'
                          unCheckedChildren='Không cần đăng nhập'
                        />
                      </Form.Item>
                    </Col>

                    {requireAuth && (
                      <>
                        <Col xs={24} md={12}>
                          <Form.Item
                            label='Tên đăng nhập SMTP (Username)'
                            name='smtpUsername'
                            rules={[{ required: requireAuth, message: 'Vui lòng nhập tên đăng nhập' }]}
                          >
                            <Input size='large' placeholder='Tài khoản email đăng nhập' />
                          </Form.Item>
                        </Col>
                        <Col xs={24} md={12}>
                          <Form.Item
                            label={providerType === 'GMAIL' ? 'Mật khẩu ứng dụng Google (16 ký tự)' : 'Mật khẩu SMTP (Password)'}
                            name='smtpPassword'
                            required={!hasSmtpPassword}
                            rules={
                              !hasSmtpPassword
                                ? [{ required: true, message: 'Vui lòng nhập mật khẩu' }]
                                : []
                            }
                            tooltip={
                              hasSmtpPassword
                                ? 'Mật khẩu đã được mã hóa bảo vệ. Để trống nếu giữ nguyên.'
                                : 'Mật khẩu ứng dụng gửi thư'
                            }
                          >
                            <Input.Password
                              size='large'
                              placeholder={
                                hasSmtpPassword
                                  ? '•••••••••••••••••••• (Đã có mật khẩu được lưu - Để trống nếu giữ nguyên)'
                                  : 'Nhập mật khẩu SMTP'
                              }
                            />
                          </Form.Item>
                        </Col>
                      </>
                    )}
                  </Row>
                </div>
              )}

              {/* 4. Kiểm Tra Kết Nối */}
              <div className='p-5 bg-blue-50 border border-blue-200 rounded-lg mt-6'>
                <div className='flex items-center gap-2 mb-3'>
                  <IoCheckmarkCircleOutline className='text-blue-600 text-xl' />
                  <Text strong className='text-16-20 text-blue-900'>
                    Kiểm tra kết nối (Test Connection)
                  </Text>
                </div>
                <p className='text-gray-600 text-sm mb-4'>
                  Gửi một email kiểm tra tức thời đến địa chỉ bất kỳ để xác nhận cấu hình máy chủ và quyền hạn tài khoản hoạt động ổn định.
                </p>
                <div className='flex flex-col sm:flex-row gap-3 items-stretch sm:items-center'>
                  <Input
                    size='large'
                    prefix={<IoMailOutline className='text-gray-400 mr-1' />}
                    placeholder='Nhập địa chỉ email nhận kiểm tra (vd: admin.it@vsip.com.vn)'
                    value={testEmail}
                    onChange={(e) => setTestEmail(e.target.value)}
                    className='flex-1'
                  />
                  <Button
                    type='primary'
                    size='large'
                    icon={<IoSendOutline />}
                    loading={testing}
                    onClick={handleTestConnection}
                    style={{ backgroundColor: '#003366', borderColor: '#003366' }}
                  >
                    Kiểm tra kết nối
                  </Button>
                </div>

                {testResult && (
                  <div className='mt-4'>
                    <Alert
                      type={testResult.success ? 'success' : 'error'}
                      showIcon
                      message={testResult.success ? 'Kết nối thành công!' : 'Kết nối thất bại!'}
                      description={testResult.message}
                    />
                  </div>
                )}
              </div>
            </Form>
          </div>
        </Card>
      </div>

      <FullPageLoading isLoading={loading} />

      <DialogCommon
        title={''}
        visible={isShowDialog}
        handleCancel={() => setIsShowDialog(false)}
        handleConfirm={executeSave}
        isButtonSubmit={true}
        showFooter={true}
      >
        <div className='flex flex-col gap-4'>
          <div className='flex flex-col justify-center items-center my-5'>
            <p className='text-16-24 font-bold'>{_t('Xác nhận lưu cấu hình dịch vụ email')}</p>
            <p className='text-center text-gray-600 mt-2'>
              {_t('Sau khi xác nhận, cấu hình email mới sẽ được áp dụng cho toàn bộ thông báo tài khoản và gửi phiếu báo của hệ thống.')}
            </p>
          </div>
        </div>
      </DialogCommon>
    </div>
  );
};

export default EmailConfigPage;
