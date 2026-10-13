import { Card } from 'antd';
import { useEffect, useState } from 'react';
import { FaRegEdit } from 'react-icons/fa';
import { FaRegTrashCan } from 'react-icons/fa6';
import { useNavigate, useParams } from 'react-router-dom';

import { ROUTE_PATH } from '@/core/application/common/appRouters.ts';
import Constants from '@/core/application/common/constants.ts';
import { DeleteAccountRequest } from '@/core/application/dto/account/request/DeleteAccountRequest.ts';
import { GetAccountDetailRequest } from '@/core/application/dto/account/request/GetAccountDetailRequest.ts';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse.ts';
import { IAccountDetailResponseInterface } from '@/core/application/interfaces/response/account/accountResponse.interface.ts';
import { ButtonCommonCustomizable } from '@/infrastructure/common/components/button/button-common-customizable.tsx';
import { BreadcrumbUI } from '@/infrastructure/common/components/controls/BreadcrumbUI.tsx';
import { FullPageLoading } from '@/infrastructure/common/components/controls/Loading.tsx';
import DialogCommon from '@/infrastructure/common/components/popup/dialogCommon.tsx';
import PrivateComponent from '@/infrastructure/common/components/private-component/private-component.tsx';
import PageTitle from '@/infrastructure/common/components/title/page-title.tsx';
import { notifySuccess } from '@/infrastructure/common/components/toast/toastMessage.tsx';
import { useStateDataForm } from '@/infrastructure/common/libs/hooks/useStateDataForm.hook.tsx';
import useDeleteAccountHook from '@/infrastructure/repository/account/hooks/useDeleteAccount.hook.tsx';
import useGetAccountDetailHook from '@/infrastructure/repository/account/hooks/useGetAccountDetail.hook.tsx';
import { _t } from '@/infrastructure/utils/translation.ts';
import AccountForm from '@/pages/account-management/components/account-form.tsx';

const AccountDetail = () => {
  const [dataForm, setDataForm] = useStateDataForm({});
  const { id } = useParams();
  const [loading, setLoading] = useState<boolean>(false);
  const [validate, setValidate] = useState<any>({});
  const [submittedTime] = useState<Date | null>(null);
  const navigate = useNavigate();
  const [isShowDeleteDialog, setIsShowDeleteDialog] = useState<boolean>(false);
  const [deleteAccountRequest] = useDeleteAccountHook();

  const [getAccountDetail] = useGetAccountDetailHook();
  const [data, setData] = useState<IAccountDetailResponseInterface>();

  const deleteAccountAsync = async (id: number) => {
    const params = new DeleteAccountRequest({ id });
    await deleteAccountRequest(
      params,
      () => {
        setIsShowDeleteDialog(!isShowDeleteDialog);
        notifySuccess('', _t('Xoá tài khoản thành công'));
        navigate(ROUTE_PATH.ACCOUNT_MANAGEMENT);
      },
      () => {
        setIsShowDeleteDialog(!isShowDeleteDialog);
      },
      setLoading
    );
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

  useEffect(() => {
    if (id) getAccountDetailAsync({ id });
  }, [id]);

  useEffect(() => {
    if (data) setDataForm(data);
  }, [data]);

  return (
    <div>
      <PageTitle title={'Quản lý tài khoản'} />
      <div className={'flex items-center justify-between'}>
        <BreadcrumbUI
          items={[
            { name: 'Quản lý tài khoản', link: ROUTE_PATH.ACCOUNT_MANAGEMENT },
            { name: 'Thông tin tài khoản' },
          ]}
        />
        <div className={'flex items-center gap-3'}>
          <PrivateComponent roles={[Constants.Role.Admin.value, Constants.Role.SuperAdmin.value]}>
            <ButtonCommonCustomizable
              title={null}
              icon={<FaRegEdit size={20} />}
              classButton={'text-button-variant'}
              onClick={() => {
                navigate(ROUTE_PATH.EDIT_ACCOUNT.replace(':id', id?.toString() || ''));
              }}
              tooltipText={'Sửa'}
            />
          </PrivateComponent>
          <PrivateComponent roles={[Constants.Role.SuperAdmin.value]}>
            <ButtonCommonCustomizable
              title={null}
              classButton={'text-button-variant'}
              onClick={() => {
                setIsShowDeleteDialog(!isShowDeleteDialog);
              }}
              icon={<FaRegTrashCan size={20} />}
              tooltipText={'Xoá'}
            />
          </PrivateComponent>
        </div>
      </div>
      <div className='mt-10'>
        <Card className={'shadow-custom'}>
          <AccountForm
            data={dataForm}
            setData={setDataForm}
            validate={validate}
            setValidate={setValidate}
            submittedTime={submittedTime}
            disabled={true}
            viewMode={true}
          />
        </Card>
      </div>
      <DialogCommon
        title={''}
        visible={isShowDeleteDialog}
        handleCancel={() => {
          setIsShowDeleteDialog(!isShowDeleteDialog);
        }}
        handleConfirm={() => {
          deleteAccountAsync(Number(id));
        }}
        isButtonSubmit={true}
        showFooter={true}
      >
        <div className='flex flex-col gap-4'>
          <div className='flex flex-col justify-center items-center my-10 gap-2'>
            <p className='text-20-20 font-bold'>Xác nhận xoá tài khoản</p>
            <p className='text-16-16'>{_t('Sau khi xác nhận tài khoản sẽ bị xoá khỏi hệ thống')}</p>
          </div>
        </div>
      </DialogCommon>
      <FullPageLoading isLoading={loading} />
    </div>
  );
};

export default AccountDetail;
