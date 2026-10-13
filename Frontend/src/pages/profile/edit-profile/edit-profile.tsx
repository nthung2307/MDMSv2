import { Card } from 'antd';
import { useEffect, useState } from 'react';
import { MdPassword } from 'react-icons/md';
import { useNavigate } from 'react-router-dom';

import { ROUTE_PATH } from '@/core/application/common/appRouters.ts';
import { GetAccountDetailRequest } from '@/core/application/dto/account/request/GetAccountDetailRequest.ts';
import { ChangeInformationAccountRequest } from '@/core/application/dto/identity/auth/requests/ChangeInformationAccountRequest.ts';
import { ButtonCommonCustomizable } from '@/infrastructure/common/components/button/button-common-customizable';
import { ButtonCommon } from '@/infrastructure/common/components/button/button-common.tsx';
import { BreadcrumbUI } from '@/infrastructure/common/components/controls/BreadcrumbUI.tsx';
import DialogCommon from '@/infrastructure/common/components/popup/dialogCommon.tsx';
import PageTitle from '@/infrastructure/common/components/title/page-title.tsx';
import { notifySuccess } from '@/infrastructure/common/components/toast/toastMessage.tsx';
import { useStateDataForm } from '@/infrastructure/common/libs/hooks/useStateDataForm.hook.tsx';
import useGetAccountDetailHook from '@/infrastructure/repository/account/hooks/useGetAccountDetail.hook.tsx';
import useAccountId from '@/infrastructure/repository/auth/hooks/useAccountId.tsx';
import useUpdateProfileHook from '@/infrastructure/repository/auth/hooks/useUpdateProfileHook.tsx';
import { isValidData } from '@/infrastructure/utils/helpers.ts';
import { _t } from '@/infrastructure/utils/translation.ts';
import ChangePasswordModal from '@/pages/account-management/components/change-password-modal.tsx';
import ProfileForm from '@/pages/profile/components/profile-form.tsx';

const EditProfile = () => {
  const id = useAccountId();
  const [getAccountDetail] = useGetAccountDetailHook();
  const [updateProfileRequest] = useUpdateProfileHook();
  const navigate = useNavigate();
  const [dataForm, setDataForm] = useStateDataForm({});
  const [validate, setValidate] = useState<any>({});
  const [submittedTime, setSubmittedTime] = useState<Date | null>(null);
  const [loading, setLoading] = useState<boolean>(false);
  const [data, setData] = useState<any>();
  const [isShowDialog, setIsShowDialog] = useState<boolean>(false);
  const [isShowChangePassword, setIsShowChangePassword] = useState<boolean>(false);
  const isDisabled = !isValidData(validate);

  const getAccountDetailAsync = async () => {
    const params = new GetAccountDetailRequest({ id });
    await getAccountDetail(
      params,
      (res: any) => {
        setData(res?.data);
      },
      () => {},
      setLoading
    );
  };

  const updateProfileAsync = async () => {
    setSubmittedTime(new Date());

    if (!isValidData(validate)) return;

    const requestBody = new ChangeInformationAccountRequest({
      ...dataForm,
    });
    await updateProfileRequest(
      requestBody,
      () => {
        setIsShowDialog(!isShowDialog);
        notifySuccess('', _t('Cập nhật thông tin thành công'));
        navigate(ROUTE_PATH.USER_PROFILE);
      },
      () => {},
      setLoading
    );
  };

  useEffect(() => {
    if (id) {
      getAccountDetailAsync();
    }
  }, []);

  useEffect(() => {
    if (data) setDataForm(data);
  }, [data]);

  return (
    <div>
      <PageTitle title={'Thông tin tài khoản'} />
      <div className='flex justify-between items-center'>
        <BreadcrumbUI
          items={[
            { name: 'Thông tin tài khoản', link: ROUTE_PATH.USER_PROFILE },
            { name: 'Sửa thông tin tài khoản' },
          ]}
        />
        <div className='flex gap-3'>
          <ButtonCommonCustomizable
            icon={<MdPassword />}
            title={''}
            onClick={() => {
              setIsShowChangePassword(!isShowChangePassword);
            }}
            classButton={'text-button-variant'}
            tooltipText={_t('Đổi mật khẩu')}
          />
        </div>
      </div>
      <div className='mt-10'>
        <Card>
          <ProfileForm
            data={dataForm}
            setData={setDataForm}
            validate={validate}
            setValidate={setValidate}
            submittedTime={submittedTime}
            editMode={true}
            title={'Sửa thông tin tài khoản'}
          />
          <div className='flex flex-row gap-2 justify-center mt-5'>
            <ButtonCommon
              title={_t('Huỷ')}
              onClick={() => {
                navigate(ROUTE_PATH.USER_PROFILE);
              }}
              colorClass={'firebrick-outline-bg'}
            />
            <ButtonCommon
              title={_t('Lưu')}
              onClick={() => {
                setIsShowDialog(!isShowDialog);
              }}
              colorClass={'firebrick-bg'}
              disabled={isDisabled}
            />
          </div>
        </Card>
      </div>
      <DialogCommon
        title={''}
        visible={isShowDialog}
        handleCancel={() => {
          setIsShowDialog(!isShowDialog);
        }}
        handleConfirm={updateProfileAsync}
        isButtonSubmit={true}
        showFooter={true}
        buttonLoading={loading}
      >
        <div className='flex flex-col gap-4'>
          <div className='flex flex-col justify-center items-center my-5'>
            <p className='text-16-24 font-bold'>{_t('Xác nhận sửa thông tin tài khoản?')}</p>
            <p className=''>
              {_t('Sau khi xác nhận thông tin tài khoản sẽ thay đổi trên hệ thống.')}
            </p>
          </div>
        </div>
      </DialogCommon>
      <ChangePasswordModal
        title={_t('Đổi mật khẩu')}
        visible={isShowChangePassword}
        setVisible={setIsShowChangePassword}
        setLoading={setLoading}
        buttonLoading={loading}
      />
    </div>
  );
};

export default EditProfile;
