import { useCallback, useEffect, useState } from 'react';
import {
  Card,
  Divider,
  Input,
  Switch,
  Tag,
  Typography,
} from 'antd';
import {
  ApiOutlined,
  CheckCircleOutlined,
  ClockCircleOutlined,
  CloseCircleOutlined,
  CopyOutlined,
  EyeInvisibleOutlined,
  EyeOutlined,
  KeyOutlined,
  PlusOutlined,
  ReloadOutlined,
  SaveOutlined,
  ThunderboltOutlined,
} from '@ant-design/icons';
import { Button } from 'antd';

import Constants from '@/core/application/common/constants';
import { Endpoint } from '@/core/application/common/endPoint';
import { ButtonCommonCustomizable } from '@/infrastructure/common/components/button/button-common-customizable';
import { BreadcrumbUI } from '@/infrastructure/common/components/controls/BreadcrumbUI';
import { FullPageLoading } from '@/infrastructure/common/components/controls/Loading';
import PageTitle from '@/infrastructure/common/components/title/page-title';
import { notifyError, notifySuccess } from '@/infrastructure/common/components/toast/toastMessage';
import SocketCommon from '@/infrastructure/common/components/websocket/socket-common';
import useIoTConfigHook from '@/infrastructure/repository/config/hooks/useIoTConfigHook';
import { ConfigManagementService } from '@/infrastructure/repository/config/services/configManagement.service';
import { _t } from '@/infrastructure/utils/translation';

const { Text } = Typography;

interface IoTTelemetryState {
  lastPushTime: string | null;
  lastPushIp: string | null;
  lastPushStatus: string | null;
  lastPushTagCount: number;
  totalPushCount: number;
  isActiveRecently: boolean;
}

const mapTelemetryPayload = (data: any): IoTTelemetryState => ({
  lastPushTime: data?.lastPushTime ?? data?.LastPushTime ?? null,
  lastPushIp: data?.lastPushIp ?? data?.LastPushIp ?? null,
  lastPushStatus: data?.lastPushStatus ?? data?.LastPushStatus ?? null,
  lastPushTagCount: Number(data?.lastPushTagCount ?? data?.LastPushTagCount ?? 0),
  totalPushCount: Number(data?.totalPushCount ?? data?.TotalPushCount ?? 0),
  isActiveRecently: Boolean(data?.isActiveRecently ?? data?.IsActiveRecently),
});

const TELEMETRY_POLL_INTERVAL_MS = 5000;

const IoTConfigPage = () => {
  const [loading, setLoading] = useState<boolean>(false);
  const { getIoTConfig, updateIoTConfig, generateIoTKey } = useIoTConfigHook();

  // Form states
  const [apiKey, setApiKey] = useState<string>('');
  const [showKey, setShowKey] = useState<boolean>(false);
  const [requireApiKey, setRequireApiKey] = useState<boolean>(false);
  const [allowedIpsList, setAllowedIpsList] = useState<string[]>([]);
  const [inputIp, setInputIp] = useState<string>('');

  // Live Telemetry states
  const [telemetrySocketData, setTelemetrySocketData] = useState<any>(null);
  const [telemetry, setTelemetry] = useState<IoTTelemetryState>({
    lastPushTime: null,
    lastPushIp: null,
    lastPushStatus: null,
    lastPushTagCount: 0,
    totalPushCount: 0,
    isActiveRecently: false,
  });

  const loadConfigAsync = async () => {
    setLoading(true);
    try {
      await getIoTConfig(
        (res: any) => {
          const data = res?.data;
          if (data) {
            setApiKey(data.apiKey || '');
            setRequireApiKey(Boolean(data.requireApiKey));

            const rawIps = (data.allowedIps || '')
              .split(/[,;]/)
              .map((ip: string) => ip.trim())
              .filter((ip: string) => Boolean(ip));
            setAllowedIpsList(rawIps);

            setTelemetry(mapTelemetryPayload(data));
          }
        },
        () => {
          notifyError('Không thể tải cấu hình IoT Gateway.');
        },
        setLoading
      );
    } finally {
      setLoading(false);
    }
  };

  // Cập nhật im lặng trạng thái kết nối theo chu kỳ (không ghi đè form đang chỉnh sửa)
  const refreshTelemetryOnlyAsync = useCallback(async () => {
    try {
      const res: any = await new ConfigManagementService().getIoTConfigAsync(
        Endpoint.Config.GetIoTConfig,
        {},
        null as any
      );
      const data = res?.data?.data ?? res?.data;
      if (data) {
        setTelemetry(mapTelemetryPayload(data));
      }
    } catch {
      // Bỏ qua lỗi polling nền để không làm gián đoạn thao tác cấu hình
    }
  }, []);

  useEffect(() => {
    void loadConfigAsync();
    const interval = setInterval(() => {
      void refreshTelemetryOnlyAsync();
    }, TELEMETRY_POLL_INTERVAL_MS);
    return () => clearInterval(interval);
  }, [refreshTelemetryOnlyAsync]);

  useEffect(() => {
    if (telemetrySocketData) {
      setTelemetry(mapTelemetryPayload(telemetrySocketData));
    }
  }, [telemetrySocketData]);

  // Thêm IP vào Whitelist
  const handleAddIp = () => {
    const trimmed = inputIp.trim();
    if (!trimmed) return;

    if (allowedIpsList.includes(trimmed)) {
      notifyError('Địa chỉ IP này đã tồn tại trong danh sách.');
      return;
    }

    setAllowedIpsList([...allowedIpsList, trimmed]);
    setInputIp('');
  };

  // Xóa IP khỏi Whitelist
  const handleRemoveIp = (ipToRemove: string) => {
    setAllowedIpsList(allowedIpsList.filter((ip) => ip !== ipToRemove));
  };

  // Sinh khóa ngẫu nhiên
  const handleGenerateKey = async () => {
    await generateIoTKey(
      (res: any) => {
        const newKey = res?.data;
        if (newKey) {
          setApiKey(newKey);
          setShowKey(true);
          notifySuccess('Đã sinh khóa API ngẫu nhiên bảo mật cao.');
        }
      },
      () => {
        notifyError('Không thể sinh khóa API mới.');
      },
      setLoading
    );
  };

  // Lưu cấu hình
  const handleSaveConfig = async () => {
    const payload = {
      apiKey: apiKey.trim(),
      allowedIps: allowedIpsList.join(', '),
      requireApiKey: requireApiKey,
    };

    await updateIoTConfig(
      payload,
      (res: any) => {
        notifySuccess('Lưu cấu hình IoT Gateway thành công!');
        const data = res?.data;
        if (data) {
          setTelemetry(mapTelemetryPayload(data));
        }
      },
      () => {
        notifyError('Có lỗi xảy ra khi lưu cấu hình IoT Gateway.');
      },
      setLoading
    );
  };

  const formatTimestamp = (ts: string | null) => {
    if (!ts) return '---';
    try {
      const d = new Date(ts);
      return d.toLocaleString('vi-VN', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit',
        hour: '2-digit',
        minute: '2-digit',
        second: '2-digit',
      });
    } catch {
      return ts;
    }
  };

  return (
    <div>
      <PageTitle title={'Cài đặt hệ thống'} />
      <SocketCommon
        setDataSocket={setTelemetrySocketData}
        endPoint={Constants.socketIoEndpoint.summary}
        status={Constants.socketIoStatus.ReceiveIoTTelemetry}
      >
        <div className='flex justify-between items-center mb-5'>
          <BreadcrumbUI
            items={[
              { name: 'Cài đặt hệ thống' },
              { name: 'Cài đặt IOT Gateway' },
              { name: 'Cài đặt REST Server' },
            ]}
          />
          <div className='flex'>
            <ButtonCommonCustomizable
              title={null}
              tooltipText={_t('Lưu cấu hình')}
              classButton='text-button-variant'
              onClick={handleSaveConfig}
              icon={<SaveOutlined style={{ fontSize: 20 }} />}
            />
          </div>
        </div>

        <div className='space-y-6'>
          {/* CARD 1: LIVE CONNECTION TELEMETRY */}
          <Card
            className='shadow-custom'
            title={
              <div className='flex items-center justify-between'>
                <div className='flex items-center gap-2'>
                  <ThunderboltOutlined style={{ color: '#003366', fontSize: 18 }} />
                  <span className='font-bold text-16-20 text-[#003366]'>
                    Trạng thái kết nối (Live telemetry)
                  </span>
                </div>
                <div>
                  {telemetry.isActiveRecently ? (
                    <Tag color='success' icon={<CheckCircleOutlined />}>
                      Hoạt động (Active)
                    </Tag>
                  ) : (
                    <Tag color='default' icon={<ClockCircleOutlined />}>
                      Chưa có kết nối gần đây
                    </Tag>
                  )}
                </div>
              </div>
            }
          >
            <div>
              <Text strong className='text-14 text-gray-700 block mb-3'>
                Thông tin gần nhất
              </Text>
              <div className='grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-5 gap-3'>
                <div className='p-3 bg-gray-50 rounded-lg border border-gray-200'>
                  <div className='text-gray-500 text-12'>Thời điểm nhận</div>
                  <div className='text-15 font-semibold text-gray-800 mt-1'>
                    {formatTimestamp(telemetry.lastPushTime)}
                  </div>
                </div>
                <div className='p-3 bg-gray-50 rounded-lg border border-gray-200'>
                  <div className='text-gray-500 text-12'>IP máy Client</div>
                  <div className='text-15 font-semibold text-blue-700 mt-1 font-mono'>
                    {telemetry.lastPushIp || '---'}
                  </div>
                </div>
                <div className='p-3 bg-gray-50 rounded-lg border border-gray-200'>
                  <div className='text-gray-500 text-12'>Số lượng Tags</div>
                  <div className='text-15 font-semibold text-green-700 mt-1'>
                    {telemetry.lastPushTagCount > 0 ? `${telemetry.lastPushTagCount} Tags` : '---'}
                  </div>
                </div>
                <div className='p-3 bg-gray-50 rounded-lg border border-gray-200'>
                  <div className='text-gray-500 text-12'>Tổng số lượt nhận</div>
                  <div className='text-15 font-semibold text-purple-700 mt-1'>
                    {telemetry.totalPushCount.toLocaleString()} lượt
                  </div>
                </div>
                <div className='p-3 bg-gray-50 rounded-lg border border-gray-200'>
                  <div className='text-gray-500 text-12'>Trạng thái gói tin</div>
                  <div className='mt-1'>
                    {!telemetry.lastPushStatus ? (
                      <span className='text-15 font-semibold text-gray-800'>---</span>
                    ) : telemetry.lastPushStatus === 'SUCCESS' ? (
                      <Tag color='green' className='m-0'>
                        HTTP 200 - Hợp lệ
                      </Tag>
                    ) : (
                      <Tag color='red' icon={<CloseCircleOutlined />} className='m-0'>
                        {telemetry.lastPushStatus} - Bị từ chối
                      </Tag>
                    )}
                  </div>
                </div>
              </div>
            </div>
          </Card>

          {/* CARD 2: API KEY AUTHENTICATION */}
          <Card
            className='shadow-custom'
            title={
              <div className='flex items-center gap-2'>
                <KeyOutlined style={{ color: '#003366', fontSize: 18 }} />
                <span className='font-bold text-16-20 text-[#003366]'>
                  Khóa xác thực (API key)
                </span>
              </div>
            }
          >
            <div className='space-y-4'>
              <div>
                <div className='text-14 font-medium text-gray-700 mb-2'>
                  Khóa xác thực (API key)
                </div>
                <div className='flex gap-2 items-center'>
                  <Input
                    type={showKey ? 'text' : 'password'}
                    value={apiKey}
                    onChange={(e) => setApiKey(e.target.value)}
                    placeholder='Ví dụ: MDMS_kE7x_9A2_bB3qW8...'
                    className='font-mono'
                    style={{ maxWidth: 500, height: 44 }}
                    suffix={
                      <Button
                        type='text'
                        size='small'
                        icon={showKey ? <EyeInvisibleOutlined /> : <EyeOutlined />}
                        onClick={() => setShowKey(!showKey)}
                      />
                    }
                  />
                  <ButtonCommonCustomizable
                    title={null}
                    tooltipText={_t('Sinh khóa mới')}
                    classButton='text-button-variant'
                    onClick={handleGenerateKey}
                    icon={<ReloadOutlined style={{ fontSize: 18 }} />}
                  />
                </div>
              </div>

              <Divider className='my-3' />

              <div className='flex items-center justify-between p-3 bg-gray-50 rounded-lg border border-gray-200'>
                <div className='font-semibold text-gray-800'>
                  Yêu cầu kiểm tra khóa xác thực (Enforce API key)
                </div>
                <Switch checked={requireApiKey} onChange={(checked) => setRequireApiKey(checked)} />
              </div>

              <div className='bg-blue-50 border border-blue-200 rounded p-3 text-13 text-gray-700'>
                <div className='font-semibold text-blue-800 mb-2'>Cấu hình HTTP Header</div>
                <div className='flex items-center gap-2'>
                  <div className='bg-white rounded px-3 py-2.5 border border-blue-200 font-mono text-12 flex-1 text-blue-900 font-semibold select-all min-h-[44px] flex items-center'>
                    {`X-IoT-Api-Key: ${apiKey || '<Chưa tạo khóa>'}`}
                  </div>
                  <ButtonCommonCustomizable
                    title={null}
                    tooltipText={_t('Sao chép dòng cấu hình')}
                    classButton='text-button-variant'
                    onClick={() => {
                      if (!apiKey) return;
                      navigator.clipboard.writeText(`X-IoT-Api-Key: ${apiKey}`);
                      notifySuccess('Đã sao chép dòng cấu hình Kepware Header!');
                    }}
                    disabled={!apiKey}
                    icon={<CopyOutlined style={{ fontSize: 18 }} />}
                  />
                </div>
              </div>
            </div>
          </Card>

          {/* CARD 3: IP WHITELIST MANAGEMENT */}
          <Card
            className='shadow-custom'
            title={
              <div className='flex items-center gap-2'>
                <ApiOutlined style={{ color: '#003366', fontSize: 18 }} />
                <span className='font-bold text-16-20 text-[#003366]'>
                  Danh sách IP được phép kết nối (IP Whitelist)
                </span>
              </div>
            }
          >
            <div className='space-y-4'>
              <div>
                <div className='text-14 font-medium text-gray-700 mb-2'>Thêm địa chỉ IP</div>
                <div className='flex gap-2 items-center'>
                  <Input
                    value={inputIp}
                    onChange={(e) => setInputIp(e.target.value)}
                    onPressEnter={handleAddIp}
                    placeholder='Ví dụ: 192.168.28.140 hoặc 10.0.0.12'
                    style={{ maxWidth: 350, height: 44 }}
                  />
                  <ButtonCommonCustomizable
                    title={null}
                    tooltipText={_t('Thêm IP')}
                    classButton='text-button-variant'
                    onClick={handleAddIp}
                    icon={<PlusOutlined style={{ fontSize: 18 }} />}
                  />
                </div>
              </div>

              <div>
                <div className='text-13 text-gray-600 mb-2'>Danh sách các IP được phép:</div>
                <div className='flex flex-wrap gap-2 min-h-[38px] p-2 bg-gray-50 rounded border border-gray-200 items-center'>
                  {allowedIpsList.length === 0 ? (
                    <span className='text-gray-400 italic text-13'>
                      Chưa có IP tùy biến nào. Mặc định hệ thống chấp thuận Loopback (127.0.0.1) và các máy trong mạng LAN nội bộ.
                    </span>
                  ) : (
                    allowedIpsList.map((ip) => (
                      <Tag
                        key={ip}
                        closable
                        onClose={() => handleRemoveIp(ip)}
                        color='blue'
                        className='text-13 py-0.5 px-2'
                      >
                        {ip}
                      </Tag>
                    ))
                  )}
                </div>
              </div>
            </div>
          </Card>
        </div>
      </SocketCommon>

      <FullPageLoading isLoading={loading} />
    </div>
  );
};

export default IoTConfigPage;

