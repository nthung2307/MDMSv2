import PlusOutlined from '@ant-design/icons/lib/icons/PlusOutlined';
import { Card, Dropdown, MenuProps, TableProps } from 'antd';
import { useEffect, useState } from 'react';
import { BsThreeDots } from 'react-icons/bs';
import { FiDownload, FiShare } from 'react-icons/fi';
import { IoBackspaceOutline, IoSearch } from 'react-icons/io5';
import { useNavigate } from 'react-router-dom';

import { ROUTE_PATH } from '@/core/application/common/appRouters.ts';
import Constants from '@/core/application/common/constants';
import { DataResponse } from '@/core/application/dto/common/responses/successResponse';
import { GetListCustomerRequest } from '@/core/application/dto/customer/request/GetListCustomerRequest';
import { ICustomerListResponseInterface } from '@/core/application/interfaces/response/customer/customerResponse.interface';
import { ButtonCommonCustomizable } from '@/infrastructure/common/components/button/button-common-customizable.tsx';
import DialogExportForm from '@/infrastructure/common/components/dialog-export/dialog-export';
import InputSelectCommon from '@/infrastructure/common/components/input/input-select-common.tsx';
import InputTextCommon from '@/infrastructure/common/components/input/input-text-common';
import PaginationCommon from '@/infrastructure/common/components/pagination/pagination-common';
import PrivateComponent from '@/infrastructure/common/components/private-component/private-component.tsx';
import StatusTagCommon from '@/infrastructure/common/components/status-tag/status-tag-common.tsx';
import TableCommon from '@/infrastructure/common/components/table/table-common';
import PageTitle from '@/infrastructure/common/components/title/page-title.tsx';
import useDebounce from '@/infrastructure/common/libs/hooks/useDebounce.hook';
import { useStateDataForm } from '@/infrastructure/common/libs/hooks/useStateDataForm.hook';
import useGetListCustomerHook from '@/infrastructure/repository/customer/hooks/useGetCustomerList.hook';
import { deleteParams } from '@/infrastructure/utils/helpers.ts';
import { _t } from '@/infrastructure/utils/translation';
import DialogImportCustomer from '@/pages/client-management/components/dialog-import-customer';

const ClientManagement = () => {
  const navigate = useNavigate();
  const [dataForm, setDataForm] = useStateDataForm({
    status: Constants.AccountStatus.Active,
  });
  const [isShowExport, setIsShowExport] = useState<boolean>(false);
  const [isShowImport, setIsShowImport] = useState<boolean>(false);
  const [getListCustomer] = useGetListCustomerHook();
  // const [deleteCustomerRequest] = useDeleteCustomerHook();
  // const [changeCustomerStatus] = useChangeCustomerStatusHook();
  const [loading, setLoading] = useState<boolean>(false);
  const [size, setSize] = useState<number>(10);
  const [data, setData] = useState<ICustomerListResponseInterface>();
  const [getListCustomerParamsRequest, setGetListCustomerParamsRequest] =
    useState<GetListCustomerRequest>({
      searchByFields: [
        {
          searchFieldName: 'status',
          searchValue: Constants.CustomerStatus.Active,
        },
      ],
      sortByFields: [],
    });
  // const [id, setId] = useState<number>(0);
  // const [isShowDeleteDialog, setIsShowDeleteDialog] = useState<boolean>(false);
  // const [isShowChangeStatusDialog, setIsShowChangeStatusDialog] = useState<boolean>(false);

  const [paginationData, setPaginationData] = useState<{
    hasNext: boolean;
    hasPrevious: boolean;
  }>({ hasNext: false, hasPrevious: false });

  const firstCustomerID = data?.data?.[0]?.id;
  const lastCustomerID = data?.data?.[data?.data?.length - 1]?.id;

  // const checkClientStatusById = (id: number) => {
  //   const client = data?.data?.find((item) => item.id === id);
  //   return client?.status === Constants.AccountStatus.Active;
  // };

  const addListCustomerParams = (params: GetListCustomerRequest) => {
    setGetListCustomerParamsRequest((prev) => {
      return { ...prev, ...params };
    });
  };

  const updateParamsValueFromListCustomerParams = (paramsToUpdate: any) => {
    setGetListCustomerParamsRequest((prev: any) => {
      return { ...prev, ...paramsToUpdate };
    });
  };

  // Add debounced search hooks
  const debounceCustomerCode = useDebounce(dataForm?.code, 300);

  // Replace handleSearch with useEffect
  useEffect(() => {
    const searchByFields: any = [];
    if (debounceCustomerCode) {
      searchByFields.push({ searchFieldName: 'customerCode', searchValue: debounceCustomerCode });
    }
    if (dataForm?.status) {
      searchByFields.push({ searchFieldName: 'status', searchValue: dataForm.status });
    }
    if (searchByFields.length > 0) {
      addListCustomerParams({
        searchByFields,
      });
    } else {
      updateParamsValueFromListCustomerParams({ searchByFields: [] });
    }
  }, [debounceCustomerCode, dataForm?.status]);

  const handleClearSearch = () => {
    setDataForm({
      code: '',
      status: '',
    });
    updateParamsValueFromListCustomerParams({ searchByFields: [] });
  };

  const columns: TableProps['columns'] = [
    {
      title: 'Mã khách hàng',
      dataIndex: 'code',
      align: 'left',
      width: '12%',
    },
    {
      title: 'Tên khách hàng',
      dataIndex: 'name',
      align: 'left',
    },
    {
      title: 'Điện thoại',
      dataIndex: 'phoneNumber',
      align: 'left',
      width: '12%',
    },
    {
      title: 'Địa chỉ',
      dataIndex: 'address',
      align: 'left',
    },
    {
      title: 'Trạng thái',
      dataIndex: 'status',
      align: 'left',
      width: '15%',
      render: (_, record) => {
        const isActive = record.status === Constants.CustomerStatus.Active;
        const backgroundColor = isActive ? '#41A026' : '#B22222';
        const statusText = isActive ? 'Đang hoạt động' : 'Ngừng hoạt động';
        return (
          <StatusTagCommon status={statusText} bgColor={backgroundColor} textColor={'#FFFFFF'} />
        );
      },
    },
    {
      title: '',
      key: 'action',
      align: 'center',
      fixed: 'right',
      width: 50,
      render: (_, record) => {
        const items: MenuProps['items'] = [
          {
            key: '1',
            label: 'Chi tiết',
            onClick: () => {
              const url = ROUTE_PATH.CUSTOMER_DETAIL.replace(
                Constants.ReplaceRouterConfig.id,
                record.key
              );
              window.open(url, '_blank');
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

  const getListCustomerAsync = async ({
    before,
    pageSize = size,
    after,
    searchByFields = [],
    sortByFields = [],
  }: GetListCustomerRequest) => {
    const params = {
      pageSize: pageSize,
      ...(before && { before }),
      ...(after && { after }),
      ...(searchByFields.length > 0 && { searchByFields }),
      ...(sortByFields.length > 0 && { sortByFields }),
    };
    await getListCustomer(
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

  const tableDataSource = data?.data?.map((item, index) => {
    return {
      key: item.id,
      index: index + 1,
      name: item.fullName,
      code: item.customerCode,
      phoneNumber: item.phoneNumber,
      email: item.email,
      address: item.address,
      status: item.status,
    };
  });

  useEffect(() => {
    getListCustomerAsync({ ...getListCustomerParamsRequest });
  }, [size, getListCustomerParamsRequest]);

  return (
    <div>
      <PageTitle title={'Quản lý khách hàng'} />
      <div className='flex justify-between items-center'>
        <h1 className='text-24-28 font-bold'>{_t('Quản lý khách hàng')}</h1>
        <div className='flex gap-1'>
          <PrivateComponent roles={[Constants.Role.Admin.value, Constants.Role.SuperAdmin.value]}>
            <ButtonCommonCustomizable
              title={_t('')}
              tooltipText={'Thêm khách hàng'}
              classButton='text-button-variant'
              onClick={() => {
                navigate(ROUTE_PATH.ADD_CUSTOMER);
              }}
              icon={<PlusOutlined size={20} />}
            />
          </PrivateComponent>
          <ButtonCommonCustomizable
            title={''}
            tooltipText={'Xuất file'}
            disabled={!(tableDataSource && tableDataSource.length > 0)}
            onClick={() => {
              setIsShowExport(true);
            }}
            icon={<FiShare size={20} />}
            classButton='text-button-variant'
          />
          <PrivateComponent roles={[Constants.Role.Admin.value, Constants.Role.SuperAdmin.value]}>
            <ButtonCommonCustomizable
              title={''}
              tooltipText={'Nhập từ CSV'}
              onClick={() => {
                setIsShowImport(true);
              }}
              icon={<FiDownload size={20} />}
              classButton='text-button-variant'
            />
          </PrivateComponent>
        </div>
      </div>
      <Card className='mt-5 shadow-custom'>
        <div className='mb-5 flex justify-between items-center'>
          <div className='flex items-center w-2/3 gap-2'>
            <div className='basis-4/5 flex items-center gap-2'>
              <div className='basis-1/2'>
                <InputTextCommon
                  label={null}
                  dataAttribute={dataForm?.code}
                  attribute={'code'}
                  setData={setDataForm}
                  isRequired={false}
                  placeholder={_t('Nhập mã khách hàng')}
                  prefix={<IoSearch />}
                />
              </div>
              <div className='basis-1/2'>
                <InputSelectCommon
                  label={null}
                  attribute={'status'}
                  dataAttribute={dataForm?.status}
                  setData={setDataForm}
                  isRequired={false}
                  listDataItem={[
                    { label: 'Đang hoạt động', value: Constants.AccountStatus.Active },
                    { label: 'Ngừng hoạt động', value: Constants.AccountStatus.Deactivate },
                  ]}
                  placeholder={'Trạng thái'}
                  allowClear={true}
                />
              </div>
            </div>
            <div className={'flex items-center gap-1'}>
              {dataForm?.status || dataForm?.code ? (
                <ButtonCommonCustomizable
                  title={_t('')}
                  tooltipText='Xoá tất cả'
                  icon={<IoBackspaceOutline size={25} />}
                  onClick={handleClearSearch}
                  classButton='text-button-variant'
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
              setGetListCustomerParamsRequest((prev) => {
                const updatedParams = { ...prev, pageSize: newSize };
                return deleteParams(updatedParams, ['before', 'after']); // Remove 'before' and 'after' parameters
              });
            }}
            hasPrevious={paginationData?.hasPrevious}
            onClickPrevious={() => {
              // When clicking on Previous, remove the 'after' and set 'before'
              setGetListCustomerParamsRequest((prev) => {
                const updatedParams = { ...prev, before: firstCustomerID };
                return deleteParams(updatedParams, ['after']); // Remove 'after' parameter
              });
            }}
            onClickNext={() => {
              // When clicking on Next, remove the 'before' and set 'after'
              setGetListCustomerParamsRequest((prev) => {
                const updatedParams = { ...prev, after: lastCustomerID };
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
        title='Quản lý khách hàng'
        dataSource={tableDataSource}
        listHeader={Constants.listHeaderExport.clients}
        fileNameCsv={'khachhang'}
        typeExportAll={'customers'}
      />
      {/* Dialog import file */}
      <DialogImportCustomer
        open={isShowImport}
        handleCancel={() => {
          setIsShowImport(false);
        }}
        onSuccess={() => {
          getListCustomerAsync({ ...getListCustomerParamsRequest });
        }}
      />
    </div>
  );
};
export default ClientManagement;
