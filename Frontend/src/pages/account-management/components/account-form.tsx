import Constants from '@/core/application/common/constants';
import { IAccountDetailResponseInterface } from '@/core/application/interfaces/response/account/accountResponse.interface.ts';
import InputNumberCommon from '@/infrastructure/common/components/input/input-number-common.tsx';
import InputSelectCommon from '@/infrastructure/common/components/input/input-select-common';
import InputTextCommon from '@/infrastructure/common/components/input/input-text-common';
import SwitchTextValue from '@/infrastructure/common/components/switch/switch-text-value.tsx';
import { getListUserRoles } from '@/infrastructure/utils/helpers.ts';
import { _t } from '@/infrastructure/utils/translation';

interface IAccountForm {
  data: IAccountDetailResponseInterface;
  setData: any;
  validate: any;
  setValidate: any;
  submittedTime: any;
  disabled?: boolean;
  editMode?: boolean;
  title?: string;
  viewMode?: boolean;
}

const AccountForm = (props: IAccountForm) => {
  const {
    data,
    setData,
    validate,
    setValidate,
    submittedTime,
    disabled = false,
    editMode = false,
    title = 'Thông tin tài khoản',
    viewMode = false,
  } = props;

  return (
    <div>
      <div className={'flex justify-between'}>
        <h1 className='font-bold text-20-20'>{title}</h1>
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
      </div>
      <div className='mt-5 flex flex-col gap-5'>
        <div className='flex gap-3'>
          {!editMode && (
            <InputTextCommon
              dataAttribute={data?.userName}
              label={_t('Tên đăng nhập')}
              attribute={'userName'}
              validate={validate}
              setValidate={setValidate}
              submittedTime={submittedTime}
              setData={setData}
              isRequired={!viewMode}
              disabled={disabled}
              placeholder={_t('Nhập tên đăng nhập')}
            />
          )}
        </div>
        <div className='flex gap-3'>
          <div className='basis-1/2'>
            <InputTextCommon
              label={_t('Họ và tên')}
              attribute={'fullName'}
              setData={setData}
              dataAttribute={data?.fullName}
              isRequired={false}
              validate={validate}
              setValidate={setValidate}
              submittedTime={submittedTime}
              rules={[]}
              disabled={disabled}
              placeholder={_t('Nhập họ và tên')}
            />
          </div>
          <div className='basis-1/2'>
            <InputSelectCommon
              label={_t('Nhóm người dùng')}
              attribute={'roleName'}
              setData={setData}
              dataAttribute={data?.roleName}
              isRequired={!viewMode}
              validate={validate}
              setValidate={setValidate}
              submittedTime={submittedTime}
              listDataItem={getListUserRoles().filter(
                (item) => item.value !== Constants.Role.SuperAdmin.value
              )}
              disabled={disabled}
              placeholder={_t('Chọn nhóm người dùng')}
            />
          </div>
        </div>
        <div className='flex gap-3'>
          <div className='basis-1/2'>
            <InputTextCommon
              label={_t('Email')}
              attribute={'email'}
              validate={validate}
              setValidate={setValidate}
              submittedTime={submittedTime}
              setData={setData}
              dataAttribute={data?.email}
              isRequired={!viewMode}
              rules={[
                {
                  pattern: Constants.PATTERN_REGEX.Email,
                  message: 'Email không đúng định dạng',
                },
              ]}
              disabled={disabled}
              placeholder={_t('Nhập email')}
            />
          </div>
          <div className='basis-1/2'>
            <InputNumberCommon
              label={_t('Số điện thoại')}
              attribute={'phoneNumber'}
              dataAttribute={data?.phoneNumber}
              setData={setData}
              isRequired={false}
              validate={validate}
              setValidate={setValidate}
              disabled={disabled}
              placeholder={_t('Nhập số điện thoại')}
            />
          </div>
        </div>
        <div>
          <InputTextCommon
            label={_t('Mô tả')}
            attribute={'description'}
            dataAttribute={data?.description}
            setData={setData}
            isRequired={false}
            validate={validate}
            setValidate={setValidate}
            disabled={disabled}
            placeholder={_t('Nhập mô tả')}
          />
        </div>
      </div>
    </div>
  );
};

export default AccountForm;
