import {
  CopyOutlined,
  DeleteOutlined,
  EyeInvisibleOutlined,
  EyeOutlined,
  KeyOutlined,
  ReloadOutlined,
} from '@ant-design/icons';
import { Alert, Button, Card, Input, Popconfirm, Tag, Tooltip } from 'antd';
import { useEffect, useState } from 'react';
import { FiDownload, FiShare } from 'react-icons/fi';
import { LuSave } from 'react-icons/lu';
import { useLocation, useParams } from 'react-router-dom';

import { ROUTE_PATH } from '@/core/application/common/appRouters';
import Constants from '@/core/application/common/constants';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse';
import { tagNameListRequest } from '@/core/application/dto/tag-name/requests/TagNameListRequest';
import { tagNameRequest } from '@/core/application/dto/tag-name/requests/TagNameRequest';
import { ButtonCommonCustomizable } from '@/infrastructure/common/components/button/button-common-customizable.tsx';
import { BreadcrumbUI } from '@/infrastructure/common/components/controls/BreadcrumbUI';
import PageTitle from '@/infrastructure/common/components/title/page-title.tsx';
import { notifyError, notifySuccess } from '@/infrastructure/common/components/toast/toastMessage';
import useCreateTagNameHook from '@/infrastructure/repository/config/hooks/useCreateTagName.hook';
import useGetTagNameListHook from '@/infrastructure/repository/config/hooks/useGetListTagName.hook';
import useIoTConfigHook, {
  MeterProfileItem,
  MeterProfileTagMappingItem,
  PointApiKeyInfo,
} from '@/infrastructure/repository/config/hooks/useIoTConfigHook';
import useGetMeasurementPointHook from '@/infrastructure/repository/measurement-point/hooks/useGetMeasurementPoint.hook';
import { getAccountRole, copyToClipboard } from '@/infrastructure/utils/helpers';
import { _t } from '@/infrastructure/utils/translation';
import { downloadExcelTemplate } from '@/infrastructure/utils/excelHelper';
import DialogImportConfig from '@/pages/config/components/dialog-import-config';
import TableTagNameManagement from '@/pages/config/components/table-tag-name';

const DataMeasurementPoint = () => {
  const { id } = useParams();
  const location = useLocation();
  const searchParams = new URLSearchParams(location.search);
  const queryCode = searchParams.get('code');
  const record = location.state?.record || { code: queryCode || '' };

  const [getTagNameList] = useGetTagNameListHook();
  const [createTagNameList] = useCreateTagNameHook();
  const [getMeasurementPointDetail] = useGetMeasurementPointHook();
  const { getIoTConfig, getMeterProfiles, getPointApiKey, generatePointApiKey, revokePointApiKey } =
    useIoTConfigHook();
  const [loading, setLoading] = useState<boolean>(false);
  const [listTagName, setListTagName] = useState<any>();
  const [pointDetail, setPointDetail] = useState<any>(null);
  const [meterProfiles, setMeterProfiles] = useState<MeterProfileItem[]>([]);
  const [typeTable, setTypeTable] = useState<any>(Constants.TYPE_TABLE.operatingParams.value);
  const [isShowImport, setIsShowImport] = useState<boolean>(false);

  // Private key & REST client telemetry state
  const [keyInfo, setKeyInfo] = useState<PointApiKeyInfo | null>(null);
  const [revealedKey, setRevealedKey] = useState<string>('');
  const [showKey, setShowKey] = useState<boolean>(false);
  const [keyLoading, setKeyLoading] = useState<boolean>(false);
  const [globalIoTConfig, setGlobalIoTConfig] = useState<any>(null);

  const isAdminOrSuperAdmin = [
    Constants.Role.Admin.value,
    Constants.Role.SuperAdmin.value,
  ].includes(getAccountRole());

  const activeProfile =
    meterProfiles.find((p) => p.id === pointDetail?.meterProfileId) ||
    meterProfiles.find((p) => p.isDefault) ||
    meterProfiles[0];
  const profileMappings: MeterProfileTagMappingItem[] = activeProfile?.tagMappings || [];

  const getProfileDisplayName = (profile?: MeterProfileItem) => {
    if (!profile) return 'Hạ áp';
    const lowerName = (profile.name || '').toLowerCase();
    if (profile.code === 'MV_KV_KW' || lowerName.includes('trung thế') || lowerName.includes('cao áp')) {
      return 'Cao áp';
    }
    return 'Hạ áp';
  };

  const getMappingModeDisplay = (mode?: string) => {
    switch (mode) {
      case 'AUTO':
        return 'AUTO - Mã OBIS';
      case 'MANUAL':
        return 'MANUAL - Thủ công';
      case 'HYBRID':
      default:
        return 'HYBRID - Thủ công & Mã OBIS';
    }
  };

  const getTagNameListAsync = async () => {
    const params: tagNameListRequest = {
      measurementPoint: Number(id),
    };
    await getTagNameList(
      params,
      (res: DataResponse) => {
        const { data } = res;
        const dataUpdate = Constants.dataMeasurementPoint.allDataParams.map((param) => {
          const nowData = data.find((item: any) => item.description == param.value);
          const fixedUnit = param.unit && param.unit.length === 1 ? param.unit[0].value : null;
          const assignedUnit =
            fixedUnit ||
            (nowData?.unit
              ? nowData.unit
              : param.unit && param.unit.length > 0
                ? param.unit[0].value
                : '');

          return {
            description: param.value,
            tagName: nowData?.tagName || '',
            unit: assignedUnit,
            customName: nowData?.customName || '',
            type: param.type,
          };
        });
        setListTagName(dataUpdate);
      },
      () => {},
      setLoading
    );
  };

  useEffect(() => {
    if (id) {
      getMeasurementPointDetail(
        { id: Number(id) },
        (res: DataResponse) => setPointDetail(res?.data || null),
        () => {}
      );
    }
    getMeterProfiles(
      (res: DataResponse) => setMeterProfiles(res?.data || []),
      () => {}
    );
  }, [id]);

  const loadPointKeyMetadataAsync = async (pointId: number) => {
    await getPointApiKey(
      pointId,
      false,
      (res: DataResponse) => {
        setKeyInfo(res?.data || null);
        setRevealedKey('');
        setShowKey(false);
      },
      () => {},
      setKeyLoading
    );
  };

  const handleToggleRevealKey = async () => {
    if (!id) return;
    if (showKey) {
      setShowKey(false);
      return;
    }
    if (revealedKey) {
      setShowKey(true);
      return;
    }
    await getPointApiKey(
      Number(id),
      true,
      (res: DataResponse) => {
        const info = res?.data as PointApiKeyInfo;
        setKeyInfo(info);
        setRevealedKey(info?.apiKey || '');
        setShowKey(Boolean(info?.apiKey));
      },
      () => notifyError('', 'Không thể giải mã mã xác thực'),
      setKeyLoading
    );
  };

  const handleCopyPointKey = async () => {
    if (!id) return;
    let keyToCopy = revealedKey;
    if (!keyToCopy) {
      await getPointApiKey(
        Number(id),
        true,
        (res: DataResponse) => {
          const info = res?.data as PointApiKeyInfo;
          setKeyInfo(info);
          keyToCopy = info?.apiKey || '';
          setRevealedKey(keyToCopy);
        },
        () => notifyError('', 'Không thể lấy mã xác thực'),
        setKeyLoading
      );
    }
    if (keyToCopy) {
      const ok = await copyToClipboard(keyToCopy);
      if (ok) {
        notifySuccess('', 'Đã sao chép mã xác thực (Private key) của điểm đo!');
      } else {
        notifyError('', 'Không thể sao chép vào bộ nhớ tạm');
      }
    }
  };

  const handleGenerateDedicatedKey = async () => {
    if (!id) return;
    await generatePointApiKey(
      Number(id),
      (res: DataResponse) => {
        const info = res?.data as PointApiKeyInfo;
        setKeyInfo(info);
        setRevealedKey(info?.apiKey || '');
        setShowKey(true);
        notifySuccess('', 'Đã tạo mã xác thực riêng cho điểm đo!');
      },
      () => notifyError('', 'Không thể tạo mã xác thực riêng'),
      setKeyLoading
    );
  };

  const handleRevokeDedicatedKey = async () => {
    if (!id) return;
    await revokePointApiKey(
      Number(id),
      (res: DataResponse) => {
        const info = res?.data as PointApiKeyInfo;
        setKeyInfo(info);
        setRevealedKey('');
        setShowKey(false);
        notifySuccess('', 'Đã thu hồi khóa riêng (chuyển về dùng Global key).');
      },
      () => notifyError('', 'Không thể thu hồi mã xác thực riêng'),
      setKeyLoading
    );
  };

  const handleCopyHeaderBlock = async () => {
    const serialVal = pointDetail?.meterSerial || '<Số chế tạo>';
    let keyHeaderLine = '';

    if (hasDedicatedKey) {
      let keyVal = revealedKey;
      if (!keyVal && id) {
        await getPointApiKey(
          Number(id),
          true,
          (res: DataResponse) => {
            const info = res?.data as PointApiKeyInfo;
            setKeyInfo(info);
            keyVal = info?.apiKey || '';
            setRevealedKey(keyVal);
          },
          () => {},
          setKeyLoading
        );
      }
      keyHeaderLine = `Private-key: ${keyVal || displayKeyValue || '<Private key riêng của điểm đo>'}`;
    } else {
      let globalKey = globalIoTConfig?.apiKey || '';
      if (!globalKey) {
        await getIoTConfig(
          (res: DataResponse) => {
            const cfg = res?.data;
            if (cfg?.apiKey) {
              globalKey = cfg.apiKey;
              setGlobalIoTConfig(cfg);
            }
          },
          () => {}
        );
      }
      keyHeaderLine = `Global-key: ${globalKey || '<Global key tại Cài đặt REST Server>'}`;
    }

    const text = `Serial: ${serialVal}\n${keyHeaderLine}`;
    const ok = await copyToClipboard(text);
    if (ok) {
      notifySuccess('', 'Đã sao chép cấu hình HTTP Header!');
    } else {
      notifyError('', 'Không thể sao chép vào bộ nhớ tạm');
    }
  };

  useEffect(() => {
    if (id && isAdminOrSuperAdmin) {
      void loadPointKeyMetadataAsync(Number(id));
      void getIoTConfig(
        (res: DataResponse) => {
          setGlobalIoTConfig(res?.data);
        },
        () => {}
      );
    }
  }, [id, isAdminOrSuperAdmin]);

  const hasDedicatedKey = Boolean(keyInfo?.hasDedicatedKey);
  const displayKeyValue = showKey && revealedKey ? revealedKey : keyInfo?.keyPrefix || '';

  const createTagNameAsync = async () => {
    const params: tagNameRequest = {
      tagNames: listTagName,
      measurementPointId: Number(id),
    };
    await createTagNameList(
      params,
      () => {
        notifySuccess('', _t('Cập nhật dữ liệu thành công'));
      },
      () => {},
      setLoading
    );
  };

  const handleExportCsv = async () => {
    if (!listTagName || listTagName.length === 0) {
      notifyError('', 'Chưa có dữ liệu để xuất');
      return;
    }

    const getGroupName = (type: string) => {
      switch (type) {
        case 'ELECTRICAL_PARAMETERS':
          return 'Thông số vận hành';
        case 'OPERATING_PARAMETERS':
          return 'Thông số điện năng';
        case 'WARNING':
          return 'Cảnh báo';
        case 'TIME':
        case 'DATETIME':
          return 'Thời gian';
        default:
          return 'Khác';
      }
    };

    try {
      const rows = Constants.dataMeasurementPoint.allDataParams.map((param: any, idx: number) => {
        const item = listTagName.find((t: any) => t.description === param.value) || {};
        return {
          stt: idx + 1,
          groupName: getGroupName(param.type),
          label: param.label || '',
          code: param.value || '',
          tagId: item.tagName || '',
          unit: item.unit || (param.unit && param.unit.length > 0 ? param.unit[0].value : ''),
          customName: item.customName || '',
        };
      });

      const pointCode = record?.code || id;
      await downloadExcelTemplate(
        `LienKetDuLieu_${pointCode}.xlsx`,
        'LienKetDuLieu',
        [
          { header: 'STT', key: 'stt', width: 8 },
          { header: 'Nhóm thông số', key: 'groupName', width: 22 },
          { header: 'Tên thông số', key: 'label', width: 26 },
          { header: 'Mã tham số', key: 'code', width: 20 },
          { header: 'Tag ID', key: 'tagId', width: 26 },
          { header: 'Đơn vị', key: 'unit', width: 14 },
          { header: 'Tên cảnh báo', key: 'customName', width: 26 },
        ],
        rows
      );
      notifySuccess('', _t('Xuất file Excel thành công'));
    } catch (err: any) {
      notifyError(err, 'Lỗi khi xuất file Excel');
    }
  };

  useEffect(() => {
    getTagNameListAsync();
  }, []);

  return (
    <div>
      <PageTitle title={'Cài đặt hệ thống'} />
      <div className='flex justify-between items-center'>
        <BreadcrumbUI
          items={[
            { name: 'Cài đặt hệ thống' },
            { name: 'Liên kết dữ liệu', link: ROUTE_PATH.CONFIGURATION },
            { name: record.code },
          ]}
        />
        {typeTable !== Constants.TYPE_TABLE.authKeyParams.value && (
          <div className='flex gap-2 items-center'>
            <ButtonCommonCustomizable
              title={''}
              classButton='text-button-variant'
              tooltipText={'Xuất Excel'}
              onClick={handleExportCsv}
              icon={<FiShare size={20} />}
            />
            <ButtonCommonCustomizable
              title={''}
              classButton='text-button-variant'
              tooltipText={'Nhập từ Excel'}
              onClick={() => setIsShowImport(true)}
              icon={<FiDownload size={20} />}
            />
            <ButtonCommonCustomizable
              title={''}
              classButton='text-button-variant'
              tooltipText={'Lưu'}
              disabled={loading}
              onClick={createTagNameAsync}
              icon={<LuSave size={20} />}
            />
          </div>
        )}
      </div>
      <Card className='mt-5'>
        <Alert
          type='info'
          showIcon
          className='mb-4'
          message={
            <div className='flex flex-wrap items-center gap-3 text-13'>
              <span>
                <strong>Số chế tạo:</strong>{' '}
                <Tag color={pointDetail?.meterSerial ? 'blue' : 'default'} className='font-mono'>
                  {pointDetail?.meterSerial || 'Chưa gán'}
                </Tag>
              </span>
              <span>
                <strong>Hồ sơ ánh xạ:</strong>{' '}
                <Tag color='cyan'>{getProfileDisplayName(activeProfile)}</Tag>
              </span>
              <span>
                <strong>Chế độ ánh xạ:</strong>{' '}
                <Tag color='purple'>{getMappingModeDisplay(pointDetail?.mappingMode)}</Tag>
              </span>
            </div>
          }
        />

        <div className='flex border-b-slate-200 border-b-[1px] mb-4 overflow-x-auto'>
          {Constants.TYPE_TABLE.typeArray.map((item, index) => (
            <button
              key={index}
              type='button'
              className={`px-4 py-3 cursor-pointer text-16-20 font-semibold whitespace-nowrap transition-colors ${item.value === typeTable ? 'text-[#E98D8D] border-[#E98D8D] border-b-2' : 'text-gray-600 hover:text-gray-900'}`}
              onClick={() => {
                setTypeTable(item.value);
                if (
                  item.value === Constants.TYPE_TABLE.authKeyParams.value &&
                  id &&
                  isAdminOrSuperAdmin
                ) {
                  void loadPointKeyMetadataAsync(Number(id));
                }
              }}
            >
              <p>{item.label}</p>
            </button>
          ))}
        </div>

        {typeTable === Constants.TYPE_TABLE.authKeyParams.value ? (
          <div className='p-5 rounded-xl border border-blue-200 bg-gradient-to-br from-blue-50/40 to-white space-y-5'>
            <div className='flex flex-wrap items-center justify-between gap-3 border-b border-blue-100 pb-3'>
              <div className='flex items-center gap-2.5 font-bold text-16 text-[#003366]'>
                <KeyOutlined className='text-blue-600' />
                <span>Mã xác thực IoT Gateway (Private key)</span>
              </div>
              <div>
                {hasDedicatedKey ? (
                  <Tag color='green' className='px-2.5 py-0.5 text-12 font-medium'>
                    Khóa riêng điểm đo ({keyInfo?.keyPrefix})
                  </Tag>
                ) : (
                  <Tag color='orange' className='px-2.5 py-0.5 text-12 font-medium'>
                    Đang dùng Global key mặc định
                  </Tag>
                )}
              </div>
            </div>

            {!isAdminOrSuperAdmin && (
              <div className='text-13 text-gray-600'>
                <Alert
                  type='warning'
                  showIcon
                  message='Chỉ SuperAdmin mới có quyền tạo mới hoặc thu hồi mã xác thực riêng cho điểm đo.'
                />
              </div>
            )}

            {isAdminOrSuperAdmin && (
              <div className='space-y-2'>
                <label className='font-semibold text-13 text-gray-700 block'>
                  Khóa xác thực (Private key):
                </label>
                <div className='flex flex-wrap items-center gap-2'>
                  <Input
                    readOnly
                    className='font-mono bg-white text-14'
                    style={{ maxWidth: 480, height: 40 }}
                    type={showKey ? 'text' : 'password'}
                    value={
                      hasDedicatedKey
                        ? showKey && revealedKey
                          ? revealedKey
                          : displayKeyValue || '••••••••••••••••'
                        : ''
                    }
                    placeholder={
                      hasDedicatedKey
                        ? 'Nhấn biểu tượng mắt để xem mã xác thực'
                        : 'Chưa cấp mã xác thực riêng — đang dùng Global key tại Cài đặt REST Server'
                    }
                    suffix={
                      hasDedicatedKey ? (
                        <Button
                          type='text'
                          size='small'
                          loading={keyLoading}
                          icon={showKey ? <EyeInvisibleOutlined /> : <EyeOutlined />}
                          onClick={handleToggleRevealKey}
                          title={showKey ? 'Ẩn khóa' : 'Hiện khóa đầy đủ'}
                        />
                      ) : null
                    }
                  />

                  {hasDedicatedKey && (
                    <Tooltip title='Sao chép mã xác thực'>
                      <Button
                        icon={<CopyOutlined />}
                        loading={keyLoading}
                        onClick={handleCopyPointKey}
                      />
                    </Tooltip>
                  )}

                  <Popconfirm
                    title={
                      hasDedicatedKey
                        ? 'Tạo mới sẽ vô hiệu hóa khóa riêng hiện tại của điểm đo. Tiếp tục?'
                        : 'Cấp mã xác thực riêng cho điểm đo này?'
                    }
                    okText='Đồng ý'
                    cancelText='Hủy'
                    onConfirm={handleGenerateDedicatedKey}
                  >
                    <Tooltip title={hasDedicatedKey ? 'Đổi mã xác thực mới' : 'Tạo mã xác thực riêng'}>
                      <Button type='primary' ghost icon={<ReloadOutlined />} loading={keyLoading} />
                    </Tooltip>
                  </Popconfirm>

                  {hasDedicatedKey && (
                    <Popconfirm
                      title='Thu hồi mã xác thực riêng? Điểm đo sẽ chuyển về dùng Global key. Tiếp tục?'
                      okText='Thu hồi'
                      cancelText='Hủy'
                      onConfirm={handleRevokeDedicatedKey}
                    >
                      <Tooltip title='Thu hồi mã xác thực riêng'>
                        <Button danger icon={<DeleteOutlined />} loading={keyLoading} />
                      </Tooltip>
                    </Popconfirm>
                  )}
                </div>
              </div>
            )}

            <div className='bg-white border border-blue-200 rounded-lg p-4 space-y-3 mt-4'>
              <div>
                <span className='font-bold text-gray-800 text-13'>Endpoint:</span>{' '}
                <span className='font-mono text-blue-700 font-semibold select-all'>
                  http://&lt;host's IP&gt;:&lt;port&gt;/IoT/telemetry
                </span>
              </div>
              <div className='border-t border-blue-100 pt-3'>
                <div className='flex items-center justify-between mb-2'>
                  <div className='font-semibold text-gray-700 text-13'>HTTP Header:</div>
                  <ButtonCommonCustomizable
                    title={null}
                    tooltipText={_t('Sao chép HTTP Header')}
                    classButton='text-button-variant'
                    onClick={handleCopyHeaderBlock}
                    icon={<CopyOutlined style={{ fontSize: 16 }} />}
                  />
                </div>
                <div className='bg-gray-50 p-3 rounded border border-gray-200 font-mono text-12 space-y-1 select-all text-blue-900'>
                  <div>Serial: {pointDetail?.meterSerial || '<Số chế tạo>'}</div>
                  <div>
                    {hasDedicatedKey ? 'Private-key' : 'Global-key'}:{' '}
                    {hasDedicatedKey
                      ? showKey && revealedKey
                        ? revealedKey
                        : displayKeyValue || '<Private key riêng của điểm đo>'
                      : globalIoTConfig?.apiKey || '<Global key tại Cài đặt REST Server>'}
                  </div>
                </div>
              </div>
            </div>
          </div>
        ) : (
          <div className={'flex-1'}>
            <TableTagNameManagement
              listTagName={listTagName}
              setListTagName={setListTagName}
              typeTable={typeTable}
              loading={loading}
              mappingMode={pointDetail?.mappingMode || 'HYBRID'}
              profileMappings={profileMappings}
            />
          </div>
        )}
      </Card>
      <DialogImportConfig
        open={isShowImport}
        handleCancel={() => setIsShowImport(false)}
        onSuccess={(updatedList) => {
          setListTagName(updatedList);
        }}
        currentListTagName={listTagName || []}
        pointCode={record?.code || id || ''}
      />
    </div>
  );
};

export default DataMeasurementPoint;
