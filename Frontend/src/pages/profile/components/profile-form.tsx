import Constants from '@/core/application/common/constants.ts';
import { IAccountDetailResponseInterface } from '@/core/application/interfaces/response/account/accountResponse.interface.ts';
import InputNumberCommon from '@/infrastructure/common/components/input/input-number-common.tsx';
import InputSelectCommon from '@/infrastructure/common/components/input/input-select-common.tsx';
import InputTextCommon from '@/infrastructure/common/components/input/input-text-common.tsx';
import { getListUserRoles } from '@/infrastructure/utils/helpers.ts';
import { _t } from '@/infrastructure/utils/translation.ts';

interface IProfileForm {
  data: IAccountDetailResponseInterface;
  setData: any;
  validate: any;
  setValidate: any;
  submittedTime: any;
  disabled?: boolean;
  editMode?: boolean;
  viewMode?: boolean;
  title?: string;
}

const ProfileForm = (props: IProfileForm) => {
  const {
    data,
    setData,
    validate,
    setValidate,
    submittedTime,
    disabled = false,
    editMode = false,
    viewMode = false,
    title = 'Thông tin tài khoản',
  } = props;
  return (
    <div>
      <div className={'flex justify-between'}>
        <h1 className='font-bold text-20-20'>{title}</h1>
      </div>
      <div className='mt-5 flex flex-col gap-5'>
        {!editMode && (
          <div className='flex gap-3'>
            <div className={'w-full'}>
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
              />
            </div>
          </div>
        )}
        {!editMode && (
          <div>
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
            />
          </div>
        )}
        <div>
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
          />
        </div>
        <div>
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
          />
        </div>
        <div>
          <InputNumberCommon
            label={_t('Số điện thoại')}
            attribute={'phoneNumber'}
            dataAttribute={data?.phoneNumber}
            setData={setData}
            isRequired={false}
            validate={validate}
            setValidate={setValidate}
            disabled={disabled}
          />
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
          />
        </div>
      </div>
    </div>
  );
};

export default ProfileForm;
