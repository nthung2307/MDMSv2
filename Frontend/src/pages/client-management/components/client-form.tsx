import Constants from '@/core/application/common/constants';
import InputNumberCommon from '@/infrastructure/common/components/input/input-number-common.tsx';
import InputSelectCommon from '@/infrastructure/common/components/input/input-select-common.tsx';
import InputTextCommon from '@/infrastructure/common/components/input/input-text-common';
import SwitchTextValue from '@/infrastructure/common/components/switch/switch-text-value';
import { _t } from '@/infrastructure/utils/translation';
import TableClientPoint from '@/pages/client-management/components/table-client-point';

interface IClientForm {
  data: any;
  setData: any;
  validate: any;
  setValidate: any;
  submittedTime: any;
  disabled?: boolean;
  accountData?: any;
  onSearch?: (value: string) => void;
  createMode?: boolean;
  title?: string;
}

const ClientForm = (props: IClientForm) => {
  const {
    data,
    setData,
    validate,
    setValidate,
    submittedTime,
    disabled = false,
    accountData,
    onSearch,
    title = 'Thông tin khách hàng',
    createMode = false,
  } = props;
  return (
    <div>
      <div className={'flex items-center justify-between'}>
        <h1 className='font-bold text-20-20'>{title}</h1>
        {createMode ? (
          <div>
            <SwitchTextValue
              data={{ status: Constants.AccountStatus.Deactivate }}
              expectedData={[
                { key: true, value: Constants.AccountStatus.Active },
                { key: false, value: Constants.AccountStatus.Deactivate },
              ]}
              setData={setData}
              attribute={'status'}
              label={_t('Kích hoạt')}
              isRequired={false}
              reverseLabel={true}
              disabled={false}
            />
          </div>
        ) : (
          <div>
            <SwitchTextValue
              data={data}
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
        )}
      </div>
      <div className='mt-5 flex flex-col gap-5'>
        <div className='flex gap-3'>
          <div className='basis-1/2'>
            <InputTextCommon
              dataAttribute={data?.fullName}
              label={_t('Tên khách hàng')}
              placeholder={_t('Nhập tên khách hàng')}
              attribute={'fullName'}
              validate={validate}
              setValidate={setValidate}
              submittedTime={submittedTime}
              setData={setData}
              isRequired={true}
              disabled={disabled}
            />
          </div>
          <div className='basis-1/2'>
            <InputTextCommon
              label={_t('Mã khách hàng')}
              attribute={'customerCode'}
              placeholder={_t('Nhập mã khách hàng')}
              setData={setData}
              dataAttribute={data?.customerCode}
              validate={validate}
              setValidate={setValidate}
              submittedTime={submittedTime}
              isRequired={true}
              disabled={disabled}
            />
          </div>
        </div>
        <div className='flex gap-2'>
          <div className='basis-1/2'>
            <InputNumberCommon
              label={_t('Điện thoại')}
              attribute={'phoneNumber'}
              placeholder={_t('Nhập số điện thoại')}
              setData={setData}
              dataAttribute={data?.phoneNumber}
              isRequired={false}
              validate={validate}
              setValidate={setValidate}
              submittedTime={submittedTime}
              disabled={disabled}
            />
          </div>
          <div className='basis-1/2'>
            <InputTextCommon
              label={_t('Email')}
              attribute={'email'}
              placeholder={_t('Nhập email')}
              validate={validate}
              setValidate={setValidate}
              submittedTime={submittedTime}
              setData={setData}
              dataAttribute={data?.email}
              isRequired={true}
              rules={[
                {
                  pattern: Constants.PATTERN_REGEX.Email,
                  message: 'Email không đúng định dạng',
                },
              ]}
              disabled={disabled}
            />
          </div>
        </div>
        <div>
          <InputTextCommon
            label={_t('Địa chỉ')}
            attribute={'address'}
            placeholder={_t('Nhập địa chỉ')}
            setData={setData}
            dataAttribute={data?.address}
            validate={validate}
            setValidate={setValidate}
            submittedTime={submittedTime}
            isRequired={false}
            disabled={disabled}
          />
        </div>

        <div>
          <InputSelectCommon
            dataAttribute={data?.userIds}
            label={'Liên kết tài khoản'}
            attribute={'userIds'}
            setData={setData}
            isRequired={false}
            listDataItem={accountData}
            mode={'multiple'}
            onSearch={onSearch}
            disabled={disabled}
          />
        </div>
        {!createMode && (
          <div>
            <p className='font-bold font-14-20 text-[#3B3B3B] mb-3'>Danh sách điểm đo</p>
            <TableClientPoint
              dataSource={data.measurePoints ?? []}
              setData={setData}
              data={data}
              disabled={disabled}
              validate={validate}
              setValidate={setValidate}
            />
          </div>
        )}
      </div>
    </div>
  );
};
export default ClientForm;
