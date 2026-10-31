import { Card } from 'antd';
import { useEffect, useState } from 'react';
import { IoIosWarning } from 'react-icons/io';
import { useNavigate, useParams } from 'react-router-dom';

import { ROUTE_PATH } from '@/core/application/common/appRouters.ts';
import Constants from '@/core/application/common/constants.ts';
import { GetListAccountRequest } from '@/core/application/dto/account/request/GetListAccountRequest.ts';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse';
import { GetCustomerDetailRequest } from '@/core/application/dto/customer/request/GetCustomerDetailRequest';
import { UpdateCustomerRequest } from '@/core/application/dto/customer/request/UpdateCustomerRequest';
import {
  ICustomerDetailResponseInterface,
  ICustomerListResponseInterface,
} from '@/core/application/interfaces/response/customer/customerResponse.interface';
import { ButtonCommon } from '@/infrastructure/common/components/button/button-common';
import { BreadcrumbUI } from '@/infrastructure/common/components/controls/BreadcrumbUI';
import DialogCommon from '@/infrastructure/common/components/popup/dialogCommon.tsx';
import PageTitle from '@/infrastructure/common/components/title/page-title.tsx';
import { notifySuccess } from '@/infrastructure/common/components/toast/toastMessage';
import useDebounce from '@/infrastructure/common/libs/hooks/useDebounce.hook.tsx';
import { useStateDataForm } from '@/infrastructure/common/libs/hooks/useStateDataForm.hook';
import useGetListAccountHook from '@/infrastructure/repository/account/hooks/useGetAccount.hook.tsx';
import useGetCustomerDetailHook from '@/infrastructure/repository/customer/hooks/useGetCustomerDetail.hook';
import useUpdateCustomerHook from '@/infrastructure/repository/customer/hooks/useUpdateCustomer.hook';
import { isValidData } from '@/infrastructure/utils/helpers.ts';
import { _t } from '@/infrastructure/utils/translation';
import ClientForm from '@/pages/client-management/components/client-form';

const EditClient = () => {
  const [updateCustomerRequest] = useUpdateCustomerHook();
  const [getCustomerDetail] = useGetCustomerDetailHook();
  const [getListAccounts] = useGetListAccountHook();
  const [dataForm, setDataForm] = useStateDataForm({});
  const [validate, setValidate] = useState<any>({});
  const [submittedTime, setSubmittedTime] = useState<Date | null>(null);
  const [isShowDialog, setIsShowDialog] = useState<boolean>(false);
  const [loading, setLoading] = useState<boolean>(false);
  const { id } = useParams();
  const [data, setData] = useState<ICustomerDetailResponseInterface>();
  const [accountData, setAccountData] = useState<ICustomerListResponseInterface>();
  const [linkedAccountSearchValue, setLinkedAccountSearchValue] = useState<string>('');
  const isDisabled = !isValidData(validate);
  const debouncedSearch = useDebounce(linkedAccountSearchValue, 300);
  const navigate = useNavigate();

  const getCustomerDetailAsync = async ({ id }: GetCustomerDetailRequest) => {
    const params = new GetCustomerDetailRequest({ id });
    await getCustomerDetail(
      params,
      (res: DataResponse) => {
        setData(res?.data);
      },
      () => {},
      setLoading
    );
  };

  const convertSelectData = () => {
    const userCustomers = data?.userCustomer;
    return userCustomers?.map((record: any) => {
      return {
        value: record?.userId,
        label: record?.user.userName,
      };
    });
  };

  const getListAccountsAsync = async () => {
    const params = new GetListAccountRequest({
      pageSize: 30,
      searchByFields: [
        {
          searchFieldName: 'userName',
          searchValue: debouncedSearch,
        },
        {
          searchFieldName: 'status',
          searchValue: Constants.AccountStatus.Active,
        },
      ],
    });
    await getListAccounts(
      params,
      (res: DataResponse) => {
        const { data } = res;
        const users = data.data;
        setAccountData(
          users
            .filter((item: any) => item?.roleName.includes(Constants.Role.Customer.value))
            .map((item: any) => ({
              value: item.id,
              label: item.userName,
            }))
        );
      },
      () => {},
      () => {}
    );
  };

  const updateCustomerAsync = async () => {
    setSubmittedTime(new Date());

    if (!isValidData(validate)) return;

    const rawUserIds = dataForm.userIds || [];
    const userIds = Array.isArray(rawUserIds)
      ? rawUserIds
          .map((item: any) => {
            if (typeof item === 'object' && item !== null) {
              return Number(item.id || item.value);
            }
            return Number(item);
          })
          .filter((val: number) => !isNaN(val) && val > 0)
      : [];

    const requestBody = new UpdateCustomerRequest({
      fullName: dataForm.fullName,
      customerCode: dataForm.customerCode,
      email: dataForm.email,
      phoneNumber: dataForm.phoneNumber,
      address: dataForm.address,
      userIds: userIds,
      status: dataForm.status,
      measurementPointCustomers: dataForm.measurementPointCustomers,
      id: dataForm.id,
    });

    await updateCustomerRequest(
      requestBody,
      () => {
        setIsShowDialog(!isShowDialog);
        notifySuccess('', _t('Cập nhật thông tin khách hàng thành công'));
      },
      () => {
        setIsShowDialog(!isShowDialog);
      },
      setLoading
    ).then(() => {
      setSubmittedTime(null);
    });
  };

  const onSearch = (value: string) => {
    setLinkedAccountSearchValue(value);
  };

  useEffect(() => {
    if (id) getCustomerDetailAsync({ id: Number(id) });
  }, [id]);

  useEffect(() => {
    if (data)
      setDataForm({
        fullName: data.fullName,
        customerCode: data.customerCode,
        email: data.email,
        phoneNumber: data.phoneNumber,
        address: data.address,
        userIds: convertSelectData(),
        status: data.status,
        measurePoints: data.measurePoints,
        measurementPointCustomers: data.measurePoints?.map((item: any) => {
          return {
            measurementPointId: item.id,
            email: item.email,
            email2: item.email2,
            email3: item.email3,
          };
        }),
        id: data.id,
      });
  }, [data]);

  useEffect(() => {
    getListAccountsAsync();
  }, [debouncedSearch]);

  return (
    <div>
      <PageTitle title={'Quản lý khách hàng'} />
      <div>
        <BreadcrumbUI
          items={[
            { name: 'Quản lý khách hàng', link: ROUTE_PATH.CUSTOMER_MANAGEMENT },
            { name: 'Sửa thông tin khách hàng' },
          ]}
        />
      </div>
      <div className='mt-10'>
        <Card className={'shadow-custom'}>
          <ClientForm
            data={dataForm}
            setData={setDataForm}
            validate={validate}
            setValidate={setValidate}
            submittedTime={submittedTime}
            accountData={accountData}
            onSearch={onSearch}
            title={'Sửa thông tin khách hàng'}
            initialAccounts={data?.userCustomer}
          />
          <div className='flex flex-row gap-2 justify-center mt-5'>
            <ButtonCommon
              title={_t('Huỷ')}
              onClick={() => {
                navigate(ROUTE_PATH.CUSTOMER_MANAGEMENT);
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
        </Card>
        <DialogCommon
          title={''}
          visible={isShowDialog}
          handleCancel={() => {
            setIsShowDialog(!isShowDialog);
          }}
          handleConfirm={() => {
            updateCustomerAsync();
          }}
          isButtonSubmit={true}
          showFooter={true}
          buttonLoading={loading}
        >
          <div className='flex flex-col gap-4'>
            <div className='flex flex-col justify-center items-center my-5'>
              <IoIosWarning className='text-firebrick self-center' size={140} />
              <p className='text-16-24 font-bold'>{_t('Xác nhận sửa thông tin khách hàng?')}</p>
              <p className=''>
                {_t('Sau khi xác nhận thông tin khách hàng sẽ thay đổi trên hệ thống.')}
              </p>
            </div>
          </div>
        </DialogCommon>
      </div>
    </div>
  );
};

export default EditClient;
