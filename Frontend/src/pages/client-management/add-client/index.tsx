import { Card } from 'antd';
import { useEffect, useMemo, useState } from 'react';
import { IoIosWarning } from 'react-icons/io';
import { useNavigate } from 'react-router-dom';

import { ROUTE_PATH } from '@/core/application/common/appRouters.ts';
import Constants from '@/core/application/common/constants';
import { GetListAccountRequest } from '@/core/application/dto/account/request/GetListAccountRequest.ts';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse.ts';
import { CreateCustomerRequest } from '@/core/application/dto/customer/request/CreateCustomerRequest';
import { ICustomerListResponseInterface } from '@/core/application/interfaces/response/customer/customerResponse.interface.ts';
import { ButtonCommon } from '@/infrastructure/common/components/button/button-common';
import { BreadcrumbUI } from '@/infrastructure/common/components/controls/BreadcrumbUI';
import DialogCommon from '@/infrastructure/common/components/popup/dialogCommon';
import PageTitle from '@/infrastructure/common/components/title/page-title.tsx';
import { notifySuccess } from '@/infrastructure/common/components/toast/toastMessage';
import useDebounce from '@/infrastructure/common/libs/hooks/useDebounce.hook.tsx';
import { useStateDataForm } from '@/infrastructure/common/libs/hooks/useStateDataForm.hook';
import useGetListAccountHook from '@/infrastructure/repository/account/hooks/useGetAccount.hook.tsx';
import useCreateCustomerHook from '@/infrastructure/repository/customer/hooks/useCreateCustomer.hook';
import { isFormFieldsEmpty, isValidData } from '@/infrastructure/utils/helpers.ts';
import { _t } from '@/infrastructure/utils/translation';
import ClientForm from '@/pages/client-management/components/client-form';

const AddClient = () => {
  const [data, setData] = useStateDataForm({});
  const [validate, setValidate] = useState<any>({});
  const [submittedTime, setSubmittedTime] = useState<Date | null>(null);
  const [isShowDialog, setIsShowDialog] = useState<boolean>(false);
  const [loading, setLoading] = useState<boolean>(false);
  const [createCustomerRequest] = useCreateCustomerHook();
  const [getListAccounts] = useGetListAccountHook();
  const [linkedAccountSearchValue, setLinkedAccountSearchValue] = useState<string>('');
  const [accountData, setAccountData] = useState<ICustomerListResponseInterface>();
  const debouncedSearch = useDebounce(linkedAccountSearchValue, 300);
  const cachedDebounce = useMemo(() => debouncedSearch, [debouncedSearch]);
  const isDisabled =
    !isValidData(validate) || isFormFieldsEmpty(['fullName', 'customerCode', 'email'], data);
  const navigate = useNavigate();

  const onSearch = (value: string) => {
    setLinkedAccountSearchValue(value);
  };

  const getListAccountsAsync = async () => {
    const params = new GetListAccountRequest({
      pageSize: 30,
      searchByFields: [
        {
          searchFieldName: 'userName',
          searchValue: debouncedSearch || '',
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
  const createCustomerAsync = async () => {
    if (!isValidData(validate)) return;

    setSubmittedTime(new Date());

    const userIds = data.userIds?.[0]?.value
      ? data.userIds.map((item: any) => item.value)
      : data.userIds;

    const requestBody = new CreateCustomerRequest({
      ...data,
      status: data.status ?? Constants.AccountStatus.Deactivate,
      userIds,
    });

    await createCustomerRequest(
      requestBody,
      () => {
        setIsShowDialog(!isShowDialog);
        notifySuccess('', _t('Thêm khách hàng thành công'));
      },
      () => {
        setIsShowDialog(!isShowDialog);
      },
      setLoading
    );
  };

  useEffect(() => {
    getListAccountsAsync();
  }, [cachedDebounce]);

  return (
    <div>
      <PageTitle title={'Quản lý khách hàng'} />
      <BreadcrumbUI
        items={[
          { name: 'Quản lý khách hàng', link: ROUTE_PATH.CUSTOMER_MANAGEMENT },
          { name: 'Thêm khách hàng' },
        ]}
      />
      <div className='mt-10'>
        <Card className={'shadow-custom'}>
          <div className='flex flex-col gap-4'>
            <ClientForm
              data={data}
              setData={setData}
              validate={validate}
              setValidate={setValidate}
              submittedTime={submittedTime}
              createMode={true}
              accountData={accountData}
              onSearch={onSearch}
            />
            <div className='flex flex-row gap-2 justify-center'>
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
                disabled={isDisabled}
                classButton={'w-14'}
                onClick={() => {
                  setIsShowDialog(!isShowDialog);
                }}
                colorClass='firebrick-bg'
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
        handleConfirm={() => {
          createCustomerAsync();
        }}
        isButtonSubmit={true}
        showFooter={true}
        buttonLoading={loading}
      >
        <div className='flex flex-col gap-4'>
          <div className='flex flex-col justify-center items-center my-5'>
            <IoIosWarning className='text-firebrick self-center' size={140} />
            <p className='text-16-24 font-bold'>{_t('Xác nhận thêm khách hàng?')}</p>
            <p className=''>
              {_t('Sau khi xác nhận thông tin khách hàng sẽ được lưu vào hệ thống.')}
            </p>
          </div>
        </div>
      </DialogCommon>
    </div>
  );
};

export default AddClient;
