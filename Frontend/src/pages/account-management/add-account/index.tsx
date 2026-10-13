import { Card } from 'antd';
import { useEffect, useState } from 'react';
import { IoIosWarning } from 'react-icons/io';
import { useNavigate } from 'react-router-dom';

import { ROUTE_PATH } from '@/core/application/common/appRouters.ts';
import Constants from '@/core/application/common/constants';
import { CreateUserRequest } from '@/core/application/dto/account/request/CreateUserRequest';
import { ButtonCommon } from '@/infrastructure/common/components/button/button-common';
import { BreadcrumbUI } from '@/infrastructure/common/components/controls/BreadcrumbUI.tsx';
import DialogCommon from '@/infrastructure/common/components/popup/dialogCommon.tsx';
import PageTitle from '@/infrastructure/common/components/title/page-title.tsx';
import { notifySuccess } from '@/infrastructure/common/components/toast/toastMessage';
import { useStateDataForm } from '@/infrastructure/common/libs/hooks/useStateDataForm.hook.tsx';
import useCreateAccountHook from '@/infrastructure/repository/account/hooks/useCreateAccount.hook';
import { isFormFieldsEmpty, isValidData } from '@/infrastructure/utils/helpers.ts';
import { _t } from '@/infrastructure/utils/translation.ts';
import AccountForm from '@/pages/account-management/components/account-form.tsx';

const AddAccount = () => {
  const [createAccountRequest] = useCreateAccountHook();
  const navigate = useNavigate();
  const [loading, setLoading] = useState<boolean>(false);
  const [dataForm, setDataForm] = useStateDataForm({});
  const [validate, setValidate] = useState<any>({});
  const [submittedTime] = useState<Date | null>(null);
  const [isShowDialog, setIsShowDialog] = useState<boolean>(false);
  const isDisabled =
    !isValidData(validate) || isFormFieldsEmpty(['email', 'userName', 'roleName'], dataForm);

  const createAccountAsync = async () => {
    if (!isValidData(validate)) return;

    const requestBody = new CreateUserRequest({
      ...dataForm,
      email: dataForm?.email || '',
      status:
        dataForm?.status === Constants.AccountStatus.Active
          ? Constants.AccountStatus.Active
          : Constants.AccountStatus.Deactivate || Constants.AccountStatus.Deactivate,
    });

    await createAccountRequest(
      requestBody,
      () => {
        setIsShowDialog(!isShowDialog);
        notifySuccess('', _t('Thêm tài khoản thành công'));
        setDataForm({});
      },
      () => {
        setIsShowDialog(!isShowDialog);
      },
      setLoading
    );
  };

  useEffect(() => {
    if (dataForm?.status === undefined) {
      setDataForm({ ...dataForm, status: Constants.AccountStatus.Deactivate });
    }
  }, []);

  return (
    <div>
      <PageTitle title={'Quản lý tài khoản'} />
      <BreadcrumbUI
        items={[
          { name: 'Quản lý tài khoản', link: ROUTE_PATH.ACCOUNT_MANAGEMENT },
          { name: 'Tạo tài khoản' },
        ]}
      />
      <div className='mt-10'>
        <Card className={'shadow-custom'}>
          <div className='flex flex-col gap-4'>
            <AccountForm
              data={dataForm}
              setData={setDataForm}
              validate={validate}
              setValidate={setValidate}
              submittedTime={submittedTime}
              title={'Tạo tài khoản'}
            />
            <div className='flex flex-row gap-2 justify-center'>
              <ButtonCommon
                title={_t('Huỷ')}
                onClick={() => {
                  navigate(ROUTE_PATH.ACCOUNT_MANAGEMENT);
                }}
                classButton={'w-14'}
                colorClass={'firebrick-outline-bg'}
              />
              <ButtonCommon
                title={_t('Lưu')}
                onClick={() => {
                  setIsShowDialog(!isShowDialog);
                }}
                disabled={isDisabled}
                classButton={'w-14'}
                colorClass={'firebrick-bg'}
              />
            </div>
          </div>
        </Card>
      </div>
      <DialogCommon
        title={''}
        visible={isShowDialog}
        handleCancel={() => {
          setIsShowDialog(!isShowDialog);
        }}
        handleConfirm={createAccountAsync}
        isButtonSubmit={true}
        showFooter={true}
        buttonLoading={loading}
      >
        <div className='flex flex-col gap-4'>
          <div className='flex flex-col justify-center items-center my-5'>
            <IoIosWarning className='text-firebrick self-center' size={140} />
            <p className='text-16-24 font-bold'>{_t('Xác nhận thêm tài khoản?')}</p>
            <p className=''>
              {_t('Sau khi xác nhận thông tin tài khoản sẽ được lưu vào hệ thống.')}
            </p>
          </div>
        </div>
      </DialogCommon>
    </div>
  );
};

export default AddAccount;
