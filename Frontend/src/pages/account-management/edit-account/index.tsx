import { Card } from 'antd';
import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';

import { ROUTE_PATH } from '@/core/application/common/appRouters.ts';
import { GetAccountDetailRequest } from '@/core/application/dto/account/request/GetAccountDetailRequest.ts';
import { UpdateUserRequest } from '@/core/application/dto/account/request/UpdateUserRequest';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse.ts';
import { IAccountDetailResponseInterface } from '@/core/application/interfaces/response/account/accountResponse.interface.ts';
import { ButtonCommon } from '@/infrastructure/common/components/button/button-common';
import { BreadcrumbUI } from '@/infrastructure/common/components/controls/BreadcrumbUI.tsx';
import { FullPageLoading } from '@/infrastructure/common/components/controls/Loading.tsx';
import DialogCommon from '@/infrastructure/common/components/popup/dialogCommon.tsx';
import PageTitle from '@/infrastructure/common/components/title/page-title.tsx';
import { notifySuccess } from '@/infrastructure/common/components/toast/toastMessage';
import { useStateDataForm } from '@/infrastructure/common/libs/hooks/useStateDataForm.hook.tsx';
import useGetAccountDetailHook from '@/infrastructure/repository/account/hooks/useGetAccountDetail.hook.tsx';
import useUpdateAccountHook from '@/infrastructure/repository/account/hooks/useUpdateAccount.hook';
import { _t } from '@/infrastructure/utils/translation.ts';
import AccountForm from '@/pages/account-management/components/account-form.tsx';

const EditAccount = () => {
  const [getAccountDetail] = useGetAccountDetailHook();
  const [updateAccountRequest] = useUpdateAccountHook();
  const [dataForm, setDataForm] = useStateDataForm({});
  const [loading, setLoading] = useState<boolean>(false);
  const [submittedTime, setSubmittedTime] = useState<Date | null>(null);
  const { id } = useParams();
  const [validate, setValidate] = useState<any>({});
  const [isShowDialog, setIsShowDialog] = useState<boolean>(false);
  const [data, setData] = useState<IAccountDetailResponseInterface>();
  const navigate = useNavigate();
  const [isDisabled, setIsDisabled] = useState<boolean>(true);

  const isValidData = () => {
    return !Object.values(validate).some((it: any) => it.isError);
  };

  const getAccountDetailAsync = async ({ id }: GetAccountDetailRequest) => {
    await getAccountDetail(
      { id },
      (data: DataResponse) => {
        setData(data?.data);
      },
      () => {},
      setLoading
    );
  };

  const updateAccountAsync = async () => {
    setSubmittedTime(new Date());

    if (!isValidData()) return;

    const requestBody = new UpdateUserRequest({
      ...dataForm,
    });
    await updateAccountRequest(
      requestBody,
      () => {
        setIsShowDialog(!isShowDialog);
        notifySuccess('', _t('Cập nhật thông tin tài khoản thành công'));
      },
      () => {
        setIsShowDialog(!isShowDialog);
      },
      setLoading
    ).then(() => {
      setSubmittedTime(null);
    });
  };

  useEffect(() => {
    if (id) getAccountDetailAsync({ id });
  }, [id]);

  useEffect(() => {
    if (data) setDataForm(data);
  }, [data]);

  useEffect(() => {
    if (isValidData()) {
      setIsDisabled(false);
    } else {
      setIsDisabled(true);
    }
  }, [validate]);
  return (
    <div>
      <PageTitle title={'Quản lý tài khoản'} />
      <div className=''>
        <BreadcrumbUI
          items={[
            { name: 'Quản lý tài khoản', link: ROUTE_PATH.ACCOUNT_MANAGEMENT },
            { name: 'Sửa thông tin tài khoản' },
          ]}
        />
      </div>
      <div className='mt-10'>
        <Card className={'shadow-custom'}>
          <AccountForm
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
              classButton={'w-14'}
              disabled={isDisabled}
              colorClass={'firebrick-bg'}
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
        handleConfirm={updateAccountAsync}
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
      <FullPageLoading isLoading={loading} />
    </div>
  );
};
export default EditAccount;
