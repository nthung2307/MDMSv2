import PlusOutlined from '@ant-design/icons/lib/icons/PlusOutlined';
import { Card, Dropdown, MenuProps, TableProps } from 'antd';
import { useEffect, useState } from 'react';
import { BsThreeDots } from 'react-icons/bs';
import { FiShare } from 'react-icons/fi';
import { IoBackspaceOutline, IoSearch } from 'react-icons/io5';
import { useNavigate } from 'react-router-dom';

import { ROUTE_PATH } from '@/core/application/common/appRouters.ts';
import Constants from '@/core/application/common/constants';
import { GetListAccountRequest } from '@/core/application/dto/account/request/GetListAccountRequest.ts';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse.ts';
import { IAccountListResponseInterface } from '@/core/application/interfaces/response/account/accountResponse.interface.ts';
import { ButtonCommonCustomizable } from '@/infrastructure/common/components/button/button-common-customizable.tsx';
import DialogExportForm from '@/infrastructure/common/components/dialog-export/dialog-export';
import InputSelectCommon from '@/infrastructure/common/components/input/input-select-common.tsx';
import InputTextCommon from '@/infrastructure/common/components/input/input-text-common.tsx';
import PaginationCommon from '@/infrastructure/common/components/pagination/pagination-common';
import PrivateComponent from '@/infrastructure/common/components/private-component/private-component.tsx';
import StatusTagCommon from '@/infrastructure/common/components/status-tag/status-tag-common.tsx';
import TableCommon from '@/infrastructure/common/components/table/table-common.tsx';
import PageTitle from '@/infrastructure/common/components/title/page-title.tsx';
import useDebounce from '@/infrastructure/common/libs/hooks/useDebounce.hook';
import { useStateDataForm } from '@/infrastructure/common/libs/hooks/useStateDataForm.hook';
import useGetListAccountHook from '@/infrastructure/repository/account/hooks/useGetAccount.hook.tsx';
import { deleteParams, getListUserRoles } from '@/infrastructure/utils/helpers.ts';
import { _t } from '@/infrastructure/utils/translation.ts';

const AccountManagement = () => {
  const navigate = useNavigate();
  const [dataForm, setDataForm] = useStateDataForm({
    status: Constants.AccountStatus.Active,
  });
  const [getListAccount] = useGetListAccountHook();
  // const [updateAccountStatus] = useUpdateStatusHook();
  // const [deleteAccountRequest] = useDeleteAccountHook();
  const [loading, setLoading] = useState<boolean>(false);
  const [size, setSize] = useState<number>(10);
  const [data, setData] = useState<IAccountListResponseInterface>();
  // const [isShowDialog, setIsShowDialog] = useState<boolean>(false);
  // const [isShowDeleteDialog, setIsShowDeleteDialog] = useState<boolean>(false);
  const [isShowExport, setIsShowExport] = useState<boolean>(false);

  const [getListAccountParamsRequest, setGetListAccountParamsRequest] =
    useState<GetListAccountRequest>({
      searchByFields: [
        {
          searchFieldName: 'status',
          searchValue: Constants.CustomerStatus.Active,
        },
      ],
      sortByFields: [],
    });

  const [paginationData, setPaginationData] = useState<{
    hasNext: boolean;
    hasPrevious: boolean;
  }>({ hasNext: false, hasPrevious: false });

  const firstAccountID = data?.data?.[0]?.id;
  const lastAccountID = data?.data?.[data?.data?.length - 1]?.id;

  const getListAccountAsync = async ({
    before,
    pageSize = size,
    after,
    searchByFields = [],
    sortByFields = [],
  }: GetListAccountRequest) => {
    const params = {
      pageSize: pageSize,
      ...(before && { before }),
      ...(after && { after }),
      ...(searchByFields.length > 0 && { searchByFields }),
      ...(sortByFields.length > 0 && { sortByFields }),
    };
    await getListAccount(
      params,
      (data: DataResponse) => {
        setData(data?.data);
        if (!data.success) return;
        setPaginationData({
          hasNext: data?.data?.hasNext,
          hasPrevious: data?.data?.hasPrevious,
        });
      },
      () => {},
      setLoading
    );
  };

  // const deleteAccountAsync = async (id: number) => {
  //   const params = new DeleteAccountRequest({ id });
  //   await deleteAccountRequest(
  //     params,
  //     () => {
  //       setIsShowDeleteDialog(!isShowDeleteDialog);
  //       notifySuccess('', _t('Xoá tài khoản thành công'));
  //       getListAccountAsync({ ...getListAccountParamsRequest });
  //     },
  //     () => {},
  //     setLoading
  //   );
  // };
  //
  // const updateStatus = async (id: string) => {
  //   await updateAccountStatus(
  //     { id },
  //     () => {
  //       setIsShowDialog(!isShowDialog);
  //       notifySuccess('', _t('Cập nhật trạng thái thành công'));
  //       getListAccountAsync({ ...getListAccountParamsRequest });
  //     },
  //     () => {},
  //     () => {}
  //   );
  // };

  const debounceClientName = useDebounce(dataForm?.clientName, 300);

  const handleClearSearch = () => {
    setDataForm({
      clientName: '',
      roleName: '',
      status: '',
    });
    setGetListAccountParamsRequest({});
  };

  useEffect(() => {
    const searchByFields: any = [];
    if (debounceClientName) {
      searchByFields.push({ searchFieldName: 'fullName', searchValue: debounceClientName });
    }
    if (dataForm?.roleName) {
      searchByFields.push({ searchFieldName: 'roleName', searchValue: dataForm.roleName });
    }
    if (dataForm?.status) {
      searchByFields.push({ searchFieldName: 'status', searchValue: dataForm.status });
    }

    setGetListAccountParamsRequest((prev) => ({
      ...prev,
      searchByFields: searchByFields.length > 0 ? searchByFields : [],
    }));
  }, [debounceClientName, dataForm?.roleName, dataForm?.status]);

  const getRoleName = (role: string) => {
    const listRole = getListUserRoles();
    const roleName = listRole.find((item) => item.value === role);
    return roleName?.label || 'N/A';
  };

  const columns: TableProps['columns'] = [
    {
      title: 'Tên đăng nhập',
      dataIndex: 'userName',
      align: 'left',
    },
    {
      title: 'Nhóm người dùng',
      dataIndex: 'roleName',
      align: 'left',
    },
    {
      title: 'Họ và tên',
      dataIndex: 'fullName',
      align: 'left',
    },
    {
      title: 'Số điện thoại',
      dataIndex: 'phoneNumber',
      align: 'left',
    },
    {
      title: 'Email',
      dataIndex: 'email',
      align: 'left',
    },
    {
      title: 'Trạng thái',
      dataIndex: 'status',
      align: 'left',
      width: '10%',
      render: (_, record) => {
        const isActive = record.status === Constants.CustomerStatus.Active;
        const backgroundColor = isActive ? '#41A026' : '#B22222';
        const statusText = isActive ? 'Hoạt động' : 'Vô hiệu';
        return (
          <StatusTagCommon status={statusText} bgColor={backgroundColor} textColor={'#FFFFFF'} />
        );
      },
    },
    {
      key: 'action',
      // fixed: 'right',
      width: 50,
      render: (_, record) => {
        const items: MenuProps['items'] = [
          {
            key: '1',
            label: 'Chi tiết',
            onClick: () => {
              navigate(
                ROUTE_PATH.ACCOUNT_DETAIL.replace(Constants.ReplaceRouterConfig.id, record.id)
              );
            },
          },
        ];

        return (
          <div className={'flex justify-center cursor-pointer'}>
            <Dropdown menu={{ items }}>
              <BsThreeDots />
            </Dropdown>
          </div>
        );
      },
    },
  ];

  const tableDataSource = data?.data?.map((item, index) => {
    return {
      key: item.id,
      index: index + 1,
      userName: item.userName,
      fullName: item.fullName,
      roleName: getRoleName(item.roleName),
      id: item.id,
      phoneNumber: item.phoneNumber,
      email: item.email,
      status: item.status,
      changeStatus: item.status,
    };
  });

  useEffect(() => {
    getListAccountAsync({ ...getListAccountParamsRequest });
  }, [size, getListAccountParamsRequest]);

  return (
    <div>
      <PageTitle title={'Quản lý tài khoản'} />
      <div className='flex justify-between items-center'>
        <h1 className='text-24-28 font-bold'>{_t('Quản lý tài khoản')}</h1>
        <div className='flex gap-1'>
          <PrivateComponent roles={[Constants.Role.SuperAdmin.value]}>
            <ButtonCommonCustomizable
              title={_t('')}
              classButton='text-button-variant'
              tooltipText='Tạo tài khoản'
              onClick={() => {
                navigate(ROUTE_PATH.ADD_ACCOUNT);
              }}
              icon={<PlusOutlined size={20} />}
            />
          </PrivateComponent>
          <ButtonCommonCustomizable
            title={_t('')}
            classButton='text-button-variant'
            tooltipText='Xuất file'
            disabled={!(tableDataSource && tableDataSource.length > 0)}
            onClick={() => {
              setIsShowExport(true);
            }}
            icon={<FiShare size={20} />}
          />
        </div>
      </div>
      <Card className='mt-5 shadow-custom'>
        <div className='mb-5 flex items-center justify-between'>
          <div className={'flex items-center gap-2 w-2/3'}>
            <div className={'basis-1/3'}>
              <InputTextCommon
                label={null}
                attribute={'clientName'}
                isLabel={false}
                dataAttribute={dataForm?.clientName}
                placeholder={_t('Nhập họ và tên')}
                setData={setDataForm}
                isRequired={false}
                prefix={<IoSearch size={20} />}
                allowClear={true}
              />
            </div>
            <div className={'basis-1/3'}>
              <InputSelectCommon
                label={null}
                attribute={'roleName'}
                isLabel={false}
                dataAttribute={dataForm?.roleName}
                setData={setDataForm}
                isRequired={false}
                listDataItem={
                  getListUserRoles().filter(
                    (item) => item.value !== Constants.Role.SuperAdmin.value
                  ) || []
                }
                placeholder={_t('Nhóm người dùng')}
                allowClear={true}
              />
            </div>
            <div className={'basis-1/3'}>
              <InputSelectCommon
                label={null}
                attribute={'status'}
                dataAttribute={dataForm?.status}
                isLabel={false}
                setData={setDataForm}
                isRequired={false}
                listDataItem={[
                  { label: 'Hoạt động', value: Constants.AccountStatus.Active },
                  { label: 'Vô hiệu', value: Constants.AccountStatus.Deactivate },
                ]}
                placeholder={_t('Trạng thái')}
                allowClear={true}
              />
            </div>

            <div className={'flex items-center gap-1'}>
              {dataForm?.clientName || dataForm?.roleName || dataForm?.status ? (
                <ButtonCommonCustomizable
                  title={''}
                  classButton='text-button-variant'
                  tooltipText='Xoá tất cả'
                  icon={<IoBackspaceOutline size={25} />}
                  onClick={handleClearSearch}
                  variant={'text'}
                />
              ) : null}
            </div>
          </div>
          <PaginationCommon
            pageSize={size}
            hasNext={paginationData?.hasNext}
            setSize={(newSize) => {
              // Update the page size and reset pagination
              setSize(newSize);
              setGetListAccountParamsRequest((prev) => {
                const updatedParams = { ...prev, pageSize: newSize };
                return deleteParams(updatedParams, ['before', 'after']); // Remove 'before' and 'after' parameters
              });
            }}
            hasPrevious={paginationData?.hasPrevious}
            onClickPrevious={() => {
              // When clicking on Previous, remove the 'after' and set 'before'
              setGetListAccountParamsRequest((prev) => {
                const updatedParams = { ...prev, before: firstAccountID };
                return deleteParams(updatedParams, ['after']); // Remove 'after' parameter
              });
            }}
            onClickNext={() => {
              // When clicking on Next, remove the 'before' and set 'after'
              setGetListAccountParamsRequest((prev) => {
                const updatedParams = { ...prev, after: lastAccountID };
                return deleteParams(updatedParams, ['before']); // Remove 'before' parameter
              });
            }}
          />
        </div>
        <TableCommon
          columns={columns}
          dataSource={tableDataSource}
          pagination={false}
          loading={loading}
          scroll={{ x: 'max-content', y: window.innerHeight * 0.66 }}
        />
      </Card>
      {/* Dialog export file */}
      <DialogExportForm
        open={isShowExport}
        handleCancel={() => {
          setIsShowExport(false);
        }}
        title={'Quản lý tài khoản'}
        dataSource={tableDataSource}
        listHeader={Constants.listHeaderExport.accounts}
        fileNameCsv={'taikhoan'}
        typeExportAll={'user'}
      />
    </div>
  );
};

export default AccountManagement;
