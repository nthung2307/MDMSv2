import { useEffect, useState } from 'react';

import Constants from '@/core/application/common/constants';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse';
import { GetListGCSRequest } from '@/core/application/dto/gcs/request/GetListGCSRequest.ts';
import { CustomerSelector } from '@/infrastructure/common/components/customer-selector/CustomerSelector';
import InputNumberCalculateCommon from '@/infrastructure/common/components/input/input-number-calculate-common';
import InputSelectCommon from '@/infrastructure/common/components/input/input-select-common.tsx';
import InputTextAreaCommon from '@/infrastructure/common/components/input/input-text-area-common';
import InputTextCommon from '@/infrastructure/common/components/input/input-text-common';
import SwitchTextValue from '@/infrastructure/common/components/switch/switch-text-value';
import useIoTConfigHook, {
  MeterProfileItem,
} from '@/infrastructure/repository/config/hooks/useIoTConfigHook';
import useGetListGCSHook from '@/infrastructure/repository/gcs/hooks/useGetGCSList.hook';
import { _t } from '@/infrastructure/utils/translation';

interface IPointForm {
  data: any;
  setData: any;
  validate: any;
  setValidate: any;
  submittedTime: any;
  disabled?: boolean;
  userData?: any;
  statusUser: any;
  title?: string;
  editMode?: boolean;
}

const MAPPING_MODE_OPTIONS = [
  { label: 'HYBRID - Thủ công & Mã OBIS', value: 'HYBRID' },
  { label: 'AUTO - Mã OBIS', value: 'AUTO' },
  { label: 'MANUAL - Thủ công', value: 'MANUAL' },
];

const PointForm = (props: IPointForm) => {
  const {
    data,
    setData,
    validate,
    setValidate,
    submittedTime,
    disabled = false,
    statusUser,
    title = 'Thêm điểm đo',
    editMode = false,
  } = props;

  const [GCSCode, setGCSCode] = useState<any>();
  const [profileOptions, setProfileOptions] = useState<{ label: string; value: number }[]>([]);

  const [getListGCS] = useGetListGCSHook();
  const { getMeterProfiles } = useIoTConfigHook();

  const getListGCSAsync = async () => {
    const params: GetListGCSRequest = {
      pageSize: 500,
      searchByFields: [],
      sortByFields: [],
    };
    await getListGCS(
      params as any,
      (res: DataResponse) => {
        const { data } = res;
        const result = data.data;
        const listActive = result.filter((item: any) => item.status === 'ACTIVE');
        const listDeactive = result.filter((item: any) => item.status !== 'ACTIVE');
        setGCSCode([
          {
            label: <span>Đang mở</span>,
            title: 'Đang mở',
            options: listActive.map((item: any) => ({
              value: item.id,
              label: `${item.code} - ${item.name}`,
            })),
          },
          {
            label: <span>Đã đóng</span>,
            title: 'Đã đóng',
            options: listDeactive.map((item: any) => ({
              value: item.id,
              label: `${item.code} - ${item.name}`,
            })),
          },
        ]);
      },
      () => {},
      () => {}
    );
  };

  const loadProfilesAsync = async () => {
    await getMeterProfiles(
      (res: DataResponse) => {
        const profiles: MeterProfileItem[] = res?.data || [];
        setProfileOptions(
          profiles.map((p) => {
            let displayName = p.name;
            const lowerName = (p.name || '').toLowerCase();
            if (p.code === 'MV_KV_KW' || lowerName.includes('trung thế') || lowerName.includes('cao áp')) {
              displayName = 'Cao áp';
            } else if (p.code === 'LV_V_KW' || lowerName.includes('hạ thế') || lowerName.includes('hạ áp')) {
              displayName = 'Hạ áp';
            }
            return {
              value: p.id,
              label: `${displayName}${p.isDefault ? ' - Mặc định' : ''}`,
            };
          })
        );
      },
      () => {}
    );
  };

  const fakedata = {
    typePoint: [
      {
        label: '3 pha, 3 giá, 1 giao',
        value: '3 pha, 3 giá, 1 giao',
      },
    ],
  };

  useEffect(() => {
    getListGCSAsync();
    loadProfilesAsync();
  }, []);

  return (
    <div>
      <div className={'flex items-center justify-between'}>
        <h1 className='font-bold text-20-20'>{title}</h1>
        <div>
          <SwitchTextValue
            data={statusUser == Constants.userStatus.Add ? { status: 'DEACTIVATE' } : data}
            expectedData={[
              { key: true, value: Constants.AccountStatus.Active },
              { key: false, value: Constants.AccountStatus.Deactivate },
            ]}
            setData={setData}
            attribute={'status'}
            label={_t('Kích hoạt')}
            isRequired={false}
            reverseLabel={true}
            disabled={disabled}
          />
        </div>
      </div>

      <div className='mt-5 flex flex-col gap-5'>
        <div className='flex gap-2'>
          <div className='basis-1/2'>
            <InputTextCommon
              dataAttribute={data?.code}
              label={_t('Mã điểm đo')}
              attribute={'code'}
              validate={validate}
              setValidate={setValidate}
              submittedTime={submittedTime}
              setData={setData}
              isRequired={editMode}
              placeholder={'Nhập mã điểm đo'}
              disabled={disabled}
            />
          </div>
          <div className='basis-1/2'>
            <InputSelectCommon
              label={_t('Loại điểm đo')}
              attribute={'type'}
              validate={validate}
              setValidate={setValidate}
              submittedTime={submittedTime}
              setData={setData}
              dataAttribute={data?.type}
              isRequired={editMode}
              disabled={disabled}
              placeholder={'Nhập loại điểm đo'}
              listDataItem={fakedata.typePoint}
              allowClear={true}
            />
          </div>
        </div>

        <div className='flex gap-2'>
          <div className='basis-1/2'>
            <InputTextCommon
              dataAttribute={data?.meterSerial}
              label={'Số chế tạo (Serial number)'}
              attribute={'meterSerial'}
              validate={validate}
              setValidate={setValidate}
              submittedTime={submittedTime}
              setData={setData}
              isRequired={false}
              placeholder={'Ví dụ: 209555163 (định danh REST Client)'}
              disabled={disabled}
            />
          </div>
          <div className='basis-1/2'>
            <InputSelectCommon
              label={'Hồ sơ ánh xạ (Profile)'}
              attribute={'meterProfileId'}
              setData={setData}
              dataAttribute={data?.meterProfileId}
              isRequired={false}
              validate={validate}
              setValidate={setValidate}
              listDataItem={profileOptions}
              disabled={disabled}
              placeholder={'Mặc định: Hạ áp'}
              allowClear={true}
            />
          </div>
        </div>

        <div className='flex gap-2'>
          <div className='basis-1/2'>
            <InputSelectCommon
              label={'Chế độ ánh xạ (Mapping mode)'}
              attribute={'mappingMode'}
              setData={setData}
              dataAttribute={data?.mappingMode || 'HYBRID'}
              isRequired={false}
              validate={validate}
              setValidate={setValidate}
              listDataItem={MAPPING_MODE_OPTIONS}
              disabled={disabled}
              placeholder={'HYBRID'}
              allowClear={false}
            />
          </div>
          <div className='basis-1/2'>
            <InputNumberCalculateCommon
              label={_t('Hệ số nhân')}
              attribute={'multiplierFactor'}
              setData={setData}
              dataAttribute={data?.multiplierFactor}
              validate={validate}
              setValidate={setValidate}
              submittedTime={submittedTime}
              isRequired={editMode}
              placeholder={'Nhập hệ số nhân'}
              disabled={disabled}
              isInteger={false}
              rules={[
                {
                  pattern: Constants.PATTERN_REGEX.MinNumber,
                  message: 'Hệ số nhân phải lớn hơn hoặc bằng 1',
                },
              ]}
            />
          </div>
        </div>

        <div className='flex gap-2 items-start'>
          <div className='basis-1/2'>
            <InputSelectCommon
              label={'Sổ ghi chỉ số'}
              attribute={'gcsId'}
              setData={setData}
              dataAttribute={data?.gcsId}
              isRequired={editMode}
              validate={validate}
              setValidate={setValidate}
              listDataItem={GCSCode ?? []}
              disabled={disabled}
              placeholder={'Nhập sổ ghi chỉ số'}
              allowClear={true}
            />
          </div>
          <div className='basis-1/2'>
            <CustomerSelector
              label={'Mã khách hàng'}
              attribute={'customerId'}
              setData={setData}
              dataAttribute={data?.customerId}
              customerData={data?.customer}
              isRequired={editMode}
              validate={validate}
              setValidate={setValidate}
              submittedTime={submittedTime}
              disabled={disabled}
            />
          </div>
        </div>

        <InputTextCommon
          label={_t('Địa chỉ')}
          attribute={'address'}
          validate={validate}
          setValidate={setValidate}
          submittedTime={submittedTime}
          setData={setData}
          dataAttribute={data?.address}
          isRequired={false}
          placeholder={'Nhập địa chỉ'}
          disabled={disabled}
        />

        <InputTextAreaCommon
          label={_t('Mô tả')}
          attribute={'description'}
          validate={validate}
          setValidate={setValidate}
          submittedTime={submittedTime}
          setData={setData}
          dataAttribute={data?.description}
          isRequired={false}
          placeholder={'Nhập mô tả'}
          disabled={disabled}
        />
      </div>
    </div>
  );
};

export default PointForm;
