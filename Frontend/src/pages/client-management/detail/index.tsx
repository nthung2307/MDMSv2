import { Card } from 'antd';
import { useEffect, useState } from 'react';
import { FaRegEdit } from 'react-icons/fa';
import { FaRegTrashCan } from 'react-icons/fa6';
import { useNavigate, useParams } from 'react-router-dom';

import { ROUTE_PATH } from '@/core/application/common/appRouters';
import Constants from '@/core/application/common/constants.ts';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse';
import { DeleteCustomerRequest } from '@/core/application/dto/customer/request/DeleteCustomerRequest.ts';
import { GetCustomerDetailRequest } from '@/core/application/dto/customer/request/GetCustomerDetailRequest';
import { ICustomerDetailResponseInterface } from '@/core/application/interfaces/response/customer/customerResponse.interface';
import { ButtonCommonCustomizable } from '@/infrastructure/common/components/button/button-common-customizable.tsx';
import { BreadcrumbUI } from '@/infrastructure/common/components/controls/BreadcrumbUI';
import { FullPageLoading } from '@/infrastructure/common/components/controls/Loading';
import DialogCommon from '@/infrastructure/common/components/popup/dialogCommon.tsx';
import PrivateComponent from '@/infrastructure/common/components/private-component/private-component.tsx';
import PageTitle from '@/infrastructure/common/components/title/page-title.tsx';
import { notifySuccess } from '@/infrastructure/common/components/toast/toastMessage.tsx';
import { useStateDataForm } from '@/infrastructure/common/libs/hooks/useStateDataForm.hook';
import useDeleteCustomerHook from '@/infrastructure/repository/customer/hooks/useDeleteCustomer.hook.tsx';
import useGetCustomerDetailHook from '@/infrastructure/repository/customer/hooks/useGetCustomerDetail.hook';
import { _t } from '@/infrastructure/utils/translation.ts';
import ClientForm from '@/pages/client-management/components/client-form';

const ClientDetail = () => {
  const navigate = useNavigate();
  const [deleteCustomerRequest] = useDeleteCustomerHook();
  const [dataForm, setDataForm] = useStateDataForm({});
  const [validate, setValidate] = useState<any>({});
  const [submittedTime] = useState<Date | null>(null);
  const [loading, setLoading] = useState<boolean>(false);
  const [isShowDeleteDialog, setIsShowDeleteDialog] = useState<boolean>(false);
  const { id } = useParams();
  const recordId = id ? Number(id) : 0;
  const [getCustomerDetail] = useGetCustomerDetailHook();
  const [data, setData] = useState<ICustomerDetailResponseInterface>();

  const convertSelectData = () => {
    const userCustomers = data?.userCustomer;
    return userCustomers?.map((record: any) => {
      return {
        value: record?.userId,
        label: record?.user.userName,
      };
    });
  };

  const deleteCustomerAsync = async (id: number) => {
    const params = new DeleteCustomerRequest({ id });
    await deleteCustomerRequest(
      params,
      () => {
        setIsShowDeleteDialog(!isShowDeleteDialog);
        notifySuccess('', _t('Xóa khách hàng thành công'));
        navigate(ROUTE_PATH.CUSTOMER_MANAGEMENT);
      },
      () => {
        setIsShowDeleteDialog(!isShowDeleteDialog);
      },
      setLoading
    );
  };

  const getCustomerDetailAsync = async ({ id }: GetCustomerDetailRequest) => {
    await getCustomerDetail(
      { id },
      (res: DataResponse) => {
        setData(res?.data);
      },
      () => {},
      setLoading
    );
  };

  useEffect(() => {
    if (id) getCustomerDetailAsync({ id: recordId });
  }, [id]);

  useEffect(() => {
    if (data) setDataForm({ ...data, userIds: convertSelectData() });
  }, [data]);

  return (
    <div>
      <PageTitle title={'Quản lý khách hàng'} />
      <div className='flex justify-between items-center'>
        <BreadcrumbUI
          items={[
            { name: 'Quản lý khách hàng', link: ROUTE_PATH.CUSTOMER_MANAGEMENT },
            { name: 'Thông tin khách hàng' },
          ]}
        />
        <div className={'flex items-center gap-3'}>
          <PrivateComponent roles={[Constants.Role.Admin.value, Constants.Role.SuperAdmin.value]}>
            <ButtonCommonCustomizable
              title={null}
              icon={<FaRegEdit size={20} />}
              classButton={'text-button-variant'}
              onClick={() => {
                navigate(ROUTE_PATH.EDIT_CUSTOMER.replace(':id', recordId.toString()));
              }}
              tooltipText={'Sửa'}
            />
          </PrivateComponent>
          <PrivateComponent roles={[Constants.Role.SuperAdmin.value]}>
            <ButtonCommonCustomizable
              title={null}
              onClick={() => {
                setIsShowDeleteDialog(!isShowDeleteDialog);
              }}
              classButton={'text-button-variant'}
              icon={<FaRegTrashCan size={20} />}
              tooltipText={'Xoá'}
            />
          </PrivateComponent>
        </div>
      </div>
      <div className='mt-10'>
        <Card className={'shadow-custom'}>
          <ClientForm
            data={dataForm}
            setData={setDataForm}
            validate={validate}
            setValidate={setValidate}
            submittedTime={submittedTime}
            disabled={true}
            initialAccounts={data?.userCustomer}
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
          deleteCustomerAsync(Number(id));
        }}
        isButtonSubmit={true}
        showFooter={true}
      >
        <div className='flex flex-col gap-4'>
          <div className='flex flex-col justify-center items-center my-10 gap-2'>
            <p className='text-20-20 font-bold'>Xác nhận xoá khách hàng</p>
            <p className='text-16-16'>
              {_t('Sau khi xác nhận khách hàng sẽ bị xoá khỏi hệ thống')}
            </p>
          </div>
        </div>
      </DialogCommon>
      <FullPageLoading isLoading={loading} />
    </div>
  );
};

export default ClientDetail;
